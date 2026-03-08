# US006 – Notes & Activity Timeline

## Description

**As** an Admin user managing providers and contracts in Allocore,
**I need** to attach notes to providers, contracts, and other entities — recording interactions, negotiation history, decisions, reminders, and general observations,
**So that** I have a chronological timeline of everything that happened with a provider or contract, making it easy to onboard new team members, recall past decisions, and track the relationship lifecycle.

Currently, Allocore has no way to record free-text notes or activity history on any entity. This story introduces a **polymorphic Note entity** that can be attached to any entity type (Provider, Contract, and future entities) using a `EntityType` + `EntityId` pattern. Notes form a timeline — ordered by creation date — and support categorization (e.g., "Negotiation", "Meeting", "Decision", "Reminder", "General").

**Priority**: Medium
**Dependencies**: US004 – Provider Management, US005 – Provider Contracts

---

## Step 1: Domain Layer — Note Entity & Enums

### 1.1 Create NoteEntityType enum

- [ ] Create `Allocore.Domain/Entities/Notes/NoteEntityType.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Notes;

  public enum NoteEntityType
  {
      Provider = 0,
      Contract = 1
      // Future: Employee, CostCenter, Project, Service, Cost
  }
  ```
  - **Business rule**: Every note must be linked to exactly one entity via `EntityType` + `EntityId`.
  - **Note**: New entity types can be added to this enum as the platform grows. The note system is designed to be extensible without schema changes.

### 1.2 Create NoteCategory enum

- [ ] Create `Allocore.Domain/Entities/Notes/NoteCategory.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Notes;

  public enum NoteCategory
  {
      General = 0,
      Negotiation = 1,
      Meeting = 2,
      Decision = 3,
      Reminder = 4,
      Issue = 5,
      FollowUp = 6,
      PhoneCall = 7,
      Email = 8,
      InternalDiscussion = 9
  }
  ```
  - **Note**: Categories help filter and visually distinguish notes in the timeline. They are not enforced as workflow — just labels.

### 1.3 Create Note entity

- [ ] Create `Allocore.Domain/Entities/Notes/Note.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Notes;

  using Allocore.Domain.Common;

  public class Note : Entity
  {
      public Guid CompanyId { get; private set; }
      public NoteEntityType EntityType { get; private set; }
      public Guid EntityId { get; private set; }
      public Guid AuthorUserId { get; private set; }
      public string Content { get; private set; } = string.Empty;
      public NoteCategory Category { get; private set; } = NoteCategory.General;
      public bool IsPinned { get; private set; }
      public DateTime? ReminderDate { get; private set; }

      private Note() { } // EF Core

      public static Note Create(
          Guid companyId,
          NoteEntityType entityType,
          Guid entityId,
          Guid authorUserId,
          string content,
          NoteCategory category = NoteCategory.General,
          bool isPinned = false,
          DateTime? reminderDate = null)
      {
          return new Note
          {
              CompanyId = companyId,
              EntityType = entityType,
              EntityId = entityId,
              AuthorUserId = authorUserId,
              Content = content,
              Category = category,
              IsPinned = isPinned,
              ReminderDate = reminderDate
          };
      }

      public void Update(string content, NoteCategory category, bool isPinned, DateTime? reminderDate)
      {
          Content = content;
          Category = category;
          IsPinned = isPinned;
          ReminderDate = reminderDate;
          UpdatedAt = DateTime.UtcNow;
      }

      public void Pin()
      {
          IsPinned = true;
          UpdatedAt = DateTime.UtcNow;
      }

      public void Unpin()
      {
          IsPinned = false;
          UpdatedAt = DateTime.UtcNow;
      }
  }
  ```
  - **Business rule**: `CompanyId` is required — notes are tenant-scoped.
  - **Business rule**: `EntityType` + `EntityId` together identify the parent entity. This is a polymorphic association — no FK constraint at the DB level.
  - **Business rule**: `AuthorUserId` is the user who created the note. Required and immutable.
  - **Business rule**: `Content` is required, max 10,000 chars (supports rich text / markdown).
  - **Business rule**: `IsPinned` notes appear at the top of the timeline regardless of date.
  - **Business rule**: `ReminderDate` is optional — when set, the note acts as a reminder for that date. No automated notification in this story.
  - **Note**: `EntityId` does NOT have a FK constraint because it can point to different tables depending on `EntityType`. Referential integrity is enforced at the application layer.
  - **Note**: Only the author can edit/delete their own notes (enforced at application layer). Admins can delete any note.

