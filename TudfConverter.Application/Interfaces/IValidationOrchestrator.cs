using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TudfConverter.Application.Pipeline;
using TudfConverter.Domain.Models;
using TudfConverter.Domain.Validation.Results;

namespace TudfConverter.Application.Interfaces
{
    public interface IValidationOrchestrator
    {
        Task<List<RecordValidationResult>> ValidateAllAsync(
            List<ConsumerRecord> records,
            DateOnly headerDateReported,
            IProgress<ProcessingProgress>? progress = null,
            CancellationToken ct = default);
    }
}
