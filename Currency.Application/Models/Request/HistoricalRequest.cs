using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Models.Request
{
    public class HistoricalRequest : CommonFields
    {
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
