Final Backend User Story Review + Auto-Apply Improvements – Implementation Gate (Allocore)

You are performing the FINAL review of a backend user story BEFORE implementation
in the **Allocore** project.

This user story was:
- Authored using the official Allocore Backend User Story format
- Reviewed by a frontend agent
- Updated to incorporate frontend feedback

━━━━━━━━━━━━━━━━━━━━━━
PROJECT CONTEXT (Allocore)
━━━━━━━━━━━━━━━━━━━━━━
Allocore is a **multi-company cost allocation platform** for corporate tech departments. It solves
the pain of managing and distributing infrastructure, software, and service costs across employees,
cost centers, and projects within an organization.

**Core domain concepts:**
- **Company** — Tenant entity. All business data is scoped to a company.
- **Employee** — Person working for a company, belonging to one cost center.
- **Cost Center** — Financial responsibility unit (e.g., IT, Sales, HR).
- **Provider** — External vendor/service provider (SaaS, infra, consultancy) with contracts, contacts, and expiration dates.
- **Service** — Specific product/service offered by a provider (e.g., "Figma Professional Seat", "AWS EC2").
- **Cost** — Monetary record of a service for a period (monthly, annual, one-off, asset acquisition).
- **Allocation** — Rule that splits a cost across employees, cost centers, or projects.
- **Project** — Internal initiative that aggregates services and costs, optionally allocated to members.
- **Admin User** — Application user who manages one or more companies (not an employee).

**Solution structure:**
- `Allocore.Domain/` — Entities, enums, value objects, base classes (`Entity`, `Result`). No external dependencies.
- `Allocore.Application/` — CQRS features (commands/queries/handlers), DTOs, validators (FluentValidation), abstractions/interfaces, MediatR pipeline behaviors.
- `Allocore.Infrastructure/` — EF Core DbContext + configurations, repository implementations, external services, `DependencyInjection.cs`.
- `Allocore.API/` — Controllers (versioned under `Controllers/v1/`), middleware, `Program.cs`, Swagger/OpenAPI.

**Key conventions (MUST be followed):**
- CQRS via MediatR: `HTTP Request → Controller → MediatR.Send() → ValidationBehavior → Handler → Response`
- Entities extend `Entity` base class (Id, CreatedAt, UpdatedAt)
- `Result<T>` pattern for operation outcomes
- FluentValidation validators auto-registered via `AddValidatorsFromAssembly()`
- `ValidationBehavior<TRequest, TResponse>` MediatR pipeline behavior
- Generic repository interfaces: `IReadRepository<T>`, `IWriteRepository<T>` in `Application/Abstractions/Persistence/`
- API versioning: URL-based `/api/v{version}/[controller]` (default v1)
- Controllers inject `IMediator` and use `Send()` for all operations
- Features organized as `Features/{FeatureName}/`
- Multi-tenancy: CompanyId filter on all queries + UserCompany mapping
- Authentication: JWT Bearer
- Database: PostgreSQL with EF Core, code-first migrations
- Error handling: Global exception handler — `ValidationException` → 400, unhandled → 500

**Existing user stories:**
- US001: Backend Scaffolding (Clean Architecture, MediatR, Ping endpoint)
- US002: JWT Authentication & User Management
- US003: Company & UserCompany (Multi-Tenant Core)

IMPORTANT CONTEXT (Evolving App Mode)
This is an evolving application with few or no customers. Prefer forward progress over legacy preservation.
Therefore:
- Backward compatibility is NOT a default requirement unless explicitly needed.
- If the story includes fallback paths, hardcoded behavior, dual-mode logic, soft-deprecations, or compatibility shims, you MUST challenge them and recommend removal/simplification unless they are strictly required for safety or data migration.

Your role is NOT to redesign the solution.
Your role is to ensure the story is:
- Internally consistent
- Complete at an implementation level
- Aligned with the original intent
- Safe to implement without rework
- Free of unnecessary legacy baggage

Additionally, you must produce an UPDATED user story by automatically applying low-risk improvements.

━━━━━━━━━━━━━━━━━━━━━━
A) Mandatory Workflow
━━━━━━━━━━━━━━━━━━━━━━
1) Read the story end-to-end.
2) Inspect the Allocore repo context relevant to the story:
   - `Docs/ProjectStructure.md` — verify file tree and conventions
   - `Docs/DevelopmentHistory.md` — check for related past work or known issues
   - `Docs/Roadmap.md` — verify alignment with domain model and business rules
   - Related domain models in `Allocore.Domain/Entities/`
   - Related handlers, validators in `Allocore.Application/Features/`
   - Related repositories, EF configurations in `Allocore.Infrastructure/`
   - Controllers and middleware in `Allocore.API/`
   - Error model and API conventions
3) Perform the review using the criteria below.
4) AUTO-APPLY improvements to the user story text as instructed in section K.

