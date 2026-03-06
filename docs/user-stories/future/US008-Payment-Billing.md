# US008 – Payment & Billing Domain

## Description

**As** an Admin user managing a company in Allocore,
**I need** to register and manage payments to providers (recurring and one-off), track due dates, amounts, and payment statuses,
**So that** I can have full visibility into my provider-related financial obligations, know what is pending, overdue, or paid, and maintain an auditable payment history with supporting documents attached.

Currently, Allocore tracks providers and contracts but has no concept of payments or billing. This story introduces the Payment, RecurringPayment, and PaymentAttachment entities (all company-scoped), full CRUD operations, status lifecycle management, and file attachment support. Payments are always linked to a Provider (ProviderId is required).

**Priority**: High
**Dependencies**: US004 – Provider Management (ProviderId FK required)

---

## Step 1: Domain Layer — Enums

### 1.1 Create PaymentStatus enum

- [ ] Create `Allocore.Domain/Entities/Payments/PaymentStatus.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Payments;

  public enum PaymentStatus
  {
      Pending = 0,
      Paid = 1,
      Overdue = 2,
      Cancelled = 3,
      Disputed = 4
  }
  ```
  - **Business rule**: Default status for new payments is `Pending`.
  - **Business rule**: Valid transitions: Pending→Paid, Pending→Overdue, Pending→Cancelled, Overdue→Paid, Overdue→Cancelled, Paid→Disputed, Disputed→Paid, Disputed→Cancelled.

### 1.2 Create PaymentMethod enum

- [ ] Create `Allocore.Domain/Entities/Payments/PaymentMethod.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Payments;

  public enum PaymentMethod
  {
      BankSlip = 0,
      BankTransfer = 1,
      CreditCard = 2,
      Pix = 3,
      DebitCard = 4,
      Other = 5
  }
  ```

### 1.3 Create BillingFrequency enum (shared)

- [ ] Create `Allocore.Domain/Common/BillingFrequency.cs`:
  ```csharp
  namespace Allocore.Domain.Common;

  public enum BillingFrequency
  {
      Monthly = 0,
      Quarterly = 1,
      SemiAnnual = 2,
      Annual = 3,
      OneOff = 4
  }
  ```
  - **Note**: This enum is shared between US005 (Contracts) and US008 (Payments). Lives in `Domain/Common/` to avoid cross-dependency between `Entities/Contracts/` and `Entities/Payments/`.
  - **Note**: If US005 was implemented first and has `BillingFrequency` in `Entities/Contracts/`, move it to `Domain/Common/` and update the US005 using statements. If US008 is implemented first, US005 should reference this shared location.

---

## Step 2: Domain Layer — RecurringPayment Entity

### 2.1 Create RecurringPayment entity

- [ ] Create `Allocore.Domain/Entities/Payments/RecurringPayment.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Payments;

  using Allocore.Domain.Common;

  public class RecurringPayment : Entity
  {
      public Guid CompanyId { get; private set; }
      public Guid ProviderId { get; private set; }
      public string Description { get; private set; } = string.Empty;
      public decimal ExpectedAmount { get; private set; }
      public string Currency { get; private set; } = "BRL";
      public BillingFrequency Frequency { get; private set; }
      public int DayOfMonth { get; private set; }
      public DateTime StartDate { get; private set; }
      public DateTime? EndDate { get; private set; }
      public PaymentMethod PaymentMethod { get; private set; }
      public bool IsActive { get; private set; } = true;
      public string? Notes { get; private set; }

      private readonly List<Payment> _payments = new();
      public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

      private RecurringPayment() { } // EF Core

      public static RecurringPayment Create(
          Guid companyId,
          Guid providerId,
          string description,
          decimal expectedAmount,
          string currency,
          BillingFrequency frequency,
          int dayOfMonth,
          DateTime startDate,
          DateTime? endDate,
          PaymentMethod paymentMethod,
          string? notes = null)
      {
          if (dayOfMonth < 1 || dayOfMonth > 28)
              throw new ArgumentOutOfRangeException(nameof(dayOfMonth), "Day of month must be between 1 and 28.");

          if (endDate.HasValue && endDate.Value <= startDate)
              throw new ArgumentException("End date must be after start date.", nameof(endDate));

          return new RecurringPayment
          {
              CompanyId = companyId,
              ProviderId = providerId,
              Description = description,
              ExpectedAmount = expectedAmount,
              Currency = currency,
              Frequency = frequency,
              DayOfMonth = dayOfMonth,
              StartDate = startDate,
              EndDate = endDate,
              PaymentMethod = paymentMethod,
              Notes = notes,
              IsActive = true
          };
      }

      public void Update(
          string description,
          decimal expectedAmount,
          string currency,
          BillingFrequency frequency,
          int dayOfMonth,
          DateTime startDate,
          DateTime? endDate,
          PaymentMethod paymentMethod,
          string? notes)
      {
          if (dayOfMonth < 1 || dayOfMonth > 28)
              throw new ArgumentOutOfRangeException(nameof(dayOfMonth), "Day of month must be between 1 and 28.");

          if (endDate.HasValue && endDate.Value <= startDate)
              throw new ArgumentException("End date must be after start date.", nameof(endDate));

          Description = description;
          ExpectedAmount = expectedAmount;
          Currency = currency;
          Frequency = frequency;
          DayOfMonth = dayOfMonth;
          StartDate = startDate;
          EndDate = endDate;
          PaymentMethod = paymentMethod;
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
  }
  ```
  - **Business rule**: `CompanyId` and `ProviderId` are required and immutable after creation.
  - **Business rule**: `DayOfMonth` must be 1–28 (avoids month-length edge cases with 29/30/31).
  - **Business rule**: `EndDate` must be after `StartDate` if provided.
  - **Business rule**: `Description` is required, max 500 chars.
  - **Business rule**: `Currency` is a 3-char ISO code, defaults to "BRL".
  - **Note**: Follows same pattern as `Provider.cs` — private constructor, static `Create()`, mutation methods.

---

## Step 3: Domain Layer — Payment & PaymentAttachment Entities

### 3.1 Create Payment entity

- [ ] Create `Allocore.Domain/Entities/Payments/Payment.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Payments;

  using Allocore.Domain.Common;

  public class Payment : Entity
  {
      public Guid CompanyId { get; private set; }
      public Guid ProviderId { get; private set; }
      public Guid? RecurringPaymentId { get; private set; }
      public string Description { get; private set; } = string.Empty;
      public decimal Amount { get; private set; }
      public string Currency { get; private set; } = "BRL";
      public DateTime DueDate { get; private set; }
      public DateTime? PaidDate { get; private set; }
      public decimal? PaidAmount { get; private set; }
      public PaymentMethod PaymentMethod { get; private set; }
      public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
      public string? ReferenceNumber { get; private set; }
      public string? InvoiceNumber { get; private set; }
      public string? Notes { get; private set; }
      public bool IsActive { get; private set; } = true;

      private readonly List<PaymentAttachment> _attachments = new();
      public IReadOnlyCollection<PaymentAttachment> Attachments => _attachments.AsReadOnly();

      // Navigation
      public RecurringPayment? RecurringPayment { get; private set; }

      private Payment() { } // EF Core

      public static Payment Create(
          Guid companyId,
          Guid providerId,
          string description,
          decimal amount,
          string currency,
          DateTime dueDate,
          PaymentMethod paymentMethod,
          Guid? recurringPaymentId = null,
          string? referenceNumber = null,
          string? invoiceNumber = null,
          string? notes = null)
      {
          return new Payment
          {
              CompanyId = companyId,
              ProviderId = providerId,
              RecurringPaymentId = recurringPaymentId,
              Description = description,
              Amount = amount,
              Currency = currency,
              DueDate = dueDate,
              PaymentMethod = paymentMethod,
              Status = PaymentStatus.Pending,
              ReferenceNumber = referenceNumber,
              InvoiceNumber = invoiceNumber,
              Notes = notes,
              IsActive = true
          };
      }

      public void Update(
          string description,
          decimal amount,
          string currency,
          DateTime dueDate,
          PaymentMethod paymentMethod,
          string? referenceNumber,
          string? invoiceNumber,
          string? notes)
      {
          if (Status == PaymentStatus.Cancelled)
              throw new InvalidOperationException("Cannot update a cancelled payment.");

          Description = description;
          Amount = amount;
          Currency = currency;
          DueDate = dueDate;
          PaymentMethod = paymentMethod;
          ReferenceNumber = referenceNumber;
          InvoiceNumber = invoiceNumber;
          Notes = notes;
          UpdatedAt = DateTime.UtcNow;
      }

      public void MarkAsPaid(DateTime paidDate, decimal paidAmount, string? referenceNumber = null)
      {
          if (Status == PaymentStatus.Cancelled)
              throw new InvalidOperationException("Cannot mark a cancelled payment as paid.");

          Status = PaymentStatus.Paid;
          PaidDate = paidDate;
          PaidAmount = paidAmount;
          if (referenceNumber != null)
              ReferenceNumber = referenceNumber;
          UpdatedAt = DateTime.UtcNow;
      }

      public void MarkAsOverdue()
      {
          if (Status != PaymentStatus.Pending)
              throw new InvalidOperationException("Only pending payments can be marked as overdue.");

          Status = PaymentStatus.Overdue;
          UpdatedAt = DateTime.UtcNow;
      }

      public void Cancel(string? reason = null)
      {
          if (Status == PaymentStatus.Cancelled)
              throw new InvalidOperationException("Payment is already cancelled.");

          Status = PaymentStatus.Cancelled;
          if (reason != null)
              Notes = string.IsNullOrEmpty(Notes) ? $"Cancelled: {reason}" : $"{Notes}\nCancelled: {reason}";
          UpdatedAt = DateTime.UtcNow;
      }

      public void Dispute(string? reason = null)
      {
          if (Status != PaymentStatus.Paid)
              throw new InvalidOperationException("Only paid payments can be disputed.");

          Status = PaymentStatus.Disputed;
          if (reason != null)
              Notes = string.IsNullOrEmpty(Notes) ? $"Disputed: {reason}" : $"{Notes}\nDisuted: {reason}";
          UpdatedAt = DateTime.UtcNow;
      }

      public void AddAttachment(PaymentAttachment attachment)
      {
          _attachments.Add(attachment);
          UpdatedAt = DateTime.UtcNow;
      }

      public void RemoveAttachment(PaymentAttachment attachment)
      {
          _attachments.Remove(attachment);
          UpdatedAt = DateTime.UtcNow;
      }
  }
  ```
  - **Business rule**: `CompanyId` and `ProviderId` are required and immutable after creation.
  - **Business rule**: `RecurringPaymentId` is nullable — one-off payments have no recurring parent.
  - **Business rule**: Status transitions enforced by domain methods: Pending→Paid, Pending→Overdue, Pending→Cancelled, Overdue→Paid, Overdue→Cancelled, Paid→Disputed, Disputed→Paid, Disputed→Cancelled.
  - **Business rule**: `PaidAmount` can differ from `Amount` (partial payments, interest, fees).
  - **Business rule**: `InvoiceNumber` must be unique within a company (enforced at DB level with filtered index, nullable).
  - **Business rule**: When a Payment is linked to a RecurringPayment, their `ProviderId` must match (enforced at application layer in commands).
  - **Note**: Overdue detection is done at query time (`DueDate < now && Status == Pending`), not via background jobs — deferred to a future story.

