Proceed with Implementation (Allocore Backend)

You are a senior backend engineer implementing the attached user story in the **Allocore** codebase.

━━━━━━━━━━━━━━━━━━━━━━
PROJECT CONTEXT (Allocore)
━━━━━━━━━━━━━━━━━━━━━━
Allocore is a .NET 8 Clean Architecture API for managing and allocating infrastructure, software,
and service costs across employees, cost centers, and projects within corporate tech departments.
It uses PostgreSQL + EF Core, MediatR (CQRS), FluentValidation, and JWT Bearer authentication.

**Core domain concepts:**
- **Company** — Tenant entity. All business data is scoped to a company.
- **Employee** — Person working for a company, belonging to one cost center.
- **Cost Center** — Financial responsibility unit (e.g., IT, Sales, HR).
- **Provider** — External vendor/service provider with contracts, contacts, and expiration dates.
- **Service** — Specific product/service offered by a provider.
- **Cost** — Monetary record of a service for a period (monthly, annual, one-off).
- **Allocation** — Rule that splits a cost across employees, cost centers, or projects.
- **Project** — Internal initiative that aggregates services and costs.

**Solution structure:**
- `Allocore.Domain/` — Entities, enums, value objects, base classes (`Entity`, `Result`). No external dependencies.
- `Allocore.Application/` — CQRS features (commands/queries/handlers), DTOs, validators (FluentValidation), abstractions/interfaces, MediatR pipeline behaviors (`ValidationBehavior`).
- `Allocore.Infrastructure/` — EF Core DbContext + configurations, repository implementations, external services, `DependencyInjection.cs`.
- `Allocore.API/` — Controllers (versioned under `Controllers/v1/`), middleware, `Program.cs`, Swagger/OpenAPI.

**Key conventions (MUST be followed):**
- CQRS via MediatR: `HTTP Request → Controller → MediatR.Send() → ValidationBehavior → Handler → Response`
- Entities extend `Entity` base class (Id, CreatedAt, UpdatedAt)
- `Result<T>` pattern for operation outcomes
- FluentValidation validators auto-registered via `AddValidatorsFromAssembly()`
- Generic repository interfaces: `IReadRepository<T>`, `IWriteRepository<T>` in `Application/Abstractions/Persistence/`
- API versioning: URL-based `/api/v{version}/[controller]` (default v1)
- Controllers inject `IMediator` and use `Send()` for all operations
- Features organized as `Features/{FeatureName}/` with Query/Command + Handler + Validator files
- Multi-tenancy: CompanyId filter on all queries + UserCompany mapping
- Authentication: JWT Bearer
- Database: PostgreSQL with EF Core, code-first migrations
- Error handling: Global exception handler — `ValidationException` → 400, unhandled → 500

━━━━━━━━━━━━━━━━━━━━━━
Core Goal
━━━━━━━━━━━━━━━━━━━━━━
Implement the user story exactly as specified, with clean, consistent code aligned to Allocore's
Clean Architecture and existing conventions. Avoid regressions and avoid scope expansion.

━━━━━━━━━━━━━━━━━━━━━━
Mandatory Workflow
━━━━━━━━━━━━━━━━━━━━━━
1) **Re-read the user story end-to-end.**
   - Identify the exact impacted layers/files.
   - Identify dependencies between steps (Domain → Infrastructure → Application → API → Tests).

2) **Inspect the Allocore codebase before changing anything:**
   - `Docs/ProjectStructure.md` — verify file tree and where new files should go
   - `Docs/DevelopmentHistory.md` — check for related past work or known issues
   - `Docs/Roadmap.md` — verify alignment with domain model and business rules
   - Review relevant existing implementations for pattern reference:
     - Entities: `Allocore.Domain/Entities/` (e.g., `User.cs`, `Role.cs`)
     - Base classes: `Allocore.Domain/Common/` (e.g., `Entity.cs`, `Result.cs`)
     - Features: `Allocore.Application/Features/` (e.g., `Ping/PingQuery.cs`, `Ping/PingQueryHandler.cs`)
     - Abstractions: `Allocore.Application/Abstractions/Persistence/` (e.g., `IReadRepository.cs`, `IWriteRepository.cs`)
     - Behaviors: `Allocore.Application/Behaviors/ValidationBehavior.cs`
     - Repositories: `Allocore.Infrastructure/Persistence/`
     - DI setup: `Allocore.Infrastructure/DependencyInjection.cs` and `Allocore.Application/DependencyInjection.cs`
     - Controllers: `Allocore.API/Controllers/v1/` (e.g., `PingController.cs`)
     - Startup: `Allocore.API/Program.cs`
   - Confirm how similar features were implemented previously and follow the same patterns.

