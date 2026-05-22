namespace TudfConverter.Domain.Constants;

/// <summary>
/// Contains valid state codes as defined in Appendix B of the UCRF specification.
/// </summary>
public static class StateCodes
{
    public static readonly HashSet<int> ValidCodes = new()
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
        21, 22, 23, 24, 25, 27, 28, 29, 30,
        31, 32, 33, 34, 35, 36, 37,
        77, // Foreign Address
        99  // APO Address
    };
}
