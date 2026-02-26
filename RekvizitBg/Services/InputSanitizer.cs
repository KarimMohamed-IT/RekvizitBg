using System.Text.RegularExpressions;
using System.Web;

namespace RekvizitBg.Services
{
    /// <summary>
    /// Input sanitization utility to prevent XSS, SQL injection, and other injection attacks.
    /// </summary>
    public static class InputSanitizer
    {
        // Regex patterns for dangerous content
        private static readonly Regex ScriptTagPattern = new(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex HtmlTagPattern = new(@"<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex SqlInjectionPattern = new(@"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|EXEC|UNION|TRUNCATE|DECLARE|CAST)\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex MultipleSpacesPattern = new(@"\s{2,}", RegexOptions.Compiled);

        /// <summary>
        /// Sanitize a text input: strips HTML/script tags, trims, limits length.
        /// </summary>
        public static string SanitizeText(string? input, int maxLength = 500)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // 1. Trim whitespace
            var sanitized = input.Trim();

            // 2. HTML decode first to catch encoded attacks
            sanitized = HttpUtility.HtmlDecode(sanitized);

            // 3. Remove script tags
            sanitized = ScriptTagPattern.Replace(sanitized, string.Empty);

            // 4. Remove all HTML tags
            sanitized = HtmlTagPattern.Replace(sanitized, string.Empty);

            // 5. Re-encode for safety
            sanitized = HttpUtility.HtmlEncode(sanitized);

            // 6. Collapse multiple spaces
            sanitized = MultipleSpacesPattern.Replace(sanitized, " ");

            // 7. Enforce max length
            if (sanitized.Length > maxLength)
                sanitized = sanitized[..maxLength];

            return sanitized;
        }

        /// <summary>
        /// Sanitize an email address.
        /// </summary>
        public static string SanitizeEmail(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var email = input.Trim().ToLowerInvariant();

            // Basic email validation
            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                return string.Empty;

            // Max length for email
            if (email.Length > 254)
                return string.Empty;

            return email;
        }

        /// <summary>
        /// Sanitize a phone number — only allow digits, +, spaces, dashes, parentheses.
        /// </summary>
        public static string SanitizePhone(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var phone = input.Trim();

            // Remove everything except digits, +, spaces, dashes, parens
            phone = Regex.Replace(phone, @"[^\d\+\-\s\(\)]", string.Empty);

            if (phone.Length > 20)
                phone = phone[..20];

            return phone;
        }

        /// <summary>
        /// Check if input contains SQL injection patterns.
        /// </summary>
        public static bool ContainsSqlInjection(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return SqlInjectionPattern.IsMatch(input);
        }

        /// <summary>
        /// Sanitize a filename for safe storage.
        /// </summary>
        public static string SanitizeFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return $"{Guid.NewGuid():N}";

            // Remove path traversal characters
            var name = Path.GetFileNameWithoutExtension(fileName);
            name = Regex.Replace(name, @"[^\w\-.]", "_");

            // Limit length
            if (name.Length > 50)
                name = name[..50];

            return name;
        }

        /// <summary>
        /// Validate file extension against allowed list.
        /// </summary>
        public static bool IsAllowedFileExtension(string? fileName, string[] allowedExtensions)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Contains(ext);
        }
    }
}
