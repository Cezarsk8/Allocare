namespace Allocore.Application.Features.Contracts;

using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Domain.Entities.Contracts;

internal static class ContractMapper
{
    public static ContractDto ToDto(Contract contract, string providerName) => new(
        contract.Id,
        contract.CompanyId,
        contract.ProviderId,
        providerName,
        contract.Title,
        contract.ContractNumber,
        contract.Status.ToString(),
        contract.StartDate,
        contract.EndDate,
        contract.RenewalDate,
        contract.AutoRenew,
        contract.RenewalNoticeDays,
        contract.BillingFrequency.ToString(),
        contract.TotalValue,
        contract.Currency,
        contract.PaymentTerms,
        contract.PriceConditions,
        contract.LegalTeamContact,
        contract.InternalOwner,
        contract.Description,
        contract.TermsAndConditions,
        contract.IsExpired,
        contract.IsExpiringSoon(),
        contract.CreatedAt,
        contract.UpdatedAt,
        contract.ContractServices.Select(ToServiceDto)
    );

    public static ContractListItemDto ToListItemDto(Contract contract) => new(
        contract.Id,
        contract.Title,
        contract.ContractNumber,
        contract.Provider?.Name ?? string.Empty,
        contract.Status.ToString(),
        contract.StartDate,
        contract.EndDate,
        contract.BillingFrequency.ToString(),
        contract.TotalValue,
        contract.Currency,
        contract.IsExpired,
        contract.IsExpiringSoon(),
        contract.ContractServices.Count
    );

    public static ContractServiceDto ToServiceDto(ContractService svc) => new(
        svc.Id,
        svc.ServiceName,
        svc.ServiceDescription,
        svc.UnitPrice,
        svc.UnitType,
        svc.Quantity,
        svc.Notes
    );
}
