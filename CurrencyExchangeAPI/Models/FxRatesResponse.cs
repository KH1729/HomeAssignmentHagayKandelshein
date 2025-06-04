using System.Text.Json.Serialization;

namespace CurrencyExchangeAPI.Models;

public class FxRatesResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("base")]
    public string Base { get; set; } = "USD";

    [JsonPropertyName("rates")]
    public Dictionary<string, decimal> Rates { get; set; } = new();
} 