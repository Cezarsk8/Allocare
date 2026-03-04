# US005 – Provider Contracts

## Description

**As** an Admin user managing providers in Allocore,
**I need** to register and manage contracts with providers — including contract terms, start/end/renewal dates, price conditions, associated services, and legal team contacts,
**So that** I can track the lifecycle of every vendor relationship, know when contracts expire or need renewal, understand what services each contract covers, and have all negotiation-relevant information in one place.

Currently, Allocore has providers (US004) but no way to track the contractual relationship between a company and a provider. This story introduces the `Contract` entity (company-scoped, linked to a provider), a `ContractService` join entity (which services are covered by a contract), and a `ContractStatus` lifecycle. Contracts are the second pillar of the provider management domain, sitting between providers and costs.

**Priority**: High
**Dependencies**: US004 – Provider Management

---

## Step 1: Domain Layer — Contract Entities & Enums

### 1.1 Create ContractStatus enum

- [ ] Create `Allocore.Domain/Entities/Contracts/ContractStatus.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Contracts;

  public enum ContractStatus
  {
      Draft = 0,
      InNegotiation = 1,
      Active = 2,
      Expiring = 3,
      Expired = 4,
      Renewed = 5,
      Cancelled = 6,
      Terminated = 7
  }
  ```
  - **Business rule**: Contracts start as `Draft` or `InNegotiation`. They transition to `Active` when signed. `Expiring` is a derived/manual flag when renewal date approaches. `Renewed` means a successor contract exists.
  - **Note**: Status transitions are NOT enforced as a state machine in this story — they are set explicitly by the user. A future story may add transition rules.

### 1.2 Create BillingFrequency enum

- [ ] Create `Allocore.Domain/Entities/Contracts/BillingFrequency.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Contracts;

  public enum BillingFrequency
  {
      Monthly = 0,
      Quarterly = 1,
      SemiAnnual = 2,
      Annual = 3,
      OneOff = 4,
      Custom = 5
  }
  ```

### 1.3 Create Contract entity

- [ ] Create `Allocore.Domain/Entities/Contracts/Contract.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Contracts;

  using Allocore.Domain.Common;

  public class Contract : Entity
  {
      public Guid CompanyId { get; private set; }
      public Guid ProviderId { get; private set; }
      public string Title { get; private set; } = string.Empty;
      public string? ContractNumber { get; private set; }
      public ContractStatus Status { get; private set; } = ContractStatus.Draft;
      public DateTime? StartDate { get; private set; }
      public DateTime? EndDate { get; private set; }
      public DateTime? RenewalDate { get; private set; }
      public bool AutoRenew { get; private set; }
      public int? RenewalNoticeDays { get; private set; }
      public BillingFrequency BillingFrequency { get; private set; } = BillingFrequency.Monthly;
      public decimal? TotalValue { get; private set; }
      public string? Currency { get; private set; }
      public string? PaymentTerms { get; private set; }
      public string? PriceConditions { get; private set; }
      public string? LegalTeamContact { get; private set; }
      public string? InternalOwner { get; private set; }
      public string? Description { get; private set; }
      public string? TermsAndConditions { get; private set; }

      // Navigation properties
      public Provider? Provider { get; private set; }

      private readonly List<ContractService> _contractServices = new();
      public IReadOnlyCollection<ContractService> ContractServices => _contractServices.AsReadOnly();

      private Contract() { } // EF Core

      public static Contract Create(
          Guid companyId,
          Guid providerId,
          string title,
          string? contractNumber = null,
          ContractStatus status = ContractStatus.Draft,
          DateTime? startDate = null,
          DateTime? endDate = null,
          DateTime? renewalDate = null,
          bool autoRenew = false,
          int? renewalNoticeDays = null,
          BillingFrequency billingFrequency = BillingFrequency.Monthly,
          decimal? totalValue = null,
          string? currency = null,
          string? paymentTerms = null,
          string? priceConditions = null,
          string? legalTeamContact = null,
          string? internalOwner = null,
          string? description = null,
          string? termsAndConditions = null)
      {
          return new Contract
          {
              CompanyId = companyId,
              ProviderId = providerId,
              Title = title,
              ContractNumber = contractNumber,
              Status = status,
              StartDate = startDate,
              EndDate = endDate,
              RenewalDate = renewalDate,
              AutoRenew = autoRenew,
              RenewalNoticeDays = renewalNoticeDays,
              BillingFrequency = billingFrequency,
              TotalValue = totalValue,
              Currency = currency,
              PaymentTerms = paymentTerms,
              PriceConditions = priceConditions,
              LegalTeamContact = legalTeamContact,
              InternalOwner = internalOwner,
              Description = description,
              TermsAndConditions = termsAndConditions
          };
      }

      public void Update(
          string title,
          string? contractNumber,
          ContractStatus status,
          DateTime? startDate,
          DateTime? endDate,
          DateTime? renewalDate,
          bool autoRenew,
          int? renewalNoticeDays,
          BillingFrequency billingFrequency,
          decimal? totalValue,
          string? currency,
          string? paymentTerms,
          string? priceConditions,
          string? legalTeamContact,
          string? internalOwner,
          string? description,
          string? termsAndConditions)
      {
          Title = title;
          ContractNumber = contractNumber;
          Status = status;
          StartDate = startDate;
          EndDate = endDate;
          RenewalDate = renewalDate;
          AutoRenew = autoRenew;
          RenewalNoticeDays = renewalNoticeDays;
          BillingFrequency = billingFrequency;
          TotalValue = totalValue;
          Currency = currency;
          PaymentTerms = paymentTerms;
          PriceConditions = priceConditions;
          LegalTeamContact = legalTeamContact;
          InternalOwner = internalOwner;
          Description = description;
          TermsAndConditions = termsAndConditions;
          UpdatedAt = DateTime.UtcNow;
      }

      public void UpdateStatus(ContractStatus newStatus)
      {
          Status = newStatus;
          UpdatedAt = DateTime.UtcNow;
      }

      public void AddService(ContractService contractService)
      {
          _contractServices.Add(contractService);
          UpdatedAt = DateTime.UtcNow;
      }

      public void RemoveService(ContractService contractService)
      {
          _contractServices.Remove(contractService);
          UpdatedAt = DateTime.UtcNow;
      }

      // Computed properties
      public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.UtcNow && Status != ContractStatus.Renewed;
      public bool IsExpiringSoon(int withinDays = 30) => EndDate.HasValue && EndDate.Value <= DateTime.UtcNow.AddDays(withinDays) && EndDate.Value >= DateTime.UtcNow;
      public bool NeedsRenewalAttention(int withinDays = 30) => RenewalDate.HasValue && RenewalDate.Value <= DateTime.UtcNow.AddDays(withinDays) && RenewalDate.Value >= DateTime.UtcNow;
  }
  ```
  - **Business rule**: `CompanyId` and `ProviderId` are required and immutable after creation.
  - **Business rule**: `Title` is required, max 300 chars.
  - **Business rule**: `TotalValue` uses `decimal` for financial precision. If provided, `Currency` should also be provided.
  - **Business rule**: `EndDate` must be after `StartDate` when both are provided (enforced at validator level).
  - **Business rule**: `RenewalDate` is when the renewal decision must be made — typically before `EndDate`.
  - **Business rule**: `RenewalNoticeDays` is how many days before `RenewalDate` the team should be notified.
  - **Note**: `LegalTeamContact` and `InternalOwner` are free-text fields for now. A future story may link these to `User` or `ProviderContact` entities.
  - **Note**: `PriceConditions` is a free-text field for describing pricing terms (e.g., "per-seat pricing at $15/user/month with volume discounts above 100 seats").
  - **Note**: Navigation to `Provider` is included. The `using` statement for `Allocore.Domain.Entities.Providers` is needed.

