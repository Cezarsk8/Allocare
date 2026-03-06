# US007 – Asset & Inventory Management

## Description

**As** an Admin user managing a company in Allocore,
**I need** to register and manage physical assets and inventory items (notebooks, monitors, keyboards, chairs, etc.),
**So that** I can track what the company owns, know the current status and condition of each item, record purchase details and warranty information, and assign items to employees with a full assignment history.

Currently, Allocore has no concept of an inventory item or physical asset. This story introduces the InventoryItem entity (company-scoped), an InventoryAssignment child entity (assignment history per item), and full CRUD operations plus assignment/return workflows. Inventory items are distinct from provider services — they represent owned physical assets, not recurring subscriptions.

**Priority**: Medium
**Dependencies**: US003 – Company & UserCompany (Multi-Tenant Core), US011 – Employees (for assignment target — EmployeeId FK)

---

## Step 1: Domain Layer — Enums & Entities

### 1.1 Create InventoryCategory enum

- [ ] Create `Allocore.Domain/Entities/Inventory/InventoryCategory.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Inventory;

  public enum InventoryCategory
  {
      Notebook = 0,
      Monitor = 1,
      Keyboard = 2,
      Mouse = 3,
      Headset = 4,
      Chair = 5,
      Desk = 6,
      Phone = 7,
      Tablet = 8,
      Printer = 9,
      NetworkEquipment = 10,
      Peripheral = 11,
      Other = 12
  }
  ```
  - **Business rule**: Every inventory item must have exactly one category.
  - **Note**: This is an extensible enum. Future stories may add categories, but the domain enforces a known set at compile time.

### 1.2 Create InventoryItemStatus enum

- [ ] Create `Allocore.Domain/Entities/Inventory/InventoryItemStatus.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Inventory;

  public enum InventoryItemStatus
  {
      Available = 0,
      InUse = 1,
      UnderMaintenance = 2,
      Decommissioned = 3,
      Lost = 4,
      Disposed = 5
  }
  ```
  - **Business rule**: Status controls what operations are allowed on an item (e.g., cannot assign a Decommissioned item).

### 1.3 Create InventoryItemCondition enum

- [ ] Create `Allocore.Domain/Entities/Inventory/InventoryItemCondition.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Inventory;

  public enum InventoryItemCondition
  {
      New = 0,
      Good = 1,
      Fair = 2,
      Poor = 3,
      Damaged = 4
  }
  ```
  - **Business rule**: Condition is tracked independently from status. An item can be InUse but in Poor condition.

### 1.4 Create InventoryItem entity

- [ ] Create `Allocore.Domain/Entities/Inventory/InventoryItem.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Inventory;

  using Allocore.Domain.Common;

  public class InventoryItem : Entity
  {
      public Guid CompanyId { get; private set; }
      public string Name { get; private set; } = string.Empty;
      public string? Description { get; private set; }
      public InventoryCategory Category { get; private set; }
      public string? Brand { get; private set; }
      public string? Model { get; private set; }
      public string? SerialNumber { get; private set; }
      public string? Barcode { get; private set; }
      public string? InternalCode { get; private set; }
      public InventoryItemStatus Status { get; private set; } = InventoryItemStatus.Available;
      public InventoryItemCondition Condition { get; private set; } = InventoryItemCondition.New;
      public DateTime? PurchaseDate { get; private set; }
      public decimal? PurchasePrice { get; private set; }
      public string? Currency { get; private set; }
      public string? InvoiceNumber { get; private set; }
      public DateTime? WarrantyExpirationDate { get; private set; }
      public string? Notes { get; private set; }
      public bool IsActive { get; private set; } = true;

      private readonly List<InventoryAssignment> _assignments = new();
      public IReadOnlyCollection<InventoryAssignment> Assignments => _assignments.AsReadOnly();

      private InventoryItem() { } // EF Core

      public static InventoryItem Create(
          Guid companyId,
          string name,
          InventoryCategory category,
          string? description = null,
          string? brand = null,
          string? model = null,
          string? serialNumber = null,
          string? barcode = null,
          string? internalCode = null,
          InventoryItemCondition condition = InventoryItemCondition.New,
          DateTime? purchaseDate = null,
          decimal? purchasePrice = null,
          string? currency = null,
          string? invoiceNumber = null,
          DateTime? warrantyExpirationDate = null,
          string? notes = null)
      {
          return new InventoryItem
          {
              CompanyId = companyId,
              Name = name,
              Category = category,
              Description = description,
              Brand = brand,
              Model = model,
              SerialNumber = serialNumber,
              Barcode = barcode,
              InternalCode = internalCode,
              Status = InventoryItemStatus.Available,
              Condition = condition,
              PurchaseDate = purchaseDate,
              PurchasePrice = purchasePrice,
              Currency = currency,
              InvoiceNumber = invoiceNumber,
              WarrantyExpirationDate = warrantyExpirationDate,
              Notes = notes,
              IsActive = true
          };
      }

      public void Update(
          string name,
          InventoryCategory category,
          string? description,
          string? brand,
          string? model,
          string? serialNumber,
          string? barcode,
          string? internalCode,
          DateTime? purchaseDate,
          decimal? purchasePrice,
          string? currency,
          string? invoiceNumber,
          DateTime? warrantyExpirationDate,
          string? notes)
      {
          Name = name;
          Category = category;
          Description = description;
          Brand = brand;
          Model = model;
          SerialNumber = serialNumber;
          Barcode = barcode;
          InternalCode = internalCode;
          PurchaseDate = purchaseDate;
          PurchasePrice = purchasePrice;
          Currency = currency;
          InvoiceNumber = invoiceNumber;
          WarrantyExpirationDate = warrantyExpirationDate;
          Notes = notes;
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

      public Result ChangeStatus(InventoryItemStatus newStatus)
      {
          // Cannot change status of deactivated items
          if (!IsActive)
              return Result.Failure("Cannot change status of a deactivated item.");

          Status = newStatus;
          UpdatedAt = DateTime.UtcNow;
          return Result.Success();
      }

      public void UpdateCondition(InventoryItemCondition newCondition)
      {
          Condition = newCondition;
          UpdatedAt = DateTime.UtcNow;
      }

      public Result AssignTo(Guid employeeId, string? notes = null)
      {
          // Cannot assign items that are Decommissioned, Lost, or Disposed
          if (Status == InventoryItemStatus.Decommissioned ||
              Status == InventoryItemStatus.Lost ||
              Status == InventoryItemStatus.Disposed)
              return Result.Failure($"Cannot assign an item with status '{Status}'.");

          // Check for existing active assignment
          var activeAssignment = _assignments.FirstOrDefault(a => a.ReturnedAt == null);
          if (activeAssignment != null)
              return Result.Failure("Item is already assigned. Return it first before reassigning.");

          var assignment = InventoryAssignment.Create(Id, employeeId, Condition, notes);
          _assignments.Add(assignment);
          Status = InventoryItemStatus.InUse;
          UpdatedAt = DateTime.UtcNow;

          return Result.Success();
      }

      public Result ReturnFromAssignee(InventoryItemCondition conditionAtReturn, string? notes = null)
      {
          var activeAssignment = _assignments.FirstOrDefault(a => a.ReturnedAt == null);
          if (activeAssignment == null)
              return Result.Failure("Item has no active assignment to return.");

          activeAssignment.MarkReturned(conditionAtReturn, notes);
          Status = InventoryItemStatus.Available;
          Condition = conditionAtReturn;
          UpdatedAt = DateTime.UtcNow;

          return Result.Success();
      }
  }
  ```
  - **Business rule**: `CompanyId` is required and immutable after creation (tenant scoping).
  - **Business rule**: `Name` is required, max 200 chars.
  - **Business rule**: Only one active assignment per item at a time (`ReturnedAt == null`).
  - **Business rule**: Cannot assign items with status `Decommissioned`, `Lost`, or `Disposed`.
  - **Business rule**: Assigning an item sets `Status = InUse`; returning sets `Status = Available` and updates `Condition`.
  - **Note**: Follows the same pattern as `Provider.cs` — private constructor, static `Create()`, mutation methods.

