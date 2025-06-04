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
        private readonly string[] _supportedCurrencies = { "USD", "EUR", "GBP", "ILS" };

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

        public async Task<ExchangeRate> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            // Validate currencies
            if (!_supportedCurrencies.Contains(fromCurrency))
            {
                throw new ArgumentException($"Unsupported currency: {fromCurrency}. Supported currencies are: {string.Join(", ", _supportedCurrencies)}");
            }
            if (!_supportedCurrencies.Contains(toCurrency))
            {
                throw new ArgumentException($"Unsupported currency: {toCurrency}. Supported currencies are: {string.Join(", ", _supportedCurrencies)}");
            }

            // If currencies are the same, return rate of 1
            if (fromCurrency == toCurrency)
            {
                _logger.LogInformation($"Same currency pair {fromCurrency}/{toCurrency} - returning rate 1.0");
                return new ExchangeRate
                {
                    BaseCurrency = fromCurrency,
                    TargetCurrency = toCurrency,
                    Rate = 1.0m,
                    Timestamp = DateTime.UtcNow
                };
            }

            _logger.LogInformation($"Getting exchange rate for {fromCurrency}/{toCurrency}");

            // If the base currency is USD, we can get the rate directly
            if (fromCurrency == "USD")
            {
                var quotes = await GetExchangeRatesAsync(toCurrency);
                if (!quotes.TryGetValue(toCurrency, out var rate))
                {
                    throw new Exception($"Exchange rate not found for USD/{toCurrency}");
                }
                _logger.LogInformation($"Fetched direct rate for {fromCurrency}/{toCurrency}: {rate}");
                return new ExchangeRate
                {
                    BaseCurrency = fromCurrency,
                    TargetCurrency = toCurrency,
                    Rate = rate,
                    Timestamp = DateTime.UtcNow
                };
            }

            // For other currency pairs, we need to calculate the cross-rate
            var allQuotes = await GetExchangeRatesAsync($"{fromCurrency},{toCurrency}");
            
            // Get the USD rates for both currencies
            var usdFromRate = allQuotes[fromCurrency];
            var usdToRate = allQuotes[toCurrency];

            // Calculate the cross-rate
            var crossRate = usdToRate / usdFromRate;
            
            _logger.LogInformation($"Calculated cross-rate for {fromCurrency}/{toCurrency}: {crossRate} (using USD/{fromCurrency}: {usdFromRate} and USD/{toCurrency}: {usdToRate})");
            
            return new ExchangeRate
            {
                BaseCurrency = fromCurrency,
                TargetCurrency = toCurrency,
                Rate = crossRate,
                Timestamp = DateTime.UtcNow
            };
        }

        internal async Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string currencies)
        {
            // Remove any spaces and ensure currencies are comma-separated
            var cleanCurrencies = string.Join(",", currencies.Split(',').Select(c => c.Trim()));
            
            // Construct the URL with the correct format for FX Rates API
            var url = $"{_baseUrl}api_key={_apiKey}&currencies={cleanCurrencies}&base=USD";
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

                try
                {
                    var result = JsonSerializer.Deserialize<FxRatesResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result == null)
                    {
                        _logger.LogError("Failed to deserialize response from FX Rates API");
                        throw new Exception("Failed to deserialize response from FX Rates API");
                    }

                    if (!result.Success)
                    {
                        var errorMessage = result.Error?.Message ?? "Unknown error";
                        var errorCode = result.Error?.Code ?? 0;
                        
                        if (errorCode == 104)
                        {
                            throw new Exception("API request limit exceeded. Please try again later or upgrade your API plan.");
                        }
                        
                        _logger.LogError($"API returned error: {errorMessage} (Code: {errorCode})");
                        throw new Exception($"Failed to get exchange rates: {errorMessage}");
                    }

                    if (result.Rates == null || !result.Rates.Any())
                    {
                        _logger.LogError("No rates returned from FX Rates API");
                        throw new Exception("No rates returned from FX Rates API");
                    }

                    _logger.LogInformation($"Successfully fetched rates: {string.Join(", ", result.Rates.Select(q => $"{q.Key}: {q.Value}"))}");
                    return result.Rates;
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"JSON parsing error: {ex.Message}");
                    _logger.LogError($"Raw response content: {content}");
                    throw new Exception($"Invalid response from API: {content}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during API call: {ex.Message}");
                throw;
            }
        }
    }

    public class FxRatesResponse
    {
        public bool Success { get; set; }
        public long Timestamp { get; set; }
        public string? Base { get; set; }
        public Dictionary<string, decimal>? Rates { get; set; }
        public FxRatesError? Error { get; set; }
    }

    public class FxRatesError
    {
        public string? Message { get; set; }
        public int Code { get; set; }
    }
} 