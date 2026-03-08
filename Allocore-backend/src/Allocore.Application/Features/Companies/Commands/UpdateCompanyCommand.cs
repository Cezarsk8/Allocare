namespace Allocore.Application.Features.Companies.Commands;

using MediatR;
using Allocore.Application.Features.Companies.DTOs;
using Allocore.Domain.Common;

public record UpdateCompanyCommand(Guid CompanyId, UpdateCompanyRequest Request) : IRequest<Result<CompanyDto>>;
