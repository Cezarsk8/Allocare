namespace Allocore.Domain.Entities.Contracts;

using Allocore.Domain.Common;
using Allocore.Domain.Entities.Providers;

public class Contract : Entity
{
    public Guid CompanyId { get; private set; }
    public Guid ProviderId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? ContractNumber { get; private set; }
    public ContractStatus Status { get; private set; } = ContractStatus.Draft;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? RenewalDate { get; private set; }
    public bool AutoRenew { get; private set; }
    public int? RenewalNoticeDays { get; private set; }
    public BillingFrequency BillingFrequency { get; private set; } = BillingFrequency.Monthly;
    public decimal? TotalValue { get; private set; }
    public string? Currency { get; private set; }
    public string? PaymentTerms { get; private set; }
    public string? PriceConditions { get; private set; }
    public string? LegalTeamContact { get; private set; }
    public string? InternalOwner { get; private set; }
    public string? Description { get; private set; }
    public string? TermsAndConditions { get; private set; }

    // Navigation properties
    public Provider? Provider { get; private set; }

    private readonly List<ContractService> _contractServices = new();
    public IReadOnlyCollection<ContractService> ContractServices => _contractServices.AsReadOnly();

    private Contract() { } // EF Core

    public static Contract Create(
        Guid companyId,
        Guid providerId,
        string title,
        string? contractNumber = null,
        ContractStatus status = ContractStatus.Draft,
        DateTime? startDate = null,
        DateTime? endDate = null,
        DateTime? renewalDate = null,
        bool autoRenew = false,
        int? renewalNoticeDays = null,
        BillingFrequency billingFrequency = BillingFrequency.Monthly,
        decimal? totalValue = null,
        string? currency = null,
        string? paymentTerms = null,
        string? priceConditions = null,
        string? legalTeamContact = null,
        string? internalOwner = null,
        string? description = null,
        string? termsAndConditions = null)
    {
        return new Contract
        {
            CompanyId = companyId,
            ProviderId = providerId,
            Title = title,
            ContractNumber = contractNumber,
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            RenewalDate = renewalDate,
            AutoRenew = autoRenew,
            RenewalNoticeDays = renewalNoticeDays,
            BillingFrequency = billingFrequency,
            TotalValue = totalValue,
            Currency = currency,
            PaymentTerms = paymentTerms,
            PriceConditions = priceConditions,
            LegalTeamContact = legalTeamContact,
            InternalOwner = internalOwner,
            Description = description,
            TermsAndConditions = termsAndConditions
        };
    }

    public void Update(
        string title,
        string? contractNumber,
        ContractStatus status,
        DateTime? startDate,
        DateTime? endDate,
        DateTime? renewalDate,
        bool autoRenew,
        int? renewalNoticeDays,
        BillingFrequency billingFrequency,
        decimal? totalValue,
        string? currency,
        string? paymentTerms,
        string? priceConditions,
        string? legalTeamContact,
        string? internalOwner,
        string? description,
        string? termsAndConditions)
    {
        Title = title;
        ContractNumber = contractNumber;
        Status = status;
        StartDate = startDate;
        EndDate = endDate;
        RenewalDate = renewalDate;
        AutoRenew = autoRenew;
        RenewalNoticeDays = renewalNoticeDays;
        BillingFrequency = billingFrequency;
        TotalValue = totalValue;
        Currency = currency;
        PaymentTerms = paymentTerms;
        PriceConditions = priceConditions;
        LegalTeamContact = legalTeamContact;
        InternalOwner = internalOwner;
        Description = description;
        TermsAndConditions = termsAndConditions;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(ContractStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddService(ContractService contractService)
    {
        _contractServices.Add(contractService);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveService(ContractService contractService)
    {
        _contractServices.Remove(contractService);
        UpdatedAt = DateTime.UtcNow;
    }

    // Computed properties
    public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.UtcNow && Status != ContractStatus.Renewed;
    public bool IsExpiringSoon(int withinDays = 30) => EndDate.HasValue && EndDate.Value <= DateTime.UtcNow.AddDays(withinDays) && EndDate.Value >= DateTime.UtcNow;
    public bool NeedsRenewalAttention(int withinDays = 30) => RenewalDate.HasValue && RenewalDate.Value <= DateTime.UtcNow.AddDays(withinDays) && RenewalDate.Value >= DateTime.UtcNow;
}