---

## Step 2: Infrastructure Layer — EF Core Configuration

### 2.1 Note configuration

- [ ] Create `Allocore.Infrastructure/Persistence/Configurations/NoteConfiguration.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Configurations;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using Allocore.Domain.Entities.Notes;

  public class NoteConfiguration : IEntityTypeConfiguration<Note>
  {
      public void Configure(EntityTypeBuilder<Note> builder)
      {
          builder.ToTable("Notes");

          builder.HasKey(n => n.Id);

          builder.Property(n => n.CompanyId)
              .IsRequired();

          builder.Property(n => n.EntityType)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(n => n.EntityId)
              .IsRequired();

          builder.Property(n => n.AuthorUserId)
              .IsRequired();

          builder.Property(n => n.Content)
              .IsRequired()
              .HasMaxLength(10000);

          builder.Property(n => n.Category)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          builder.Property(n => n.IsPinned)
              .IsRequired()
              .HasDefaultValue(false);

          // Composite index for querying notes by entity
          builder.HasIndex(n => new { n.EntityType, n.EntityId });

          builder.HasIndex(n => n.CompanyId);
          builder.HasIndex(n => n.AuthorUserId);
          builder.HasIndex(n => n.Category);
          builder.HasIndex(n => n.ReminderDate)
              .HasFilter("\"ReminderDate\" IS NOT NULL");

          // No FK on EntityId — polymorphic association
          // No FK on AuthorUserId — avoid cascade complexity; enforce at app layer
      }
  }
  ```
  - **Note**: No foreign key on `EntityId` because it points to different tables. The composite index `(EntityType, EntityId)` is the primary query path.
  - **Note**: No FK on `AuthorUserId` to avoid cascade delete complexity. If a user is deleted, their notes remain (orphaned author is acceptable — display "Deleted User").

### 2.2 Update ApplicationDbContext

- [ ] Update `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs` — add DbSet:
  ```csharp
  using Allocore.Domain.Entities.Notes;

  public DbSet<Note> Notes => Set<Note>();
  ```

### 2.3 Create migration

- [ ] Run migration:
  ```bash
  dotnet ef migrations add AddNotes -s Allocore.API -p Allocore.Infrastructure
  ```
  - **Impact on existing data**: No existing rows affected. One new table created.

---

## Step 3: Infrastructure Layer — Repository

### 3.1 Create INoteRepository interface

- [ ] Create `Allocore.Application/Abstractions/Persistence/INoteRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;

  using Allocore.Domain.Entities.Notes;

  public interface INoteRepository : IReadRepository<Note>, IWriteRepository<Note>
  {
      Task<(IEnumerable<Note> Notes, int TotalCount)> GetPagedByEntityAsync(
          NoteEntityType entityType,
          Guid entityId,
          int page,
          int pageSize,
          NoteCategory? categoryFilter = null,
          CancellationToken cancellationToken = default);
      Task<IEnumerable<Note>> GetPinnedByEntityAsync(
          NoteEntityType entityType,
          Guid entityId,
          CancellationToken cancellationToken = default);
      Task<IEnumerable<Note>> GetRemindersForCompanyAsync(
          Guid companyId,
          DateTime fromDate,
          DateTime toDate,
          CancellationToken cancellationToken = default);
      Task<int> GetCountByEntityAsync(
          NoteEntityType entityType,
          Guid entityId,
          CancellationToken cancellationToken = default);
  }
  ```

### 3.2 Create NoteRepository implementation