### 1.4 Create ContractService join entity

- [ ] Create `Allocore.Domain/Entities/Contracts/ContractService.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Contracts;

  using Allocore.Domain.Common;

  public class ContractService : Entity
  {
      public Guid ContractId { get; private set; }
      public string ServiceName { get; private set; } = string.Empty;
      public string? ServiceDescription { get; private set; }
      public decimal? UnitPrice { get; private set; }
      public string? UnitType { get; private set; }
      public int? Quantity { get; private set; }
      public string? Notes { get; private set; }

      // Navigation property
      public Contract? Contract { get; private set; }

      private ContractService() { } // EF Core

      public static ContractService Create(
          Guid contractId,
          string serviceName,
          string? serviceDescription = null,
          decimal? unitPrice = null,
          string? unitType = null,
          int? quantity = null,
          string? notes = null)
      {
          return new ContractService
          {
              ContractId = contractId,
              ServiceName = serviceName,
              ServiceDescription = serviceDescription,
              UnitPrice = unitPrice,
              UnitType = unitType,
              Quantity = quantity,
              Notes = notes
          };
      }

      public void Update(
          string serviceName,
          string? serviceDescription,
          decimal? unitPrice,
          string? unitType,
          int? quantity,
          string? notes)
      {
          ServiceName = serviceName;
          ServiceDescription = serviceDescription;
          UnitPrice = unitPrice;
          UnitType = unitType;
          Quantity = quantity;
          Notes = notes;
          UpdatedAt = DateTime.UtcNow;
      }
  }
  ```
  - **Business rule**: `ServiceName` is required, max 200 chars.
  - **Business rule**: `UnitType` is free-text (e.g., "Seat", "GB", "Hour", "License", "Instance") — max 50 chars.
  - **Business rule**: `UnitPrice` uses `decimal` for financial precision.
  - **Note**: This is NOT the same as the future `Service` entity from the Roadmap. `ContractService` is a line item within a contract describing what is covered. The full `Service` entity (which links to costs and allocations) will be introduced in a later story and may reference `ContractService` records.

---

## Step 2: Infrastructure Layer — EF Core Configurations

### 2.1 Contract configuration

- [ ] Create `Allocore.Infrastructure/Persistence/Configurations/ContractConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.Contracts;

  public class ContractConfiguration : IEntityTypeConfiguration<Contract>
  {
      public void Configure(EntityTypeBuilder<Contract> builder)
      {
          builder.ToTable("Contracts");

          builder.HasKey(c => c.Id);

          builder.Property(c => c.CompanyId)
              .IsRequired();

          builder.Property(c => c.ProviderId)
              .IsRequired();

          builder.Property(c => c.Title)
              .IsRequired()
              .HasMaxLength(300);

          builder.Property(c => c.ContractNumber)
              .HasMaxLength(100);

          builder.Property(c => c.Status)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(c => c.BillingFrequency)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(c => c.TotalValue)
              .HasPrecision(18, 2);

          builder.Property(c => c.Currency)
              .HasMaxLength(3);

          builder.Property(c => c.PaymentTerms)
              .HasMaxLength(500);

          builder.Property(c => c.PriceConditions)
              .HasMaxLength(2000);

          builder.Property(c => c.LegalTeamContact)
              .HasMaxLength(300);

          builder.Property(c => c.InternalOwner)
              .HasMaxLength(200);

          builder.Property(c => c.Description)
              .HasMaxLength(2000);

          builder.Property(c => c.TermsAndConditions)
              .HasColumnType("text");

          builder.HasIndex(c => c.CompanyId);
          builder.HasIndex(c => c.ProviderId);
          builder.HasIndex(c => c.Status);
          builder.HasIndex(c => new { c.CompanyId, c.ProviderId });
          builder.HasIndex(c => c.EndDate);
          builder.HasIndex(c => c.RenewalDate);

          builder.HasIndex(c => new { c.CompanyId, c.ContractNumber })
              .IsUnique()
              .HasFilter("\"ContractNumber\" IS NOT NULL");

          builder.HasOne(c => c.Provider)
              .WithMany()
              .HasForeignKey(c => c.ProviderId)
              .OnDelete(DeleteBehavior.Restrict);

          builder.HasMany(c => c.ContractServices)
              .WithOne(cs => cs.Contract)
              .HasForeignKey(cs => cs.ContractId)
              .OnDelete(DeleteBehavior.Cascade);
      }
  }
  ```
  - **Note**: `TotalValue` uses `precision(18, 2)` — standard for monetary amounts.
  - **Note**: `Currency` is ISO 4217 (3 chars, e.g., "USD", "BRL", "EUR").
  - **Note**: `TermsAndConditions` uses `text` column type — no length limit for legal text.
  - **Note**: FK to Provider uses `DeleteBehavior.Restrict` — cannot delete a provider that has contracts.
  - **Note**: `ContractNumber` is unique within a company (filtered unique index, nullable).
  - **Note**: Indexes on `EndDate` and `RenewalDate` support queries for expiring/renewal-due contracts.

