using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TudfConverter.Infrastructure.FileStorage;

public class FileCleanupService
{
    private readonly ILogger<FileCleanupService> _logger;

    public FileCleanupService(ILogger<FileCleanupService> logger)
    {
        _logger = logger;
    }

    public Task CleanOldFilesAsync(string folder, int retentionDays, CancellationToken ct = default)
    {
        if (!Directory.Exists(folder))
        {
            return Task.CompletedTask;
        }

        int deletedCount = 0;
        var cutoffDate = DateTime.Now.AddDays(-retentionDays);

        var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            try
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    fileInfo.Delete();
                    deletedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old file {File}", file);
            }
        }

        _logger.LogInformation("Cleaned up {Count} files older than {Days} days from {Folder}", deletedCount, retentionDays, folder);
        
        return Task.CompletedTask;
    }
}
