# US009 – Email Payment Integration

## Description

**As** an Admin user managing a company in Allocore,
**I need** to connect a company email account (Microsoft 365) so that incoming payment notifications, invoices, and billing emails are automatically read, classified by AI, and converted into Payment records,
**So that** I can eliminate the manual process of checking emails, identifying payment-related messages, and creating payment entries by hand — reducing human error and ensuring no payment deadline is missed.

Currently, Allocore has no email integration. Gestores receive boletos, faturas, and payment confirmations in a centralized mailbox and manually create Payment records. This story introduces email account connection (Microsoft 365 via Graph API), webhook-based real-time email reception, AI-powered classification and data extraction (via Claude LLM), fuzzy provider matching, automatic Payment creation, and attachment storage (S3/MinIO). The pipeline handles the full lifecycle from email arrival to Payment record creation.

**Priority**: High
**Dependencies**: US008 – Payment & Billing Domain (Payment entity, IFileStorageService), US004 – Provider Management (Provider matching)

---

## Step 1: Domain Layer — Enums

### 1.1 Create EmailProviderType enum

- [ ] Create `Allocore.Domain/Entities/EmailIntegration/EmailProviderType.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.EmailIntegration;

  public enum EmailProviderType
  {
      Microsoft365 = 0,
      Gmail = 1
  }
  ```
  - **Note**: Gmail is included for future extensibility. Only Microsoft365 is implemented in this story.

### 1.2 Create EmailProcessingStatus enum

- [ ] Create `Allocore.Domain/Entities/EmailIntegration/EmailProcessingStatus.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.EmailIntegration;

  public enum EmailProcessingStatus
  {
      Pending = 0,
      Processing = 1,
      Classified = 2,
      PaymentCreated = 3,
      Ignored = 4,
      Error = 5
  }
  ```
  - **Business rule**: Happy path flow: Pending → Processing → Classified → PaymentCreated.
  - **Business rule**: Non-payment emails: Pending → Processing → Classified → Ignored.
  - **Business rule**: Errors: any state → Error.
  - **Business rule**: `Classified` without payment = provider not found; requires manual intervention.

### 1.3 Create EmailClassification enum

- [ ] Create `Allocore.Domain/Entities/EmailIntegration/EmailClassification.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.EmailIntegration;

  public enum EmailClassification
  {
      PaymentNotification = 0,
      OverdueNotice = 1,
      PaymentConfirmation = 2,
      InvoiceReceived = 3,
      Unrelated = 4,
      Unknown = 5
  }
  ```
  - **Business rule**: Only `PaymentNotification`, `OverdueNotice`, `PaymentConfirmation`, and `InvoiceReceived` trigger data extraction and payment creation.
  - **Business rule**: `Unrelated` and `Unknown` are marked as `Ignored` after classification.

---

## Step 2: Domain Layer — EmailAccount Entity

### 2.1 Create EmailAccount entity

- [ ] Create `Allocore.Domain/Entities/EmailIntegration/EmailAccount.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.EmailIntegration;

  using Allocore.Domain.Common;

  public class EmailAccount : Entity
  {
      public Guid CompanyId { get; private set; }
      public string EmailAddress { get; private set; } = string.Empty;
      public EmailProviderType ProviderType { get; private set; }
      public string? DisplayName { get; private set; }
      public bool IsActive { get; private set; } = true;
      public DateTime? LastSyncedAt { get; private set; }

      // Sensitive — stored encrypted at rest (app-level or DB-level)
      public string? AccessToken { get; private set; }
      public string? RefreshToken { get; private set; }
      public DateTime? TokenExpiresAt { get; private set; }

      // Microsoft Graph webhook subscription ID
      public string? SubscriptionId { get; private set; }

      private EmailAccount() { } // EF Core

      public static EmailAccount Create(
          Guid companyId,
          string emailAddress,
          EmailProviderType providerType,
          string? displayName = null)
      {
          return new EmailAccount
          {
              CompanyId = companyId,
              EmailAddress = emailAddress,
              ProviderType = providerType,
              DisplayName = displayName,
              IsActive = true
          };
      }

      public void Update(string? displayName)
      {
          DisplayName = displayName;
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

      public void UpdateTokens(string accessToken, string refreshToken, DateTime expiresAt)
      {
          AccessToken = accessToken;
          RefreshToken = refreshToken;
          TokenExpiresAt = expiresAt;
          UpdatedAt = DateTime.UtcNow;
      }

      public void UpdateLastSyncedAt()
      {
          LastSyncedAt = DateTime.UtcNow;
          UpdatedAt = DateTime.UtcNow;
      }

      public void SetSubscriptionId(string? subscriptionId)
      {
          SubscriptionId = subscriptionId;
          UpdatedAt = DateTime.UtcNow;
      }
  }
  ```
  - **Business rule**: One email account per company (unique constraint on CompanyId).
  - **Business rule**: `AccessToken` and `RefreshToken` are sensitive — should be treated as secrets.
  - **Business rule**: `TokenExpiresAt` tracks when the OAuth2 token expires for proactive refresh.
  - **Business rule**: `SubscriptionId` stores the Microsoft Graph webhook subscription ID for renewal/deletion.
  - **Note**: Follows same pattern as other entities — private constructor, static `Create()`, mutation methods.

---

## Step 3: Domain Layer — ProcessedEmail Entity

### 3.1 Create ProcessedEmail entity

- [ ] Create `Allocore.Domain/Entities/EmailIntegration/ProcessedEmail.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.EmailIntegration;

  using Allocore.Domain.Common;

  public class ProcessedEmail : Entity
  {
      public Guid EmailAccountId { get; private set; }
      public string ExternalMessageId { get; private set; } = string.Empty;
      public string Subject { get; private set; } = string.Empty;
      public string SenderEmail { get; private set; } = string.Empty;
      public string? SenderName { get; private set; }
      public DateTime ReceivedAt { get; private set; }

      public EmailProcessingStatus ProcessingStatus { get; private set; } = EmailProcessingStatus.Pending;
      public EmailClassification? Classification { get; private set; }
      public string? ExtractedData { get; private set; }

      public Guid? MatchedProviderId { get; private set; }
      public Guid? MatchedPaymentId { get; private set; }
      public string? ErrorMessage { get; private set; }
      public DateTime? ProcessedAt { get; private set; }

      // Navigation
      public EmailAccount? EmailAccount { get; private set; }

      private ProcessedEmail() { } // EF Core

      public static ProcessedEmail Create(
          Guid emailAccountId,
          string externalMessageId,
          string subject,
          string senderEmail,
          string? senderName,
          DateTime receivedAt)
      {
          return new ProcessedEmail
          {
              EmailAccountId = emailAccountId,
              ExternalMessageId = externalMessageId,
              Subject = subject,
              SenderEmail = senderEmail,
              SenderName = senderName,
              ReceivedAt = receivedAt,
              ProcessingStatus = EmailProcessingStatus.Pending
          };
      }

      public void MarkAsProcessing()
      {
          ProcessingStatus = EmailProcessingStatus.Processing;
          UpdatedAt = DateTime.UtcNow;
      }

      public void SetClassification(EmailClassification classification, string? extractedData, Guid? matchedProviderId)
      {
          Classification = classification;
          ExtractedData = extractedData;
          MatchedProviderId = matchedProviderId;
          ProcessingStatus = EmailProcessingStatus.Classified;
          ProcessedAt = DateTime.UtcNow;
          UpdatedAt = DateTime.UtcNow;
      }

      public void SetPaymentCreated(Guid paymentId)
      {
          MatchedPaymentId = paymentId;
          ProcessingStatus = EmailProcessingStatus.PaymentCreated;
          UpdatedAt = DateTime.UtcNow;
      }

      public void MarkAsIgnored()
      {
          ProcessingStatus = EmailProcessingStatus.Ignored;
          ProcessedAt = DateTime.UtcNow;
          UpdatedAt = DateTime.UtcNow;
      }

      public void MarkAsError(string errorMessage)
      {
          ProcessingStatus = EmailProcessingStatus.Error;
          ErrorMessage = errorMessage;
          ProcessedAt = DateTime.UtcNow;
          UpdatedAt = DateTime.UtcNow;
      }

      public void ResetForReprocessing()
      {
          ProcessingStatus = EmailProcessingStatus.Pending;
          Classification = null;
          ExtractedData = null;
          MatchedProviderId = null;
          MatchedPaymentId = null;
          ErrorMessage = null;
          ProcessedAt = null;
          UpdatedAt = DateTime.UtcNow;
      }
  }
  ```
  - **Business rule**: `ExternalMessageId` is unique per EmailAccount — prevents duplicate processing.
  - **Business rule**: `ExtractedData` is a JSON string with structured data:
    ```json
    {
      "amount": 1500.00,
      "currency": "BRL",
      "dueDate": "2026-04-15",
      "providerName": "Azure Cloud Services",
      "invoiceNumber": "INV-2026-0042",
      "description": "Monthly cloud hosting - April 2026"
    }
    ```
  - **Business rule**: `MatchedProviderId` is set during classification if a provider match was found.
  - **Business rule**: `MatchedPaymentId` is set after a Payment is successfully created.
  - **Business rule**: `ResetForReprocessing()` allows manual reprocessing of failed or classified-without-payment emails.
  - **Note**: Navigation to Provider and Payment intentionally NOT included to avoid circular dependencies. Use IDs for lookups.

