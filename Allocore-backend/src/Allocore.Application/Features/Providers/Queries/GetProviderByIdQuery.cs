namespace Allocore.Application.Features.Providers.Queries;

using MediatR;
using Allocore.Application.Features.Providers.DTOs;
using Allocore.Domain.Common;

public record GetProviderByIdQuery(Guid CompanyId, Guid ProviderId) : IRequest<Result<ProviderDto>>;