### 2.2 ContractService configuration

- [ ] Create `Allocore.Infrastructure/Persistence/Configurations/ContractServiceConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.Contracts;

  public class ContractServiceConfiguration : IEntityTypeConfiguration<ContractService>
  {
      public void Configure(EntityTypeBuilder<ContractService> builder)
      {
          builder.ToTable("ContractServices");

          builder.HasKey(cs => cs.Id);

          builder.Property(cs => cs.ContractId)
              .IsRequired();

          builder.Property(cs => cs.ServiceName)
              .IsRequired()
              .HasMaxLength(200);

          builder.Property(cs => cs.ServiceDescription)
              .HasMaxLength(1000);

          builder.Property(cs => cs.UnitPrice)
              .HasPrecision(18, 2);

          builder.Property(cs => cs.UnitType)
              .HasMaxLength(50);

          builder.Property(cs => cs.Notes)
              .HasMaxLength(1000);

          builder.HasIndex(cs => cs.ContractId);

          builder.HasOne(cs => cs.Contract)
              .WithMany(c => c.ContractServices)
              .HasForeignKey(cs => cs.ContractId)
              .OnDelete(DeleteBehavior.Cascade);
      }
  }
  ```

### 2.3 Update ApplicationDbContext

- [ ] Update `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs` — add DbSets:
  ```csharp
  using Allocore.Domain.Entities.Contracts;

  public DbSet<Contract> Contracts => Set<Contract>();
  public DbSet<ContractService> ContractServices => Set<ContractService>();
  ```

### 2.4 Create migration

- [ ] Run migration:
  ```bash
  dotnet ef migrations add AddContracts -s Allocore.API -p Allocore.Infrastructure
  ```
  - **Impact on existing data**: No existing rows affected. Two new tables created. Provider table gains a Restrict FK constraint from Contracts.

---

## Step 3: Infrastructure Layer — Repository

### 3.1 Create IContractRepository interface

- [ ] Create `Allocore.Application/Abstractions/Persistence/IContractRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;

  using Allocore.Domain.Entities.Contracts;

  public interface IContractRepository : IReadRepository<Contract>, IWriteRepository<Contract>
  {
      Task<Contract?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
      Task<bool> ExistsByContractNumberInCompanyAsync(Guid companyId, string contractNumber, CancellationToken cancellationToken = default);
      Task<bool> ExistsByContractNumberInCompanyExcludingAsync(Guid companyId, string contractNumber, Guid excludeContractId, CancellationToken cancellationToken = default);
      Task<(IEnumerable<Contract> Contracts, int TotalCount)> GetPagedByCompanyAsync(
          Guid companyId, int page, int pageSize,
          Guid? providerIdFilter = null,
          ContractStatus? statusFilter = null,
          bool? expiringWithinDays = null,
          int expiringDays = 30,
          string? searchTerm = null,
          CancellationToken cancellationToken = default);
      Task<IEnumerable<Contract>> GetByProviderAsync(Guid providerId, CancellationToken cancellationToken = default);
      Task<IEnumerable<Contract>> GetExpiringContractsAsync(Guid companyId, int withinDays, CancellationToken cancellationToken = default);
      Task<IEnumerable<Contract>> GetContractsNeedingRenewalAsync(Guid companyId, int withinDays, CancellationToken cancellationToken = default);
  }
  ```
  - **Note**: `GetByIdWithDetailsAsync` eagerly loads `ContractServices` and `Provider` — used for detail views.
  - **Note**: `GetExpiringContractsAsync` and `GetContractsNeedingRenewalAsync` support dashboard/alert views.

### 3.2 Create ContractRepository implementation

