# US002 – JWT Authentication & User Management

## Description

**As** an administrator of a company using Allocore,  
**I need** to register, authenticate, and manage users securely,  
**So that** only authorized people can access the cost and provider data of my company(ies).

---

## Step 1: Domain Layer – User & RefreshToken Entities

- [x] Update `Allocore.Domain/Entities/Users/Role.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Users;
  
  public enum Role
  {
      User = 0,
      Admin = 1
  }
  ```
- [x] Create `Allocore.Domain/Entities/Users/LocaleTag.cs` (Value Object):
  ```csharp
  namespace Allocore.Domain.Entities.Users;
  
  public record LocaleTag
  {
      public string Value { get; }
      
      private LocaleTag(string value) => Value = value;
      
      public static LocaleTag Default => new("en-US");
      
      public static LocaleTag Create(string? value)
      {
          if (string.IsNullOrWhiteSpace(value))
              return Default;
          return new LocaleTag(value.Trim());
      }
  }
  ```
- [x] Update `Allocore.Domain/Entities/Users/User.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Users;
  
  using Allocore.Domain.Common;
  
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
  ```
- [x] Create `Allocore.Domain/Entities/Users/RefreshToken.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Users;
  
  using Allocore.Domain.Common;
  
  public class RefreshToken : Entity
  {
      public Guid UserId { get; private set; }
      public string TokenHash { get; private set; } = string.Empty;
      public DateTime ExpiresAt { get; private set; }
      public bool IsRevoked { get; private set; }
      public DateTime? RevokedAt { get; private set; }
      public string? ReplacedByTokenHash { get; private set; }
      public string? DeviceInfo { get; private set; }
      public string? IpAddress { get; private set; }
      
      private RefreshToken() { } // EF Core
      
      public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAt, string? deviceInfo = null, string? ipAddress = null)
      {
          return new RefreshToken
          {
              UserId = userId,
              TokenHash = tokenHash,
              ExpiresAt = expiresAt,
              DeviceInfo = deviceInfo,
              IpAddress = ipAddress
          };
      }
      
      public void Revoke(string? replacedByTokenHash = null)
      {
          IsRevoked = true;
          RevokedAt = DateTime.UtcNow;
          ReplacedByTokenHash = replacedByTokenHash;
      }
      
      public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
      public bool IsActive => !IsRevoked && !IsExpired;
  }
  ```

---

## Step 2: Application Layer – Abstractions & Interfaces

- [x] Create `Allocore.Application/Abstractions/Services/ICurrentUser.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Services;
  
  public interface ICurrentUser
  {
      Guid? UserId { get; }
      string? Email { get; }
      IEnumerable<string> Roles { get; }
      bool IsAuthenticated { get; }
  }
  ```
- [x] Create `Allocore.Application/Abstractions/Services/IJwtTokenService.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Services;
  
  using Allocore.Domain.Entities.Users;
  
  public interface IJwtTokenService
  {
      string GenerateAccessToken(User user);
      string GenerateRefreshToken();
      bool ValidateRefreshToken(string token, string storedHash);
      string HashToken(string token);
  }
  ```
- [x] Create `Allocore.Application/Abstractions/Services/IDateTime.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Services;
  
  public interface IDateTime
  {
      DateTime UtcNow { get; }
  }
  ```
- [x] Create `Allocore.Application/Abstractions/Services/IEmailService.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Services;
  
  public interface IEmailService
  {
      Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default);
      Task SendWelcomeEmailAsync(string email, string firstName, CancellationToken cancellationToken = default);
  }
  ```
- [x] Create `Allocore.Application/Abstractions/Persistence/IUserRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;
  
  using Allocore.Domain.Entities.Users;
  
  public interface IUserRepository : IReadRepository<User>, IWriteRepository<User>
  {
      Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
      Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
      Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
      Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
  }
  ```
- [x] Create `Allocore.Application/Abstractions/Persistence/IRefreshTokenRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;
  
  using Allocore.Domain.Entities.Users;
  
  public interface IRefreshTokenRepository : IReadRepository<RefreshToken>, IWriteRepository<RefreshToken>
  {
      Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
      Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
      Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
  }
  ```
- [x] Create `Allocore.Application/Abstractions/Persistence/IUnitOfWork.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;
  
  public interface IUnitOfWork
  {
      Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
  }
  ```

---

## Step 3: Application Layer – DTOs

- [x] Create `Allocore.Application/Features/Auth/DTOs/RegisterRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Auth.DTOs;
  
  public record RegisterRequest(
      string Email,
      string Password,
      string FirstName,
      string LastName
  );
  ```
