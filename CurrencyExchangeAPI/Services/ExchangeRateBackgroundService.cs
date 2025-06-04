using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
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
        private readonly string[] _supportedCurrencies = { "USD", "EUR", "GBP", "ILS" };

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

                        // Generate all possible currency pairs
                        var currencyPairs = _supportedCurrencies
                            .SelectMany(from => _supportedCurrencies.Select(to => (From: from, To: to)))
                            .ToList();

                        foreach (var pair in currencyPairs)
                        {
                            try
                            {
                                var rate = await currencyLayerService.GetExchangeRateAsync(pair.From, pair.To);
                                dbContext.ExchangeRates.Add(rate);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error updating rate for {pair.From}/{pair.To}: {ex.Message}");
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