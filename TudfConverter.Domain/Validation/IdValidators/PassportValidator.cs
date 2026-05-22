using System;

namespace TudfConverter.Domain.Validation.IdValidators;

/// <summary>
/// Validates Indian Passport details (ID Type 2).
/// </summary>
public class PassportValidator : IIdNumberValidator
{
    public int IdType => 2;

    public bool Validate(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber)) return false;

        // Strip spaces and hyphens
        string cleaned = idNumber.Replace(" ", "").Replace("-", "");
        
        // Minimum 7 characters, maximum 10 characters
        if (cleaned.Length < 7 || cleaned.Length > 10) return false;

        // Determine number of leading letters (first one or two characters must be letters)
        int letterCount = 0;
        if (char.IsAsciiLetter(cleaned[0]))
        {
            letterCount = 1;
            if (cleaned.Length > 1 && char.IsAsciiLetter(cleaned[1]))
            {
                letterCount = 2;
            }
        }

        if (letterCount == 0) return false;

        // Remaining characters must be digits
        for (int i = letterCount; i < cleaned.Length; i++)
        {
            if (!char.IsDigit(cleaned[i])) return false;
        }

        return true;
    }
}
