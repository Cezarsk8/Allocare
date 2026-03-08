namespace Allocore.Application.Features.Companies.Queries;

using MediatR;
using Allocore.Application.Features.Companies.DTOs;

public record GetMyCompaniesQuery : IRequest<IEnumerable<CompanyDto>>;
