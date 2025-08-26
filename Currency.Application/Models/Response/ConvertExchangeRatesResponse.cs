using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Models.Response
{
    public class ConvertExchangeRatesResponse
    {
        public required string From { get; set; }

        public required string To { get; set; }

        public required decimal Amount { get; set; }

        public DateTime Date { get; set; }
        public Dictionary<string, decimal> ConvertedAmounts { get; set; } = [];
    }
}
