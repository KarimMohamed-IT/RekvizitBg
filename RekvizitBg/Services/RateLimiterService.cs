using System.Collections.Concurrent;

namespace RekvizitBg.Services
{
    /// <summary>
    /// In-memory rate limiter to prevent abuse (form spam, brute force).
    /// Tracks requests per IP and blocks excessive submissions.
    /// </summary>
    public class RateLimiterService
    {
        private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();
        private readonly int _maxRequests;
        private readonly TimeSpan _window;

        public RateLimiterService(int maxRequestsPerWindow = 5, int windowSeconds = 60)
        {
            _maxRequests = maxRequestsPerWindow;
            _window = TimeSpan.FromSeconds(windowSeconds);
        }

        /// <summary>
        /// Check if the given key (e.g. IP) is rate limited.
        /// Returns true if under limit, false if blocked.
        /// </summary>
        public bool IsAllowed(string key)
        {
            CleanupExpired();

            var entry = _entries.GetOrAdd(key, _ => new RateLimitEntry());

            lock (entry)
            {
                var now = DateTime.UtcNow;

                // Reset window if expired
                if (now - entry.WindowStart > _window)
                {
                    entry.WindowStart = now;
                    entry.RequestCount = 0;
                }

                entry.RequestCount++;
                return entry.RequestCount <= _maxRequests;
            }
        }

        /// <summary>
        /// Get remaining requests for a key in the current window.
        /// </summary>
        public int GetRemaining(string key)
        {
            if (_entries.TryGetValue(key, out var entry))
            {
                lock (entry)
                {
                    if (DateTime.UtcNow - entry.WindowStart > _window)
                        return _maxRequests;
                    return Math.Max(0, _maxRequests - entry.RequestCount);
                }
            }
            return _maxRequests;
        }

        private void CleanupExpired()
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _entries)
            {
                if (now - kvp.Value.WindowStart > _window * 2)
                {
                    _entries.TryRemove(kvp.Key, out _);
                }
            }
        }

        private class RateLimitEntry
        {
            public DateTime WindowStart { get; set; } = DateTime.UtcNow;
            public int RequestCount { get; set; } = 0;
        }
    }
}
