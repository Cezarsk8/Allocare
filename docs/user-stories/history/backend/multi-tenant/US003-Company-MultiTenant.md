# US003 – Company Management & User ↔ Company Mapping (Multi-Tenant Core)

## Description

**As** an Admin user of Allocore,  
**I need** to create and manage companies and link users to those companies,  
**So that** I can completely separate cost/metrics data for each organization and allow a single admin to manage multiple companies.

---

## Step 1: Domain Layer – Company & UserCompany Entities

- [x] ✅ DONE Create `Allocore.Domain/Companies/` folder
- [x] ✅ DONE Create `Allocore.Domain/Companies/Company.cs`:
  ```csharp
  namespace Allocore.Domain.Companies;
  
  using Allocore.Domain.Common;
  
  public class Company : Entity
  {
      public string Name { get; private set; } = string.Empty;
      public string? LegalName { get; private set; }
      public string? TaxId { get; private set; }
      public bool IsActive { get; private set; } = true;
      
      private readonly List<UserCompany> _userCompanies = new();
      public IReadOnlyCollection<UserCompany> UserCompanies => _userCompanies.AsReadOnly();
      
      private Company() { } // EF Core
      
      public static Company Create(string name, string? legalName = null, string? taxId = null)
      {
          return new Company
          {
              Name = name,
              LegalName = legalName,
              TaxId = taxId,
              IsActive = true
          };
      }
      
      public void Update(string name, string? legalName, string? taxId)
      {
          Name = name;
          LegalName = legalName;
          TaxId = taxId;
          UpdatedAt = DateTime.UtcNow;
      }
      
      public void Deactivate()
      {
          IsActive = false;
          UpdatedAt = DateTime.UtcNow;
      }
      
      public void Activate()
      {
          IsActive = true;
          UpdatedAt = DateTime.UtcNow;
      }
  }
  ```
- [x] ✅ DONE Create `Allocore.Domain/Companies/RoleInCompany.cs`:
  ```csharp
  namespace Allocore.Domain.Companies;
  
  public enum RoleInCompany
  {
      Viewer = 0,
      Manager = 1,
      Owner = 2
  }
  ```
- [x] ✅ DONE Create `Allocore.Domain/Companies/UserCompany.cs`:
  ```csharp
  namespace Allocore.Domain.Companies;
  
  using Allocore.Domain.Common;
  using Allocore.Domain.Users;
  
  public class UserCompany : Entity
  {
      public Guid UserId { get; private set; }
      public Guid CompanyId { get; private set; }
      public RoleInCompany RoleInCompany { get; private set; }
      
      // Navigation properties
      public User? User { get; private set; }
      public Company? Company { get; private set; }
      
      private UserCompany() { } // EF Core
      
      public static UserCompany Create(Guid userId, Guid companyId, RoleInCompany roleInCompany)
      {
          return new UserCompany
          {
              UserId = userId,
              CompanyId = companyId,
              RoleInCompany = roleInCompany
          };
      }
      
      public void UpdateRole(RoleInCompany newRole)
      {
          RoleInCompany = newRole;
          UpdatedAt = DateTime.UtcNow;
      }
  }
  ```
- [x] ✅ DONE Update `Allocore.Domain/Users/User.cs` to add navigation property:
  ```csharp
  // Add to User class
  private readonly List<UserCompany> _userCompanies = new();
  public IReadOnlyCollection<UserCompany> UserCompanies => _userCompanies.AsReadOnly();
  ```

---

## Step 2: Application Layer – Abstractions & Interfaces

- [x] ✅ DONE Create `Allocore.Application/Abstractions/Persistence/ICompanyRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;
  
  using Allocore.Domain.Companies;
  
  public interface ICompanyRepository : IReadRepository<Company>, IWriteRepository<Company>
  {
      Task<Company?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default);
      Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
      Task<(IEnumerable<Company> Companies, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
  }
  ```
- [x] ✅ DONE Create `Allocore.Application/Abstractions/Persistence/IUserCompanyRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;
  
  using Allocore.Domain.Companies;
  
  public interface IUserCompanyRepository : IReadRepository<UserCompany>, IWriteRepository<UserCompany>
  {
      Task<UserCompany?> GetByUserAndCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
      Task<IEnumerable<UserCompany>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
      Task<IEnumerable<UserCompany>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
      Task<bool> UserHasAccessToCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
      Task<bool> UserIsOwnerOfCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
  }
  ```