### 3.2 Create PaymentAttachment entity

- [ ] Create `Allocore.Domain/Entities/Payments/PaymentAttachment.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Payments;

  using Allocore.Domain.Common;

  public class PaymentAttachment : Entity
  {
      public Guid PaymentId { get; private set; }
      public string FileName { get; private set; } = string.Empty;
      public string ContentType { get; private set; } = string.Empty;
      public long FileSizeBytes { get; private set; }
      public string StoragePath { get; private set; } = string.Empty;
      public DateTime UploadedAt { get; private set; }

      // Navigation
      public Payment? Payment { get; private set; }

      private PaymentAttachment() { } // EF Core

      public static PaymentAttachment Create(
          Guid paymentId,
          string fileName,
          string contentType,
          long fileSizeBytes,
          string storagePath)
      {
          return new PaymentAttachment
          {
              PaymentId = paymentId,
              FileName = fileName,
              ContentType = contentType,
              FileSizeBytes = fileSizeBytes,
              StoragePath = storagePath,
              UploadedAt = DateTime.UtcNow
          };
      }
  }
  ```
  - **Business rule**: PaymentAttachment is immutable — no `Update()` method. To change an attachment, remove and re-upload.
  - **Business rule**: `FileName` max 500 chars, `ContentType` max 100 chars, `StoragePath` max 1000 chars.
  - **Note**: Actual file bytes are stored in object storage (S3/MinIO). The `StoragePath` is the key/path in the bucket.

---

## Step 4: Infrastructure Layer — EF Core Configurations

### 4.1 RecurringPayment configuration

- [ ] Create `Allocore.Infrastructure/Persistence/Configurations/RecurringPaymentConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Common;
  using Allocore.Domain.Entities.Payments;

  public class RecurringPaymentConfiguration : IEntityTypeConfiguration<RecurringPayment>
  {
      public void Configure(EntityTypeBuilder<RecurringPayment> builder)
      {
          builder.ToTable("RecurringPayments");

          builder.HasKey(rp => rp.Id);

          builder.Property(rp => rp.CompanyId)
              .IsRequired();

          builder.Property(rp => rp.ProviderId)
              .IsRequired();

          builder.Property(rp => rp.Description)
              .IsRequired()
              .HasMaxLength(500);

          builder.Property(rp => rp.ExpectedAmount)
              .HasPrecision(18, 2)
              .IsRequired();

          builder.Property(rp => rp.Currency)
              .IsRequired()
              .HasMaxLength(3)
              .HasDefaultValue("BRL");

          builder.Property(rp => rp.Frequency)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(rp => rp.DayOfMonth)
              .IsRequired();

          builder.Property(rp => rp.StartDate)
              .IsRequired();

          builder.Property(rp => rp.PaymentMethod)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(rp => rp.IsActive)
              .IsRequired()
              .HasDefaultValue(true);

          builder.Property(rp => rp.Notes)
              .HasMaxLength(2000);

          // FK to Provider (restrict — cannot delete provider with recurring payments)
          builder.HasOne<Allocore.Domain.Entities.Providers.Provider>()
              .WithMany()
              .HasForeignKey(rp => rp.ProviderId)
              .OnDelete(DeleteBehavior.Restrict);

          builder.HasMany(rp => rp.Payments)
              .WithOne(p => p.RecurringPayment)
              .HasForeignKey(p => p.RecurringPaymentId)
              .OnDelete(DeleteBehavior.SetNull);

          builder.HasIndex(rp => rp.CompanyId);
          builder.HasIndex(rp => rp.ProviderId);
      }
  }
  ```
  - **Note**: FK to Provider uses `Restrict` — cannot delete a provider that has recurring payments. Deactivate the provider instead.
  - **Note**: FK from Payment to RecurringPayment uses `SetNull` — if a recurring payment is deleted, linked payments remain but lose the association.
  - **Note**: Enum properties stored as strings for DB readability, same pattern as US004 `ProviderCategory`.

### 4.2 Payment configuration

- [ ] Create `Allocore.Infrastructure/Persistence/Configurations/PaymentConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.Payments;

  public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
  {
      public void Configure(EntityTypeBuilder<Payment> builder)
      {
          builder.ToTable("Payments");

          builder.HasKey(p => p.Id);

          builder.Property(p => p.CompanyId)
              .IsRequired();

          builder.Property(p => p.ProviderId)
              .IsRequired();

          builder.Property(p => p.Description)
              .IsRequired()
              .HasMaxLength(500);

          builder.Property(p => p.Amount)
              .HasPrecision(18, 2)
              .IsRequired();

          builder.Property(p => p.Currency)
              .IsRequired()
              .HasMaxLength(3)
              .HasDefaultValue("BRL");

          builder.Property(p => p.DueDate)
              .IsRequired();

          builder.Property(p => p.PaidAmount)
              .HasPrecision(18, 2);

          builder.Property(p => p.PaymentMethod)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(p => p.Status)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired()
              .HasDefaultValue(PaymentStatus.Pending);

          builder.Property(p => p.ReferenceNumber)
              .HasMaxLength(100);

          builder.Property(p => p.InvoiceNumber)
              .HasMaxLength(100);

          builder.Property(p => p.Notes)
              .HasMaxLength(2000);

          builder.Property(p => p.IsActive)
              .IsRequired()
              .HasDefaultValue(true);

          // FK to Provider (restrict — cannot delete provider with payments)
          builder.HasOne<Allocore.Domain.Entities.Providers.Provider>()
              .WithMany()
              .HasForeignKey(p => p.ProviderId)
              .OnDelete(DeleteBehavior.Restrict);

          // FK to RecurringPayment (set null — payments survive if recurring parent is deleted)
          builder.HasOne(p => p.RecurringPayment)
              .WithMany(rp => rp.Payments)
              .HasForeignKey(p => p.RecurringPaymentId)
              .OnDelete(DeleteBehavior.SetNull);

          builder.HasMany(p => p.Attachments)
              .WithOne(a => a.Payment)
              .HasForeignKey(a => a.PaymentId)
              .OnDelete(DeleteBehavior.Cascade);

          // Filtered unique index on InvoiceNumber within a company (only non-null values)
          builder.HasIndex(p => new { p.CompanyId, p.InvoiceNumber })
              .IsUnique()
              .HasFilter("\"InvoiceNumber\" IS NOT NULL");

          builder.HasIndex(p => p.CompanyId);
          builder.HasIndex(p => p.ProviderId);
          builder.HasIndex(p => p.Status);
          builder.HasIndex(p => p.DueDate);
      }
  }
  ```
  - **Note**: Filtered unique index on `(CompanyId, InvoiceNumber)` ensures invoice numbers are unique per company but allows multiple NULL values.
  - **Note**: Multiple indexes on Status and DueDate support the overdue payment query and filtered listing.

### 4.3 PaymentAttachment configuration