---

## Step 4: Infrastructure Layer — EF Core Configurations

### 4.1 EmailAccount configuration

- [ ] Create `Allocore.Infrastructure/Persistence/Configurations/EmailAccountConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.EmailIntegration;

  public class EmailAccountConfiguration : IEntityTypeConfiguration<EmailAccount>
  {
      public void Configure(EntityTypeBuilder<EmailAccount> builder)
      {
          builder.ToTable("EmailAccounts");

          builder.HasKey(ea => ea.Id);

          builder.Property(ea => ea.CompanyId)
              .IsRequired();

          builder.Property(ea => ea.EmailAddress)
              .IsRequired()
              .HasMaxLength(254);

          builder.Property(ea => ea.ProviderType)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(ea => ea.DisplayName)
              .HasMaxLength(200);

          builder.Property(ea => ea.IsActive)
              .IsRequired()
              .HasDefaultValue(true);

          builder.Property(ea => ea.AccessToken)
              .HasMaxLength(4000);

          builder.Property(ea => ea.RefreshToken)
              .HasMaxLength(4000);

          builder.Property(ea => ea.SubscriptionId)
              .HasMaxLength(500);

          // One email account per company
          builder.HasIndex(ea => ea.CompanyId)
              .IsUnique();
      }
  }
  ```
  - **Note**: Unique index on `CompanyId` enforces one email account per company.
  - **Note**: Token columns are 4000 chars to accommodate OAuth2 tokens.
  - **Note**: Enum stored as string for DB readability.

### 4.2 ProcessedEmail configuration

- [ ] Create `Allocore.Infrastructure/Persistence/Configurations/ProcessedEmailConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.EmailIntegration;

  public class ProcessedEmailConfiguration : IEntityTypeConfiguration<ProcessedEmail>
  {
      public void Configure(EntityTypeBuilder<ProcessedEmail> builder)
      {
          builder.ToTable("ProcessedEmails");

          builder.HasKey(pe => pe.Id);

          builder.Property(pe => pe.EmailAccountId)
              .IsRequired();

          builder.Property(pe => pe.ExternalMessageId)
              .IsRequired()
              .HasMaxLength(500);

          builder.Property(pe => pe.Subject)
              .IsRequired()
              .HasMaxLength(1000);

          builder.Property(pe => pe.SenderEmail)
              .IsRequired()
              .HasMaxLength(254);

          builder.Property(pe => pe.SenderName)
              .HasMaxLength(200);

          builder.Property(pe => pe.ReceivedAt)
              .IsRequired();

          builder.Property(pe => pe.ProcessingStatus)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired()
              .HasDefaultValue(EmailProcessingStatus.Pending);

          builder.Property(pe => pe.Classification)
              .HasConversion<string>()
              .HasMaxLength(50);

          builder.Property(pe => pe.ExtractedData)
              .HasColumnType("jsonb");

          builder.Property(pe => pe.ErrorMessage)
              .HasMaxLength(2000);

          // FK to EmailAccount (cascade — delete account deletes all processed emails)
          builder.HasOne(pe => pe.EmailAccount)
              .WithMany()
              .HasForeignKey(pe => pe.EmailAccountId)
              .OnDelete(DeleteBehavior.Cascade);

          // Unique external message ID per email account (prevent duplicate processing)
          builder.HasIndex(pe => new { pe.EmailAccountId, pe.ExternalMessageId })
              .IsUnique();

          builder.HasIndex(pe => pe.ProcessingStatus);
          builder.HasIndex(pe => pe.MatchedProviderId);
          builder.HasIndex(pe => pe.MatchedPaymentId);
      }
  }
  ```
  - **Note**: `ExtractedData` uses `jsonb` column type for PostgreSQL native JSON support.
  - **Note**: FK to Provider and Payment are NOT configured as formal FK constraints — they are lookup IDs only. This avoids complex cascade chains.
  - **Note**: Unique index on `(EmailAccountId, ExternalMessageId)` prevents processing the same email twice.

### 4.3 Update ApplicationDbContext

- [ ] Update `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs` — add DbSets:
  ```csharp
  // Add these using statements
  using Allocore.Domain.Entities.EmailIntegration;

  // Add these DbSets
  public DbSet<EmailAccount> EmailAccounts => Set<EmailAccount>();
  public DbSet<ProcessedEmail> ProcessedEmails => Set<ProcessedEmail>();
  ```
  - **Note**: `OnModelCreating` already calls `ApplyConfigurationsFromAssembly` so the new configurations will be auto-discovered.

### 4.4 Create migration

- [ ] Run migration:
  ```bash
  dotnet ef migrations add AddEmailIntegration -s Allocore.API -p Allocore.Infrastructure
  ```
  - **Impact on existing data**: No existing rows affected. Two new tables created.

---

## Step 5: Infrastructure Layer — Repositories

### 5.1 Create IEmailAccountRepository interface

- [ ] Create `Allocore.Application/Abstractions/Persistence/IEmailAccountRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;

  using Allocore.Domain.Entities.EmailIntegration;

  public interface IEmailAccountRepository : IReadRepository<EmailAccount>, IWriteRepository<EmailAccount>
  {
      Task<EmailAccount?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
      Task<EmailAccount?> GetBySubscriptionIdAsync(string subscriptionId, CancellationToken cancellationToken = default);
  }
  ```
  - **Note**: `GetByCompanyIdAsync` returns the single email account for a company (1:1 relationship).
  - **Note**: `GetBySubscriptionIdAsync` is used by the webhook handler to find the account from the Graph subscription notification.

### 5.2 Create EmailAccountRepository implementation

- [ ] Create `Allocore.Infrastructure/Persistence/Repositories/EmailAccountRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Repositories;

  using Microsoft.EntityFrameworkCore;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Entities.EmailIntegration;

  public class EmailAccountRepository : IEmailAccountRepository
  {
      private readonly ApplicationDbContext _context;

      public EmailAccountRepository(ApplicationDbContext context)
      {
          _context = context;
      }

      public async Task<EmailAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.EmailAccounts.FirstOrDefaultAsync(ea => ea.Id == id, cancellationToken);

      public async Task<EmailAccount?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
          => await _context.EmailAccounts.FirstOrDefaultAsync(ea => ea.CompanyId == companyId, cancellationToken);

      public async Task<EmailAccount?> GetBySubscriptionIdAsync(string subscriptionId, CancellationToken cancellationToken = default)
          => await _context.EmailAccounts.FirstOrDefaultAsync(ea => ea.SubscriptionId == subscriptionId, cancellationToken);

      public async Task<IEnumerable<EmailAccount>> GetAllAsync(CancellationToken cancellationToken = default)
          => await _context.EmailAccounts.ToListAsync(cancellationToken);

      public async Task<EmailAccount> AddAsync(EmailAccount entity, CancellationToken cancellationToken = default)
      {
          await _context.EmailAccounts.AddAsync(entity, cancellationToken);
          return entity;
      }

      public Task UpdateAsync(EmailAccount entity, CancellationToken cancellationToken = default)
      {
          _context.EmailAccounts.Update(entity);
          return Task.CompletedTask;
      }

      public Task DeleteAsync(EmailAccount entity, CancellationToken cancellationToken = default)
      {
          _context.EmailAccounts.Remove(entity);
          return Task.CompletedTask;
      }
  }
  ```

### 5.3 Create IProcessedEmailRepository interface

- [ ] Create `Allocore.Application/Abstractions/Persistence/IProcessedEmailRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;

  using Allocore.Domain.Entities.EmailIntegration;

  public interface IProcessedEmailRepository : IReadRepository<ProcessedEmail>, IWriteRepository<ProcessedEmail>
  {
      Task<bool> ExistsByExternalMessageIdAsync(Guid emailAccountId, string externalMessageId, CancellationToken cancellationToken = default);
      Task<ProcessedEmail?> GetByExternalMessageIdAsync(Guid emailAccountId, string externalMessageId, CancellationToken cancellationToken = default);
      Task<(IEnumerable<ProcessedEmail> Emails, int TotalCount)> GetPagedByAccountAsync(
          Guid emailAccountId, int page, int pageSize,
          EmailProcessingStatus? statusFilter = null,
          EmailClassification? classificationFilter = null,
          string? searchTerm = null,
          CancellationToken cancellationToken = default);
  }
  ```