### 1.5 Create InventoryAssignment entity

- [ ] Create `Allocore.Domain/Entities/Inventory/InventoryAssignment.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Inventory;

  using Allocore.Domain.Common;

  public class InventoryAssignment : Entity
  {
      public Guid InventoryItemId { get; private set; }
      public Guid EmployeeId { get; private set; }
      public DateTime AssignedAt { get; private set; }
      public DateTime? ReturnedAt { get; private set; }
      public InventoryItemCondition ConditionAtAssignment { get; private set; }
      public InventoryItemCondition? ConditionAtReturn { get; private set; }
      public string? Notes { get; private set; }

      // Navigation property
      public InventoryItem? InventoryItem { get; private set; }

      private InventoryAssignment() { } // EF Core

      public static InventoryAssignment Create(
          Guid inventoryItemId,
          Guid employeeId,
          InventoryItemCondition conditionAtAssignment,
          string? notes = null)
      {
          return new InventoryAssignment
          {
              InventoryItemId = inventoryItemId,
              EmployeeId = employeeId,
              AssignedAt = DateTime.UtcNow,
              ConditionAtAssignment = conditionAtAssignment,
              Notes = notes
          };
      }

      public void MarkReturned(InventoryItemCondition conditionAtReturn, string? notes = null)
      {
          ReturnedAt = DateTime.UtcNow;
          ConditionAtReturn = conditionAtReturn;
          if (notes != null)
              Notes = notes;
          UpdatedAt = DateTime.UtcNow;
      }
  }
  ```
  - **Business rule**: `EmployeeId` is a plain Guid — no FK constraint in the database to avoid hard dependency on US011 infrastructure. Validated at application layer.
  - **Business rule**: `AssignedAt` is set automatically on creation (UTC).
  - **Business rule**: `ReturnedAt == null` means the assignment is currently active.
  - **Note**: `InventoryAssignment` does NOT have its own `CompanyId` — it inherits tenant scoping through its parent `InventoryItem`.

---

## Step 2: Infrastructure Layer — EF Core Configurations

### 2.1 InventoryItem configuration

- [ ] Create `Allocore.Infrastructure/Persistence/Configurations/InventoryItemConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.Inventory;

  public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
  {
      public void Configure(EntityTypeBuilder<InventoryItem> builder)
      {
          builder.ToTable("InventoryItems");

          builder.HasKey(i => i.Id);

          builder.Property(i => i.CompanyId)
              .IsRequired();

          builder.Property(i => i.Name)
              .IsRequired()
              .HasMaxLength(200);

          builder.Property(i => i.Description)
              .HasMaxLength(2000);

          builder.Property(i => i.Category)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(i => i.Brand)
              .HasMaxLength(100);

          builder.Property(i => i.Model)
              .HasMaxLength(100);

          builder.Property(i => i.SerialNumber)
              .HasMaxLength(100);

          builder.Property(i => i.Barcode)
              .HasMaxLength(100);

          builder.Property(i => i.InternalCode)
              .HasMaxLength(50);

          builder.Property(i => i.Status)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(i => i.Condition)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(i => i.PurchasePrice)
              .HasPrecision(18, 2);

          builder.Property(i => i.Currency)
              .HasMaxLength(3);

          builder.Property(i => i.InvoiceNumber)
              .HasMaxLength(100);

          builder.Property(i => i.Notes)
              .HasMaxLength(2000);

          builder.Property(i => i.IsActive)
              .IsRequired()
              .HasDefaultValue(true);

          // Filtered unique index: InternalCode unique within company (only non-null values)
          builder.HasIndex(i => new { i.CompanyId, i.InternalCode })
              .IsUnique()
              .HasFilter("\"InternalCode\" IS NOT NULL");

          // Filtered unique index: SerialNumber unique within company (only non-null values)
          builder.HasIndex(i => new { i.CompanyId, i.SerialNumber })
              .IsUnique()
              .HasFilter("\"SerialNumber\" IS NOT NULL");

          builder.HasIndex(i => i.CompanyId);

          builder.HasIndex(i => i.Category);

          builder.HasIndex(i => i.Status);

          builder.HasMany(i => i.Assignments)
              .WithOne(a => a.InventoryItem)
              .HasForeignKey(a => a.InventoryItemId)
              .OnDelete(DeleteBehavior.Cascade);
      }
  }
  ```
  - **Note**: Enums (`Category`, `Status`, `Condition`) stored as string for readability in DB. Follows same pattern as `ProviderCategory` in `ProviderConfiguration`.
  - **Note**: Filtered unique indexes on `(CompanyId, InternalCode)` and `(CompanyId, SerialNumber)` allow multiple NULL values while enforcing uniqueness for non-null values.
  - **Note**: `PurchasePrice` uses `HasPrecision(18, 2)` for monetary values.

### 2.2 InventoryAssignment configuration

- [ ] Create `Allocore.Infrastructure/Persistence/Configurations/InventoryAssignmentConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.Inventory;

  public class InventoryAssignmentConfiguration : IEntityTypeConfiguration<InventoryAssignment>
  {
      public void Configure(EntityTypeBuilder<InventoryAssignment> builder)
      {
          builder.ToTable("InventoryAssignments");

          builder.HasKey(a => a.Id);

          builder.Property(a => a.InventoryItemId)
              .IsRequired();

          builder.Property(a => a.EmployeeId)
              .IsRequired();

          builder.Property(a => a.AssignedAt)
              .IsRequired();

          builder.Property(a => a.ConditionAtAssignment)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(a => a.ConditionAtReturn)
              .HasConversion<string>()
              .HasMaxLength(50);

          builder.Property(a => a.Notes)
              .HasMaxLength(2000);

          builder.HasIndex(a => a.InventoryItemId);

          builder.HasIndex(a => a.EmployeeId);

          builder.HasOne(a => a.InventoryItem)
              .WithMany(i => i.Assignments)
              .HasForeignKey(a => a.InventoryItemId)
              .OnDelete(DeleteBehavior.Cascade);
      }
  }
  ```
  - **Note**: `EmployeeId` is stored as a plain `Guid` with NO foreign key constraint. This avoids a hard dependency on the Employees table (US011). Validation that the employee exists is done at the application layer.

