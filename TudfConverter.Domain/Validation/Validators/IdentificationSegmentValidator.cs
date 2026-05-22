using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using TudfConverter.Domain.Constants;
using TudfConverter.Domain.Models;
using TudfConverter.Domain.Validation.IdValidators;

namespace TudfConverter.Domain.Validation.Validators;

public class IdentificationSegmentValidator : AbstractValidator<IdentificationModel>
{
    private readonly IEnumerable<IIdNumberValidator> _idValidators;

    public IdentificationSegmentValidator(IEnumerable<IIdNumberValidator> idValidators)
    {
        _idValidators = idValidators;

        RuleFor(x => x.IdType)
            .Must(type => UcrfCatalogues.ValidIdTypes.Contains(type) && type != 7 && type != 8)
            .WithErrorCode("ID-01")
            .WithName("IdType")
            .WithMessage("ID type is invalid. Valid types are 1 PAN, 2 Passport, 3 Voter ID, 4 Driver License, 5 Ration Card, 6 UID Aadhaar, 9 CKYC, 10 G RAM G. Types 7 and 8 are reserved.");

        RuleFor(x => x.IdNumber)
            .NotEmpty()
            .WithErrorCode("ID-02")
            .WithMessage("ID number is required when ID type is provided.");

        RuleFor(x => x)
            .Must(BeValidFormat)
            .When(x => !string.IsNullOrWhiteSpace(x.IdNumber))
            .WithErrorCode("ID-02-FMT")
            .WithName("IdNumber")
            .WithMessage("ID number format is invalid for the specified ID type. Check the format requirements for PAN, Aadhaar, or Passport as applicable.");

        RuleFor(x => x)
            .Must(x => !x.IssueDate.HasValue || !x.ExpirationDate.HasValue || x.IssueDate.Value < x.ExpirationDate.Value)
            .WithErrorCode("ID-03")
            .WithName("IssueDate")
            .WithMessage("Issue date must be earlier than expiration date.");
    }

    private bool BeValidFormat(IdentificationModel model)
    {
        var validator = _idValidators.FirstOrDefault(v => v.IdType == model.IdType);
        if (validator != null)
        {
            return validator.Validate(model.IdNumber);
        }
        return true;
    }
}
