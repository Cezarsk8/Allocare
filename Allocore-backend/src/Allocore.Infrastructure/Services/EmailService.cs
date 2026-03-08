namespace Allocore.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using Allocore.Application.Abstractions.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }
    
    public Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual email sending (e.g., SendGrid, SMTP)
        _logger.LogInformation("Password reset email would be sent to {Email} with token {Token}", email, resetToken);
        return Task.CompletedTask;
    }
    
    public Task SendWelcomeEmailAsync(string email, string firstName, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual email sending
        _logger.LogInformation("Welcome email would be sent to {Email} for {FirstName}", email, firstName);
        return Task.CompletedTask;
    }
}
