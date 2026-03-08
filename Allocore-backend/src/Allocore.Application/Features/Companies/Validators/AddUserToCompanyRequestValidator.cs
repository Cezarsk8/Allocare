namespace Allocore.Application.Features.Companies.Validators;

using FluentValidation;
using Allocore.Application.Features.Companies.DTOs;

public class AddUserToCompanyRequestValidator : AbstractValidator<AddUserToCompanyRequest>
{
    public AddUserToCompanyRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.RoleInCompany)
            .NotEmpty().WithMessage("Role is required")
            .Must(role => role is "Viewer" or "Manager" or "Owner")
            .WithMessage("Role must be one of: Viewer, Manager, Owner");
    }
}