- [ ] Create `Allocore.Infrastructure/Persistence/Repositories/NoteRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.Repositories;

  using Microsoft.EntityFrameworkCore;
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Entities.Notes;

  public class NoteRepository : INoteRepository
  {
      private readonly ApplicationDbContext _context;

      public NoteRepository(ApplicationDbContext context)
      {
          _context = context;
      }

      public async Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
          => await _context.Notes.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

      public async Task<(IEnumerable<Note> Notes, int TotalCount)> GetPagedByEntityAsync(
          NoteEntityType entityType,
          Guid entityId,
          int page,
          int pageSize,
          NoteCategory? categoryFilter = null,
          CancellationToken cancellationToken = default)
      {
          var query = _context.Notes
              .Where(n => n.EntityType == entityType && n.EntityId == entityId);

          if (categoryFilter.HasValue)
              query = query.Where(n => n.Category == categoryFilter.Value);

          var totalCount = await query.CountAsync(cancellationToken);

          // Pinned notes first, then by CreatedAt descending (newest first)
          var notes = await query
              .OrderByDescending(n => n.IsPinned)
              .ThenByDescending(n => n.CreatedAt)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync(cancellationToken);

          return (notes, totalCount);
      }

      public async Task<IEnumerable<Note>> GetPinnedByEntityAsync(
          NoteEntityType entityType,
          Guid entityId,
          CancellationToken cancellationToken = default)
          => await _context.Notes
              .Where(n => n.EntityType == entityType && n.EntityId == entityId && n.IsPinned)
              .OrderByDescending(n => n.CreatedAt)
              .ToListAsync(cancellationToken);

      public async Task<IEnumerable<Note>> GetRemindersForCompanyAsync(
          Guid companyId,
          DateTime fromDate,
          DateTime toDate,
          CancellationToken cancellationToken = default)
          => await _context.Notes
              .Where(n => n.CompanyId == companyId
                  && n.ReminderDate != null
                  && n.ReminderDate >= fromDate
                  && n.ReminderDate <= toDate)
              .OrderBy(n => n.ReminderDate)
              .ToListAsync(cancellationToken);

      public async Task<int> GetCountByEntityAsync(
          NoteEntityType entityType,
          Guid entityId,
          CancellationToken cancellationToken = default)
          => await _context.Notes
              .CountAsync(n => n.EntityType == entityType && n.EntityId == entityId, cancellationToken);

      public async Task<IEnumerable<Note>> GetAllAsync(CancellationToken cancellationToken = default)
          => await _context.Notes.ToListAsync(cancellationToken);

      public async Task<Note> AddAsync(Note entity, CancellationToken cancellationToken = default)
      {
          await _context.Notes.AddAsync(entity, cancellationToken);
          return entity;
      }

      public Task UpdateAsync(Note entity, CancellationToken cancellationToken = default)
      {
          _context.Notes.Update(entity);
          return Task.CompletedTask;
      }

      public Task DeleteAsync(Note entity, CancellationToken cancellationToken = default)
      {
          _context.Notes.Remove(entity);
          return Task.CompletedTask;
      }
  }
  ```

### 3.3 Register in DI

- [ ] Update `Allocore.Infrastructure/DependencyInjection.cs`:
  ```csharp
  services.AddScoped<INoteRepository, NoteRepository>();
  ```

---

## Step 4: Application Layer — DTOs

- [ ] Create `Allocore.Application/Features/Notes/DTOs/NoteDto.cs`:
  ```csharp
  namespace Allocore.Application.Features.Notes.DTOs;

  public record NoteDto(
      Guid Id,
      string EntityType,
      Guid EntityId,
      Guid AuthorUserId,
      string AuthorName,
      string Content,
      string Category,
      bool IsPinned,
      DateTime? ReminderDate,
      DateTime CreatedAt,
      DateTime? UpdatedAt
  );
  ```
  - **Note**: `AuthorName` is resolved by joining with the User table at query time.

- [ ] Create `Allocore.Application/Features/Notes/DTOs/CreateNoteRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Notes.DTOs;

  public record CreateNoteRequest(
      string Content,
      string? Category,
      bool IsPinned,
      DateTime? ReminderDate
  );
  ```