### 5.4 Create ProcessedEmailRepository implementation

- [ ] Create `Allocore.Infrastructure/Persistence/Repositories/ProcessedEmailRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Repositories;

  using Microsoft.EntityFrameworkCore;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Entities.EmailIntegration;

  public class ProcessedEmailRepository : IProcessedEmailRepository
  {
      private readonly ApplicationDbContext _context;

      public ProcessedEmailRepository(ApplicationDbContext context)
      {
          _context = context;
      }

      public async Task<ProcessedEmail?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.ProcessedEmails.FirstOrDefaultAsync(pe => pe.Id == id, cancellationToken);

      public async Task<bool> ExistsByExternalMessageIdAsync(Guid emailAccountId, string externalMessageId, CancellationToken cancellationToken = default)
          => await _context.ProcessedEmails.AnyAsync(
              pe => pe.EmailAccountId == emailAccountId && pe.ExternalMessageId == externalMessageId,
              cancellationToken);

      public async Task<ProcessedEmail?> GetByExternalMessageIdAsync(Guid emailAccountId, string externalMessageId, CancellationToken cancellationToken = default)
          => await _context.ProcessedEmails.FirstOrDefaultAsync(
              pe => pe.EmailAccountId == emailAccountId && pe.ExternalMessageId == externalMessageId,
              cancellationToken);

      public async Task<(IEnumerable<ProcessedEmail> Emails, int TotalCount)> GetPagedByAccountAsync(
          Guid emailAccountId, int page, int pageSize,
          EmailProcessingStatus? statusFilter = null,
          EmailClassification? classificationFilter = null,
          string? searchTerm = null,
          CancellationToken cancellationToken = default)
      {
          var query = _context.ProcessedEmails
              .Where(pe => pe.EmailAccountId == emailAccountId);

          if (statusFilter.HasValue)
              query = query.Where(pe => pe.ProcessingStatus == statusFilter.Value);

          if (classificationFilter.HasValue)
              query = query.Where(pe => pe.Classification == classificationFilter.Value);

          if (!string.IsNullOrWhiteSpace(searchTerm))
          {
              var term = searchTerm.ToLowerInvariant();
              query = query.Where(pe =>
                  pe.Subject.ToLower().Contains(term) ||
                  pe.SenderEmail.ToLower().Contains(term) ||
                  (pe.SenderName != null && pe.SenderName.ToLower().Contains(term)));
          }

          var totalCount = await query.CountAsync(cancellationToken);
          var emails = await query
              .OrderByDescending(pe => pe.ReceivedAt)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync(cancellationToken);

          return (emails, totalCount);
      }

      public async Task<IEnumerable<ProcessedEmail>> GetAllAsync(CancellationToken cancellationToken = default)
          => await _context.ProcessedEmails.ToListAsync(cancellationToken);

      public async Task<ProcessedEmail> AddAsync(ProcessedEmail entity, CancellationToken cancellationToken = default)
      {
          await _context.ProcessedEmails.AddAsync(entity, cancellationToken);
          return entity;
      }

      public Task UpdateAsync(ProcessedEmail entity, CancellationToken cancellationToken = default)
      {
          _context.ProcessedEmails.Update(entity);
          return Task.CompletedTask;
      }

      public Task DeleteAsync(ProcessedEmail entity, CancellationToken cancellationToken = default)
      {
          _context.ProcessedEmails.Remove(entity);
          return Task.CompletedTask;
      }
  }
  ```

### 5.5 Register repositories in DI

- [ ] Update `Allocore.Infrastructure/DependencyInjection.cs`:
  ```csharp
  // Add registrations
  services.AddScoped<IEmailAccountRepository, EmailAccountRepository>();
  services.AddScoped<IProcessedEmailRepository, ProcessedEmailRepository>();
  ```

---

## Step 6: Application Layer — Service Interfaces

### 6.1 Create IEmailReaderService interface

- [ ] Create `Allocore.Application/Abstractions/Services/IEmailReaderService.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Services;

  public interface IEmailReaderService
  {
      Task<bool> ValidateConnectionAsync(string accessToken, CancellationToken cancellationToken = default);
      Task<IEnumerable<EmailMessageSummary>> GetUnreadEmailsAsync(string accessToken, int maxResults = 50, CancellationToken cancellationToken = default);
      Task<EmailMessageContent> GetEmailContentAsync(string accessToken, string messageId, CancellationToken cancellationToken = default);
      Task<IEnumerable<EmailAttachmentInfo>> GetEmailAttachmentsAsync(string accessToken, string messageId, CancellationToken cancellationToken = default);
      Task<Stream> DownloadAttachmentAsync(string accessToken, string messageId, string attachmentId, CancellationToken cancellationToken = default);
      Task MarkAsReadAsync(string accessToken, string messageId, CancellationToken cancellationToken = default);
      Task<string> CreateWebhookSubscriptionAsync(string accessToken, string notificationUrl, CancellationToken cancellationToken = default);
      Task RenewWebhookSubscriptionAsync(string accessToken, string subscriptionId, CancellationToken cancellationToken = default);
      Task DeleteWebhookSubscriptionAsync(string accessToken, string subscriptionId, CancellationToken cancellationToken = default);
  }

  public record EmailMessageSummary(
      string MessageId,
      string Subject,
      string SenderEmail,
      string? SenderName,
      DateTime ReceivedAt,
      bool HasAttachments
  );

  public record EmailMessageContent(
      string MessageId,
      string Subject,
      string SenderEmail,
      string? SenderName,
      DateTime ReceivedAt,
      string BodyText,
      string? BodyHtml,
      bool HasAttachments
  );

  public record EmailAttachmentInfo(
      string AttachmentId,
      string FileName,
      string ContentType,
      long SizeBytes
  );
  ```
  - **Note**: All methods take `accessToken` as parameter — the service is stateless. Token management is the caller's responsibility.
  - **Note**: Records for email data are defined alongside the interface for cohesion.

### 6.2 Create ILlmService interface

- [ ] Create `Allocore.Application/Abstractions/Services/ILlmService.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Services;

  using Allocore.Domain.Entities.EmailIntegration;

  public interface ILlmService
  {
      Task<EmailClassificationResult> ClassifyEmailAsync(string subject, string body, string senderEmail, CancellationToken cancellationToken = default);
      Task<PaymentDataExtractionResult> ExtractPaymentDataAsync(string subject, string body, string senderEmail, CancellationToken cancellationToken = default);
  }

  public record EmailClassificationResult(
      EmailClassification Classification,
      double Confidence,
      string? Reasoning
  );

  public record PaymentDataExtractionResult(
      string? ProviderName,
      decimal? Amount,
      string? Currency,
      DateTime? DueDate,
      string? InvoiceNumber,
      string? Description,
      double Confidence
  );
  ```
  - **Note**: `Confidence` is a 0.0–1.0 value indicating how confident the LLM is in its classification/extraction.
  - **Note**: All extracted fields are nullable — the LLM may not find all data in every email.
  - **Note**: Abstraction supports swapping LLM providers (Claude, GPT, local models) without changing application code.

### 6.3 Create IFileStorageService implementation (replaces US008 stub)

- [ ] Create `Allocore.Infrastructure/Services/S3FileStorageService.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Services;

  using Amazon.S3;
  using Amazon.S3.Model;
  using Allocore.Application.Abstractions.Services;
  using Microsoft.Extensions.Configuration;

  public class S3FileStorageService : IFileStorageService
  {
      private readonly IAmazonS3 _s3Client;
      private readonly string _bucketName;

      public S3FileStorageService(IAmazonS3 s3Client, IConfiguration configuration)
      {
          _s3Client = s3Client;
          _bucketName = configuration["EmailIntegration:FileStorage:BucketName"] ?? "allocare-attachments";
      }

      public async Task<string> UploadAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default)
      {
          var request = new PutObjectRequest
          {
              BucketName = _bucketName,
              Key = path,
              InputStream = content,
              ContentType = contentType
          };

          await _s3Client.PutObjectAsync(request, cancellationToken);
          return path;
      }

      public async Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
      {
          var response = await _s3Client.GetObjectAsync(_bucketName, path, cancellationToken);
          return response.ResponseStream;
      }

      public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
      {
          await _s3Client.DeleteObjectAsync(_bucketName, path, cancellationToken);
      }

      public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
      {
          try
          {
              await _s3Client.GetObjectMetadataAsync(_bucketName, path, cancellationToken);
              return true;
          }
          catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
          {
              return false;
          }
      }
  }
  ```
  - **Note**: Replaces `NotImplementedFileStorageService` from US008 in DI registration.
  - **Note**: Uses `AWSSDK.S3` which is compatible with MinIO (S3-compatible object storage).
  - **Note**: For local development, MinIO runs as a Docker container on `http://localhost:9000`.

