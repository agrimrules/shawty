# [Shawty](https://shawty.agrim.dev) 🔗


A URL shortener built with ASP.NET Core, featuring a beautiful  UI, automatic link expiration, and Docker support. currently hosted [here](https://shawty.agrim.dev).

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/license-%20%20GNU%20GPLv3%20-green?style=plastic)

## ✨ Features
- **⚡ Fast & Lightweight**: Built with ASP.NET Core Minimal API
- **🔒 Automatic Cleanup**: Links expire after 72 hours
- **🐳 Docker Ready**: Full Docker and Docker Compose support
- **🌐 Domain Agnostic**: Works with any custom domain
- **💾 SQLite Database**: Simple, file-based storage with indexing
- **🔄 Background Service**: Automatic cleanup of expired URLs

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) (optional)

### Running Locally

1. **Clone the repository**
   ```bash
   git clone https://github.com/agrimrules/shawty.git
   cd shawty
   ```

2. **Run the application**
   ```bash
   cd shawty
   dotnet run
   ```

3. **Open your browser**
   ```
   http://localhost:5277
   ```

### Running with Docker Compose

1. **Start the containers**
   ```bash
   docker-compose up --build -d
   ```

2. **Access the application**
   ```
   http://localhost:5277
   ```

3. **Stop the containers**
   ```bash
   docker-compose down
   ```

## 📖 Usage

### Web UI

1. Navigate to the home page
2. Paste your long URL into the input field
3. Click "Shorten URL"
4. Copy and share your shortened link!

**Note:** All shortened links are valid for 72 hours.

### API Endpoints

#### Shorten a URL
```bash
POST /api/shorten
Content-Type: application/json

{
  "url": "https://example.com/very/long/url"
}
```

**Response:**
```json
{
  "shortCode": "a1b2c3d4"
}
```

#### Access a shortened URL
```bash
GET /{shortCode}
```

Redirects to the original URL or returns a 404 page if not found.

## 🏗️ Architecture

### Tech Stack

- **Backend**: ASP.NET Core 9.0 (Minimal API + Razor Pages)
- **Database**: SQLite with Microsoft.Data.Sqlite
- **Hashing**: MD5 (8-character Base64 encoding)
- **Styling**: Vanilla CSS with custom design system
- **Fonts**: Google Fonts (Outfit)

### Project Structure

```
shawty/
├── Database/
│   └── DatabaseManager.cs      # SQLite operations
├── Pages/
│   ├── Index.cshtml            # Home page
│   ├── Index.cshtml.cs         # Home page logic
│   ├── NotFound.cshtml         # Custom 404 page
│   ├── Shared/
│   │   └── _Layout.cshtml      # Layout template
│   └── _ViewImports.cshtml     # Razor imports
├── Services/
│   └── UrlCleanupService.cs    # Background cleanup job
├── wwwroot/
│   └── css/
│       └── site.css            # Premium styling
├── Program.cs                  # Application entry point
├── Dockerfile                  # Docker configuration
└── docker-compose.yml          # Docker Compose setup
```

## 🔧 Configuration

### Database Location

The SQLite database is stored at `../sqlite/data.db` relative to the application directory.

### Expiration Time

Links expire after **72 hours**. To change this, modify the cleanup service:

```csharp
// In Services/UrlCleanupService.cs
DatabaseManager.DeleteOldUrls(72);  // Change 72 to desired hours
```

### Cleanup Frequency

The background service runs every **1 hour**. To change this:

```csharp
// In Services/UrlCleanupService.cs
private readonly TimeSpan _period = TimeSpan.FromHours(1);  // Adjust as needed
```

### Other Platforms

The application works on any platform that supports:
- Docker containers
- ASP.NET Core 9.0 runtime
- Port mapping

## 🛠️ Development

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Database Schema

```sql
CREATE TABLE urls (
    url TEXT,
    encoded TEXT,
    timestamp DATETIME
);

CREATE INDEX idx_urls_encoded ON urls(encoded);
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📝 License

This project is licensed under the GPL-3.0 License.

## 🙏 Acknowledgments

- Built with [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- Styled with [Google Fonts](https://fonts.google.com/)

---


