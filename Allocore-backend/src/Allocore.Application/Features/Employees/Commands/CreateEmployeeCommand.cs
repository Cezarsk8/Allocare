namespace Allocore.Application.Features.Employees.Commands;

using MediatR;
using Allocore.Application.Features.Employees.DTOs;
using Allocore.Domain.Common;

public record CreateEmployeeCommand(Guid CompanyId, CreateEmployeeRequest Request) : IRequest<Result<EmployeeDto>>;
