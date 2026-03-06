# Allocare — Roadmap

> Last updated: 2026-03-06

> **Note:** Backend stories use `USXXX` IDs. Frontend stories use `USFWXXX` IDs. Both tracks can progress in parallel once their dependencies are met.

---

## Implementation Status

### Completed
| Story | Title | Domain |
|-------|-------|--------|
| US001 | Backend Scaffolding | Infrastructure |
| US002 | JWT Authentication | Authentication |
| US003 | Company & Multi-Tenant | Multi-Tenant |
| USFW001 | Frontend Project Scaffolding | Frontend / Infrastructure |
| USFW002 | Company Management Frontend | Frontend / Multi-Tenant |

### In Progress / Pending
| Story | Title | Domain | Dependencies | Status |
|-------|-------|--------|-------------|--------|
| US004 | Provider Management | Providers | US003 | Pending |
| US005 | Provider Contracts | Contracts | US004 | Pending |
| US006 | Notes System | Notes | US004, US005 | Pending |
| US007 | Asset & Inventory Management | Inventory | US003, US011 | Pending |
| US008 | Payment & Billing Domain | Payments | US004 | Pending |
| US009 | Email Payment Integration | Email/AI | US008 | Pending |

---

## Tier System

### ~~Tier 1 — Multi-Tenant Foundation~~ ✅ Completed
| Story | Title | Effort | Status |
|-------|-------|--------|--------|
| US003 | Company & Multi-Tenant Core | Medium | ✅ Done |

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

### Tier 4 — Asset & Inventory Management
| Story | Title | Effort |
|-------|-------|--------|
| US007 | Asset & Inventory Management | Large |

> **PM Commentary**: Physical asset tracking (notebooks, monitors, etc.) with assignment history to employees. Depends on US003 for company scoping and US011 for employee assignments. Independent from the Provider/Contract domain.

### Tier 5 — Payment & Email Integration
| Story | Title | Effort |
|-------|-------|--------|
| US008 | Payment & Billing Domain | Large |
| US009 | Email Payment Integration | XLarge |

> **PM Commentary**: US008 introduces the payment domain (recurring and one-off payments, status lifecycle, attachments). US009 adds email integration with AI classification to auto-create payments from incoming invoices/boletos. Together they automate the payment tracking workflow.

### ~~Frontend Tier 1 — Frontend Foundation~~ ✅ Completed
| Story | Title | Effort | Status |
|-------|-------|--------|--------|
| USFW001 | Frontend Project Scaffolding | Small | ✅ Done |
| USFW002 | Company Management Frontend | Medium | ✅ Done |

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
BACKEND:
US001 (Scaffolding) ✅
  └── US002 (JWT Auth) ✅
       └── US003 (Multi-Tenant) ✅
            ├── US004 (Providers)
            │    ├── US005 (Contracts)
            │    │    └── US006 (Notes) ← also depends on US004
            │    ├── US006 (Notes)
            │    └── US008 (Payments)
            │         └── US009 (Email Integration)
            ├── US010 (Cost Centers) — skeleton
            └── US011 (Employees) — skeleton
                 ├── US007 (Asset & Inventory Management)
                 └── US030+ (Costs, Allocations, Projects, Reports)

FRONTEND:
USFW001 (Scaffolding) ✅
  └── USFW002 (Company Management) ✅
  └── USFW003+ (Provider UI, Contract UI, Dashboard, etc.)
```

---

## Reference

Full product scaffolding document with domain model, tech stack, and epic skeletons:
`Allocore-backend/Docs/User Story/Future Developments/Roadmap.md`
