using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Interfaces
{
    public interface IClaimService
    {
        ClaimsPrincipal? CurrentUser { get; }

        string? GetUserId();
        string? GetClientId();
        string? GetUserRole();
        string? GetCurrencyProvider();
        int GetRateLimit(int defaultValue = 3);
        string GetCorrelationId();
    }
}
