using IPCountryBlocker.API.Interfaces;

namespace IPCountryBlocker.API.BackgroundServices
{
    public class TemporalBlockCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TemporalBlockCleanupService> _logger;

        public TemporalBlockCleanupService(IServiceProvider serviceProvider, ILogger<TemporalBlockCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Temporal Block Cleanup Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var countryRepo = scope.ServiceProvider.GetRequiredService<ICountryRepository>();
                        countryRepo.RemoveExpiredTemporalBlocks();
                        _logger.LogInformation($"Temporal blocks cleaned at {DateTime.UtcNow}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing TemporalBlockCleanupService.");
                }

                // Run every 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}