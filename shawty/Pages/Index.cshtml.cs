using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shawty.Database;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Shawty.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMemoryCache _cache;

        public IndexModel(IMemoryCache cache)
        {
            _cache = cache;
        }
        [BindProperty]
        public string LongUrl { get; set; }

        public string ShortUrl { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(LongUrl))
            {
                ErrorMessage = "Please enter a valid URL.";
                return Page();
            }

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            string cacheKey = $"RateLimit_{ipAddress}";

            if (_cache.TryGetValue(cacheKey, out DateTime resetTime))
            {
                var remaining = (resetTime - DateTime.Now).TotalSeconds;
                if (remaining > 0)
                {
                    ErrorMessage = $"Rate limit reached. Try again in {Math.Ceiling(remaining)} seconds.";
                    return Page();
                }
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(5));

            _cache.Set(cacheKey, DateTime.Now.AddSeconds(5), cacheEntryOptions);

            try
            {
                // Basic validation
                if (!Uri.TryCreate(LongUrl, UriKind.Absolute, out _))
                {
                    ErrorMessage = "Invalid URL format. Please include http:// or https://";
                    return Page();
                }

                byte[] ip = Encoding.UTF8.GetBytes(LongUrl);
                using (var md5 = MD5.Create())
                {
                    byte[] hashBytes = md5.ComputeHash(ip);
                    string b64 = Convert.ToBase64String(hashBytes)
                        .Replace("+", "-")
                        .Replace("/", "_")
                        .Substring(0, 8);
                    
                    DatabaseManager.Insert("urls", new string[] { LongUrl, b64, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
                    
                    var request = HttpContext.Request;
                    var baseUrl = $"{request.Scheme}://{request.Host}";
                    ShortUrl = $"{baseUrl}/{b64}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while shortening the URL.";
                // Log error
            }

            return Page();
        }
    }
}
