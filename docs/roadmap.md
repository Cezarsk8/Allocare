# Allocare — Roadmap

> Last updated: 2026-03-04

---

## Implementation Status

### Completed
| Story | Title | Domain |
|-------|-------|--------|
| US001 | Backend Scaffolding | Infrastructure |
| US002 | JWT Authentication | Authentication |

### In Progress / Pending
| Story | Title | Domain | Dependencies | Status |
|-------|-------|--------|-------------|--------|
| US003 | Company & Multi-Tenant | Multi-Tenant | US002 | Pending |
| US004 | Provider Management | Providers | US003 | Pending |
| US005 | Provider Contracts | Contracts | US004 | Pending |
| US006 | Notes System | Notes | US004, US005 | Pending |

---

## Tier System

### Tier 1 — Multi-Tenant Foundation
| Story | Title | Effort |
|-------|-------|--------|
| US003 | Company & Multi-Tenant Core | Medium |

> **PM Commentary**: US003 is the foundation for everything. All future entities are company-scoped. Must be implemented first.

### Tier 2 — Provider Management
| Story | Title | Effort |
|-------|-------|--------|
| US004 | Provider Management | Medium |
| US005 | Provider Contracts | Large |

> **PM Commentary**: Providers and contracts are the core domain of Allocare. This tier delivers the primary value proposition — centralizing provider data.

### Tier 3 — Enrichment & Activity
| Story | Title | Effort |
|-------|-------|--------|
| US006 | Notes System | Medium |

> **PM Commentary**: Notes add context to providers and contracts. Important for daily use but not blocking core functionality.

### Future Tiers (from Product Scaffolding)
These stories exist as skeletons in the original Roadmap and need to be written as full user stories:

- **EPIC 2**: Employees & Cost Centers (US010, US011)
- **EPIC 4**: Cost Registration (US030, US031)
- **EPIC 5**: Cost Allocation (US040–US043)
- **EPIC 6**: Projects (US050, US051)
- **EPIC 7**: Reporting & Analytics (US060–US062)

---

## Dependency Graph

```
US001 (Scaffolding) ✅
  └── US002 (JWT Auth) ✅
       └── US003 (Multi-Tenant)
            ├── US004 (Providers)
            │    ├── US005 (Contracts)
            │    │    └── US006 (Notes) ← also depends on US004
            │    └── US006 (Notes)
            ├── US010 (Cost Centers) — skeleton
            └── US011 (Employees) — skeleton
                 └── US030+ (Costs, Allocations, Projects, Reports)
```

---

## Reference

Full product scaffolding document with domain model, tech stack, and epic skeletons:
`Allocore-backend/Docs/User Story/Future Developments/Roadmap.md`
