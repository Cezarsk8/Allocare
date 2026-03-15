namespace Allocore.Application.Features.Employees.Validators;

using FluentValidation;
using Allocore.Application.Features.Employees.DTOs;

public class TerminateEmployeeRequestValidator : AbstractValidator<TerminateEmployeeRequest>
{
    public TerminateEmployeeRequestValidator()
    {
        RuleFor(x => x.TerminationDate)
            .NotEmpty().WithMessage("Termination date is required");
    }
}