---

## Step 3: Application Layer – DTOs

- [x] ✅ DONE Create `Allocore.Application/Features/Companies/DTOs/CompanyDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.DTOs;
  
  public record CompanyDto(
      Guid Id,
      string Name,
      string? LegalName,
      string? TaxId,
      bool IsActive,
      DateTime CreatedAt,
      string? UserRole  // Role of current user in this company (when applicable)
  );
  ```
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/DTOs/CreateCompanyRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.DTOs;
  
  public record CreateCompanyRequest(
      string Name,
      string? LegalName,
      string? TaxId
  );
  ```
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/DTOs/UpdateCompanyRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.DTOs;
  
  public record UpdateCompanyRequest(
      string Name,
      string? LegalName,
      string? TaxId
  );
  ```
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/DTOs/AddUserToCompanyRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.DTOs;
  
  public record AddUserToCompanyRequest(
      Guid UserId,
      string RoleInCompany  // "Viewer", "Manager", "Owner"
  );
  ```
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/DTOs/UserCompanyDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.DTOs;
  
  public record UserCompanyDto(
      Guid UserId,
      string UserEmail,
      string UserFullName,
      Guid CompanyId,
      string CompanyName,
      string RoleInCompany,
      DateTime JoinedAt
  );
  ```

---

## Step 4: Application Layer – Validators

- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Validators/CreateCompanyRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.Validators;
  
  using FluentValidation;
  using Allocore.Application.Features.Companies.DTOs;
  
  public class CreateCompanyRequestValidator : AbstractValidator<CreateCompanyRequest>
  {
      public CreateCompanyRequestValidator()
      {
          RuleFor(x => x.Name)
              .NotEmpty().WithMessage("Company name is required")
              .MaximumLength(200).WithMessage("Company name must not exceed 200 characters");
          
          RuleFor(x => x.LegalName)
              .MaximumLength(300).WithMessage("Legal name must not exceed 300 characters")
              .When(x => !string.IsNullOrEmpty(x.LegalName));
          
          RuleFor(x => x.TaxId)
              .MaximumLength(50).WithMessage("Tax ID must not exceed 50 characters")
              .When(x => !string.IsNullOrEmpty(x.TaxId));
      }
  }
  ```
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Validators/UpdateCompanyRequestValidator.cs`
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Validators/AddUserToCompanyRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.Validators;
  
  using FluentValidation;
  using Allocore.Application.Features.Companies.DTOs;
  
  public class AddUserToCompanyRequestValidator : AbstractValidator<AddUserToCompanyRequest>
  {
      public AddUserToCompanyRequestValidator()
      {
          RuleFor(x => x.UserId)
              .NotEmpty().WithMessage("User ID is required");
          
          RuleFor(x => x.RoleInCompany)
              .NotEmpty().WithMessage("Role is required")
              .Must(role => role is "Viewer" or "Manager" or "Owner")
              .WithMessage("Role must be one of: Viewer, Manager, Owner");
      }
  }
  ```

---

## Step 5: Application Layer – CQRS Commands & Queries

- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Commands/CreateCompanyCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.Commands;
  
  using MediatR;
  using Allocore.Application.Features.Companies.DTOs;
  using Allocore.Domain.Common;
  
  public record CreateCompanyCommand(CreateCompanyRequest Request) : IRequest<Result<CompanyDto>>;
  ```
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Commands/CreateCompanyCommandHandler.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.Commands;
  
  using MediatR;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Application.Abstractions.Services;
  using Allocore.Application.Features.Companies.DTOs;
  using Allocore.Domain.Common;
  using Allocore.Domain.Companies;
  
  public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Result<CompanyDto>>
  {
      private readonly ICompanyRepository _companyRepository;
      private readonly IUserCompanyRepository _userCompanyRepository;
      private readonly ICurrentUser _currentUser;
      private readonly IUnitOfWork _unitOfWork;
      
      public CreateCompanyCommandHandler(
          ICompanyRepository companyRepository,
          IUserCompanyRepository userCompanyRepository,
          ICurrentUser currentUser,
          IUnitOfWork unitOfWork)
      {
          _companyRepository = companyRepository;
          _userCompanyRepository = userCompanyRepository;
          _currentUser = currentUser;
          _unitOfWork = unitOfWork;
      }
      
      public async Task<Result<CompanyDto>> Handle(CreateCompanyCommand command, CancellationToken cancellationToken)
      {
          var request = command.Request;
          
          // Check for duplicate name
          if (await _companyRepository.ExistsByNameAsync(request.Name, cancellationToken))
              return Result<CompanyDto>.Failure("A company with this name already exists.");
          
          // Create company
          var company = Company.Create(request.Name, request.LegalName, request.TaxId);
          await _companyRepository.AddAsync(company, cancellationToken);
          
          // Link creating user as Owner
          if (_currentUser.UserId.HasValue)
          {
              var userCompany = UserCompany.Create(_currentUser.UserId.Value, company.Id, RoleInCompany.Owner);
              await _userCompanyRepository.AddAsync(userCompany, cancellationToken);
          }
          
          await _unitOfWork.SaveChangesAsync(cancellationToken);
          
          return Result<CompanyDto>.Success(new CompanyDto(
              company.Id,
              company.Name,
              company.LegalName,
              company.TaxId,
              company.IsActive,
              company.CreatedAt,
              "Owner"
          ));
      }
  }
  ```
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Commands/UpdateCompanyCommand.cs` and handler
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Commands/AddUserToCompanyCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.Commands;
  
  using MediatR;
  using Allocore.Application.Features.Companies.DTOs;
  using Allocore.Domain.Common;
  
  public record AddUserToCompanyCommand(Guid CompanyId, AddUserToCompanyRequest Request) : IRequest<Result<UserCompanyDto>>;
  ```
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Commands/AddUserToCompanyCommandHandler.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.Commands;
  
  using MediatR;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Application.Abstractions.Services;
  using Allocore.Application.Features.Companies.DTOs;
  using Allocore.Domain.Common;
  using Allocore.Domain.Companies;
  
  public class AddUserToCompanyCommandHandler : IRequestHandler<AddUserToCompanyCommand, Result<UserCompanyDto>>
  {
      private readonly ICompanyRepository _companyRepository;
      private readonly IUserRepository _userRepository;
      private readonly IUserCompanyRepository _userCompanyRepository;
      private readonly ICurrentUser _currentUser;
      private readonly IUnitOfWork _unitOfWork;
      
      public AddUserToCompanyCommandHandler(
          ICompanyRepository companyRepository,
          IUserRepository userRepository,
          IUserCompanyRepository userCompanyRepository,
          ICurrentUser currentUser,
          IUnitOfWork unitOfWork)
      {
          _companyRepository = companyRepository;
          _userRepository = userRepository;
          _userCompanyRepository = userCompanyRepository;
          _currentUser = currentUser;
          _unitOfWork = unitOfWork;
      }
      
      public async Task<Result<UserCompanyDto>> Handle(AddUserToCompanyCommand command, CancellationToken cancellationToken)
      {
          // Verify company exists
          var company = await _companyRepository.GetByIdAsync(command.CompanyId, cancellationToken);
          if (company is null)
              return Result<UserCompanyDto>.Failure("Company not found.");
          
          // Verify current user has permission (Owner or Admin)
          if (_currentUser.UserId.HasValue)
          {
              var isOwner = await _userCompanyRepository.UserIsOwnerOfCompanyAsync(_currentUser.UserId.Value, command.CompanyId, cancellationToken);
              var isAdmin = _currentUser.Roles.Contains("Admin");
              
              if (!isOwner && !isAdmin)
                  return Result<UserCompanyDto>.Failure("You don't have permission to add users to this company.");
          }
          
          // Verify target user exists
          var user = await _userRepository.GetByIdAsync(command.Request.UserId, cancellationToken);
          if (user is null)
              return Result<UserCompanyDto>.Failure("User not found.");
          
          // Check if user is already linked
          var existingLink = await _userCompanyRepository.GetByUserAndCompanyAsync(command.Request.UserId, command.CompanyId, cancellationToken);
          if (existingLink is not null)
              return Result<UserCompanyDto>.Failure("User is already linked to this company.");
          
          // Parse role
          if (!Enum.TryParse<RoleInCompany>(command.Request.RoleInCompany, out var role))
              return Result<UserCompanyDto>.Failure("Invalid role specified.");
          
          // Create link
          var userCompany = UserCompany.Create(command.Request.UserId, command.CompanyId, role);
          await _userCompanyRepository.AddAsync(userCompany, cancellationToken);
          await _unitOfWork.SaveChangesAsync(cancellationToken);
          
          return Result<UserCompanyDto>.Success(new UserCompanyDto(
              user.Id,
              user.Email,
              user.FullName,
              company.Id,
              company.Name,
              role.ToString(),
              userCompany.CreatedAt
          ));
      }
  }
  ```
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Commands/RemoveUserFromCompanyCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.Commands;
  
  using MediatR;
  using Allocore.Domain.Common;
  
  public record RemoveUserFromCompanyCommand(Guid CompanyId, Guid UserId) : IRequest<Result>;
  ```
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Commands/RemoveUserFromCompanyCommandHandler.cs`:
  - Verify permissions (Owner or Admin)
  - Prevent removing the last Owner
  - Delete UserCompany record
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Queries/GetMyCompaniesQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.Queries;
  
  using MediatR;
  using Allocore.Application.Features.Companies.DTOs;
  
  public record GetMyCompaniesQuery : IRequest<IEnumerable<CompanyDto>>;
  ```
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Queries/GetMyCompaniesQueryHandler.cs`:
  ```csharp
  namespace Allocore.Application.Features.Companies.Queries;
  
  using MediatR;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Application.Abstractions.Services;
  using Allocore.Application.Features.Companies.DTOs;
  
  public class GetMyCompaniesQueryHandler : IRequestHandler<GetMyCompaniesQuery, IEnumerable<CompanyDto>>
  {
      private readonly IUserCompanyRepository _userCompanyRepository;
      private readonly ICurrentUser _currentUser;
      
      public GetMyCompaniesQueryHandler(
          IUserCompanyRepository userCompanyRepository,
          ICurrentUser currentUser)
      {
          _userCompanyRepository = userCompanyRepository;
          _currentUser = currentUser;
      }
      
      public async Task<IEnumerable<CompanyDto>> Handle(GetMyCompaniesQuery request, CancellationToken cancellationToken)
      {
          if (!_currentUser.UserId.HasValue)
              return Enumerable.Empty<CompanyDto>();
          
          var userCompanies = await _userCompanyRepository.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
          
          return userCompanies
              .Where(uc => uc.Company is not null)
              .Select(uc => new CompanyDto(
                  uc.Company!.Id,
                  uc.Company.Name,
                  uc.Company.LegalName,
                  uc.Company.TaxId,
                  uc.Company.IsActive,
                  uc.Company.CreatedAt,
                  uc.RoleInCompany.ToString()
              ));
      }
  }
  ```
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Queries/GetCompanyByIdQuery.cs` and handler
- [x] ✅ DONE Create `Allocore.Application/Features/Companies/Queries/GetCompanyUsersQuery.cs` and handler:
  - Returns all users linked to a company with their roles

---

## Step 6: Infrastructure Layer – Database Configuration

- [x] ✅ DONE Create `Allocore.Infrastructure/Persistence/Configurations/CompanyConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;
  
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Companies;
  
  public class CompanyConfiguration : IEntityTypeConfiguration<Company>
  {
      public void Configure(EntityTypeBuilder<Company> builder)
      {
          builder.ToTable("Companies");
          
          builder.HasKey(c => c.Id);
          
          builder.Property(c => c.Name)
              .IsRequired()
              .HasMaxLength(200);
          
          builder.HasIndex(c => c.Name);
          
          builder.Property(c => c.LegalName)
              .HasMaxLength(300);
          
          builder.Property(c => c.TaxId)
              .HasMaxLength(50);
          
          builder.HasIndex(c => c.TaxId)
              .IsUnique()
              .HasFilter("\"TaxId\" IS NOT NULL");
          
          builder.HasMany(c => c.UserCompanies)
              .WithOne(uc => uc.Company)
              .HasForeignKey(uc => uc.CompanyId)
              .OnDelete(DeleteBehavior.Cascade);
      }
  }
  ```
- [x] ✅ DONE Create `Allocore.Infrastructure/Persistence/Configurations/UserCompanyConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;
  
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Companies;
  
  public class UserCompanyConfiguration : IEntityTypeConfiguration<UserCompany>
  {
      public void Configure(EntityTypeBuilder<UserCompany> builder)
      {
          builder.ToTable("UserCompanies");
          
          builder.HasKey(uc => uc.Id);
          
          // Unique constraint on UserId + CompanyId
          builder.HasIndex(uc => new { uc.CompanyId, uc.UserId })
              .IsUnique();
          
          builder.HasIndex(uc => uc.UserId);
          builder.HasIndex(uc => uc.CompanyId);
          
          builder.Property(uc => uc.RoleInCompany)
              .HasConversion<string>()
              .HasMaxLength(50);
          
          builder.HasOne(uc => uc.User)
              .WithMany(u => u.UserCompanies)
              .HasForeignKey(uc => uc.UserId)
              .OnDelete(DeleteBehavior.Cascade);
          
          builder.HasOne(uc => uc.Company)
              .WithMany(c => c.UserCompanies)
              .HasForeignKey(uc => uc.CompanyId)
              .OnDelete(DeleteBehavior.Cascade);
      }
  }
  ```
- [x] ✅ DONE Update `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs`:
  ```csharp
  // Add DbSets
  public DbSet<Company> Companies => Set<Company>();
  public DbSet<UserCompany> UserCompanies => Set<UserCompany>();
  ```

---

## Step 7: Infrastructure Layer – Repository Implementations

- [x] ✅ DONE Create `Allocore.Infrastructure/Persistence/Repositories/CompanyRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Repositories;
  
  using Microsoft.EntityFrameworkCore;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Companies;
  
  public class CompanyRepository : ICompanyRepository
  {
      private readonly ApplicationDbContext _context;
      
      public CompanyRepository(ApplicationDbContext context)
      {
          _context = context;
      }
      
      public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
      
      public async Task<Company?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default)
          => await _context.Companies.FirstOrDefaultAsync(c => c.TaxId == taxId, cancellationToken);
      
      public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
          => await _context.Companies.AnyAsync(c => c.Name == name, cancellationToken);
      
      public async Task<IEnumerable<Company>> GetAllAsync(CancellationToken cancellationToken = default)
          => await _context.Companies.ToListAsync(cancellationToken);
      
      public async Task<(IEnumerable<Company> Companies, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
      {
          var totalCount = await _context.Companies.CountAsync(cancellationToken);
          var companies = await _context.Companies
              .OrderBy(c => c.Name)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync(cancellationToken);
          return (companies, totalCount);
      }
      
      public async Task<Company> AddAsync(Company entity, CancellationToken cancellationToken = default)
      {
          await _context.Companies.AddAsync(entity, cancellationToken);
          return entity;
      }
      
      public Task UpdateAsync(Company entity, CancellationToken cancellationToken = default)
      {
          _context.Companies.Update(entity);
          return Task.CompletedTask;
      }
      
      public Task DeleteAsync(Company entity, CancellationToken cancellationToken = default)
      {
          _context.Companies.Remove(entity);
          return Task.CompletedTask;
      }
  }
  ```
- [x] ✅ DONE Create `Allocore.Infrastructure/Persistence/Repositories/UserCompanyRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Repositories;
  
  using Microsoft.EntityFrameworkCore;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Companies;
  
  public class UserCompanyRepository : IUserCompanyRepository
  {
      private readonly ApplicationDbContext _context;
      
      public UserCompanyRepository(ApplicationDbContext context)
      {
          _context = context;
      }
      
      public async Task<UserCompany?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.UserCompanies
              .Include(uc => uc.User)
              .Include(uc => uc.Company)
              .FirstOrDefaultAsync(uc => uc.Id == id, cancellationToken);
      
      public async Task<UserCompany?> GetByUserAndCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
          => await _context.UserCompanies
              .Include(uc => uc.User)
              .Include(uc => uc.Company)
              .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == companyId, cancellationToken);
      
      public async Task<IEnumerable<UserCompany>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
          => await _context.UserCompanies
              .Include(uc => uc.Company)
              .Where(uc => uc.UserId == userId)
              .ToListAsync(cancellationToken);
      
      public async Task<IEnumerable<UserCompany>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
          => await _context.UserCompanies
              .Include(uc => uc.User)
              .Where(uc => uc.CompanyId == companyId)
              .ToListAsync(cancellationToken);
      
      public async Task<bool> UserHasAccessToCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
          => await _context.UserCompanies.AnyAsync(uc => uc.UserId == userId && uc.CompanyId == companyId, cancellationToken);
      
      public async Task<bool> UserIsOwnerOfCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
          => await _context.UserCompanies.AnyAsync(uc => uc.UserId == userId && uc.CompanyId == companyId && uc.RoleInCompany == RoleInCompany.Owner, cancellationToken);
      
      public async Task<IEnumerable<UserCompany>> GetAllAsync(CancellationToken cancellationToken = default)
          => await _context.UserCompanies.ToListAsync(cancellationToken);
      
      public async Task<UserCompany> AddAsync(UserCompany entity, CancellationToken cancellationToken = default)
      {
          await _context.UserCompanies.AddAsync(entity, cancellationToken);
          return entity;
      }
      
      public Task UpdateAsync(UserCompany entity, CancellationToken cancellationToken = default)
      {
          _context.UserCompanies.Update(entity);
          return Task.CompletedTask;
      }
      
      public Task DeleteAsync(UserCompany entity, CancellationToken cancellationToken = default)
      {
          _context.UserCompanies.Remove(entity);
          return Task.CompletedTask;
      }
  }
  ```

---

## Step 8: Infrastructure Layer – Dependency Injection Update

- [x] ✅ DONE Update `Allocore.Infrastructure/DependencyInjection.cs`:
  ```csharp
  // Add repository registrations
  services.AddScoped<ICompanyRepository, CompanyRepository>();
  services.AddScoped<IUserCompanyRepository, UserCompanyRepository>();
  ```

---

## Step 9: Infrastructure Layer – Database Seeding Update

- [x] ✅ DONE Update `Allocore.Infrastructure/Persistence/Seeding/DatabaseSeeder.cs`:
  ```csharp
  // After creating admin user, create test company
  if (!await context.Companies.AnyAsync())
  {
      var testCompany = Company.Create(
          name: "Test Company",
          legalName: "Test Company LLC",
          taxId: "12-3456789"
      );
      
      await context.Companies.AddAsync(testCompany);
      await context.SaveChangesAsync();
      
      // Link admin to test company as Owner
      var adminUser = await context.Users.FirstAsync(u => u.Email == "admin@local");
      var userCompany = UserCompany.Create(adminUser.Id, testCompany.Id, RoleInCompany.Owner);
      
      await context.UserCompanies.AddAsync(userCompany);
      await context.SaveChangesAsync();
  }
  ```

---

## Step 10: API Layer – Controllers

- [x] ✅ DONE Create `Allocore.API/Controllers/v1/CompaniesController.cs`:
  ```csharp
  namespace Allocore.API.Controllers.v1;
  
  using Asp.Versioning;
  using MediatR;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Allocore.Application.Features.Companies.Commands;
  using Allocore.Application.Features.Companies.DTOs;
  using Allocore.Application.Features.Companies.Queries;
  
  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/companies")]
  [Authorize]
  public class CompaniesController : ControllerBase
  {
      private readonly IMediator _mediator;
      
      public CompaniesController(IMediator mediator)
      {
          _mediator = mediator;
      }
      
      /// <summary>
      /// Create a new company. The creating user becomes the Owner.
      /// </summary>
      [HttpPost]
      [Authorize(Policy = "CanManageUsers")] // Admin only for MVP
      public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new CreateCompanyCommand(request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return CreatedAtAction(nameof(GetCompany), new { id = result.Value!.Id }, result.Value);
      }
      
      /// <summary>
      /// Get a company by ID.
      /// </summary>
      [HttpGet("{id:guid}")]
      public async Task<IActionResult> GetCompany(Guid id, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new GetCompanyByIdQuery(id), cancellationToken);
          if (!result.IsSuccess)
              return NotFound(new { error = result.Error });
          return Ok(result.Value);
      }
      
      /// <summary>
      /// Update a company.
      /// </summary>
      [HttpPut("{id:guid}")]
      public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new UpdateCompanyCommand(id, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }
      
      /// <summary>
      /// Add a user to a company.
      /// </summary>
      [HttpPost("{companyId:guid}/users")]
      public async Task<IActionResult> AddUserToCompany(Guid companyId, [FromBody] AddUserToCompanyRequest request, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new AddUserToCompanyCommand(companyId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }
      
      /// <summary>
      /// Remove a user from a company.
      /// </summary>
      [HttpDelete("{companyId:guid}/users/{userId:guid}")]
      public async Task<IActionResult> RemoveUserFromCompany(Guid companyId, Guid userId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new RemoveUserFromCompanyCommand(companyId, userId), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }
      
      /// <summary>
      /// Get all users in a company.
      /// </summary>
      [HttpGet("{companyId:guid}/users")]
      public async Task<IActionResult> GetCompanyUsers(Guid companyId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new GetCompanyUsersQuery(companyId), cancellationToken);
          return Ok(result);
      }
  }
  ```
- [x] ✅ DONE Create `Allocore.API/Controllers/v1/MyController.cs` (for user-scoped endpoints):
  ```csharp
  namespace Allocore.API.Controllers.v1;
  
  using Asp.Versioning;
  using MediatR;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Allocore.Application.Features.Companies.Queries;
  
  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/my")]
  [Authorize]
  public class MyController : ControllerBase
  {
      private readonly IMediator _mediator;
      
      public MyController(IMediator mediator)
      {
          _mediator = mediator;
      }
      
      /// <summary>
      /// Get all companies the current user is associated with.
      /// </summary>
      [HttpGet("companies")]
      public async Task<IActionResult> GetMyCompanies(CancellationToken cancellationToken)
      {
          var companies = await _mediator.Send(new GetMyCompaniesQuery(), cancellationToken);
          return Ok(companies);
      }
  }
  ```

---

## Step 11: Migrations

- [x] ✅ DONE Create migration for Companies:
  ```bash
  dotnet ef migrations add AddCompanies -s Allocore.API -p Allocore.Infrastructure
  ```
- [x] ✅ DONE Apply migration:
  ```bash
  dotnet ef database update -s Allocore.API -p Allocore.Infrastructure
  ```

---

## Step 12: Documentation Updates

- [x] ✅ DONE Update `Docs/Architecture.md` with multi-tenant layer details
- [x] ✅ DONE Update `Docs/DevelopmentHistory.md` with v0.3 – Companies implementation
- [x] ✅ DONE Update `Docs/UserStories.md` with US003 reference

---

## Technical Details

### Dependencies

No new packages required beyond US002.

### Project Structure

```
Allocore.Domain/
├── Companies/
│   ├── Company.cs
│   ├── RoleInCompany.cs
│   └── UserCompany.cs
├── Users/
│   └── User.cs (updated with UserCompanies navigation)

