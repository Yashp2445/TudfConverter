namespace TudfConverter.Domain.Validation.IdValidators;

/// <summary>
/// Defines validation contracts for individual government-issued identity documents.
/// </summary>
public interface IIdNumberValidator
{
    int IdType { get; }
    bool Validate(string idNumber);
}
