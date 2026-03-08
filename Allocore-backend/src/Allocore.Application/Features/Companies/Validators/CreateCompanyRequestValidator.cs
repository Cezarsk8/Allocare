namespace Allocore.Application.Features.Companies.Validators;

using FluentValidation;
using Allocore.Application.Features.Companies.DTOs;

public class CreateCompanyRequestValidator : AbstractValidator<CreateCompanyRequest>
{
    public CreateCompanyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters");

        RuleFor(x => x.LegalName)
            .MaximumLength(300).WithMessage("Legal name must not exceed 300 characters")
            .When(x => !string.IsNullOrEmpty(x.LegalName));

        RuleFor(x => x.TaxId)
            .MaximumLength(50).WithMessage("Tax ID must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.TaxId));
    }
}
