# US004 – Provider Management

## Description

**As** an Admin user managing a company in Allocore,
**I need** to register and manage external providers (vendors, SaaS companies, infrastructure suppliers, consultancies),
**So that** I can track who supplies services to my organization, maintain contact information for each provider, categorize them, and have a foundation for attaching contracts, services, and costs later.

Currently, Allocore has no concept of a Provider. This story introduces the Provider entity (company-scoped), a ProviderContact child entity (multiple contacts per provider), and full CRUD operations. Providers are the central entity around which contracts, services, and costs revolve.

**Priority**: High
**Dependencies**: US003 – Company & UserCompany (Multi-Tenant Core)

---

## Step 1: Domain Layer — Provider & ProviderContact Entities

### 1.1 Create ProviderCategory enum

- [x] ✅ DONE Create `Allocore.Domain/Entities/Providers/ProviderCategory.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Providers;

  public enum ProviderCategory
  {
      SaaS = 0,
      Infrastructure = 1,
      Consultancy = 2,
      Benefits = 3,
      Licensing = 4,
      Telecommunications = 5,
      Hardware = 6,
      Other = 7
  }
  ```
  - **Business rule**: Every provider must have exactly one category.
  - **Note**: This is an extensible enum. Future stories may add categories, but the domain enforces a known set at compile time.

### 1.2 Create Provider entity

- [x] ✅ DONE Create `Allocore.Domain/Entities/Providers/Provider.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Providers;

  using Allocore.Domain.Common;

  public class Provider : Entity
  {
      public Guid CompanyId { get; private set; }
      public string Name { get; private set; } = string.Empty;
      public string? LegalName { get; private set; }
      public string? TaxId { get; private set; }
      public ProviderCategory Category { get; private set; }
      public string? Website { get; private set; }
      public string? Description { get; private set; }
      public bool IsActive { get; private set; } = true;

      private readonly List<ProviderContact> _contacts = new();
      public IReadOnlyCollection<ProviderContact> Contacts => _contacts.AsReadOnly();

      private Provider() { } // EF Core

      public static Provider Create(
          Guid companyId,
          string name,
          ProviderCategory category,
          string? legalName = null,
          string? taxId = null,
          string? website = null,
          string? description = null)
      {
          return new Provider
          {
              CompanyId = companyId,
              Name = name,
              Category = category,
              LegalName = legalName,
              TaxId = taxId,
              Website = website,
              Description = description,
              IsActive = true
          };
      }

      public void Update(
          string name,
          ProviderCategory category,
          string? legalName,
          string? taxId,
          string? website,
          string? description)
      {
          Name = name;
          Category = category;
          LegalName = legalName;
          TaxId = taxId;
          Website = website;
          Description = description;
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

      public void AddContact(ProviderContact contact)
      {
          _contacts.Add(contact);
          UpdatedAt = DateTime.UtcNow;
      }

      public void RemoveContact(ProviderContact contact)
      {
          _contacts.Remove(contact);
          UpdatedAt = DateTime.UtcNow;
      }
  }
  ```
  - **Business rule**: `CompanyId` is required and immutable after creation (tenant scoping).
  - **Business rule**: `Name` is required, max 200 chars.
  - **Business rule**: Provider names must be unique within a company (enforced at application layer).
  - **Note**: Follows the same pattern as `Company.cs` — private constructor, static `Create()`, mutation methods.

### 1.3 Create ProviderContact entity

- [x] ✅ DONE Create `Allocore.Domain/Entities/Providers/ProviderContact.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Providers;

  using Allocore.Domain.Common;

  public class ProviderContact : Entity
  {
      public Guid ProviderId { get; private set; }
      public string Name { get; private set; } = string.Empty;
      public string? Email { get; private set; }
      public string? Phone { get; private set; }
      public string? Role { get; private set; }
      public bool IsPrimary { get; private set; }

      // Navigation property
      public Provider? Provider { get; private set; }

      private ProviderContact() { } // EF Core

      public static ProviderContact Create(
          Guid providerId,
          string name,
          string? email = null,
          string? phone = null,
          string? role = null,
          bool isPrimary = false)
      {
          return new ProviderContact
          {
              ProviderId = providerId,
              Name = name,
              Email = email,
              Phone = phone,
              Role = role,
              IsPrimary = isPrimary
          };
      }

      public void Update(string name, string? email, string? phone, string? role, bool isPrimary)
      {
          Name = name;
          Email = email;
          Phone = phone;
          Role = role;
          IsPrimary = isPrimary;
          UpdatedAt = DateTime.UtcNow;
      }
  }
  ```
  - **Business rule**: `Name` is required, max 150 chars.
  - **Business rule**: `Role` is a free-text field (e.g., "Account Manager", "Technical Support", "Legal Contact") — max 100 chars.
  - **Business rule**: At most one contact per provider can be `IsPrimary = true`. Enforced at application layer.
  - **Note**: `ProviderContact` does NOT have its own `CompanyId` — it inherits tenant scoping through its parent `Provider`.

