using System;

namespace TudfConverter.Domain.Validation.IdValidators;

/// <summary>
/// Validates Indian Permanent Account Number (PAN) details (ID Type 1).
/// </summary>
public class PanValidator : IIdNumberValidator
{
    public int IdType => 1;

    public bool Validate(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber)) return false;
        
        // Strip spaces and hyphens, convert to uppercase
        string cleaned = idNumber.Replace(" ", "").Replace("-", "").ToUpperInvariant();
        if (cleaned.Length != 10) return false;

        // First three characters must be letters (A-Z)
        for (int i = 0; i < 3; i++)
        {
            if (!char.IsAsciiLetter(cleaned[i])) return false;
        }

        // Fourth character must be either P or H
        char fourth = cleaned[3];
        if (fourth != 'P' && fourth != 'H') return false;

        // Fifth character must be a letter (A-Z)
        if (!char.IsAsciiLetter(cleaned[4])) return false;

        // Characters 6 through 9 must be digits (0-9)
        for (int i = 5; i < 9; i++)
        {
            if (!char.IsDigit(cleaned[i])) return false;
        }

        // Character 10 must be a letter (A-Z)
        if (!char.IsAsciiLetter(cleaned[9])) return false;

        return true;
    }
}
