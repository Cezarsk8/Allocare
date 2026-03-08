namespace Allocore.Application.Features.Users.Queries;

using MediatR;
using Allocore.Application.Features.Users.DTOs;
using Allocore.Domain.Common;

public record GetMeQuery : IRequest<Result<UserDto>>;
