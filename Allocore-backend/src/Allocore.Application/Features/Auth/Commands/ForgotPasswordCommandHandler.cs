namespace Allocore.Application.Features.Auth.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
    }

    public async Task Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(command.Request.Email, cancellationToken);
        
        // Always return success to prevent email enumeration
        if (user == null || !user.IsActive)
        {
            return;
        }

        // Generate reset token
        var resetToken = _jwtTokenService.GenerateRefreshToken();
        var resetTokenHash = _jwtTokenService.HashToken(resetToken);

        // Set token on user (expires in 1 hour)
        user.SetPasswordResetToken(resetTokenHash, TimeSpan.FromHours(1));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send email with unhashed token
        await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, cancellationToken);
    }
}