- [x] Create `Allocore.Application/Features/Auth/DTOs/LoginRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Auth.DTOs;
  
  public record LoginRequest(string Email, string Password);
  ```
- [x] Create `Allocore.Application/Features/Auth/DTOs/RefreshTokenRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Auth.DTOs;
  
  public record RefreshTokenRequest(string RefreshToken);
  ```
- [x] Create `Allocore.Application/Features/Auth/DTOs/ForgotPasswordRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Auth.DTOs;
  
  public record ForgotPasswordRequest(string Email);
  ```
- [x] Create `Allocore.Application/Features/Auth/DTOs/ResetPasswordRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Auth.DTOs;

  public record ResetPasswordRequest(string Token, string NewPassword);
  ```
- [x] Create `Allocore.Application/Features/Auth/DTOs/AuthResponse.cs`:
  ```csharp
  namespace Allocore.Application.Features.Auth.DTOs;

  using Allocore.Application.Features.Users.DTOs;

  public record AuthResponse(
      string AccessToken,
      string RefreshToken,
      DateTime ExpiresAt,
      UserDto User
  );
  ```
- [x] Create `Allocore.Application/Features/Users/DTOs/UserDto.cs`:
  ```csharp
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
  ```
- [x] Create `Allocore.Application/Features/Users/DTOs/UpdateUserRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Users.DTOs;

  public record UpdateUserRequest(
      string FirstName,
      string LastName,
      string? Locale
  );
  ```

---

## Step 4: Application Layer – Validators

- [x] Create `Allocore.Application/Features/Auth/Validators/RegisterRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Auth.Validators;
  
  using FluentValidation;
  using Allocore.Application.Features.Auth.DTOs;
  
  public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
  {
      public RegisterRequestValidator()
      {
          RuleFor(x => x.Email)
              .NotEmpty().WithMessage("Email is required")
              .EmailAddress().WithMessage("Invalid email format")
              .MaximumLength(256);
          
          RuleFor(x => x.Password)
              .NotEmpty().WithMessage("Password is required")
              .MinimumLength(8).WithMessage("Password must be at least 8 characters")
              .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
              .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
              .Matches("[0-9]").WithMessage("Password must contain at least one digit")
              .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
          
          RuleFor(x => x.FirstName)
              .NotEmpty().WithMessage("First name is required")
              .MaximumLength(100);
          
          RuleFor(x => x.LastName)
              .NotEmpty().WithMessage("Last name is required")
              .MaximumLength(100);
      }
  }
  ```
- [x] Create `Allocore.Application/Features/Auth/Validators/LoginRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Auth.Validators;
  
  using FluentValidation;
  using Allocore.Application.Features.Auth.DTOs;
  
  public class LoginRequestValidator : AbstractValidator<LoginRequest>
  {
      public LoginRequestValidator()
      {
          RuleFor(x => x.Email)
              .NotEmpty().WithMessage("Email is required")
              .EmailAddress().WithMessage("Invalid email format");
          
          RuleFor(x => x.Password)
              .NotEmpty().WithMessage("Password is required");
      }
  }
  ```
- [x] Create `Allocore.Application/Features/Auth/Validators/RefreshTokenRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Auth.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Auth.DTOs;

  public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
  {
      public RefreshTokenRequestValidator()
      {
          RuleFor(x => x.RefreshToken)
              .NotEmpty().WithMessage("Refresh token is required");
      }
  }
  ```
- [x] Create `Allocore.Application/Features/Auth/Validators/ForgotPasswordRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Auth.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Auth.DTOs;

  public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
  {
      public ForgotPasswordRequestValidator()
      {
          RuleFor(x => x.Email)
              .NotEmpty().WithMessage("Email is required")
              .EmailAddress().WithMessage("Invalid email format");
      }
  }
  ```
- [x] Create `Allocore.Application/Features/Auth/Validators/ResetPasswordRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Auth.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Auth.DTOs;

  public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
  {
      public ResetPasswordRequestValidator()
      {
          RuleFor(x => x.Token)
              .NotEmpty().WithMessage("Reset token is required");
          
          RuleFor(x => x.NewPassword)
              .NotEmpty().WithMessage("Password is required")
              .MinimumLength(8).WithMessage("Password must be at least 8 characters")
              .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
              .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
              .Matches("[0-9]").WithMessage("Password must contain at least one digit")
              .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
      }
  }
  ```
- [x] Create `Allocore.Application/Features/Users/Validators/UpdateUserRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Users.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Users.DTOs;

  public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
  {
      public UpdateUserRequestValidator()
      {
          RuleFor(x => x.FirstName)
              .NotEmpty().WithMessage("First name is required")
              .MaximumLength(100);
          
          RuleFor(x => x.LastName)
              .NotEmpty().WithMessage("Last name is required")
              .MaximumLength(100);
          
          RuleFor(x => x.Locale)
              .MaximumLength(10)
              .When(x => x.Locale != null);
      }
  }
  ```

