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

        // Split FullName into words
        var words = (name.FullName ?? string.Empty)
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Greedy word-packing: pack words (space-separated) into up to 5 tags, max 25 chars each
        var packedTags = new System.Collections.Generic.List<string>();
        int wordIndex = 0;
        while (wordIndex < words.Length && packedTags.Count < 5)
        {
            var word = words[wordIndex].Length > 25 ? words[wordIndex].Substring(0, 25) : words[wordIndex];
            var current = word;
            wordIndex++;

            while (wordIndex < words.Length)
            {
                var nextWord = words[wordIndex];
                if (current.Length + 1 + nextWord.Length <= 25)
                {
                    current += " " + nextWord;
                    wordIndex++;
                }
                else
                {
                    break;
                }
            }

            packedTags.Add(current);
        }

        for (int i = 0; i < packedTags.Count; i++)
        {
            var tag = (i + 1).ToString("D2"); // 01, 02, 03, 04, 05
            sb.Append(TudfFieldFormatter.FormatVariableField(tag, packedTags[i]));
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