---

## Step 2: Infrastructure Layer — EF Core Configurations

### 2.1 Provider configuration

- [x] ✅ DONE Create `Allocore.Infrastructure/Persistence/Configurations/ProviderConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.Providers;

  public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
  {
      public void Configure(EntityTypeBuilder<Provider> builder)
      {
          builder.ToTable("Providers");

          builder.HasKey(p => p.Id);

          builder.Property(p => p.CompanyId)
              .IsRequired();

          builder.Property(p => p.Name)
              .IsRequired()
              .HasMaxLength(200);

          builder.Property(p => p.LegalName)
              .HasMaxLength(300);

          builder.Property(p => p.TaxId)
              .HasMaxLength(50);

          builder.Property(p => p.Category)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(p => p.Website)
              .HasMaxLength(500);

          builder.Property(p => p.Description)
              .HasMaxLength(2000);

          builder.Property(p => p.IsActive)
              .IsRequired()
              .HasDefaultValue(true);

          // Unique provider name within a company
          builder.HasIndex(p => new { p.CompanyId, p.Name })
              .IsUnique();

          builder.HasIndex(p => p.CompanyId);

          builder.HasIndex(p => p.Category);

          builder.HasMany(p => p.Contacts)
              .WithOne(c => c.Provider)
              .HasForeignKey(c => c.ProviderId)
              .OnDelete(DeleteBehavior.Cascade);
      }
  }
  ```
  - **Note**: `Category` stored as string for readability in DB. Follows same pattern as `RoleInCompany` in `UserCompanyConfiguration`.
  - **Note**: Unique index on `(CompanyId, Name)` enforces no duplicate provider names within a company.

### 2.2 ProviderContact configuration

- [x] ✅ DONE Create `Allocore.Infrastructure/Persistence/Configurations/ProviderContactConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.Providers;

  public class ProviderContactConfiguration : IEntityTypeConfiguration<ProviderContact>
  {
      public void Configure(EntityTypeBuilder<ProviderContact> builder)
      {
          builder.ToTable("ProviderContacts");

          builder.HasKey(c => c.Id);

          builder.Property(c => c.ProviderId)
              .IsRequired();

          builder.Property(c => c.Name)
              .IsRequired()
              .HasMaxLength(150);

          builder.Property(c => c.Email)
              .HasMaxLength(254);

          builder.Property(c => c.Phone)
              .HasMaxLength(30);

          builder.Property(c => c.Role)
              .HasMaxLength(100);

          builder.Property(c => c.IsPrimary)
              .IsRequired()
              .HasDefaultValue(false);

          builder.HasIndex(c => c.ProviderId);

          builder.HasOne(c => c.Provider)
              .WithMany(p => p.Contacts)
              .HasForeignKey(c => c.ProviderId)
              .OnDelete(DeleteBehavior.Cascade);
      }
  }
  ```

### 2.3 Update ApplicationDbContext

- [x] ✅ DONE Update `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs` — add DbSets:
  ```csharp
  // Add these using statements
  using Allocore.Domain.Entities.Providers;

  // Add these DbSets
  public DbSet<Provider> Providers => Set<Provider>();
  public DbSet<ProviderContact> ProviderContacts => Set<ProviderContact>();
  ```
  - **Note**: `OnModelCreating` already calls `ApplyConfigurationsFromAssembly` so the new configurations will be auto-discovered.

### 2.4 Create migration

- [x] ✅ DONE Run migration:
  ```bash
  dotnet ef migrations add AddProviders -s Allocore.API -p Allocore.Infrastructure
  ```
  - **Impact on existing data**: No existing rows affected. Two new tables created.

---

## Step 3: Infrastructure Layer — Repository

### 3.1 Create IProviderRepository interface

- [x] ✅ DONE Create `Allocore.Application/Abstractions/Persistence/IProviderRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;

  using Allocore.Domain.Entities.Providers;

  public interface IProviderRepository : IReadRepository<Provider>, IWriteRepository<Provider>
  {
      Task<Provider?> GetByIdWithContactsAsync(Guid id, CancellationToken cancellationToken = default);
      Task<bool> ExistsByNameInCompanyAsync(Guid companyId, string name, CancellationToken cancellationToken = default);
      Task<bool> ExistsByNameInCompanyExcludingAsync(Guid companyId, string name, Guid excludeProviderId, CancellationToken cancellationToken = default);
      Task<(IEnumerable<Provider> Providers, int TotalCount)> GetPagedByCompanyAsync(
          Guid companyId, int page, int pageSize,
          ProviderCategory? categoryFilter = null,
          bool? isActiveFilter = null,
          string? searchTerm = null,
          CancellationToken cancellationToken = default);
      Task<IEnumerable<Provider>> GetAllByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
  }
  ```
  - **Note**: Interface lives in Application layer (same pattern as `IUserRepository`, `ICompanyRepository`).
  - **Note**: `GetByIdWithContactsAsync` eagerly loads contacts — used for detail views.
  - **Note**: `GetPagedByCompanyAsync` supports filtering by category, active status, and search term (name/legalName).

