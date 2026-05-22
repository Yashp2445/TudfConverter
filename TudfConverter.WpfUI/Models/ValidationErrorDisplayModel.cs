using TudfConverter.Domain.Validation.Results;

namespace TudfConverter.WpfUI.Models;

public class ValidationErrorDisplayModel
{
    public int RowNumber { get; set; }
    public string RecordStatus { get; set; } = string.Empty;
    public string SegmentTag { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;

    public static ValidationErrorDisplayModel FromValidationError(ValidationError error, bool isRecordRejected)
    {
        return new ValidationErrorDisplayModel
        {
            RowNumber = error.RowNumber,
            RecordStatus = isRecordRejected ? "Rejected" : "Accepted",
            SegmentTag = error.SegmentTag ?? "Unknown",
            ErrorCode = error.ErrorCode ?? "N/A",
            FieldName = error.FieldName ?? "N/A",
            ErrorMessage = error.ErrorMessage ?? "Unknown error",
            Outcome = error.Outcome.ToString()
        };
    }
}
