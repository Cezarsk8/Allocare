# Provider System – Architecture & Product Scaffolding

## 1. Product Overview

**Problem**

Company managers need to understand **how services and tools impact their costs per employee, cost center and project**. Today this is usually done with spreadsheets and ad-hoc reports, making it hard to:

- See how much each employee or cost center is really costing (SaaS, benefits, infra, etc.)
- Split shared services (e.g., an IT tool) across multiple cost centers or projects
- Track which providers and services are actually used and by whom

**Solution**

Provider System is a **multi-company cost allocation platform** where an admin user can:

- Register one or more **companies**
- Manage **employees**, **cost centers**, **providers**, **services**, and **projects**
- Register **costs** (recurring or one-off) from providers
- Allocate those costs to:
  - Employees (user-based services)
  - Cost centers (department / BU)
  - Projects (aggregated initiatives)
- Generate reports by **company, cost center, employee, provider, service and project**.

---

## 2. Core Concepts & Business Rules

### 2.1 Glossary

| Concept        | Description                                                                                       |
|----------------|---------------------------------------------------------------------------------------------------|
| Admin User     | Application user who manages one or more companies.                                               |
| Company        | Legal or operational entity being managed (tenant).                                               |
| Employee       | Person working for a company, belonging to one cost center; can be associated to projects.        |
| Cost Center    | Financial responsibility unit inside a company (e.g., IT, Sales, HR).                             |
| Provider       | External service/vendor (SaaS, infra, consultancy, etc.).                                         |
| Service        | Specific service/product offered by a provider (e.g., "Figma Professional Seat", "AWS EC2").      |
| Cost           | Monetary record of a service for a period (monthly, annual, lump sum, asset acquisition, etc.).   |
| Allocation     | Rule or record that splits a cost across employees, cost centers or projects.                     |
| Project        | Internal initiative that aggregates multiple services and costs, optionally allocated to users.    |

### 2.2 High-Level Business Rules

1. **Multi-tenant**  
   - One Admin User can manage multiple Companies.  
   - All entities (employees, cost centers, providers, services, costs, projects) are scoped to a Company.

2. **Employees & Cost Centers**
   - Each employee belongs to exactly one primary **Cost Center**.
   - Optional: support secondary cost centers or percentages in a later version.

3. **Providers & Services**
   - A **Provider** can offer multiple **Services**.
   - A **Service** is defined within a Company context (same provider could exist in multiple companies independently).

4. **Cost Types**
   - **Recurring**: monthly, quarterly, annual subscriptions.
   - **Lump sum / project**: one-off services (consulting, implementation).
   - **Asset / acquisition**: equipment or licenses purchased once but amortized over time (future enhancement).

5. **Allocation Types**
   - **Per Employee**: cost divided among specific employees (per seat, per license).
   - **Per Cost Center**: cost distributed among one or more cost centers using percentages or weights.
   - **Per Project**: project aggregates service costs; project cost can then be allocated to:
     - employees using the project, and/or  
     - cost centers responsible for the project.

6. **Reporting**
   - At minimum, support:
     - Cost by **company** per month
     - Cost by **cost center** per month
     - Cost by **employee** (total + breakdown by provider/service)
     - Cost by **project** (total and per contributor)
   - All reports must respect company boundaries (multi-tenant isolation).

---

## 3. Domain Model (MVP)

> This is the **initial** model. We can extend with more tables (e.g. invoices, amortization, usage metrics) later.

### 3.1 Main Entities (Logical)