---

## Step 5: Application Layer – CQRS Commands & Queries

- [x] Create `Allocore.Application/Features/Auth/Commands/RegisterCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Auth.Commands;
  
  using MediatR;
  using Allocore.Application.Features.Auth.DTOs;
  using Allocore.Domain.Common;
  
  public record RegisterCommand(RegisterRequest Request) : IRequest<Result<AuthResponse>>;
  ```
- [x] Create `Allocore.Application/Features/Auth/Commands/RegisterCommandHandler.cs`:
  - Validate email uniqueness
  - Hash password with BCrypt
  - Create User entity
  - Generate JWT + refresh token
  - Save to database
  - Return AuthResponse
- [x] Create `Allocore.Application/Features/Auth/Commands/LoginCommand.cs` and handler:
  - Check lockout status
  - Validate credentials
  - Record failed attempts or reset on success
  - Generate tokens
  - Return AuthResponse
- [x] Create `Allocore.Application/Features/Auth/Commands/RefreshTokenCommand.cs` and handler:
  - Validate refresh token
  - Revoke old token
  - Generate new token pair (rotation)
  - Return AuthResponse
- [x] Create `Allocore.Application/Features/Auth/Commands/LogoutCommand.cs` and handler:
  - Revoke current refresh token
- [x] Create `Allocore.Application/Features/Auth/Commands/ForgotPasswordCommand.cs` and handler:
  - Generate reset token (use `IJwtTokenService.GenerateRefreshToken()` for random token)
  - Hash token with SHA256 before storing (use `IJwtTokenService.HashToken()`)
  - Store hashed token in User entity
  - Send unhashed token via email (stub)
- [x] Create `Allocore.Application/Features/Auth/Commands/ResetPasswordCommand.cs` and handler:
  - Hash the incoming token
  - Find user by hashed token (`GetByPasswordResetTokenAsync`)
  - Validate token not expired
  - Update password with BCrypt hash
  - Clear reset token
- [x] Create `Allocore.Application/Features/Users/Queries/GetMeQuery.cs` and handler:
  - Return current user's data
- [x] Create `Allocore.Application/Features/Users/Queries/GetUsersQuery.cs` and handler:
  - Paginated list of users (Admin only)
- [x] Create `Allocore.Application/Features/Users/Queries/GetUserByIdQuery.cs` and handler
- [x] Create `Allocore.Application/Features/Users/Commands/UpdateUserCommand.cs` and handler
- [x] Create `Allocore.Application/Features/Users/Commands/DeleteUserCommand.cs` and handler

---

## Step 6: Infrastructure Layer – Database Configuration

- [x] Install packages:
  ```bash
  dotnet add Allocore.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
  dotnet add Allocore.Infrastructure package BCrypt.Net-Next
  ```
- [x] Create `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence;
  
  using Microsoft.EntityFrameworkCore;
  using Allocore.Domain.Entities.Users;
  using Allocore.Application.Abstractions.Persistence;
  
  public class ApplicationDbContext : DbContext, IUnitOfWork
  {
      public DbSet<User> Users => Set<User>();
      public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
      
      public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
      
      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
          modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
      }
  }
  ```
- [x] Create `Allocore.Infrastructure/Persistence/Configurations/UserConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;
  
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.Users;
  
  public class UserConfiguration : IEntityTypeConfiguration<User>
  {
      public void Configure(EntityTypeBuilder<User> builder)
      {
          builder.ToTable("Users");
          
          builder.HasKey(u => u.Id);
          
          builder.Property(u => u.Email)
              .IsRequired()
              .HasMaxLength(256);
          
          builder.HasIndex(u => u.Email)
              .IsUnique();
          
          builder.Property(u => u.PasswordHash)
              .IsRequired()
              .HasMaxLength(256);
          
          builder.Property(u => u.FirstName)
              .IsRequired()
              .HasMaxLength(100);
          
          builder.Property(u => u.LastName)
              .IsRequired()
              .HasMaxLength(100);
          
          builder.Property(u => u.Role)
              .HasConversion<string>()
              .HasMaxLength(50);
          
          // LocaleTag value object - use HasConversion for simpler mapping
          builder.Property(u => u.Locale)
              .HasConversion(
                  l => l.Value,
                  v => LocaleTag.Create(v))
              .HasColumnName("Locale")
              .HasMaxLength(10);
          
          // Password reset token (stored as hash for security)
          builder.Property(u => u.PasswordResetToken)
              .HasMaxLength(256);
          
          builder.HasIndex(u => u.PasswordResetToken);
      }
  }
  ```
