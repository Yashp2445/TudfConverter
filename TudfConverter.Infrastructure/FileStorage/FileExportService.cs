using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TudfConverter.Application.Interfaces;

namespace TudfConverter.Infrastructure.FileStorage;

public class FileExportService : IFileExportService
{
    private readonly ILogger<FileExportService> _logger;

    public FileExportService(ILogger<FileExportService> logger)
    {
        _logger = logger;
    }

    public async Task WriteAsync(string filePath, string content, CancellationToken ct = default)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // UTF8 without BOM
            var encoding = new UTF8Encoding(false);
            await File.WriteAllTextAsync(filePath, content, encoding, ct);

            _logger.LogInformation("File written to {Path}. Size: {Bytes} bytes.", filePath, encoding.GetByteCount(content));
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to write output file at {Path}", filePath);
            throw new InvalidOperationException($"Failed to write output file at {filePath}: {ex.Message}", ex);
        }
    }

    public Task<string> BuildOutputFilePathAsync(string outputFolder, string memberUserId, DateOnly reportDate)
    {
        var trimmedMemberId = (memberUserId ?? string.Empty).Trim().Replace(" ", "");
        var filename = $"{trimmedMemberId}_{reportDate:ddMMyyyy}_{DateTime.Now:HHmmss}.tudf";
        return Task.FromResult(Path.Combine(outputFolder, filename));
    }
}
