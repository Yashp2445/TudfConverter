using System;
using System.Text;
using TudfConverter.Domain.Models;

namespace TudfConverter.Infrastructure.Tudf.Builders;

/// <summary>
/// Builds the Account History (AH) segment. Tags: H01 through H47.
/// </summary>
public class AccountHistorySegmentBuilder
{
    public string Build(AccountHistoryModel history, bool isCreditCard = false)
    {
        var sb = new StringBuilder();

        // Segment tag: H + index zero-padded to 2 digits
        var segTag = "H" + history.SegmentIndex.ToString("D2");
        sb.Append(segTag);

        // Tag 01: AccountHistoryDate as fixed 8-byte date
        sb.Append(TudfFieldFormatter.FormatVariableField("01", 
            TudfFieldFormatter.FormatDate(history.AccountHistoryDate)));

        // Tag 02: AssetClassificationNdpd as fixed 3-byte field
        sb.Append(TudfFieldFormatter.FormatVariableField("02", 
            TudfFieldFormatter.FormatFixedAlpha(history.AssetClassificationNdpd, 3)));

        // Tag 03: AmountOverdue (optional)
        if (history.AmountOverdue.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("03", history.AmountOverdue));

        // Tag 04: HighCreditSanctionedAmount (optional)
        if (history.HighCreditSanctionedAmount.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("04", history.HighCreditSanctionedAmount));

        // Tag 05: CreditLimit (only for credit card accounts)
        if (isCreditCard && history.CreditLimit.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("05", history.CreditLimit));

        // Tag 06: CashLimit (only for credit card accounts)
        if (isCreditCard && history.CashLimit.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("06", history.CashLimit));

        // Tag 07: CurrentBalance (signed)
        sb.Append(TudfFieldFormatter.FormatSignedAmountField("07", 
            history.CurrentBalance, history.IsCurrentBalanceNegative));

        // Tag 08: DateOfLastPayment (optional)
        if (history.DateOfLastPayment.HasValue)
            sb.Append(TudfFieldFormatter.FormatVariableDateField("08", history.DateOfLastPayment));

        // Tag 09: ActualPaymentAmount (optional)
        if (history.ActualPaymentAmount.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("09", history.ActualPaymentAmount));

        return sb.ToString();
    }
}
