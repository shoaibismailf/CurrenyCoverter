namespace Currency.Application.Constants
{
    public class CurrencyConstants
    {
        public static readonly HashSet<string> ExcludedCurrencies = new(StringComparer.OrdinalIgnoreCase)
        {
            "TRY", "PLN", "THB", "MXN"
        };

        public const string ExcludedCurrencyErrorMessage = "Currency conversion is not supported for TRY, PLN, THB, and MXN currencies.";
    }
}