- [ ] Create `Allocore.Infrastructure/Persistence/Repositories/ContractRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Repositories;

  using Microsoft.EntityFrameworkCore;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Entities.Contracts;

  public class ContractRepository : IContractRepository
  {
      private readonly ApplicationDbContext _context;

      public ContractRepository(ApplicationDbContext context)
      {
          _context = context;
      }

      public async Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.Contracts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

      public async Task<Contract?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.Contracts
              .Include(c => c.Provider)
              .Include(c => c.ContractServices)
              .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

      public async Task<bool> ExistsByContractNumberInCompanyAsync(Guid companyId, string contractNumber, CancellationToken cancellationToken = default)
          => await _context.Contracts.AnyAsync(
              c => c.CompanyId == companyId && c.ContractNumber == contractNumber,
              cancellationToken);

      public async Task<bool> ExistsByContractNumberInCompanyExcludingAsync(Guid companyId, string contractNumber, Guid excludeContractId, CancellationToken cancellationToken = default)
          => await _context.Contracts.AnyAsync(
              c => c.CompanyId == companyId && c.ContractNumber == contractNumber && c.Id != excludeContractId,
              cancellationToken);

      public async Task<(IEnumerable<Contract> Contracts, int TotalCount)> GetPagedByCompanyAsync(
          Guid companyId, int page, int pageSize,
          Guid? providerIdFilter = null,
          ContractStatus? statusFilter = null,
          bool? expiringWithinDays = null,
          int expiringDays = 30,
          string? searchTerm = null,
          CancellationToken cancellationToken = default)
      {
          var query = _context.Contracts
              .Include(c => c.Provider)
              .Include(c => c.ContractServices)
              .Where(c => c.CompanyId == companyId);

          if (providerIdFilter.HasValue)
              query = query.Where(c => c.ProviderId == providerIdFilter.Value);

          if (statusFilter.HasValue)
              query = query.Where(c => c.Status == statusFilter.Value);

          if (expiringWithinDays == true)
          {
              var cutoff = DateTime.UtcNow.AddDays(expiringDays);
              query = query.Where(c => c.EndDate != null && c.EndDate <= cutoff && c.EndDate >= DateTime.UtcNow);
          }

          if (!string.IsNullOrWhiteSpace(searchTerm))
          {
              var term = searchTerm.ToLowerInvariant();
              query = query.Where(c =>
                  c.Title.ToLower().Contains(term) ||
                  (c.ContractNumber != null && c.ContractNumber.ToLower().Contains(term)) ||
                  (c.Provider != null && c.Provider.Name.ToLower().Contains(term)));
          }

          var totalCount = await query.CountAsync(cancellationToken);
          var contracts = await query
              .OrderByDescending(c => c.CreatedAt)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync(cancellationToken);

          return (contracts, totalCount);
      }

      public async Task<IEnumerable<Contract>> GetByProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
          => await _context.Contracts
              .Include(c => c.ContractServices)
              .Where(c => c.ProviderId == providerId)
              .OrderByDescending(c => c.StartDate)
              .ToListAsync(cancellationToken);

      public async Task<IEnumerable<Contract>> GetExpiringContractsAsync(Guid companyId, int withinDays, CancellationToken cancellationToken = default)
      {
          var cutoff = DateTime.UtcNow.AddDays(withinDays);
          return await _context.Contracts
              .Include(c => c.Provider)
              .Where(c => c.CompanyId == companyId
                  && c.EndDate != null
                  && c.EndDate <= cutoff
                  && c.EndDate >= DateTime.UtcNow
                  && c.Status == ContractStatus.Active)
              .OrderBy(c => c.EndDate)
              .ToListAsync(cancellationToken);
      }

      public async Task<IEnumerable<Contract>> GetContractsNeedingRenewalAsync(Guid companyId, int withinDays, CancellationToken cancellationToken = default)
      {
          var cutoff = DateTime.UtcNow.AddDays(withinDays);
          return await _context.Contracts
              .Include(c => c.Provider)
              .Where(c => c.CompanyId == companyId
                  && c.RenewalDate != null
                  && c.RenewalDate <= cutoff
                  && c.RenewalDate >= DateTime.UtcNow
                  && c.Status == ContractStatus.Active)
              .OrderBy(c => c.RenewalDate)
              .ToListAsync(cancellationToken);
      }

      public async Task<IEnumerable<Contract>> GetAllAsync(CancellationToken cancellationToken = default)
          => await _context.Contracts.ToListAsync(cancellationToken);

      public async Task<Contract> AddAsync(Contract entity, CancellationToken cancellationToken = default)
      {
          await _context.Contracts.AddAsync(entity, cancellationToken);
          return entity;
      }

      public Task UpdateAsync(Contract entity, CancellationToken cancellationToken = default)
      {
          _context.Contracts.Update(entity);
          return Task.CompletedTask;
      }

      public Task DeleteAsync(Contract entity, CancellationToken cancellationToken = default)
      {
          _context.Contracts.Remove(entity);
          return Task.CompletedTask;
      }
  }
  ```

### 3.3 Register in DI

- [ ] Update `Allocore.Infrastructure/DependencyInjection.cs`:
  ```csharp
  services.AddScoped<IContractRepository, ContractRepository>();
  ```

---

## Step 4: Application Layer — DTOs

### 4.1 Create Contract DTOs

- [ ] Create `Allocore.Application/Features/Contracts/DTOs/ContractDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Contracts.DTOs;

  public record ContractDto(
      Guid Id,
      Guid CompanyId,
      Guid ProviderId,
      string ProviderName,
      string Title,
      string? ContractNumber,
      string Status,
      DateTime? StartDate,
      DateTime? EndDate,
      DateTime? RenewalDate,
      bool AutoRenew,
      int? RenewalNoticeDays,
      string BillingFrequency,
      decimal? TotalValue,
      string? Currency,
      string? PaymentTerms,
      string? PriceConditions,
      string? LegalTeamContact,
      string? InternalOwner,
      string? Description,
      string? TermsAndConditions,
      bool IsExpired,
      bool IsExpiringSoon,
      DateTime CreatedAt,
      DateTime? UpdatedAt,
      IEnumerable<ContractServiceDto> Services
  );
  ```

- [ ] Create `Allocore.Application/Features/Contracts/DTOs/ContractServiceDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Contracts.DTOs;

  public record ContractServiceDto(
      Guid Id,
      string ServiceName,
      string? ServiceDescription,
      decimal? UnitPrice,
      string? UnitType,
      int? Quantity,
      string? Notes
  );
  ```

- [ ] Create `Allocore.Application/Features/Contracts/DTOs/ContractListItemDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Contracts.DTOs;

  public record ContractListItemDto(
      Guid Id,
      string Title,
      string? ContractNumber,
      string ProviderName,
      string Status,
      DateTime? StartDate,
      DateTime? EndDate,
      string BillingFrequency,
      decimal? TotalValue,
      string? Currency,
      bool IsExpired,
      bool IsExpiringSoon,
      int ServiceCount
  );
  ```

