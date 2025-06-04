using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CurrencyExchangeAPI.Services
{
    public class ExchangeRateBackgroundService : BackgroundService
    {
        private readonly ILogger<ExchangeRateBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly PeriodicTimer _timer;

        public ExchangeRateBackgroundService(
            ILogger<ExchangeRateBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Exchange Rate Background Service is starting.");

            try
            {
                while (await _timer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogInformation("Fetching exchange rates...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var exchangeRateService = scope.ServiceProvider.GetRequiredService<ExchangeRateService>();
                        await exchangeRateService.FetchAndStoreRatesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching exchange rates.");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Exchange Rate Background Service is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
} 