### 2.3 Update ApplicationDbContext

- [ ] Update `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs` — add DbSets:
  ```csharp
  // Add these using statements
  using Allocore.Domain.Entities.Inventory;

  // Add these DbSets
  public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
  public DbSet<InventoryAssignment> InventoryAssignments => Set<InventoryAssignment>();
  ```
  - **Note**: `OnModelCreating` already calls `ApplyConfigurationsFromAssembly` so the new configurations will be auto-discovered.

### 2.4 Create migration

- [ ] Run migration:
  ```bash
  dotnet ef migrations add AddInventory -s Allocore.API -p Allocore.Infrastructure
  ```
  - **Impact on existing data**: No existing rows affected. Two new tables created.

---

## Step 3: Infrastructure Layer — Repository

### 3.1 Create IInventoryItemRepository interface

- [ ] Create `Allocore.Application/Abstractions/Persistence/IInventoryItemRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;

  using Allocore.Domain.Entities.Inventory;

  public interface IInventoryItemRepository : IReadRepository<InventoryItem>, IWriteRepository<InventoryItem>
  {
      Task<InventoryItem?> GetByIdWithAssignmentsAsync(Guid id, CancellationToken cancellationToken = default);
      Task<bool> ExistsByInternalCodeInCompanyAsync(Guid companyId, string internalCode, CancellationToken cancellationToken = default);
      Task<bool> ExistsByInternalCodeInCompanyExcludingAsync(Guid companyId, string internalCode, Guid excludeItemId, CancellationToken cancellationToken = default);
      Task<bool> ExistsBySerialNumberInCompanyAsync(Guid companyId, string serialNumber, CancellationToken cancellationToken = default);
      Task<bool> ExistsBySerialNumberInCompanyExcludingAsync(Guid companyId, string serialNumber, Guid excludeItemId, CancellationToken cancellationToken = default);
      Task<(IEnumerable<InventoryItem> Items, int TotalCount)> GetPagedByCompanyAsync(
          Guid companyId, int page, int pageSize,
          InventoryCategory? categoryFilter = null,
          InventoryItemStatus? statusFilter = null,
          InventoryItemCondition? conditionFilter = null,
          bool? isActiveFilter = null,
          string? searchTerm = null,
          CancellationToken cancellationToken = default);
      Task<(IEnumerable<InventoryAssignment> Assignments, int TotalCount)> GetAssignmentsByItemPagedAsync(
          Guid itemId, int page, int pageSize,
          CancellationToken cancellationToken = default);
  }
  ```
  - **Note**: Interface lives in Application layer (same pattern as `IProviderRepository`).
  - **Note**: `GetByIdWithAssignmentsAsync` eagerly loads assignments — used for detail views.
  - **Note**: `GetPagedByCompanyAsync` supports filtering by category, status, condition, active status, and search term (name/brand/model/serialNumber/internalCode).
  - **Note**: `GetAssignmentsByItemPagedAsync` returns paginated assignment history for a specific item.

### 3.2 Create InventoryItemRepository implementation

- [ ] Create `Allocore.Infrastructure/Persistence/Repositories/InventoryItemRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Repositories;

  using Microsoft.EntityFrameworkCore;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Entities.Inventory;

  public class InventoryItemRepository : IInventoryItemRepository
  {
      private readonly ApplicationDbContext _context;

      public InventoryItemRepository(ApplicationDbContext context)
      {
          _context = context;
      }

      public async Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

      public async Task<InventoryItem?> GetByIdWithAssignmentsAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.InventoryItems
              .Include(i => i.Assignments.OrderByDescending(a => a.AssignedAt))
              .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

      public async Task<bool> ExistsByInternalCodeInCompanyAsync(Guid companyId, string internalCode, CancellationToken cancellationToken = default)
          => await _context.InventoryItems.AnyAsync(
              i => i.CompanyId == companyId && i.InternalCode == internalCode,
              cancellationToken);

      public async Task<bool> ExistsByInternalCodeInCompanyExcludingAsync(Guid companyId, string internalCode, Guid excludeItemId, CancellationToken cancellationToken = default)
          => await _context.InventoryItems.AnyAsync(
              i => i.CompanyId == companyId && i.InternalCode == internalCode && i.Id != excludeItemId,
              cancellationToken);

      public async Task<bool> ExistsBySerialNumberInCompanyAsync(Guid companyId, string serialNumber, CancellationToken cancellationToken = default)
          => await _context.InventoryItems.AnyAsync(
              i => i.CompanyId == companyId && i.SerialNumber == serialNumber,
              cancellationToken);

      public async Task<bool> ExistsBySerialNumberInCompanyExcludingAsync(Guid companyId, string serialNumber, Guid excludeItemId, CancellationToken cancellationToken = default)
          => await _context.InventoryItems.AnyAsync(
              i => i.CompanyId == companyId && i.SerialNumber == serialNumber && i.Id != excludeItemId,
              cancellationToken);

      public async Task<(IEnumerable<InventoryItem> Items, int TotalCount)> GetPagedByCompanyAsync(
          Guid companyId, int page, int pageSize,
          InventoryCategory? categoryFilter = null,
          InventoryItemStatus? statusFilter = null,
          InventoryItemCondition? conditionFilter = null,
          bool? isActiveFilter = null,
          string? searchTerm = null,
          CancellationToken cancellationToken = default)
      {
          var query = _context.InventoryItems
              .Where(i => i.CompanyId == companyId);

          if (categoryFilter.HasValue)
              query = query.Where(i => i.Category == categoryFilter.Value);

          if (statusFilter.HasValue)
              query = query.Where(i => i.Status == statusFilter.Value);

          if (conditionFilter.HasValue)
              query = query.Where(i => i.Condition == conditionFilter.Value);

          if (isActiveFilter.HasValue)
              query = query.Where(i => i.IsActive == isActiveFilter.Value);

          if (!string.IsNullOrWhiteSpace(searchTerm))
          {
              var term = searchTerm.ToLowerInvariant();
              query = query.Where(i =>
                  i.Name.ToLower().Contains(term) ||
                  (i.Brand != null && i.Brand.ToLower().Contains(term)) ||
                  (i.Model != null && i.Model.ToLower().Contains(term)) ||
                  (i.SerialNumber != null && i.SerialNumber.ToLower().Contains(term)) ||
                  (i.InternalCode != null && i.InternalCode.ToLower().Contains(term)));
          }

          var totalCount = await query.CountAsync(cancellationToken);
          var items = await query
              .OrderBy(i => i.Name)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync(cancellationToken);

          return (items, totalCount);
      }

      public async Task<(IEnumerable<InventoryAssignment> Assignments, int TotalCount)> GetAssignmentsByItemPagedAsync(
          Guid itemId, int page, int pageSize,
          CancellationToken cancellationToken = default)
      {
          var query = _context.InventoryAssignments
              .Where(a => a.InventoryItemId == itemId);

          var totalCount = await query.CountAsync(cancellationToken);
          var assignments = await query
              .OrderByDescending(a => a.AssignedAt)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync(cancellationToken);

          return (assignments, totalCount);
      }

      public async Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
          => await _context.InventoryItems.ToListAsync(cancellationToken);

      public async Task<InventoryItem> AddAsync(InventoryItem entity, CancellationToken cancellationToken = default)
      {
          await _context.InventoryItems.AddAsync(entity, cancellationToken);
          return entity;
      }

      public Task UpdateAsync(InventoryItem entity, CancellationToken cancellationToken = default)
      {
          _context.InventoryItems.Update(entity);
          return Task.CompletedTask;
      }

      public Task DeleteAsync(InventoryItem entity, CancellationToken cancellationToken = default)
      {
          _context.InventoryItems.Remove(entity);
          return Task.CompletedTask;
      }
  }
  ```
  - **Note**: Follows exact same pattern as `ProviderRepository`.
  - **Note**: Search spans multiple fields: name, brand, model, serial number, internal code.
  - **Note**: Assignments are ordered by `AssignedAt` descending (most recent first).