- [x] Create `Allocore.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;
  
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.Users;
  
  public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
  {
      public void Configure(EntityTypeBuilder<RefreshToken> builder)
      {
          builder.ToTable("RefreshTokens");
          
          builder.HasKey(rt => rt.Id);
          
          builder.Property(rt => rt.TokenHash)
              .IsRequired()
              .HasMaxLength(256);
          
          builder.HasIndex(rt => rt.TokenHash);
          
          builder.HasIndex(rt => rt.UserId);
          
          // Foreign key relationship to User
          builder.HasOne<User>()
              .WithMany()
              .HasForeignKey(rt => rt.UserId)
              .OnDelete(DeleteBehavior.Cascade);
          
          builder.Property(rt => rt.DeviceInfo)
              .HasMaxLength(500);
          
          builder.Property(rt => rt.IpAddress)
              .HasMaxLength(45); // IPv6 max length
      }
  }
  ```

---

## Step 7: Infrastructure Layer – Repository Implementations

- [x] Create `Allocore.Infrastructure/Persistence/Repositories/UserRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Repositories;
  
  using Microsoft.EntityFrameworkCore;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Entities.Users;
  
  public class UserRepository : IUserRepository
  {
      private readonly ApplicationDbContext _context;
      
      public UserRepository(ApplicationDbContext context)
      {
          _context = context;
      }
      
      public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
      
      public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
          => await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
      
      public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
          => await _context.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
      
      public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
          => await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token, cancellationToken);
      
      public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
          => await _context.Users.ToListAsync(cancellationToken);
      
      public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
      {
          var totalCount = await _context.Users.CountAsync(cancellationToken);
          var users = await _context.Users
              .OrderBy(u => u.Email)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync(cancellationToken);
          return (users, totalCount);
      }
      
      public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
      {
          await _context.Users.AddAsync(entity, cancellationToken);
          return entity;
      }
      
      public Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
      {
          _context.Users.Update(entity);
          return Task.CompletedTask;
      }
      
      public Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
      {
          _context.Users.Remove(entity);
          return Task.CompletedTask;
      }
  }
  ```
- [x] Create `Allocore.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Repositories;

  using Microsoft.EntityFrameworkCore;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Entities.Users;

  public class RefreshTokenRepository : IRefreshTokenRepository
  {
      private readonly ApplicationDbContext _context;
      
      public RefreshTokenRepository(ApplicationDbContext context)
      {
          _context = context;
      }
      
      public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
      
      public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
          => await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
      
      public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
          => await _context.RefreshTokens
              .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
              .ToListAsync(cancellationToken);
      
      public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
      {
          var tokens = await _context.RefreshTokens
              .Where(rt => rt.UserId == userId && !rt.IsRevoked)
              .ToListAsync(cancellationToken);
          
          foreach (var token in tokens)
          {
              token.Revoke();
          }
      }
      
      public async Task<IEnumerable<RefreshToken>> GetAllAsync(CancellationToken cancellationToken = default)
          => await _context.RefreshTokens.ToListAsync(cancellationToken);
      
      public async Task<RefreshToken> AddAsync(RefreshToken entity, CancellationToken cancellationToken = default)
      {
          await _context.RefreshTokens.AddAsync(entity, cancellationToken);
          return entity;
      }
      
      public Task UpdateAsync(RefreshToken entity, CancellationToken cancellationToken = default)
      {
          _context.RefreshTokens.Update(entity);
          return Task.CompletedTask;
      }
      
      public Task DeleteAsync(RefreshToken entity, CancellationToken cancellationToken = default)
      {
          _context.RefreshTokens.Remove(entity);
          return Task.CompletedTask;
      }
  }
  ```

---

## Step 8: Infrastructure Layer – Services

