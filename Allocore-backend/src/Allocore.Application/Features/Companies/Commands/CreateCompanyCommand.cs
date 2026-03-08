namespace Allocore.Application.Features.Companies.Commands;

using MediatR;
using Allocore.Application.Features.Companies.DTOs;
using Allocore.Domain.Common;

public record CreateCompanyCommand(CreateCompanyRequest Request) : IRequest<Result<CompanyDto>>;
