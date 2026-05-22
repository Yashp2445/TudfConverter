using System.Text;
using TudfConverter.Domain.Models;

namespace TudfConverter.Infrastructure.Tudf.Builders;

/// <summary>
/// Builds the Identification (ID) segment. Tags: I01 through I08.
/// </summary>
public class IdentificationSegmentBuilder
{
    public string Build(IdentificationModel id)
    {
        var sb = new StringBuilder();

        // Segment tag: ID + 03 + I + index zero-padded to 2 digits
        var segTag = "I" + id.SegmentIndex.ToString("D2");
        sb.Append("ID");
        sb.Append("03");
        sb.Append(segTag);

        // Tag 01: IdType as fixed 2-byte numeric
        sb.Append(TudfFieldFormatter.FormatVariableField("01", id.IdType.ToString("D2")));

        // Tag 02: IdNumber as variable field
        sb.Append(TudfFieldFormatter.FormatVariableField("02", id.IdNumber));

        // Tag 03: IssueDate
        if (id.IssueDate.HasValue)
        {
            sb.Append(TudfFieldFormatter.FormatVariableDateField("03", id.IssueDate));
        }

        // Tag 04: ExpirationDate
        if (id.ExpirationDate.HasValue)
        {
            sb.Append(TudfFieldFormatter.FormatVariableDateField("04", id.ExpirationDate));
        }

        return sb.ToString();
    }
}