- [ ] Create `Allocore.Application/Features/Notes/DTOs/UpdateNoteRequest.cs`:
  ```csharp
  namespace Allocore.Application.Features.Notes.DTOs;

  public record UpdateNoteRequest(
      string Content,
      string? Category,
      bool IsPinned,
      DateTime? ReminderDate
  );
  ```

---

## Step 5: Application Layer — Validators

- [ ] Create `Allocore.Application/Features/Notes/Validators/CreateNoteRequestValidator.cs`:
  ```csharp
  namespace Allocore.Application.Features.Notes.Validators;

  using FluentValidation;
  using Allocore.Application.Features.Notes.DTOs;
  using Allocore.Domain.Entities.Notes;

  public class CreateNoteRequestValidator : AbstractValidator<CreateNoteRequest>
  {
      public CreateNoteRequestValidator()
      {
          RuleFor(x => x.Content)
              .NotEmpty().WithMessage("Note content is required")
              .MaximumLength(10000).WithMessage("Note content must not exceed 10,000 characters");

          RuleFor(x => x.Category)
              .Must(c => c == null || Enum.TryParse<NoteCategory>(c, true, out _))
              .WithMessage("Category must be one of: General, Negotiation, Meeting, Decision, Reminder, Issue, FollowUp, PhoneCall, Email, InternalDiscussion");

          RuleFor(x => x.ReminderDate)
              .GreaterThan(DateTime.UtcNow).WithMessage("Reminder date must be in the future")
              .When(x => x.ReminderDate.HasValue);
      }
  }
  ```

- [ ] Create `Allocore.Application/Features/Notes/Validators/UpdateNoteRequestValidator.cs`:
  - Same rules as `CreateNoteRequestValidator`.

---

## Step 6: Application Layer — CQRS Commands

### 6.1 CreateNote command

- [ ] Create `Allocore.Application/Features/Notes/Commands/CreateNoteCommand.cs`:
  ```csharp
  namespace Allocore.Application.Features.Notes.Commands;

  using MediatR;
  using Allocore.Application.Features.Notes.DTOs;
  using Allocore.Domain.Common;
  using Allocore.Domain.Entities.Notes;

  public record CreateNoteCommand(
      Guid CompanyId,
      NoteEntityType EntityType,
      Guid EntityId,
      CreateNoteRequest Request
  ) : IRequest<Result<NoteDto>>;
  ```

- [ ] Create `Allocore.Application/Features/Notes/Commands/CreateNoteCommandHandler.cs`:
  - Verify user has access to company
  - Verify the target entity exists and belongs to the company:
    - If `EntityType == Provider`: load provider, check `provider.CompanyId == command.CompanyId`
    - If `EntityType == Contract`: load contract, check `contract.CompanyId == command.CompanyId`
  - Parse `Category` enum (default to `General` if null)
  - Create `Note` with `AuthorUserId = _currentUser.UserId`
  - Save and return `NoteDto` (resolve author name from User)

### 6.2 UpdateNote command

- [ ] Create `Allocore.Application/Features/Notes/Commands/UpdateNoteCommand.cs`:
  ```csharp
  public record UpdateNoteCommand(Guid CompanyId, Guid NoteId, UpdateNoteRequest Request) : IRequest<Result<NoteDto>>;
  ```

- [ ] Create handler:
  - Verify user access to company
  - Load note, verify `note.CompanyId == command.CompanyId`
  - Verify current user is the author (`note.AuthorUserId == _currentUser.UserId`) OR user is Admin
  - Parse category, call `note.Update(...)`, save, return DTO

### 6.3 DeleteNote command

- [ ] Create `Allocore.Application/Features/Notes/Commands/DeleteNoteCommand.cs`:
  ```csharp
  public record DeleteNoteCommand(Guid CompanyId, Guid NoteId) : IRequest<Result>;
  ```

- [ ] Create handler:
  - Verify user access to company
  - Load note, verify company
  - Verify current user is author OR Admin
  - Delete note, save

### 6.4 TogglePinNote command

