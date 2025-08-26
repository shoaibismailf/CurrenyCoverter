using Currency.Application.Constants;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static Currency.Application.Models.ValidateRequest;

namespace Currency.Application.Models.Request
{
    public class ConvertExchangeRatesRequest
    {
        [SingleCurrencyCode]
        public required string From { get; set; }

        [CurrencySymbols]
        public required string To { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public required decimal Amount { get; set; }

        public bool IsValid(out string errorMessage)
        {
            if (CurrencyConstants.ExcludedCurrencies.Contains(From))
            {
                errorMessage = $"From currency '{From}' is not supported. {CurrencyConstants.ExcludedCurrencyErrorMessage}";
                return false;
            }

            if (CurrencyConstants.ExcludedCurrencies.Contains(To))
            {
                errorMessage = $"To currency '{To}' is not supported. {CurrencyConstants.ExcludedCurrencyErrorMessage}";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
