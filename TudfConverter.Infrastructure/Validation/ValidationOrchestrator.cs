using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Logging;
using TudfConverter.Application.Helpers;
using TudfConverter.Application.Interfaces;
using TudfConverter.Application.Pipeline;
using TudfConverter.Domain.Models;
using TudfConverter.Domain.Validation;
using TudfConverter.Domain.Validation.Results;

namespace TudfConverter.Infrastructure.Validation;

public class ValidationOrchestrator : IValidationOrchestrator
{
    private readonly IValidator<NameSegmentModel> _nameValidator;
    private readonly IValidator<AccountSegmentModel> _accountValidator;
    private readonly IValidator<AddressModel> _addressValidator;
    private readonly IValidator<IdentificationModel> _idValidator;
    private readonly IValidator<TelephoneModel> _telephoneValidator;
    private readonly IValidator<EmailModel> _emailValidator;
    private readonly CrossSegmentValidator _crossSegmentValidator;
    private readonly ILogger<ValidationOrchestrator> _logger;

    public ValidationOrchestrator(
        IValidator<NameSegmentModel> nameValidator,
        IValidator<AccountSegmentModel> accountValidator,
        IValidator<AddressModel> addressValidator,
        IValidator<IdentificationModel> idValidator,
        IValidator<TelephoneModel> telephoneValidator,
        IValidator<EmailModel> emailValidator,
        CrossSegmentValidator crossSegmentValidator,
        ILogger<ValidationOrchestrator> logger)
    {
        _nameValidator = nameValidator;
        _accountValidator = accountValidator;
        _addressValidator = addressValidator;
        _idValidator = idValidator;
        _telephoneValidator = telephoneValidator;
        _emailValidator = emailValidator;
        _crossSegmentValidator = crossSegmentValidator;
        _logger = logger;
    }

    public async Task<List<RecordValidationResult>> ValidateAllAsync(
        List<ConsumerRecord> records,
        DateOnly headerDateReported,
        IProgress<ProcessingProgress>? progress = null,
        CancellationToken ct = default)
    {
        var results = new List<RecordValidationResult>();
        int count = 0;

        foreach (var record in records)
        {
            if (ct.IsCancellationRequested) break;

            var result = new RecordValidationResult { RowNumber = record.RowNumber };
            
            // Name
            var nameRes = await _nameValidator.ValidateAsync(record.Name, ct);
            result.Errors.AddRange(ValidationErrorMapper.ToValidationErrors(nameRes, record.RowNumber, "PN"));

            // Addresses
            int validAddresses = 0;
            foreach (var addr in record.Addresses)
            {
                var addrRes = await _addressValidator.ValidateAsync(addr, ct);
                var addrErrors = ValidationErrorMapper.ToValidationErrors(addrRes, record.RowNumber, "PA");
                result.Errors.AddRange(addrErrors);
                if (!addrErrors.Any(e => e.Outcome == FailureOutcome.RejectSegment || e.Outcome == FailureOutcome.RejectRecord))
                {
                    validAddresses++;
                }
            }
            if (validAddresses == 0 && record.Addresses.Any())
            {
                result.IsRecordRejected = true;
            }

            // IDs
            foreach (var id in record.Identifications)
            {
                var idRes = await _idValidator.ValidateAsync(id, ct);
                result.Errors.AddRange(ValidationErrorMapper.ToValidationErrors(idRes, record.RowNumber, "ID"));
            }

            // Telephones
            foreach (var phone in record.Telephones)
            {
                var ptRes = await _telephoneValidator.ValidateAsync(phone, ct);
                result.Errors.AddRange(ValidationErrorMapper.ToValidationErrors(ptRes, record.RowNumber, "PT"));
            }

            // Emails
            foreach (var email in record.Emails)
            {
                var ecRes = await _emailValidator.ValidateAsync(email, ct);
                result.Errors.AddRange(ValidationErrorMapper.ToValidationErrors(ecRes, record.RowNumber, "EC"));
            }

            // Account
            var accRes = await _accountValidator.ValidateAsync(record.Account, ct);
            result.Errors.AddRange(ValidationErrorMapper.ToValidationErrors(accRes, record.RowNumber, "TL"));

            // Cross Segment
            var crossErrors = _crossSegmentValidator.Validate(record, headerDateReported);
            foreach (var ce in crossErrors)
            {
                result.Errors.Add(ce);
            }

            if (result.Errors.Any(e => e.Outcome == FailureOutcome.RejectRecord))
            {
                result.IsRecordRejected = true;
            }

            if (result.IsRecordRejected)
            {
                var codes = string.Join(", ", result.Errors.Select(e => e.ErrorCode));
                _logger.LogWarning("Row {RowNumber} rejected. Errors: {Codes}", record.RowNumber, codes);
            }

            results.Add(result);

            count++;
            if (progress != null && count % 100 == 0)
            {
                progress.Report(new ProcessingProgress 
                { 
                    ProcessedRows = count, 
                    TotalRows = records.Count,
                    Percentage = (int)((double)count / records.Count * 100),
                    Message = $"Validated {count} records"
                });
            }
        }

        return results;
    }
}