---

## Step 7: Infrastructure Layer — External Service Implementations

### 7.1 Create Microsoft365EmailReaderService

- [ ] Create `Allocore.Infrastructure/Services/Microsoft365EmailReaderService.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Services;

  using Microsoft.Graph;
  using Microsoft.Graph.Models;
  using Microsoft.Kiota.Abstractions.Authentication;
  using Allocore.Application.Abstractions.Services;

  public class Microsoft365EmailReaderService : IEmailReaderService
  {
      public async Task<bool> ValidateConnectionAsync(string accessToken, CancellationToken cancellationToken = default)
      {
          var client = CreateGraphClient(accessToken);
          try
          {
              var me = await client.Me.GetAsync(cancellationToken: cancellationToken);
              return me != null;
          }
          catch
          {
              return false;
          }
      }

      public async Task<IEnumerable<EmailMessageSummary>> GetUnreadEmailsAsync(string accessToken, int maxResults = 50, CancellationToken cancellationToken = default)
      {
          var client = CreateGraphClient(accessToken);
          var messages = await client.Me.Messages.GetAsync(config =>
          {
              config.QueryParameters.Filter = "isRead eq false";
              config.QueryParameters.Top = maxResults;
              config.QueryParameters.Orderby = new[] { "receivedDateTime desc" };
              config.QueryParameters.Select = new[] { "id", "subject", "from", "receivedDateTime", "hasAttachments" };
          }, cancellationToken: cancellationToken);

          return messages?.Value?.Select(m => new EmailMessageSummary(
              m.Id!,
              m.Subject ?? string.Empty,
              m.From?.EmailAddress?.Address ?? string.Empty,
              m.From?.EmailAddress?.Name,
              m.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow,
              m.HasAttachments ?? false
          )) ?? Enumerable.Empty<EmailMessageSummary>();
      }

      public async Task<EmailMessageContent> GetEmailContentAsync(string accessToken, string messageId, CancellationToken cancellationToken = default)
      {
          var client = CreateGraphClient(accessToken);
          var message = await client.Me.Messages[messageId].GetAsync(cancellationToken: cancellationToken);

          return new EmailMessageContent(
              message!.Id!,
              message.Subject ?? string.Empty,
              message.From?.EmailAddress?.Address ?? string.Empty,
              message.From?.EmailAddress?.Name,
              message.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow,
              message.Body?.ContentType == BodyType.Text ? message.Body.Content ?? string.Empty : StripHtml(message.Body?.Content),
              message.Body?.ContentType == BodyType.Html ? message.Body.Content : null,
              message.HasAttachments ?? false
          );
      }

      public async Task<IEnumerable<EmailAttachmentInfo>> GetEmailAttachmentsAsync(string accessToken, string messageId, CancellationToken cancellationToken = default)
      {
          var client = CreateGraphClient(accessToken);
          var attachments = await client.Me.Messages[messageId].Attachments.GetAsync(cancellationToken: cancellationToken);

          return attachments?.Value?
              .OfType<FileAttachment>()
              .Select(a => new EmailAttachmentInfo(
                  a.Id!,
                  a.Name ?? "unknown",
                  a.ContentType ?? "application/octet-stream",
                  a.Size ?? 0
              )) ?? Enumerable.Empty<EmailAttachmentInfo>();
      }

      public async Task<Stream> DownloadAttachmentAsync(string accessToken, string messageId, string attachmentId, CancellationToken cancellationToken = default)
      {
          var client = CreateGraphClient(accessToken);
          var attachment = await client.Me.Messages[messageId].Attachments[attachmentId].GetAsync(cancellationToken: cancellationToken);

          if (attachment is FileAttachment fileAttachment && fileAttachment.ContentBytes != null)
              return new MemoryStream(fileAttachment.ContentBytes);

          return Stream.Null;
      }

      public async Task MarkAsReadAsync(string accessToken, string messageId, CancellationToken cancellationToken = default)
      {
          var client = CreateGraphClient(accessToken);
          await client.Me.Messages[messageId].PatchAsync(new Message { IsRead = true }, cancellationToken: cancellationToken);
      }

      public async Task<string> CreateWebhookSubscriptionAsync(string accessToken, string notificationUrl, CancellationToken cancellationToken = default)
      {
          var client = CreateGraphClient(accessToken);
          var subscription = await client.Subscriptions.PostAsync(new Subscription
          {
              ChangeType = "created",
              NotificationUrl = notificationUrl,
              Resource = "me/mailFolders('Inbox')/messages",
              ExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes(4230), // Max ~3 days
              ClientState = "allocare-email-webhook"
          }, cancellationToken: cancellationToken);

          return subscription!.Id!;
      }

      public async Task RenewWebhookSubscriptionAsync(string accessToken, string subscriptionId, CancellationToken cancellationToken = default)
      {
          var client = CreateGraphClient(accessToken);
          await client.Subscriptions[subscriptionId].PatchAsync(new Subscription
          {
              ExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes(4230)
          }, cancellationToken: cancellationToken);
      }

      public async Task DeleteWebhookSubscriptionAsync(string accessToken, string subscriptionId, CancellationToken cancellationToken = default)
      {
          var client = CreateGraphClient(accessToken);
          await client.Subscriptions[subscriptionId].DeleteAsync(cancellationToken: cancellationToken);
      }

      private static GraphServiceClient CreateGraphClient(string accessToken)
      {
          var authProvider = new BaseBearerTokenAuthenticationProvider(
              new TokenProvider(accessToken));
          return new GraphServiceClient(authProvider);
      }

      private static string StripHtml(string? html)
      {
          if (string.IsNullOrEmpty(html)) return string.Empty;
          // Basic HTML stripping — production should use a proper library
          return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ").Trim();
      }

      private class TokenProvider : IAccessTokenProvider
      {
          private readonly string _token;
          public TokenProvider(string token) => _token = token;
          public AllowedHostsValidator AllowedHostsValidator => new();
          public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
              => Task.FromResult(_token);
      }
  }
  ```
  - **Note**: Uses `Microsoft.Graph` v5.x SDK.
  - **Note**: Creates a new `GraphServiceClient` per call with the provided access token — stateless design.
  - **Note**: Webhook subscription max expiration is ~3 days for Mail resources. A renewal mechanism should be scheduled.
  - **Note**: `ClientState` is used to validate incoming webhook notifications.

### 7.2 Create ClaudeLlmService