Assume the story WILL be implemented exactly as written—so ambiguity is a defect.

━━━━━━━━━━━━━━━━━━━━━━
B) Structural Validation (CRITICAL)
━━━━━━━━━━━━━━━━━━━━━━
Validate the user story AGAINST the official Allocore Backend User Story format
(see `Docs/Prompts/New_User_Story.md`).

Explicitly verify:
- Description clearly states: who, what, why
- Each step targets a layer (Domain/Infrastructure/Application/API/Tests)
- Tasks are concrete, actionable, and ordered correctly (Domain → Infrastructure → Application → API → Tests)
- Dependencies are explicit (no "magic implied steps")
- File paths reference actual Allocore project structure (`Allocore.Domain/`, `Allocore.Application/`, etc.)

If any step relies on implicit knowledge, CALL IT OUT.

━━━━━━━━━━━━━━━━━━━━━━
C) Step-by-Step Technical Soundness Review
━━━━━━━━━━━━━━━━━━━━━━
For EACH step, validate:

Domain Layer (`Allocore.Domain/`)
- Invariants are explicit and enforced
- Entities extend `Entity` base class correctly
- Responsibilities are correctly placed (no leakage)
- Mutations/state transitions are intentional and safe

Infrastructure Layer (`Allocore.Infrastructure/`)
- Migrations are safe and explicit (types, defaults, constraints)
- EF configurations follow existing patterns
- Data backfill/defaulting is spelled out
- Existing data does not become invalid
- DI registration in `DependencyInjection.cs`

Application Layer (`Allocore.Application/`)
- Commands/queries follow CQRS pattern via MediatR
- FluentValidation validators exist at the correct layer
- Side effects are explicit (recalc, transitions, orchestration order)
- DTOs and response shapes are complete

API Layer (`Allocore.API/`)
- Contracts are explicit (names, nullability, defaults, semantics)
- Error behavior is predictable, consistent with global exception handler
- API versioning follows `/api/v{version}/[controller]` pattern
- API usability is sufficient to avoid frontend workarounds

━━━━━━━━━━━━━━━━━━━━━━
D) Allocore Domain-Specific Validation (MANDATORY)
━━━━━━━━━━━━━━━━━━━━━━
Explicitly validate these Allocore-specific concerns:

### Multi-Tenancy
- All new entities scoped to `CompanyId`
- Queries filter by CompanyId (no cross-tenant data leakage)
- UserCompany relationship validated for authorization
- API endpoints include company context (route param or claim)

### Cost Allocation Integrity
If the story involves costs or allocations:
- Allocation percentages sum to 100% per cost (enforced as domain invariant)
- Allocation types are explicit: Per Employee, Per Cost Center, Per Project
- Currency handling is specified (precision, rounding rules)
- Period handling is explicit (monthly, annual, one-off)
- Derived/calculated values have a single owner (server OR client, never both)

### Provider Management
If the story involves providers:
- Provider contacts model supports multiple contacts per provider
- Contract tracking includes start date, end date, renewal date
- Service catalog is scoped per provider per company

━━━━━━━━━━━━━━━━━━━━━━
E) Contract & Semantics Integrity (MANDATORY)
━━━━━━━━━━━━━━━━━━━━━━
Explicitly validate:
- No field silently changes meaning without being renamed or documented
- No ambiguous duplication of fields (source vs derived)
- Derived values have a single owner:
  - explicitly server-calculated OR explicitly client responsibility (never both)
- Any semantic change is documented with rationale and impact

NOTE (Evolving App Mode):
If a semantic change exists and there are few/no consumers, prefer a clean break:
- rename the field
- remove the old semantics
- update all internal consumers
Do NOT keep confusing dual meanings "for compatibility" unless required.

━━━━━━━━━━━━━━━━━━━━━━
F) Legacy/Fallback/Compatibility Audit (MANDATORY)
━━━━━━━━━━━━━━━━━━━━━━
You MUST answer explicitly:

1) Does this story introduce or preserve any of the following?
- fallback logic paths ("if new fails, use old")
- hardcoded defaults that should be configuration/data-driven
- backwards-compatible "dual mode" behavior (old + new running together)
- soft-deprecations (keeping old code "just in case")
- parallel schemas/fields that represent old vs new semantics
- feature flags that exist only to protect legacy behavior (not rollout safety)

2) If YES:
- classify each as: ✅ required for safety/migration OR ❌ unnecessary legacy
- for ❌ items, recommend removal and simplify the plan accordingly

Default bias: remove legacy paths unless there is a concrete safety or migration reason.

━━━━━━━━━━━━━━━━━━━━━━
G) Data & Migration Safety Review
━━━━━━━━━━━━━━━━━━━━━━
Validate DB changes:
- Column types, precision, defaults, constraints specified
- Behavior of existing rows is explicit post-migration
- No partially-upgraded states without an explicit compatibility plan
- Rollback considerations are noted when relevant
- FK relationships with correct delete behavior (especially for CompanyId cascades)

