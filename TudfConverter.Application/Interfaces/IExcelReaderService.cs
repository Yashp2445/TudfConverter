using System.Threading;
using System.Threading.Tasks;
using TudfConverter.Application.Pipeline;

namespace TudfConverter.Application.Interfaces
{
    public interface IExcelReaderService
    {
        Task<ExcelReadResult> ReadAsync(string filePath, CancellationToken ct = default);
    }
}
