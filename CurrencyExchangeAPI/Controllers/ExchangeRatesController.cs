using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CurrencyExchangeAPI.Data;
using CurrencyExchangeAPI.Models;
using CurrencyExchangeAPI.Services;

namespace CurrencyExchangeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly RateFetcherService _rateFetcherService;
        private readonly string[] _supportedCurrencies = { "USD", "EUR", "GBP", "ILS" };

        public ExchangeRatesController(
            ApplicationDbContext context,
            RateFetcherService rateFetcherService)
        {
            _context = context;
            _rateFetcherService = rateFetcherService;
        }

        [HttpGet("latest/{fromCurrency}/{toCurrency}")]
        public async Task<ActionResult<ExchangeRate>> GetLatestRate(string fromCurrency, string toCurrency)
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

                var rate = await _rateFetcherService.GetExchangeRateAsync(fromCurrency, toCurrency);
                return Ok(rate);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("history/{fromCurrency}/{toCurrency}")]
        public async Task<ActionResult<IEnumerable<ExchangeRate>>> GetExchangeRateHistory(string fromCurrency, string toCurrency)
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

                var rates = await _context.ExchangeRates
                    .Where(r => r.BaseCurrency == fromCurrency && r.TargetCurrency == toCurrency)
                    .OrderByDescending(r => r.Timestamp)
                    .ToListAsync();

                return Ok(rates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
} 