### 3.3 Register in DI

- [ ] Update `Allocore.Infrastructure/DependencyInjection.cs`:
  ```csharp
  // Add registration
  services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
  ```

---

## Step 4: Application Layer — DTOs

### 4.1 Create Inventory DTOs

- [ ] Create `Allocore.Application/Features/Inventory/DTOs/InventoryItemDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.DTOs;

  public record InventoryItemDto(
      Guid Id,
      Guid CompanyId,
      string Name,
      string? Description,
      string Category,
      string? Brand,
      string? Model,
      string? SerialNumber,
      string? Barcode,
      string? InternalCode,
      string Status,
      string Condition,
      DateTime? PurchaseDate,
      decimal? PurchasePrice,
      string? Currency,
      string? InvoiceNumber,
      DateTime? WarrantyExpirationDate,
      string? Notes,
      bool IsActive,
      DateTime CreatedAt,
      DateTime? UpdatedAt,
      Guid? CurrentAssigneeId,
      IEnumerable<InventoryAssignmentDto> Assignments
  );
  ```

- [ ] Create `Allocore.Application/Features/Inventory/DTOs/InventoryItemListDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.DTOs;

  public record InventoryItemListDto(
      Guid Id,
      string Name,
      string Category,
      string? Brand,
      string? Model,
      string? InternalCode,
      string Status,
      string Condition,
      bool IsActive,
      Guid? CurrentAssigneeId
  );
  ```
  - **Note**: Lightweight DTO for list views. Avoids sending full details in paginated lists.

- [ ] Create `Allocore.Application/Features/Inventory/DTOs/InventoryAssignmentDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.DTOs;

  public record InventoryAssignmentDto(
      Guid Id,
      Guid EmployeeId,
      DateTime AssignedAt,
      DateTime? ReturnedAt,
      string ConditionAtAssignment,
      string? ConditionAtReturn,
      string? Notes
  );
  ```

- [ ] Create `Allocore.Application/Features/Inventory/DTOs/CreateInventoryItemRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.DTOs;

  public record CreateInventoryItemRequest(
      string Name,
      string Category,
      string? Description,
      string? Brand,
      string? Model,
      string? SerialNumber,
      string? Barcode,
      string? InternalCode,
      string? Condition,
      DateTime? PurchaseDate,
      decimal? PurchasePrice,
      string? Currency,
      string? InvoiceNumber,
      DateTime? WarrantyExpirationDate,
      string? Notes
  );
  ```

- [ ] Create `Allocore.Application/Features/Inventory/DTOs/UpdateInventoryItemRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.DTOs;

  public record UpdateInventoryItemRequest(
      string Name,
      string Category,
      string? Description,
      string? Brand,
      string? Model,
      string? SerialNumber,
      string? Barcode,
      string? InternalCode,
      DateTime? PurchaseDate,
      decimal? PurchasePrice,
      string? Currency,
      string? InvoiceNumber,
      DateTime? WarrantyExpirationDate,
      string? Notes
  );
  ```

- [ ] Create `Allocore.Application/Features/Inventory/DTOs/AssignInventoryItemRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.DTOs;

  public record AssignInventoryItemRequest(
      Guid EmployeeId,
      string? Notes
  );
  ```

- [ ] Create `Allocore.Application/Features/Inventory/DTOs/ReturnInventoryItemRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.DTOs;

  public record ReturnInventoryItemRequest(
      string Condition,
      string? Notes
  );
  ```

- [ ] Create `Allocore.Application/Features/Inventory/DTOs/ChangeInventoryItemStatusRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.DTOs;

  public record ChangeInventoryItemStatusRequest(
      string Status
  );
  ```

---

## Step 5: Application Layer — Validators

### 5.1 Create validators

- [ ] Create `Allocore.Application/Features/Inventory/Validators/CreateInventoryItemRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Inventory.DTOs;
  using Allocore.Domain.Entities.Inventory;

  public class CreateInventoryItemRequestValidator : AbstractValidator<CreateInventoryItemRequest>
  {
      public CreateInventoryItemRequestValidator()
      {
          RuleFor(x => x.Name)
              .NotEmpty().WithMessage("Item name is required")
              .MaximumLength(200).WithMessage("Item name must not exceed 200 characters");

          RuleFor(x => x.Category)
              .NotEmpty().WithMessage("Category is required")
              .Must(c => Enum.TryParse<InventoryCategory>(c, true, out _))
              .WithMessage("Category must be one of: Notebook, Monitor, Keyboard, Mouse, Headset, Chair, Desk, Phone, Tablet, Printer, NetworkEquipment, Peripheral, Other");

          RuleFor(x => x.Description)
              .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
              .When(x => !string.IsNullOrEmpty(x.Description));

          RuleFor(x => x.Brand)
              .MaximumLength(100).WithMessage("Brand must not exceed 100 characters")
              .When(x => !string.IsNullOrEmpty(x.Brand));

          RuleFor(x => x.Model)
              .MaximumLength(100).WithMessage("Model must not exceed 100 characters")
              .When(x => !string.IsNullOrEmpty(x.Model));

          RuleFor(x => x.SerialNumber)
              .MaximumLength(100).WithMessage("Serial number must not exceed 100 characters")
              .When(x => !string.IsNullOrEmpty(x.SerialNumber));

          RuleFor(x => x.Barcode)
              .MaximumLength(100).WithMessage("Barcode must not exceed 100 characters")
              .When(x => !string.IsNullOrEmpty(x.Barcode));

          RuleFor(x => x.InternalCode)
              .MaximumLength(50).WithMessage("Internal code must not exceed 50 characters")
              .When(x => !string.IsNullOrEmpty(x.InternalCode));

          RuleFor(x => x.Condition)
              .Must(c => Enum.TryParse<InventoryItemCondition>(c, true, out _))
              .WithMessage("Condition must be one of: New, Good, Fair, Poor, Damaged")
              .When(x => !string.IsNullOrEmpty(x.Condition));

          RuleFor(x => x.PurchasePrice)
              .GreaterThan(0).WithMessage("Purchase price must be greater than zero")
              .When(x => x.PurchasePrice.HasValue);

          RuleFor(x => x.Currency)
              .MaximumLength(3).WithMessage("Currency must be a 3-letter ISO code (e.g., BRL, USD)")
              .When(x => !string.IsNullOrEmpty(x.Currency));

          RuleFor(x => x.InvoiceNumber)
              .MaximumLength(100).WithMessage("Invoice number must not exceed 100 characters")
              .When(x => !string.IsNullOrEmpty(x.InvoiceNumber));

          RuleFor(x => x.Notes)
              .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters")
              .When(x => !string.IsNullOrEmpty(x.Notes));
      }
  }
  ```