Allocore.Application/
├── Abstractions/
│   └── Persistence/
│       ├── ICompanyRepository.cs
│       └── IUserCompanyRepository.cs
├── Features/
│   └── Companies/
│       ├── Commands/
│       │   ├── CreateCompanyCommand.cs
│       │   ├── UpdateCompanyCommand.cs
│       │   ├── AddUserToCompanyCommand.cs
│       │   └── RemoveUserFromCompanyCommand.cs
│       ├── DTOs/
│       │   ├── CompanyDto.cs
│       │   ├── CreateCompanyRequest.cs
│       │   ├── UpdateCompanyRequest.cs
│       │   ├── AddUserToCompanyRequest.cs
│       │   └── UserCompanyDto.cs
│       ├── Queries/
│       │   ├── GetMyCompaniesQuery.cs
│       │   ├── GetCompanyByIdQuery.cs
│       │   └── GetCompanyUsersQuery.cs
│       └── Validators/
│           ├── CreateCompanyRequestValidator.cs
│           ├── UpdateCompanyRequestValidator.cs
│           └── AddUserToCompanyRequestValidator.cs

Allocore.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs (updated)
│   ├── Configurations/
│   │   ├── CompanyConfiguration.cs
│   │   └── UserCompanyConfiguration.cs
│   ├── Repositories/
│   │   ├── CompanyRepository.cs
│   │   └── UserCompanyRepository.cs
│   └── Seeding/
│       └── DatabaseSeeder.cs (updated)
└── DependencyInjection.cs (updated)

