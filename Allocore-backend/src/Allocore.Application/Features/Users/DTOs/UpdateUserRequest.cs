namespace Allocore.Application.Features.Users.DTOs;

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string? Locale
);