- [ ] Create `Allocore.Application/Features/Inventory/Validators/UpdateInventoryItemRequestValidator.cs`:
  - Same rules as `CreateInventoryItemRequestValidator` but without the `Condition` field rule (condition is updated via separate operations).

- [ ] Create `Allocore.Application/Features/Inventory/Validators/AssignInventoryItemRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Inventory.DTOs;

  public class AssignInventoryItemRequestValidator : AbstractValidator<AssignInventoryItemRequest>
  {
      public AssignInventoryItemRequestValidator()
      {
          RuleFor(x => x.EmployeeId)
              .NotEmpty().WithMessage("Employee ID is required");

          RuleFor(x => x.Notes)
              .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters")
              .When(x => !string.IsNullOrEmpty(x.Notes));
      }
  }
  ```

- [ ] Create `Allocore.Application/Features/Inventory/Validators/ReturnInventoryItemRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Inventory.DTOs;
  using Allocore.Domain.Entities.Inventory;

  public class ReturnInventoryItemRequestValidator : AbstractValidator<ReturnInventoryItemRequest>
  {
      public ReturnInventoryItemRequestValidator()
      {
          RuleFor(x => x.Condition)
              .NotEmpty().WithMessage("Condition at return is required")
              .Must(c => Enum.TryParse<InventoryItemCondition>(c, true, out _))
              .WithMessage("Condition must be one of: New, Good, Fair, Poor, Damaged");

          RuleFor(x => x.Notes)
              .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters")
              .When(x => !string.IsNullOrEmpty(x.Notes));
      }
  }
  ```

- [ ] Create `Allocore.Application/Features/Inventory/Validators/ChangeInventoryItemStatusRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Inventory.DTOs;
  using Allocore.Domain.Entities.Inventory;

  public class ChangeInventoryItemStatusRequestValidator : AbstractValidator<ChangeInventoryItemStatusRequest>
  {
      public ChangeInventoryItemStatusRequestValidator()
      {
          RuleFor(x => x.Status)
              .NotEmpty().WithMessage("Status is required")
              .Must(s => Enum.TryParse<InventoryItemStatus>(s, true, out _))
              .WithMessage("Status must be one of: Available, InUse, UnderMaintenance, Decommissioned, Lost, Disposed");
      }
  }
  ```

---

## Step 6: Application Layer — CQRS Commands

### 6.1 CreateInventoryItem command

- [ ] Create `Allocore.Application/Features/Inventory/Commands/CreateInventoryItemCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Commands;

  using MediatR;
  using Allocore.Application.Features.Inventory.DTOs;
  using Allocore.Domain.Common;

  public record CreateInventoryItemCommand(Guid CompanyId, CreateInventoryItemRequest Request) : IRequest<Result<InventoryItemDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Inventory/Commands/CreateInventoryItemCommandHandler.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Commands;

  using MediatR;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Application.Abstractions.Services;
  using Allocore.Application.Features.Inventory.DTOs;
  using Allocore.Domain.Common;
  using Allocore.Domain.Entities.Inventory;

  public class CreateInventoryItemCommandHandler : IRequestHandler<CreateInventoryItemCommand, Result<InventoryItemDto>>
  {
      private readonly IInventoryItemRepository _inventoryItemRepository;
      private readonly IUserCompanyRepository _userCompanyRepository;
      private readonly ICurrentUser _currentUser;
      private readonly IUnitOfWork _unitOfWork;

      public CreateInventoryItemCommandHandler(
          IInventoryItemRepository inventoryItemRepository,
          IUserCompanyRepository userCompanyRepository,
          ICurrentUser currentUser,
          IUnitOfWork unitOfWork)
      {
          _inventoryItemRepository = inventoryItemRepository;
          _userCompanyRepository = userCompanyRepository;
          _currentUser = currentUser;
          _unitOfWork = unitOfWork;
      }

      public async Task<Result<InventoryItemDto>> Handle(CreateInventoryItemCommand command, CancellationToken cancellationToken)
      {
          // Verify user has access to company
          if (!_currentUser.UserId.HasValue)
              return Result.Failure<InventoryItemDto>("User not authenticated.");

          var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
              _currentUser.UserId.Value, command.CompanyId, cancellationToken);
          if (!hasAccess)
              return Result.Failure<InventoryItemDto>("You don't have access to this company.");

          // Check for duplicate InternalCode within company
          if (!string.IsNullOrEmpty(command.Request.InternalCode))
          {
              if (await _inventoryItemRepository.ExistsByInternalCodeInCompanyAsync(
                  command.CompanyId, command.Request.InternalCode, cancellationToken))
                  return Result.Failure<InventoryItemDto>("An item with this internal code already exists in this company.");
          }

          // Check for duplicate SerialNumber within company
          if (!string.IsNullOrEmpty(command.Request.SerialNumber))
          {
              if (await _inventoryItemRepository.ExistsBySerialNumberInCompanyAsync(
                  command.CompanyId, command.Request.SerialNumber, cancellationToken))
                  return Result.Failure<InventoryItemDto>("An item with this serial number already exists in this company.");
          }

          // Parse category
          if (!Enum.TryParse<InventoryCategory>(command.Request.Category, true, out var category))
              return Result.Failure<InventoryItemDto>("Invalid inventory category.");

          // Parse condition (default to New)
          var condition = InventoryItemCondition.New;
          if (!string.IsNullOrEmpty(command.Request.Condition))
          {
              if (!Enum.TryParse<InventoryItemCondition>(command.Request.Condition, true, out condition))
                  return Result.Failure<InventoryItemDto>("Invalid item condition.");
          }

          // Create inventory item
          var item = InventoryItem.Create(
              command.CompanyId,
              command.Request.Name,
              category,
              command.Request.Description,
              command.Request.Brand,
              command.Request.Model,
              command.Request.SerialNumber,
              command.Request.Barcode,
              command.Request.InternalCode,
              condition,
              command.Request.PurchaseDate,
              command.Request.PurchasePrice,
              command.Request.Currency,
              command.Request.InvoiceNumber,
              command.Request.WarrantyExpirationDate,
              command.Request.Notes);

          await _inventoryItemRepository.AddAsync(item, cancellationToken);
          await _unitOfWork.SaveChangesAsync(cancellationToken);

          return Result.Success(MapToDto(item));
      }

      private static InventoryItemDto MapToDto(InventoryItem item) => new(
          item.Id,
          item.CompanyId,
          item.Name,
          item.Description,
          item.Category.ToString(),
          item.Brand,
          item.Model,
          item.SerialNumber,
          item.Barcode,
          item.InternalCode,
          item.Status.ToString(),
          item.Condition.ToString(),
          item.PurchaseDate,
          item.PurchasePrice,
          item.Currency,
          item.InvoiceNumber,
          item.WarrantyExpirationDate,
          item.Notes,
          item.IsActive,
          item.CreatedAt,
          item.UpdatedAt,
          item.Assignments.FirstOrDefault(a => a.ReturnedAt == null)?.EmployeeId,
          item.Assignments.Select(a => new InventoryAssignmentDto(
              a.Id, a.EmployeeId, a.AssignedAt, a.ReturnedAt,
              a.ConditionAtAssignment.ToString(), a.ConditionAtReturn?.ToString(),
              a.Notes))
      );
  }
  ```
  - **Business rule**: User must have access to the company. Creating items requires at minimum Manager role — enforce via `UserHasAccessToCompanyAsync` for now; role-level granularity deferred.
  - **Note**: Follows same authorization pattern as `CreateProviderCommandHandler` — check user access, then proceed.

