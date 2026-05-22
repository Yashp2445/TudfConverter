using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TudfConverter.Application.Pipeline;
using TudfConverter.Domain.Models;
using TudfConverter.Domain.Validation.IdValidators;

namespace TudfConverter.Application.Mapping
{
    public class ExcelToConsumerRecordMapper
    {
        private readonly ILogger<ExcelToConsumerRecordMapper> _logger;
        private readonly IEnumerable<IIdNumberValidator> _validators;

        public ExcelToConsumerRecordMapper(
            ILogger<ExcelToConsumerRecordMapper> logger,
            IEnumerable<IIdNumberValidator> validators)
        {
            _logger = logger;
            _validators = validators;
        }

        public ConsumerRecord Map(RawExcelRow row)
        {
            var record = new ConsumerRecord
            {
                RowNumber = row.RowNumber,
                Name = MapName(row),
                Account = MapAccount(row)
            };

            try
            {
                record.Addresses.AddRange(MapAddresses(row));
                record.Identifications.AddRange(MapIdentifications(row));
                record.Telephones.AddRange(MapTelephones(row));
                record.Emails.AddRange(MapEmails(row));
                record.AccountHistory.AddRange(MapAccountHistory(row));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error mapping row {RowNumber}", row.RowNumber);
            }

            return record;
        }

        private NameSegmentModel MapName(RawExcelRow row)
        {
            var consumerName = GetValue(row, ExcelColumnMap.ConsumerName) ?? string.Empty;
            
            DateOnly? dob = null;
            if (TryParseDate(GetValue(row, ExcelColumnMap.DateOfBirth), out var parsedDob))
            {
                dob = parsedDob;
            }

            int? genderCode = null;
            var genderStr = GetValue(row, ExcelColumnMap.Gender);
            if (int.TryParse(genderStr, out int g))
            {
                genderCode = g;
            }

            return new NameSegmentModel
            {
                FullName = consumerName,
                DateOfBirth = dob,
                Gender = genderCode
            };
        }

        private AccountSegmentModel MapAccount(RawExcelRow row)
        {
            var currentBalanceStr = GetValue(row, ExcelColumnMap.CurrentBalance);
            bool isNegative = false;
            long currentBalance = 0;
            if (!string.IsNullOrWhiteSpace(currentBalanceStr))
            {
                if (currentBalanceStr.StartsWith("-") || currentBalanceStr.EndsWith("-"))
                {
                    isNegative = true;
                    currentBalanceStr = currentBalanceStr.Replace("-", "").Trim();
                }
                TryParseLong(currentBalanceStr, out currentBalance);
            }

            TryParseLong(GetValue(row, ExcelColumnMap.HighCreditSanctionedAmt), out var highCredit);
            
            TryParseDate(GetValue(row, ExcelColumnMap.DateOpenedDisbursed), out var dateOpened);
            
            int.TryParse(GetValue(row, ExcelColumnMap.AccountType), out int accType);
            int.TryParse(GetValue(row, ExcelColumnMap.OwnershipIndicator), out int ownInd);

            var model = new AccountSegmentModel
            {
                CurrentMemberCode = GetValue(row, ExcelColumnMap.CurrentNewMemberCode) ?? string.Empty,
                MemberShortName = GetValue(row, ExcelColumnMap.CurrentNewMemberShortName),
                AccountNumber = GetValue(row, ExcelColumnMap.CurrNewAccountNo) ?? string.Empty,
                AccountType = accType,
                OwnershipIndicator = ownInd,
                DateOpenedDisbursed = dateOpened,
                HighCreditSanctionedAmount = highCredit,
                CurrentBalance = currentBalance,
                IsCurrentBalanceNegative = isNegative,
                // We default DateReportedAndCertified here but it can be overwritten later
                DateReportedAndCertified = default
            };

            if (TryParseDate(GetValue(row, ExcelColumnMap.DateOfLastPayment), out var dateLastPay))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = dateLastPay };

            if (TryParseDate(GetValue(row, ExcelColumnMap.DateClosed), out var dateClosed))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = dateClosed };