### 3.2 Create ProviderRepository implementation

- [x] ✅ DONE Create `Allocore.Infrastructure/Persistence/Repositories/ProviderRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Repositories;

  using Microsoft.EntityFrameworkCore;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Entities.Providers;

  public class ProviderRepository : IProviderRepository
  {
      private readonly ApplicationDbContext _context;

      public ProviderRepository(ApplicationDbContext context)
      {
          _context = context;
      }

      public async Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.Providers.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

      public async Task<Provider?> GetByIdWithContactsAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.Providers
              .Include(p => p.Contacts)
              .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

      public async Task<bool> ExistsByNameInCompanyAsync(Guid companyId, string name, CancellationToken cancellationToken = default)
          => await _context.Providers.AnyAsync(
              p => p.CompanyId == companyId && p.Name == name,
              cancellationToken);

      public async Task<bool> ExistsByNameInCompanyExcludingAsync(Guid companyId, string name, Guid excludeProviderId, CancellationToken cancellationToken = default)
          => await _context.Providers.AnyAsync(
              p => p.CompanyId == companyId && p.Name == name && p.Id != excludeProviderId,
              cancellationToken);

      public async Task<(IEnumerable<Provider> Providers, int TotalCount)> GetPagedByCompanyAsync(
          Guid companyId, int page, int pageSize,
          ProviderCategory? categoryFilter = null,
          bool? isActiveFilter = null,
          string? searchTerm = null,
          CancellationToken cancellationToken = default)
      {
          var query = _context.Providers
              .Include(p => p.Contacts)
              .Where(p => p.CompanyId == companyId);

          if (categoryFilter.HasValue)
              query = query.Where(p => p.Category == categoryFilter.Value);

          if (isActiveFilter.HasValue)
              query = query.Where(p => p.IsActive == isActiveFilter.Value);

          if (!string.IsNullOrWhiteSpace(searchTerm))
          {
              var term = searchTerm.ToLowerInvariant();
              query = query.Where(p =>
                  p.Name.ToLower().Contains(term) ||
                  (p.LegalName != null && p.LegalName.ToLower().Contains(term)));
          }

          var totalCount = await query.CountAsync(cancellationToken);
          var providers = await query
              .OrderBy(p => p.Name)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync(cancellationToken);

          return (providers, totalCount);
      }

      public async Task<IEnumerable<Provider>> GetAllByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
          => await _context.Providers
              .Where(p => p.CompanyId == companyId)
              .OrderBy(p => p.Name)
              .ToListAsync(cancellationToken);

      public async Task<IEnumerable<Provider>> GetAllAsync(CancellationToken cancellationToken = default)
          => await _context.Providers.ToListAsync(cancellationToken);

      public async Task<Provider> AddAsync(Provider entity, CancellationToken cancellationToken = default)
      {
          await _context.Providers.AddAsync(entity, cancellationToken);
          return entity;
      }

      public Task UpdateAsync(Provider entity, CancellationToken cancellationToken = default)
      {
          _context.Providers.Update(entity);
          return Task.CompletedTask;
      }

      public Task DeleteAsync(Provider entity, CancellationToken cancellationToken = default)
      {
          _context.Providers.Remove(entity);
          return Task.CompletedTask;
      }
  }
  ```
  - **Note**: Follows exact same pattern as `CompanyRepository` and `UserRepository`.
  - **Note**: All queries that return providers for display include `.Include(p => p.Contacts)` to avoid N+1.

### 3.3 Register in DI

- [x] ✅ DONE Update `Allocore.Infrastructure/DependencyInjection.cs`:
  ```csharp
  // Add registration
  services.AddScoped<IProviderRepository, ProviderRepository>();
  ```

---

## Step 4: Application Layer — DTOs

