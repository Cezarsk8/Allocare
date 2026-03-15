# Allocore — Product Vision

## Identity

**Allocore is a provider management and cost control platform for businesses.** It exists so that companies can have full visibility and control over their service providers — who they are, what they cost, and whether they deliver value.

---

## Core Thesis

Businesses of all sizes rely on a web of service providers — from IT contractors and cleaning services to logistics partners and consultants. Managing these relationships is fragmented: contracts live in email threads, invoices pile up in spreadsheets, and nobody has a clear picture of total spend per provider, cost trends over time, or whether a provider is actually delivering on their commitments.

**Allocore centralizes provider management and cost intelligence.** It turns scattered provider data into a structured, searchable, auditable system where every provider, every contract, and every expense is tracked and analyzed.

---

## What Allocore Does

### 1. Provider Registry
A single source of truth for all service providers. Every provider has a profile with contact details, contract terms, service categories, and engagement history. No more digging through emails to find who provides what.

### 2. Expense Tracking & Cost Management
Every invoice, payment, and cost associated with a provider is captured and categorized. Allocore gives operators instant visibility into:
- Total spend per provider
- Cost trends over time
- Budget vs. actual comparisons
- Cost breakdowns by category, department, or project

### 3. Contract & Relationship Management
Track contract terms, renewal dates, SLAs, and performance metrics. Know when contracts are expiring, which providers are underperforming, and where renegotiation opportunities exist.

### 4. Multi-Tenant Architecture
Built for organizations with multiple business units or companies. Each tenant operates in full data isolation while sharing the platform infrastructure.

---

## Who Allocore Serves

**The business operator responsible for managing vendors and controlling costs.** The operations manager, the finance team, the procurement lead — anyone who needs to answer "How much are we spending on providers, and are we getting value?"

Allocore is NOT:
- An accounting system (it integrates with them)
- A project management tool
- A generic ERP

It is a **focused provider intelligence platform** that sits alongside existing financial and operational tools.

---

## Design Principles

1. **Visibility first.** Every feature must contribute to clearer, faster insight into provider relationships and costs.
2. **Simple data model.** Providers, contracts, expenses — keep the core model clean and extensible.
3. **Multi-tenant by design.** Company-scoped data isolation from day one.
4. **Integration-ready.** Built to connect with accounting systems, payment platforms, and ERP tools.
5. **Auditability.** Every transaction and change is tracked for compliance and accountability.

---

## Technical Foundation

- **.NET 8** with Clean Architecture (Domain → Application → Infrastructure → API)
- **Next.js 15** with TypeScript, Tailwind CSS 4, Atomic Design
- **PostgreSQL** via EF Core
- **CQRS + MediatR** for command/query separation
- **FluentValidation** for input validation
- **JWT Authentication** with role-based authorization
- **Multi-tenant** with company-scoped data isolation

---

**Last Updated**: March 2026
