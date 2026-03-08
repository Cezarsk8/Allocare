namespace Allocore.Application.Features.Auth.Validators;

using FluentValidation;
using Allocore.Application.Features.Auth.DTOs;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