- [ ] Create `Allocore.Infrastructure/Persistence/Configurations/PaymentAttachmentConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.Payments;

  public class PaymentAttachmentConfiguration : IEntityTypeConfiguration<PaymentAttachment>
  {
      public void Configure(EntityTypeBuilder<PaymentAttachment> builder)
      {
          builder.ToTable("PaymentAttachments");

          builder.HasKey(a => a.Id);

          builder.Property(a => a.PaymentId)
              .IsRequired();

          builder.Property(a => a.FileName)
              .IsRequired()
              .HasMaxLength(500);

          builder.Property(a => a.ContentType)
              .IsRequired()
              .HasMaxLength(100);

          builder.Property(a => a.FileSizeBytes)
              .IsRequired();

          builder.Property(a => a.StoragePath)
              .IsRequired()
              .HasMaxLength(1000);

          builder.Property(a => a.UploadedAt)
              .IsRequired();

          builder.HasOne(a => a.Payment)
              .WithMany(p => p.Attachments)
              .HasForeignKey(a => a.PaymentId)
              .OnDelete(DeleteBehavior.Cascade);

          builder.HasIndex(a => a.PaymentId);
      }
  }
  ```

### 4.4 Update ApplicationDbContext

- [ ] Update `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs` — add DbSets:
  ```csharp
  // Add these using statements
  using Allocore.Domain.Entities.Payments;

  // Add these DbSets
  public DbSet<RecurringPayment> RecurringPayments => Set<RecurringPayment>();
  public DbSet<Payment> Payments => Set<Payment>();
  public DbSet<PaymentAttachment> PaymentAttachments => Set<PaymentAttachment>();
  ```
  - **Note**: `OnModelCreating` already calls `ApplyConfigurationsFromAssembly` so the new configurations will be auto-discovered.

### 4.5 Create migration

- [ ] Run migration:
  ```bash
  dotnet ef migrations add AddPayments -s Allocore.API -p Allocore.Infrastructure
  ```
  - **Impact on existing data**: No existing rows affected. Three new tables created.

---

## Step 5: Infrastructure Layer — Repositories & Services

### 5.1 Create IPaymentRepository interface

- [ ] Create `Allocore.Application/Abstractions/Persistence/IPaymentRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;

  using Allocore.Domain.Entities.Payments;

  public interface IPaymentRepository : IReadRepository<Payment>, IWriteRepository<Payment>
  {
      Task<Payment?> GetByIdWithAttachmentsAsync(Guid id, CancellationToken cancellationToken = default);
      Task<bool> ExistsByInvoiceNumberInCompanyAsync(Guid companyId, string invoiceNumber, CancellationToken cancellationToken = default);
      Task<bool> ExistsByInvoiceNumberInCompanyExcludingAsync(Guid companyId, string invoiceNumber, Guid excludePaymentId, CancellationToken cancellationToken = default);
      Task<(IEnumerable<Payment> Payments, int TotalCount)> GetPagedByCompanyAsync(
          Guid companyId, int page, int pageSize,
          PaymentStatus? statusFilter = null,
          Guid? providerFilter = null,
          DateTime? dueDateFrom = null,
          DateTime? dueDateTo = null,
          string? searchTerm = null,
          CancellationToken cancellationToken = default);
      Task<IEnumerable<Payment>> GetOverdueByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
  }
  ```
  - **Note**: Interface lives in Application layer (same pattern as `IProviderRepository`).
  - **Note**: `GetByIdWithAttachmentsAsync` eagerly loads attachments for detail views.
  - **Note**: `GetOverdueByCompanyAsync` returns payments where `DueDate < DateTime.UtcNow && Status == Pending`.

### 5.2 Create PaymentRepository implementation

- [ ] Create `Allocore.Infrastructure/Persistence/Repositories/PaymentRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Repositories;

  using Microsoft.EntityFrameworkCore;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Entities.Payments;

  public class PaymentRepository : IPaymentRepository
  {
      private readonly ApplicationDbContext _context;

      public PaymentRepository(ApplicationDbContext context)
      {
          _context = context;
      }

      public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

      public async Task<Payment?> GetByIdWithAttachmentsAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.Payments
              .Include(p => p.Attachments)
              .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

      public async Task<bool> ExistsByInvoiceNumberInCompanyAsync(Guid companyId, string invoiceNumber, CancellationToken cancellationToken = default)
          => await _context.Payments.AnyAsync(
              p => p.CompanyId == companyId && p.InvoiceNumber == invoiceNumber,
              cancellationToken);

      public async Task<bool> ExistsByInvoiceNumberInCompanyExcludingAsync(Guid companyId, string invoiceNumber, Guid excludePaymentId, CancellationToken cancellationToken = default)
          => await _context.Payments.AnyAsync(
              p => p.CompanyId == companyId && p.InvoiceNumber == invoiceNumber && p.Id != excludePaymentId,
              cancellationToken);

      public async Task<(IEnumerable<Payment> Payments, int TotalCount)> GetPagedByCompanyAsync(
          Guid companyId, int page, int pageSize,
          PaymentStatus? statusFilter = null,
          Guid? providerFilter = null,
          DateTime? dueDateFrom = null,
          DateTime? dueDateTo = null,
          string? searchTerm = null,
          CancellationToken cancellationToken = default)
      {
          var query = _context.Payments
              .Include(p => p.Attachments)
              .Where(p => p.CompanyId == companyId);

          if (statusFilter.HasValue)
              query = query.Where(p => p.Status == statusFilter.Value);

          if (providerFilter.HasValue)
              query = query.Where(p => p.ProviderId == providerFilter.Value);

          if (dueDateFrom.HasValue)
              query = query.Where(p => p.DueDate >= dueDateFrom.Value);

          if (dueDateTo.HasValue)
              query = query.Where(p => p.DueDate <= dueDateTo.Value);

          if (!string.IsNullOrWhiteSpace(searchTerm))
          {
              var term = searchTerm.ToLowerInvariant();
              query = query.Where(p =>
                  p.Description.ToLower().Contains(term) ||
                  (p.InvoiceNumber != null && p.InvoiceNumber.ToLower().Contains(term)) ||
                  (p.ReferenceNumber != null && p.ReferenceNumber.ToLower().Contains(term)));
          }

          var totalCount = await query.CountAsync(cancellationToken);
          var payments = await query
              .OrderByDescending(p => p.DueDate)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync(cancellationToken);

          return (payments, totalCount);
      }

      public async Task<IEnumerable<Payment>> GetOverdueByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
          => await _context.Payments
              .Include(p => p.Attachments)
              .Where(p => p.CompanyId == companyId
                  && p.Status == PaymentStatus.Pending
                  && p.DueDate < DateTime.UtcNow)
              .OrderBy(p => p.DueDate)
              .ToListAsync(cancellationToken);

      public async Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
          => await _context.Payments.ToListAsync(cancellationToken);

      public async Task<Payment> AddAsync(Payment entity, CancellationToken cancellationToken = default)
      {
          await _context.Payments.AddAsync(entity, cancellationToken);
          return entity;
      }

      public Task UpdateAsync(Payment entity, CancellationToken cancellationToken = default)
      {
          _context.Payments.Update(entity);
          return Task.CompletedTask;
      }

      public Task DeleteAsync(Payment entity, CancellationToken cancellationToken = default)
      {
          _context.Payments.Remove(entity);
          return Task.CompletedTask;
      }
  }
  ```
  - **Note**: Follows exact same pattern as `ProviderRepository`.
  - **Note**: `GetOverdueByCompanyAsync` detects overdue at query time — no background job needed.

### 5.3 Create IRecurringPaymentRepository interface

- [ ] Create `Allocore.Application/Abstractions/Persistence/IRecurringPaymentRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;

  using Allocore.Domain.Common;
  using Allocore.Domain.Entities.Payments;

  public interface IRecurringPaymentRepository : IReadRepository<RecurringPayment>, IWriteRepository<RecurringPayment>
  {
      Task<RecurringPayment?> GetByIdWithPaymentsAsync(Guid id, CancellationToken cancellationToken = default);
      Task<(IEnumerable<RecurringPayment> RecurringPayments, int TotalCount)> GetPagedByCompanyAsync(
          Guid companyId, int page, int pageSize,
          Guid? providerFilter = null,
          bool? isActiveFilter = null,
          CancellationToken cancellationToken = default);
      Task<RecurringPayment?> FindMatchingAsync(
          Guid companyId, Guid providerId, decimal amount, BillingFrequency? frequency,
          CancellationToken cancellationToken = default);
  }
  ```
  - **Note**: `FindMatchingAsync` is used by US009 to reconcile incoming email payments with existing recurring schedules.

### 5.4 Create RecurringPaymentRepository implementation

