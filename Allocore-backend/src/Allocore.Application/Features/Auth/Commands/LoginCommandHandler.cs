namespace Allocore.Application.Features.Auth.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Auth.DTOs;
using Allocore.Application.Features.Users.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Users;
using BCrypt.Net;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return Result.Failure<AuthResponse>("Invalid email or password");
        }

        // Check if user is locked out
        if (user.IsLockedOut)
        {
            return Result.Failure<AuthResponse>("Account is locked. Please try again later.");
        }

        // Verify password
        if (!BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // Record failed login attempt
            var maxAttempts = _configuration.GetValue<int>("Lockout:MaxFailedAttempts", 5);
            var lockoutMinutes = _configuration.GetValue<int>("Lockout:LockoutDurationMinutes", 15);
            user.RecordFailedLogin(maxAttempts, TimeSpan.FromMinutes(lockoutMinutes));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Result.Failure<AuthResponse>("Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result.Failure<AuthResponse>("Account is deactivated");
        }

        // Reset failed login attempts on successful login
        user.ResetFailedLoginAttempts();

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = _jwtTokenService.HashToken(refreshToken);

        var refreshTokenEntity = RefreshToken.Create(
            user.Id,
            refreshTokenHash,
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        );

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.IsEmailVerified,
            user.IsActive,
            user.Locale.Value,
            user.CreatedAt
        );

        return Result.Success(new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            userDto
        ));
    }
}
