using System.Collections.Generic;

namespace TudfConverter.Domain.Constants;

/// <summary>
/// Contains PIN prefix range validation rules for each State Code as defined in Appendix C of the UCRF specification.
/// </summary>
public static class PinCodeRanges
{
    public static readonly IReadOnlyDictionary<int, (int Min, int Max)> Ranges = new Dictionary<int, (int Min, int Max)>
    {
        { 1, (18, 19) },   // Jammu & Kashmir
        { 2, (17, 17) },   // Himachal Pradesh
        { 3, (14, 15) },   // Punjab
        { 4, (16, 16) },   // Chandigarh
        { 5, (24, 26) },   // Uttarakhand
        { 6, (12, 13) },   // Haryana
        { 7, (11, 11) },   // Delhi
        { 8, (30, 34) },   // Rajasthan
        { 9, (20, 28) },   // Uttar Pradesh
        { 10, (80, 85) },  // Bihar
        { 11, (73, 73) },  // Sikkim
        { 12, (79, 79) },  // Arunachal Pradesh
        { 13, (79, 79) },  // Nagaland
        { 14, (79, 79) },  // Manipur
        { 15, (79, 79) },  // Mizoram
        { 16, (79, 79) },  // Tripura
        { 17, (79, 79) },  // Meghalaya
        { 18, (78, 78) },  // Assam
        { 19, (70, 74) },  // West Bengal
        { 20, (80, 85) },  // Jharkhand
        { 21, (75, 77) },  // Odisha
        { 22, (49, 49) },  // Chhattisgarh
        { 23, (45, 48) },  // Madhya Pradesh
        { 24, (36, 39) },  // Gujarat
        { 25, (39, 39) },  // Daman & Diu
        { 26, (39, 39) },  // Dadra & Nagar Haveli
        { 27, (40, 44) },  // Maharashtra
        { 28, (50, 53) },  // Andhra Pradesh
        { 29, (56, 59) },  // Karnataka
        { 30, (40, 40) },  // Goa
        { 31, (68, 68) },  // Lakshadweep
        { 32, (67, 69) },  // Kerala
        { 33, (60, 64) },  // Tamil Nadu
        { 34, (60, 60) },  // Puducherry
        { 35, (74, 74) },  // Andaman & Nicobar Islands
        { 36, (50, 53) },  // Telangana
        { 37, (19, 19) },  // Ladakh
        { 99, (90, 99) }   // APO Address
        // For State Code 77 (Foreign Address), there is no PIN prefix range, and PIN validation is bypassed.
    };
}
