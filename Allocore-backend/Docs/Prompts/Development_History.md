# End of Development Cycle – Documentation & Release Closure (Allocore)

You are closing a development cycle (feature, refactor, bug fix, or infrastructure change)
in the **Allocore** project.

Your task is to produce COMPLETE, concise, and accurate documentation updates that reflect
what was actually delivered, why it matters, and who is impacted.

This is a closure step, not an analysis or ideation step.
Do not invent features or speculate beyond implemented code.

━━━━━━━━━━━━━━━━━━━━━━
PROJECT CONTEXT (Allocore)
━━━━━━━━━━━━━━━━━━━━━━
Allocore is a .NET 8 Clean Architecture API for managing and allocating infrastructure, software,
and service costs across employees, cost centers, and projects within corporate tech departments.
It uses PostgreSQL + EF Core, MediatR (CQRS), FluentValidation, and JWT Bearer authentication.

**Solution structure:**
- `Allocore.Domain/` — Entities, enums, value objects, base classes (`Entity`, `Result`).
- `Allocore.Application/` — CQRS features (commands/queries/handlers), DTOs, validators (FluentValidation), abstractions/interfaces, MediatR pipeline behaviors.
- `Allocore.Infrastructure/` — EF Core DbContext + configurations, repository implementations, external services, `DependencyInjection.cs`.
- `Allocore.API/` — Controllers (versioned under `Controllers/v1/`), middleware, `Program.cs`, Swagger/OpenAPI.

**Documentation structure:**
- `Docs/DevelopmentHistory.md` — Chronological development log (append-only)
- `Docs/ProjectStructure.md` — Full file tree and architecture overview
- `Docs/Roadmap.md` — Product roadmap with domain model and business rules
- `Docs/ReleaseNotes/` — Versioned release notes
- `Docs/User Story/` — Completed and future user stories

**Existing user stories:**
- US001: Backend Scaffolding (Clean Architecture, MediatR, Ping endpoint)
- US002: JWT Authentication & User Management
- US003: Company & UserCompany (Multi-Tenant Core)

━━━━━━━━━━━━━━━━━━━━━━
## 1) Source of Truth (MANDATORY)
━━━━━━━━━━━━━━━━━━━━━━

Base all updates ONLY on:
- The **implemented code** in the Allocore solution (merged or ready-to-merge)
- The **final approved user story** from `Docs/User Story/`
- Commit history / diff if needed
- **Existing docs** structure and conventions in `Docs/`

If something is unclear in code, state it explicitly instead of guessing.

━━━━━━━━━━━━━━━━━━━━━━
## 2) DevelopmentHistory.md (MANDATORY)
━━━━━━━━━━━━━━━━━━━━━━

**File:** `Docs/DevelopmentHistory.md`

Append a NEW entry at the **top** of the file (newest first — match existing order).

Rules:
- Do NOT delete or edit previous entries
- Add ONE entry per development cycle
- Style: factual, concise, technical narrative (no marketing language)
- Follow the existing entry format in the file

### Entry Format

Use this heading format (match existing entries exactly):
```
## YYYY-MM-DD: USXXX – [Title]
```

### Content Structure by Layer

Organize the entry following Allocore's Clean Architecture layers. Use bold prefixes for each layer section when the change spans multiple layers:

- **Domain Layer:** New/updated entities, enums, value objects, business rules in `Allocore.Domain/`
- **Infrastructure Layer:** Repository implementations, EF Core configurations, migrations in `Allocore.Infrastructure/`
- **Application Layer:** Features (commands/queries/handlers), DTOs, validators, MediatR behaviors in `Allocore.Application/`
- **API Layer:** Controllers, endpoints, authorization, error handling in `Allocore.API/`
- **Configuration:** DI registration, appsettings changes, environment variables
- **Database:** Migration name and what it does (tables, columns, constraints, indexes)
- **Breaking Changes:** API contract changes, renamed fields/endpoints, removed endpoints (bold with `**Breaking:**` prefix)
- **Documentation:** Release notes, frontend guides, or other docs created

### What to Include
- What was implemented or changed (high-level, not step-by-step)
- Key architectural or domain decisions and their rationale
- New entities/services/endpoints introduced (with paths)
- Breaking API changes (field renames, endpoint changes, removed endpoints)
- Major refactors or removals (explicitly call out deleted legacy code)
- Multi-tenancy implications (new CompanyId-scoped entities, tenant isolation changes)
- Notable risks or follow-ups intentionally deferred
- Build status and any remaining warnings

