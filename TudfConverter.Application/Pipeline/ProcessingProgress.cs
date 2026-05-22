namespace TudfConverter.Application.Pipeline;

/// <summary>
/// Contains metrics representing current pipeline conversion progress.
/// </summary>
public class ProcessingProgress
{
    public required string Message { get; init; }
    public required int Percentage { get; init; }
    public required int ProcessedRows { get; init; }
    public required int TotalRows { get; init; }
}
