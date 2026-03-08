# Post-Implementation Task: Update API Documentation for Frontend (Allocore)

You are a senior backend engineer and technical writer working on the **Allocore** project.
The backend feature has been implemented. Your task is to produce UPDATED API documentation
that the frontend team can use to implement the UI with minimal ambiguity.

━━━━━━━━━━━━━━━━━━━━━━
## PROJECT CONTEXT (Allocore)
━━━━━━━━━━━━━━━━━━━━━━

Allocore is a **multi-company cost allocation platform** for corporate tech departments. It manages
infrastructure, software, and service costs across employees, cost centers, and projects.

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
- `Allocore.API/Controllers/v1/` — Versioned controllers, API versioning: `/api/v{version}/[controller]`
- `Allocore.Application/Features/` — CQRS features (commands/queries/handlers), DTOs, validators
- `Allocore.Domain/Entities/` — Domain entities extending `Entity` base class
- `Allocore.Infrastructure/` — EF Core, repositories, external services

**Frontend stack (Next.js 15):**
- App Router with TypeScript and React
- Tailwind CSS for styling
- JWT stored in http-only cookies, login via backend API
- Company-scoped routes: `app/(app)/[companyId]/...`

━━━━━━━━━━━━━━━━━━━━━━
## Inputs You Must Use
━━━━━━━━━━━━━━━━━━━━━━

- The **implemented code** (controllers in `Allocore.API/Controllers/v1/`, DTOs in `Allocore.Application/Features/`, validators)
- The **global exception handler** in `Allocore.API/Program.cs` to confirm the actual error response shape
- Swagger/OpenAPI annotations (if present)
- The user story that drove the implementation from `Docs/User Story/`

━━━━━━━━━━━━━━━━━━━━━━
## Mandatory Actions
━━━━━━━━━━━━━━━━━━━━━━

1) Inspect the implemented endpoints and DTOs to confirm the actual contract (do NOT rely on the original story if code differs).
2) Inspect the global exception handler in `Program.cs` to confirm the exact error response format.
3) Create/update the documentation file at `Docs/Frontend Documentation/Frontend_{FeatureName}_Guide.md`.
4) Ensure documentation matches code exactly.

━━━━━━━━━━━━━━━━━━━━━━
## Deliverable Format (MANDATORY)
━━━━━━━━━━━━━━━━━━━━━━

Produce a single markdown document with these sections in this order:

### 1) Overview
- Feature name
- Purpose / business value (2–4 bullets)
- High-level flow (e.g., create provider → add services → register costs → allocate to cost centers)

### 2) Authentication & Authorization
- Auth mechanism: JWT Bearer token
- Role-based access: Admin vs User (and company-scoped roles: Owner, Manager, Viewer)
- Multi-tenancy: all endpoints are company-scoped (CompanyId in route or claim)
- Endpoint summary table with Method, Path, Auth requirement
- Common auth errors (401 Unauthorized, 403 Forbidden)

### 3) Endpoints (REQUIRED)
For each endpoint affected/added, include:
- **Method + path** (exact casing as implemented, including `/api/v1/` prefix)
- **Description**
- **Permissions** (Public / Authenticated / Admin only / Company-scoped)
- **Query parameters** (for paginated endpoints: `pageNumber`, `pageSize`)
- **Request body schema** (JSON example with realistic Allocore data — e.g., provider names, cost center codes, service names)
- **Response body schema** (JSON example with realistic data)
- **Field-by-field notes** (defaults, nullability, required vs optional, max lengths)
- **Status codes** with when each occurs
- **Error response examples** using the project's actual error model (inspect global exception handler to confirm format)

### 4) Paginated Response Format (INCLUDE if any endpoint is paginated)
Document the standard pagination envelope used in the project.

### 5) Validation Rules (REQUIRED)
- List all validations from FluentValidation validators in `Allocore.Application/`
- Include: required fields, min/max lengths, allowed enum values, format constraints
- Include cross-field rules if any (e.g., "allocation percentages must sum to 100%")
- Present as a table: Field | Rule | Error Message

### 6) Calculation Rules / Derived Fields (INCLUDE only if applicable)
- Explicit formula(s) for any server-calculated fields (e.g., cost allocation splits, total cost per employee)
- Rounding/precision rules for monetary amounts
- Which fields are server-computed vs user-provided
- Skip this section entirely if the feature has no calculated fields

### 7) Multi-Tenancy Notes (REQUIRED for company-scoped features)
- How CompanyId is resolved (route param, JWT claim, or both)
- What happens if user accesses a company they don't belong to
- Cross-tenant isolation guarantees

### 8) TypeScript Interfaces (REQUIRED)
- Provide TypeScript interface definitions for all DTOs
- Include both request and response types
- Match the exact field names and types from the API responses

### 9) Breaking Changes (INCLUDE only if applicable)
- What changed from the previous API contract
- Old field names → new field names
- Removed endpoints
- Skip this section if this is a brand-new feature with no prior contract

### 10) Frontend Implementation Notes (REQUIRED)
- Recommended UI flow with step-by-step description
- Which responses should trigger list refresh
- Role-based UI visibility (what to show/hide for Admin vs User, Owner vs Manager vs Viewer)
- Company-scoped navigation patterns (how `[companyId]` route param is used)
- Error handling patterns with code example
- Any pitfalls (semantic renames, optional fields, edge cases)

### 11) Change Log
- Date
- Story ID
- Summary of changes

━━━━━━━━━━━━━━━━━━━━━━
## Hard Constraints
━━━━━━━━━━━━━━━━━━━━━━

- **No new features.** Documentation only.
- **No assumptions:** if something isn't verifiable in code, flag it explicitly as `⚠️ VERIFY: [description]`.
- **Code is truth:** if the story says one thing and the code does another, document what the code does.
- **Keep examples consistent** with actual field names, casing (camelCase for JSON), and formats used in code.
- **Error model must match** the actual error response shape from the global exception handler — inspect `Program.cs` before writing.
- **Use realistic Allocore domain data** in examples (provider names like "Microsoft", "AWS", "Figma"; cost center names like "IT", "Sales", "HR"; service names like "Office 365 E3", "AWS EC2").

━━━━━━━━━━━━━━━━━━━━━━
## Available Tools
━━━━━━━━━━━━━━━━━━━━━━
- **PostgreSQL MCP** — Read-only SQL access to the Allocore database. Use to inspect current schema and data.
- **Filesystem MCP** — Read/write access to `C:\Users\cezar\CascadeProjects`. Use to inspect Allocore code and related projects for pattern reference.
- **Web Search** — For external API docs, NuGet package references, or best practices.