namespace Allocore.Application.Features.Employees.Queries;

using MediatR;
using Allocore.Application.Features.Employees.DTOs;
using Allocore.Domain.Common;

public record GetEmployeeByIdQuery(Guid CompanyId, Guid EmployeeId) : IRequest<Result<EmployeeDto>>;
