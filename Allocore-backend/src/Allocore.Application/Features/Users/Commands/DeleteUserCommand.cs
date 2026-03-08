namespace Allocore.Application.Features.Users.Commands;

using MediatR;
using Allocore.Domain.Common;

public record DeleteUserCommand(Guid Id) : IRequest<Result>;
