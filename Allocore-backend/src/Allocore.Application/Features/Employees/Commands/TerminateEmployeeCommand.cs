namespace Allocore.Application.Features.Employees.Commands;

using MediatR;
using Allocore.Application.Features.Employees.DTOs;
using Allocore.Domain.Common;

public record TerminateEmployeeCommand(Guid CompanyId, Guid EmployeeId, TerminateEmployeeRequest Request) : IRequest<Result>;