- [x] Create `Allocore.Infrastructure/Services/JwtTokenService.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Services;
  
  using System.IdentityModel.Tokens.Jwt;
  using System.Security.Claims;
  using System.Security.Cryptography;
  using System.Text;
  using Microsoft.Extensions.Options;
  using Microsoft.IdentityModel.Tokens;
  using Allocore.Application.Abstractions.Services;
  using Allocore.Domain.Entities.Users;
  
  public class JwtTokenService : IJwtTokenService
  {
      private readonly JwtSettings _settings;
      
      public JwtTokenService(IOptions<JwtSettings> settings)
      {
          _settings = settings.Value;
      }
      
      public string GenerateAccessToken(User user)
      {
          var claims = new List<Claim>
          {
              new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
              new(JwtRegisteredClaimNames.Email, user.Email),
              new(ClaimTypes.NameIdentifier, user.Id.ToString()),
              new(ClaimTypes.Role, user.Role.ToString()),
              new("email_verified", user.IsEmailVerified.ToString().ToLower())
          };
          
          var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
          var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
          
          var token = new JwtSecurityToken(
              issuer: _settings.Issuer,
              audience: _settings.Audience,
              claims: claims,
              expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
              signingCredentials: creds
          );
          
          return new JwtSecurityTokenHandler().WriteToken(token);
      }
      
      public string GenerateRefreshToken()
      {
          var randomBytes = new byte[64];
          using var rng = RandomNumberGenerator.Create();
          rng.GetBytes(randomBytes);
          return Convert.ToBase64String(randomBytes);
      }
      
      public string HashToken(string token)
      {
          using var sha256 = SHA256.Create();
          var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
          return Convert.ToBase64String(bytes);
      }
      
      public bool ValidateRefreshToken(string token, string storedHash)
      {
          return HashToken(token) == storedHash;
      }
  }
  
  public class JwtSettings
  {
      public string Secret { get; set; } = string.Empty;
      public string Issuer { get; set; } = string.Empty;
      public string Audience { get; set; } = string.Empty;
      public int AccessTokenExpirationMinutes { get; set; } = 15;
      public int RefreshTokenExpirationDays { get; set; } = 7;
  }
  ```
- [x] Create `Allocore.Infrastructure/Services/DateTimeService.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Services;

  using Allocore.Application.Abstractions.Services;

  public class DateTimeService : IDateTime
  {
      public DateTime UtcNow => DateTime.UtcNow;
  }
  ```
- [x] Create `Allocore.Infrastructure/Services/CurrentUserService.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Services;

  using System.Security.Claims;
  using Microsoft.AspNetCore.Http;
  using Allocore.Application.Abstractions.Services;

  public class CurrentUserService : ICurrentUser
  {
      private readonly IHttpContextAccessor _httpContextAccessor;
      
      public CurrentUserService(IHttpContextAccessor httpContextAccessor)
      {
          _httpContextAccessor = httpContextAccessor;
      }
      
      public Guid? UserId
      {
          get
          {
              var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
              return Guid.TryParse(userId, out var id) ? id : null;
          }
      }
      
      public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
      
      public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?
          .FindAll(ClaimTypes.Role)
          .Select(c => c.Value) ?? Enumerable.Empty<string>();
      
      public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
  }
  ```
- [x] Create `Allocore.Infrastructure/Services/EmailService.cs` (stub implementation):
  ```csharp
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
  ```

---

## Step 9: Infrastructure Layer – Dependency Injection

- [x] Update `Allocore.Infrastructure/DependencyInjection.cs`:
  ```csharp
  namespace Allocore.Infrastructure;
  
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Application.Abstractions.Services;
  using Allocore.Infrastructure.Persistence;
  using Allocore.Infrastructure.Persistence.Repositories;
  using Allocore.Infrastructure.Services;
  
  public static class DependencyInjection
  {
      public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
      {
          // Database
          services.AddDbContext<ApplicationDbContext>(options =>
              options.UseNpgsql(configuration.GetConnectionString("Default")));
          
          // Repositories
          services.AddScoped<IUserRepository, UserRepository>();
          services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
          services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
          
          // Services
          services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
          services.AddScoped<IJwtTokenService, JwtTokenService>();
          services.AddScoped<IDateTime, DateTimeService>();
          services.AddScoped<IEmailService, EmailService>();
          services.AddScoped<ICurrentUser, CurrentUserService>();
          
          return services;
      }
  }
  ```

---

## Step 10: API Layer – Authentication Configuration

- [x] Install package:
  ```bash
  dotnet add Allocore.API package Microsoft.AspNetCore.Authentication.JwtBearer
  ```
- [x] Update `appsettings.json`:
  ```json
  {
    "ConnectionStrings": {
      "Default": "Host=localhost;Port=5432;Database=Allocore;Username=postgres;Password=t4P5X0Ae9"
    },
    "Jwt": {
      "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
      "Issuer": "Allocore",
      "Audience": "AllocoreClients",
      "AccessTokenExpirationMinutes": 15,
      "RefreshTokenExpirationDays": 7
    },
    "Lockout": {
      "MaxFailedAttempts": 5,
      "LockoutDurationMinutes": 15
    }
  }
  ```
