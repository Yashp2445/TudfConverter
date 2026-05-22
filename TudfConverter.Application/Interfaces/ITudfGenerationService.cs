using System.Collections.Generic;
using TudfConverter.Domain.Models;

namespace TudfConverter.Application.Interfaces
{
    public interface ITudfGenerationService
    {
        string Generate(List<ConsumerRecord> validRecords, HeaderSegmentModel header);
    }
}
