namespace TudfConverter.Domain.Models;

/// <summary>
/// Represents an Address Segment containing borrower addresses and residency information.
/// </summary>
public class AddressModel
{
    public required int SegmentIndex { get; init; }
    public required string AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? AddressLine3 { get; init; }
    public string? AddressLine4 { get; init; }
    public string? AddressLine5 { get; init; }
    public string? StateCode { get; init; }
    public string? PinCode { get; init; }
    public int? AddressCategory { get; init; }
    public int? ResidenceCode { get; init; }
}