- [ ] Create `Allocore.Application/Features/Notes/Commands/TogglePinNoteCommand.cs`:
  ```csharp
  public record TogglePinNoteCommand(Guid CompanyId, Guid NoteId) : IRequest<Result>;
  ```

- [ ] Create handler:
  - Verify access, load note, verify company
  - Toggle: if pinned → unpin, if unpinned → pin
  - Save

---

## Step 7: Application Layer — CQRS Queries

### 7.1 GetNotesByEntity query

- [ ] Create `Allocore.Application/Features/Notes/Queries/GetNotesByEntityQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Notes.Queries;

  using MediatR;
  using Allocore.Application.Common;
  using Allocore.Application.Features.Notes.DTOs;
  using Allocore.Domain.Entities.Notes;

  public record GetNotesByEntityQuery(
      Guid CompanyId,
      NoteEntityType EntityType,
      Guid EntityId,
      int Page = 1,
      int PageSize = 20,
      string? Category = null
  ) : IRequest<PagedResult<NoteDto>>;
  ```

- [ ] Create handler:
  - Verify user access to company
  - Verify entity exists and belongs to company
  - Parse category filter if provided
  - Call `_noteRepository.GetPagedByEntityAsync(...)`
  - For each note, resolve `AuthorName` from User (batch load user IDs for efficiency)
  - Return `PagedResult<NoteDto>`

### 7.2 GetReminders query

- [ ] Create `Allocore.Application/Features/Notes/Queries/GetRemindersQuery.cs`:
  ```csharp
  public record GetRemindersQuery(
      Guid CompanyId,
      DateTime? FromDate = null,
      DateTime? ToDate = null
  ) : IRequest<IEnumerable<NoteDto>>;
  ```

- [ ] Create handler:
  - Verify user access to company
  - Default `FromDate` to today, `ToDate` to 30 days from now
  - Call `_noteRepository.GetRemindersForCompanyAsync(...)`
  - Resolve author names, return DTOs

---

## Step 8: API Layer — NotesController

