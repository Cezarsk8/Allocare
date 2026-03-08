namespace Allocore.Application.Features.Providers.Validators;

using FluentValidation;
using Allocore.Application.Features.Providers.DTOs;

public class AddProviderContactRequestValidator : AbstractValidator<AddProviderContactRequest>
{
    public AddProviderContactRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Contact name is required")
            .MaximumLength(150).WithMessage("Contact name must not exceed 150 characters");

        RuleFor(x => x.Email)
            .MaximumLength(254).WithMessage("Email must not exceed 254 characters")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone must not exceed 30 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Role)
            .MaximumLength(100).WithMessage("Role must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Role));
    }
}
