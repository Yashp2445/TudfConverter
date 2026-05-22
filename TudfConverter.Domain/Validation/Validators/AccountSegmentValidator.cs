using System;
using System.Linq;
using FluentValidation;
using TudfConverter.Domain.Constants;
using TudfConverter.Domain.Models;

namespace TudfConverter.Domain.Validation.Validators;

public class AccountSegmentValidator : AbstractValidator<AccountSegmentModel>
{
    private static readonly int[] CreditCardAccountTypes = { 10, 16, 31, 35 };

    public AccountSegmentValidator()
    {
        RuleFor(x => x.CurrentMemberCode)
            .NotEmpty()
            .WithErrorCode("TL-01")
            .WithMessage("Reporting member code is required and cannot be empty.");

        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .WithErrorCode("TL-03")
            .WithMessage("Account number is required and cannot be empty.");

        RuleFor(x => x.AccountType)
            .Must(type => AccountTypes.ValidCodes.Contains(type))
            .WithErrorCode("TL-04")
            .WithName("AccountType")
            .WithMessage("Account type code is invalid. Refer to Appendix D for valid account type values.");

        RuleFor(x => x.OwnershipIndicator)
            .Must(ind => UcrfCatalogues.ValidOwnershipIndicators.Contains(ind))
            .WithErrorCode("TL-05")
            .WithName("OwnershipIndicator")
            .WithMessage("Ownership indicator is invalid. Valid values are 1 Individual, 2 Authorised User, 3 Guarantor, 4 Joint, 5 Deceased.");

        RuleFor(x => x.HighCreditSanctionedAmount)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("TL-12")
            .WithMessage("High Credit or Sanctioned Amount must be zero or a positive number.");

        RuleFor(x => x.CurrentBalance)
            .NotNull()
            .WithErrorCode("TL-13");

        RuleFor(x => x.DateOpenedDisbursed)
            .NotEqual(default(DateOnly))
            .WithErrorCode("TL-08")
            .WithMessage("Date Opened or Disbursed is required.");

        RuleFor(x => x)
            .Must(x => !x.DateOfLastPayment.HasValue || x.DateOfLastPayment.Value >= x.DateOpenedDisbursed)
            .WithErrorCode("TL-09A")
            .WithName("DateOfLastPayment")
            .WithMessage("Date of Last Payment must be on or after Date Opened or Disbursed.");

        RuleFor(x => x)
            .Must(x => !x.DateClosed.HasValue || x.DateClosed.Value >= x.DateOpenedDisbursed)
            .WithErrorCode("TL-10A")
            .WithName("DateClosed")
            .WithMessage("Date Closed must be on or after Date Opened or Disbursed.");

        RuleFor(x => x)
            .Must(x => {
                if (!x.DateClosed.HasValue) return true;
                if (x.CreditFacilityStatus.HasValue && IsSpecialStatus(x.CreditFacilityStatus.Value)) return true;
                if (!x.DateOfLastPayment.HasValue) return true;
                return x.DateOfLastPayment.Value <= x.DateClosed.Value;
            })
            .WithErrorCode("TL-10B")
            .WithName("DateClosed")
            .WithMessage("When Date Closed is provided and no special Credit Facility Status exists, Date of Last Payment must be on or before Date Closed.");

        RuleFor(x => x)
            .Must(x => !x.DateClosed.HasValue || x.IsCurrentBalanceNegative || x.CurrentBalance == 0)
            .WithErrorCode("TL-10C")
            .WithName("CurrentBalance")
            .WithMessage("When Date Closed is provided, Current Balance must be zero or negative. A positive balance cannot exist on a closed account.");

        RuleFor(x => x.DateReportedAndCertified)
            .NotEqual(default(DateOnly))
            .WithErrorCode("TL-11")
            .WithMessage("Date Reported and Certified is required.");

        RuleFor(x => x)
            .Must(x => {
                if (!CreditCardAccountTypes.Contains(x.AccountType) && x.NumberOfDaysPastDue > 0)
                    return x.AmountOverdue > 0;
                return true;
            })
            .WithErrorCode("TL-14A")
            .WithName("AmountOverdue")
            .WithMessage("When Days Past Due is greater than zero for a non-credit-card account, Amount Overdue must also be greater than zero.");

        RuleFor(x => x)
            .Must(x => {
                if (!CreditCardAccountTypes.Contains(x.AccountType) && x.AmountOverdue > 0)
                    return x.NumberOfDaysPastDue > 0;
                return true;
            })
            .WithErrorCode("TL-15A")
            .WithName("NumberOfDaysPastDue")
            .WithMessage("When Amount Overdue is greater than zero for a non-credit-card account, Days Past Due must also be greater than zero.");

        RuleFor(x => x)
            .Must(x => x.NumberOfDaysPastDue.HasValue || x.AssetClassification.HasValue)
            .WithErrorCode("TL-15-26")
            .WithName("DPD_AssetClassification")
            .WithMessage("Either Number of Days Past Due or Asset Classification must be provided. Both cannot be absent.");

        RuleFor(x => x)
            .Must(x => {
                if (x.NumberOfDaysPastDue > 900) return false;
                return true;
            })
            .WithErrorCode("TL-15-CAP")
            .WithName("NumberOfDaysPastDue")
            .WithMessage("Number of Days Past Due exceeds maximum of 900. It will be capped at 900.");

        RuleFor(x => x)
            .Must(x => x.OwnershipIndicator != 2 || x.AccountType == 10)
            .WithErrorCode("TL-05A")
            .WithName("OwnershipIndicator")
            .WithMessage("Ownership indicator of 2 (Authorised User) is only valid for account type 10 (Credit Card).");

        RuleFor(x => x)
            .Must(x => CreditCardAccountTypes.Contains(x.AccountType) || !x.CreditLimit.HasValue)
            .WithErrorCode("TL-36")
            .WithName("CreditLimit")
            .WithMessage("Credit Limit must not be reported for account types other than Credit Card (10), Fleet Card (16), Secured Credit Card (31), and Corporate Credit Card (35).");

        RuleFor(x => x)
            .Must(x => CreditCardAccountTypes.Contains(x.AccountType) || !x.CashLimit.HasValue)
            .WithErrorCode("TL-37")
            .WithName("CashLimit")
            .WithMessage("Cash Limit must not be reported for account types other than Credit Card (10), Fleet Card (16), Secured Credit Card (31), and Corporate Credit Card (35).");

        RuleFor(x => x)
            .Must(x => !CreditCardAccountTypes.Contains(x.AccountType) && x.AccountType != 12 || !x.EmiAmount.HasValue)
            .WithErrorCode("TL-40")
            .WithName("EmiAmount")
            .WithMessage("EMI Amount must not be reported for Credit Card, Fleet Card, Secured Credit Card, Corporate Credit Card, or Overdraft account types.");

        RuleFor(x => x)
            .Must(x => {
                if (x.CreditFacilityStatus == 2 || x.CreditFacilityStatus == 3 || x.CreditFacilityStatus == 4)
                    return x.WrittenOffAmountTotal > 0 && x.WrittenOffAmountPrincipal > 0;
                return true;
            })
            .WithErrorCode("TL-41")
            .WithName("WrittenOffAmount")
            .WithMessage("Written-off Amount Total and Written-off Amount Principal are required when Credit Facility Status is 02 Written-off, 03 Settled, or 04 Post Written-off Settled.");

        RuleFor(x => x)
            .Must(x => !x.WrittenOffAmountPrincipal.HasValue || !x.WrittenOffAmountTotal.HasValue || x.WrittenOffAmountPrincipal <= x.WrittenOffAmountTotal)
            .WithErrorCode("TL-42")
            .WithName("WrittenOffAmountPrincipal")
            .WithMessage("Written-off Amount Principal cannot exceed Written-off Amount Total.");

        RuleFor(x => x)
            .Must(x => {
                if (x.CreditFacilityStatus == 3 || x.CreditFacilityStatus == 4 || x.CreditFacilityStatus == 15 || x.CreditFacilityStatus == 16)
                    return x.SettlementAmount > 0;
                return true;
            })
            .WithErrorCode("TL-43")
            .WithName("SettlementAmount")
            .WithMessage("Settlement Amount is required and must be greater than zero when Credit Facility Status is 03 Settled, 04 Post Written-off Settled, 15 Auctioned and Settled, or 16 Repossessed and Settled.");

        RuleFor(x => x)
            .Must(x => x.CreditFacilityStatus != 17 || x.OwnershipIndicator == 3)
            .WithErrorCode("TL-22A")
            .WithName("CreditFacilityStatus")
            .WithMessage("Credit Facility Status of 17 (Guarantee Invoked) requires Ownership Indicator to be 3 (Guarantor).");

        RuleFor(x => x.EmiAmount)
            .Must(emi => !emi.HasValue || emi > 0)
            .WithErrorCode("TL-40B")
            .WithName("EmiAmount")
            .WithMessage("EMI Amount must be greater than zero when provided.");

        RuleFor(x => x.Income)
            .Must(inc => !inc.HasValue || inc > 0)
            .WithErrorCode("TL-47")
            .WithName("Income")
            .WithMessage("Income amount must be greater than zero when provided.");
    }

    private bool IsSpecialStatus(int status)
    {
        return status == 2 || status == 3 || status == 4 || status == 5 || status == 6; 
    }
}