- [x] Configure JWT authentication in `Program.cs`:
  ```csharp
  builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
          options.TokenValidationParameters = new TokenValidationParameters
          {
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateLifetime = true,
              ValidateIssuerSigningKey = true,
              ValidIssuer = builder.Configuration["Jwt:Issuer"],
              ValidAudience = builder.Configuration["Jwt:Audience"],
              IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
          };
      });
  ```
- [x] Configure authorization policies:
  ```csharp
  builder.Services.AddAuthorization(options =>
  {
      options.AddPolicy("CanManageUsers", policy =>
          policy.RequireRole("Admin"));
      
      options.AddPolicy("RequireVerifiedEmail", policy =>
          policy.RequireClaim("email_verified", "true"));
  });
  ```
- [x] Add rate limiting for login endpoint:
  ```csharp
  builder.Services.AddRateLimiter(options =>
  {
      options.AddFixedWindowLimiter("login", opt =>
      {
          opt.Window = TimeSpan.FromMinutes(1);
          opt.PermitLimit = 5;
          opt.QueueLimit = 0;
      });
  });
  ```
- [x] Add HttpContextAccessor for ICurrentUser:
  ```csharp
  builder.Services.AddHttpContextAccessor();
  ```
- [x] Configure middleware pipeline (add after `app.UseCors()`):
  ```csharp
  // Authentication & Authorization - MUST be in this order
  app.UseAuthentication();
  app.UseAuthorization();
  
  // Rate limiting
  app.UseRateLimiter();
  ```
  > **IMPORTANT**: `UseAuthentication()` must come BEFORE `UseAuthorization()`. The rate limiter should be added to the pipeline for the `[EnableRateLimiting]` attribute to work.

---

## Step 11: API Layer – Controllers

- [x] Create `Allocore.API/Controllers/v1/AuthController.cs`:
  ```csharp
  namespace Allocore.API.Controllers.v1;
  
  using Asp.Versioning;
  using MediatR;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.RateLimiting;
  using Allocore.Application.Features.Auth.Commands;
  using Allocore.Application.Features.Auth.DTOs;
  
  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/auth")]
  public class AuthController : ControllerBase
  {
      private readonly IMediator _mediator;
      
      public AuthController(IMediator mediator)
      {
          _mediator = mediator;
      }
      
      [HttpPost("register")]
      public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new RegisterCommand(request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }
      
      [HttpPost("login")]
      [EnableRateLimiting("login")]
      public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new LoginCommand(request), cancellationToken);
          if (!result.IsSuccess)
              return Unauthorized(new { error = result.Error });
          return Ok(result.Value);
      }
      
      [HttpPost("refresh")]
      public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new RefreshTokenCommand(request), cancellationToken);
          if (!result.IsSuccess)
              return Unauthorized(new { error = result.Error });
          return Ok(result.Value);
      }
      
      [HttpPost("logout")]
      public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
      {
          await _mediator.Send(new LogoutCommand(request), cancellationToken);
          return NoContent();
      }
      
      [HttpPost("forgot-password")]
      public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
      {
          await _mediator.Send(new ForgotPasswordCommand(request), cancellationToken);
          return Ok(new { message = "If the email exists, a reset link has been sent." });
      }
      
      [HttpPost("reset-password")]
      public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new ResetPasswordCommand(request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(new { message = "Password reset successfully." });
      }
  }
  ```
- [x] Create `Allocore.API/Controllers/v1/UsersController.cs`:
  ```csharp
  namespace Allocore.API.Controllers.v1;
  
  using Asp.Versioning;
  using MediatR;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Allocore.Application.Features.Users.Commands;
  using Allocore.Application.Features.Users.DTOs;
  using Allocore.Application.Features.Users.Queries;
  
  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/users")]
  [Authorize]
  public class UsersController : ControllerBase
  {
      private readonly IMediator _mediator;
      
      public UsersController(IMediator mediator)
      {
          _mediator = mediator;
      }
      
      [HttpGet("me")]
      public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new GetMeQuery(), cancellationToken);
          if (!result.IsSuccess)
              return NotFound(new { error = result.Error });
          return Ok(result.Value);
      }
      
      [HttpGet]
      [Authorize(Policy = "CanManageUsers")]
      public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(new GetUsersQuery(page, pageSize), cancellationToken);
          return Ok(result);
      }
      
      [HttpGet("{id:guid}")]
      [Authorize(Policy = "CanManageUsers")]
      public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
          if (!result.IsSuccess)
              return NotFound(new { error = result.Error });
          return Ok(result.Value);
      }
      
      [HttpPut("{id:guid}")]
      [Authorize(Policy = "CanManageUsers")]
      public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new UpdateUserCommand(id, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }
      
      [HttpDelete("{id:guid}")]
      [Authorize(Policy = "CanManageUsers")]
      public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }
  }
  ```

