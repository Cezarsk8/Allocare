namespace Allocore.Application.Features.Employees.Commands;

using MediatR;
using Allocore.Application.Features.Employees.DTOs;
using Allocore.Domain.Common;

public record UpdateEmployeeCommand(Guid CompanyId, Guid EmployeeId, UpdateEmployeeRequest Request) : IRequest<Result<EmployeeDto>>;