### 4.1 Create Provider DTOs

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/DTOs/ProviderDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.DTOs;

  public record ProviderDto(
      Guid Id,
      Guid CompanyId,
      string Name,
      string? LegalName,
      string? TaxId,
      string Category,
      string? Website,
      string? Description,
      bool IsActive,
      DateTime CreatedAt,
      DateTime? UpdatedAt,
      IEnumerable<ProviderContactDto> Contacts
  );
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/DTOs/ProviderContactDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.DTOs;

  public record ProviderContactDto(
      Guid Id,
      string Name,
      string? Email,
      string? Phone,
      string? Role,
      bool IsPrimary
  );
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/DTOs/ProviderListItemDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.DTOs;

  public record ProviderListItemDto(
      Guid Id,
      string Name,
      string Category,
      string? Website,
      bool IsActive,
      int ContactCount,
      string? PrimaryContactName
  );
  ```
  - **Note**: Lightweight DTO for list views. Avoids sending full contact details in paginated lists.

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/DTOs/CreateProviderRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.DTOs;

  public record CreateProviderRequest(
      string Name,
      string Category,
      string? LegalName,
      string? TaxId,
      string? Website,
      string? Description,
      IEnumerable<CreateProviderContactRequest>? Contacts
  );
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/DTOs/CreateProviderContactRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.DTOs;

  public record CreateProviderContactRequest(
      string Name,
      string? Email,
      string? Phone,
      string? Role,
      bool IsPrimary
  );
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/DTOs/UpdateProviderRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.DTOs;

  public record UpdateProviderRequest(
      string Name,
      string Category,
      string? LegalName,
      string? TaxId,
      string? Website,
      string? Description
  );
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/DTOs/AddProviderContactRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.DTOs;

  public record AddProviderContactRequest(
      string Name,
      string? Email,
      string? Phone,
      string? Role,
      bool IsPrimary
  );
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/DTOs/UpdateProviderContactRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.DTOs;

  public record UpdateProviderContactRequest(
      string Name,
      string? Email,
      string? Phone,
      string? Role,
      bool IsPrimary
  );
  ```

---

## Step 5: Application Layer — Validators

