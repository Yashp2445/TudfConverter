using System.Collections.Generic;
using FluentValidation.Results;
using TudfConverter.Domain.Validation.Results;

namespace TudfConverter.Application.Helpers;

public static class ValidationErrorMapper
{
    private static readonly Dictionary<string, FailureOutcome> OutcomeMap = new()
    {
        { "PN-01", FailureOutcome.RejectRecord },
        { "PN-NUM", FailureOutcome.RejectRecord },
        { "PN-CHAR", FailureOutcome.RejectRecord },
        { "PN-TOKEN", FailureOutcome.RejectRecord },
        { "PN-08", FailureOutcome.RejectRecord },
        { "PN-07", FailureOutcome.RejectField },

        { "PA-01", FailureOutcome.RejectSegment },
        { "PA-06", FailureOutcome.RejectSegment },
        { "PA-07", FailureOutcome.RejectSegment }, // Simplifying to RejectSegment
        { "PA-08", FailureOutcome.RejectField },

        { "ID-01", FailureOutcome.RejectSegment },
        { "ID-02", FailureOutcome.RejectSegment },
        { "ID-02-FMT", FailureOutcome.RejectSegment },
        { "ID-03", FailureOutcome.RejectField },

        { "TL-01", FailureOutcome.RejectRecord },
        { "TL-03", FailureOutcome.RejectRecord },
        { "TL-04", FailureOutcome.RejectRecord },
        { "TL-05", FailureOutcome.RejectRecord },
        { "TL-12", FailureOutcome.RejectRecord },
        { "TL-13", FailureOutcome.RejectRecord },
        { "TL-08", FailureOutcome.RejectRecord },
        { "TL-09A", FailureOutcome.RejectField },
        { "TL-10A", FailureOutcome.RejectRecord },
        { "TL-10B", FailureOutcome.RejectRecord },
        { "TL-10C", FailureOutcome.RejectRecord },
        { "TL-11", FailureOutcome.RejectRecord },
        { "TL-14A", FailureOutcome.RejectRecord },
        { "TL-15A", FailureOutcome.RejectRecord },
        { "TL-15-26", FailureOutcome.RejectRecord },
        { "TL-05A", FailureOutcome.RejectRecord },
        { "TL-36", FailureOutcome.RejectField },
        { "TL-37", FailureOutcome.RejectField },
        { "TL-40", FailureOutcome.RejectField },
        { "TL-41", FailureOutcome.RejectRecord },
        { "TL-42", FailureOutcome.RejectField },
        { "TL-43", FailureOutcome.RejectField },
        { "TL-22A", FailureOutcome.RejectRecord },
        { "TL-40B", FailureOutcome.RejectField },
        { "TL-47", FailureOutcome.RejectField },
        { "TL-15-CAP", FailureOutcome.RejectField }, // Treat as warning/RejectField

        { "PT-01", FailureOutcome.RejectSegment },
        { "PT-01-LEN", FailureOutcome.RejectSegment },
        { "PT-01-START", FailureOutcome.RejectSegment },
        { "PT-01-MOBILE", FailureOutcome.RejectSegment },
        { "PT-03", FailureOutcome.RejectField },

        { "EC-01", FailureOutcome.RejectSegment }
    };

    public static List<ValidationError> ToValidationErrors(ValidationResult result, int rowNumber, string segmentTag)
    {
        var errors = new List<ValidationError>();
        foreach (var failure in result.Errors)
        {
            var outcome = OutcomeMap.TryGetValue(failure.ErrorCode, out var mapped) ? mapped : FailureOutcome.RejectField;

            errors.Add(new ValidationError
            {
                ErrorCode = failure.ErrorCode,
                Outcome = outcome,
                SegmentTag = segmentTag,
                FieldName = failure.PropertyName,
                ErrorMessage = failure.ErrorMessage,
                RowNumber = rowNumber
            });
        }
        return errors;
    }
}
