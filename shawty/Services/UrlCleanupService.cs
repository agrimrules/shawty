using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shawty.Database;

namespace Shawty.Services
{
    public class UrlCleanupService : BackgroundService
    {
        private readonly ILogger<UrlCleanupService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1);

        public UrlCleanupService(ILogger<UrlCleanupService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UrlCleanupService is starting.");

            using PeriodicTimer timer = new PeriodicTimer(_period);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation("Running cleanup job...");
                    DatabaseManager.DeleteOldUrls(72);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up old URLs.");
                }
            }
        }
    }
}
