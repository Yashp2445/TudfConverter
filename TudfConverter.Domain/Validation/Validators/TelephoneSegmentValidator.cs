using System.Linq;
using FluentValidation;
using TudfConverter.Domain.Models;

namespace TudfConverter.Domain.Validation.Validators;

public class TelephoneSegmentValidator : AbstractValidator<TelephoneModel>
{
    public TelephoneSegmentValidator()
    {
        RuleFor(x => x.TelephoneNumber)
            .NotEmpty()
            .WithErrorCode("PT-01")
            .WithMessage("Telephone number is required.");

        RuleFor(x => x.TelephoneNumber)
            .Must(x => {
                if (string.IsNullOrWhiteSpace(x)) return true;
                return x.Count(char.IsDigit) >= 5;
            })
            .WithErrorCode("PT-01-LEN")
            .WithName("TelephoneNumber")
            .WithMessage("Telephone number must be at least 5 digits.");

        RuleFor(x => x.TelephoneNumber)
            .Must(x => {
                if (string.IsNullOrWhiteSpace(x)) return true;
                var digits = new string(x.Where(char.IsDigit).ToArray());
                return !digits.StartsWith("1");
            })
            .WithErrorCode("PT-01-START")
            .WithName("TelephoneNumber")
            .WithMessage("Telephone number must not start with digit 1.");

        RuleFor(x => x)
            .Must(x => {
                if (x.TelephoneType == "01" && !string.IsNullOrWhiteSpace(x.TelephoneNumber))
                {
                    return IsValidIndianMobile(x.TelephoneNumber);
                }
                return true;
            })
            .WithErrorCode("PT-01-MOBILE")
            .WithName("TelephoneNumber")
            .WithMessage("Indian mobile numbers must start with 5, 6, 7, 8, or 9 and contain at least 10 digits.");

        RuleFor(x => x.TelephoneType)
            .Must(x => x == "00" || x == "01" || x == "02" || x == "03")
            .WithErrorCode("PT-03")
            .WithName("TelephoneType")
            .WithMessage("Telephone type is invalid. Valid values are 00 Not Classified, 01 Mobile, 02 Home, 03 Office.");
    }

    private static bool IsValidIndianMobile(string number)
    {
        var digits = new string(number.Where(char.IsDigit).ToArray());
        
        if (digits.StartsWith("91") && digits.Length > 10)
        {
            digits = digits.Substring(2);
        }
        else if (digits.StartsWith("0") && digits.Length > 10)
        {
            digits = digits.Substring(1);
        }

        if (digits.Length < 10) return false;

        var first = digits[0];
        return first == '5' || first == '6' || first == '7' || first == '8' || first == '9';
    }
}
