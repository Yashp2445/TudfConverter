using System;
using System.Threading;
using System.Threading.Tasks;
using TudfConverter.Application.Pipeline;
using TudfConverter.Domain.Validation.Results;

namespace TudfConverter.Application.Interfaces
{
    public interface IFileProcessingService
    {
        Task<FileProcessingResult> ProcessFileAsync(
            string filePath,
            ProcessingOptions options,
            IProgress<ProcessingProgress>? progress = null,
            CancellationToken ct = default);
    }
}