---

## Step 12: API Layer – Swagger Configuration

- [x] Update Swagger configuration for Bearer auth:
  ```csharp
  builder.Services.AddSwaggerGen(c =>
  {
      c.SwaggerDoc("v1", new() { Title = "Allocore API", Version = "v1" });
      
      c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
          Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
          Name = "Authorization",
          In = ParameterLocation.Header,
          Type = SecuritySchemeType.ApiKey,
          Scheme = "Bearer"
      });
      
      c.AddSecurityRequirement(new OpenApiSecurityRequirement
      {
          {
              new OpenApiSecurityScheme
              {
                  Reference = new OpenApiReference
                  {
                      Type = ReferenceType.SecurityScheme,
                      Id = "Bearer"
                  }
              },
              Array.Empty<string>()
          }
      });
  });
  ```

---

## Step 13: Database Seeding

- [x] Create `Allocore.Infrastructure/Persistence/Seeding/DatabaseSeeder.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Seeding;
  
  using BCrypt.Net;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Allocore.Domain.Entities.Users;
  
  public static class DatabaseSeeder
  {
      public static async Task SeedAsync(IServiceProvider services, IHostEnvironment environment)
      {
          if (!environment.IsDevelopment())
              return;
          
          using var scope = services.CreateScope();
          var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
          
          await context.Database.MigrateAsync();
          
          if (!await context.Users.AnyAsync())
          {
              var adminUser = User.Create(
                  email: "admin@local",
                  passwordHash: BCrypt.HashPassword("Passw0rd!A"),
                  firstName: "Admin",
                  lastName: "User",
                  role: Role.Admin
              );
              adminUser.VerifyEmail();
              
              await context.Users.AddAsync(adminUser);
              await context.SaveChangesAsync();
          }
      }
  }
  ```
- [x] Call seeder in `Program.cs`:
  ```csharp
  await DatabaseSeeder.SeedAsync(app.Services, app.Environment);
  ```

---

## Step 14: Migrations

- [x] Create initial migration (requires running PostgreSQL):
  ```bash
  dotnet ef migrations add InitAuth -s Allocore.API -p Allocore.Infrastructure
  ```
- [x] Apply migration (requires running PostgreSQL):
  ```bash
  dotnet ef database update -s Allocore.API -p Allocore.Infrastructure
  ```

---

## Step 15: Documentation Updates

- [ ] Update `Docs/Architecture.md` with Auth layer details (optional)
- [ ] Update `Docs/DevelopmentHistory.md` with v0.2 – Auth implementation (optional)
- [ ] Update `Docs/UserStories.md` with US002 reference (optional)

---

## Technical Details

### Dependencies

| Project | Package | Version |
|---------|---------|---------|
| Allocore.Infrastructure | Npgsql.EntityFrameworkCore.PostgreSQL | 8.x |
| Allocore.Infrastructure | BCrypt.Net-Next | 4.x |
| Allocore.API | Microsoft.AspNetCore.Authentication.JwtBearer | 8.x |

### Project Structure

```
Allocore.Domain/
├── Users/
│   ├── User.cs
│   ├── Role.cs
│   ├── LocaleTag.cs
│   └── RefreshToken.cs

Allocore.Application/
├── Abstractions/
│   ├── Persistence/
│   │   ├── IUserRepository.cs
│   │   ├── IRefreshTokenRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Services/
│       ├── ICurrentUser.cs
│       ├── IJwtTokenService.cs
│       ├── IDateTime.cs
│       └── IEmailService.cs
├── Features/
│   ├── Auth/
│   │   ├── Commands/
│   │   │   ├── RegisterCommand.cs
│   │   │   ├── LoginCommand.cs
│   │   │   ├── RefreshTokenCommand.cs
│   │   │   ├── LogoutCommand.cs
│   │   │   ├── ForgotPasswordCommand.cs
│   │   │   └── ResetPasswordCommand.cs
│   │   ├── DTOs/
│   │   │   ├── RegisterRequest.cs
│   │   │   ├── LoginRequest.cs
│   │   │   ├── RefreshTokenRequest.cs
│   │   │   ├── ForgotPasswordRequest.cs
│   │   │   ├── ResetPasswordRequest.cs
│   │   │   └── AuthResponse.cs
│   │   └── Validators/
│   │       ├── RegisterRequestValidator.cs
│   │       └── LoginRequestValidator.cs
│   └── Users/
│       ├── Commands/
│       │   ├── UpdateUserCommand.cs
│       │   └── DeleteUserCommand.cs
│       ├── DTOs/
│       │   ├── UserDto.cs
│       │   └── UpdateUserRequest.cs
│       └── Queries/
│           ├── GetMeQuery.cs
│           ├── GetUsersQuery.cs
│           └── GetUserByIdQuery.cs

Allocore.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   ├── UserConfiguration.cs
│   │   └── RefreshTokenConfiguration.cs
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   └── RefreshTokenRepository.cs
│   └── Seeding/
│       └── DatabaseSeeder.cs
├── Services/
│   ├── JwtTokenService.cs
│   ├── DateTimeService.cs
│   ├── CurrentUserService.cs
│   └── EmailService.cs
└── DependencyInjection.cs

Allocore.API/
├── Controllers/v1/
│   ├── AuthController.cs
│   └── UsersController.cs
```