### 6.2 UpdateInventoryItem command

- [ ] Create `Allocore.Application/Features/Inventory/Commands/UpdateInventoryItemCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Commands;

  using MediatR;
  using Allocore.Application.Features.Inventory.DTOs;
  using Allocore.Domain.Common;

  public record UpdateInventoryItemCommand(Guid CompanyId, Guid ItemId, UpdateInventoryItemRequest Request) : IRequest<Result<InventoryItemDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Inventory/Commands/UpdateInventoryItemCommandHandler.cs`:
  - Verify user has access to company
  - Load item by ID, verify it belongs to the company (`item.CompanyId == command.CompanyId`)
  - Check for duplicate InternalCode within company (excluding current item via `ExistsByInternalCodeInCompanyExcludingAsync`)
  - Check for duplicate SerialNumber within company (excluding current item via `ExistsBySerialNumberInCompanyExcludingAsync`)
  - Parse category enum
  - Call `item.Update(...)`, save changes
  - Return updated `InventoryItemDto`

### 6.3 DeactivateInventoryItem command

- [ ] Create `Allocore.Application/Features/Inventory/Commands/DeactivateInventoryItemCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Commands;

  using MediatR;
  using Allocore.Domain.Common;

  public record DeactivateInventoryItemCommand(Guid CompanyId, Guid ItemId) : IRequest<Result>;
  ```

- [ ] Create `Allocore.Application/Features/Inventory/Commands/DeactivateInventoryItemCommandHandler.cs`:
  - Verify user access to company
  - Load item, verify it belongs to company
  - Call `item.Deactivate()`, save changes
  - Return `Result.Success()`

### 6.4 ChangeInventoryItemStatus command

- [ ] Create `Allocore.Application/Features/Inventory/Commands/ChangeInventoryItemStatusCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Commands;

  using MediatR;
  using Allocore.Application.Features.Inventory.DTOs;
  using Allocore.Domain.Common;

  public record ChangeInventoryItemStatusCommand(Guid CompanyId, Guid ItemId, ChangeInventoryItemStatusRequest Request) : IRequest<Result>;
  ```

- [ ] Create `Allocore.Application/Features/Inventory/Commands/ChangeInventoryItemStatusCommandHandler.cs`:
  - Verify user access to company
  - Load item, verify it belongs to company
  - Parse status enum
  - Call `item.ChangeStatus(newStatus)`, save changes
  - Return `Result.Success()`

### 6.5 AssignInventoryItem command

- [ ] Create `Allocore.Application/Features/Inventory/Commands/AssignInventoryItemCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Commands;

  using MediatR;
  using Allocore.Application.Features.Inventory.DTOs;
  using Allocore.Domain.Common;

  public record AssignInventoryItemCommand(Guid CompanyId, Guid ItemId, AssignInventoryItemRequest Request) : IRequest<Result>;
  ```

- [ ] Create `Allocore.Application/Features/Inventory/Commands/AssignInventoryItemCommandHandler.cs`:
  - Verify user access to company
  - Load item with assignments (`GetByIdWithAssignmentsAsync`), verify it belongs to company
  - Call `item.AssignTo(request.EmployeeId, request.Notes)` — domain method handles all validation
  - Save changes
  - Return `Result.Success()`

### 6.6 ReturnInventoryItem command

- [ ] Create `Allocore.Application/Features/Inventory/Commands/ReturnInventoryItemCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Commands;

  using MediatR;
  using Allocore.Application.Features.Inventory.DTOs;
  using Allocore.Domain.Common;

  public record ReturnInventoryItemCommand(Guid CompanyId, Guid ItemId, ReturnInventoryItemRequest Request) : IRequest<Result>;
  ```

- [ ] Create `Allocore.Application/Features/Inventory/Commands/ReturnInventoryItemCommandHandler.cs`:
  - Verify user access to company
  - Load item with assignments, verify it belongs to company
  - Parse condition enum
  - Call `item.ReturnFromAssignee(conditionAtReturn, request.Notes)` — domain method handles all validation
  - Save changes
  - Return `Result.Success()`

---

## Step 7: Application Layer — CQRS Queries

### 7.1 GetInventoryItemById query

- [ ] Create `Allocore.Application/Features/Inventory/Queries/GetInventoryItemByIdQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Queries;

  using MediatR;
  using Allocore.Application.Features.Inventory.DTOs;
  using Allocore.Domain.Common;

  public record GetInventoryItemByIdQuery(Guid CompanyId, Guid ItemId) : IRequest<Result<InventoryItemDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Inventory/Queries/GetInventoryItemByIdQueryHandler.cs`:
  - Verify user access to company
  - Load item with assignments, verify it belongs to company
  - Return `InventoryItemDto`

### 7.2 GetInventoryItemsPaged query

- [ ] Create `Allocore.Application/Features/Inventory/Queries/GetInventoryItemsPagedQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Queries;

  using MediatR;
  using Allocore.Application.Common;
  using Allocore.Application.Features.Inventory.DTOs;

  public record GetInventoryItemsPagedQuery(
      Guid CompanyId,
      int Page = 1,
      int PageSize = 10,
      string? Category = null,
      string? Status = null,
      string? Condition = null,
      bool? IsActive = null,
      string? SearchTerm = null
  ) : IRequest<PagedResult<InventoryItemListDto>>;
  ```
  - **Note**: `PagedResult<T>` should already exist from US004. If not, create it as described in US004 Step 7.

