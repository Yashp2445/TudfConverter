using System;
using System.Threading;
using System.Threading.Tasks;

namespace TudfConverter.Application.Interfaces
{
    public interface IFileExportService
    {
        Task WriteAsync(string filePath, string content, CancellationToken ct = default);
        Task<string> BuildOutputFilePathAsync(string outputFolder, string memberUserId, DateOnly reportDate);
    }
}