- [ ] Create `Allocore.Infrastructure/Services/ClaudeLlmService.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Services;

  using System.Net.Http.Json;
  using System.Text.Json;
  using Allocore.Application.Abstractions.Services;
  using Allocore.Domain.Entities.EmailIntegration;
  using Microsoft.Extensions.Configuration;

  public class ClaudeLlmService : ILlmService
  {
      private readonly HttpClient _httpClient;
      private readonly string _model;

      public ClaudeLlmService(HttpClient httpClient, IConfiguration configuration)
      {
          _httpClient = httpClient;
          _model = configuration["EmailIntegration:Llm:Model"] ?? "claude-sonnet-4-20250514";

          var apiKey = configuration["EmailIntegration:Llm:ApiKey"];
          if (!string.IsNullOrEmpty(apiKey))
          {
              _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
              _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
          }
      }

      public async Task<EmailClassificationResult> ClassifyEmailAsync(string subject, string body, string senderEmail, CancellationToken cancellationToken = default)
      {
          var prompt = $"""
              Classify the following email into one of these categories:
              - PaymentNotification: A notification about a payment due (boleto, fatura, invoice)
              - OverdueNotice: A notice that a payment is overdue or past due
              - PaymentConfirmation: A confirmation that a payment was received/processed
              - InvoiceReceived: An invoice or billing statement received
              - Unrelated: Not related to payments, billing, or invoices
              - Unknown: Cannot determine the classification

              Email details:
              Subject: {subject}
              From: {senderEmail}
              Body: {body}

              Respond in JSON format:
              {{"classification": "CategoryName", "confidence": 0.95, "reasoning": "brief explanation"}}
              """;

          var response = await CallClaudeAsync(prompt, cancellationToken);
          var parsed = JsonSerializer.Deserialize<ClassificationResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

          var classification = Enum.TryParse<EmailClassification>(parsed?.Classification, true, out var cls)
              ? cls : EmailClassification.Unknown;

          return new EmailClassificationResult(
              classification,
              parsed?.Confidence ?? 0.0,
              parsed?.Reasoning
          );
      }

      public async Task<PaymentDataExtractionResult> ExtractPaymentDataAsync(string subject, string body, string senderEmail, CancellationToken cancellationToken = default)
      {
          var prompt = $"""
              Extract payment/billing data from the following email. Return all fields you can find.

              Email details:
              Subject: {subject}
              From: {senderEmail}
              Body: {body}

              Respond in JSON format:
              {{"providerName": "Company Name", "amount": 1500.00, "currency": "BRL", "dueDate": "2026-04-15", "invoiceNumber": "INV-001", "description": "Monthly service", "confidence": 0.90}}

              Rules:
              - providerName: The company/provider sending the bill (not the recipient)
              - amount: Numeric value only, no currency symbols
              - currency: 3-letter ISO code (default BRL if not specified)
              - dueDate: ISO 8601 format (YYYY-MM-DD)
              - All fields are optional — return null if not found
              """;

          var response = await CallClaudeAsync(prompt, cancellationToken);
          var parsed = JsonSerializer.Deserialize<ExtractionResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

          DateTime? dueDate = null;
          if (!string.IsNullOrEmpty(parsed?.DueDate) && DateTime.TryParse(parsed.DueDate, out var dt))
              dueDate = dt;

          return new PaymentDataExtractionResult(
              parsed?.ProviderName,
              parsed?.Amount,
              parsed?.Currency ?? "BRL",
              dueDate,
              parsed?.InvoiceNumber,
              parsed?.Description,
              parsed?.Confidence ?? 0.0
          );
      }

      private async Task<string> CallClaudeAsync(string prompt, CancellationToken cancellationToken)
      {
          var request = new
          {
              model = _model,
              max_tokens = 1024,
              messages = new[]
              {
                  new { role = "user", content = prompt }
              }
          };

          var response = await _httpClient.PostAsJsonAsync("https://api.anthropic.com/v1/messages", request, cancellationToken);
          response.EnsureSuccessStatusCode();

          var result = await response.Content.ReadFromJsonAsync<ClaudeResponse>(cancellationToken: cancellationToken);
          return result?.Content?.FirstOrDefault()?.Text ?? "{}";
      }

      private record ClaudeResponse(IEnumerable<ContentBlock>? Content);
      private record ContentBlock(string? Text);
      private record ClassificationResponse(string? Classification, double Confidence, string? Reasoning);
      private record ExtractionResponse(string? ProviderName, decimal? Amount, string? Currency, string? DueDate, string? InvoiceNumber, string? Description, double Confidence);
  }
  ```
  - **Note**: Uses raw HttpClient to call Claude API — avoids Anthropic SDK dependency for simplicity.
  - **Note**: Model defaults to `claude-sonnet-4-20250514` — configurable via `appsettings.json`.
  - **Note**: JSON parsing is lenient — if the LLM returns unexpected format, fields default to null/0.
  - **Note**: In production, add retry logic and rate limiting.

### 7.3 Register services in DI

- [ ] Update `Allocore.Infrastructure/DependencyInjection.cs`:
  ```csharp
  // Replace NotImplementedFileStorageService with S3FileStorageService
  services.AddScoped<IFileStorageService, S3FileStorageService>();

  // Add new service registrations
  services.AddScoped<IEmailReaderService, Microsoft365EmailReaderService>();

  // Configure HttpClient for Claude LLM
  services.AddHttpClient<ILlmService, ClaudeLlmService>();

  // Configure S3 client (MinIO-compatible)
  services.AddSingleton<IAmazonS3>(sp =>
  {
      var config = sp.GetRequiredService<IConfiguration>();
      var s3Config = new AmazonS3Config
      {
          ServiceURL = config["EmailIntegration:FileStorage:Endpoint"] ?? "http://localhost:9000",
          ForcePathStyle = true // Required for MinIO
      };
      return new AmazonS3Client(
          config["EmailIntegration:FileStorage:AccessKey"] ?? "minioadmin",
          config["EmailIntegration:FileStorage:SecretKey"] ?? "minioadmin",
          s3Config);
  });
  ```

### 7.4 Add NuGet packages

- [ ] Add required NuGet packages to `Allocore.Infrastructure`:
  ```bash
  dotnet add Allocore.Infrastructure package Microsoft.Graph --version 5.*
  dotnet add Allocore.Infrastructure package AWSSDK.S3
  ```

### 7.5 Add configuration section to appsettings

- [ ] Update `Allocore.API/appsettings.json` — add EmailIntegration section:
  ```json
  {
    "EmailIntegration": {
      "Microsoft365": {
        "ClientId": "",
        "ClientSecret": "",
        "TenantId": "common"
      },
      "Llm": {
        "Provider": "Claude",
        "ApiKey": "",
        "Model": "claude-sonnet-4-20250514"
      },
      "FileStorage": {
        "Endpoint": "http://localhost:9000",
        "BucketName": "allocare-attachments",
        "AccessKey": "minioadmin",
        "SecretKey": "minioadmin"
      }
    }
  }
  ```
  - **Note**: API keys should be empty in committed config — use user-secrets or environment variables for real values.

---

## Step 8: Application Layer — Commands, Queries & DTOs

### 8.1 Create DTOs

- [ ] Create `Allocore.Application/Features/EmailIntegration/DTOs/EmailAccountDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.EmailIntegration.DTOs;

  public record EmailAccountDto(
      Guid Id,
      Guid CompanyId,
      string EmailAddress,
      string ProviderType,
      string? DisplayName,
      bool IsActive,
      DateTime? LastSyncedAt,
      bool HasWebhookSubscription,
      DateTime CreatedAt,
      DateTime? UpdatedAt
  );
  ```
  - **Note**: Does NOT expose tokens — sensitive data stays in domain.

- [ ] Create `Allocore.Application/Features/EmailIntegration/DTOs/ProcessedEmailDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.EmailIntegration.DTOs;

  public record ProcessedEmailDto(
      Guid Id,
      string ExternalMessageId,
      string Subject,
      string SenderEmail,
      string? SenderName,
      DateTime ReceivedAt,
      string ProcessingStatus,
      string? Classification,
      string? ExtractedData,
      Guid? MatchedProviderId,
      Guid? MatchedPaymentId,
      string? ErrorMessage,
      DateTime? ProcessedAt,
      DateTime CreatedAt
  );
  ```

- [ ] Create `Allocore.Application/Features/EmailIntegration/DTOs/ProcessedEmailListItemDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.EmailIntegration.DTOs;

  public record ProcessedEmailListItemDto(
      Guid Id,
      string Subject,
      string SenderEmail,
      DateTime ReceivedAt,
      string ProcessingStatus,
      string? Classification,
      Guid? MatchedProviderId,
      string? MatchedProviderName,
      Guid? MatchedPaymentId,
      DateTime? ProcessedAt
  );
  ```

- [ ] Create `Allocore.Application/Features/EmailIntegration/DTOs/ConnectEmailRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.EmailIntegration.DTOs;

  public record ConnectEmailRequest(
      string EmailAddress,
      string AccessToken,
      string RefreshToken,
      DateTime TokenExpiresAt,
      string? DisplayName
  );
  ```
  - **Note**: In a real OAuth2 flow, the frontend exchanges the auth code for tokens and sends them here. The backend stores them.

### 8.2 ConnectEmail command

- [ ] Create `Allocore.Application/Features/EmailIntegration/Commands/ConnectEmailCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.EmailIntegration.Commands;

  using MediatR;
  using Allocore.Application.Features.EmailIntegration.DTOs;
  using Allocore.Domain.Common;

  public record ConnectEmailCommand(Guid CompanyId, ConnectEmailRequest Request) : IRequest<Result<EmailAccountDto>>;
  ```

- [ ] Create `Allocore.Application/Features/EmailIntegration/Commands/ConnectEmailCommandHandler.cs`:
  - Verify user access to company
  - Check if company already has an email account (1:1 constraint)
  - Validate connection via `IEmailReaderService.ValidateConnectionAsync`
  - Create `EmailAccount` entity
  - Update tokens via `UpdateTokens()`
  - Create webhook subscription via `IEmailReaderService.CreateWebhookSubscriptionAsync`
  - Set subscription ID on entity
  - Save and return `EmailAccountDto`

### 8.3 DisconnectEmail command

- [ ] Create `Allocore.Application/Features/EmailIntegration/Commands/DisconnectEmailCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.EmailIntegration.Commands;

  using MediatR;
  using Allocore.Domain.Common;

  public record DisconnectEmailCommand(Guid CompanyId) : IRequest<Result>;
  ```

- [ ] Create handler:
  - Verify user access to company
  - Load email account by company ID
  - If has subscription, delete webhook via `IEmailReaderService.DeleteWebhookSubscriptionAsync`
  - Delete email account entity (cascade deletes processed emails)
  - Save changes

### 8.4 ProcessIncomingEmail command (core pipeline)

- [ ] Create `Allocore.Application/Features/EmailIntegration/Commands/ProcessIncomingEmailCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.EmailIntegration.Commands;

  using MediatR;
  using Allocore.Domain.Common;

  public record ProcessIncomingEmailCommand(string SubscriptionId, string MessageId) : IRequest<Result>;
  ```

