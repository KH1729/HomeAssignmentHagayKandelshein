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
        private readonly CurrencyLayerService _currencyLayerService;
        private readonly string[] _supportedCurrencies = { "USD", "EUR", "GBP", "ILS" };

        public ExchangeRatesController(
            ApplicationDbContext context,
            CurrencyLayerService currencyLayerService)
        {
            _context = context;
            _currencyLayerService = currencyLayerService;
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

                var rate = await _currencyLayerService.GetExchangeRateAsync(fromCurrency, toCurrency);
                return Ok(rate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("history/{fromCurrency}/{toCurrency}")]
        public async Task<ActionResult<IEnumerable<ExchangeRate>>> GetExchangeRateHistory(string fromCurrency, string toCurrency)
        {
            if (!_supportedCurrencies.Contains(fromCurrency))
            {
                return BadRequest($"Unsupported currency: {fromCurrency}. Supported currencies are: {string.Join(", ", _supportedCurrencies)}");
            }
            if (!_supportedCurrencies.Contains(toCurrency))
            {
                return BadRequest($"Unsupported currency: {toCurrency}. Supported currencies are: {string.Join(", ", _supportedCurrencies)}");
            }

            var rates = await _context.ExchangeRates
                .Where(r => r.BaseCurrency == fromCurrency && r.TargetCurrency == toCurrency)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();

            return rates;
        }
    }
} 