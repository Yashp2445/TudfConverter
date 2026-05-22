using System;
using System.Linq;
using System.Text;
using TudfConverter.Domain.Models;

namespace TudfConverter.Infrastructure.Tudf.Builders;

/// <summary>
/// Builds the variable-length Name (PN) segment.
/// </summary>
public class NameSegmentBuilder
{
    public string Build(NameSegmentModel name)
    {
        var sb = new StringBuilder();

        // Segment identifier: PN + 03 + segment tag N01
        sb.Append("PN");
        sb.Append("03");
        sb.Append("N01");

        // Split FullName into tokens, take up to 5
        var tokens = (name.FullName ?? string.Empty)
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Take(5)
            .ToArray();

        for (int i = 0; i < tokens.Length; i++)
        {
            var tag = (i + 1).ToString("D2"); // 01, 02, 03, 04, 05
            var token = tokens[i].Length > 26 ? tokens[i].Substring(0, 26) : tokens[i];
            sb.Append(TudfFieldFormatter.FormatVariableField(tag, token));
        }

        // Tag 07: Date of Birth
        if (name.DateOfBirth.HasValue)
        {
            sb.Append(TudfFieldFormatter.FormatVariableDateField("07", name.DateOfBirth));
        }

        // Tag 08: Gender
        if (name.Gender.HasValue)
        {
            sb.Append(TudfFieldFormatter.FormatVariableField("08", name.Gender.Value.ToString()));
        }

        return sb.ToString();
    }
}
