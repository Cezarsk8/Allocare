namespace Allocore.Application.Features.Employees.Commands;

using MediatR;
using Allocore.Domain.Common;

public record DeactivateEmployeeCommand(Guid CompanyId, Guid EmployeeId) : IRequest<Result>;