Allocore.API/
├── Controllers/v1/
│   ├── CompaniesController.cs
│   └── MyController.cs
```

### Database

**Table: Companies**

| Column | Type | Constraints |
|--------|------|-------------|
| Id | uuid | PK |
| Name | varchar(200) | NOT NULL, INDEX |
| LegalName | varchar(300) | |
| TaxId | varchar(50) | UNIQUE (where not null) |
| IsActive | boolean | NOT NULL, DEFAULT true |
| CreatedAt | timestamp | NOT NULL |
| UpdatedAt | timestamp | |

**Table: UserCompanies**

| Column | Type | Constraints |
|--------|------|-------------|
| Id | uuid | PK |
| UserId | uuid | NOT NULL, FK → Users, INDEX |
| CompanyId | uuid | NOT NULL, FK → Companies, INDEX |
| RoleInCompany | varchar(50) | NOT NULL |
| CreatedAt | timestamp | NOT NULL |
| UpdatedAt | timestamp | |

**Indexes:**
- `IX_UserCompanies_CompanyId_UserId` (UNIQUE)
- `IX_UserCompanies_UserId`
- `IX_UserCompanies_CompanyId`
- `IX_Companies_Name`
- `IX_Companies_TaxId` (UNIQUE, filtered)

### API Contract

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/v1/companies` | POST | Admin | Create new company |
| `/api/v1/companies/{id}` | GET | Yes | Get company by ID |
| `/api/v1/companies/{id}` | PUT | Owner/Admin | Update company |
| `/api/v1/companies/{companyId}/users` | POST | Owner/Admin | Add user to company |
| `/api/v1/companies/{companyId}/users/{userId}` | DELETE | Owner/Admin | Remove user from company |
| `/api/v1/companies/{companyId}/users` | GET | Yes | List company users |
| `/api/v1/my/companies` | GET | Yes | List current user's companies |

