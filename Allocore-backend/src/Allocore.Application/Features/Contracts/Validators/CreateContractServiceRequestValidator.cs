namespace Allocore.Application.Features.Contracts.Validators;

using FluentValidation;
using Allocore.Application.Features.Contracts.DTOs;

public class CreateContractServiceRequestValidator : AbstractValidator<CreateContractServiceRequest>
{
    public CreateContractServiceRequestValidator()
    {
        RuleFor(x => x.ServiceName)
            .NotEmpty().WithMessage("Service name is required")
            .MaximumLength(200).WithMessage("Service name must not exceed 200 characters");

        RuleFor(x => x.ServiceDescription)
            .MaximumLength(1000).WithMessage("Service description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.ServiceDescription));

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be non-negative")
            .When(x => x.UnitPrice.HasValue);

        RuleFor(x => x.UnitType)
            .MaximumLength(50).WithMessage("Unit type must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.UnitType));

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be positive")
            .When(x => x.Quantity.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