- [ ] Create `Allocore.API/Controllers/v1/NotesController.cs`:
  ```csharp
  namespace Allocore.API.Controllers.v1;

  using Asp.Versioning;
  using MediatR;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Allocore.Application.Features.Notes.Commands;
  using Allocore.Application.Features.Notes.DTOs;
  using Allocore.Application.Features.Notes.Queries;
  using Allocore.Domain.Entities.Notes;

  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/companies/{companyId:guid}")]
  [Authorize]
  public class NotesController : ControllerBase
  {
      private readonly IMediator _mediator;

      public NotesController(IMediator mediator)
      {
          _mediator = mediator;
      }

      /// <summary>
      /// Get notes for a provider (timeline).
      /// </summary>
      [HttpGet("providers/{providerId:guid}/notes")]
      public async Task<IActionResult> GetProviderNotes(
          Guid companyId,
          Guid providerId,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 20,
          [FromQuery] string? category = null,
          CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(
              new GetNotesByEntityQuery(companyId, NoteEntityType.Provider, providerId, page, pageSize, category),
              cancellationToken);
          return Ok(result);
      }

      /// <summary>
      /// Add a note to a provider.
      /// </summary>
      [HttpPost("providers/{providerId:guid}/notes")]
      public async Task<IActionResult> AddProviderNote(
          Guid companyId,
          Guid providerId,
          [FromBody] CreateNoteRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(
              new CreateNoteCommand(companyId, NoteEntityType.Provider, providerId, request),
              cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Get notes for a contract (timeline).
      /// </summary>
      [HttpGet("contracts/{contractId:guid}/notes")]
      public async Task<IActionResult> GetContractNotes(
          Guid companyId,
          Guid contractId,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 20,
          [FromQuery] string? category = null,
          CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(
              new GetNotesByEntityQuery(companyId, NoteEntityType.Contract, contractId, page, pageSize, category),
              cancellationToken);
          return Ok(result);
      }

      /// <summary>
      /// Add a note to a contract.
      /// </summary>
      [HttpPost("contracts/{contractId:guid}/notes")]
      public async Task<IActionResult> AddContractNote(
          Guid companyId,
          Guid contractId,
          [FromBody] CreateNoteRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(
              new CreateNoteCommand(companyId, NoteEntityType.Contract, contractId, request),
              cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Update a note.
      /// </summary>
      [HttpPut("notes/{noteId:guid}")]
      public async Task<IActionResult> UpdateNote(
          Guid companyId,
          Guid noteId,
          [FromBody] UpdateNoteRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new UpdateNoteCommand(companyId, noteId, request), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return Ok(result.Value);
      }

      /// <summary>
      /// Delete a note.
      /// </summary>
      [HttpDelete("notes/{noteId:guid}")]
      public async Task<IActionResult> DeleteNote(
          Guid companyId,
          Guid noteId,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new DeleteNoteCommand(companyId, noteId), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }

      /// <summary>
      /// Toggle pin status of a note.
      /// </summary>
      [HttpPatch("notes/{noteId:guid}/pin")]
      public async Task<IActionResult> TogglePin(
          Guid companyId,
          Guid noteId,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new TogglePinNoteCommand(companyId, noteId), cancellationToken);
          if (!result.IsSuccess)
              return BadRequest(new { error = result.Error });
          return NoContent();
      }

      /// <summary>
      /// Get upcoming reminders for the company.
      /// </summary>
      [HttpGet("reminders")]
      public async Task<IActionResult> GetReminders(
          Guid companyId,
          [FromQuery] DateTime? fromDate = null,
          [FromQuery] DateTime? toDate = null,
          CancellationToken cancellationToken = default)
      {
          var result = await _mediator.Send(
              new GetRemindersQuery(companyId, fromDate, toDate),
              cancellationToken);
          return Ok(result);
      }
  }
  ```
  - **Note**: The controller uses a mixed routing strategy:
    - Entity-specific note endpoints: `providers/{id}/notes`, `contracts/{id}/notes`
    - Generic note operations: `notes/{noteId}` (update, delete, pin)
    - Company-level: `reminders`
  - **Note**: This pattern allows the frontend to fetch notes in context (provider detail page, contract detail page) while still having generic CRUD on notes themselves.

---

## Step 9: Build, Verify & Manual Test

- [ ] Run `dotnet build` — ensure entire solution compiles
- [ ] Apply migration: `dotnet ef database update -s Allocore.API -p Allocore.Infrastructure`
- [ ] Run application and verify Swagger shows all new endpoints
- [ ] Manual test via Swagger:
  1. Add a note to a provider → 200
  2. Add a note to a contract → 200
  3. Get provider notes (timeline) → 200, ordered by pinned then date
  4. Get contract notes → 200
  5. Update a note → 200
  6. Pin a note → 204
  7. Delete a note → 204
  8. Get reminders → 200
  9. Verify only author or admin can edit/delete
  10. Verify wrong company returns error
  11. Verify non-existent entity returns error

---

## Technical Details

### Dependencies

No new NuGet packages required.

### Project Structure — Affected Files

| Layer | File | Change |
|-------|------|--------|
| **Domain** | `Allocore.Domain/Entities/Notes/NoteEntityType.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Notes/NoteCategory.cs` | **Create** |
| **Domain** | `Allocore.Domain/Entities/Notes/Note.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Configurations/NoteConfiguration.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/ApplicationDbContext.cs` | **Update** — add DbSet |
| **Infrastructure** | `Allocore.Infrastructure/Persistence/Repositories/NoteRepository.cs` | **Create** |
| **Infrastructure** | `Allocore.Infrastructure/DependencyInjection.cs` | **Update** — add registration |
| **Application** | `Allocore.Application/Abstractions/Persistence/INoteRepository.cs` | **Create** |
| **Application** | `Allocore.Application/Features/Notes/DTOs/*.cs` | **Create** (3 files) |
| **Application** | `Allocore.Application/Features/Notes/Validators/*.cs` | **Create** (2 files) |
| **Application** | `Allocore.Application/Features/Notes/Commands/*.cs` | **Create** (8 files — 4 commands + 4 handlers) |
| **Application** | `Allocore.Application/Features/Notes/Queries/*.cs` | **Create** (4 files — 2 queries + 2 handlers) |
| **API** | `Allocore.API/Controllers/v1/NotesController.cs` | **Create** |

