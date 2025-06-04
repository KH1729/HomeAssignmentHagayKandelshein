using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CurrencyExchangeAPI.Data;
using CurrencyExchangeAPI.Models;
using CurrencyExchangeAPI.Services;
using Microsoft.Extensions.Logging;

namespace CurrencyExchangeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly ExchangeRateService _exchangeRateService;
        private readonly ILogger<ExchangeRatesController> _logger;
        private readonly string[] _supportedCurrencies = { "USD", "EUR", "GBP", "ILS" };

        public ExchangeRatesController(
            ExchangeRateService exchangeRateService,
            ILogger<ExchangeRatesController> logger)
        {
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        [HttpGet("{fromCurrency}/{toCurrency}")]
        public async Task<IActionResult> GetExchangeRate(string fromCurrency, string toCurrency)
        {
            try
            {
                if (!_supportedCurrencies.Contains(fromCurrency))
                {
                    return BadRequest($"Unsupported currency: {fromCurrency}. Supported currencies are: {string.Join(", ", _supportedCurrencies)}");
                }
                if (!_supportedCurrencies.Contains(toCurrency))
                {
                    return BadRequest($"Unsupported currency: {toCurrency}. Supported currencies are: {string.Join(", ", _supportedCurrencies)}");
                }

                var rate = await _exchangeRateService.GetLatestRateAsync(fromCurrency, toCurrency);
                
                if (rate == null)
                {
                    return NotFound($"No exchange rate found for {fromCurrency}/{toCurrency}");
                }

                return Ok(rate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching exchange rate");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRates()
        {
            try
            {
                var rates = await _exchangeRateService.GetAllLatestRatesAsync();
                return Ok(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all rates");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet("history/{fromCurrency}/{toCurrency}")]
        public async Task<IActionResult> GetExchangeRateHistory(string fromCurrency, string toCurrency)
        {
            try
            {
                if (!_supportedCurrencies.Contains(fromCurrency))
                {
                    return BadRequest($"Unsupported currency: {fromCurrency}. Supported currencies are: {string.Join(", ", _supportedCurrencies)}");
                }
                if (!_supportedCurrencies.Contains(toCurrency))
                {
                    return BadRequest($"Unsupported currency: {toCurrency}. Supported currencies are: {string.Join(", ", _supportedCurrencies)}");
                }

                if (fromCurrency == toCurrency)
                {
                    return BadRequest("The rate of a currency with itself is always 1. Please select different currencies.");
                }

                var rates = await _exchangeRateService.GetExchangeRateHistoryAsync(fromCurrency, toCurrency);
                return Ok(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching exchange rate history");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
} 