- [ ] Create `Allocore.Application/Features/EmailIntegration/Commands/ProcessIncomingEmailCommandHandler.cs`:
  ```csharp
  namespace Allocore.Application.Features.EmailIntegration.Commands;

  using MediatR;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Application.Abstractions.Services;
  using Allocore.Domain.Common;
  using Allocore.Domain.Entities.EmailIntegration;
  using Allocore.Domain.Entities.Payments;

  public class ProcessIncomingEmailCommandHandler : IRequestHandler<ProcessIncomingEmailCommand, Result>
  {
      private readonly IEmailAccountRepository _emailAccountRepository;
      private readonly IProcessedEmailRepository _processedEmailRepository;
      private readonly IPaymentRepository _paymentRepository;
      private readonly IRecurringPaymentRepository _recurringPaymentRepository;
      private readonly IProviderRepository _providerRepository;
      private readonly IEmailReaderService _emailReaderService;
      private readonly ILlmService _llmService;
      private readonly IFileStorageService _fileStorageService;
      private readonly IUnitOfWork _unitOfWork;

      public ProcessIncomingEmailCommandHandler(
          IEmailAccountRepository emailAccountRepository,
          IProcessedEmailRepository processedEmailRepository,
          IPaymentRepository paymentRepository,
          IRecurringPaymentRepository recurringPaymentRepository,
          IProviderRepository providerRepository,
          IEmailReaderService emailReaderService,
          ILlmService llmService,
          IFileStorageService fileStorageService,
          IUnitOfWork unitOfWork)
      {
          _emailAccountRepository = emailAccountRepository;
          _processedEmailRepository = processedEmailRepository;
          _paymentRepository = paymentRepository;
          _recurringPaymentRepository = recurringPaymentRepository;
          _providerRepository = providerRepository;
          _emailReaderService = emailReaderService;
          _llmService = llmService;
          _fileStorageService = fileStorageService;
          _unitOfWork = unitOfWork;
      }

      public async Task<Result> Handle(ProcessIncomingEmailCommand command, CancellationToken cancellationToken)
      {
          // 1. Find email account by subscription ID
          var emailAccount = await _emailAccountRepository.GetBySubscriptionIdAsync(command.SubscriptionId, cancellationToken);
          if (emailAccount is null || !emailAccount.IsActive)
              return Result.Failure("Email account not found or inactive.");

          // 2. Check if already processed (idempotency)
          if (await _processedEmailRepository.ExistsByExternalMessageIdAsync(emailAccount.Id, command.MessageId, cancellationToken))
              return Result.Success(); // Already processed — skip silently

          // 3. Fetch email content
          EmailMessageContent emailContent;
          try
          {
              emailContent = await _emailReaderService.GetEmailContentAsync(emailAccount.AccessToken!, command.MessageId, cancellationToken);
          }
          catch (Exception ex)
          {
              return Result.Failure($"Failed to fetch email: {ex.Message}");
          }

          // 4. Create ProcessedEmail record
          var processedEmail = ProcessedEmail.Create(
              emailAccount.Id,
              command.MessageId,
              emailContent.Subject,
              emailContent.SenderEmail,
              emailContent.SenderName,
              emailContent.ReceivedAt);

          processedEmail.MarkAsProcessing();
          await _processedEmailRepository.AddAsync(processedEmail, cancellationToken);
          await _unitOfWork.SaveChangesAsync(cancellationToken);

          try
          {
              // 5. Classify email via LLM
              var classification = await _llmService.ClassifyEmailAsync(
                  emailContent.Subject, emailContent.BodyText, emailContent.SenderEmail, cancellationToken);

              // 6. If not payment-related, mark as ignored
              if (classification.Classification == EmailClassification.Unrelated ||
                  classification.Classification == EmailClassification.Unknown)
              {
                  processedEmail.SetClassification(classification.Classification, null, null);
                  processedEmail.MarkAsIgnored();
                  await _unitOfWork.SaveChangesAsync(cancellationToken);
                  await _emailReaderService.MarkAsReadAsync(emailAccount.AccessToken!, command.MessageId, cancellationToken);
                  return Result.Success();
              }

              // 7. Extract payment data via LLM
              var extraction = await _llmService.ExtractPaymentDataAsync(
                  emailContent.Subject, emailContent.BodyText, emailContent.SenderEmail, cancellationToken);

              var extractedDataJson = System.Text.Json.JsonSerializer.Serialize(new
              {
                  extraction.ProviderName,
                  extraction.Amount,
                  extraction.Currency,
                  extraction.DueDate,
                  extraction.InvoiceNumber,
                  extraction.Description,
                  extraction.Confidence
              });

              // 8. Fuzzy match provider by name
              Guid? matchedProviderId = null;
              if (!string.IsNullOrEmpty(extraction.ProviderName))
              {
                  var providers = await _providerRepository.GetAllByCompanyAsync(emailAccount.CompanyId, cancellationToken);
                  var bestMatch = providers.FirstOrDefault(p =>
                      p.Name.Contains(extraction.ProviderName, StringComparison.OrdinalIgnoreCase) ||
                      extraction.ProviderName.Contains(p.Name, StringComparison.OrdinalIgnoreCase) ||
                      (p.LegalName != null && (
                          p.LegalName.Contains(extraction.ProviderName, StringComparison.OrdinalIgnoreCase) ||
                          extraction.ProviderName.Contains(p.LegalName, StringComparison.OrdinalIgnoreCase))));
                  matchedProviderId = bestMatch?.Id;
              }

              processedEmail.SetClassification(classification.Classification, extractedDataJson, matchedProviderId);

              // 9. If no provider matched, stay in Classified state (manual intervention required)
              if (!matchedProviderId.HasValue)
              {
                  await _unitOfWork.SaveChangesAsync(cancellationToken);
                  await _emailReaderService.MarkAsReadAsync(emailAccount.AccessToken!, command.MessageId, cancellationToken);
                  return Result.Success();
              }

              // 10. Try to match with recurring payment
              Guid? recurringPaymentId = null;
              if (extraction.Amount.HasValue)
              {
                  var recurringMatch = await _recurringPaymentRepository.FindMatchingAsync(
                      emailAccount.CompanyId, matchedProviderId.Value, extraction.Amount.Value, null, cancellationToken);
                  recurringPaymentId = recurringMatch?.Id;
              }

              // 11. Save email attachments to storage
              if (emailContent.HasAttachments)
              {
                  var attachments = await _emailReaderService.GetEmailAttachmentsAsync(
                      emailAccount.AccessToken!, command.MessageId, cancellationToken);

                  foreach (var att in attachments)
                  {
                      using var stream = await _emailReaderService.DownloadAttachmentAsync(
                          emailAccount.AccessToken!, command.MessageId, att.AttachmentId, cancellationToken);
                      var storagePath = $"companies/{emailAccount.CompanyId}/emails/{processedEmail.Id}/{att.FileName}";
                      await _fileStorageService.UploadAsync(storagePath, stream, att.ContentType, cancellationToken);
                  }
              }

              // 12. Create Payment
              var payment = Payment.Create(
                  emailAccount.CompanyId,
                  matchedProviderId.Value,
                  extraction.Description ?? emailContent.Subject,
                  extraction.Amount ?? 0,
                  extraction.Currency ?? "BRL",
                  extraction.DueDate ?? DateTime.UtcNow.AddDays(30),
                  PaymentMethod.BankSlip, // Default — can be refined
                  recurringPaymentId,
                  null,
                  extraction.InvoiceNumber,
                  $"Auto-created from email: {emailContent.Subject}");

              await _paymentRepository.AddAsync(payment, cancellationToken);

              // 13. Update processed email with payment link
              processedEmail.SetPaymentCreated(payment.Id);
              await _unitOfWork.SaveChangesAsync(cancellationToken);

              // 14. Mark email as read
              await _emailReaderService.MarkAsReadAsync(emailAccount.AccessToken!, command.MessageId, cancellationToken);

              return Result.Success();
          }
          catch (Exception ex)
          {
              processedEmail.MarkAsError(ex.Message);
              await _unitOfWork.SaveChangesAsync(cancellationToken);
              return Result.Failure($"Email processing failed: {ex.Message}");
          }
      }
  }
  ```
  - **Business rule**: Idempotent — if `ExternalMessageId` already exists for this account, skip silently.
  - **Business rule**: Non-payment emails (Unrelated, Unknown) are classified then ignored.
  - **Business rule**: If provider not found, email stays in `Classified` status without creating a Payment. Manual intervention required via `ReprocessEmail`.
  - **Business rule**: Default `PaymentMethod` is `BankSlip` for auto-created payments — most common in Brazil.
  - **Business rule**: If `Amount` is null from extraction, defaults to 0 (requires manual correction).
  - **Business rule**: If `DueDate` is null, defaults to 30 days from now.
  - **Note**: The fuzzy matching uses simple `Contains` — a more sophisticated matching (Levenshtein distance, etc.) can be added in a future iteration.
  - **Note**: Email attachments are stored separately from Payment attachments. Future enhancement could link them.

