using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TudfConverter.Application.Interfaces;
using TudfConverter.Domain.Models;

namespace TudfConverter.Infrastructure.Tudf;

/// <summary>
/// Implements the TUDF generation service by delegating to the file assembler.
/// </summary>
public class TudfGenerationService : ITudfGenerationService
{
    private readonly TudfFileAssembler _assembler;
    private readonly ILogger<TudfGenerationService> _logger;

    public TudfGenerationService(TudfFileAssembler assembler, ILogger<TudfGenerationService> logger)
    {
        _assembler = assembler;
        _logger = logger;
    }

    public string Generate(List<ConsumerRecord> validRecords, HeaderSegmentModel header)
    {
        _logger.LogInformation("TUDF generation starting with {Count} valid records.", validRecords.Count);

        var result = _assembler.Assemble(validRecords, header);

        _logger.LogInformation("TUDF generation completed. Output length: {Length} bytes.", result.Length);

        return result;
    }
}