- [ ] Create `Allocore.Infrastructure/Persistence/Repositories/RecurringPaymentRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Repositories;

  using Microsoft.EntityFrameworkCore;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Common;
  using Allocore.Domain.Entities.Payments;

  public class RecurringPaymentRepository : IRecurringPaymentRepository
  {
      private readonly ApplicationDbContext _context;

      public RecurringPaymentRepository(ApplicationDbContext context)
      {
          _context = context;
      }

      public async Task<RecurringPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.RecurringPayments.FirstOrDefaultAsync(rp => rp.Id == id, cancellationToken);

      public async Task<RecurringPayment?> GetByIdWithPaymentsAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.RecurringPayments
              .Include(rp => rp.Payments)
              .FirstOrDefaultAsync(rp => rp.Id == id, cancellationToken);

      public async Task<(IEnumerable<RecurringPayment> RecurringPayments, int TotalCount)> GetPagedByCompanyAsync(
          Guid companyId, int page, int pageSize,
          Guid? providerFilter = null,
          bool? isActiveFilter = null,
          CancellationToken cancellationToken = default)
      {
          var query = _context.RecurringPayments
              .Where(rp => rp.CompanyId == companyId);

          if (providerFilter.HasValue)
              query = query.Where(rp => rp.ProviderId == providerFilter.Value);

          if (isActiveFilter.HasValue)
              query = query.Where(rp => rp.IsActive == isActiveFilter.Value);

          var totalCount = await query.CountAsync(cancellationToken);
          var recurringPayments = await query
              .OrderBy(rp => rp.Description)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync(cancellationToken);

          return (recurringPayments, totalCount);
      }

      public async Task<RecurringPayment?> FindMatchingAsync(
          Guid companyId, Guid providerId, decimal amount, BillingFrequency? frequency,
          CancellationToken cancellationToken = default)
      {
          var query = _context.RecurringPayments
              .Where(rp => rp.CompanyId == companyId
                  && rp.ProviderId == providerId
                  && rp.IsActive
                  && rp.ExpectedAmount == amount);

          if (frequency.HasValue)
              query = query.Where(rp => rp.Frequency == frequency.Value);

          return await query.FirstOrDefaultAsync(cancellationToken);
      }

      public async Task<IEnumerable<RecurringPayment>> GetAllAsync(CancellationToken cancellationToken = default)
          => await _context.RecurringPayments.ToListAsync(cancellationToken);

      public async Task<RecurringPayment> AddAsync(RecurringPayment entity, CancellationToken cancellationToken = default)
      {
          await _context.RecurringPayments.AddAsync(entity, cancellationToken);
          return entity;
      }

      public Task UpdateAsync(RecurringPayment entity, CancellationToken cancellationToken = default)
      {
          _context.RecurringPayments.Update(entity);
          return Task.CompletedTask;
      }

      public Task DeleteAsync(RecurringPayment entity, CancellationToken cancellationToken = default)
      {
          _context.RecurringPayments.Remove(entity);
          return Task.CompletedTask;
      }
  }
  ```

### 5.5 Create IFileStorageService abstraction

- [ ] Create `Allocore.Application/Abstractions/Services/IFileStorageService.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Services;

  public interface IFileStorageService
  {
      Task<string> UploadAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default);
      Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default);
      Task DeleteAsync(string path, CancellationToken cancellationToken = default);
      Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);
  }
  ```
  - **Note**: Abstraction only — no implementation in this story. US009 provides the real `S3FileStorageService`.
  - **Note**: `UploadAsync` returns the final storage path (may differ from input if the service adds prefixes/UUIDs).

### 5.6 Create stub IFileStorageService implementation

- [ ] Create `Allocore.Infrastructure/Services/NotImplementedFileStorageService.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Services;

  using Allocore.Application.Abstractions.Services;

  public class NotImplementedFileStorageService : IFileStorageService
  {
      public Task<string> UploadAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default)
          => throw new NotImplementedException("File storage is not configured. See US009 for S3/MinIO implementation.");

      public Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
          => throw new NotImplementedException("File storage is not configured. See US009 for S3/MinIO implementation.");

      public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
          => throw new NotImplementedException("File storage is not configured. See US009 for S3/MinIO implementation.");

      public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
          => throw new NotImplementedException("File storage is not configured. See US009 for S3/MinIO implementation.");
  }
  ```
  - **Note**: This is a placeholder. Attachment upload endpoints will exist but return 500 until US009 provides the real implementation. This is acceptable for an evolving app.

### 5.7 Register in DI

- [ ] Update `Allocore.Infrastructure/DependencyInjection.cs`:
  ```csharp
  // Add registrations
  services.AddScoped<IPaymentRepository, PaymentRepository>();
  services.AddScoped<IRecurringPaymentRepository, RecurringPaymentRepository>();
  services.AddScoped<IFileStorageService, NotImplementedFileStorageService>();
  ```

---

## Step 6: Application Layer — DTOs

### 6.1 Create Payment DTOs

- [ ] Create `Allocore.Application/Features/Payments/DTOs/PaymentDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.DTOs;

  public record PaymentDto(
      Guid Id,
      Guid CompanyId,
      Guid ProviderId,
      Guid? RecurringPaymentId,
      string Description,
      decimal Amount,
      string Currency,
      DateTime DueDate,
      DateTime? PaidDate,
      decimal? PaidAmount,
      string PaymentMethod,
      string Status,
      string? ReferenceNumber,
      string? InvoiceNumber,
      string? Notes,
      bool IsActive,
      DateTime CreatedAt,
      DateTime? UpdatedAt,
      IEnumerable<PaymentAttachmentDto> Attachments
  );
  ```

- [ ] Create `Allocore.Application/Features/Payments/DTOs/PaymentListItemDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.DTOs;

  public record PaymentListItemDto(
      Guid Id,
      Guid ProviderId,
      string ProviderName,
      string Description,
      decimal Amount,
      string Currency,
      DateTime DueDate,
      string PaymentMethod,
      string Status,
      string? InvoiceNumber,
      int AttachmentCount
  );
  ```
  - **Note**: Lightweight DTO for list views. Includes `ProviderName` for display without extra lookup.

- [ ] Create `Allocore.Application/Features/Payments/DTOs/PaymentAttachmentDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.DTOs;

  public record PaymentAttachmentDto(
      Guid Id,
      string FileName,
      string ContentType,
      long FileSizeBytes,
      DateTime UploadedAt
  );
  ```
  - **Note**: Does NOT expose `StoragePath` — that is an internal implementation detail.

- [ ] Create `Allocore.Application/Features/Payments/DTOs/CreatePaymentRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.DTOs;

  public record CreatePaymentRequest(
      Guid ProviderId,
      string Description,
      decimal Amount,
      string? Currency,
      DateTime DueDate,
      string PaymentMethod,
      Guid? RecurringPaymentId,
      string? ReferenceNumber,
      string? InvoiceNumber,
      string? Notes
  );
  ```

- [ ] Create `Allocore.Application/Features/Payments/DTOs/UpdatePaymentRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.DTOs;

  public record UpdatePaymentRequest(
      string Description,
      decimal Amount,
      string? Currency,
      DateTime DueDate,
      string PaymentMethod,
      string? ReferenceNumber,
      string? InvoiceNumber,
      string? Notes
  );
  ```

- [ ] Create `Allocore.Application/Features/Payments/DTOs/MarkAsPaidRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.DTOs;

  public record MarkAsPaidRequest(
      DateTime PaidDate,
      decimal PaidAmount,
      string? ReferenceNumber
  );
  ```

- [ ] Create `Allocore.Application/Features/Payments/DTOs/CancelPaymentRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.DTOs;

  public record CancelPaymentRequest(
      string? Reason
  );
  ```

### 6.2 Create RecurringPayment DTOs

- [ ] Create `Allocore.Application/Features/Payments/DTOs/RecurringPaymentDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.DTOs;

  public record RecurringPaymentDto(
      Guid Id,
      Guid CompanyId,
      Guid ProviderId,
      string Description,
      decimal ExpectedAmount,
      string Currency,
      string Frequency,
      int DayOfMonth,
      DateTime StartDate,
      DateTime? EndDate,
      string PaymentMethod,
      bool IsActive,
      string? Notes,
      DateTime CreatedAt,
      DateTime? UpdatedAt,
      IEnumerable<PaymentListItemDto> RecentPayments
  );
  ```
  - **Note**: `RecentPayments` shows the most recent payments linked to this recurring schedule.

- [ ] Create `Allocore.Application/Features/Payments/DTOs/RecurringPaymentListItemDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.DTOs;

  public record RecurringPaymentListItemDto(
      Guid Id,
      Guid ProviderId,
      string ProviderName,
      string Description,
      decimal ExpectedAmount,
      string Currency,
      string Frequency,
      int DayOfMonth,
      bool IsActive,
      int PaymentCount
  );
  ```

- [ ] Create `Allocore.Application/Features/Payments/DTOs/CreateRecurringPaymentRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.DTOs;

  public record CreateRecurringPaymentRequest(
      Guid ProviderId,
      string Description,
      decimal ExpectedAmount,
      string? Currency,
      string Frequency,
      int DayOfMonth,
      DateTime StartDate,
      DateTime? EndDate,
      string PaymentMethod,
      string? Notes
  );
  ```

- [ ] Create `Allocore.Application/Features/Payments/DTOs/UpdateRecurringPaymentRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.DTOs;

  public record UpdateRecurringPaymentRequest(
      string Description,
      decimal ExpectedAmount,
      string? Currency,
      string Frequency,
      int DayOfMonth,
      DateTime StartDate,
      DateTime? EndDate,
      string PaymentMethod,
      string? Notes
  );
  ```

---

## Step 7: Application Layer — Validators & Commands

### 7.1 Create validators

