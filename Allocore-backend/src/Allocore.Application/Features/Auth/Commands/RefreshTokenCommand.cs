namespace Allocore.Application.Features.Auth.Commands;

using MediatR;
using Allocore.Application.Features.Auth.DTOs;
using Allocore.Domain.Common;

public record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<Result<AuthResponse>>;