| Entity            | Key Fields (Conceptual, not final types)                                                      | Notes |
|-------------------|-----------------------------------------------------------------------------------------------|-------|
| **User**          | Id, Name, Email, PasswordHash, Role (Admin, maybe future roles), CreatedAt                   | Application account. Not an employee. Multi-company admin. |
| **Company**       | Id, Name, TaxId (optional), Active, CreatedAt                                                | Tenant. Linked to Users via UserCompany table. |
| **UserCompany**   | UserId, CompanyId, RoleInCompany (Owner, Manager)                                            | Many-to-many between User and Company. |
| **CostCenter**    | Id, CompanyId, Code, Name, Description                                                       | One company, many cost centers. |
| **Employee**      | Id, CompanyId, Name, Email, CostCenterId, HireDate, TerminationDate (nullable), Active       | Employee belongs to a single company. |
| **Provider**      | Id, CompanyId, Name, Category (SaaS, Infra, Benefits, etc.), ContactInfo                     | Provider is defined per company. |
| **Service**       | Id, CompanyId, ProviderId, Name, BillingType (Monthly/Annual/OneOff), UnitType (Seat, GB)    | Abstract description of what is being billed. |
| **Project**       | Id, CompanyId, Code, Name, Description, StartDate, EndDate (nullable), Status                | Aggregates costs and users. |
| **ProjectMember** | ProjectId, EmployeeId, Role, AllocationWeight (optional)                                     | Allows splitting project costs by member. |
| **Cost**          | Id, CompanyId, ServiceId, ProviderId, PeriodStart, PeriodEnd, Amount, Currency, Notes        | Single line of actual monetary cost. |
| **CostAllocation**| Id, CostId, AllocationType (Employee/CostCenter/Project), TargetId, Percentage, FixedAmount  | Splits the Cost across targets. Sum of percentages per cost = 100%. |
| **EmployeeService** (optional MVP+) | CompanyId, EmployeeId, ServiceId, StartDate, EndDate                        | For seat-based services per user. Can be used to auto-generate allocations. |

---

## 4. Technology Stack

### 4.1 Backend

- **.NET 8 Web API** (or latest LTS) with **Clean Architecture** (Domain, Application, Infrastructure, API layers):contentReference[oaicite:0]{index=0}  
- **PostgreSQL** as primary database.  
- ORM: EF Core (or Dapper if you prefer, but EF Core is easier for scaffolding).:contentReference[oaicite:1]{index=1}  
- Authentication: JWT Bearer (email + password login).  
- Multi-tenancy: CompanyId filter on all queries + UserCompany mapping.  
- Migrations: EF Core Migrations.  
- Documentation: Swagger/OpenAPI.

### 4.2 Frontend

- **Next.js 15** with **App Router**, TypeScript, and React.:contentReference[oaicite:2]{index=2}  
- UI framework: Tailwind CSS (and optionally a component library like shadcn/ui in the future).  
- Authentication: JWT stored in http-only cookies or secure storage, login via backend API.  
- Folder structure aligned with Next.js project structure best practices (e.g. `app/(public)/login`, `app/(app)/[companyId]/dashboard`).:contentReference[oaicite:3]{index=3}  

---

## 5. High-Level Architecture

### 5.1 Backend Layers

| Layer          | Responsibility                                                                                                  | Example Contents                                                                 |
|----------------|------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------|
| **Domain**     | Enterprise rules, entities and value objects. No dependencies on frameworks.                                   | Entities (`Company`, `Employee`, `Cost`, `CostAllocation`), domain services.     |
| **Application**| Use cases / business logic; orchestrates repositories and services.                                            | Commands/queries (CreateCompany, RegisterCost), interfaces for repositories.     |
| **Infrastructure** | Persistence and external services implementation.                                                          | EF Core DbContext, repository implementations, migrations.                       |
| **API**        | HTTP endpoints, controllers/minimal APIs, DTO mapping, auth.                                                   | Controllers, request/response models, validation, Swagger config.                |

This pattern is consistent with modern .NET Clean Architecture approaches for scalable Web APIs.:contentReference[oaicite:4]{index=4}  

### 5.2 Frontend Structure (App Router)

| Area                 | Path / Folder                                 | Responsibility                                         |
|----------------------|-----------------------------------------------|-------------------------------------------------------|
| Public pages         | `app/(public)/login`, `app/(public)/register`| Authentication, onboarding                            |
| Company dashboard    | `app/(app)/[companyId]/dashboard`            | Overview cards: total costs, by cost center, top providers |
| Master data          | `app/(app)/[companyId]/employees`, `providers`, `services`, `projects`, `cost-centers` | CRUD operations |
| Cost management      | `app/(app)/[companyId]/costs`                | List + filter of costs and allocations                |
| Reports              | `app/(app)/[companyId]/reports/...`          | Tables/charts by employee, cost center, project       |

Use **client components** only where you need interactivity, and server components for data-heavy pages, following Next.js recommendations.:contentReference[oaicite:5]{index=5}  

### 5.3 API Design Guidelines

- RESTful endpoints versioned under `/api/v1`.
- Every resource constrained by `companyId` (multi-tenant).
- Use standard patterns:
  - `GET /api/v1/companies/{companyId}/employees`
  - `POST /api/v1/companies/{companyId}/providers`
  - `POST /api/v1/companies/{companyId}/costs/{costId}/allocations`
- Use pagination for listing endpoints; filtering by period/date for costs and reports.
- All endpoints require authentication except login/registration.