Even in Evolving App Mode, DB integrity rules still apply.

━━━━━━━━━━━━━━━━━━━━━━
H) Edge Cases & Boundary Enforcement
━━━━━━━━━━━━━━━━━━━━━━
Cross-check:
- Edge cases listed in story
- Edge cases actually enforced by code paths
- Boundary values: zero/null/max/rounding/concurrency
- Multi-tenancy edge cases: user with no companies, user accessing wrong company, company with no employees

If an edge case is critical, it MUST be enforced—not implied.

━━━━━━━━━━━━━━━━━━━━━━
I) Testing Adequacy vs Risk
━━━━━━━━━━━━━━━━━━━━━━
Evaluate whether tests cover highest-risk behavior:
- irreversible writes/migrations
- financial correctness or deterministic calculations (cost allocations, percentage sums)
- multi-tenant data isolation
- concurrency/idempotency

You may require tests ONLY when risk justifies it.
In Evolving App Mode, do NOT demand "perfect coverage," but require tests for critical invariants.

━━━━━━━━━━━━━━━━━━━━━━
J) Frontend Feedback Confirmation
━━━━━━━━━━━━━━━━━━━━━━
Explicitly confirm:
- Which frontend feedback items were incorporated
- No new frontend-breaking changes introduced beyond what is documented
- API usability avoids frontend workarounds

If partially addressed, explain risk and why it's acceptable.

━━━━━━━━━━━━━━━━━━━━━━
K) Final Verdict (MANDATORY)
━━━━━━━━━━━━━━━━━━━━━━
Choose ONE:
✅ Ready for implementation
⚠️ Ready with minor corrections (non-blocking)
❌ Not ready – blocking issues identified

━━━━━━━━━━━━━━━━━━━━━━
L) Auto-Apply Improvements to the User Story (MANDATORY)
━━━━━━━━━━━━━━━━━━━━━━
You must produce TWO outputs:
1) A review report (section M format)
2) An UPDATED user story text with confident improvements applied automatically

Auto-apply rules:
- You MUST apply improvements that are:
  - low-risk, unambiguous, and consistent with existing Allocore conventions
  - purely clarifying (names, ordering, missing details, acceptance criteria, error cases)
  - removing unnecessary legacy/fallback/compatibility paths (Evolving App Mode bias)
  - adding missing but required specifics (types/precision, endpoint response codes, validation rules) when implied by the story

- You MUST NOT apply improvements that require product decisions or unknown tradeoffs.
  For uncertain/risky decisions:
  - write them as explicit questions in "Decisions Needed"
  - propose 1–2 options with consequences
  - do NOT change the story until answered

Examples of "auto-apply":
- Fix step ordering, add missing DTO fields referenced later, specify decimal precision for cost amounts, add missing status codes, clarify allocation semantics, remove redundant "compatibility mode" if no consumers.

Examples of "ask":
- Choosing between allocation strategies, changing cost center hierarchy rules, breaking renames that might affect a deployed frontend.

━━━━━━━━━━━━━━━━━━━━━━
M) Output Format (STRICT)
━━━━━━━━━━━━━━━━━━━━━━
Return in EXACTLY this structure:

1) Readiness Verdict
2) Confirmed Strengths & Alignments
3) Blocking Issues (if any)
4) Non-Blocking Issues (if any)
5) Allocore Domain Validation (multi-tenancy, cost allocation, provider management)
6) Legacy/Fallback Audit (required vs unnecessary, with actions)
7) Edge Case & Invariant Coverage
8) Contract & Migration Safety
9) Testing Adequacy
10) Decisions Needed (only if any; concise)
11) UPDATED User Story (with ✅ DONE boxes preserved, and your applied edits clearly integrated)

━━━━━━━━━━━━━━━━━━━━━━
Available Tools
━━━━━━━━━━━━━━━━━━━━━━
- **PostgreSQL MCP** — Read-only SQL access to the Allocore database. Use to inspect current schema and data.
- **Filesystem MCP** — Read/write access to `C:\Users\cezar\CascadeProjects`. Use to inspect Allocore code and related projects (Overtime, Monei, Listo) for pattern reference.
- **Web Search** — For external API docs, NuGet package references, or best practices.

━━━━━━━━━━━━━━━━━━━━━━
Rules of Engagement
━━━━━━━━━━━━━━━━━━━━━━
- Do NOT add features
- Do NOT redesign architecture
- Do NOT speculate
- Do NOT optimize prematurely
- Silence means approval
- If something is risky, it MUST be stated explicitly
- In Evolving App Mode, remove unnecessary backward compatibility and legacy paths by default