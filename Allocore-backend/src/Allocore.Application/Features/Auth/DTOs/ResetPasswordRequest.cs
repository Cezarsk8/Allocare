namespace Allocore.Application.Features.Auth.DTOs;

public record ResetPasswordRequest(string Token, string NewPassword);