**Request/Response Examples:**

```json
// POST /api/v1/companies
{
  "name": "Acme Corp",
  "legalName": "Acme Corporation LLC",
  "taxId": "12-3456789"
}

// Response
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Acme Corp",
  "legalName": "Acme Corporation LLC",
  "taxId": "12-3456789",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "userRole": "Owner"
}

// POST /api/v1/companies/{companyId}/users
{
  "userId": "660e8400-e29b-41d4-a716-446655440001",
  "roleInCompany": "Manager"
}

// Response
{
  "userId": "660e8400-e29b-41d4-a716-446655440001",
  "userEmail": "manager@example.com",
  "userFullName": "Jane Manager",
  "companyId": "550e8400-e29b-41d4-a716-446655440000",
  "companyName": "Acme Corp",
  "roleInCompany": "Manager",
  "joinedAt": "2024-01-15T11:00:00Z"
}

// GET /api/v1/my/companies
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Acme Corp",
    "legalName": "Acme Corporation LLC",
    "taxId": "12-3456789",
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "userRole": "Owner"
  },
  {
    "id": "770e8400-e29b-41d4-a716-446655440002",
    "name": "Beta Inc",
    "legalName": null,
    "taxId": null,
    "isActive": true,
    "createdAt": "2024-01-10T08:00:00Z",
    "userRole": "Viewer"
  }
]
```

