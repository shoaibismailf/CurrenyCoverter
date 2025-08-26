using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Currency.Application.Models.Response
{
    public class ExchangeRateResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; } = string.Empty;

        [JsonPropertyName("documentation")]
        public string Documentation { get; set; } = string.Empty;

        [JsonPropertyName("terms_of_use")]
        public string TermsOfUse { get; set; } = string.Empty;

        [JsonPropertyName("time_last_update_unix")]
        public long TimeLastUpdateUnix { get; set; }

        [JsonPropertyName("time_last_update_utc")]
        public string TimeLastUpdateUtc { get; set; } = string.Empty;

        [JsonPropertyName("time_next_update_unix")]
        public long TimeNextUpdateUnix { get; set; }

        [JsonPropertyName("time_next_update_utc")]
        public string TimeNextUpdateUtc { get; set; } = string.Empty;

        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; } = string.Empty;

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, decimal> ConversionRates { get; set; } = [];
    }
}
