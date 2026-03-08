namespace Allocore.Application.Features.Auth.Commands;

using MediatR;
using Allocore.Application.Features.Auth.DTOs;
using Allocore.Domain.Common;

public record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<Result>;