- [ ] Create `Allocore.Application/Features/Payments/Validators/CreatePaymentRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Entities.Payments;

  public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
  {
      public CreatePaymentRequestValidator()
      {
          RuleFor(x => x.ProviderId)
              .NotEmpty().WithMessage("Provider ID is required");

          RuleFor(x => x.Description)
              .NotEmpty().WithMessage("Description is required")
              .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

          RuleFor(x => x.Amount)
              .GreaterThan(0).WithMessage("Amount must be greater than zero");

          RuleFor(x => x.Currency)
              .MaximumLength(3).WithMessage("Currency must be a 3-character ISO code")
              .When(x => !string.IsNullOrEmpty(x.Currency));

          RuleFor(x => x.PaymentMethod)
              .NotEmpty().WithMessage("Payment method is required")
              .Must(pm => Enum.TryParse<PaymentMethod>(pm, true, out _))
              .WithMessage("Payment method must be one of: BankSlip, BankTransfer, CreditCard, Pix, DebitCard, Other");

          RuleFor(x => x.ReferenceNumber)
              .MaximumLength(100).WithMessage("Reference number must not exceed 100 characters")
              .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

          RuleFor(x => x.InvoiceNumber)
              .MaximumLength(100).WithMessage("Invoice number must not exceed 100 characters")
              .When(x => !string.IsNullOrEmpty(x.InvoiceNumber));

          RuleFor(x => x.Notes)
              .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters")
              .When(x => !string.IsNullOrEmpty(x.Notes));
      }
  }
  ```

- [ ] Create `Allocore.Application/Features/Payments/Validators/UpdatePaymentRequestValidator.cs`:
  - Same rules as `CreatePaymentRequestValidator` but without `ProviderId` rule (provider cannot be changed after creation).

- [ ] Create `Allocore.Application/Features/Payments/Validators/MarkAsPaidRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Payments.DTOs;

  public class MarkAsPaidRequestValidator : AbstractValidator<MarkAsPaidRequest>
  {
      public MarkAsPaidRequestValidator()
      {
          RuleFor(x => x.PaidAmount)
              .GreaterThan(0).WithMessage("Paid amount must be greater than zero");

          RuleFor(x => x.ReferenceNumber)
              .MaximumLength(100).WithMessage("Reference number must not exceed 100 characters")
              .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));
      }
  }
  ```

- [ ] Create `Allocore.Application/Features/Payments/Validators/CreateRecurringPaymentRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Common;
  using Allocore.Domain.Entities.Payments;

  public class CreateRecurringPaymentRequestValidator : AbstractValidator<CreateRecurringPaymentRequest>
  {
      public CreateRecurringPaymentRequestValidator()
      {
          RuleFor(x => x.ProviderId)
              .NotEmpty().WithMessage("Provider ID is required");

          RuleFor(x => x.Description)
              .NotEmpty().WithMessage("Description is required")
              .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

          RuleFor(x => x.ExpectedAmount)
              .GreaterThan(0).WithMessage("Expected amount must be greater than zero");

          RuleFor(x => x.Currency)
              .MaximumLength(3).WithMessage("Currency must be a 3-character ISO code")
              .When(x => !string.IsNullOrEmpty(x.Currency));

          RuleFor(x => x.Frequency)
              .NotEmpty().WithMessage("Billing frequency is required")
              .Must(f => Enum.TryParse<BillingFrequency>(f, true, out _))
              .WithMessage("Frequency must be one of: Monthly, Quarterly, SemiAnnual, Annual, OneOff");

          RuleFor(x => x.DayOfMonth)
              .InclusiveBetween(1, 28).WithMessage("Day of month must be between 1 and 28");

          RuleFor(x => x.PaymentMethod)
              .NotEmpty().WithMessage("Payment method is required")
              .Must(pm => Enum.TryParse<PaymentMethod>(pm, true, out _))
              .WithMessage("Payment method must be one of: BankSlip, BankTransfer, CreditCard, Pix, DebitCard, Other");

          RuleFor(x => x.EndDate)
              .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
              .When(x => x.EndDate.HasValue);

          RuleFor(x => x.Notes)
              .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters")
              .When(x => !string.IsNullOrEmpty(x.Notes));
      }
  }
  ```

- [ ] Create `Allocore.Application/Features/Payments/Validators/UpdateRecurringPaymentRequestValidator.cs`:
  - Same rules as `CreateRecurringPaymentRequestValidator` but without `ProviderId` rule.

### 7.2 CreatePayment command

- [ ] Create `Allocore.Application/Features/Payments/Commands/CreatePaymentCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Commands;

  using MediatR;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Common;

  public record CreatePaymentCommand(Guid CompanyId, CreatePaymentRequest Request) : IRequest<Result<PaymentDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Payments/Commands/CreatePaymentCommandHandler.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Commands;

  using MediatR;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Application.Abstractions.Services;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Common;
  using Allocore.Domain.Entities.Payments;

  public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
  {
      private readonly IPaymentRepository _paymentRepository;
      private readonly IRecurringPaymentRepository _recurringPaymentRepository;
      private readonly IProviderRepository _providerRepository;
      private readonly IUserCompanyRepository _userCompanyRepository;
      private readonly ICurrentUser _currentUser;
      private readonly IUnitOfWork _unitOfWork;

      public CreatePaymentCommandHandler(
          IPaymentRepository paymentRepository,
          IRecurringPaymentRepository recurringPaymentRepository,
          IProviderRepository providerRepository,
          IUserCompanyRepository userCompanyRepository,
          ICurrentUser currentUser,
          IUnitOfWork unitOfWork)
      {
          _paymentRepository = paymentRepository;
          _recurringPaymentRepository = recurringPaymentRepository;
          _providerRepository = providerRepository;
          _userCompanyRepository = userCompanyRepository;
          _currentUser = currentUser;
          _unitOfWork = unitOfWork;
      }

      public async Task<Result<PaymentDto>> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
      {
          // Verify user has access to company
          if (!_currentUser.UserId.HasValue)
              return Result<PaymentDto>.Failure("User not authenticated.");

          var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
              _currentUser.UserId.Value, command.CompanyId, cancellationToken);
          if (!hasAccess)
              return Result<PaymentDto>.Failure("You don't have access to this company.");

          // Verify provider exists and belongs to company
          var provider = await _providerRepository.GetByIdAsync(command.Request.ProviderId, cancellationToken);
          if (provider is null || provider.CompanyId != command.CompanyId)
              return Result<PaymentDto>.Failure("Provider not found in this company.");

          // Validate invoice number uniqueness
          if (!string.IsNullOrEmpty(command.Request.InvoiceNumber))
          {
              if (await _paymentRepository.ExistsByInvoiceNumberInCompanyAsync(
                  command.CompanyId, command.Request.InvoiceNumber, cancellationToken))
                  return Result<PaymentDto>.Failure("A payment with this invoice number already exists in this company.");
          }

          // Validate recurring payment link
          if (command.Request.RecurringPaymentId.HasValue)
          {
              var recurring = await _recurringPaymentRepository.GetByIdAsync(
                  command.Request.RecurringPaymentId.Value, cancellationToken);
              if (recurring is null || recurring.CompanyId != command.CompanyId)
                  return Result<PaymentDto>.Failure("Recurring payment not found in this company.");
              if (recurring.ProviderId != command.Request.ProviderId)
                  return Result<PaymentDto>.Failure("Payment provider must match the recurring payment's provider.");
          }

          // Parse payment method
          if (!Enum.TryParse<PaymentMethod>(command.Request.PaymentMethod, true, out var paymentMethod))
              return Result<PaymentDto>.Failure("Invalid payment method.");

          var payment = Payment.Create(
              command.CompanyId,
              command.Request.ProviderId,
              command.Request.Description,
              command.Request.Amount,
              command.Request.Currency ?? "BRL",
              command.Request.DueDate,
              paymentMethod,
              command.Request.RecurringPaymentId,
              command.Request.ReferenceNumber,
              command.Request.InvoiceNumber,
              command.Request.Notes);

          await _paymentRepository.AddAsync(payment, cancellationToken);
          await _unitOfWork.SaveChangesAsync(cancellationToken);

          return Result<PaymentDto>.Success(MapToDto(payment));
      }

      private static PaymentDto MapToDto(Payment payment) => new(
          payment.Id,
          payment.CompanyId,
          payment.ProviderId,
          payment.RecurringPaymentId,
          payment.Description,
          payment.Amount,
          payment.Currency,
          payment.DueDate,
          payment.PaidDate,
          payment.PaidAmount,
          payment.PaymentMethod.ToString(),
          payment.Status.ToString(),
          payment.ReferenceNumber,
          payment.InvoiceNumber,
          payment.Notes,
          payment.IsActive,
          payment.CreatedAt,
          payment.UpdatedAt,
          payment.Attachments.Select(a => new PaymentAttachmentDto(
              a.Id, a.FileName, a.ContentType, a.FileSizeBytes, a.UploadedAt))
      );
  }
  ```
  - **Business rule**: Provider must exist and belong to the same company.
  - **Business rule**: If linking to a RecurringPayment, the `ProviderId` must match.
  - **Business rule**: InvoiceNumber must be unique within the company if provided.
  - **Note**: Follows same authorization pattern as `CreateProviderCommandHandler`.

### 7.3 UpdatePayment command