- [ ] Create `Allocore.Application/Features/Contracts/DTOs/CreateContractRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Contracts.DTOs;

  public record CreateContractRequest(
      Guid ProviderId,
      string Title,
      string? ContractNumber,
      string? Status,
      DateTime? StartDate,
      DateTime? EndDate,
      DateTime? RenewalDate,
      bool AutoRenew,
      int? RenewalNoticeDays,
      string? BillingFrequency,
      decimal? TotalValue,
      string? Currency,
      string? PaymentTerms,
      string? PriceConditions,
      string? LegalTeamContact,
      string? InternalOwner,
      string? Description,
      string? TermsAndConditions,
      IEnumerable<CreateContractServiceRequest>? Services
  );
  ```

- [ ] Create `Allocore.Application/Features/Contracts/DTOs/CreateContractServiceRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Contracts.DTOs;

  public record CreateContractServiceRequest(
      string ServiceName,
      string? ServiceDescription,
      decimal? UnitPrice,
      string? UnitType,
      int? Quantity,
      string? Notes
  );
  ```

- [ ] Create `Allocore.Application/Features/Contracts/DTOs/UpdateContractRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Contracts.DTOs;

  public record UpdateContractRequest(
      string Title,
      string? ContractNumber,
      string? Status,
      DateTime? StartDate,
      DateTime? EndDate,
      DateTime? RenewalDate,
      bool AutoRenew,
      int? RenewalNoticeDays,
      string? BillingFrequency,
      decimal? TotalValue,
      string? Currency,
      string? PaymentTerms,
      string? PriceConditions,
      string? LegalTeamContact,
      string? InternalOwner,
      string? Description,
      string? TermsAndConditions
  );
  ```

- [ ] Create `Allocore.Application/Features/Contracts/DTOs/AddContractServiceRequest.cs`:
  - Same shape as `CreateContractServiceRequest`.

- [ ] Create `Allocore.Application/Features/Contracts/DTOs/UpdateContractServiceRequest.cs`:
  - Same shape as `CreateContractServiceRequest`.

---

## Step 5: Application Layer — Validators

### 5.1 Create validators

- [ ] Create `Allocore.Application/Features/Contracts/Validators/CreateContractRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Contracts.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Contracts.DTOs;
  using Allocore.Domain.Entities.Contracts;

  public class CreateContractRequestValidator : AbstractValidator<CreateContractRequest>
  {
      public CreateContractRequestValidator()
      {
          RuleFor(x => x.ProviderId)
              .NotEmpty().WithMessage("Provider ID is required");

          RuleFor(x => x.Title)
              .NotEmpty().WithMessage("Contract title is required")
              .MaximumLength(300).WithMessage("Contract title must not exceed 300 characters");

          RuleFor(x => x.ContractNumber)
              .MaximumLength(100).WithMessage("Contract number must not exceed 100 characters")
              .When(x => !string.IsNullOrEmpty(x.ContractNumber));

          RuleFor(x => x.Status)
              .Must(s => s == null || Enum.TryParse<ContractStatus>(s, true, out _))
              .WithMessage("Status must be one of: Draft, InNegotiation, Active, Expiring, Expired, Renewed, Cancelled, Terminated");

          RuleFor(x => x.EndDate)
              .GreaterThan(x => x.StartDate)
              .WithMessage("End date must be after start date")
              .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

          RuleFor(x => x.RenewalDate)
              .LessThanOrEqualTo(x => x.EndDate)
              .WithMessage("Renewal date should be on or before end date")
              .When(x => x.RenewalDate.HasValue && x.EndDate.HasValue);

          RuleFor(x => x.RenewalNoticeDays)
              .GreaterThan(0).WithMessage("Renewal notice days must be positive")
              .When(x => x.RenewalNoticeDays.HasValue);

          RuleFor(x => x.BillingFrequency)
              .Must(bf => bf == null || Enum.TryParse<BillingFrequency>(bf, true, out _))
              .WithMessage("Billing frequency must be one of: Monthly, Quarterly, SemiAnnual, Annual, OneOff, Custom");

          RuleFor(x => x.TotalValue)
              .GreaterThanOrEqualTo(0).WithMessage("Total value must be non-negative")
              .When(x => x.TotalValue.HasValue);

          RuleFor(x => x.Currency)
              .MaximumLength(3).WithMessage("Currency must be a 3-letter ISO code (e.g., USD, BRL)")
              .When(x => !string.IsNullOrEmpty(x.Currency));

          RuleFor(x => x.PaymentTerms)
              .MaximumLength(500).WithMessage("Payment terms must not exceed 500 characters")
              .When(x => !string.IsNullOrEmpty(x.PaymentTerms));

          RuleFor(x => x.PriceConditions)
              .MaximumLength(2000).WithMessage("Price conditions must not exceed 2000 characters")
              .When(x => !string.IsNullOrEmpty(x.PriceConditions));

          RuleFor(x => x.LegalTeamContact)
              .MaximumLength(300).WithMessage("Legal team contact must not exceed 300 characters")
              .When(x => !string.IsNullOrEmpty(x.LegalTeamContact));

          RuleFor(x => x.InternalOwner)
              .MaximumLength(200).WithMessage("Internal owner must not exceed 200 characters")
              .When(x => !string.IsNullOrEmpty(x.InternalOwner));

          RuleFor(x => x.Description)
              .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
              .When(x => !string.IsNullOrEmpty(x.Description));

          RuleForEach(x => x.Services)
              .SetValidator(new CreateContractServiceRequestValidator())
              .When(x => x.Services != null);
      }
  }
  ```

