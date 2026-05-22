using System.Linq;
using FluentValidation;
using TudfConverter.Domain.Models;

namespace TudfConverter.Domain.Validation.Validators;

public class EmailSegmentValidator : AbstractValidator<EmailModel>
{
    public EmailSegmentValidator()
    {
        RuleFor(x => x.EmailId)
            .Must(IsValidEmail)
            .WithErrorCode("EC-01")
            .WithName("EmailId")
            .WithMessage("Email address format is invalid. Check that it contains exactly one at symbol, no spaces, a valid domain with at least 2 characters before and after the dot, and ends with at least 2 alphabetic characters.");
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        email = email.ToLowerInvariant();

        if (email.Contains(" ")) return false;
        
        var atCount = email.Count(c => c == '@');
        if (atCount != 1) return false;

        var parts = email.Split('@');
        var local = parts[0];
        var domain = parts[1];

        if (local.Count(char.IsLetterOrDigit) < 2) return false;

        if (domain.Count(char.IsLetterOrDigit) < 2) return false;

        if (!domain.Contains('.')) return false;

        if (email.EndsWith("@") || email.EndsWith(".")) return false;

        if (domain.Contains("..")) return false;

        var domainParts = domain.Split('.');
        var firstDomainPart = domainParts[0];
        if (firstDomainPart.Length < 2) return false;

        var lastDomainPart = domainParts.Last();
        if (lastDomainPart.Length < 2 || !lastDomainPart.All(char.IsLetter)) return false;

        return true;
    }
}
