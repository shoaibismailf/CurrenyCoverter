using System.Security.Claims;
using Currency.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Currency.Application.Helpers.Extensions
{
    public sealed class ClaimService : IClaimService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimService(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        public ClaimsPrincipal? CurrentUser => _httpContextAccessor.HttpContext?.User;
        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        private string? GetClaim(string claimType) =>
            CurrentUser?.FindFirst(claimType)?.Value;

        private int? GetIntClaim(string claimType) =>
            int.TryParse(GetClaim(claimType), out var v) ? v : (int?)null;

        public string? GetUserId() =>
            GetClaim(ClaimTypes.NameIdentifier) ?? GetClaim("sub");

        public string? GetClientId() =>
            GetClaim("client_id");

        public string? GetUserRole() =>
            GetClaim(ClaimTypes.Role) ?? GetClaim("role");

        public string? GetCurrencyProvider() =>
            GetClaim("currency_provider");

        public int GetRateLimit(int defaultValue = 3) =>
            GetIntClaim("rate_limit") ?? defaultValue;

        public string GetCorrelationId()
        {
            if (HttpContext == null)
                return Guid.NewGuid().ToString();

            var correlationId = HttpContext.Request.Headers["X-Correlation-ID"].ToString();

            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            // Ensure it’s available in the response as well
            HttpContext.Response.Headers["X-Correlation-ID"] = correlationId;

            return correlationId;
        }
    }

}