### 5.1 Create validators

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Validators/CreateProviderRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Providers.DTOs;
  using Allocore.Domain.Entities.Providers;

  public class CreateProviderRequestValidator : AbstractValidator<CreateProviderRequest>
  {
      public CreateProviderRequestValidator()
      {
          RuleFor(x => x.Name)
              .NotEmpty().WithMessage("Provider name is required")
              .MaximumLength(200).WithMessage("Provider name must not exceed 200 characters");

          RuleFor(x => x.Category)
              .NotEmpty().WithMessage("Category is required")
              .Must(c => Enum.TryParse<ProviderCategory>(c, true, out _))
              .WithMessage("Category must be one of: SaaS, Infrastructure, Consultancy, Benefits, Licensing, Telecommunications, Hardware, Other");

          RuleFor(x => x.LegalName)
              .MaximumLength(300).WithMessage("Legal name must not exceed 300 characters")
              .When(x => !string.IsNullOrEmpty(x.LegalName));

          RuleFor(x => x.TaxId)
              .MaximumLength(50).WithMessage("Tax ID must not exceed 50 characters")
              .When(x => !string.IsNullOrEmpty(x.TaxId));

          RuleFor(x => x.Website)
              .MaximumLength(500).WithMessage("Website must not exceed 500 characters")
              .When(x => !string.IsNullOrEmpty(x.Website));

          RuleFor(x => x.Description)
              .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
              .When(x => !string.IsNullOrEmpty(x.Description));

          RuleFor(x => x.Contacts)
              .Must(contacts => contacts == null || contacts.Count(c => c.IsPrimary) <= 1)
              .WithMessage("Only one contact can be marked as primary")
              .When(x => x.Contacts != null && x.Contacts.Any());

          RuleForEach(x => x.Contacts)
              .SetValidator(new CreateProviderContactRequestValidator())
              .When(x => x.Contacts != null);
      }
  }
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Validators/CreateProviderContactRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Providers.DTOs;

  public class CreateProviderContactRequestValidator : AbstractValidator<CreateProviderContactRequest>
  {
      public CreateProviderContactRequestValidator()
      {
          RuleFor(x => x.Name)
              .NotEmpty().WithMessage("Contact name is required")
              .MaximumLength(150).WithMessage("Contact name must not exceed 150 characters");

          RuleFor(x => x.Email)
              .MaximumLength(254).WithMessage("Email must not exceed 254 characters")
              .EmailAddress().WithMessage("Email must be a valid email address")
              .When(x => !string.IsNullOrEmpty(x.Email));

          RuleFor(x => x.Phone)
              .MaximumLength(30).WithMessage("Phone must not exceed 30 characters")
              .When(x => !string.IsNullOrEmpty(x.Phone));

          RuleFor(x => x.Role)
              .MaximumLength(100).WithMessage("Role must not exceed 100 characters")
              .When(x => !string.IsNullOrEmpty(x.Role));
      }
  }
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Validators/UpdateProviderRequestValidator.cs`:
  - Same rules as `CreateProviderRequestValidator` but without the `Contacts` collection rules.

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Validators/AddProviderContactRequestValidator.cs`:
  - Same rules as `CreateProviderContactRequestValidator`.

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Validators/UpdateProviderContactRequestValidator.cs`:
  - Same rules as `CreateProviderContactRequestValidator`.

---

## Step 6: Application Layer — CQRS Commands

### 6.1 CreateProvider command

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Commands/CreateProviderCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.Commands;

  using MediatR;
  using Allocore.Application.Features.Providers.DTOs;
  using Allocore.Domain.Common;

  public record CreateProviderCommand(Guid CompanyId, CreateProviderRequest Request) : IRequest<Result<ProviderDto>>;
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Commands/CreateProviderCommandHandler.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.Commands;

  using MediatR;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Application.Abstractions.Services;
  using Allocore.Application.Features.Providers.DTOs;
  using Allocore.Domain.Common;
  using Allocore.Domain.Entities.Providers;

  public class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, Result<ProviderDto>>
  {
      private readonly IProviderRepository _providerRepository;
      private readonly IUserCompanyRepository _userCompanyRepository;
      private readonly ICurrentUser _currentUser;
      private readonly IUnitOfWork _unitOfWork;

      public CreateProviderCommandHandler(
          IProviderRepository providerRepository,
          IUserCompanyRepository userCompanyRepository,
          ICurrentUser currentUser,
          IUnitOfWork unitOfWork)
      {
          _providerRepository = providerRepository;
          _userCompanyRepository = userCompanyRepository;
          _currentUser = currentUser;
          _unitOfWork = unitOfWork;
      }

      public async Task<Result<ProviderDto>> Handle(CreateProviderCommand command, CancellationToken cancellationToken)
      {
          // Verify user has access to company
          if (!_currentUser.UserId.HasValue)
              return Result<ProviderDto>.Failure("User not authenticated.");

          var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
              _currentUser.UserId.Value, command.CompanyId, cancellationToken);
          if (!hasAccess)
              return Result<ProviderDto>.Failure("You don't have access to this company.");

          // Check for duplicate name within company
          if (await _providerRepository.ExistsByNameInCompanyAsync(command.CompanyId, command.Request.Name, cancellationToken))
              return Result<ProviderDto>.Failure("A provider with this name already exists in this company.");

          // Parse category
          if (!Enum.TryParse<ProviderCategory>(command.Request.Category, true, out var category))
              return Result<ProviderDto>.Failure("Invalid provider category.");

          // Create provider
          var provider = Provider.Create(
              command.CompanyId,
              command.Request.Name,
              category,
              command.Request.LegalName,
              command.Request.TaxId,
              command.Request.Website,
              command.Request.Description);

          // Add contacts if provided
          if (command.Request.Contacts != null)
          {
              foreach (var contactReq in command.Request.Contacts)
              {
                  var contact = ProviderContact.Create(
                      provider.Id,
                      contactReq.Name,
                      contactReq.Email,
                      contactReq.Phone,
                      contactReq.Role,
                      contactReq.IsPrimary);
                  provider.AddContact(contact);
              }
          }

          await _providerRepository.AddAsync(provider, cancellationToken);
          await _unitOfWork.SaveChangesAsync(cancellationToken);

          return Result<ProviderDto>.Success(MapToDto(provider));
      }

      private static ProviderDto MapToDto(Provider provider) => new(
          provider.Id,
          provider.CompanyId,
          provider.Name,
          provider.LegalName,
          provider.TaxId,
          provider.Category.ToString(),
          provider.Website,
          provider.Description,
          provider.IsActive,
          provider.CreatedAt,
          provider.UpdatedAt,
          provider.Contacts.Select(c => new ProviderContactDto(
              c.Id, c.Name, c.Email, c.Phone, c.Role, c.IsPrimary))
      );
  }
  ```
  - **Business rule**: User must have access to the company (any role: Viewer, Manager, Owner). Creating providers requires at minimum Manager role — enforce via `UserHasAccessToCompanyAsync` for now; role-level granularity deferred.
  - **Note**: Follows same authorization pattern as `CreateCompanyCommandHandler` — check user access, then proceed.

