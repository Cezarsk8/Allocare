namespace Allocore.Application.Features.Companies.Queries;

using MediatR;
using Allocore.Application.Features.Companies.DTOs;
using Allocore.Domain.Common;

public record GetCompanyByIdQuery(Guid CompanyId) : IRequest<Result<CompanyDto>>;
