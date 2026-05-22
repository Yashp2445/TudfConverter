using System;

namespace TudfConverter.Domain.Models;

/// <summary>
/// Represents the Account History Segment representing individual historical monthly milestones.
/// </summary>
public class AccountHistoryModel
{
    public required int SegmentIndex { get; init; }
    public required DateOnly AccountHistoryDate { get; init; }
    public required string AssetClassificationNdpd { get; init; }
    public long? AmountOverdue { get; init; }
    public long? HighCreditSanctionedAmount { get; init; }
    public long? CreditLimit { get; init; }
    public long? CashLimit { get; init; }
    public required long CurrentBalance { get; init; }
    public required bool IsCurrentBalanceNegative { get; init; }
    public DateOnly? DateOfLastPayment { get; init; }
    public long? ActualPaymentAmount { get; init; }
}
