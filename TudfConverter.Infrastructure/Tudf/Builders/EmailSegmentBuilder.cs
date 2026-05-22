using System.Text;
using TudfConverter.Domain.Models;

namespace TudfConverter.Infrastructure.Tudf.Builders;

/// <summary>
/// Builds the Email Contact (EC) segment. Tags: C01 through C10.
/// </summary>
public class EmailSegmentBuilder
{
    public string Build(EmailModel email)
    {
        var sb = new StringBuilder();

        // Segment tag: C + index zero-padded to 2 digits
        var segTag = "C" + email.SegmentIndex.ToString("D2");
        sb.Append(segTag);

        // Tag 01: EmailId as variable field
        sb.Append(TudfFieldFormatter.FormatVariableField("01", email.EmailId));

        return sb.ToString();
    }
}
