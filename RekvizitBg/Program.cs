using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using RekvizitBg.Data;
using RekvizitBg.Middleware;
using RekvizitBg.Services;

namespace RekvizitBg
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== BUILD CONNECTION STRING WITH ENV PASSWORD =====
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
            var dbPassword = Environment.GetEnvironmentVariable("DatabasePassword");
            if (!string.IsNullOrEmpty(dbPassword))
            {
                // Append password if provided via env var
                if (!connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase))
                {
                    connectionString = connectionString.TrimEnd(';') + $";Password={dbPassword};";
                }
            }

            // ===== SERVICES =====
            builder.Services.AddRazorPages();

            // SMTP settings with env var password
            builder.Services.Configure<SmtpSettings>(options =>
            {
                var section = builder.Configuration.GetSection("SmtpSettings");
                section.Bind(options);
                // Override password from environment variable
                var emailPassword = Environment.GetEnvironmentVariable("EmailPassword");
                if (!string.IsNullOrEmpty(emailPassword))
                {
                    options.Password = emailPassword;
                }
            });
            builder.Services.AddTransient<EmailService>();

            // Database with connection pooling for performance
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                }));

            // Rate limiter (singleton for shared state across requests)
            builder.Services.AddSingleton(new RateLimiterService(
                maxRequestsPerWindow: 5,
                windowSeconds: 60));

            // Response compression for performance
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "application/javascript",
                    "text/css",
                    "image/svg+xml",
                    "application/font-woff2"
                });
            });
            builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = System.IO.Compression.CompressionLevel.Optimal;
            });
            builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = System.IO.Compression.CompressionLevel.Optimal;
            });

            // Output caching
            builder.Services.AddOutputCache(options =>
            {
                options.AddBasePolicy(p => p.Expire(TimeSpan.FromMinutes(5)));
                options.AddPolicy("StaticFiles", p => p.Expire(TimeSpan.FromHours(24)));
            });

            // Antiforgery for CSRF protection
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            // Request size limit
            builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB max upload
            });

            // Configure Kestrel
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
                options.Limits.MaxConcurrentConnections = 200;
                options.Limits.MaxConcurrentUpgradedConnections = 200;
                options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                options.AddServerHeader = false; // Don't expose server info
            });

            var app = builder.Build();

            // ===== AUTO-MIGRATE/SEED DATABASE =====
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    db.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Database initialization failed.");
                }
            }

            // ===== MIDDLEWARE PIPELINE =====

            // 1. Security headers (first in pipeline)
            app.UseSecurityHeaders();

            // 2. Response compression
            app.UseResponseCompression();

            // 3. Exception handling
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // 4. HTTPS redirect
            app.UseHttpsRedirection();

            // 5. Static files with aggressive caching
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    // Cache static files for 30 days in production
                    ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=2592000";
                    ctx.Context.Response.Headers["Vary"] = "Accept-Encoding";
                }
            });

            // 6. Output caching
            app.UseOutputCache();

            // 7. Routing
            app.UseRouting();

            // 8. Authorization
            app.UseAuthorization();

            // 9. Map Razor Pages
            app.MapRazorPages();

            app.Run();
        }
    }
}