- [ ] Create `Allocore.Application/Features/Contracts/Validators/CreateContractServiceRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Contracts.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Contracts.DTOs;

  public class CreateContractServiceRequestValidator : AbstractValidator<CreateContractServiceRequest>
  {
      public CreateContractServiceRequestValidator()
      {
          RuleFor(x => x.ServiceName)
              .NotEmpty().WithMessage("Service name is required")
              .MaximumLength(200).WithMessage("Service name must not exceed 200 characters");

          RuleFor(x => x.ServiceDescription)
              .MaximumLength(1000).WithMessage("Service description must not exceed 1000 characters")
              .When(x => !string.IsNullOrEmpty(x.ServiceDescription));

          RuleFor(x => x.UnitPrice)
              .GreaterThanOrEqualTo(0).WithMessage("Unit price must be non-negative")
              .When(x => x.UnitPrice.HasValue);

          RuleFor(x => x.UnitType)
              .MaximumLength(50).WithMessage("Unit type must not exceed 50 characters")
              .When(x => !string.IsNullOrEmpty(x.UnitType));

          RuleFor(x => x.Quantity)
              .GreaterThan(0).WithMessage("Quantity must be positive")
              .When(x => x.Quantity.HasValue);

          RuleFor(x => x.Notes)
              .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
              .When(x => !string.IsNullOrEmpty(x.Notes));
      }
  }
  ```

- [ ] Create `Allocore.Application/Features/Contracts/Validators/UpdateContractRequestValidator.cs`:
  - Same rules as `CreateContractRequestValidator` but without `ProviderId` (provider cannot change after creation) and without `Services` collection.

---

## Step 6: Application Layer — CQRS Commands

### 6.1 CreateContract command

- [ ] Create `Allocore.Application/Features/Contracts/Commands/CreateContractCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Contracts.Commands;

  using MediatR;
  using Allocore.Application.Features.Contracts.DTOs;
  using Allocore.Domain.Common;

  public record CreateContractCommand(Guid CompanyId, CreateContractRequest Request) : IRequest<Result<ContractDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Contracts/Commands/CreateContractCommandHandler.cs`:
  - Verify user has access to company
  - Verify provider exists and belongs to company (`_providerRepository.GetByIdAsync`, check `provider.CompanyId == command.CompanyId`)
  - If `ContractNumber` provided, check uniqueness within company via `ExistsByContractNumberInCompanyAsync`
  - Parse `Status` enum (default to `Draft` if null)
  - Parse `BillingFrequency` enum (default to `Monthly` if null)
  - Create `Contract` entity
  - Add `ContractService` items if provided
  - Save and return `ContractDto`

### 6.2 UpdateContract command

- [ ] Create `Allocore.Application/Features/Contracts/Commands/UpdateContractCommand.cs`:
  ```csharp
  public record UpdateContractCommand(Guid CompanyId, Guid ContractId, UpdateContractRequest Request) : IRequest<Result<ContractDto>>;
  ```

- [ ] Create handler:
  - Verify user access to company
  - Load contract with details, verify `contract.CompanyId == command.CompanyId`
  - If `ContractNumber` changed, check uniqueness (excluding current contract)
  - Parse enums, call `contract.Update(...)`, save, return DTO

### 6.3 UpdateContractStatus command

- [ ] Create `Allocore.Application/Features/Contracts/Commands/UpdateContractStatusCommand.cs`:
  ```csharp
  public record UpdateContractStatusCommand(Guid CompanyId, Guid ContractId, string NewStatus) : IRequest<Result>;
  ```

- [ ] Create handler:
  - Verify access, load contract, verify company
  - Parse status enum
  - Call `contract.UpdateStatus(newStatus)`, save

### 6.4 AddContractService command

- [ ] Create `Allocore.Application/Features/Contracts/Commands/AddContractServiceCommand.cs`:
  ```csharp
  public record AddContractServiceCommand(Guid CompanyId, Guid ContractId, CreateContractServiceRequest Request) : IRequest<Result<ContractServiceDto>>;
  ```

- [ ] Create handler:
  - Verify access, load contract with services, verify company
  - Create `ContractService`, add to contract, save, return DTO

### 6.5 UpdateContractService command

- [ ] Create `Allocore.Application/Features/Contracts/Commands/UpdateContractServiceCommand.cs`:
  ```csharp
  public record UpdateContractServiceCommand(Guid CompanyId, Guid ContractId, Guid ServiceId, UpdateContractServiceRequest Request) : IRequest<Result<ContractServiceDto>>;
  ```

- [ ] Create handler — verify access, load contract with services, find service, update, save.

### 6.6 RemoveContractService command

- [ ] Create `Allocore.Application/Features/Contracts/Commands/RemoveContractServiceCommand.cs`:
  ```csharp
  public record RemoveContractServiceCommand(Guid CompanyId, Guid ContractId, Guid ServiceId) : IRequest<Result>;
  ```

- [ ] Create handler — verify access, load contract with services, find service, remove, save.

---

## Step 7: Application Layer — CQRS Queries

### 7.1 GetContractById query

- [ ] Create `Allocore.Application/Features/Contracts/Queries/GetContractByIdQuery.cs`:
  ```csharp
  public record GetContractByIdQuery(Guid CompanyId, Guid ContractId) : IRequest<Result<ContractDto>>;
  ```

- [ ] Create handler — verify access, load with details, verify company, return DTO.

### 7.2 GetContractsPaged query

- [ ] Create `Allocore.Application/Features/Contracts/Queries/GetContractsPagedQuery.cs`:
  ```csharp
  public record GetContractsPagedQuery(
      Guid CompanyId,
      int Page = 1,
      int PageSize = 10,
      Guid? ProviderId = null,
      string? Status = null,
      bool? ExpiringOnly = null,
      int ExpiringDays = 30,
      string? SearchTerm = null
  ) : IRequest<PagedResult<ContractListItemDto>>;
  ```

- [ ] Create handler — verify access, parse filters, call repository, map to list DTOs, return paged result.

### 7.3 GetExpiringContracts query

- [ ] Create `Allocore.Application/Features/Contracts/Queries/GetExpiringContractsQuery.cs`:
  ```csharp
  public record GetExpiringContractsQuery(Guid CompanyId, int WithinDays = 30) : IRequest<IEnumerable<ContractListItemDto>>;
  ```

