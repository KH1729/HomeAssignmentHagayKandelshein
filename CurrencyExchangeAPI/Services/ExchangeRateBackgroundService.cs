using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CurrencyExchangeAPI.Data;
using CurrencyExchangeAPI.Models;

namespace CurrencyExchangeAPI.Services
{
    public class ExchangeRateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ExchangeRateBackgroundService> _logger;
        private readonly string[] _supportedPairs = { "USD/ILS", "EUR/ILS", "GBP/ILS", "EUR/USD", "EUR/GBP" };

        public ExchangeRateBackgroundService(
            IServiceProvider services,
            ILogger<ExchangeRateBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var currencyLayerService = scope.ServiceProvider.GetRequiredService<CurrencyLayerService>();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        foreach (var pair in _supportedPairs)
                        {
                            try
                            {
                                var rate = await currencyLayerService.GetExchangeRateAsync(pair);
                                dbContext.ExchangeRates.Add(rate);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error updating rate for {pair}: {ex.Message}");
                            }
                        }

                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation("Exchange rates updated successfully");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in background service: {ex.Message}");
                }

                // Wait for 10 seconds before the next update
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
} 