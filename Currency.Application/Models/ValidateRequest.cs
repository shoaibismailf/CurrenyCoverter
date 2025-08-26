using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Currency.Application.Models
{
    public class ValidateRequest
    {
        public class CurrencySymbolsAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value is string str && !string.IsNullOrWhiteSpace(str))
                {
                    var codes = str.Split(',')
                                   .Select(s => s.Trim())
                                   .ToList();

                    foreach (var code in codes)
                    {
                        if (code.Length != 3 || !Regex.IsMatch(code, "^[A-Z]{3}$"))
                        {
                            // Force error key to "symbols"
                            return new ValidationResult(
                                $"symbols must contain only 3-letter uppercase currency codes separated by commas. Invalid: {code}",
                                new[] { "symbols" }
                            );
                        }
                    }
                }

                return ValidationResult.Success!;
            }
        }

        /// <summary>
        /// Validates that a property contains exactly one 3-letter uppercase code.
        /// Used for baseCurrency.
        /// </summary>
        public class SingleCurrencyCodeAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value is string str && !string.IsNullOrWhiteSpace(str))
                {
                    if (str.Contains(",")) // ❌ no multiple codes
                    {
                        return new ValidationResult(
                            $"{validationContext.MemberName} must contain exactly one 3-letter uppercase currency code (no commas)."
                        );
                    }

                    if (str.Length != 3 || !Regex.IsMatch(str, "^[A-Z]{3}$"))
                    {
                        return new ValidationResult(
                            $"{validationContext.MemberName} must be a 3-letter uppercase currency code."
                        );
                    }
                }

                return ValidationResult.Success!;
            }
        }
    }
}
