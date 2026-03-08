namespace Allocore.Application.Features.Companies.Commands;

using MediatR;
using Allocore.Application.Features.Companies.DTOs;
using Allocore.Domain.Common;

public record AddUserToCompanyCommand(Guid CompanyId, AddUserToCompanyRequest Request) : IRequest<Result<UserCompanyDto>>;
