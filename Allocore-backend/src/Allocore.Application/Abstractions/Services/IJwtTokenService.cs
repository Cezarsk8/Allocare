namespace Allocore.Application.Abstractions.Services;

using Allocore.Domain.Entities.Users;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateRefreshToken(string token, string storedHash);
    string HashToken(string token);
}
