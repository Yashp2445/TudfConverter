using System.Collections.Generic;

namespace TudfConverter.Domain.Models;

/// <summary>
/// Represents the top-level entity aggregating a borrower's demographic and account details.
/// </summary>
public class ConsumerRecord
{
    public required int RowNumber { get; init; }
    public required NameSegmentModel Name { get; init; }
    public List<IdentificationModel> Identifications { get; }
    public List<TelephoneModel> Telephones { get; }
    public List<EmailModel> Emails { get; }
    public List<AddressModel> Addresses { get; }
    public required AccountSegmentModel Account { get; init; }
    public List<AccountHistoryModel> AccountHistory { get; }

    public ConsumerRecord()
    {
        Identifications = new List<IdentificationModel>();
        Telephones = new List<TelephoneModel>();
        Emails = new List<EmailModel>();
        Addresses = new List<AddressModel>();
        AccountHistory = new List<AccountHistoryModel>();
    }
}
