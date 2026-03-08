namespace Allocore.Application.Features.Auth.DTOs;

using Allocore.Application.Features.Users.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);
