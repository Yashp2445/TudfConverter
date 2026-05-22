using System;
using System.Linq;

namespace TudfConverter.Infrastructure.Tudf.Builders;

/// <summary>
/// Provides static methods for formatting fields according to the TUDF specification.
/// </summary>
public static class TudfFieldFormatter
{
    /// <summary>
    /// Formats a variable-length field: tag (2 chars) + length (2 digits zero-padded) + value.
    /// Returns empty string if value is null or empty.
    /// </summary>
    public static string FormatVariableField(string tag, string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var len = value.Length.ToString("D2");
        return $"{tag}{len}{value}";
    }

    /// <summary>
    /// Formats a numeric value as a variable-length field.
    /// Returns empty string if value is null.
    /// </summary>
    public static string FormatNumericVariableField(string tag, long? value)
    {
        if (!value.HasValue) return string.Empty;
        return FormatVariableField(tag, value.Value.ToString());
    }

    /// <summary>
    /// Formats a signed amount field. If isNegative, appends '-' to the amount string.
    /// </summary>
    public static string FormatSignedAmountField(string tag, long amount, bool isNegative)
    {
        var amountStr = amount.ToString();
        if (isNegative)
        {
            amountStr += "-";
        }
        return FormatVariableField(tag, amountStr);
    }

    /// <summary>
    /// Returns value right-padded with spaces to exactly length characters.
    /// Truncates if value exceeds length.
    /// </summary>
    public static string FormatFixedAlpha(string? value, int length)
    {
        var v = value ?? string.Empty;
        if (v.Length > length)
            v = v.Substring(0, length);
        return v.PadRight(length);
    }

    /// <summary>
    /// Returns value left-padded with zeros to exactly length characters.
    /// Truncates from the left if value exceeds length.
    /// </summary>
    public static string FormatFixedNumeric(string? value, int length)
    {
        var v = value ?? string.Empty;
        if (v.Length > length)
            v = v.Substring(v.Length - length);
        return v.PadLeft(length, '0');
    }

    /// <summary>
    /// Formats a DateOnly as ddMMyyyy. Returns empty string if null.
    /// </summary>
    public static string FormatDate(DateOnly? date)
    {
        if (!date.HasValue) return string.Empty;
        return date.Value.ToString("ddMMyyyy");
    }

    /// <summary>
    /// Formats a date as a variable-length field with the given tag.
    /// Returns empty string if date is null.
    /// </summary>
    public static string FormatVariableDateField(string tag, DateOnly? date)
    {
        if (!date.HasValue) return string.Empty;
        return FormatVariableField(tag, FormatDate(date));
    }
}