- [ ] Create `Allocore.Application/Features/Inventory/Queries/GetInventoryItemsPagedQueryHandler.cs`:
  - Verify user access to company
  - Parse category, status, condition filters if provided
  - Call `_inventoryItemRepository.GetPagedByCompanyAsync(...)` with filters
  - Map to `InventoryItemListDto` (lightweight: name, category, brand, model, internalCode, status, condition, isActive, currentAssigneeId)
  - Return `PagedResult<InventoryItemListDto>`

### 7.3 GetInventoryItemAssignments query

- [ ] Create `Allocore.Application/Features/Inventory/Queries/GetInventoryItemAssignmentsQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Inventory.Queries;

  using MediatR;
  using Allocore.Application.Common;
  using Allocore.Application.Features.Inventory.DTOs;

  public record GetInventoryItemAssignmentsQuery(
      Guid CompanyId,
      Guid ItemId,
      int Page = 1,
      int PageSize = 10
  ) : IRequest<PagedResult<InventoryAssignmentDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Inventory/Queries/GetInventoryItemAssignmentsQueryHandler.cs`:
  - Verify user access to company
  - Verify item exists and belongs to company
  - Call `_inventoryItemRepository.GetAssignmentsByItemPagedAsync(...)`
  - Map to `InventoryAssignmentDto`
  - Return `PagedResult<InventoryAssignmentDto>`

---

## Step 8: API Layer — InventoryController

- [ ] Create `Allocore.API/Controllers/v1/InventoryController.cs`:
  ```csharp
  namespace Allocore.API.Controllers.v1;

  using Asp.Versioning;
  using MediatR;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Allocore.Application.Features.Inventory.Commands;
  using Allocore.Application.Features.Inventory.DTOs;
  using Allocore.Application.Features.Inventory.Queries;

  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/companies/{companyId:guid}/inventory")]
  [Authorize]
  public class InventoryController : ControllerBase
  {
      private readonly IMediator _mediator;

      public InventoryController(IMediator mediator)
      {
          _mediator = mediator;
      }

      /// <summary>
      /// List inventory items for a company (paginated, filterable).
      /// </summary>
      [HttpGet]
      public async Task<IActionResult> GetItems(
          Guid companyId,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 10,
          [FromQuery] string? category = null,
          [FromQuery] string? status = null,
          [FromQuery] string? condition = null,
          [FromQuery] bool? isActive = null,
          [FromQuery] string? search = null,
          CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(
              new GetInventoryItemsPagedQuery(companyId, page, pageSize, category, status, condition, isActive, search),
              cancellationToken);
          return Ok(result);
      }

      /// <summary>
      /// Get an inventory item by ID with full details and assignment history.
      /// </summary>
      [HttpGet("{itemId:guid}")]
      public async Task<IActionResult> GetItem(Guid companyId, Guid itemId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new GetInventoryItemByIdQuery(companyId, itemId), cancellationToken);
          if (!result.IsSuccess)
              return NotFound(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Create a new inventory item.
      /// </summary>
      [HttpPost]
      public async Task<IActionResult> CreateItem(
          Guid companyId,
          [FromBody] CreateInventoryItemRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new CreateInventoryItemCommand(companyId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return CreatedAtAction(nameof(GetItem), new { companyId, itemId = result.Value!.Id }, result.Value);
      }

      /// <summary>
      /// Update an inventory item's details.
      /// </summary>
      [HttpPut("{itemId:guid}")]
      public async Task<IActionResult> UpdateItem(
          Guid companyId,
          Guid itemId,
          [FromBody] UpdateInventoryItemRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new UpdateInventoryItemCommand(companyId, itemId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Deactivate an inventory item (soft delete).
      /// </summary>
      [HttpPatch("{itemId:guid}/deactivate")]
      public async Task<IActionResult> DeactivateItem(Guid companyId, Guid itemId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new DeactivateInventoryItemCommand(companyId, itemId), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }

      /// <summary>
      /// Change an inventory item's status directly.
      /// </summary>
      [HttpPatch("{itemId:guid}/status")]
      public async Task<IActionResult> ChangeStatus(
          Guid companyId,
          Guid itemId,
          [FromBody] ChangeInventoryItemStatusRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new ChangeInventoryItemStatusCommand(companyId, itemId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }

      /// <summary>
      /// Assign an inventory item to an employee.
      /// </summary>
      [HttpPost("{itemId:guid}/assign")]
      public async Task<IActionResult> AssignItem(
          Guid companyId,
          Guid itemId,
          [FromBody] AssignInventoryItemRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new AssignInventoryItemCommand(companyId, itemId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }

      /// <summary>
      /// Return an inventory item from its current assignee.
      /// </summary>
      [HttpPost("{itemId:guid}/return")]
      public async Task<IActionResult> ReturnItem(
          Guid companyId,
          Guid itemId,
          [FromBody] ReturnInventoryItemRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new ReturnInventoryItemCommand(companyId, itemId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }

      /// <summary>
      /// Get assignment history for an inventory item (paginated).
      /// </summary>
      [HttpGet("{itemId:guid}/assignments")]
      public async Task<IActionResult> GetAssignments(
          Guid companyId,
          Guid itemId,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 10,
          CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(
              new GetInventoryItemAssignmentsQuery(companyId, itemId, page, pageSize),
              cancellationToken);
          return Ok(result);
      }
  }
  ```
  - **Note**: Route is nested under company: `/api/v1/companies/{companyId}/inventory`. This enforces company context in every request.
  - **Note**: Follows same controller pattern as `ProvidersController` — inject `IMediator`, use `Send()`, return appropriate status codes.

---

## Step 9: Build, Verify & Manual Test

- [ ] Run `dotnet build` — ensure entire solution compiles
- [ ] Apply migration: `dotnet ef database update -s Allocore.API -p Allocore.Infrastructure`
- [ ] Run application and verify Swagger shows all new endpoints
- [ ] Manual test via Swagger:
  1. Create an inventory item → 201
  2. Get item by ID → 200 with details
  3. List items (paginated) → 200
  4. Update item → 200
  5. Change status to UnderMaintenance → 204
  6. Assign item to employee → 204
  7. Return item from employee → 204
  8. Get assignment history → 200
  9. Deactivate item → 204
  10. Verify duplicate InternalCode returns 400
  11. Verify duplicate SerialNumber returns 400
  12. Verify assigning a Decommissioned item returns 400
  13. Verify wrong company returns error

---

## Technical Details

### Dependencies

No new NuGet packages required beyond US003.

### Project Structure — Affected Files