### 6.2 UpdateProvider command

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Commands/UpdateProviderCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.Commands;

  using MediatR;
  using Allocore.Application.Features.Providers.DTOs;
  using Allocore.Domain.Common;

  public record UpdateProviderCommand(Guid CompanyId, Guid ProviderId, UpdateProviderRequest Request) : IRequest<Result<ProviderDto>>;
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Commands/UpdateProviderCommandHandler.cs`:
  - Verify user has access to company
  - Load provider by ID, verify it belongs to the company (`provider.CompanyId == command.CompanyId`)
  - Check for duplicate name within company (excluding current provider via `ExistsByNameInCompanyExcludingAsync`)
  - Parse category enum
  - Call `provider.Update(...)`, save changes
  - Return updated `ProviderDto`

### 6.3 DeactivateProvider command

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Commands/DeactivateProviderCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.Commands;

  using MediatR;
  using Allocore.Domain.Common;

  public record DeactivateProviderCommand(Guid CompanyId, Guid ProviderId) : IRequest<Result>;
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Commands/DeactivateProviderCommandHandler.cs`:
  - Verify user access to company
  - Load provider, verify it belongs to company
  - Call `provider.Deactivate()`, save changes
  - Return `Result.Success()`

### 6.4 AddProviderContact command

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Commands/AddProviderContactCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.Commands;

  using MediatR;
  using Allocore.Application.Features.Providers.DTOs;
  using Allocore.Domain.Common;

  public record AddProviderContactCommand(Guid CompanyId, Guid ProviderId, AddProviderContactRequest Request) : IRequest<Result<ProviderContactDto>>;
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Commands/AddProviderContactCommandHandler.cs`:
  - Verify user access to company
  - Load provider with contacts (`GetByIdWithContactsAsync`), verify it belongs to company
  - If `IsPrimary = true`, unset any existing primary contact on this provider
  - Create `ProviderContact`, add to provider, save changes
  - Return `ProviderContactDto`

### 6.5 UpdateProviderContact command

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Commands/UpdateProviderContactCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.Commands;

  using MediatR;
  using Allocore.Application.Features.Providers.DTOs;
  using Allocore.Domain.Common;

  public record UpdateProviderContactCommand(Guid CompanyId, Guid ProviderId, Guid ContactId, UpdateProviderContactRequest Request) : IRequest<Result<ProviderContactDto>>;
  ```

- [x] ✅ DONE Create handler — same pattern: verify access, load provider with contacts, find contact, update, handle primary flag, save.

### 6.6 RemoveProviderContact command

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Commands/RemoveProviderContactCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.Commands;

  using MediatR;
  using Allocore.Domain.Common;

  public record RemoveProviderContactCommand(Guid CompanyId, Guid ProviderId, Guid ContactId) : IRequest<Result>;
  ```

- [x] ✅ DONE Create handler — verify access, load provider with contacts, find contact, remove, save.

---

## Step 7: Application Layer — CQRS Queries

### 7.1 GetProviderById query

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Queries/GetProviderByIdQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.Queries;

  using MediatR;
  using Allocore.Application.Features.Providers.DTOs;
  using Allocore.Domain.Common;

  public record GetProviderByIdQuery(Guid CompanyId, Guid ProviderId) : IRequest<Result<ProviderDto>>;
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Queries/GetProviderByIdQueryHandler.cs`:
  - Verify user access to company
  - Load provider with contacts, verify it belongs to company
  - Return `ProviderDto`