            if (TryParseLong(GetValue(row, ExcelColumnMap.AmtOverdue), out var amtOverdue))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = amtOverdue };

            if (int.TryParse(GetValue(row, ExcelColumnMap.NoOfDaysPastDue), out int dpd))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = dpd };

            if (int.TryParse(GetValue(row, ExcelColumnMap.SuitFiledWilfulDefault), out int suitFiled))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = suitFiled };

            var assetClass = GetValue(row, ExcelColumnMap.AssetClassification);
            if (int.TryParse(assetClass, out int ac))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = ac };

            if (TryParseLong(GetValue(row, ExcelColumnMap.ValueOfCollateral), out var valueCol))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = model.AssetClassification, ValueOfCollateral = valueCol };

            if (int.TryParse(GetValue(row, ExcelColumnMap.TypeOfCollateral), out int typeCol))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = model.AssetClassification, ValueOfCollateral = model.ValueOfCollateral, TypeOfCollateral = typeCol };

            if (TryParseLong(GetValue(row, ExcelColumnMap.CreditLimit), out var creditLimit))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = model.AssetClassification, ValueOfCollateral = model.ValueOfCollateral, TypeOfCollateral = model.TypeOfCollateral, CreditLimit = creditLimit };

            if (TryParseLong(GetValue(row, ExcelColumnMap.CashLimit), out var cashLimit))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = model.AssetClassification, ValueOfCollateral = model.ValueOfCollateral, TypeOfCollateral = model.TypeOfCollateral, CreditLimit = model.CreditLimit, CashLimit = cashLimit };

            var roiStr = GetValue(row, ExcelColumnMap.RateOfInterest);
            if (!string.IsNullOrWhiteSpace(roiStr))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = model.AssetClassification, ValueOfCollateral = model.ValueOfCollateral, TypeOfCollateral = model.TypeOfCollateral, CreditLimit = model.CreditLimit, CashLimit = model.CashLimit, RateOfInterest = roiStr };

            if (int.TryParse(GetValue(row, ExcelColumnMap.RepaymentTenure), out int tenure))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = model.AssetClassification, ValueOfCollateral = model.ValueOfCollateral, TypeOfCollateral = model.TypeOfCollateral, CreditLimit = model.CreditLimit, CashLimit = model.CashLimit, RateOfInterest = model.RateOfInterest, RepaymentTenure = tenure };

            if (TryParseLong(GetValue(row, ExcelColumnMap.EmiAmount), out var emi))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = model.AssetClassification, ValueOfCollateral = model.ValueOfCollateral, TypeOfCollateral = model.TypeOfCollateral, CreditLimit = model.CreditLimit, CashLimit = model.CashLimit, RateOfInterest = model.RateOfInterest, RepaymentTenure = model.RepaymentTenure, EmiAmount = emi };

            if (TryParseLong(GetValue(row, ExcelColumnMap.WrittenOffAmountTotal), out var woTotal))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = model.AssetClassification, ValueOfCollateral = model.ValueOfCollateral, TypeOfCollateral = model.TypeOfCollateral, CreditLimit = model.CreditLimit, CashLimit = model.CashLimit, RateOfInterest = model.RateOfInterest, RepaymentTenure = model.RepaymentTenure, EmiAmount = model.EmiAmount, WrittenOffAmountTotal = woTotal };

            if (TryParseLong(GetValue(row, ExcelColumnMap.WrittenOffPrincipalAmount), out var woPrincipal))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = model.AssetClassification, ValueOfCollateral = model.ValueOfCollateral, TypeOfCollateral = model.TypeOfCollateral, CreditLimit = model.CreditLimit, CashLimit = model.CashLimit, RateOfInterest = model.RateOfInterest, RepaymentTenure = model.RepaymentTenure, EmiAmount = model.EmiAmount, WrittenOffAmountTotal = model.WrittenOffAmountTotal, WrittenOffAmountPrincipal = woPrincipal };

            if (TryParseLong(GetValue(row, ExcelColumnMap.SettlementAmt), out var settlement))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = model.AssetClassification, ValueOfCollateral = model.ValueOfCollateral, TypeOfCollateral = model.TypeOfCollateral, CreditLimit = model.CreditLimit, CashLimit = model.CashLimit, RateOfInterest = model.RateOfInterest, RepaymentTenure = model.RepaymentTenure, EmiAmount = model.EmiAmount, WrittenOffAmountTotal = model.WrittenOffAmountTotal, WrittenOffAmountPrincipal = model.WrittenOffAmountPrincipal, SettlementAmount = settlement };

            if (int.TryParse(GetValue(row, ExcelColumnMap.PaymentFrequency), out int freq))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = model.AssetClassification, ValueOfCollateral = model.ValueOfCollateral, TypeOfCollateral = model.TypeOfCollateral, CreditLimit = model.CreditLimit, CashLimit = model.CashLimit, RateOfInterest = model.RateOfInterest, RepaymentTenure = model.RepaymentTenure, EmiAmount = model.EmiAmount, WrittenOffAmountTotal = model.WrittenOffAmountTotal, WrittenOffAmountPrincipal = model.WrittenOffAmountPrincipal, SettlementAmount = model.SettlementAmount, PaymentFrequency = freq };

            if (TryParseLong(GetValue(row, ExcelColumnMap.ActualPaymentAmt), out var actualPayment))
                model = new AccountSegmentModel { CurrentMemberCode = model.CurrentMemberCode, AccountNumber = model.AccountNumber, AccountType = model.AccountType, OwnershipIndicator = model.OwnershipIndicator, DateOpenedDisbursed = model.DateOpenedDisbursed, HighCreditSanctionedAmount = model.HighCreditSanctionedAmount, CurrentBalance = model.CurrentBalance, IsCurrentBalanceNegative = model.IsCurrentBalanceNegative, DateReportedAndCertified = model.DateReportedAndCertified, MemberShortName = model.MemberShortName, DateOfLastPayment = model.DateOfLastPayment, DateClosed = model.DateClosed, AmountOverdue = model.AmountOverdue, NumberOfDaysPastDue = model.NumberOfDaysPastDue, SuitFiledWilfulDefault = model.SuitFiledWilfulDefault, AssetClassification = model.AssetClassification, ValueOfCollateral = model.ValueOfCollateral, TypeOfCollateral = model.TypeOfCollateral, CreditLimit = model.CreditLimit, CashLimit = model.CashLimit, RateOfInterest = model.RateOfInterest, RepaymentTenure = model.RepaymentTenure, EmiAmount = model.EmiAmount, WrittenOffAmountTotal = model.WrittenOffAmountTotal, WrittenOffAmountPrincipal = model.WrittenOffAmountPrincipal, SettlementAmount = model.SettlementAmount, PaymentFrequency = model.PaymentFrequency, ActualPaymentAmount = actualPayment };

            return model;
        }

        private List<AddressModel> MapAddresses(RawExcelRow row)
        {
            var addresses = new List<AddressModel>();

            var address1 = GetValue(row, ExcelColumnMap.AddressLine1);
            if (!string.IsNullOrWhiteSpace(address1))
            {
                var addr = new AddressModel
                {
                    SegmentIndex = 1,
                    AddressLine1 = address1,
                    AddressLine2 = GetValue(row, ExcelColumnMap.AddressLine2),
                    StateCode = GetValue(row, ExcelColumnMap.StateCode1),
                    PinCode = GetValue(row, ExcelColumnMap.PinCode1)
                };

                if (int.TryParse(GetValue(row, ExcelColumnMap.AddressCategory1), out int category))
                    addr = new AddressModel { SegmentIndex = addr.SegmentIndex, AddressLine1 = addr.AddressLine1, AddressLine2 = addr.AddressLine2, StateCode = addr.StateCode, PinCode = addr.PinCode, AddressCategory = category };

                if (int.TryParse(GetValue(row, ExcelColumnMap.ResidenceCode1), out int residence))
                    addr = new AddressModel { SegmentIndex = addr.SegmentIndex, AddressLine1 = addr.AddressLine1, AddressLine2 = addr.AddressLine2, StateCode = addr.StateCode, PinCode = addr.PinCode, AddressCategory = addr.AddressCategory, ResidenceCode = residence };

                addresses.Add(addr);
            }

            return addresses;
        }

        private List<IdentificationModel> MapIdentifications(RawExcelRow row)
        {
            var ids = new List<IdentificationModel>();
            int index = 1;

            void AddId(string col, int typeCode, string? issueCol = null, string? expCol = null)
            {
                if (ids.Count >= 8) return;
                var val = GetValue(row, col);
                if (!string.IsNullOrWhiteSpace(val))
                {
                    var idModel = new IdentificationModel
                    {
                        SegmentIndex = index++,
                        IdType = typeCode,
                        IdNumber = val
                    };

                    if (issueCol != null && TryParseDate(GetValue(row, issueCol), out var issueDate))
                        idModel = new IdentificationModel { SegmentIndex = idModel.SegmentIndex, IdType = idModel.IdType, IdNumber = idModel.IdNumber, IssueDate = issueDate };

                    if (expCol != null && TryParseDate(GetValue(row, expCol), out var expDate))
                        idModel = new IdentificationModel { SegmentIndex = idModel.SegmentIndex, IdType = idModel.IdType, IdNumber = idModel.IdNumber, IssueDate = idModel.IssueDate, ExpirationDate = expDate };

                    ids.Add(idModel);
                }
            }

            AddId(ExcelColumnMap.IncomeTaxIdNumber, 1);
            AddId(ExcelColumnMap.PassportNumber, 2, ExcelColumnMap.PassportIssueDate, ExcelColumnMap.PassportExpiryDate);
            AddId(ExcelColumnMap.VoterIdNumber, 3);
            AddId(ExcelColumnMap.DrivingLicenseNumber, 4, ExcelColumnMap.DrivingLicenseIssueDate, ExcelColumnMap.DrivingLicenseExpiryDate);
            AddId(ExcelColumnMap.RationCardNumber, 5);
            AddId(ExcelColumnMap.UniversalIdNumber, 6);
            AddId(ExcelColumnMap.AdditionalId1, 7);
            AddId(ExcelColumnMap.AdditionalId2, 8);

            return ids;
        }

        private List<TelephoneModel> MapTelephones(RawExcelRow row)
        {
            var phones = new List<TelephoneModel>();
            int index = 1;

            void AddPhone(string col, string typeCode, string? extCol = null)
            {
                if (phones.Count >= 10) return;
                var val = GetValue(row, col);
                if (!string.IsNullOrWhiteSpace(val))
                {
                    var phoneModel = new TelephoneModel
                    {
                        SegmentIndex = index++,
                        TelephoneType = typeCode,
                        TelephoneNumber = val
                    };
                    if (extCol != null)
                    {
                        phoneModel = new TelephoneModel { SegmentIndex = phoneModel.SegmentIndex, TelephoneType = phoneModel.TelephoneType, TelephoneNumber = phoneModel.TelephoneNumber, TelephoneExtension = GetValue(row, extCol) };
                    }
                    phones.Add(phoneModel);
                }
            }

            AddPhone(ExcelColumnMap.TelephoneNoMobile, "01");
            AddPhone(ExcelColumnMap.TelephoneNoResidence, "02");
            AddPhone(ExcelColumnMap.TelephoneNoOffice, "03", ExcelColumnMap.ExtensionOffice);
            AddPhone(ExcelColumnMap.TelephoneNoOther, "04", ExcelColumnMap.ExtensionOther);

            return phones;
        }

        private List<EmailModel> MapEmails(RawExcelRow row)
        {
            var emails = new List<EmailModel>();
            var email1 = GetValue(row, ExcelColumnMap.EmailId1);
            if (!string.IsNullOrWhiteSpace(email1))
            {
                emails.Add(new EmailModel
                {
                    SegmentIndex = 1,
                    EmailId = email1
                });
            }
            return emails;
        }

        private List<AccountHistoryModel> MapAccountHistory(RawExcelRow row)
        {
            var history = new List<AccountHistoryModel>();
            int index = 1;
            
            for (int i = 1; i <= 47; i++)
            {
                var dpdKey = $"Month{i}_DPD";
                var balKey = $"Month{i}_Balance";

                if (row.Columns.ContainsKey(dpdKey) || row.Columns.ContainsKey(balKey))
                {
                    TryParseLong(GetValue(row, balKey), out var bal);
                    
                    var histModel = new AccountHistoryModel
                    {
                        SegmentIndex = index++,
                        AccountHistoryDate = default,
                        AssetClassificationNdpd = GetValue(row, dpdKey) ?? string.Empty,
                        CurrentBalance = bal,
                        IsCurrentBalanceNegative = false
                    };
                    
                    history.Add(histModel);
                }
            }

            return history;
        }

        private string? GetValue(RawExcelRow row, string column)
        {
            if (row.Columns.TryGetValue(column, out var val))
            {
                return val?.Trim();
            }
            return null;
        }

        private static bool TryParseDate(string? value, out DateOnly date)
        {
            date = default;
            if (string.IsNullOrWhiteSpace(value)) return false;

            if (DateOnly.TryParseExact(value, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out date))
                return true;

            if (DateOnly.TryParseExact(value, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out date))
                return true;

            if (DateOnly.TryParseExact(value, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out date))
                return true;

            if (DateTime.TryParse(value, out var dt))
            {
                date = DateOnly.FromDateTime(dt);
                return true;
            }

            return false;
        }

        private static bool TryParseLong(string? value, out long result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(value)) return false;

            value = value.Replace(",", "").Trim();
            return long.TryParse(value, out result);
        }
    }
}
