namespace Allocore.Application.Features.Users.Validators;

using FluentValidation;
using Allocore.Application.Features.Users.DTOs;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100);
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100);
        
        RuleFor(x => x.Locale)
            .MaximumLength(10)
            .When(x => x.Locale != null);
    }
}
