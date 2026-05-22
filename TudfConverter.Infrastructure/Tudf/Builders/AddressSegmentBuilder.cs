using System.Linq;
using System.Text;
using TudfConverter.Domain.Models;

namespace TudfConverter.Infrastructure.Tudf.Builders;

/// <summary>
/// Builds the Address (PA) segment. Tags: A01 through A05.
/// </summary>
public class AddressSegmentBuilder
{
    public string Build(AddressModel address)
    {
        var sb = new StringBuilder();

        // Segment tag: A + index zero-padded to 2 digits
        var segTag = "A" + address.SegmentIndex.ToString("D2");
        sb.Append(segTag);

        // Tag 01: AddressLine1
        sb.Append(TudfFieldFormatter.FormatVariableField("01", address.AddressLine1));

        // Tag 02: AddressLine2 (optional)
        if (!string.IsNullOrEmpty(address.AddressLine2))
            sb.Append(TudfFieldFormatter.FormatVariableField("02", address.AddressLine2));

        // Tag 03: AddressLine3 (optional)
        if (!string.IsNullOrEmpty(address.AddressLine3))
            sb.Append(TudfFieldFormatter.FormatVariableField("03", address.AddressLine3));

        // Tag 04: AddressLine4 (optional)
        if (!string.IsNullOrEmpty(address.AddressLine4))
            sb.Append(TudfFieldFormatter.FormatVariableField("04", address.AddressLine4));

        // Tag 05: AddressLine5 (optional)
        if (!string.IsNullOrEmpty(address.AddressLine5))
            sb.Append(TudfFieldFormatter.FormatVariableField("05", address.AddressLine5));

        // Tag 06: StateCode as fixed 2-byte field (optional)
        if (!string.IsNullOrEmpty(address.StateCode))
            sb.Append(TudfFieldFormatter.FormatVariableField("06", TudfFieldFormatter.FormatFixedNumeric(address.StateCode, 2)));

        // Tag 07: PinCode - strip non-digits, write as 6-digit variable field
        if (!string.IsNullOrEmpty(address.PinCode))
        {
            var digitsOnly = new string(address.PinCode.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length > 0)
            {
                var pinFormatted = TudfFieldFormatter.FormatFixedNumeric(digitsOnly, 6);
                sb.Append(TudfFieldFormatter.FormatVariableField("07", pinFormatted));
            }
        }

        // Tag 08: AddressCategory, default to 04 if null
        var addrCat = address.AddressCategory.HasValue
            ? address.AddressCategory.Value.ToString("D2")
            : "04";
        sb.Append(TudfFieldFormatter.FormatVariableField("08", addrCat));

        // Tag 09: ResidenceCode (optional)
        if (address.ResidenceCode.HasValue)
        {
            sb.Append(TudfFieldFormatter.FormatVariableField("09", address.ResidenceCode.Value.ToString("D2")));
        }

        return sb.ToString();
    }
}
