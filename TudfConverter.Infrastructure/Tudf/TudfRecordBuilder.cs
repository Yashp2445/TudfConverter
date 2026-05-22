using System;
using System.Linq;
using System.Text;
using TudfConverter.Domain.Constants;
using TudfConverter.Domain.Models;
using TudfConverter.Infrastructure.Tudf.Builders;

namespace TudfConverter.Infrastructure.Tudf;

/// <summary>
/// Assembles a complete TUDF record for a single consumer by concatenating
/// all segment builders in the spec-mandated order.
/// </summary>
public class TudfRecordBuilder
{
    private readonly NameSegmentBuilder _nameBuilder;
    private readonly IdentificationSegmentBuilder _idBuilder;
    private readonly TelephoneSegmentBuilder _phoneBuilder;
    private readonly EmailSegmentBuilder _emailBuilder;
    private readonly AddressSegmentBuilder _addressBuilder;
    private readonly AccountSegmentBuilder _accountBuilder;
    private readonly AccountHistorySegmentBuilder _historyBuilder;

    public TudfRecordBuilder(
        NameSegmentBuilder nameBuilder,
        IdentificationSegmentBuilder idBuilder,
        TelephoneSegmentBuilder phoneBuilder,
        EmailSegmentBuilder emailBuilder,
        AddressSegmentBuilder addressBuilder,
        AccountSegmentBuilder accountBuilder,
        AccountHistorySegmentBuilder historyBuilder)
    {
        _nameBuilder = nameBuilder;
        _idBuilder = idBuilder;
        _phoneBuilder = phoneBuilder;
        _emailBuilder = emailBuilder;
        _addressBuilder = addressBuilder;
        _accountBuilder = accountBuilder;
        _historyBuilder = historyBuilder;
    }

    public string BuildRecord(ConsumerRecord record)
    {
        var sb = new StringBuilder();

        // 1. Name segment (always present)
        sb.Append(_nameBuilder.Build(record.Name));

        // 2. Identification segments (up to 8)
        foreach (var id in record.Identifications.Take(8))
        {
            sb.Append(_idBuilder.Build(id));
        }

        // 3. Telephone segments (up to 10)
        foreach (var phone in record.Telephones.Take(10))
        {
            sb.Append(_phoneBuilder.Build(phone));
        }

        // 4. Email segments (up to 10)
        foreach (var email in record.Emails.Take(10))
        {
            sb.Append(_emailBuilder.Build(email));
        }

        // 5. Address segments (up to 5)
        foreach (var addr in record.Addresses.Take(5))
        {
            sb.Append(_addressBuilder.Build(addr));
        }

        // 6. Account segment (always present, exactly once)
        sb.Append(_accountBuilder.Build(record.Account));

        // 7. Account history segments (up to 47)
        bool isCreditCard = UcrfCatalogues.CreditCardAccountTypes.Contains(record.Account.AccountType);
        foreach (var hist in record.AccountHistory.Take(47))
        {
            sb.Append(_historyBuilder.Build(hist, isCreditCard));
        }

        // 8. End of subject segment
        sb.Append("ES02**");

        return sb.ToString();
    }
}