### Database

**Table: Users**

| Column | Type | Constraints |
|--------|------|-------------|
| Id | uuid | PK |
| Email | varchar(256) | NOT NULL, UNIQUE |
| PasswordHash | varchar(256) | NOT NULL |
| FirstName | varchar(100) | NOT NULL |
| LastName | varchar(100) | NOT NULL |
| Role | varchar(50) | NOT NULL |
| IsEmailVerified | boolean | NOT NULL, DEFAULT false |
| IsActive | boolean | NOT NULL, DEFAULT true |
| Locale | varchar(10) | |
| FailedLoginAttempts | integer | NOT NULL, DEFAULT 0 |
| LockoutEnd | timestamp | |
| PasswordResetToken | varchar(256) | |
| PasswordResetTokenExpiry | timestamp | |
| CreatedAt | timestamp | NOT NULL |
| UpdatedAt | timestamp | |

**Table: RefreshTokens**

| Column | Type | Constraints |
|--------|------|-------------|
| Id | uuid | PK |
| UserId | uuid | NOT NULL, FK → Users |
| TokenHash | varchar(256) | NOT NULL, INDEX |
| ExpiresAt | timestamp | NOT NULL |
| IsRevoked | boolean | NOT NULL, DEFAULT false |
| RevokedAt | timestamp | |
| ReplacedByTokenHash | varchar(256) | |
| DeviceInfo | varchar(500) | |
| IpAddress | varchar(45) | |
| CreatedAt | timestamp | NOT NULL |
| UpdatedAt | timestamp | |

### API Contract

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/v1/auth/register` | POST | No | Register new user |
| `/api/v1/auth/login` | POST | No | Login (rate limited) |
| `/api/v1/auth/refresh` | POST | No | Refresh tokens |
| `/api/v1/auth/logout` | POST | No | Revoke refresh token |
| `/api/v1/auth/forgot-password` | POST | No | Request password reset |
| `/api/v1/auth/reset-password` | POST | No | Reset password with token |
| `/api/v1/users/me` | GET | Yes | Get current user |
| `/api/v1/users` | GET | Admin | List users (paginated) |
| `/api/v1/users/{id}` | GET | Admin | Get user by ID |
| `/api/v1/users/{id}` | PUT | Admin | Update user |
| `/api/v1/users/{id}` | DELETE | Admin | Delete user |

**Request/Response Examples:**

```json
// POST /api/v1/auth/register
{
  "email": "user@example.com",
  "password": "SecureP@ss123",
  "firstName": "John",
  "lastName": "Doe"
}

// Response
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2g...",
  "expiresAt": "2024-01-15T10:45:00Z",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "User",
    "isEmailVerified": false,
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

### Authentication/Authorization

- **JWT Claims:**
  - `sub`: User ID
  - `email`: User email
  - `nameid`: User ID
  - `role`: User role (User/Admin)
  - `email_verified`: Boolean string

- **Policies:**
  - `CanManageUsers`: Requires Admin role
  - `RequireVerifiedEmail`: Requires `email_verified` claim = "true"

- **Security Rules:**
  - Passwords hashed with BCrypt (work factor 12)
  - Refresh tokens hashed with SHA256
  - Refresh token rotation on each use
  - Lockout after 5 failed login attempts for 15 minutes
  - Rate limiting: 5 login attempts per minute per IP

---

## Acceptance Criteria

- [ ] All auth endpoints work end-to-end against PostgreSQL
- [ ] Tokens contain sub, nameid, email, role(s) and respect policies
- [ ] Refresh tokens are hashed, rotated, and revocable
- [ ] Lockout and failed login reset implemented
- [ ] Swagger displays Bearer Auth and allows testing authenticated endpoints
- [ ] Admin seed created in Development environment
- [ ] `dotnet build` passes without errors
- [ ] Rate limiting active on login endpoint