### 8.5 ReprocessEmail command

- [ ] Create `Allocore.Application/Features/EmailIntegration/Commands/ReprocessEmailCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.EmailIntegration.Commands;

  using MediatR;
  using Allocore.Domain.Common;

  public record ReprocessEmailCommand(Guid CompanyId, Guid EmailId) : IRequest<Result>;
  ```

- [ ] Create handler:
  - Verify user access to company
  - Load processed email, verify its email account belongs to the company
  - Call `processedEmail.ResetForReprocessing()`
  - Re-run the same pipeline logic (or dispatch `ProcessIncomingEmailCommand` with the same subscription/message IDs)
  - Save changes

### 8.6 GetEmailAccount query

- [ ] Create `Allocore.Application/Features/EmailIntegration/Queries/GetEmailAccountQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.EmailIntegration.Queries;

  using MediatR;
  using Allocore.Application.Features.EmailIntegration.DTOs;
  using Allocore.Domain.Common;

  public record GetEmailAccountQuery(Guid CompanyId) : IRequest<Result<EmailAccountDto?>>;
  ```

- [ ] Create handler — verify access, load email account by company ID, map to DTO.

### 8.7 GetProcessedEmailsPaged query

- [ ] Create `Allocore.Application/Features/EmailIntegration/Queries/GetProcessedEmailsPagedQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.EmailIntegration.Queries;

  using MediatR;
  using Allocore.Application.Common;
  using Allocore.Application.Features.EmailIntegration.DTOs;

  public record GetProcessedEmailsPagedQuery(
      Guid CompanyId,
      int Page = 1,
      int PageSize = 20,
      string? Status = null,
      string? Classification = null,
      string? SearchTerm = null
  ) : IRequest<PagedResult<ProcessedEmailListItemDto>>;
  ```

- [ ] Create handler — verify access, load email account, parse filters, call repository, map to list DTOs.

### 8.8 GetProcessedEmailById query

- [ ] Create `Allocore.Application/Features/EmailIntegration/Queries/GetProcessedEmailByIdQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.EmailIntegration.Queries;

  using MediatR;
  using Allocore.Application.Features.EmailIntegration.DTOs;
  using Allocore.Domain.Common;

  public record GetProcessedEmailByIdQuery(Guid CompanyId, Guid EmailId) : IRequest<Result<ProcessedEmailDto>>;
  ```

- [ ] Create handler — verify access, load processed email, verify it belongs to company's email account, map to DTO.

---

## Step 9: API Layer — Controllers

### 9.1 EmailIntegrationController

- [ ] Create `Allocore.API/Controllers/v1/EmailIntegrationController.cs`:
  ```csharp
  namespace Allocore.API.Controllers.v1;

  using Asp.Versioning;
  using MediatR;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Allocore.Application.Features.EmailIntegration.Commands;
  using Allocore.Application.Features.EmailIntegration.DTOs;
  using Allocore.Application.Features.EmailIntegration.Queries;

  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/companies/{companyId:guid}/email-integration")]
  [Authorize]
  public class EmailIntegrationController : ControllerBase
  {
      private readonly IMediator _mediator;

      public EmailIntegrationController(IMediator mediator)
      {
          _mediator = mediator;
      }

      /// <summary>
      /// Get the email account configuration for a company.
      /// </summary>
      [HttpGet("account")]
      public async Task<IActionResult> GetEmailAccount(Guid companyId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new GetEmailAccountQuery(companyId), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          if (result.Value is null)
              return NotFound(new { error = "No email account configured for this company." });
          return Ok(result.Value);
      }

      /// <summary>
      /// Connect an email account to a company (OAuth2 tokens provided by frontend).
      /// </summary>
      [HttpPost("connect")]
      public async Task<IActionResult> ConnectEmail(
          Guid companyId,
          [FromBody] ConnectEmailRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new ConnectEmailCommand(companyId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Disconnect the email account from a company.
      /// </summary>
      [HttpDelete("disconnect")]
      public async Task<IActionResult> DisconnectEmail(Guid companyId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new DisconnectEmailCommand(companyId), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }

      /// <summary>
      /// List processed emails for a company (paginated, filterable).
      /// </summary>
      [HttpGet("emails")]
      public async Task<IActionResult> GetProcessedEmails(
          Guid companyId,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 20,
          [FromQuery] string? status = null,
          [FromQuery] string? classification = null,
          [FromQuery] string? search = null,
          CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(
              new GetProcessedEmailsPagedQuery(companyId, page, pageSize, status, classification, search),
              cancellationToken);
          return Ok(result);
      }

      /// <summary>
      /// Get a processed email by ID with full details.
      /// </summary>
      [HttpGet("emails/{emailId:guid}")]
      public async Task<IActionResult> GetProcessedEmail(Guid companyId, Guid emailId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new GetProcessedEmailByIdQuery(companyId, emailId), cancellationToken);
          if (!result.IsSuccess)
              return NotFound(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Reprocess a failed or classified-without-payment email.
      /// </summary>
      [HttpPost("emails/{emailId:guid}/reprocess")]
      public async Task<IActionResult> ReprocessEmail(Guid companyId, Guid emailId, CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new ReprocessEmailCommand(companyId, emailId), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(new { message = "Email reprocessing initiated." });
      }
  }
  ```
  - **Note**: Route is nested under company: `/api/v1/companies/{companyId}/email-integration`.
  - **Note**: All endpoints require JWT authentication.
  - **Note**: The `connect` endpoint expects OAuth2 tokens already exchanged by the frontend.

### 9.2 WebhookController (public — no JWT)

- [ ] Create `Allocore.API/Controllers/WebhookController.cs`:
  ```csharp
  namespace Allocore.API.Controllers;

  using MediatR;
  using Microsoft.AspNetCore.Mvc;
  using System.Text.Json;
  using Allocore.Application.Features.EmailIntegration.Commands;

  [ApiController]
  [Route("webhooks")]
  public class WebhookController : ControllerBase
  {
      private readonly IMediator _mediator;

      public WebhookController(IMediator mediator)
      {
          _mediator = mediator;
      }

      /// <summary>
      /// Receives webhook notifications from Microsoft Graph.
      /// Public endpoint — no JWT required. Validated via clientState.
      /// </summary>
      [HttpPost("microsoft-graph")]
      public async Task<IActionResult> MicrosoftGraphWebhook(CancellationToken cancellationToken)
      {
          // Handle Graph validation request
          var validationToken = HttpContext.Request.Query["validationToken"].FirstOrDefault();
          if (!string.IsNullOrEmpty(validationToken))
          {
              return Content(validationToken, "text/plain");
          }

          // Parse notification payload
          using var reader = new StreamReader(Request.Body);
          var body = await reader.ReadToEndAsync(cancellationToken);
          var payload = JsonSerializer.Deserialize<GraphNotificationPayload>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

          if (payload?.Value == null)
              return Ok(); // Nothing to process

          foreach (var notification in payload.Value)
          {
              // Validate clientState
              if (notification.ClientState != "allocare-email-webhook")
                  continue;

              // Extract message ID from resource path: "Messages/{messageId}"
              var resourceParts = notification.Resource?.Split('/');
              var messageId = resourceParts?.LastOrDefault();

              if (string.IsNullOrEmpty(messageId) || string.IsNullOrEmpty(notification.SubscriptionId))
                  continue;

              // Fire and forget — process asynchronously
              _ = _mediator.Send(new ProcessIncomingEmailCommand(notification.SubscriptionId, messageId), CancellationToken.None);
          }

          return Ok();
      }

      private record GraphNotificationPayload(IEnumerable<GraphNotification>? Value);
      private record GraphNotification(string? SubscriptionId, string? ClientState, string? Resource, string? ChangeType);
  }
  ```
  - **Note**: This endpoint is PUBLIC — no `[Authorize]` attribute. Microsoft Graph cannot send JWT tokens.
  - **Note**: Validation is done via `clientState` matching.
  - **Note**: Graph sends a validation request on subscription creation — must return `validationToken` as plain text.
  - **Note**: Email processing is fire-and-forget — the webhook must return 200 quickly (Graph has a 3-second timeout).
  - **Note**: NOT under `/api/v{version}` prefix — webhooks are version-agnostic.

### 9.3 Build, Verify & Manual Test

