using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Interfaces
{
    public interface IProviderFactoryService
    {
        ICurrencyProvider GetRequiredService(string providerName);
        string? GetProviderName();
    }
}
