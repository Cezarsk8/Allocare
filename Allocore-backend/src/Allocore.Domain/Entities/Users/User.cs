namespace Allocore.Domain.Entities.Users;

using Allocore.Domain.Common;
using Allocore.Domain.Entities.Companies;

public class User : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Role Role { get; private set; } = Role.User;
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; } = true;
    public LocaleTag Locale { get; private set; } = LocaleTag.Default;

    // Lockout fields
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }

    // Password reset
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    // Company associations
    private readonly List<UserCompany> _userCompanies = new();
    public IReadOnlyCollection<UserCompany> UserCompanies => _userCompanies.AsReadOnly();

    private User() { } // EF Core
    
    public static User Create(string email, string passwordHash, string firstName, string lastName, Role role = Role.User)
    {
        return new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            IsEmailVerified = false,
            IsActive = true
        };
    }
    
    public void UpdateProfile(string firstName, string lastName, string? locale)
    {
        FirstName = firstName;
        LastName = lastName;
        Locale = LocaleTag.Create(locale);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void VerifyEmail()
    {
        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RecordFailedLogin(int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            LockoutEnd = DateTime.UtcNow.Add(lockoutDuration);
        }
    }
    
    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }
    
    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;
    
    /// <summary>
    /// Sets the password reset token. The token should be hashed before passing to this method
    /// for security (prevents database leaks from exposing valid tokens).
    /// </summary>
    public void SetPasswordResetToken(string tokenHash, TimeSpan expiry)
    {
        PasswordResetToken = tokenHash;
        PasswordResetTokenExpiry = DateTime.UtcNow.Add(expiry);
    }
    
    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
    }
    
    public string FullName => $"{FirstName} {LastName}";
}