### Database

**Table: Notes**

| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| Id | uuid | NO | gen | PK |
| CompanyId | uuid | NO | — | |
| EntityType | varchar(50) | NO | — | |
| EntityId | uuid | NO | — | |
| AuthorUserId | uuid | NO | — | |
| Content | varchar(10000) | NO | — | |
| Category | varchar(50) | NO | General | |
| IsPinned | boolean | NO | false | |
| ReminderDate | timestamp | YES | — | |
| CreatedAt | timestamp | NO | — | |
| UpdatedAt | timestamp | YES | — | |

**Indexes:**
- `IX_Notes_EntityType_EntityId` (composite — primary query path)
- `IX_Notes_CompanyId`
- `IX_Notes_AuthorUserId`
- `IX_Notes_Category`
- `IX_Notes_ReminderDate` (filtered where not null)

**No foreign keys on EntityId or AuthorUserId** — polymorphic association pattern.

### API Contract

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/v1/companies/{companyId}/providers/{providerId}/notes` | GET | Yes | Get provider notes (paginated timeline) |
| `/api/v1/companies/{companyId}/providers/{providerId}/notes` | POST | Yes | Add note to provider |
| `/api/v1/companies/{companyId}/contracts/{contractId}/notes` | GET | Yes | Get contract notes (paginated timeline) |
| `/api/v1/companies/{companyId}/contracts/{contractId}/notes` | POST | Yes | Add note to contract |
| `/api/v1/companies/{companyId}/notes/{noteId}` | PUT | Yes | Update note (author or admin) |
| `/api/v1/companies/{companyId}/notes/{noteId}` | DELETE | Yes | Delete note (author or admin) |
| `/api/v1/companies/{companyId}/notes/{noteId}/pin` | PATCH | Yes | Toggle pin status |
| `/api/v1/companies/{companyId}/reminders` | GET | Yes | Get upcoming reminders |

### Authentication/Authorization

- All endpoints require JWT Bearer authentication
- All endpoints verify user has access to the specified company
- Note update/delete restricted to the note's author or Admin users
- Fine-grained role checks deferred (same as US004/US005)

---

## Acceptance Criteria

- [ ] Users can add notes to providers and contracts
- [ ] Notes form a chronological timeline (newest first, pinned at top)
- [ ] Notes support categories (General, Negotiation, Meeting, Decision, etc.)
- [ ] Notes can be pinned to stay at the top of the timeline
- [ ] Notes support optional reminder dates
- [ ] Only the note author or an Admin can edit/delete a note
- [ ] Notes are company-scoped — no cross-tenant data leakage
- [ ] Notes validate that the target entity exists and belongs to the company
- [ ] Reminders endpoint returns notes with upcoming reminder dates
- [ ] Migrations created and applied (`AddNotes`)
- [ ] `dotnet build` passes without errors
- [ ] Swagger displays all new endpoints

---

## What is explicitly NOT changing?

- **Provider entity** — no changes
- **Contract entity** — no changes
- **Authentication/Authorization model** — no new roles or policies
- **Company/User entities** — no changes
- **No automated notifications** — reminders are queryable but no push/email

---

## Follow-ups (Intentionally Deferred)

| Item | Reason | Related Story |
|------|--------|---------------|
| Automated reminder notifications (email/push) | Requires notification infrastructure | Future US |
| Rich text / markdown rendering support | Frontend concern, backend stores raw text | Frontend story |
| File attachments on notes | Requires file storage infrastructure | Future US |
| Activity log (auto-generated notes for entity changes) | Requires audit/event infrastructure | Future US |
| Notes on Employee, CostCenter, Project entities | Add enum values when those entities exist | Future US (per entity) |
| Note search (full-text across all notes in a company) | Requires full-text search setup | Future US |
| Batch author name resolution optimization | Performance optimization if needed | Tech debt |