- [ ] Create `Allocore.Application/Features/Payments/Commands/UpdatePaymentCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Commands;

  using MediatR;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Common;

  public record UpdatePaymentCommand(Guid CompanyId, Guid PaymentId, UpdatePaymentRequest Request) : IRequest<Result<PaymentDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Payments/Commands/UpdatePaymentCommandHandler.cs`:
  - Verify user has access to company
  - Load payment with attachments, verify it belongs to the company (`payment.CompanyId == command.CompanyId`)
  - Check invoice number uniqueness excluding current payment (via `ExistsByInvoiceNumberInCompanyExcludingAsync`)
  - Parse payment method enum
  - Call `payment.Update(...)`, save changes
  - Return updated `PaymentDto`

### 7.4 MarkAsPaid command

- [ ] Create `Allocore.Application/Features/Payments/Commands/MarkAsPaidCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Commands;

  using MediatR;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Common;

  public record MarkAsPaidCommand(Guid CompanyId, Guid PaymentId, MarkAsPaidRequest Request) : IRequest<Result<PaymentDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Payments/Commands/MarkAsPaidCommandHandler.cs`:
  - Verify user access to company
  - Load payment with attachments, verify it belongs to company
  - Call `payment.MarkAsPaid(request.PaidDate, request.PaidAmount, request.ReferenceNumber)`
  - If domain method throws `InvalidOperationException`, return `Result.Failure` with the exception message
  - Save changes, return updated `PaymentDto`

### 7.5 CancelPayment command

- [ ] Create `Allocore.Application/Features/Payments/Commands/CancelPaymentCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Commands;

  using MediatR;
  using Allocore.Domain.Common;

  public record CancelPaymentCommand(Guid CompanyId, Guid PaymentId, string? Reason) : IRequest<Result>;
  ```

- [ ] Create `Allocore.Application/Features/Payments/Commands/CancelPaymentCommandHandler.cs`:
  - Verify user access to company
  - Load payment, verify it belongs to company
  - Call `payment.Cancel(reason)`
  - If domain method throws `InvalidOperationException`, return `Result.Failure`
  - Save changes, return `Result.Success()`

### 7.6 CreateRecurringPayment command

- [ ] Create `Allocore.Application/Features/Payments/Commands/CreateRecurringPaymentCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Commands;

  using MediatR;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Common;

  public record CreateRecurringPaymentCommand(Guid CompanyId, CreateRecurringPaymentRequest Request) : IRequest<Result<RecurringPaymentDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Payments/Commands/CreateRecurringPaymentCommandHandler.cs`:
  - Verify user access to company
  - Verify provider exists and belongs to company
  - Parse `Frequency` and `PaymentMethod` enums
  - Call `RecurringPayment.Create(...)`, save changes
  - Return `RecurringPaymentDto`

### 7.7 UpdateRecurringPayment command

- [ ] Create `Allocore.Application/Features/Payments/Commands/UpdateRecurringPaymentCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Commands;

  using MediatR;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Common;

  public record UpdateRecurringPaymentCommand(Guid CompanyId, Guid RecurringPaymentId, UpdateRecurringPaymentRequest Request) : IRequest<Result<RecurringPaymentDto>>;
  ```

- [ ] Create handler — verify access, load recurring payment, verify it belongs to company, parse enums, call `Update(...)`, save.

### 7.8 DeactivateRecurringPayment command

- [ ] Create `Allocore.Application/Features/Payments/Commands/DeactivateRecurringPaymentCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Commands;

  using MediatR;
  using Allocore.Domain.Common;

  public record DeactivateRecurringPaymentCommand(Guid CompanyId, Guid RecurringPaymentId) : IRequest<Result>;
  ```

- [ ] Create handler — verify access, load recurring payment, verify it belongs to company, call `Deactivate()`, save.

### 7.9 AddPaymentAttachment command

- [ ] Create `Allocore.Application/Features/Payments/Commands/AddPaymentAttachmentCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Commands;

  using MediatR;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Common;

  public record AddPaymentAttachmentCommand(
      Guid CompanyId,
      Guid PaymentId,
      string FileName,
      string ContentType,
      long FileSizeBytes,
      Stream FileStream
  ) : IRequest<Result<PaymentAttachmentDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Payments/Commands/AddPaymentAttachmentCommandHandler.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Commands;

  using MediatR;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Application.Abstractions.Services;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Common;
  using Allocore.Domain.Entities.Payments;

  public class AddPaymentAttachmentCommandHandler : IRequestHandler<AddPaymentAttachmentCommand, Result<PaymentAttachmentDto>>
  {
      private readonly IPaymentRepository _paymentRepository;
      private readonly IUserCompanyRepository _userCompanyRepository;
      private readonly ICurrentUser _currentUser;
      private readonly IFileStorageService _fileStorageService;
      private readonly IUnitOfWork _unitOfWork;

      public AddPaymentAttachmentCommandHandler(
          IPaymentRepository paymentRepository,
          IUserCompanyRepository userCompanyRepository,
          ICurrentUser currentUser,
          IFileStorageService fileStorageService,
          IUnitOfWork unitOfWork)
      {
          _paymentRepository = paymentRepository;
          _userCompanyRepository = userCompanyRepository;
          _currentUser = currentUser;
          _fileStorageService = fileStorageService;
          _unitOfWork = unitOfWork;
      }

      public async Task<Result<PaymentAttachmentDto>> Handle(AddPaymentAttachmentCommand command, CancellationToken cancellationToken)
      {
          if (!_currentUser.UserId.HasValue)
              return Result<PaymentAttachmentDto>.Failure("User not authenticated.");

          var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
              _currentUser.UserId.Value, command.CompanyId, cancellationToken);
          if (!hasAccess)
              return Result<PaymentAttachmentDto>.Failure("You don't have access to this company.");

          var payment = await _paymentRepository.GetByIdWithAttachmentsAsync(command.PaymentId, cancellationToken);
          if (payment is null || payment.CompanyId != command.CompanyId)
              return Result<PaymentAttachmentDto>.Failure("Payment not found in this company.");

          // Upload file to storage
          var storagePath = $"companies/{command.CompanyId}/payments/{command.PaymentId}/{Guid.NewGuid()}/{command.FileName}";
          var finalPath = await _fileStorageService.UploadAsync(storagePath, command.FileStream, command.ContentType, cancellationToken);

          var attachment = PaymentAttachment.Create(
              command.PaymentId,
              command.FileName,
              command.ContentType,
              command.FileSizeBytes,
              finalPath);

          payment.AddAttachment(attachment);
          await _unitOfWork.SaveChangesAsync(cancellationToken);

          return Result<PaymentAttachmentDto>.Success(new PaymentAttachmentDto(
              attachment.Id,
              attachment.FileName,
              attachment.ContentType,
              attachment.FileSizeBytes,
              attachment.UploadedAt));
      }
  }
  ```
  - **Note**: Storage path follows a structured pattern: `companies/{companyId}/payments/{paymentId}/{guid}/{fileName}`.
  - **Note**: Uses `IFileStorageService` which is a stub in this story. Will throw `NotImplementedException` until US009.

---

## Step 8: Application Layer — CQRS Queries

### 8.1 GetPaymentById query

- [ ] Create `Allocore.Application/Features/Payments/Queries/GetPaymentByIdQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Queries;

  using MediatR;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Common;

  public record GetPaymentByIdQuery(Guid CompanyId, Guid PaymentId) : IRequest<Result<PaymentDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Payments/Queries/GetPaymentByIdQueryHandler.cs`:
  - Verify user access to company
  - Load payment with attachments, verify it belongs to company
  - Return `PaymentDto`

### 8.2 GetPaymentsPaged query

- [ ] Create `Allocore.Application/Features/Payments/Queries/GetPaymentsPagedQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Queries;

  using MediatR;
  using Allocore.Application.Common;
  using Allocore.Application.Features.Payments.DTOs;

  public record GetPaymentsPagedQuery(
      Guid CompanyId,
      int Page = 1,
      int PageSize = 10,
      string? Status = null,
      Guid? ProviderId = null,
      DateTime? DueDateFrom = null,
      DateTime? DueDateTo = null,
      string? SearchTerm = null
  ) : IRequest<PagedResult<PaymentListItemDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Payments/Queries/GetPaymentsPagedQueryHandler.cs`:
  - Verify user access to company
  - Parse status filter if provided
  - Call `_paymentRepository.GetPagedByCompanyAsync(...)` with filters
  - Map to `PaymentListItemDto` (requires joining with Provider for `ProviderName` — include via repository or separate lookup)
  - Return `PagedResult<PaymentListItemDto>`

### 8.3 GetOverduePayments query

- [ ] Create `Allocore.Application/Features/Payments/Queries/GetOverduePaymentsQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Queries;

  using MediatR;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Common;

  public record GetOverduePaymentsQuery(Guid CompanyId) : IRequest<Result<IEnumerable<PaymentListItemDto>>>;
  ```

- [ ] Create `Allocore.Application/Features/Payments/Queries/GetOverduePaymentsQueryHandler.cs`:
  - Verify user access to company
  - Call `_paymentRepository.GetOverdueByCompanyAsync(companyId)`
  - Map to `PaymentListItemDto`
  - Return list

### 8.4 GetRecurringPaymentsPaged query

