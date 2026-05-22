using System;
using System.Linq;
using System.Text;
using TudfConverter.Domain.Models;

namespace TudfConverter.Infrastructure.Tudf.Builders;

/// <summary>
/// Builds the Account/Trade Line (TL) segment.
/// </summary>
public class AccountSegmentBuilder
{
    private static readonly int[] CreditCardAccountTypes = { 10, 16, 31, 35 };

    public string Build(AccountSegmentModel account)
    {
        var sb = new StringBuilder();

        // Segment identifier: TL + 04 + fixed 4-byte tag T001
        sb.Append("TL");
        sb.Append("04");
        sb.Append("T001");

        // Tag 01: CurrentMemberCode - padded to 10 chars, as variable field
        sb.Append(TudfFieldFormatter.FormatVariableField("01", 
            TudfFieldFormatter.FormatFixedAlpha(account.CurrentMemberCode, 10)));

        // Tag 02: MemberShortName (optional)
        if (!string.IsNullOrEmpty(account.MemberShortName))
            sb.Append(TudfFieldFormatter.FormatVariableField("02", account.MemberShortName));

        // Tag 03: AccountNumber (required)
        sb.Append(TudfFieldFormatter.FormatVariableField("03", account.AccountNumber));

        // Tag 04: AccountType as fixed 2-byte numeric
        sb.Append(TudfFieldFormatter.FormatVariableField("04", account.AccountType.ToString("D2")));

        // Tag 05: OwnershipIndicator as fixed 1-byte numeric
        sb.Append(TudfFieldFormatter.FormatVariableField("05", account.OwnershipIndicator.ToString()));

        // Tag 08: DateOpenedDisbursed
        sb.Append(TudfFieldFormatter.FormatVariableDateField("08", account.DateOpenedDisbursed));

        // Tag 09: DateOfLastPayment (optional)
        if (account.DateOfLastPayment.HasValue)
            sb.Append(TudfFieldFormatter.FormatVariableDateField("09", account.DateOfLastPayment));

        // Tag 10: DateClosed (optional)
        if (account.DateClosed.HasValue)
            sb.Append(TudfFieldFormatter.FormatVariableDateField("10", account.DateClosed));

        // Tag 11: DateReportedAndCertified
        sb.Append(TudfFieldFormatter.FormatVariableDateField("11", account.DateReportedAndCertified));

        // Tag 12: HighCreditSanctionedAmount
        sb.Append(TudfFieldFormatter.FormatNumericVariableField("12", account.HighCreditSanctionedAmount));

        // Tag 13: CurrentBalance (signed)
        sb.Append(TudfFieldFormatter.FormatSignedAmountField("13", account.CurrentBalance, account.IsCurrentBalanceNegative));

        // Tag 14: AmountOverdue (optional)
        if (account.AmountOverdue.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("14", account.AmountOverdue));

        // Tag 15: NumberOfDaysPastDue (optional, 3 digits, capped at 900)
        if (account.NumberOfDaysPastDue.HasValue)
        {
            var dpd = Math.Min(account.NumberOfDaysPastDue.Value, 900);
            sb.Append(TudfFieldFormatter.FormatVariableField("15", dpd.ToString("D3")));
        }

        // Tag 16: OldReportingMemberCode (optional, fixed 10-byte)
        if (!string.IsNullOrEmpty(account.OldReportingMemberCode))
            sb.Append(TudfFieldFormatter.FormatVariableField("16", 
                TudfFieldFormatter.FormatFixedAlpha(account.OldReportingMemberCode, 10)));

        // Tag 17: OldMemberShortName (optional)
        if (!string.IsNullOrEmpty(account.OldMemberShortName))
            sb.Append(TudfFieldFormatter.FormatVariableField("17", account.OldMemberShortName));

        // Tag 18: OldAccountNumber (optional)
        if (!string.IsNullOrEmpty(account.OldAccountNumber))
            sb.Append(TudfFieldFormatter.FormatVariableField("18", account.OldAccountNumber));

        // Tag 19: OldAccountType (optional, fixed 2-byte)
        if (account.OldAccountType.HasValue)
            sb.Append(TudfFieldFormatter.FormatVariableField("19", account.OldAccountType.Value.ToString("D2")));

        // Tag 20: OldOwnershipIndicator (optional, fixed 1-byte)
        if (account.OldOwnershipIndicator.HasValue)
            sb.Append(TudfFieldFormatter.FormatVariableField("20", account.OldOwnershipIndicator.Value.ToString()));

        // Tag 21: SuitFiledWilfulDefault (optional, fixed 2-byte)
        if (account.SuitFiledWilfulDefault.HasValue)
            sb.Append(TudfFieldFormatter.FormatVariableField("21", account.SuitFiledWilfulDefault.Value.ToString("D2")));

        // Tag 22: CreditFacilityStatus (optional, fixed 2-byte)
        if (account.CreditFacilityStatus.HasValue)
            sb.Append(TudfFieldFormatter.FormatVariableField("22", account.CreditFacilityStatus.Value.ToString("D2")));

        // Tag 26: AssetClassification (fixed 2-byte) - only if DPD is null
        if (!account.NumberOfDaysPastDue.HasValue && account.AssetClassification.HasValue)
            sb.Append(TudfFieldFormatter.FormatVariableField("26", account.AssetClassification.Value.ToString("D2")));

        // Tag 34: ValueOfCollateral (optional)
        if (account.ValueOfCollateral.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("34", account.ValueOfCollateral));

        // Tag 35: TypeOfCollateral (optional, fixed 2-byte)
        if (account.TypeOfCollateral.HasValue)
            sb.Append(TudfFieldFormatter.FormatVariableField("35", account.TypeOfCollateral.Value.ToString("D2")));

        // Tag 36: CreditLimit (only for credit card types)
        if (CreditCardAccountTypes.Contains(account.AccountType) && account.CreditLimit.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("36", account.CreditLimit));

        // Tag 37: CashLimit (only for credit card types)
        if (CreditCardAccountTypes.Contains(account.AccountType) && account.CashLimit.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("37", account.CashLimit));

        // Tag 38: RateOfInterest (optional)
        if (!string.IsNullOrEmpty(account.RateOfInterest))
            sb.Append(TudfFieldFormatter.FormatVariableField("38", account.RateOfInterest));

        // Tag 39: RepaymentTenure (optional)
        if (account.RepaymentTenure.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("39", account.RepaymentTenure.Value));

        // Tag 40: EMIAmount (only for non-credit-card and non-overdraft)
        if (!CreditCardAccountTypes.Contains(account.AccountType) && account.AccountType != 12 && account.EmiAmount.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("40", account.EmiAmount));

        // Tag 41: WrittenOffAmountTotal (optional)
        if (account.WrittenOffAmountTotal.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("41", account.WrittenOffAmountTotal));

        // Tag 42: WrittenOffAmountPrincipal (optional)
        if (account.WrittenOffAmountPrincipal.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("42", account.WrittenOffAmountPrincipal));

        // Tag 43: SettlementAmount (optional)
        if (account.SettlementAmount.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("43", account.SettlementAmount));

        // Tag 44: PaymentFrequency (optional, fixed 2-byte)
        if (account.PaymentFrequency.HasValue)
            sb.Append(TudfFieldFormatter.FormatVariableField("44", account.PaymentFrequency.Value.ToString("D2")));

        // Tag 45: ActualPaymentAmount (optional)
        if (account.ActualPaymentAmount.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("45", account.ActualPaymentAmount));

        // Tag 46: OccupationCode (optional, fixed 2-byte)
        if (account.OccupationCode.HasValue)
            sb.Append(TudfFieldFormatter.FormatVariableField("46", account.OccupationCode.Value.ToString("D2")));

        // Tag 47: Income (optional)
        if (account.Income.HasValue)
            sb.Append(TudfFieldFormatter.FormatNumericVariableField("47", account.Income));

        // Tag 48: NetGrossIncomeIndicator (optional, fixed 1-byte)
        if (!string.IsNullOrEmpty(account.NetGrossIncomeIndicator))
            sb.Append(TudfFieldFormatter.FormatVariableField("48", account.NetGrossIncomeIndicator.Substring(0, Math.Min(1, account.NetGrossIncomeIndicator.Length))));

        // Tag 49: MonthlyAnnualIncomeIndicator (optional, fixed 1-byte)
        if (!string.IsNullOrEmpty(account.MonthlyAnnualIncomeIndicator))
            sb.Append(TudfFieldFormatter.FormatVariableField("49", account.MonthlyAnnualIncomeIndicator.Substring(0, Math.Min(1, account.MonthlyAnnualIncomeIndicator.Length))));

        return sb.ToString();
    }
}
