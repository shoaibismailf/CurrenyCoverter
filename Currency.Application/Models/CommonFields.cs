using Currency.Application.Constants;
using static Currency.Application.Models.ValidateRequest;

namespace Currency.Application.Models
{
    public abstract class CommonFields
    {
        [SingleCurrencyCode] 
        public string BaseCurrency { get; set; } = string.Empty;

        [CurrencySymbols] 
        public string? Symbols { get; set; }
        public virtual bool IsValid(out string errorMessage)
        {
            if (CurrencyConstants.ExcludedCurrencies.Contains(BaseCurrency.ToUpperInvariant()))
            {
                errorMessage = $"Base currency '{BaseCurrency}' is not supported. {CurrencyConstants.ExcludedCurrencyErrorMessage}";
                return false;
            }

            if (!string.IsNullOrEmpty(Symbols))
            {
                var excludedSymbols = Symbols.Split(',')
                    .Select(s => s.Trim().ToUpper())
                    .Where(CurrencyConstants.ExcludedCurrencies.Contains)
                    .ToArray();

                if (excludedSymbols.Any())
                {
                    errorMessage = $"Target currencies '{string.Join(", ", excludedSymbols)}' are not supported. {CurrencyConstants.ExcludedCurrencyErrorMessage}";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
