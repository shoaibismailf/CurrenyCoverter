using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Currency.Application.Models.Response
{
    public class HistoricalRateResponse
    {
        [JsonPropertyName("base")]
        public string BaseCurrency { get; set; } = string.Empty;

        [JsonPropertyName("start_date")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("rates")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, Dictionary<string, decimal>>? HistoricalRates { get; set; }
        public List<HistoricalRateItem> items { get; set; } = new();
        // Pagination
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class HistoricalRateItem
    {
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = [];
    }
}
