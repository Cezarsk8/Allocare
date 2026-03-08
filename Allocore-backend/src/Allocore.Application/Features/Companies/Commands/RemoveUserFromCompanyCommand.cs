namespace Allocore.Application.Features.Companies.Commands;

using MediatR;
using Allocore.Domain.Common;

public record RemoveUserFromCompanyCommand(Guid CompanyId, Guid UserId) : IRequest<Result>;