### What to Exclude
- Step-by-step task checklists
- Code-level trivia (line numbers, import statements)
- Future ideas not implemented
- Duplicate information already in other docs

### Style Reference

Read the last 2-3 entries in `Docs/DevelopmentHistory.md` before writing to match:
- Level of detail
- Bold layer prefixes for multi-layer features
- Bullet point style
- How breaking changes are called out

━━━━━━━━━━━━━━━━━━━━━━
## 3) ProjectStructure.md (CONDITIONAL)
━━━━━━━━━━━━━━━━━━━━━━

**File:** `Docs/ProjectStructure.md`

Update ONLY IF the change affects:
- New entities, repositories, features, or controllers added
- New configuration sections or environment variables
- Changes to the DI container registration pattern
- New architectural patterns or conventions

If updated:
- Add new files to the correct location in the file tree
- Add new features to the Features list
- Keep the existing format and logical ordering
- Do NOT rewrite existing sections

If no structural impact:
- Explicitly state: "No project structure changes required for this cycle."

━━━━━━━━━━━━━━━━━━━━━━
## 4) Release Notes (MANDATORY)
━━━━━━━━━━━━━━━━━━━━━━

**Directory:** `Docs/ReleaseNotes/`
**Filename:** `vX.Y.Z-[feature-name].md` (match existing naming convention)

Check the latest release note file in `Docs/ReleaseNotes/` to determine the next version number. Increment:
- **Major (X):** Breaking changes that require frontend updates
- **Minor (Y):** New features, non-breaking
- **Patch (Z):** Bug fixes, minor improvements

### Format (match existing release notes style):

```markdown
# Release Notes - vX.Y.Z

## [Feature Title]

**Release Date:** [Month Day, Year]
**User Story:** [USXXX]

---

## What's New
[User-facing description of new capabilities]

---

## Breaking Changes (if any)
[What changed, what frontend/users need to do]

---

## Technical Details
[Brief technical summary for developers]

---

## Notes
[Limitations, known issues, follow-ups]
```

Tone:
- Clear, concise, user-facing for the "What's New" section
- Technical but accessible for the "Technical Details" section
- No internal code jargon in user-facing sections

━━━━━━━━━━━━━━━━━━━━━━
## 5) Frontend Documentation (CONDITIONAL)
━━━━━━━━━━━━━━━━━━━━━━

If this cycle introduced or changed API endpoints that the frontend consumes, create or update
a frontend guide using the **Backend_Documentation_for_Frontend** prompt
(see `Docs/Prompts/Backend_Documentation_for_Frontend.md`).

If no frontend-facing API changes:
- Explicitly state: "No frontend documentation changes required for this cycle."

━━━━━━━━━━━━━━━━━━━━━━
## 6) Final Output Checklist (MANDATORY)
━━━━━━━━━━━━━━━━━━━━━━

At the end of your response, include a checklist confirming:

- [ ] `Docs/DevelopmentHistory.md` — entry appended at top
- [ ] `Docs/ProjectStructure.md` — updated or explicitly not needed
- [ ] `Docs/ReleaseNotes/vX.Y.Z_*.md` — release note created
- [ ] Frontend documentation — guide created/updated or explicitly not needed

━━━━━━━━━━━━━━━━━━━━━━
## Rules of Engagement
━━━━━━━━━━━━━━━━━━━━━━

- **Code is truth.** If the story says one thing and the code does another, document what the code does.
- Do NOT invent or exaggerate functionality
- Do NOT delete or edit historical documentation entries
- Be precise, not verbose
- Prefer clarity over completeness
- If something was intentionally NOT done, say so
- If something is unclear, flag it as `⚠️ VERIFY: [description]`
- Match the style and detail level of existing entries — read before writing

━━━━━━━━━━━━━━━━━━━━━━
Available Tools
━━━━━━━━━━━━━━━━━━━━━━
- **PostgreSQL MCP** — Read-only SQL access to the Allocore database. Use to inspect current schema and data.
- **Filesystem MCP** — Read/write access to `C:\Users\cezar\CascadeProjects`. Use to inspect Allocore code and related projects for pattern reference.
- **Web Search** — For external API docs, NuGet package references, or best practices.