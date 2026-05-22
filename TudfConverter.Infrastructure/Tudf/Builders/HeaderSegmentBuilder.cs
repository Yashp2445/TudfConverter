using System;
using System.Text;
using TudfConverter.Domain.Models;

namespace TudfConverter.Infrastructure.Tudf.Builders;

/// <summary>
/// Builds the fixed-length 146-byte Header (TUDF) segment.
/// </summary>
public class HeaderSegmentBuilder
{
    public string Build(HeaderSegmentModel header)
    {
        var sb = new StringBuilder(146);

        // Position 1, length 4: Segment tag
        sb.Append("TUDF");

        // Position 5, length 2: Version
        sb.Append("12");

        // Position 7, length 30: Reporting Member Processor User ID
        sb.Append(TudfFieldFormatter.FormatFixedAlpha(header.MemberUserId, 30));

        // Position 37, length 16: Reporting Member Short Name
        sb.Append(TudfFieldFormatter.FormatFixedAlpha(header.ShortName, 16));

        // Position 53, length 2: Reporting Cycle
        sb.Append(TudfFieldFormatter.FormatFixedAlpha(header.ReportingCycle, 2));

        // Position 55, length 8: Date Reported and Certified (ddMMyyyy)
        sb.Append(header.DateReportedAndCertified.ToString("ddMMyyyy"));

        // Position 63, length 30: Future use (spaces)
        sb.Append(new string(' ', 30));

        // Position 93, length 1: Future use (A)
        sb.Append('A');

        // Position 94, length 5: Future use (zeros)
        sb.Append("00000");

        // Position 99, length 48: Member Data
        sb.Append(TudfFieldFormatter.FormatFixedAlpha(header.MemberData, 48));

        var result = sb.ToString();

        if (result.Length != 146)
        {
            throw new InvalidOperationException(
                $"HD segment length {result.Length} does not equal required 146 bytes.");
        }

        return result;
    }
}
