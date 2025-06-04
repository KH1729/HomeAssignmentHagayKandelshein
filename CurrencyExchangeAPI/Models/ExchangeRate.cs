using System;

namespace CurrencyExchangeAPI.Models
{
    public class ExchangeRate
    {
        public int Id { get; set; }
        public string PairName { get; set; } = string.Empty;  // e.g., "USD/ILS"
        public decimal Rate { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
} 