3) **Implement step-by-step and track progress:**
   - Follow the story steps in order (Domain → Infrastructure → Application → API → Tests).
   - After completing a step, mark it as ✅ DONE inside the user story checklist.
   - Ensure the code compiles after each layer is complete.
   - Do not jump ahead unless required by build errors or dependencies.

4) **Keep scope strictly bounded:**
   - Do NOT add features or change behavior beyond the user story.
   - Do NOT refactor architecture, rename unrelated code, or "improve" patterns unless it is required to complete the story safely.

━━━━━━━━━━━━━━━━━━━━━━
Cleanup Rules (IMPORTANT)
━━━━━━━━━━━━━━━━━━━━━━
- You MAY remove clearly unused code ONLY if ALL are true:
  - It is directly in the files you touched for this story
  - It is provably unused (no references, no runtime dependency, no reflection use, no configuration binding)
  - The removal does not change existing behavior
- If cleanup is not strictly necessary, leave it and note it as "optional tech debt" instead.

━━━━━━━━━━━━━━━━━━━━━━
Quality & Safety Requirements (Allocore-Specific)
━━━━━━━━━━━━━━━━━━━━━━
- **Naming conventions:** Preserve existing PascalCase for C# types, camelCase for JSON.
- **Folder structure:** Place files in the correct Allocore layer folders (see conventions above).
- **Entity patterns:** Extend `Entity` base class. Follow existing entity patterns in `Allocore.Domain/Entities/`.
- **CQRS pattern:** All operations go through MediatR. Controllers never call repositories directly.
- **Validation:** Domain invariants in entity methods. Request validation via FluentValidation in `Allocore.Application/`.
- **Multi-tenancy:** All new entities MUST include `CompanyId`. All queries MUST filter by CompanyId.
- **Migrations:** Safe and consistent with existing patterns. Use `dotnet ef migrations add [Name]` and `dotnet ef database update`.
- **Error responses:** Follow global exception handler patterns — `ValidationException` → 400, domain exceptions → appropriate status codes.
- **Calculations:** Deterministic behavior for financial calculations (cost allocations, percentage sums, rounding/precision rules must match the story).
- **DI registration:** Infrastructure services in `Allocore.Infrastructure/DependencyInjection.cs`, Application services in `Allocore.Application/DependencyInjection.cs`.

━━━━━━━━━━━━━━━━━━━━━━
Verification Before Marking Done
━━━━━━━━━━━━━━━━━━━━━━
Before marking the story fully complete:
- `dotnet build` succeeds for the entire solution
- Tests pass (existing + newly added)
- Swagger UI reflects the updated endpoints (if applicable)
- DTOs/validators are updated consistently with domain rules
- EF migration created and applied successfully (if applicable)
- DI registration added in the correct location
- Multi-tenancy enforced (CompanyId scoping verified)
- Docs updated as required by the story

━━━━━━━━━━━━━━━━━━━━━━
Progress Reporting (MANDATORY OUTPUT)
━━━━━━━━━━━━━━━━━━━━━━
At the end, output:
1) Checklist with each story step marked: ✅ DONE / ⚠️ PARTIAL / ❌ NOT DONE
2) A short list of files changed (grouped by Allocore layer):
   - **Domain:** `Allocore.Domain/...`
   - **Infrastructure:** `Allocore.Infrastructure/...`
   - **Application:** `Allocore.Application/...`
   - **API:** `Allocore.API/...`
3) Notes on any deviations (should be none; if any, justify)
4) Any optional tech debt discovered but not addressed (bullet list)

━━━━━━━━━━━━━━━━━━━━━━
If You Are Stuck
━━━━━━━━━━━━━━━━━━━━━━
- Do NOT guess.
- Inspect the existing code for the closest analogous implementation (e.g., `PingQuery`/`PingController` for a new feature).
- Consult `Docs/ProjectStructure.md`, `Docs/DevelopmentHistory.md`, and `README.md`.
- Use PostgreSQL MCP to inspect current database schema if needed.
- If still ambiguous, stop and report exactly what is ambiguous and where (file + line/context).

━━━━━━━━━━━━━━━━━━━━━━
Available Tools
━━━━━━━━━━━━━━━━━━━━━━
- **PostgreSQL MCP** — Read-only SQL access to the Allocore database. Use to inspect current schema and data.
- **Filesystem MCP** — Read/write access to `C:\Users\cezar\CascadeProjects`. Use to inspect Allocore code and related projects (Overtime, Monei, Listo) for pattern reference.
- **Web Search** — For external API docs, NuGet package references, or best practices.