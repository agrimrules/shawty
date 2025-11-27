using Shawty.Database;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<Shawty.Services.UrlCleanupService>();

var app = builder.Build();

// Ensure DB setup
string[,] colsTypes = { { "url", "string" }, { "encoded", "string" }, { "timestamp", "datetime" } };
DatabaseManager.CreateTable("urls", colsTypes);
DatabaseManager.CreateIndex("urls", "encoded");
DatabaseManager.RemoveDuplicates("urls", "url");
DatabaseManager.CreateUniqueIndex("urls", "url");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/NotFound");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.MapPost("/api/shorten", async ([FromBody] UrlRequest request, HttpClient httpClient, IConfiguration config) =>
{
    if (string.IsNullOrWhiteSpace(request.Url))
    {
        return Results.BadRequest("URL is required");
    }

    string shortCode;

    try
    {
        // Scrape page content
        var html = await httpClient.GetStringAsync(request.Url);
        var text = StripHtml(html);
        var truncated = text.Substring(0, Math.Min(2000, text.Length));

        // Ask LLM for 2-3 word summary slug
        shortCode = await GetSlugFromLLM(httpClient, config["OpenAI:ApiKey"]!, truncated);
        Console.WriteLine($"Successfully generated mnemonic slug '{shortCode}' for {request.Url}");
    }
    catch (Exception ex)
    {
        shortCode = GenerateHash(request.Url);
        Console.WriteLine($"Failed to generate mnemonic slug for {request.Url}: {ex.Message}. Falling back to hash: {shortCode}");
    }

    DatabaseManager.Insert("urls", new string[] { request.Url, shortCode, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
    return Results.Ok(new { shortCode });
});

app.MapGet("/{encoded}", (string encoded) =>
{
    string? url = DatabaseManager.GetUrl(encoded);
    if (url != null)
    {
        return Results.Redirect(url);
    }
    return Results.NotFound();
});

app.Run();

static async Task<string> GetSlugFromLLM(HttpClient http, string apiKey, string content)
{
    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
    request.Headers.Add("Authorization", $"Bearer {apiKey}");
    request.Content = JsonContent.Create(new
    {
        model = "gpt-3.5-turbo",
        messages = new[]
        {
            new { role = "system", content = "Generate a 2-3 word URL slug summarizing this content. Lowercase, hyphens only, no special chars. Reply with ONLY the slug." },
            new { role = "user", content }
        },
        max_tokens = 20
    });

    var response = await http.SendAsync(request);
    var responseBody = await response.Content.ReadAsStringAsync();
    
    if (!response.IsSuccessStatusCode)
    {
        throw new Exception($"OpenAI API error ({response.StatusCode}): {responseBody}");
    }
    
    var json = JsonDocument.Parse(responseBody).RootElement;
    return json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()!.Trim();
}

static string StripHtml(string html)
{
    return Regex.Replace(html, @"<[^>]+>|&nbsp;", " ")
        .Replace("\n", " ").Replace("\r", " ");
}

static string GenerateHash(string url)
{
    using var md5 = MD5.Create();
    byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(url));
    return Convert.ToBase64String(hashBytes)
        .Replace("+", "-")
        .Replace("/", "_")
        .Substring(0, 8);
}

record UrlRequest(string Url, bool UseMnemonic = true);