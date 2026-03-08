namespace Allocore.Application.Features.Users.Queries;

using MediatR;
using Allocore.Application.Features.Users.DTOs;
using Allocore.Domain.Common;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;
