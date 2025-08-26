using Currency.Application.Interfaces;
using Currency.Application.Interfaces.Redis;
using Currency.Application.Models;
using Currency.Application.Models.Request;
using Currency.Application.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Currency.Application.Services
{
    [Authorize]
    public class FrankFurterProviderService : ICurrencyProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FrankFurterProviderService> _logger;
        private readonly IClaimService _claimService;
        private readonly IRedisCacheService _cacheService;

        public FrankFurterProviderService(HttpClient httpClient, ILogger<FrankFurterProviderService> logger,IClaimService claimService, IRedisCacheService cacheService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://api.frankfurter.app/");
            _claimService = claimService;
            _cacheService = cacheService;
        }

        public async Task<ApiResponse> GetLatestRatesAsync(RatesRequest ratesRequest)
        {
            var (correlationId, clientId) = GetLogContext();

            _logger.LogInformation(
                "Fetching latest rates {@RatesRequest} CorrelationId={CorrelationId} ClientId={ClientId}",
                ratesRequest, correlationId, clientId);
            try
            {
                string endpoint;

                if (ratesRequest.StartDate.HasValue)
                {
                    endpoint = $"{ratesRequest.StartDate:yyyy-MM-dd}";
                }
                else
                {
                    endpoint = "latest";
                }

                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(ratesRequest.BaseCurrency))
                    queryParams.Add($"base={ratesRequest.BaseCurrency}");

                if (!string.IsNullOrWhiteSpace(ratesRequest.Symbols))
                    queryParams.Add($"symbols={ratesRequest.Symbols}");

                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);

                _logger.LogDebug(
                    "Requesting endpoint {Endpoint} CorrelationId={CorrelationId} ClientId={ClientId}",
                    endpoint, correlationId, clientId);

                var response = await _httpClient.GetFromJsonAsync<RateResponse>(endpoint);

                if (response?.Rates == null)
                {
                    _logger.LogError(
                        "Failed to fetch currency rates from endpoint {Endpoint} CorrelationId={CorrelationId} ClientId={ClientId}",
                        endpoint, correlationId, clientId);
                    throw new Exception("Failed to fetch currency rates.");
                }

                _logger.LogInformation(
                    "Successfully fetched latest rates for Base={BaseCurrency} CorrelationId={CorrelationId} ClientId={ClientId}",
                    ratesRequest.BaseCurrency, correlationId, clientId);

                var cacheKey = $"{_claimService.GetCurrencyProvider()}:latest:{ratesRequest.BaseCurrency ?? ""}_{ratesRequest.Symbols ?? ""}_{ratesRequest.StartDate ?? null}";

                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(2));

                return new ApiResponse { Data = response };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while fetching latest rates {@RatesRequest} CorrelationId={CorrelationId} ClientId={ClientId}",
                    ratesRequest, correlationId, clientId);
                throw;
            }
        }

        public async Task<ApiResponse> GetHistoricalExchangeRates(HistoricalRequest ratesRequest)
        {
            var (correlationId, clientId) = GetLogContext();

            _logger.LogInformation(
                "Fetching historical rates {@HistoricalRequest} CorrelationId={CorrelationId} ClientId={ClientId}",
                ratesRequest, correlationId, clientId);
            try
            {
                string endpoint;

                if (ratesRequest.StartDate.HasValue && ratesRequest.EndDate.HasValue)
                {
                    endpoint = $"{ratesRequest.StartDate:yyyy-MM-dd}..{ratesRequest.EndDate:yyyy-MM-dd}";
                }
                else
                {
                    endpoint = "latest";
                }

                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(ratesRequest.BaseCurrency))
                    queryParams.Add($"base={ratesRequest.BaseCurrency}");

                if (!string.IsNullOrWhiteSpace(ratesRequest.Symbols))
                    queryParams.Add($"symbols={ratesRequest.Symbols}");

                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);

                _logger.LogDebug(
                    "Requesting historical endpoint {Endpoint} CorrelationId={CorrelationId} ClientId={ClientId}",
                    endpoint, correlationId, clientId);

                var historical = await _httpClient.GetFromJsonAsync<HistoricalRateResponse>(endpoint);

                if (historical?.HistoricalRates == null || historical.HistoricalRates.Count == 0)
                {
                    _logger.LogError(
                        "Failed to fetch historical rates from endpoint {Endpoint} CorrelationId={CorrelationId} ClientId={ClientId}",
                        endpoint, correlationId, clientId);
                    throw new Exception("Failed to fetch historical currency rates.");
                }

                _logger.LogInformation(
                    "Successfully fetched historical rates from {StartDate} to {EndDate} CorrelationId={CorrelationId} ClientId={ClientId}",
                    ratesRequest.StartDate, ratesRequest.EndDate, correlationId, clientId);

                var all = historical.HistoricalRates.Select(kv => new HistoricalRateItem
                {
                    Date = DateTime.Parse(kv.Key),
                    Rates = kv.Value,
                }).OrderByDescending(x => x.Date).ToList();

                var total = all.Count;
                var totalPages = (int)Math.Ceiling(total / (double)ratesRequest.PageSize);

                var items = all.Skip((ratesRequest.PageNumber - 1) * ratesRequest.PageSize).Take(ratesRequest.PageSize).ToList();

                var cacheKey = $"{_claimService.GetCurrencyProvider()}:historical:{ratesRequest.StartDate:yyyy-MM-dd}_{ratesRequest.EndDate:yyyy-MM-dd}:{ratesRequest.BaseCurrency ?? ""}_{ratesRequest.Symbols ?? ""}";
                await _cacheService.SetAsync(cacheKey, historical, TimeSpan.FromMinutes(1));

                return new ApiResponse
                {
                    Data = new HistoricalRateApiResponse
                    {
                        BaseCurrency = historical.BaseCurrency,
                        StartDate = ratesRequest.StartDate,
                        EndDate = ratesRequest.EndDate,
                        HistoricalRates = items,
                        Page = ratesRequest.PageNumber,
                        PageSize = ratesRequest.PageSize,
                        TotalItems = total,
                        TotalPages = totalPages
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while fetching historical rates {@HistoricalRequest} CorrelationId={CorrelationId} ClientId={ClientId}",
                    ratesRequest, correlationId, clientId);
                throw;
            }
        }

        public async Task<ApiResponse> ConvertCurrencyAsync(ConvertExchangeRatesRequest ratesRequest)
        {
            var (correlationId, clientId) = GetLogContext();

            _logger.LogInformation(
                "Converting currency {@ConvertRequest} CorrelationId={CorrelationId} ClientId={ClientId}",
                ratesRequest, correlationId, clientId);

            try
            {
                if (string.IsNullOrWhiteSpace(ratesRequest.From) || string.IsNullOrWhiteSpace(ratesRequest.To))
                {
                    _logger.LogWarning(
                        "Invalid conversion request From={From} To={To} CorrelationId={CorrelationId} ClientId={ClientId}",
                        ratesRequest.From, ratesRequest.To, correlationId, clientId);

                    throw new ArgumentException("Currency codes must be provided.");
                }

                var endpoint = $"latest?base={ratesRequest.From}&symbols={ratesRequest.To}";
                _logger.LogDebug("Requesting conversion endpoint: {Endpoint}", endpoint);

                var response = await _httpClient.GetFromJsonAsync<RateResponseDto>(endpoint);

                if (response == null)
                {
                    _logger.LogError(
                        "Failed to fetch conversion rate from endpoint {Endpoint} CorrelationId={CorrelationId} ClientId={ClientId}",
                        endpoint, correlationId, clientId);

                    throw new Exception("Failed to fetch conversion rate.");
                }

                var result = new ConvertExchangeRatesResponse
                {
                    Date = response.Date,
                    From = ratesRequest.From,
                    To = ratesRequest.To,
                    Amount = ratesRequest.Amount,
                    ConvertedAmounts = response.Rates.ToDictionary(
                        kvp => kvp.Key,
                        kvp => Math.Round(ratesRequest.Amount * kvp.Value, 2))
                };

                _logger.LogInformation(
                    "Conversion completed From={From} To={To} Amount={Amount} Converted={Converted} CorrelationId={CorrelationId} ClientId={ClientId}",
                    ratesRequest.From,
                    ratesRequest.To,
                    ratesRequest.Amount,
                    string.Join(", ", result.ConvertedAmounts.Select(kvp => $"{kvp.Key}:{kvp.Value}")),
                    correlationId,
                    clientId
                );

                return new ApiResponse { Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while converting currency {@ConvertRequest} CorrelationId={CorrelationId} ClientId={ClientId}",
                    ratesRequest, correlationId, clientId);
                throw;
            }
        }

        private (string? CorrelationId, string? ClientId) GetLogContext()
        {
            var correlationId = _claimService.GetCorrelationId();
            var clientId = _claimService.GetClientId();
            return (correlationId, clientId);
        }
    }
}