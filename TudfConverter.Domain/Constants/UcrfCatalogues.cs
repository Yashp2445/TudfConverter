namespace TudfConverter.Domain.Constants;

/// <summary>
/// Contains various valid code sets and classifications defined in the UCRF specification.
/// </summary>
public static class UcrfCatalogues
{
    public static readonly HashSet<int> ValidGenderCodes = new() { 1, 2, 3 };
    public static readonly HashSet<int> ValidOwnershipIndicators = new() { 1, 2, 3, 4, 5 };
    public static readonly HashSet<int> ValidIdTypes = new() { 1, 2, 3, 4, 5, 6, 9, 10 };
    public static readonly HashSet<int> ValidAddressCategories = new() { 1, 2, 3, 4, 5 };
    
    public static readonly HashSet<int> ValidCreditFacilityStatuses = new()
    {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 99
    };
    
    public static readonly HashSet<int> ValidAssetClassifications = new() { 1, 2, 3, 4, 5 };
    public static readonly HashSet<int> ValidTelephoneTypes = new() { 0, 1, 2, 3 };
    public static readonly HashSet<int> ValidPaymentFrequencies = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    public static readonly HashSet<int> ValidCollateralTypes = new() { 0, 1, 2, 3, 4, 5, 6 };
    public static readonly HashSet<int> CreditCardAccountTypes = new() { 10, 16, 31, 35 };
}
