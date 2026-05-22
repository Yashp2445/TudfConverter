using System.Collections.Generic;
using System.Linq;

namespace TudfConverter.Domain.Validation.Results;

/// <summary>
/// Aggregates all validation errors associated with a specific row.
/// </summary>
public class RecordValidationResult
{
    public required int RowNumber { get; init; }
    public bool IsRecordRejected { get; set; }
    public List<ValidationError> Errors { get; }

    public bool HasErrors => Errors.Count > 0;
    
    public bool HasRecordLevelErrors => Errors.Any(e => e.Outcome == FailureOutcome.RejectRecord);

    public RecordValidationResult()
    {
        Errors = new List<ValidationError>();
    }
}