| Layer | File | Change |
|-------|------|--------|
| **Domain** | `Allocore.Domain/Entities/Inventory/InventoryCategory.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Inventory/InventoryItemStatus.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Inventory/InventoryItemCondition.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Inventory/InventoryItem.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Inventory/InventoryAssignment.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Configurations/InventoryItemConfiguration.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Configurations/InventoryAssignmentConfiguration.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs` | **Update** — add DbSets |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Repositories/InventoryItemRepository.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/DependencyInjection.cs` | **Update** — add registration |
| **Application** | `Allocore.Application/Abstractions/Persistence/IInventoryItemRepository.cs` | **Create** |
| **Application** | `Allocore.Application/Features/Inventory/DTOs/*.cs` | **Create** (8 files) |
| **Application** | `Allocore.Application/Features/Inventory/Validators/*.cs` | **Create** (5 files) |
| **Application** | `Allocore.Application/Features/Inventory/Commands/*.cs` | **Create** (12 files — 6 commands + 6 handlers) |
| **Application** | `Allocore.Application/Features/Inventory/Queries/*.cs` | **Create** (6 files — 3 queries + 3 handlers) |
| **API** | `Allocore.API/Controllers/v1/InventoryController.cs` | **Create** |

### Database

**Table: InventoryItems**

| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| Id | uuid | NO | gen | PK |
| CompanyId | uuid | NO | — | FK → Companies |
| Name | varchar(200) | NO | — | |
| Description | varchar(2000) | YES | — | |
| Category | varchar(50) | NO | — | |
| Brand | varchar(100) | YES | — | |
| Model | varchar(100) | YES | — | |
| SerialNumber | varchar(100) | YES | — | |
| Barcode | varchar(100) | YES | — | |
| InternalCode | varchar(50) | YES | — | |
| Status | varchar(50) | NO | — | |
| Condition | varchar(50) | NO | — | |
| PurchaseDate | timestamp | YES | — | |
| PurchasePrice | decimal(18,2) | YES | — | |
| Currency | varchar(3) | YES | — | |
| InvoiceNumber | varchar(100) | YES | — | |
| WarrantyExpirationDate | timestamp | YES | — | |
| Notes | varchar(2000) | YES | — | |
| IsActive | boolean | NO | true | |
| CreatedAt | timestamp | NO | — | |
| UpdatedAt | timestamp | YES | — | |

**Indexes:**
- `IX_InventoryItems_CompanyId_InternalCode` (UNIQUE, filtered: InternalCode IS NOT NULL)
- `IX_InventoryItems_CompanyId_SerialNumber` (UNIQUE, filtered: SerialNumber IS NOT NULL)
- `IX_InventoryItems_CompanyId`
- `IX_InventoryItems_Category`
- `IX_InventoryItems_Status`

**Table: InventoryAssignments**

| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| Id | uuid | NO | gen | PK |
| InventoryItemId | uuid | NO | — | FK → InventoryItems (CASCADE) |
| EmployeeId | uuid | NO | — | (no FK — validated at app layer) |
| AssignedAt | timestamp | NO | — | |
| ReturnedAt | timestamp | YES | — | |
| ConditionAtAssignment | varchar(50) | NO | — | |
| ConditionAtReturn | varchar(50) | YES | — | |
| Notes | varchar(2000) | YES | — | |
| CreatedAt | timestamp | NO | — | |
| UpdatedAt | timestamp | YES | — | |

**Indexes:**
- `IX_InventoryAssignments_InventoryItemId`
- `IX_InventoryAssignments_EmployeeId`

### API Contract

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/v1/companies/{companyId}/inventory` | GET | Yes | List items (paginated, filterable) |
| `/api/v1/companies/{companyId}/inventory/{itemId}` | GET | Yes | Get item with assignment history |
| `/api/v1/companies/{companyId}/inventory` | POST | Yes | Create item |
| `/api/v1/companies/{companyId}/inventory/{itemId}` | PUT | Yes | Update item details |
| `/api/v1/companies/{companyId}/inventory/{itemId}/deactivate` | PATCH | Yes | Soft-delete item |
| `/api/v1/companies/{companyId}/inventory/{itemId}/status` | PATCH | Yes | Change status directly |
| `/api/v1/companies/{companyId}/inventory/{itemId}/assign` | POST | Yes | Assign to employee |
| `/api/v1/companies/{companyId}/inventory/{itemId}/return` | POST | Yes | Return from employee |
| `/api/v1/companies/{companyId}/inventory/{itemId}/assignments` | GET | Yes | Assignment history (paginated) |

### Authentication/Authorization

- All endpoints require JWT Bearer authentication
- All endpoints verify user has access to the specified company via `UserCompany` relationship
- Fine-grained role checks (Viewer = read-only, Manager = write, Owner = full) deferred to a future authorization story

---

## Acceptance Criteria

- [ ] Authenticated users can create inventory items within their companies
- [ ] Inventory items are company-scoped — no cross-tenant data leakage
- [ ] InternalCode is unique within a company (nullable, filtered unique index)
- [ ] SerialNumber is unique within a company (nullable, filtered unique index)
- [ ] Items can be assigned to employees with full assignment history tracked
- [ ] Only one active assignment per item at a time
- [ ] Cannot assign items with status Decommissioned, Lost, or Disposed
- [ ] Assigning sets status to InUse; returning sets status to Available and updates condition
- [ ] Items support status changes (Available, InUse, UnderMaintenance, Decommissioned, Lost, Disposed)
- [ ] Items support condition tracking (New, Good, Fair, Poor, Damaged)
- [ ] Items can be listed with pagination, category/status/condition filters, and search
- [ ] Items can be soft-deleted (deactivated) rather than hard-deleted
- [ ] Assignment history can be queried with pagination
- [ ] Migrations created and applied (`AddInventory`)
- [ ] `dotnet build` passes without errors
- [ ] Swagger displays all new endpoints

---

## What is explicitly NOT changing?

- **Authentication/Authorization model** — no new roles or policies added
- **Company entity** — no changes to Company or UserCompany
- **User entity** — no changes
- **Provider entity** — no changes to Provider or ProviderContact
- **Existing endpoints** — no modifications to Ping, Auth, Companies, or Providers controllers
- **Employee entity** — EmployeeId is used as plain Guid, no dependency on Employee infrastructure

---

## Follow-ups (Intentionally Deferred)

| Item | Reason | Related Story |
|------|--------|---------------|
| Depreciation tracking (calculate current value over time) | Requires accounting logic, complex business rules | Future US |
| Bulk import from CSV/Excel | Convenience feature, not core | Future US |
| QR code / barcode scanning integration | Frontend feature, requires camera access | Future US |
| Photo attachments per item | Requires file storage infrastructure (S3/blob) | Future US |
| Employee validation at assignment time | Requires US011 (Employees) to be implemented first | US011 |
| Reports: items per employee, items by status, total asset value | Requires reporting infrastructure | EPIC 7 |
| Audit log for status changes | Requires event sourcing or audit trail infrastructure | Future US |
| Location tracking (which office/room an item is in) | Additional dimension, not core for MVP | Future US |