### 7.2 GetProvidersPaged query

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Queries/GetProvidersPagedQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Providers.Queries;

  using MediatR;
  using Allocore.Application.Features.Providers.DTOs;

  public record GetProvidersPagedQuery(
      Guid CompanyId,
      int Page = 1,
      int PageSize = 10,
      string? Category = null,
      bool? IsActive = null,
      string? SearchTerm = null
  ) : IRequest<PagedResult<ProviderListItemDto>>;
  ```
  - **Note**: `PagedResult<T>` — if this doesn't exist yet, create it as a generic wrapper:

- [x] ✅ DONE Create `Allocore.Application/Common/PagedResult.cs`:
  ```csharp
  namespace Allocore.Application.Common;

  public record PagedResult<T>(
      IEnumerable<T> Items,
      int Page,
      int PageSize,
      int TotalCount,
      int TotalPages
  )
  {
      public bool HasPreviousPage => Page > 1;
      public bool HasNextPage => Page < TotalPages;
  }
  ```

- [x] ✅ DONE Create `Allocore.Application/Features/Providers/Queries/GetProvidersPagedQueryHandler.cs`:
  - Verify user access to company
  - Parse category filter if provided
  - Call `_providerRepository.GetPagedByCompanyAsync(...)` with filters
  - Map to `ProviderListItemDto` (lightweight: name, category, website, isActive, contactCount, primaryContactName)
  - Return `PagedResult<ProviderListItemDto>`

---

## Step 8: API Layer — ProvidersController

- [x] ✅ DONE Create `Allocore.API/Controllers/v1/ProvidersController.cs`:
  ```csharp
  namespace Allocore.API.Controllers.v1;

  using Asp.Versioning;
  using MediatR;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Allocore.Application.Features.Providers.Commands;
  using Allocore.Application.Features.Providers.DTOs;
  using Allocore.Application.Features.Providers.Queries;

  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/companies/{companyId:guid}/providers")]
  [Authorize]
  public class ProvidersController : ControllerBase
  {
      private readonly IMediator _mediator;

      public ProvidersController(IMediator mediator)
      {
          _mediator = mediator;
      }

      /// <summary>
      /// List providers for a company (paginated, filterable).
      /// </summary>
      [HttpGet]
      public async Task<IActionResult> GetProviders(
          Guid companyId,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 10,
          [FromQuery] string? category = null,
          [FromQuery] bool? isActive = null,
          [FromQuery] string? search = null,
          CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(
              new GetProvidersPagedQuery(companyId, page, pageSize, category, isActive, search),
              cancellationToken);
          return Ok(result);
      }

      /// <summary>
      /// Get a provider by ID with full details and contacts.
      /// </summary>
      [HttpGet("{providerId:guid}")]
      public async Task<IActionResult> GetProvider(Guid companyId, Guid providerId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new GetProviderByIdQuery(companyId, providerId), cancellationToken);
          if (!result.IsSuccess)
              return NotFound(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Create a new provider (optionally with contacts).
      /// </summary>
      [HttpPost]
      public async Task<IActionResult> CreateProvider(
          Guid companyId,
          [FromBody] CreateProviderRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new CreateProviderCommand(companyId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return CreatedAtAction(nameof(GetProvider), new { companyId, providerId = result.Value!.Id }, result.Value);
      }

      /// <summary>
      /// Update a provider's details (not contacts).
      /// </summary>
      [HttpPut("{providerId:guid}")]
      public async Task<IActionResult> UpdateProvider(
          Guid companyId,
          Guid providerId,
          [FromBody] UpdateProviderRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new UpdateProviderCommand(companyId, providerId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Deactivate a provider (soft delete).
      /// </summary>
      [HttpPatch("{providerId:guid}/deactivate")]
      public async Task<IActionResult> DeactivateProvider(Guid companyId, Guid providerId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new DeactivateProviderCommand(companyId, providerId), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }

      /// <summary>
      /// Add a contact to a provider.
      /// </summary>
      [HttpPost("{providerId:guid}/contacts")]
      public async Task<IActionResult> AddContact(
          Guid companyId,
          Guid providerId,
          [FromBody] AddProviderContactRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new AddProviderContactCommand(companyId, providerId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Update a provider contact.
      /// </summary>
      [HttpPut("{providerId:guid}/contacts/{contactId:guid}")]
      public async Task<IActionResult> UpdateContact(
          Guid companyId,
          Guid providerId,
          Guid contactId,
          [FromBody] UpdateProviderContactRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new UpdateProviderContactCommand(companyId, providerId, contactId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Remove a contact from a provider.
      /// </summary>
      [HttpDelete("{providerId:guid}/contacts/{contactId:guid}")]
      public async Task<IActionResult> RemoveContact(
          Guid companyId,
          Guid providerId,
          Guid contactId,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new RemoveProviderContactCommand(companyId, providerId, contactId), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }
  }
  ```
  - **Note**: Route is nested under company: `/api/v1/companies/{companyId}/providers`. This enforces company context in every request.
  - **Note**: Follows same controller pattern as `CompaniesController` — inject `IMediator`, use `Send()`, return appropriate status codes.

---

## Step 9: Build, Verify & Manual Test

- [x] ✅ DONE Run `dotnet build` — ensure entire solution compiles
- [x] ✅ DONE Apply migration: `dotnet ef database update -s Allocore.API -p Allocore.Infrastructure`
- [ ] Run application and verify Swagger shows all new endpoints
- [ ] Manual test via Swagger:
  1. Create a provider with contacts → 201
  2. Get provider by ID → 200 with contacts
  3. List providers (paginated) → 200
  4. Update provider → 200
  5. Add contact → 200
  6. Update contact → 200
  7. Remove contact → 204
  8. Deactivate provider → 204
  9. Verify duplicate name returns 400
  10. Verify wrong company returns error

---

## Technical Details

### Dependencies

No new NuGet packages required beyond US003.

### Project Structure — Affected Files

| Layer | File | Change |
|-------|------|--------|
| **Domain** | `Allocore.Domain/Entities/Providers/ProviderCategory.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Providers/Provider.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Providers/ProviderContact.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Configurations/ProviderConfiguration.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Configurations/ProviderContactConfiguration.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs` | **Update** — add DbSets |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Repositories/ProviderRepository.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/DependencyInjection.cs` | **Update** — add registration |
| **Application** | `Allocore.Application/Abstractions/Persistence/IProviderRepository.cs` | **Create** |
| **Application** | `Allocore.Application/Common/PagedResult.cs` | **Create** (if not present) |
| **Application** | `Allocore.Application/Features/Providers/DTOs/*.cs` | **Create** (8 files) |
| **Application** | `Allocore.Application/Features/Providers/Validators/*.cs` | **Create** (5 files) |
| **Application** | `Allocore.Application/Features/Providers/Commands/*.cs` | **Create** (12 files — 6 commands + 6 handlers) |
| **Application** | `Allocore.Application/Features/Providers/Queries/*.cs` | **Create** (4 files — 2 queries + 2 handlers) |
| **API** | `Allocore.API/Controllers/v1/ProvidersController.cs` | **Create** |

### Database

**Table: Providers**

| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| Id | uuid | NO | gen | PK |
| CompanyId | uuid | NO | — | FK → Companies |
| Name | varchar(200) | NO | — | |
| LegalName | varchar(300) | YES | — | |
| TaxId | varchar(50) | YES | — | |
| Category | varchar(50) | NO | — | |
| Website | varchar(500) | YES | — | |
| Description | varchar(2000) | YES | — | |
| IsActive | boolean | NO | true | |
| CreatedAt | timestamp | NO | — | |
| UpdatedAt | timestamp | YES | — | |

**Indexes:**
- `IX_Providers_CompanyId_Name` (UNIQUE)
- `IX_Providers_CompanyId`
- `IX_Providers_Category`

**Table: ProviderContacts**

| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| Id | uuid | NO | gen | PK |
| ProviderId | uuid | NO | — | FK → Providers (CASCADE) |
| Name | varchar(150) | NO | — | |
| Email | varchar(254) | YES | — | |
| Phone | varchar(30) | YES | — | |
| Role | varchar(100) | YES | — | |
| IsPrimary | boolean | NO | false | |
| CreatedAt | timestamp | NO | — | |
| UpdatedAt | timestamp | YES | — | |

**Indexes:**
- `IX_ProviderContacts_ProviderId`

### API Contract

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/v1/companies/{companyId}/providers` | GET | Yes | List providers (paginated, filterable) |
| `/api/v1/companies/{companyId}/providers/{providerId}` | GET | Yes | Get provider with contacts |
| `/api/v1/companies/{companyId}/providers` | POST | Yes | Create provider (with optional contacts) |
| `/api/v1/companies/{companyId}/providers/{providerId}` | PUT | Yes | Update provider details |
| `/api/v1/companies/{companyId}/providers/{providerId}/deactivate` | PATCH | Yes | Soft-delete provider |
| `/api/v1/companies/{companyId}/providers/{providerId}/contacts` | POST | Yes | Add contact |
| `/api/v1/companies/{companyId}/providers/{providerId}/contacts/{contactId}` | PUT | Yes | Update contact |
| `/api/v1/companies/{companyId}/providers/{providerId}/contacts/{contactId}` | DELETE | Yes | Remove contact |

### Authentication/Authorization

- All endpoints require JWT Bearer authentication
- All endpoints verify user has access to the specified company via `UserCompany` relationship
- Fine-grained role checks (Viewer = read-only, Manager = write, Owner = full) deferred to a future authorization story

---

## Acceptance Criteria

- [ ] Authenticated users can create providers within their companies
- [ ] Providers are company-scoped — no cross-tenant data leakage
- [ ] Provider names are unique within a company
- [ ] Providers support multiple contacts with one optional primary contact
- [ ] Providers can be listed with pagination, category filter, active filter, and search
- [ ] Providers can be soft-deleted (deactivated) rather than hard-deleted
- [ ] All CRUD operations for provider contacts work correctly
- [ ] Migrations created and applied (`AddProviders`)
- [ ] `dotnet build` passes without errors
- [ ] Swagger displays all new endpoints

---

## What is explicitly NOT changing?

- **Authentication/Authorization model** — no new roles or policies added
- **Company entity** — no changes to Company or UserCompany
- **User entity** — no changes
- **Existing endpoints** — no modifications to Ping, Auth, or Companies controllers
- **Services/Costs/Allocations** — not part of this story

---

## Follow-ups (Intentionally Deferred)

| Item | Reason | Related Story |
|------|--------|---------------|
| Role-based write permissions (Viewer vs Manager vs Owner) | Requires authorization policy infrastructure | Future: Authorization Policies |
| Provider contracts (start/end dates, renewal, terms) | Separate domain concept, depends on US004 | US005 |
| Notes/timeline on providers | Polymorphic notes system, depends on US004 | US006 |
| Provider services catalog | Depends on Provider + Contract foundation | Future US |
| Seed data for test providers | Nice-to-have, not blocking | Optional |
