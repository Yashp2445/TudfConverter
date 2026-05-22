using System.Text;
using TudfConverter.Domain.Models;

namespace TudfConverter.Infrastructure.Tudf.Builders;

/// <summary>
/// Builds the Telephone (PT) segment. Tags: T01 through T10.
/// </summary>
public class TelephoneSegmentBuilder
{
    public string Build(TelephoneModel phone)
    {
        var sb = new StringBuilder();

        // Segment tag: PT + 03 + T + index zero-padded to 2 digits
        var segTag = "T" + phone.SegmentIndex.ToString("D2");
        sb.Append("PT");
        sb.Append("03");
        sb.Append(segTag);

        // Tag 01: TelephoneNumber as variable field
        sb.Append(TudfFieldFormatter.FormatVariableField("01", phone.TelephoneNumber));

        // Tag 02: TelephoneExtension as variable field (optional)
        if (!string.IsNullOrEmpty(phone.TelephoneExtension))
        {
            sb.Append(TudfFieldFormatter.FormatVariableField("02", phone.TelephoneExtension));
        }

        // Tag 03: TelephoneType as fixed 2-byte field
        sb.Append(TudfFieldFormatter.FormatVariableField("03", phone.TelephoneType));

        return sb.ToString();
    }
}
