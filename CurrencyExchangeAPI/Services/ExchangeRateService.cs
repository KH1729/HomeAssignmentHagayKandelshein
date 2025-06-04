using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CurrencyExchangeAPI.Data;
using CurrencyExchangeAPI.Models;

namespace CurrencyExchangeAPI.Services;

public class ExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly string[] _supportedCurrencies = { "USD", "EUR", "GBP", "ILS" };

    public ExchangeRateService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ExchangeRateService> logger,
        ApplicationDbContext dbContext)
    {
        _httpClient = httpClient;
        _apiKey = configuration["FxRates:ApiKey"] ?? throw new ArgumentNullException("FxRates:ApiKey");
        _baseUrl = configuration["FxRates:BaseUrl"] ?? throw new ArgumentNullException("FxRates:BaseUrl");
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task FetchAndStoreRatesAsync()
    {
        try
        {
            var rates = await GetExchangeRatesAsync();
            var timestamp = DateTime.UtcNow;

            foreach (var fromCurrency in _supportedCurrencies)
            {
                foreach (var toCurrency in _supportedCurrencies)
                {
                    // Skip if currencies are the same
                    if (fromCurrency == toCurrency)
                    {
                        _logger.LogInformation($"Skipping same currency pair {fromCurrency}/{toCurrency}");
                        continue;
                    }

                    var rate = CalculateRate(rates, fromCurrency, toCurrency);

                    var exchangeRate = new ExchangeRate
                    {
                        BaseCurrency = fromCurrency,
                        TargetCurrency = toCurrency,
                        Rate = rate,
                        Timestamp = timestamp
                    };

                    _dbContext.ExchangeRates.Add(exchangeRate);
                }
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Successfully stored exchange rates in database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching and storing rates");
            throw;
        }
    }

    public async Task<ExchangeRate?> GetLatestRateAsync(string fromCurrency, string toCurrency)
    {
        return await _dbContext.ExchangeRates
            .Where(r => r.BaseCurrency == fromCurrency && r.TargetCurrency == toCurrency)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ExchangeRate>> GetAllLatestRatesAsync()
    {
        var latestTimestamp = await _dbContext.ExchangeRates
            .OrderByDescending(r => r.Timestamp)
            .Select(r => r.Timestamp)
            .FirstOrDefaultAsync();

        return await _dbContext.ExchangeRates
            .Where(r => r.Timestamp == latestTimestamp)
            .ToListAsync();
    }

    private async Task<Dictionary<string, decimal>> GetExchangeRatesAsync()
    {
        var url = $"{_baseUrl}access_key={_apiKey}";
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("API request failed with status code {StatusCode}. Response: {Content}", 
                response.StatusCode, content);
            throw new HttpRequestException($"API request failed with status code {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<FxRatesResponse>();
        if (result?.Rates == null)
        {
            throw new InvalidOperationException("Failed to deserialize API response");
        }

        return result.Rates;
    }

    private decimal CalculateRate(Dictionary<string, decimal> rates, string fromCurrency, string toCurrency)
    {
        if (fromCurrency == "USD")
        {
            return rates[toCurrency];
        }
        else if (toCurrency == "USD")
        {
            return 1 / rates[fromCurrency];
        }
        else
        {
            return rates[toCurrency] / rates[fromCurrency];
        }
    }
} 