---

## 6. Initial Epics & User Story Skeletons

> You can feed each Epic separately into your coding agent with its set of User Stories.

### EPIC 1 – Authentication & Company Management

| US Id | User Story (Skeleton) |
|-------|------------------------|
| US001 | As an **admin user**, I want to **register and log in** so that I can access my companies securely. |
| US002 | As an **admin user**, I want to **create and manage companies** so that I can separate data per company. |
| US003 | As an **admin user**, I want to **invite other admins/managers to a company** so that we can co-manage data. |

Key backend tasks: JWT auth, User, Company, UserCompany tables, auth middleware.

---

### EPIC 2 – Master Data: Employees & Cost Centers

| US Id | User Story (Skeleton) |
|-------|------------------------|
| US010 | As a **company manager**, I want to **create and manage cost centers** so that I can allocate costs to them. |
| US011 | As a **company manager**, I want to **create and manage employees** and link them to cost centers so that I can allocate per-employee costs. |

---

### EPIC 3 – Providers & Services

| US Id | User Story (Skeleton) |
|-------|------------------------|
| US020 | As a **company manager**, I want to **register providers** so that I can group services by vendor. |
| US021 | As a **company manager**, I want to **register services under each provider** (billing type, unit type) so that I can later attach costs to these services. |

---

### EPIC 4 – Cost Registration

| US Id | User Story (Skeleton) |
|-------|------------------------|
| US030 | As a **company manager**, I want to **record a cost for a service and period** so that I have a structured register of expenses. |
| US031 | As a **company manager**, I want to **define whether a cost is recurring, annual or one-off** so that the system can handle different billing models. |

---

### EPIC 5 – Cost Allocation

| US Id | User Story (Skeleton) |
|-------|------------------------|
| US040 | As a **company manager**, I want to **allocate a cost across employees** (percentage or equal split) so I can see per-employee cost. |
| US041 | As a **company manager**, I want to **allocate a cost across cost centers** so I can reflect shared services in financial reports. |
| US042 | As a **company manager**, I want to **allocate a cost to a project** so I can group service costs under project budgets. |
| US043 (MVP+) | As a **company manager**, I want to **auto-allocate recurring seat-based services** based on `EmployeeService` records so I don’t need to manually update allocations every month. |

---

### EPIC 6 – Projects

| US Id | User Story (Skeleton) |
|-------|------------------------|
| US050 | As a **company manager**, I want to **create projects and add employees as members** so that I can track project-related costs. |
| US051 | As a **company manager**, I want to **link costs to projects** so that I can see the full financial picture of each initiative. |

---

### EPIC 7 – Reporting & Analytics

| US Id | User Story (Skeleton) |
|-------|------------------------|
| US060 | As a **company manager**, I want **reports by cost center and period** so that I understand departmental spending. |
| US061 | As a **company manager**, I want **reports by employee** so that I know the total monthly cost per person and which services contribute to it. |
| US062 | As a **company manager**, I want **project cost summaries** (total and breakdown by provider/service) so that I can evaluate project profitability. |

Project cost management and cost allocation software in the market typically focus on centralizing expenses, linking them to projects or departments, and providing real-time profitability and budget tracking—this structure follows those same principles.:contentReference[oaicite:6]{index=6}  

---

## 7. Non-Functional Requirements (MVP)

| Area         | Requirement                                                                 |
|--------------|------------------------------------------------------------------------------|
| Security     | JWT auth, per-company isolation (CompanyId in all queries), HTTPS only.     |
| Performance  | Paginated lists, indexed columns on CompanyId, PeriodStart, ProviderId.     |
| Auditability | Basic created/updated timestamps on all main entities.                      |
| Extensibility| Domain layer not tied to DB or UI; new allocation types can be added later. |
| Observability| Basic logging (request logs, error logs) and health check endpoint.         |

---

## 8. Next Steps For Implementation

1. Implement **EPIC 1** (auth + company) with full clean architecture and migrations.  
2. Implement **EPIC 2 & 3** (master data) – employees, cost centers, providers, services.  
3. Implement **EPIC 4** (Cost + CostAllocation) with simple manual allocations.  
4. Add **EPIC 5 & 6** (projects + linkage) when cost base is stable.  
5. Finally, build **EPIC 7** reporting endpoints and Next.js dashboards.

Each Epic can be turned into a separate document with detailed acceptance criteria, DB schema changes, and API contracts for your coding AI.