- [ ] Create `Allocore.Application/Features/Payments/Queries/GetRecurringPaymentsPagedQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Queries;

  using MediatR;
  using Allocore.Application.Common;
  using Allocore.Application.Features.Payments.DTOs;

  public record GetRecurringPaymentsPagedQuery(
      Guid CompanyId,
      int Page = 1,
      int PageSize = 10,
      Guid? ProviderId = null,
      bool? IsActive = null
  ) : IRequest<PagedResult<RecurringPaymentListItemDto>>;
  ```

- [ ] Create handler — verify access, call repository, map to `RecurringPaymentListItemDto`, return paged result.

---

## Step 9: API Layer — Controllers

### 9.1 PaymentsController

- [ ] Create `Allocore.API/Controllers/v1/PaymentsController.cs`:
  ```csharp
  namespace Allocore.API.Controllers.v1;

  using Asp.Versioning;
  using MediatR;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Allocore.Application.Features.Payments.Commands;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Application.Features.Payments.Queries;

  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/companies/{companyId:guid}/payments")]
  [Authorize]
  public class PaymentsController : ControllerBase
  {
      private readonly IMediator _mediator;

      public PaymentsController(IMediator mediator)
      {
          _mediator = mediator;
      }

      /// <summary>
      /// List payments for a company (paginated, filterable).
      /// </summary>
      [HttpGet]
      public async Task<IActionResult> GetPayments(
          Guid companyId,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 10,
          [FromQuery] string? status = null,
          [FromQuery] Guid? providerId = null,
          [FromQuery] DateTime? dueDateFrom = null,
          [FromQuery] DateTime? dueDateTo = null,
          [FromQuery] string? search = null,
          CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(
              new GetPaymentsPagedQuery(companyId, page, pageSize, status, providerId, dueDateFrom, dueDateTo, search),
              cancellationToken);
          return Ok(result);
      }

      /// <summary>
      /// Get a payment by ID with attachments.
      /// </summary>
      [HttpGet("{paymentId:guid}")]
      public async Task<IActionResult> GetPayment(Guid companyId, Guid paymentId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new GetPaymentByIdQuery(companyId, paymentId), cancellationToken);
          if (!result.IsSuccess)
              return NotFound(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Get overdue payments for a company.
      /// </summary>
      [HttpGet("overdue")]
      public async Task<IActionResult> GetOverduePayments(Guid companyId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new GetOverduePaymentsQuery(companyId), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Create a new payment.
      /// </summary>
      [HttpPost]
      public async Task<IActionResult> CreatePayment(
          Guid companyId,
          [FromBody] CreatePaymentRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new CreatePaymentCommand(companyId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return CreatedAtAction(nameof(GetPayment), new { companyId, paymentId = result.Value!.Id }, result.Value);
      }

      /// <summary>
      /// Update a payment's details.
      /// </summary>
      [HttpPut("{paymentId:guid}")]
      public async Task<IActionResult> UpdatePayment(
          Guid companyId,
          Guid paymentId,
          [FromBody] UpdatePaymentRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new UpdatePaymentCommand(companyId, paymentId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Mark a payment as paid.
      /// </summary>
      [HttpPatch("{paymentId:guid}/mark-paid")]
      public async Task<IActionResult> MarkAsPaid(
          Guid companyId,
          Guid paymentId,
          [FromBody] MarkAsPaidRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new MarkAsPaidCommand(companyId, paymentId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Cancel a payment.
      /// </summary>
      [HttpPatch("{paymentId:guid}/cancel")]
      public async Task<IActionResult> CancelPayment(
          Guid companyId,
          Guid paymentId,
          [FromBody] CancelPaymentRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new CancelPaymentCommand(companyId, paymentId, request.Reason), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }

      /// <summary>
      /// Upload an attachment to a payment.
      /// </summary>
      [HttpPost("{paymentId:guid}/attachments")]
      public async Task<IActionResult> AddAttachment(
          Guid companyId,
          Guid paymentId,
          IFormFile file,
          CancellationToken cancellationToken)
      {
          if (file is null || file.Length == 0)
              return BadRequest(new { error = "File is required." });

          using var stream = file.OpenReadStream();
          var result = await _mediator.Send(
              new AddPaymentAttachmentCommand(companyId, paymentId, file.FileName, file.ContentType, file.Length, stream),
              cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Remove an attachment from a payment.
      /// </summary>
      [HttpDelete("{paymentId:guid}/attachments/{attachmentId:guid}")]
      public async Task<IActionResult> RemoveAttachment(
          Guid companyId,
          Guid paymentId,
          Guid attachmentId,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(
              new RemovePaymentAttachmentCommand(companyId, paymentId, attachmentId),
              cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }
  }
  ```
  - **Note**: Route is nested under company: `/api/v1/companies/{companyId}/payments`.
  - **Note**: Follows same controller pattern as `ProvidersController`.
  - **Note**: Attachment upload accepts `IFormFile` and converts to stream for the command.

### 9.2 RemovePaymentAttachment command (referenced by controller)

- [ ] Create `Allocore.Application/Features/Payments/Commands/RemovePaymentAttachmentCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Commands;

  using MediatR;
  using Allocore.Domain.Common;

  public record RemovePaymentAttachmentCommand(Guid CompanyId, Guid PaymentId, Guid AttachmentId) : IRequest<Result>;
  ```

- [ ] Create handler — verify access, load payment with attachments, find attachment, delete from storage via `IFileStorageService.DeleteAsync`, remove from entity, save.

### 9.3 RecurringPaymentsController

- [ ] Create `Allocore.API/Controllers/v1/RecurringPaymentsController.cs`:
  ```csharp
  namespace Allocore.API.Controllers.v1;

  using Asp.Versioning;
  using MediatR;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Allocore.Application.Features.Payments.Commands;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Application.Features.Payments.Queries;

  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/companies/{companyId:guid}/recurring-payments")]
  [Authorize]
  public class RecurringPaymentsController : ControllerBase
  {
      private readonly IMediator _mediator;

      public RecurringPaymentsController(IMediator mediator)
      {
          _mediator = mediator;
      }

      /// <summary>
      /// List recurring payments for a company (paginated, filterable).
      /// </summary>
      [HttpGet]
      public async Task<IActionResult> GetRecurringPayments(
          Guid companyId,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 10,
          [FromQuery] Guid? providerId = null,
          [FromQuery] bool? isActive = null,
          CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(
              new GetRecurringPaymentsPagedQuery(companyId, page, pageSize, providerId, isActive),
              cancellationToken);
          return Ok(result);
      }

      /// <summary>
      /// Get a recurring payment by ID with payment history.
      /// </summary>
      [HttpGet("{recurringPaymentId:guid}")]
      public async Task<IActionResult> GetRecurringPayment(
          Guid companyId,
          Guid recurringPaymentId,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(
              new GetRecurringPaymentByIdQuery(companyId, recurringPaymentId),
              cancellationToken);
          if (!result.IsSuccess)
              return NotFound(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Create a new recurring payment.
      /// </summary>
      [HttpPost]
      public async Task<IActionResult> CreateRecurringPayment(
          Guid companyId,
          [FromBody] CreateRecurringPaymentRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(
              new CreateRecurringPaymentCommand(companyId, request),
              cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return CreatedAtAction(nameof(GetRecurringPayment), new { companyId, recurringPaymentId = result.Value!.Id }, result.Value);
      }

      /// <summary>
      /// Update a recurring payment.
      /// </summary>
      [HttpPut("{recurringPaymentId:guid}")]
      public async Task<IActionResult> UpdateRecurringPayment(
          Guid companyId,
          Guid recurringPaymentId,
          [FromBody] UpdateRecurringPaymentRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(
              new UpdateRecurringPaymentCommand(companyId, recurringPaymentId, request),
              cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Deactivate a recurring payment.
      /// </summary>
      [HttpPatch("{recurringPaymentId:guid}/deactivate")]
      public async Task<IActionResult> DeactivateRecurringPayment(
          Guid companyId,
          Guid recurringPaymentId,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(
              new DeactivateRecurringPaymentCommand(companyId, recurringPaymentId),
              cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }
  }
  ```

### 9.4 GetRecurringPaymentById query (referenced by controller)

- [ ] Create `Allocore.Application/Features/Payments/Queries/GetRecurringPaymentByIdQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Payments.Queries;

  using MediatR;
  using Allocore.Application.Features.Payments.DTOs;
  using Allocore.Domain.Common;

  public record GetRecurringPaymentByIdQuery(Guid CompanyId, Guid RecurringPaymentId) : IRequest<Result<RecurringPaymentDto>>;
  ```

- [ ] Create handler — verify access, load recurring payment with payments, verify it belongs to company, map to `RecurringPaymentDto` with recent payments, return.

### 9.5 Build, Verify & Manual Test

- [ ] Run `dotnet build` — ensure entire solution compiles
- [ ] Apply migration: `dotnet ef database update -s Allocore.API -p Allocore.Infrastructure`
- [ ] Run application and verify Swagger shows all new endpoints
- [ ] Manual test via Swagger:
  1. Create a payment → 201
  2. Get payment by ID → 200 with attachments (empty)
  3. List payments (paginated) → 200
  4. Mark payment as paid → 200
  5. Cancel a payment → 204
  6. Verify duplicate invoice number returns 400
  7. Create a recurring payment → 201
  8. Get recurring payment by ID → 200
  9. List recurring payments → 200
  10. Deactivate recurring payment → 204
  11. Get overdue payments → 200
  12. Verify wrong company returns error
  13. Attempt attachment upload → 500 (expected — stub service)

