using System.Collections.Generic;
using System.Text;
using TudfConverter.Domain.Models;
using TudfConverter.Infrastructure.Tudf.Builders;

namespace TudfConverter.Infrastructure.Tudf;

/// <summary>
/// Assembles the complete TUDF file by combining the header, all consumer records,
/// and the trailer segment into a single continuous string.
/// </summary>
public class TudfFileAssembler
{
    private readonly HeaderSegmentBuilder _headerBuilder;
    private readonly TudfRecordBuilder _recordBuilder;

    public TudfFileAssembler(HeaderSegmentBuilder headerBuilder, TudfRecordBuilder recordBuilder)
    {
        _headerBuilder = headerBuilder;
        _recordBuilder = recordBuilder;
    }

    public string Assemble(List<ConsumerRecord> validRecords, HeaderSegmentModel header)
    {
        var sb = new StringBuilder();

        // Header segment
        sb.Append(_headerBuilder.Build(header));

        // Consumer record segments
        foreach (var record in validRecords)
        {
            sb.Append(_recordBuilder.BuildRecord(record));
        }

        // Trailer segment
        sb.Append("TRLR");

        return sb.ToString();
    }
}