- [ ] Create handler — verify access, call `GetExpiringContractsAsync`, map to DTOs.
  - **Note**: This is a dashboard/alert query — returns contracts expiring within N days.

### 7.4 GetContractsByProvider query

- [ ] Create `Allocore.Application/Features/Contracts/Queries/GetContractsByProviderQuery.cs`:
  ```csharp
  public record GetContractsByProviderQuery(Guid CompanyId, Guid ProviderId) : IRequest<IEnumerable<ContractListItemDto>>;
  ```

- [ ] Create handler — verify access, verify provider belongs to company, call `GetByProviderAsync`, map to DTOs.

---

## Step 8: API Layer — ContractsController

- [ ] Create `Allocore.API/Controllers/v1/ContractsController.cs`:
  ```csharp
  namespace Allocore.API.Controllers.v1;

  using Asp.Versioning;
  using MediatR;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Allocore.Application.Features.Contracts.Commands;
  using Allocore.Application.Features.Contracts.DTOs;
  using Allocore.Application.Features.Contracts.Queries;

  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/companies/{companyId:guid}/contracts")]
  [Authorize]
  public class ContractsController : ControllerBase
  {
      private readonly IMediator _mediator;

      public ContractsController(IMediator mediator)
      {
          _mediator = mediator;
      }

      [HttpGet]
      public async Task<IActionResult> GetContracts(
          Guid companyId,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 10,
          [FromQuery] Guid? providerId = null,
          [FromQuery] string? status = null,
          [FromQuery] bool? expiringOnly = null,
          [FromQuery] int expiringDays = 30,
          [FromQuery] string? search = null,
          CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(
              new GetContractsPagedQuery(companyId, page, pageSize, providerId, status, expiringOnly, expiringDays, search),
              cancellationToken);
          return Ok(result);
      }

      [HttpGet("{contractId:guid}")]
      public async Task<IActionResult> GetContract(Guid companyId, Guid contractId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new GetContractByIdQuery(companyId, contractId), cancellationToken);
          if (!result.IsSuccess)
              return NotFound(new { error = result.Error });
          return Ok(result.Value);
      }

      [HttpGet("expiring")]
      public async Task<IActionResult> GetExpiringContracts(
          Guid companyId,
          [FromQuery] int withinDays = 30,
          CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(new GetExpiringContractsQuery(companyId, withinDays), cancellationToken);
          return Ok(result);
      }

      [HttpGet("by-provider/{providerId:guid}")]
      public async Task<IActionResult> GetContractsByProvider(
          Guid companyId,
          Guid providerId,
          CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(new GetContractsByProviderQuery(companyId, providerId), cancellationToken);
          return Ok(result);
      }

      [HttpPost]
      public async Task<IActionResult> CreateContract(
          Guid companyId,
          [FromBody] CreateContractRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new CreateContractCommand(companyId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return CreatedAtAction(nameof(GetContract), new { companyId, contractId = result.Value!.Id }, result.Value);
      }

      [HttpPut("{contractId:guid}")]
      public async Task<IActionResult> UpdateContract(
          Guid companyId,
          Guid contractId,
          [FromBody] UpdateContractRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new UpdateContractCommand(companyId, contractId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      [HttpPatch("{contractId:guid}/status")]
      public async Task<IActionResult> UpdateContractStatus(
          Guid companyId,
          Guid contractId,
          [FromBody] UpdateContractStatusRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new UpdateContractStatusCommand(companyId, contractId, request.Status), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }

      [HttpPost("{contractId:guid}/services")]
      public async Task<IActionResult> AddService(
          Guid companyId,
          Guid contractId,
          [FromBody] CreateContractServiceRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new AddContractServiceCommand(companyId, contractId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      [HttpPut("{contractId:guid}/services/{serviceId:guid}")]
      public async Task<IActionResult> UpdateService(
          Guid companyId,
          Guid contractId,
          Guid serviceId,
          [FromBody] UpdateContractServiceRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new UpdateContractServiceCommand(companyId, contractId, serviceId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      [HttpDelete("{contractId:guid}/services/{serviceId:guid}")]
      public async Task<IActionResult> RemoveService(
          Guid companyId,
          Guid contractId,
          Guid serviceId,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new RemoveContractServiceCommand(companyId, contractId, serviceId), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }
  }
  ```
  - **Note**: Route nested under company: `/api/v1/companies/{companyId}/contracts`
  - **Note**: `UpdateContractStatusRequest` is a simple DTO: `public record UpdateContractStatusRequest(string Status);` — create in DTOs folder.

---

## Step 9: Build, Verify & Manual Test

- [ ] Run `dotnet build` — ensure entire solution compiles
- [ ] Apply migration: `dotnet ef database update -s Allocore.API -p Allocore.Infrastructure`
- [ ] Run application and verify Swagger shows all new endpoints
- [ ] Manual test via Swagger:
  1. Create a contract for an existing provider → 201
  2. Get contract by ID → 200 with services and provider name
  3. List contracts (paginated, filtered by provider/status) → 200
  4. Update contract → 200
  5. Update contract status → 204
  6. Add service to contract → 200
  7. Update service → 200
  8. Remove service → 204
  9. Get expiring contracts → 200
  10. Verify wrong company returns error
  11. Verify non-existent provider returns error

---

## Technical Details

### Dependencies

No new NuGet packages required.

### Project Structure — Affected Files

