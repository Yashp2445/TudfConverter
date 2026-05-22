using System.Linq;
using FluentValidation;
using TudfConverter.Domain.Models;

namespace TudfConverter.Domain.Validation.Validators;

public class NameSegmentValidator : AbstractValidator<NameSegmentModel>
{
    private static readonly char[] DisallowedChars = new[]
    {
        '~', '!', '#', '$', '%', '^', '&', '*', '=', '|', '?', '+', ',', '@'
    };

    public NameSegmentValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithErrorCode("PN-01")
            .WithMessage("Consumer name is required and cannot be empty.");

        RuleFor(x => x.FullName)
            .Must(x => x == null || !x.Any(char.IsDigit))
            .WithErrorCode("PN-NUM")
            .WithName("FullName")
            .WithMessage("Numbers are strictly prohibited in consumer name fields.");

        RuleFor(x => x.FullName)
            .Must(x => x == null || !ContainsDisallowedCharacters(x))
            .WithErrorCode("PN-CHAR")
            .WithName("FullName")
            .WithMessage("Consumer name contains disallowed characters. Check for symbols like tilde, hash, dollar, percent, ampersand, asterisk, at sign.");

        RuleFor(x => x.FullName)
            .Must(HasValidToken)
            .WithErrorCode("PN-TOKEN")
            .WithName("FullName")
            .WithMessage("Consumer name must contain at least one word with 2 or more alphabetic characters.");

        RuleFor(x => x.Gender)
            .Must(x => !x.HasValue || x == 1 || x == 2 || x == 3)
            .WithErrorCode("PN-08")
            .WithName("Gender")
            .WithMessage("Gender code is invalid. Valid values are 1 for Female, 2 for Male, 3 for Transgender.");

        RuleFor(x => x.DateOfBirth)
            .Must(x => !x.HasValue || x.Value.Year > 1900)
            .WithErrorCode("PN-07")
            .WithName("DateOfBirth")
            .WithMessage("Date of Birth must be a valid calendar date.");
    }

    private static bool ContainsDisallowedCharacters(string name)
    {
        if (name.IndexOfAny(DisallowedChars) >= 0)
            return true;

        if (name.Contains('/') || name.Contains('\\'))
        {
            var upper = name.ToUpperInvariant();
            var parts = upper.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (part.Contains('/') || part.Contains('\\'))
                {
                    var normalized = part.Replace('\\', '/');
                    if (normalized != "S/O" && normalized != "W/O" && normalized != "H/O" && normalized != "D/O")
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool HasValidToken(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var tokens = name.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        return tokens.Any(t => t.Count(char.IsLetter) >= 2);
    }
}
