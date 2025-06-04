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
        private readonly ILogger<ExchangeRateBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly PeriodicTimer _timer;
        private readonly string[] _supportedCurrencies = { "USD", "EUR", "GBP", "ILS" };

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
                        var rateFetcherService = scope.ServiceProvider.GetRequiredService<RateFetcherService>();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        // Generate all possible currency pairs
                        var currencyPairs = _supportedCurrencies
                            .SelectMany(from => _supportedCurrencies.Select(to => (From: from, To: to)))
                            .ToList();

                        foreach (var pair in currencyPairs)
                        {
                            try
                            {
                                var rate = await rateFetcherService.GetExchangeRateAsync(pair.From, pair.To);
                                dbContext.ExchangeRates.Add(rate);
                                _logger.LogInformation($"Successfully fetched rate for {pair.From}/{pair.To}: {rate.Rate}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error updating rate for {pair.From}/{pair.To}: {ex.Message}");
                            }
                        }

                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation("All exchange rates updated successfully");
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