- [ ] Run `dotnet build` — ensure entire solution compiles
- [ ] Apply migration: `dotnet ef database update -s Allocore.API -p Allocore.Infrastructure`
- [ ] Run application and verify Swagger shows all new endpoints
- [ ] Manual test via Swagger:
  1. Get email account (should be 404 — no account yet)
  2. Connect email account (mock tokens) → 200
  3. Get email account → 200 with config
  4. List processed emails → 200 (empty)
  5. Disconnect email → 204
  6. Verify webhook validation endpoint works (GET with validationToken)
  7. Verify wrong company returns error

---

## Technical Details

### Dependencies

New NuGet packages:

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Graph` | 5.x | Microsoft 365 email access via Graph API |
| `AWSSDK.S3` | latest | S3-compatible object storage (MinIO for dev) |

Claude API accessed via raw `HttpClient` — no additional package needed.

### Project Structure — Affected Files

| Layer | File | Change |
|-------|------|--------|
| **Domain** | `Allocore.Domain/Entities/EmailIntegration/EmailProviderType.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/EmailIntegration/EmailProcessingStatus.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/EmailIntegration/EmailClassification.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/EmailIntegration/EmailAccount.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/EmailIntegration/ProcessedEmail.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Configurations/EmailAccountConfiguration.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Configurations/ProcessedEmailConfiguration.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs` | **Update** — add DbSets |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Repositories/EmailAccountRepository.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Repositories/ProcessedEmailRepository.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Services/Microsoft365EmailReaderService.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Services/ClaudeLlmService.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Services/S3FileStorageService.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/DependencyInjection.cs` | **Update** — add registrations, replace file storage |
| **Application** | `Allocore.Application/Abstractions/Persistence/IEmailAccountRepository.cs` | **Create** |
| **Application** | `Allocore.Application/Abstractions/Persistence/IProcessedEmailRepository.cs` | **Create** |
| **Application** | `Allocore.Application/Abstractions/Services/IEmailReaderService.cs` | **Create** |
| **Application** | `Allocore.Application/Abstractions/Services/ILlmService.cs` | **Create** |
| **Application** | `Allocore.Application/Features/EmailIntegration/DTOs/*.cs` | **Create** (4 files) |
| **Application** | `Allocore.Application/Features/EmailIntegration/Commands/*.cs` | **Create** (6 files — 3 commands + 3 handlers) |
| **Application** | `Allocore.Application/Features/EmailIntegration/Queries/*.cs` | **Create** (6 files — 3 queries + 3 handlers) |
| **API** | `Allocore.API/Controllers/v1/EmailIntegrationController.cs` | **Create** |
| **API** | `Allocore.API/Controllers/WebhookController.cs` | **Create** |
| **API** | `Allocore.API/appsettings.json` | **Update** — add EmailIntegration section |

### Database

**Table: EmailAccounts**

| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| Id | uuid | NO | gen | PK |
| CompanyId | uuid | NO | — | UNIQUE |
| EmailAddress | varchar(254) | NO | — | |
| ProviderType | varchar(50) | NO | — | |
| DisplayName | varchar(200) | YES | — | |
| IsActive | boolean | NO | true | |
| LastSyncedAt | timestamp | YES | — | |
| AccessToken | varchar(4000) | YES | — | |
| RefreshToken | varchar(4000) | YES | — | |
| TokenExpiresAt | timestamp | YES | — | |
| SubscriptionId | varchar(500) | YES | — | |
| CreatedAt | timestamp | NO | — | |
| UpdatedAt | timestamp | YES | — | |

**Indexes:**
- `IX_EmailAccounts_CompanyId` (UNIQUE)

**Table: ProcessedEmails**

| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| Id | uuid | NO | gen | PK |
| EmailAccountId | uuid | NO | — | FK → EmailAccounts (CASCADE) |
| ExternalMessageId | varchar(500) | NO | — | |
| Subject | varchar(1000) | NO | — | |
| SenderEmail | varchar(254) | NO | — | |
| SenderName | varchar(200) | YES | — | |
| ReceivedAt | timestamp | NO | — | |
| ProcessingStatus | varchar(50) | NO | Pending | |
| Classification | varchar(50) | YES | — | |
| ExtractedData | jsonb | YES | — | |
| MatchedProviderId | uuid | YES | — | (lookup ID, no FK) |
| MatchedPaymentId | uuid | YES | — | (lookup ID, no FK) |
| ErrorMessage | varchar(2000) | YES | — | |
| ProcessedAt | timestamp | YES | — | |
| CreatedAt | timestamp | NO | — | |
| UpdatedAt | timestamp | YES | — | |

**Indexes:**
- `IX_ProcessedEmails_EmailAccountId_ExternalMessageId` (UNIQUE)
- `IX_ProcessedEmails_ProcessingStatus`
- `IX_ProcessedEmails_MatchedProviderId`
- `IX_ProcessedEmails_MatchedPaymentId`

### API Contract

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/v1/companies/{companyId}/email-integration/account` | GET | Yes | Get email account config |
| `/api/v1/companies/{companyId}/email-integration/connect` | POST | Yes | Connect email (OAuth2 flow) |
| `/api/v1/companies/{companyId}/email-integration/disconnect` | DELETE | Yes | Disconnect email account |
| `/api/v1/companies/{companyId}/email-integration/emails` | GET | Yes | List processed emails (paged) |
| `/api/v1/companies/{companyId}/email-integration/emails/{emailId}` | GET | Yes | Get processed email details |
| `/api/v1/companies/{companyId}/email-integration/emails/{emailId}/reprocess` | POST | Yes | Reprocess a failed email |
| `/webhooks/microsoft-graph` | POST | **No** | Microsoft Graph webhook |

### Authentication/Authorization

- All `/email-integration` endpoints require JWT Bearer authentication
- The `/webhooks/microsoft-graph` endpoint is PUBLIC — validated via `clientState` field
- Fine-grained role checks deferred to a future authorization story

### Configuration

```json
{
  "EmailIntegration": {
    "Microsoft365": {
      "ClientId": "",
      "ClientSecret": "",
      "TenantId": "common"
    },
    "Llm": {
      "Provider": "Claude",
      "ApiKey": "",
      "Model": "claude-sonnet-4-20250514"
    },
    "FileStorage": {
      "Endpoint": "http://localhost:9000",
      "BucketName": "allocare-attachments",
      "AccessKey": "minioadmin",
      "SecretKey": "minioadmin"
    }
  }
}
```

---

## Acceptance Criteria

- [ ] Email account can be connected and disconnected per company (1:1)
- [ ] Microsoft Graph webhook receives new email notifications in real-time
- [ ] Incoming emails are classified by AI (Claude) into payment-related categories
- [ ] Payment data (amount, due date, invoice number, provider name) is extracted from payment-related emails
- [ ] Provider matching uses fuzzy name comparison against existing providers
- [ ] Payment is automatically created when provider is matched
- [ ] Emails without provider match stay in `Classified` status for manual intervention
- [ ] Duplicate emails are not processed (idempotent via ExternalMessageId)
- [ ] Non-payment emails are classified and marked as `Ignored`
- [ ] Failed emails can be reprocessed manually
- [ ] Email attachments are stored in S3/MinIO object storage
- [ ] `IFileStorageService` real implementation (S3) replaces US008 stub
- [ ] Processed emails are listable with pagination, status, and classification filters
- [ ] Webhook validation (Graph subscription creation) returns validationToken correctly
- [ ] All API endpoints visible in Swagger
- [ ] Migrations created and applied (`AddEmailIntegration`)
- [ ] `dotnet build` passes without errors

---

## What is explicitly NOT changing?

- **Authentication/Authorization model** — no new roles or policies added
- **Payment entity** — no changes (created in US008, consumed here)
- **Provider entity** — no changes (created in US004, queried here)
- **Existing endpoints** — no modifications to Payments, Providers, Auth, or Companies controllers
- **Gmail support** — enum value exists but no implementation
- **Webhook renewal background job** — manual renewal only in this story
- **Advanced provider matching** — basic `Contains` matching; Levenshtein/ML deferred

---

## Follow-ups (Intentionally Deferred)

| Item | Reason | Related Story |
|------|--------|---------------|
| Gmail support (IEmailReaderService for Gmail) | Only M365 needed now | Future US |
| Webhook subscription auto-renewal background job | Graph subscriptions expire in ~3 days | Future US |
| Advanced provider matching (Levenshtein, ML) | Basic Contains is sufficient for MVP | Future US |
| Token refresh flow (OAuth2 refresh_token grant) | Token refresh logic not implemented | Future US |
| Email attachment → Payment attachment linking | Different storage paths currently | Future US |
| Manual provider association UI for unmatched emails | Frontend story | Future USFW |
| Rate limiting on webhook endpoint | Nice-to-have for production | Future US |
| LLM response caching / cost optimization | Operational concern | Future US |
| Email processing queue (background job instead of fire-and-forget) | Reliability improvement | Future US |
| Confidence threshold configuration | Currently no minimum confidence check | Future US |
