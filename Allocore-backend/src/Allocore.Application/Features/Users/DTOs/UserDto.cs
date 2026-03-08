namespace Allocore.Application.Features.Users.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsEmailVerified,
    bool IsActive,
    string? Locale,
    DateTime CreatedAt
);
