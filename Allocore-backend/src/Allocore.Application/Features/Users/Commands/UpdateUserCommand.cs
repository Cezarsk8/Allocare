namespace Allocore.Application.Features.Users.Commands;

using MediatR;
using Allocore.Application.Features.Users.DTOs;
using Allocore.Domain.Common;

public record UpdateUserCommand(Guid Id, UpdateUserRequest Request) : IRequest<Result<UserDto>>;
