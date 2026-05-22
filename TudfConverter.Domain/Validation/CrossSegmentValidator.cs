using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TudfConverter.Domain.Models;
using TudfConverter.Domain.Validation.Results;

namespace TudfConverter.Domain.Validation;

public class CrossSegmentValidator
{
    private readonly ILogger<CrossSegmentValidator> _logger;

    public CrossSegmentValidator(ILogger<CrossSegmentValidator> logger)
    {
        _logger = logger;
    }

    public List<ValidationError> Validate(ConsumerRecord record, DateOnly headerDateReported)
    {
        var errors = new List<ValidationError>();

        bool hasIds = record.Identifications != null && record.Identifications.Any();
        bool hasPhones = record.Telephones != null && record.Telephones.Any();

        if (!hasIds && !hasPhones && record.Account.DateOpenedDisbursed >= new DateOnly(2007, 6, 1))
        {
            errors.Add(new ValidationError { ErrorCode = "CROSS-01", Outcome = FailureOutcome.RejectRecord, SegmentTag = "CROSS", FieldName = "ID/Phone", ErrorMessage = "Record must have at least one valid ID segment or one valid telephone number for accounts opened on or after June 1 2007.", RowNumber = record.RowNumber });
        }

        if (record.Name.DateOfBirth.HasValue && record.Name.DateOfBirth.Value > headerDateReported)
        {
            errors.Add(new ValidationError { ErrorCode = "CROSS-02", Outcome = FailureOutcome.RejectRecord, SegmentTag = "CROSS", FieldName = "DateOfBirth", ErrorMessage = "Date of Birth must be on or before the Date Reported and Certified in the file header.", RowNumber = record.RowNumber });
        }

        if (record.Name.DateOfBirth.HasValue && record.Account.DateOfLastPayment.HasValue && record.Name.DateOfBirth.Value > record.Account.DateOfLastPayment.Value)
        {
            errors.Add(new ValidationError { ErrorCode = "CROSS-03", Outcome = FailureOutcome.RejectRecord, SegmentTag = "CROSS", FieldName = "DateOfBirth", ErrorMessage = "Date of Birth must be on or before the Date of Last Payment.", RowNumber = record.RowNumber });
        }

        if (record.Name.DateOfBirth.HasValue && record.Account.DateClosed.HasValue && record.Name.DateOfBirth.Value > record.Account.DateClosed.Value)
        {
            errors.Add(new ValidationError { ErrorCode = "CROSS-04", Outcome = FailureOutcome.RejectRecord, SegmentTag = "CROSS", FieldName = "DateOfBirth", ErrorMessage = "Date of Birth must be on or before the Date Closed.", RowNumber = record.RowNumber });
        }

        if (record.Account.DateOpenedDisbursed > headerDateReported)
        {
            errors.Add(new ValidationError { ErrorCode = "CROSS-05", Outcome = FailureOutcome.RejectRecord, SegmentTag = "CROSS", FieldName = "DateOpenedDisbursed", ErrorMessage = "Date Opened or Disbursed must be on or before the Date Reported and Certified in the file header.", RowNumber = record.RowNumber });
        }

        if (record.Account.DateReportedAndCertified > headerDateReported)
        {
            errors.Add(new ValidationError { ErrorCode = "CROSS-06", Outcome = FailureOutcome.RejectRecord, SegmentTag = "CROSS", FieldName = "DateReportedAndCertified", ErrorMessage = "Account Date Reported and Certified must not be later than the file header Date Reported and Certified.", RowNumber = record.RowNumber });
        }

        if (record.Account.DateReportedAndCertified < headerDateReported.AddYears(-1))
        {
            errors.Add(new ValidationError { ErrorCode = "CROSS-07", Outcome = FailureOutcome.RejectRecord, SegmentTag = "CROSS", FieldName = "DateReportedAndCertified", ErrorMessage = "Account Date Reported and Certified cannot be more than one year before the file header Date Reported and Certified.", RowNumber = record.RowNumber });
        }

        if (record.Addresses == null || !record.Addresses.Any())
        {
            errors.Add(new ValidationError { ErrorCode = "CROSS-08", Outcome = FailureOutcome.RejectRecord, SegmentTag = "CROSS", FieldName = "Addresses", ErrorMessage = "At least one valid address segment must be present in the record.", RowNumber = record.RowNumber });
        }

        if (record.Name.FullName != null)
        {
            var cleanName = record.Name.FullName.Replace("Mr.", "").Replace("Mrs.", "").Replace("Ms.", "").Replace("Dr.", "").Replace("Shri.", "").Replace("Smt.", "")
                            .Replace("Mr ", "").Replace("Mrs ", "").Replace("Ms ", "").Replace("Dr ", "").Replace("Shri ", "").Replace("Smt ", "");

            var tokens = cleanName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 1 && !hasIds && !hasPhones)
            {
                errors.Add(new ValidationError { ErrorCode = "CROSS-09", Outcome = FailureOutcome.RejectRecord, SegmentTag = "CROSS", FieldName = "Name/ID", ErrorMessage = "A single-name borrower requires at least one valid ID document or a valid mobile number.", RowNumber = record.RowNumber });
            }
        }

        return errors;
    }
}
