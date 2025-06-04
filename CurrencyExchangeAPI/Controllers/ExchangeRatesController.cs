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
        private readonly string[] _supportedPairs = { "USD/ILS", "EUR/ILS", "GBP/ILS", "EUR/USD", "EUR/GBP" };

        public ExchangeRatesController(
            ApplicationDbContext context,
            CurrencyLayerService currencyLayerService)
        {
            _context = context;
            _currencyLayerService = currencyLayerService;
        }

        [HttpGet("latest/{pairName}")]
        public async Task<ActionResult<ExchangeRate>> GetLatestRate(string pairName)
        {
            try
            {
                if (!_supportedPairs.Contains(pairName))
                {
                    return BadRequest($"Unsupported currency pair. Supported pairs are: {string.Join(", ", _supportedPairs)}");
                }

                var rate = await _currencyLayerService.GetExchangeRateAsync(pairName);
                return Ok(rate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("history/{pairName}")]
        public async Task<ActionResult<IEnumerable<ExchangeRate>>> GetExchangeRateHistory(string pairName)
        {
            if (!_supportedPairs.Contains(pairName))
            {
                return BadRequest($"Unsupported currency pair. Supported pairs are: {string.Join(", ", _supportedPairs)}");
            }

            var rates = await _context.ExchangeRates
                .Where(r => r.PairName == pairName)
                .OrderByDescending(r => r.LastUpdateTime)
                .ToListAsync();

            return rates;
        }
    }
} 