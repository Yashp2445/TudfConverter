using System;
using System.Text;

namespace TudfConverter.Domain.Validation.IdValidators;

/// <summary>
/// Validates Indian Aadhaar Card details (ID Type 6) using the Verhoeff checksum algorithm.
/// </summary>
public class AadhaarValidator : IIdNumberValidator
{
    public int IdType => 6;

    // Verhoeff multiplication table ( Cayley table for dihedral group D5 )
    private static readonly int[,] MultiplicationTable = new int[10, 10]
    {
        {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
        {1, 2, 3, 4, 0, 6, 7, 8, 9, 5},
        {2, 3, 4, 0, 1, 7, 8, 9, 5, 6},
        {3, 4, 0, 1, 2, 8, 9, 5, 6, 7},
        {4, 0, 1, 2, 3, 9, 5, 6, 7, 8},
        {5, 9, 8, 7, 6, 0, 4, 3, 2, 1},
        {6, 5, 9, 8, 7, 1, 0, 4, 3, 2},
        {7, 6, 5, 9, 8, 2, 1, 0, 4, 3},
        {8, 7, 6, 5, 9, 3, 2, 1, 0, 4},
        {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
    };

    // Verhoeff permutation table
    private static readonly int[,] PermutationTable = new int[8, 10]
    {
        {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
        {1, 5, 7, 6, 2, 8, 3, 0, 9, 4},
        {5, 8, 0, 3, 7, 9, 6, 1, 4, 2},
        {8, 9, 1, 6, 0, 4, 3, 5, 2, 7},
        {9, 4, 5, 3, 1, 2, 6, 8, 7, 0},
        {4, 2, 8, 6, 5, 7, 3, 9, 0, 1},
        {2, 7, 9, 3, 8, 0, 6, 4, 1, 5},
        {7, 0, 4, 6, 9, 1, 3, 2, 5, 8}
    };

    public bool Validate(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber)) return false;

        // Strip non-digit characters
        var digitsOnly = new StringBuilder();
        foreach (char ch in idNumber)
        {
            if (char.IsDigit(ch))
            {
                digitsOnly.Append(ch);
            }
        }

        string cleaned = digitsOnly.ToString();
        if (cleaned.Length != 12) return false;

        // Perform Verhoeff validation over the 12 digits
        int c = 0;
        for (int i = 0; i < cleaned.Length; i++)
        {
            int digit = cleaned[cleaned.Length - 1 - i] - '0';
            c = MultiplicationTable[c, PermutationTable[i % 8, digit]];
        }

        return c == 0;
    }
}
