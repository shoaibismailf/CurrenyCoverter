using Currency.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Services
{
    public class ProviderFactoryService(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor) : IProviderFactoryService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor= httpContextAccessor;

        public ICurrencyProvider GetRequiredService(string providerName)
        {
            return providerName.ToLower() switch
            {
                "frankfurter" => _serviceProvider.GetRequiredService<FrankFurterProviderService>(),
                "openexchange" => _serviceProvider.GetRequiredService<OpenExchangeRatesProviderService>(),
                _ => throw new NotSupportedException($"Provider {providerName} is not supported.")
            };
        }

        public string? GetProviderName()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(c => c.Type == "currency_provider")?.Value;
        }
    }
}
