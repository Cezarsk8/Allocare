namespace Allocore.Application.Features.Employees.Validators;

using FluentValidation;
using Allocore.Application.Features.Employees.DTOs;

public class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Employee name is required")
            .MaximumLength(200).WithMessage("Employee name must not exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Employee email is required")
            .MaximumLength(300).WithMessage("Email must not exceed 300 characters")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.JobTitle)
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.JobTitle));
    }
}
