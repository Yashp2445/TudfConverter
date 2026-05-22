using System.Linq;
using FluentValidation;
using TudfConverter.Domain.Constants;
using TudfConverter.Domain.Models;

namespace TudfConverter.Domain.Validation.Validators;

public class AddressSegmentValidator : AbstractValidator<AddressModel>
{
    public AddressSegmentValidator()
    {
        RuleFor(x => x)
            .Must(HaveValidAddressLines)
            .WithErrorCode("PA-01")
            .WithName("AddressLines")
            .WithMessage("Address Line 1 is required. The combined address must be at least 3 characters.");

        RuleFor(x => x.StateCode)
            .Must(code => string.IsNullOrEmpty(code) || (int.TryParse(code, out int c) && StateCodes.ValidCodes.Contains(c)))
            .WithErrorCode("PA-06")
            .WithName("StateCode")
            .WithMessage("State code is invalid. Refer to Appendix B for valid state and union territory codes.");

        RuleFor(x => x)
            .Must(HaveValidPinCode)
            .WithErrorCode("PA-07")
            .WithName("PinCode")
            .WithMessage("PIN code is invalid. Must be exactly 6 digits, last 3 digits cannot all be zero, and the first 2 digits must match the valid range for the given state code.");

        RuleFor(x => x.AddressCategory)
            .Must(cat => !cat.HasValue || (cat.Value >= 1 && cat.Value <= 5))
            .WithErrorCode("PA-08")
            .WithName("AddressCategory")
            .WithMessage("Address category is invalid. Valid values are 1 for Permanent, 2 for Residence, 3 for Office, 4 for Not Categorized, 5 for Mortgage Property.");
    }

    private bool HaveValidAddressLines(AddressModel model)
    {
        if (string.IsNullOrWhiteSpace(model.AddressLine1)) return false;
        
        int length = (model.AddressLine1?.Length ?? 0) +
                     (model.AddressLine2?.Length ?? 0) +
                     (model.AddressLine3?.Length ?? 0) +
                     (model.AddressLine4?.Length ?? 0) +
                     (model.AddressLine5?.Length ?? 0);
                     
        return length >= 3;
    }

    private bool HaveValidPinCode(AddressModel model)
    {
        if (string.IsNullOrWhiteSpace(model.PinCode)) return true;

        var digitsOnly = new string(model.PinCode.Where(char.IsDigit).ToArray());
        
        if (model.StateCode == "77") return true;
        
        if (digitsOnly.Length != 6) return false;
        if (digitsOnly.EndsWith("000")) return false;

        if (!int.TryParse(digitsOnly, out int pin)) return false;

        if (model.StateCode == "99")
        {
            return pin >= 900000 && pin <= 999999;
        }

        if (string.IsNullOrEmpty(model.StateCode) || !int.TryParse(model.StateCode, out int stateCodeInt)) return true;

        if (PinCodeRanges.Ranges.TryGetValue(stateCodeInt, out var range))
        {
            int prefix = pin / 10000;
            return prefix >= range.Min && prefix <= range.Max;
        }

        return false;
    }
}
