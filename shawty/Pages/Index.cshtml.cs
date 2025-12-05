using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using System.Text.Json;

namespace Shawty.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;

        public IndexModel(IMemoryCache cache, HttpClient httpClient)
        {
            _cache = cache;
            _httpClient = httpClient;
        }

        [BindProperty]
        public string LongUrl { get; set; }

        public string ShortUrl { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
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

                // Call the internal API (use localhost:8080 inside container)
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                
                var response = await _httpClient.PostAsJsonAsync("http://localhost:8080/api/shorten", new { url = LongUrl });
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var shortCode = result.GetProperty("shortCode").GetString();
                    ShortUrl = $"{baseUrl}/{shortCode}";
                }
                else
                {
                    ErrorMessage = "An error occurred while shortening the URL.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while shortening the URL.";
                Console.WriteLine($"Error in OnPostAsync: {ex.Message}");
            }

            return Page();
        }
    }
}
