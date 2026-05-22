using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TudfConverter.Domain.Validation.Results;

namespace TudfConverter.Application.Interfaces;

public interface IValidationReportWriter
{
    Task WriteAsync(string reportFilePath, List<RecordValidationResult> results, FileProcessingResult summary, CancellationToken ct = default);
}
