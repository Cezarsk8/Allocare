namespace Allocore.Application.Features.Providers.Validators;

using FluentValidation;
using Allocore.Application.Features.Providers.DTOs;
using Allocore.Domain.Entities.Providers;

public class UpdateProviderRequestValidator : AbstractValidator<UpdateProviderRequest>
{
    public UpdateProviderRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Provider name is required")
            .MaximumLength(200).WithMessage("Provider name must not exceed 200 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(c => Enum.TryParse<ProviderCategory>(c, true, out _))
            .WithMessage("Category must be one of: SaaS, Infrastructure, Consultancy, Benefits, Licensing, Telecommunications, Hardware, Other");

        RuleFor(x => x.LegalName)
            .MaximumLength(300).WithMessage("Legal name must not exceed 300 characters")
            .When(x => !string.IsNullOrEmpty(x.LegalName));

        RuleFor(x => x.TaxId)
            .MaximumLength(50).WithMessage("Tax ID must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.TaxId));

        RuleFor(x => x.Website)
            .MaximumLength(500).WithMessage("Website must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