| Layer | File | Change |
|-------|------|--------|
| **Domain** | `Allocore.Domain/Entities/Contracts/ContractStatus.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Contracts/BillingFrequency.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Contracts/Contract.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Contracts/ContractService.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Configurations/ContractConfiguration.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Configurations/ContractServiceConfiguration.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs` | **Update** — add DbSets |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Repositories/ContractRepository.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/DependencyInjection.cs` | **Update** — add registration |
| **Application** | `Allocore.Application/Abstractions/Persistence/IContractRepository.cs` | **Create** |
| **Application** | `Allocore.Application/Features/Contracts/DTOs/*.cs` | **Create** (9 files) |
| **Application** | `Allocore.Application/Features/Contracts/Validators/*.cs` | **Create** (3 files) |
| **Application** | `Allocore.Application/Features/Contracts/Commands/*.cs` | **Create** (12 files) |
| **Application** | `Allocore.Application/Features/Contracts/Queries/*.cs` | **Create** (8 files) |
| **API** | `Allocore.API/Controllers/v1/ContractsController.cs` | **Create** |

### Database

**Table: Contracts**

| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| Id | uuid | NO | gen | PK |
| CompanyId | uuid | NO | — | FK → Companies |
| ProviderId | uuid | NO | — | FK → Providers (RESTRICT) |
| Title | varchar(300) | NO | — | |
| ContractNumber | varchar(100) | YES | — | |
| Status | varchar(50) | NO | Draft | |
| StartDate | timestamp | YES | — | |
| EndDate | timestamp | YES | — | |
| RenewalDate | timestamp | YES | — | |
| AutoRenew | boolean | NO | false | |
| RenewalNoticeDays | integer | YES | — | |
| BillingFrequency | varchar(50) | NO | Monthly | |
| TotalValue | decimal(18,2) | YES | — | |
| Currency | varchar(3) | YES | — | |
| PaymentTerms | varchar(500) | YES | — | |
| PriceConditions | varchar(2000) | YES | — | |
| LegalTeamContact | varchar(300) | YES | — | |
| InternalOwner | varchar(200) | YES | — | |
| Description | varchar(2000) | YES | — | |
| TermsAndConditions | text | YES | — | |
| CreatedAt | timestamp | NO | — | |
| UpdatedAt | timestamp | YES | — | |

**Indexes:**
- `IX_Contracts_CompanyId`
- `IX_Contracts_ProviderId`
- `IX_Contracts_Status`
- `IX_Contracts_CompanyId_ProviderId`
- `IX_Contracts_EndDate`
- `IX_Contracts_RenewalDate`
- `IX_Contracts_CompanyId_ContractNumber` (UNIQUE, filtered where not null)

**Table: ContractServices**

| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| Id | uuid | NO | gen | PK |
| ContractId | uuid | NO | — | FK → Contracts (CASCADE) |
| ServiceName | varchar(200) | NO | — | |
| ServiceDescription | varchar(1000) | YES | — | |
| UnitPrice | decimal(18,2) | YES | — | |
| UnitType | varchar(50) | YES | — | |
| Quantity | integer | YES | — | |
| Notes | varchar(1000) | YES | — | |
| CreatedAt | timestamp | NO | — | |
| UpdatedAt | timestamp | YES | — | |

**Indexes:**
- `IX_ContractServices_ContractId`

### API Contract

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/v1/companies/{companyId}/contracts` | GET | Yes | List contracts (paginated, filterable) |
| `/api/v1/companies/{companyId}/contracts/{contractId}` | GET | Yes | Get contract with services |
| `/api/v1/companies/{companyId}/contracts/expiring` | GET | Yes | Get contracts expiring within N days |
| `/api/v1/companies/{companyId}/contracts/by-provider/{providerId}` | GET | Yes | Get contracts for a provider |
| `/api/v1/companies/{companyId}/contracts` | POST | Yes | Create contract (with optional services) |
| `/api/v1/companies/{companyId}/contracts/{contractId}` | PUT | Yes | Update contract details |
| `/api/v1/companies/{companyId}/contracts/{contractId}/status` | PATCH | Yes | Update contract status |
| `/api/v1/companies/{companyId}/contracts/{contractId}/services` | POST | Yes | Add service to contract |
| `/api/v1/companies/{companyId}/contracts/{contractId}/services/{serviceId}` | PUT | Yes | Update contract service |
| `/api/v1/companies/{companyId}/contracts/{contractId}/services/{serviceId}` | DELETE | Yes | Remove contract service |

### Authentication/Authorization

- All endpoints require JWT Bearer authentication
- All endpoints verify user has access to the specified company
- Fine-grained role checks deferred (same as US004)

---

## Acceptance Criteria

- [ ] Authenticated users can create contracts linked to providers within their companies
- [ ] Contracts are company-scoped — no cross-tenant data leakage
- [ ] Contract numbers are unique within a company (when provided)
- [ ] Contracts track full lifecycle: status, dates, renewal, billing frequency
- [ ] Contracts support financial details: total value, currency, payment terms, price conditions
- [ ] Contracts track legal and ownership info: legal team contact, internal owner
- [ ] Contracts support multiple service line items with pricing
- [ ] Contracts can be listed with pagination and filtered by provider, status, expiring
- [ ] Expiring contracts endpoint returns contracts expiring within N days
- [ ] Cannot delete a provider that has contracts (FK RESTRICT)
- [ ] Migrations created and applied (`AddContracts`)
- [ ] `dotnet build` passes without errors
- [ ] Swagger displays all new endpoints

---

## What is explicitly NOT changing?

- **Provider entity** — no changes to Provider or ProviderContact
- **Authentication/Authorization model** — no new roles or policies
- **Company/User entities** — no changes
- **Contract status state machine** — transitions are not enforced (user sets status freely)
- **Automated renewal notifications** — no background jobs or email notifications
- **Cost entity** — not introduced in this story

---

## Follow-ups (Intentionally Deferred)

| Item | Reason | Related Story |
|------|--------|---------------|
| Contract status state machine (enforced transitions) | Adds complexity, not needed for MVP | Future US |
| Automated expiration/renewal notifications | Requires background job infrastructure | Future US |
| Notes/timeline on contracts | Polymorphic notes system | US006 |
| Link ContractService to future Service entity | Service entity not yet created | Future US |
| Document/file attachments on contracts | Requires file storage infrastructure | Future US |
| Contract renewal workflow (create successor contract) | Complex workflow, deferred | Future US |
| Audit trail for contract changes | Requires audit infrastructure | Future US |
