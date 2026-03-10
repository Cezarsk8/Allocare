namespace Allocore.Application.Features.Contracts.Validators;

using FluentValidation;
using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Domain.Entities.Contracts;

public class CreateContractRequestValidator : AbstractValidator<CreateContractRequest>
{
    public CreateContractRequestValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Contract title is required")
            .MaximumLength(300).WithMessage("Contract title must not exceed 300 characters");

        RuleFor(x => x.ContractNumber)
            .MaximumLength(100).WithMessage("Contract number must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContractNumber));

        RuleFor(x => x.Status)
            .Must(s => s == null || Enum.TryParse<ContractStatus>(s, true, out _))
            .WithMessage("Status must be one of: Draft, InNegotiation, Active, Expiring, Expired, Renewed, Cancelled, Terminated");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.RenewalDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .WithMessage("Renewal date should be on or before end date")
            .When(x => x.RenewalDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.RenewalNoticeDays)
            .GreaterThan(0).WithMessage("Renewal notice days must be positive")
            .When(x => x.RenewalNoticeDays.HasValue);

        RuleFor(x => x.BillingFrequency)
            .Must(bf => bf == null || Enum.TryParse<BillingFrequency>(bf, true, out _))
            .WithMessage("Billing frequency must be one of: Monthly, Quarterly, SemiAnnual, Annual, OneOff, Custom");

        RuleFor(x => x.TotalValue)
            .GreaterThanOrEqualTo(0).WithMessage("Total value must be non-negative")
            .When(x => x.TotalValue.HasValue);

        RuleFor(x => x.Currency)
            .MaximumLength(3).WithMessage("Currency must be a 3-letter ISO code (e.g., USD, BRL)")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(500).WithMessage("Payment terms must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms));

        RuleFor(x => x.PriceConditions)
            .MaximumLength(2000).WithMessage("Price conditions must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.PriceConditions));

        RuleFor(x => x.LegalTeamContact)
            .MaximumLength(300).WithMessage("Legal team contact must not exceed 300 characters")
            .When(x => !string.IsNullOrEmpty(x.LegalTeamContact));

        RuleFor(x => x.InternalOwner)
            .MaximumLength(200).WithMessage("Internal owner must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.InternalOwner));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleForEach(x => x.Services)
            .SetValidator(new CreateContractServiceRequestValidator())
            .When(x => x.Services != null);
    }
}
