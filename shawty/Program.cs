using Shawty.Database;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHostedService<Shawty.Services.UrlCleanupService>();

var app = builder.Build();

// Ensure DB setup
string[,] colsTypes = { { "url", "string" }, { "encoded", "string" }, { "timestamp", "datetime" } };
DatabaseManager.CreateTable("urls", colsTypes);
DatabaseManager.CreateIndex("urls", "encoded");

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

app.MapPost("/api/shorten", ([FromBody] UrlRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Url))
    {
        return Results.BadRequest("URL is required");
    }

    byte[] ip = Encoding.UTF8.GetBytes(request.Url);
    using (var md5 = MD5.Create())
    {
        byte[] hashBytes = md5.ComputeHash(ip);
        string b64 = Convert.ToBase64String(hashBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Substring(0, 8);
        
        DatabaseManager.Insert("urls", new string[] { request.Url, b64, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        return Results.Ok(new { shortCode = b64 });
    }
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

record UrlRequest(string Url);