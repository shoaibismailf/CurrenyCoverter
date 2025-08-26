using Currency.Application.Models;
using Currency.Application.Models.Request;
using Currency.Application.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Web.Controllers
{
    public class RatesController : Controller
    {
        private readonly HttpClient _httpClient;
        public RatesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("CurrencyApi");
        }

        public IActionResult Index()
        {
            return View("Latest");
        }

        public IActionResult CurrencyConversion()
        {
            return View("CurrencyConversion");
        }

        public async Task<IActionResult> Latest(string? baseCurrency, string? symbols, DateTime? startDate = null)
        {
            var response = await _httpClient.PostAsJsonAsync("Rates/GetLatestRatesAsync", new
            {
                BaseCurrency = baseCurrency,
                Symbols = symbols,
                StartDate = startDate
            });

            if (response.IsSuccessStatusCode)
            {
                var rates = await response.Content.ReadFromJsonAsync<RateResponse>();
                return View("Latest", rates);
            }
            else
            {
                ModelState.AddModelError("", "Failed to fetch rates");
                return View("Latest");
            }
        }

        public async Task<IActionResult> GetHistoricalExchangeRates(string? baseCurrency, string? symbols, DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime now = DateTime.Now;

            var response = await _httpClient.PostAsJsonAsync("Rates/GetHistoricalExchangeRates", new
            {
                BaseCurrency = baseCurrency,
                Symbols = symbols,
                StartDate = startDate ?? new DateTime(now.Year, 1, 1),
                EndDate = endDate ?? new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month))
            });

            if (response.IsSuccessStatusCode)
            {
                var rates = await response.Content.ReadFromJsonAsync<HistoricalRateResponse>();
                return View("HistoricalExchangeRatesView", rates);
            }

            else
            {
                ModelState.AddModelError("", "Failed to fetch rates");
                return View("HistoricalExchangeRatesView", new HistoricalRateResponse());
            }
        }

        public async Task<IActionResult> Convert(string from, string to, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to) || amount <= 0)
            {
                ModelState.AddModelError("", "Invalid input values.");
                return View("CurrencyConversion");
            }

            var response = await _httpClient.PostAsJsonAsync("Rates/ConvertExchangeRates", new
            {
                From = from,
                To = to,
                Amount = amount,
            });

            if (response.IsSuccessStatusCode)
            {
                var rates = await response.Content.ReadFromJsonAsync<ConvertExchangeRatesRequest>();
                return View("CurrencyConversion", rates);
            }

            else
            {
                ModelState.AddModelError("", "Failed to fetch rates");
                return View("CurrencyConversion");
            }
        }


    }
}