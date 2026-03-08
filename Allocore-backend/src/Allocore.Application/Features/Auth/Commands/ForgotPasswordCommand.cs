namespace Allocore.Application.Features.Auth.Commands;

using MediatR;
using Allocore.Application.Features.Auth.DTOs;

public record ForgotPasswordCommand(ForgotPasswordRequest Request) : IRequest;
