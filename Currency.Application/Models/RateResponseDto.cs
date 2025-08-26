using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Currency.Application.Models
{
    public class RateResponseDto
    {
        [JsonPropertyName("base")]
        public string BaseCurrency { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
