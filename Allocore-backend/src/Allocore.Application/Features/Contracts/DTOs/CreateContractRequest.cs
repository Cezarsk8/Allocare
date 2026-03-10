namespace Allocore.Application.Features.Contracts.DTOs;

public record CreateContractRequest(
    Guid ProviderId,
    string Title,
    string? ContractNumber,
    string? Status,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? RenewalDate,
    bool AutoRenew,
    int? RenewalNoticeDays,
    string? BillingFrequency,
    decimal? TotalValue,
    string? Currency,
    string? PaymentTerms,
    string? PriceConditions,
    string? LegalTeamContact,
    string? InternalOwner,
    string? Description,
    string? TermsAndConditions,
    IEnumerable<CreateContractServiceRequest>? Services
);
