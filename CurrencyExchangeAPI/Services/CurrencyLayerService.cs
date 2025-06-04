using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CurrencyExchangeAPI.Models;

namespace CurrencyExchangeAPI.Services
{
    public class CurrencyLayerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly ILogger<CurrencyLayerService> _logger;
        private readonly string[] _supportedPairs = { "USD/ILS", "EUR/ILS", "GBP/ILS", "EUR/USD", "EUR/GBP" };

        public CurrencyLayerService(
            HttpClient httpClient, 
            IConfiguration configuration,
            ILogger<CurrencyLayerService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["CurrencyLayer:ApiKey"] ?? throw new ArgumentNullException(nameof(configuration), "CurrencyLayer:ApiKey is not configured");
            _baseUrl = configuration["CurrencyLayer:BaseUrl"] ?? throw new ArgumentNullException(nameof(configuration), "CurrencyLayer:BaseUrl is not configured");
            _logger = logger;
        }

        public async Task<ExchangeRate> GetExchangeRateAsync(string pairName)
        {
            if (!_supportedPairs.Contains(pairName))
            {
                throw new ArgumentException($"Unsupported currency pair: {pairName}. Supported pairs are: {string.Join(", ", _supportedPairs)}");
            }

            var currencies = pairName.Split('/');
            var fromCurrency = currencies[0];
            var toCurrency = currencies[1];

            _logger.LogInformation($"Getting exchange rate for {pairName}");

            // If the base currency is USD, we can get the rate directly
            if (fromCurrency == "USD")
            {
                var quotes = await GetExchangeRatesAsync(toCurrency);
                var quoteKey = $"USD{toCurrency}";
                if (!quotes.TryGetValue(quoteKey, out var rate))
                {
                    throw new Exception($"Exchange rate not found for {quoteKey}");
                }
                return new ExchangeRate
                {
                    PairName = pairName,
                    Rate = rate,
                    LastUpdateTime = DateTime.UtcNow
                };
            }

            // For other currency pairs, we need to calculate the cross-rate
            var allQuotes = await GetExchangeRatesAsync($"{fromCurrency},{toCurrency}");
            
            // Get the USD rates for both currencies
            var usdFromRate = allQuotes[$"USD{fromCurrency}"];
            var usdToRate = allQuotes[$"USD{toCurrency}"];

            // Calculate the cross-rate
            var crossRate = usdToRate / usdFromRate;
            
            return new ExchangeRate
            {
                PairName = pairName,
                Rate = crossRate,
                LastUpdateTime = DateTime.UtcNow
            };
        }

        internal async Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string currencies)
        {
            var url = $"{_baseUrl}?access_key={_apiKey}&currencies={currencies}";
            _logger.LogInformation($"Calling API: {url}");
            
            try 
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"API Response Status: {response.StatusCode}");
                _logger.LogInformation($"API Response Content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API call failed with status code: {response.StatusCode}");
                    _logger.LogError($"Response content: {content}");
                    throw new Exception($"API call failed with status code: {response.StatusCode}. Response: {content}");
                }

                var result = JsonSerializer.Deserialize<CurrencyLayerResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    _logger.LogError("Failed to deserialize response from CurrencyLayer API");
                    throw new Exception("Failed to deserialize response from CurrencyLayer API");
                }

                if (!result.Success)
                {
                    var errorMessage = result.Error?.Info ?? "Unknown error";
                    _logger.LogError($"API returned error: {errorMessage}");
                    throw new Exception($"Failed to get exchange rates: {errorMessage}");
                }

                if (result.Quotes == null || !result.Quotes.Any())
                {
                    _logger.LogError("No quotes returned from CurrencyLayer API");
                    throw new Exception("No quotes returned from CurrencyLayer API");
                }

                return result.Quotes;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during API call: {ex.Message}");
                throw;
            }
        }
    }

    public class CurrencyLayerResponse
    {
        public bool Success { get; set; }
        public string? Terms { get; set; }
        public string? Privacy { get; set; }
        public long Timestamp { get; set; }
        public string? Source { get; set; }
        public Dictionary<string, decimal>? Quotes { get; set; }
        public CurrencyLayerError? Error { get; set; }
    }

    public class CurrencyLayerError
    {
        public string? Info { get; set; }
    }
} 