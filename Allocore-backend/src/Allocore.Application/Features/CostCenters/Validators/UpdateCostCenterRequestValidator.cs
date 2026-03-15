namespace Allocore.Application.Features.CostCenters.Validators;

using FluentValidation;
using Allocore.Application.Features.CostCenters.DTOs;

public class UpdateCostCenterRequestValidator : AbstractValidator<UpdateCostCenterRequest>
{
    public UpdateCostCenterRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Cost center code is required")
            .MaximumLength(50).WithMessage("Cost center code must not exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cost center name is required")
            .MaximumLength(200).WithMessage("Cost center name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
