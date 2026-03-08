namespace Allocore.Application.Features.Auth.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Domain.Common;
using BCrypt.Net;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        // Hash the incoming token to find the user
        var tokenHash = _jwtTokenService.HashToken(command.Request.Token);
        
        var user = await _userRepository.GetByPasswordResetTokenAsync(tokenHash, cancellationToken);
        if (user == null)
        {
            return Result.Failure("Invalid or expired reset token");
        }

        // Check if token is expired
        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            return Result.Failure("Reset token has expired");
        }

        // Hash new password and update
        var newPasswordHash = BCrypt.HashPassword(command.Request.NewPassword, workFactor: 12);
        user.ChangePassword(newPasswordHash);
        user.ClearPasswordResetToken();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
