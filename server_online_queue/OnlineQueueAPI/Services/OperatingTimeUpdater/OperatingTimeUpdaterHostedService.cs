namespace OnlineQueueAPI.Services
{
    public class OperatingTimeUpdaterHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public OperatingTimeUpdaterHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var updaterService = scope.ServiceProvider.GetRequiredService<IOperatingTimeUpdaterService>();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await updaterService.UpdateStatusesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in OperatingTimeUpdater: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
