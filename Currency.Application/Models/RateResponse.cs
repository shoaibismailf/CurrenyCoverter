using System.Text.Json.Serialization;

namespace Currency.Application.Models
{
    public class RateResponse
    {
        [JsonPropertyName("base")]
        public string BaseCurrency { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}