### Authentication/Authorization

- **Company Creation:** Admin only (for MVP)
- **Company Update:** Owner of company or Admin
- **Add/Remove Users:** Owner of company or Admin
- **View Company:** Any user linked to the company
- **List My Companies:** Any authenticated user

**Business Rules:**
- A user can be linked to multiple companies
- Each company must have at least one Owner
- Cannot remove the last Owner from a company
- Company names should be unique (soft constraint via validation)
- TaxId must be unique when provided

---

## Acceptance Criteria

- [x] ✅ DONE Authenticated users can list companies they are associated with via `GET /api/v1/my/companies`
- [x] ✅ DONE Admin (seeded) can create companies via `POST /api/v1/companies`
- [x] ✅ DONE Admin/Owner can add users to companies via `POST /api/v1/companies/{id}/users`
- [x] ✅ DONE Admin/Owner can remove users from companies via `DELETE /api/v1/companies/{id}/users/{userId}`
- [x] ✅ DONE All company queries are filtered by UserId or require appropriate role
- [x] ✅ DONE Migrations created and applied (`AddCompanies`)
- [x] ✅ DONE Seed data includes test company linked to admin user
- [x] ✅ DONE `dotnet build` passes without errors
- [x] ✅ DONE Swagger displays all new endpoints with proper documentation

---

## Edge Cases & Error Handling

- **Duplicate company name:** Return 400 with descriptive error
- **Invalid role string:** Return 400 with valid options
- **User not found when adding to company:** Return 400
- **Company not found:** Return 404
- **Removing last Owner:** Return 400 with explanation
- **User already in company:** Return 400 with explanation
- **Unauthorized access:** Return 403

---

## Future Considerations

- **Company context selection:** Frontend will use `GET /api/v1/my/companies` to let user select active company
- **CompanyId scoping:** All future business endpoints (employees, costs, etc.) will require `CompanyId` and validate user access
- **Role-based permissions within company:** Viewer can only read, Manager can edit, Owner can manage users
