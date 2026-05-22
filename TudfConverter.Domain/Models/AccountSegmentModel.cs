using System;

namespace TudfConverter.Domain.Models;

/// <summary>
/// Represents the Account Segment containing comprehensive credit facility details.
/// </summary>
public class AccountSegmentModel
{
    public required string CurrentMemberCode { get; init; }
    public string? MemberShortName { get; init; }
    public required string AccountNumber { get; init; }
    public required int AccountType { get; init; }
    public required int OwnershipIndicator { get; init; }
    public required DateOnly DateOpenedDisbursed { get; init; }
    public DateOnly? DateOfLastPayment { get; init; }
    public DateOnly? DateClosed { get; init; }
    public DateOnly DateReportedAndCertified { get; set; }
    public required long HighCreditSanctionedAmount { get; init; }
    public required long CurrentBalance { get; init; }
    public required bool IsCurrentBalanceNegative { get; init; }
    public long? AmountOverdue { get; init; }
    public int? NumberOfDaysPastDue { get; init; }
    public int? AssetClassification { get; init; }
    public int? SuitFiledWilfulDefault { get; init; }
    public int? CreditFacilityStatus { get; init; }
    public string? OldReportingMemberCode { get; init; }
    public string? OldMemberShortName { get; init; }
    public string? OldAccountNumber { get; init; }
    public int? OldAccountType { get; init; }
    public int? OldOwnershipIndicator { get; init; }
    public long? CreditLimit { get; init; }
    public long? CashLimit { get; init; }
    public string? RateOfInterest { get; init; }
    public int? RepaymentTenure { get; init; }
    public long? EmiAmount { get; init; }
    public long? WrittenOffAmountTotal { get; init; }
    public long? WrittenOffAmountPrincipal { get; init; }
    public long? SettlementAmount { get; init; }
    public int? PaymentFrequency { get; init; }
    public long? ActualPaymentAmount { get; init; }
    public int? OccupationCode { get; init; }
    public long? Income { get; init; }
    public string? NetGrossIncomeIndicator { get; init; }
    public string? MonthlyAnnualIncomeIndicator { get; init; }
    public long? ValueOfCollateral { get; init; }
    public int? TypeOfCollateral { get; init; }
}
