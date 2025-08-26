using Currency.Application.Models;
using Currency.Application.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Interfaces
{
    public interface ICurrencyProvider
    {
        Task<ApiResponse> GetLatestRatesAsync(RatesRequest ratesRequest);
        Task<ApiResponse> GetHistoricalExchangeRates(HistoricalRequest ratesRequest);
        Task<ApiResponse> ConvertCurrencyAsync(ConvertExchangeRatesRequest ratesRequest);
    }
}