---

## Technical Details

### Dependencies

No new NuGet packages required beyond US004 (FluentValidation, MediatR, EF Core already present).
`IFileStorageService` is stubbed — real implementation deferred to US009.

### Project Structure — Affected Files

| Layer | File | Change |
|-------|------|--------|
| **Domain** | `Allocore.Domain/Entities/Payments/PaymentStatus.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Payments/PaymentMethod.cs` | **Create** |
| **Domain** | `Allocore.Domain/Common/BillingFrequency.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Payments/RecurringPayment.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Payments/Payment.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Payments/PaymentAttachment.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Configurations/RecurringPaymentConfiguration.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Configurations/PaymentConfiguration.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Configurations/PaymentAttachmentConfiguration.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs` | **Update** — add DbSets |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Repositories/PaymentRepository.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Repositories/RecurringPaymentRepository.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Services/NotImplementedFileStorageService.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/DependencyInjection.cs` | **Update** — add registrations |
| **Application** | `Allocore.Application/Abstractions/Persistence/IPaymentRepository.cs` | **Create** |
| **Application** | `Allocore.Application/Abstractions/Persistence/IRecurringPaymentRepository.cs` | **Create** |
| **Application** | `Allocore.Application/Abstractions/Services/IFileStorageService.cs` | **Create** |
| **Application** | `Allocore.Application/Features/Payments/DTOs/*.cs` | **Create** (11 files) |
| **Application** | `Allocore.Application/Features/Payments/Validators/*.cs` | **Create** (5 files) |
| **Application** | `Allocore.Application/Features/Payments/Commands/*.cs` | **Create** (18 files — 9 commands + 9 handlers) |
| **Application** | `Allocore.Application/Features/Payments/Queries/*.cs` | **Create** (8 files — 4 queries + 4 handlers) |
| **API** | `Allocore.API/Controllers/v1/PaymentsController.cs` | **Create** |
| **API** | `Allocore.API/Controllers/v1/RecurringPaymentsController.cs` | **Create** |

### Database

**Table: RecurringPayments**

| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| Id | uuid | NO | gen | PK |
| CompanyId | uuid | NO | — | FK → Companies |
| ProviderId | uuid | NO | — | FK → Providers (RESTRICT) |
| Description | varchar(500) | NO | — | |
| ExpectedAmount | decimal(18,2) | NO | — | |
| Currency | varchar(3) | NO | BRL | |
| Frequency | varchar(50) | NO | — | |
| DayOfMonth | integer | NO | — | |
| StartDate | timestamp | NO | — | |
| EndDate | timestamp | YES | — | |
| PaymentMethod | varchar(50) | NO | — | |
| IsActive | boolean | NO | true | |
| Notes | varchar(2000) | YES | — | |
| CreatedAt | timestamp | NO | — | |
| UpdatedAt | timestamp | YES | — | |

**Indexes:**
- `IX_RecurringPayments_CompanyId`
- `IX_RecurringPayments_ProviderId`

**Table: Payments**

| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| Id | uuid | NO | gen | PK |
| CompanyId | uuid | NO | — | FK → Companies |
| ProviderId | uuid | NO | — | FK → Providers (RESTRICT) |
| RecurringPaymentId | uuid | YES | — | FK → RecurringPayments (SET NULL) |
| Description | varchar(500) | NO | — | |
| Amount | decimal(18,2) | NO | — | |
| Currency | varchar(3) | NO | BRL | |
| DueDate | timestamp | NO | — | |
| PaidDate | timestamp | YES | — | |
| PaidAmount | decimal(18,2) | YES | — | |
| PaymentMethod | varchar(50) | NO | — | |
| Status | varchar(50) | NO | Pending | |
| ReferenceNumber | varchar(100) | YES | — | |
| InvoiceNumber | varchar(100) | YES | — | UNIQUE filtered (CompanyId, InvoiceNumber) |
| Notes | varchar(2000) | YES | — | |
| IsActive | boolean | NO | true | |
| CreatedAt | timestamp | NO | — | |
| UpdatedAt | timestamp | YES | — | |

**Indexes:**
- `IX_Payments_CompanyId`
- `IX_Payments_ProviderId`
- `IX_Payments_Status`
- `IX_Payments_DueDate`
- `IX_Payments_CompanyId_InvoiceNumber` (UNIQUE, filtered: InvoiceNumber IS NOT NULL)

**Table: PaymentAttachments**

| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| Id | uuid | NO | gen | PK |
| PaymentId | uuid | NO | — | FK → Payments (CASCADE) |
| FileName | varchar(500) | NO | — | |
| ContentType | varchar(100) | NO | — | |
| FileSizeBytes | bigint | NO | — | |
| StoragePath | varchar(1000) | NO | — | |
| UploadedAt | timestamp | NO | — | |
| CreatedAt | timestamp | NO | — | |
| UpdatedAt | timestamp | YES | — | |

**Indexes:**
- `IX_PaymentAttachments_PaymentId`

### API Contract

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/v1/companies/{companyId}/payments` | GET | Yes | List payments (paginated, filterable) |
| `/api/v1/companies/{companyId}/payments/{paymentId}` | GET | Yes | Get payment with attachments |
| `/api/v1/companies/{companyId}/payments/overdue` | GET | Yes | Get overdue payments |
| `/api/v1/companies/{companyId}/payments` | POST | Yes | Create payment |
| `/api/v1/companies/{companyId}/payments/{paymentId}` | PUT | Yes | Update payment |
| `/api/v1/companies/{companyId}/payments/{paymentId}/mark-paid` | PATCH | Yes | Mark as paid |
| `/api/v1/companies/{companyId}/payments/{paymentId}/cancel` | PATCH | Yes | Cancel payment |
| `/api/v1/companies/{companyId}/payments/{paymentId}/attachments` | POST | Yes | Upload attachment |
| `/api/v1/companies/{companyId}/payments/{paymentId}/attachments/{attachmentId}` | DELETE | Yes | Remove attachment |
| `/api/v1/companies/{companyId}/recurring-payments` | GET | Yes | List recurring payments |
| `/api/v1/companies/{companyId}/recurring-payments/{recurringPaymentId}` | GET | Yes | Get recurring payment with history |
| `/api/v1/companies/{companyId}/recurring-payments` | POST | Yes | Create recurring payment |
| `/api/v1/companies/{companyId}/recurring-payments/{recurringPaymentId}` | PUT | Yes | Update recurring payment |
| `/api/v1/companies/{companyId}/recurring-payments/{recurringPaymentId}/deactivate` | PATCH | Yes | Deactivate recurring payment |

### Authentication/Authorization

- All endpoints require JWT Bearer authentication
- All endpoints verify user has access to the specified company via `UserCompany` relationship
- Fine-grained role checks (Viewer = read-only, Manager = write, Owner = full) deferred to a future authorization story

---

## Acceptance Criteria

- [ ] Authenticated users can create, update, and cancel payments within their companies
- [ ] Payments are company-scoped — no cross-tenant data leakage
- [ ] Payment status transitions follow the defined state machine
- [ ] `PaidAmount` can differ from `Amount` (partial payments, interest)
- [ ] InvoiceNumber is unique within a company (nullable, filtered index)
- [ ] Payments can be linked to an existing RecurringPayment (ProviderId must match)
- [ ] Recurring payments support create, update, deactivate, and list with filters
- [ ] Overdue detection works at query time (Pending + DueDate < now)
- [ ] Attachment upload/remove endpoints exist (stub storage — 500 until US009)
- [ ] `IFileStorageService` abstraction created for future S3/MinIO implementation
- [ ] All 14 API endpoints visible in Swagger
- [ ] Migrations created and applied (`AddPayments`)
- [ ] `dotnet build` passes without errors

---

## What is explicitly NOT changing?

- **Authentication/Authorization model** — no new roles or policies added
- **Provider entity** — no changes to Provider or ProviderContact
- **Contract entity** — no changes (US005)
- **Company entity** — no changes to Company or UserCompany
- **Existing endpoints** — no modifications to Ping, Auth, Companies, or Providers controllers
- **Email integration** — not part of this story (see US009)
- **Background jobs** — no scheduled overdue detection; done at query time
- **Real file storage** — stub only; S3/MinIO implementation in US009

---

## Follow-ups (Intentionally Deferred)

| Item | Reason | Related Story |
|------|--------|---------------|
| Email-based automatic payment creation | Separate integration concern | US009 |
| Real file storage (S3/MinIO) implementation | Infrastructure dependency | US009 |
| Background job for overdue detection | Nice-to-have, query-time is sufficient for now | Future US |
| Payment reports and analytics | Separate reporting domain | Future US (EPIC 7) |
| Role-based write permissions | Requires authorization policy infrastructure | Future: Authorization Policies |
| Payment approval workflow | Process complexity beyond initial scope | Future US |
| Automatic recurring payment generation | Requires scheduled background jobs | Future US |
| BillingFrequency enum shared with US005 | If US005 creates its own, merge to `Domain/Common/` | Implementation time |
