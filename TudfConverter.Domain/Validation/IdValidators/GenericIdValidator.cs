using System;

namespace TudfConverter.Domain.Validation.IdValidators;

/// <summary>
/// A generic validator for ID types that require no specific structural validation (ID Types 3, 4, 5, 9, 10).
/// </summary>
public class GenericIdValidator : IIdNumberValidator
{
    public int IdType { get; }

    public GenericIdValidator(int idType)
    {
        IdType = idType;
    }

    public bool Validate(string idNumber)
    {
        // Spec: The Validate method returns true as long as the value is not null or empty
        return !string.IsNullOrWhiteSpace(idNumber);
    }
}
