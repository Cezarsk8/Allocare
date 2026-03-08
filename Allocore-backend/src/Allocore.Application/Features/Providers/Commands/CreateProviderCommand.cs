namespace Allocore.Application.Features.Providers.Commands;

using MediatR;
using Allocore.Application.Features.Providers.DTOs;
using Allocore.Domain.Common;

public record CreateProviderCommand(Guid CompanyId, CreateProviderRequest Request) : IRequest<Result<ProviderDto>>;
