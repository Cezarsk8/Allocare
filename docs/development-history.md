# Development History

## Backend

### v0.1 – Backend Scaffolding (US001)

**Date**: 2024-12-02 15:43 UTC-03:00

**Summary**: Implemented complete backend scaffolding for Allocore platform following Clean Architecture + CQRS patterns.

**Changes**:
- Created .NET 8 solution with 4 projects: API, Application, Domain, Infrastructure
- Configured project references following Clean Architecture dependency rules
- Installed NuGet packages: MediatR 13.x, FluentValidation 12.x, EF Core 8.x, Swashbuckle, Asp.Versioning
- Domain Layer: Created `Entity` base class, `Result` pattern, placeholder `User`/`Role` entities
- Application Layer: Added `IReadRepository`/`IWriteRepository` abstractions, `ValidationBehavior` pipeline, Ping feature (Query + Handler)
- Infrastructure Layer: Created `InMemoryRepository` implementation, DI setup
- API Layer: `PingController` with versioning, `Program.cs` with Swagger, CORS, health checks, global exception handling
- Documentation: Created `Architecture.md`, `DevelopmentHistory.md`, `UserStories.md`
- Verified all endpoints: `/`, `/swagger`, `/health`, `/api/v1/ping`

**Endpoints**:
- `GET /` → Swagger redirect
- `GET /swagger` → API documentation
- `GET /health` → Health check
- `GET /api/v1/ping` → Returns `{ "message": "pong", "timestamp": "..." }`

---

### v0.2 – JWT Authentication (US002)

**Date**: 2025 (implemented)

**Summary**: Full JWT authentication system with registration, login, refresh tokens, and role-based authorization.

**Changes**:
- User entity with email, password hash (BCrypt), roles, lockout fields
- RefreshToken entity for token rotation
- Login/Register/Refresh/Logout/ForgotPassword/ResetPassword commands
- JWT access tokens + refresh tokens with configurable expiration
- Rate limiting on auth endpoints
- User management CRUD (Admin only for listing/deletion)
- CurrentUser service for authenticated context
- LocaleTag value object for i18n

**Endpoints**:
- `POST /api/v1/auth/register` → Register new user
- `POST /api/v1/auth/login` → Login with email/password
- `POST /api/v1/auth/refresh` → Refresh access token
- `POST /api/v1/auth/logout` → Revoke refresh token
- `POST /api/v1/auth/forgot-password` → Request password reset
- `POST /api/v1/auth/reset-password` → Reset password with token
- `GET /api/v1/users/me` → Current user profile
- `GET /api/v1/users` → List users (Admin)
- `GET /api/v1/users/{id}` → Get user by ID (Admin)
- `PUT /api/v1/users/{id}` → Update user profile
- `DELETE /api/v1/users/{id}` → Deactivate user (Admin)

---

### v0.3 – Multi-Tenant Core (US003)

**Date**: 2026-03-05

**Summary**: Implemented Company management and User-Company mapping as the multi-tenant foundation for Allocore. All future business entities will be company-scoped.

**Changes**:
- Domain: `Company` entity (Name, LegalName, TaxId, IsActive), `UserCompany` join entity, `RoleInCompany` enum (Viewer, Manager, Owner)
- Application: `ICompanyRepository` and `IUserCompanyRepository` abstractions
- Application: DTOs (CompanyDto, UserCompanyDto, CreateCompanyRequest, UpdateCompanyRequest, AddUserToCompanyRequest)
- Application: Validators for all request DTOs using FluentValidation
- Application: CQRS — CreateCompany, UpdateCompany, AddUserToCompany, RemoveUserFromCompany commands with handlers
- Application: CQRS — GetMyCompanies, GetCompanyById, GetCompanyUsers queries with handlers
- Infrastructure: EF configurations for Companies and UserCompanies tables with indexes
- Infrastructure: CompanyRepository and UserCompanyRepository implementations
- Infrastructure: DatabaseSeeder updated to seed test company linked to admin user
- API: CompaniesController (CRUD + user management) and MyController (user-scoped endpoints)
- Database: Migration `AddCompanies` — Companies table, UserCompanies table with unique constraint on (CompanyId, UserId)
- Fixed .sln project paths (added `src/` prefix)
- Fixed connection string port to 5437

**Endpoints**:
- `POST /api/v1/companies` → Create company (Admin only, creator becomes Owner)
- `GET /api/v1/companies/{id}` → Get company by ID (access-checked)
- `PUT /api/v1/companies/{id}` → Update company (Owner/Admin)
- `POST /api/v1/companies/{companyId}/users` → Add user to company (Owner/Admin)
- `DELETE /api/v1/companies/{companyId}/users/{userId}` → Remove user from company (Owner/Admin, prevents last owner removal)
- `GET /api/v1/companies/{companyId}/users` → List company users (access-checked)
- `GET /api/v1/my/companies` → List current user's companies

**Business Rules**:
- A user can belong to multiple companies with different roles
- Each company must have at least one Owner
- Company names must be unique
- TaxId must be unique when provided
- Access checks: Owner/Admin for write operations, company member for read

---

### v0.4 – Provider Management (US004)

**Date**: 2026-03-06

**Summary**: Implemented Provider and ProviderContact entities as the first company-scoped business domain. Providers are the central entity around which contracts, services, and costs revolve.

**Changes**:
- Domain: `Provider` entity (CompanyId, Name, LegalName, TaxId, Category, Website, Description, IsActive), `ProviderContact` entity (Name, Email, Phone, Role, IsPrimary), `ProviderCategory` enum (SaaS, Infrastructure, Consultancy, Benefits, Licensing, Telecommunications, Hardware, Other)
- Application: `IProviderRepository` abstraction with paged/filtered queries
- Application: `PagedResult<T>` generic wrapper for paginated responses
- Application: DTOs (ProviderDto, ProviderContactDto, ProviderListItemDto, CreateProviderRequest, CreateProviderContactRequest, UpdateProviderRequest, AddProviderContactRequest, UpdateProviderContactRequest)
- Application: Validators for all request DTOs using FluentValidation (including nested contact validation and primary contact uniqueness)
- Application: CQRS Commands — CreateProvider, UpdateProvider, DeactivateProvider, AddProviderContact, UpdateProviderContact, RemoveProviderContact with handlers
- Application: CQRS Queries — GetProviderById, GetProvidersPaged with handlers
- Infrastructure: EF configurations for Providers and ProviderContacts tables with indexes (unique CompanyId+Name, Category, ProviderId)
- Infrastructure: ProviderRepository implementation with filtering by category, active status, and search term
- API: ProvidersController with 8 endpoints nested under `/companies/{companyId}/providers`
- Database: Migration `AddProviders` — Providers table, ProviderContacts table with cascade delete

**Endpoints**:
- `GET /api/v1/companies/{companyId}/providers` → List providers (paginated, filterable by category/isActive/search)
- `GET /api/v1/companies/{companyId}/providers/{providerId}` → Get provider with contacts
- `POST /api/v1/companies/{companyId}/providers` → Create provider (optionally with contacts)
- `PUT /api/v1/companies/{companyId}/providers/{providerId}` → Update provider details
- `PATCH /api/v1/companies/{companyId}/providers/{providerId}/deactivate` → Soft-delete provider
- `POST /api/v1/companies/{companyId}/providers/{providerId}/contacts` → Add contact
- `PUT /api/v1/companies/{companyId}/providers/{providerId}/contacts/{contactId}` → Update contact
- `DELETE /api/v1/companies/{companyId}/providers/{providerId}/contacts/{contactId}` → Remove contact

**Business Rules**:
- Providers are company-scoped (CompanyId is immutable after creation)
- Provider names must be unique within a company (DB unique index)
- At most one contact per provider can be marked as primary (enforced in handlers)
- User must have access to the company to perform any provider operation
- Deactivation is a soft-delete (sets IsActive = false)
- Contacts cascade-delete when provider is deleted

---

### v0.5 – Provider Contracts (US005)

**Date**: 2026-03-09

**Summary**: Full CRUD for provider contracts with service line items, status lifecycle, billing/financial tracking, and company-scoped multi-tenancy. Contracts link providers to commercial terms, dates, and services.

**Changes**:
- Domain: Created `Contract` entity with 19 fields (title, status, dates, billing, financial, legal)
- Domain: Created `ContractService` join entity (service line items with pricing)
- Domain: Created `ContractStatus` enum (8 states: Draft → Active → Expired/Cancelled/Terminated)
- Domain: Created `BillingFrequency` enum (Monthly, Quarterly, SemiAnnual, Annual, OneOff, Custom)
- Infrastructure: EF Core configurations with indexes, filtered unique constraint on ContractNumber
- Infrastructure: `ContractRepository` with paged queries, filtering, expiring/renewal queries
- Application: 8 DTOs (Contract, ContractService, ListItem, Create/Update requests)
- Application: 3 FluentValidation validators (CreateContract, UpdateContract, CreateContractService)
- Application: 6 CQRS commands (Create, Update, UpdateStatus, AddService, UpdateService, RemoveService)
- Application: 4 CQRS queries (GetById, GetPaged, GetExpiring, GetByProvider)
- Application: Shared `ContractMapper` for DTO mapping
- API: `ContractsController` with 10 endpoints nested under `/companies/{companyId}/contracts`
- Migration: `AddContracts` (Contracts + ContractServices tables)

**Files Created**: 35 files
- `Allocore.Domain/Entities/Contracts/` — 4 files (Contract, ContractService, ContractStatus, BillingFrequency)
- `Allocore.Infrastructure/Persistence/Configurations/` — 2 files (ContractConfiguration, ContractServiceConfiguration)
- `Allocore.Infrastructure/Persistence/Repositories/ContractRepository.cs`
- `Allocore.Application/Abstractions/Persistence/IContractRepository.cs`
- `Allocore.Application/Features/Contracts/` — 26 files (DTOs, Validators, Commands, Queries, Mapper)
- `Allocore.API/Controllers/v1/ContractsController.cs`

**Files Modified**: 2 files
- `ApplicationDbContext.cs` — Added Contracts + ContractServices DbSets
- `DependencyInjection.cs` — Registered IContractRepository

**Endpoints**:
- `GET /api/v1/companies/{companyId}/contracts` → List contracts (paginated, filterable by provider/status/expiring/search)
- `GET /api/v1/companies/{companyId}/contracts/{contractId}` → Get contract with services and provider
- `GET /api/v1/companies/{companyId}/contracts/expiring` → Get contracts expiring within N days
- `GET /api/v1/companies/{companyId}/contracts/by-provider/{providerId}` → Get contracts for a provider
- `POST /api/v1/companies/{companyId}/contracts` → Create contract (with optional services)
- `PUT /api/v1/companies/{companyId}/contracts/{contractId}` → Update contract details
- `PATCH /api/v1/companies/{companyId}/contracts/{contractId}/status` → Update contract status
- `POST /api/v1/companies/{companyId}/contracts/{contractId}/services` → Add service to contract
- `PUT /api/v1/companies/{companyId}/contracts/{contractId}/services/{serviceId}` → Update service
- `DELETE /api/v1/companies/{companyId}/contracts/{contractId}/services/{serviceId}` → Remove service

**Business Rules**:
- Contracts are company-scoped (CompanyId immutable after creation)
- Contract numbers unique within a company (filtered unique index, nullable)
- Provider must exist and belong to the same company
- Cannot delete a provider that has contracts (FK RESTRICT)
- ContractServices cascade-delete when contract is deleted
- Status transitions are user-driven (no state machine enforcement)
- Financial fields use decimal(18,2) precision

**User Story**: US005

---

### v0.6 – Notes System (US006)

**Date**: 2026-03-09

**Summary**: Polymorphic Notes system that attaches to any entity (Provider, Contract) via EntityType + EntityId pattern. Supports categories, pinning, reminders, and author tracking with batch user name resolution.

**Changes**:
- Domain: `Note` entity (CompanyId, EntityType, EntityId, AuthorUserId, Content, Category, IsPinned, ReminderDate)
- Domain: `NoteEntityType` enum (Provider, Contract — extensible for future entities)
- Domain: `NoteCategory` enum (10 categories: General, Negotiation, Meeting, Decision, Reminder, Issue, FollowUp, PhoneCall, Email, InternalDiscussion)
- Infrastructure: `NoteConfiguration` with composite index on (EntityType, EntityId), filtered index on ReminderDate
- Infrastructure: `NoteRepository` with paged queries, pinned notes, reminders, entity count
- Infrastructure: Added `GetByIdsAsync` to `IUserRepository` / `UserRepository` for batch author name resolution
- Application: `INoteRepository` abstraction
- Application: DTOs (NoteDto, CreateNoteRequest, UpdateNoteRequest)
- Application: `NoteMapper` (shared static mapper following ContractMapper pattern)
- Application: Validators (CreateNoteRequestValidator, UpdateNoteRequestValidator)
- Application: CQRS Commands — CreateNote, UpdateNote, DeleteNote, TogglePinNote with handlers
- Application: CQRS Queries — GetNotesByEntity (paginated), GetReminders with handlers
- API: `NotesController` with 8 endpoints (mixed routing: entity-specific + generic note operations)
- Migration: `AddNotes` (Notes table with 5 indexes, no FKs — polymorphic association)

**Files Created**: 22 files
- `Allocore.Domain/Entities/Notes/` — 3 files (Note, NoteEntityType, NoteCategory)
- `Allocore.Infrastructure/Persistence/Configurations/NoteConfiguration.cs`
- `Allocore.Infrastructure/Persistence/Repositories/NoteRepository.cs`
- `Allocore.Application/Abstractions/Persistence/INoteRepository.cs`
- `Allocore.Application/Features/Notes/` — 15 files (DTOs, Mapper, Validators, Commands, Queries)
- `Allocore.API/Controllers/v1/NotesController.cs`

**Files Modified**: 4 files
- `IUserRepository.cs` — Added `GetByIdsAsync`
- `UserRepository.cs` — Implemented `GetByIdsAsync`
- `ApplicationDbContext.cs` — Added Notes DbSet
- `DependencyInjection.cs` — Registered INoteRepository

**Endpoints**:
- `GET /api/v1/companies/{companyId}/providers/{providerId}/notes` → Get provider notes (paginated timeline)
- `POST /api/v1/companies/{companyId}/providers/{providerId}/notes` → Add note to provider
- `GET /api/v1/companies/{companyId}/contracts/{contractId}/notes` → Get contract notes (paginated timeline)
- `POST /api/v1/companies/{companyId}/contracts/{contractId}/notes` → Add note to contract
- `PUT /api/v1/companies/{companyId}/notes/{noteId}` → Update note (author or admin)
- `DELETE /api/v1/companies/{companyId}/notes/{noteId}` → Delete note (author or admin)
- `PATCH /api/v1/companies/{companyId}/notes/{noteId}/pin` → Toggle pin status
- `GET /api/v1/companies/{companyId}/reminders` → Get upcoming reminders

**Business Rules**:
- Notes are company-scoped (CompanyId, no cross-tenant leakage)
- Polymorphic association: EntityType + EntityId, no FK constraint on EntityId
- Only author or Admin can edit/delete notes
- Pinned notes appear first in timeline, then newest first
- Reminders are queryable but no automated notifications
- Author names resolved via batch user lookup (GetByIdsAsync)

**User Story**: US006

---

### v0.7 – Cost Centers & Employees (US010+011)

**Date**: 2026-03-10

**Summary**: Implemented CostCenter and Employee entities as company-scoped organizational units. Cost centers group employees by department/area. Employees can optionally belong to a cost center. These entities are the foundation for future cost allocation (US012+).

**Changes**:
- Domain: `CostCenter` entity (CompanyId, Code, Name, Description, IsActive) with uppercase-normalized Code
- Domain: `Employee` entity (CompanyId, Name, Email, CostCenterId, JobTitle, HireDate, TerminationDate, IsActive) with lifecycle methods (Terminate, Reactivate, Deactivate, Activate)
- Domain: Extended `NoteEntityType` enum with Employee = 2 and CostCenter = 3
- Infrastructure: EF configurations with unique indexes (CompanyId+Code for CostCenter, CompanyId+Email for Employee), FK CostCenterId → CostCenters with SetNull
- Infrastructure: `CostCenterRepository` with paged/filtered queries and employee count
- Infrastructure: `EmployeeRepository` with paged/filtered queries, cost center eager loading, and cost-center-scoped queries
- Application: `ICostCenterRepository` and `IEmployeeRepository` abstractions
- Application: CostCenter DTOs (CostCenterDto, CostCenterListItemDto, Create/UpdateRequest) + validators
- Application: Employee DTOs (EmployeeDto, EmployeeListItemDto, Create/Update/TerminateRequest) + validators
- Application: CostCenter CQRS — Create, Update, Deactivate, Activate commands + GetById, GetPaged queries
- Application: Employee CQRS — Create, Update, Terminate, Reactivate, Deactivate, Activate commands + GetById, GetPaged, GetByCostCenter queries
- API: `CostCentersController` with 7 endpoints nested under `/companies/{companyId}/cost-centers`
- API: `EmployeesController` with 8 endpoints nested under `/companies/{companyId}/employees`
- Migration: `AddCostCentersAndEmployees` (CostCenters + Employees tables)

**Files Created**: 48 files
- `Allocore.Domain/Entities/CostCenters/CostCenter.cs`
- `Allocore.Domain/Entities/Employees/Employee.cs`
- `Allocore.Infrastructure/Persistence/Configurations/` — 2 files (CostCenterConfiguration, EmployeeConfiguration)
- `Allocore.Infrastructure/Persistence/Repositories/` — 2 files (CostCenterRepository, EmployeeRepository)
- `Allocore.Application/Abstractions/Persistence/` — 2 files (ICostCenterRepository, IEmployeeRepository)
- `Allocore.Application/Features/CostCenters/` — 18 files (DTOs, Validators, Commands, Queries)
- `Allocore.Application/Features/Employees/` — 24 files (DTOs, Validators, Commands, Queries)
- `Allocore.API/Controllers/v1/` — 2 files (CostCentersController, EmployeesController)

**Files Modified**: 3 files
- `NoteEntityType.cs` — Added Employee = 2, CostCenter = 3
- `ApplicationDbContext.cs` — Added CostCenters + Employees DbSets
- `DependencyInjection.cs` — Registered ICostCenterRepository and IEmployeeRepository

**Endpoints**:
- `GET /api/v1/companies/{companyId}/cost-centers` → List cost centers (paginated, filterable by isActive/search)
- `GET /api/v1/companies/{companyId}/cost-centers/{id}` → Get cost center with employee count
- `POST /api/v1/companies/{companyId}/cost-centers` → Create cost center
- `PUT /api/v1/companies/{companyId}/cost-centers/{id}` → Update cost center
- `PATCH /api/v1/companies/{companyId}/cost-centers/{id}/deactivate` → Deactivate cost center
- `PATCH /api/v1/companies/{companyId}/cost-centers/{id}/activate` → Activate cost center
- `GET /api/v1/companies/{companyId}/cost-centers/{id}/employees` → List employees in cost center (paginated)
- `GET /api/v1/companies/{companyId}/employees` → List employees (paginated, filterable by costCenterId/isActive/search)
- `GET /api/v1/companies/{companyId}/employees/{id}` → Get employee with cost center details
- `POST /api/v1/companies/{companyId}/employees` → Create employee
- `PUT /api/v1/companies/{companyId}/employees/{id}` → Update employee
- `PATCH /api/v1/companies/{companyId}/employees/{id}/terminate` → Terminate employee (sets termination date + inactive)
- `PATCH /api/v1/companies/{companyId}/employees/{id}/reactivate` → Reactivate terminated employee
- `PATCH /api/v1/companies/{companyId}/employees/{id}/deactivate` → Deactivate employee
- `PATCH /api/v1/companies/{companyId}/employees/{id}/activate` → Activate employee

**Business Rules**:
- Both entities are company-scoped (CompanyId immutable after creation)
- CostCenter codes are uppercase-normalized and unique per company
- Employee emails are lowercase-normalized and unique per company
- CostCenterId on Employee is nullable (employee can exist without a cost center)
- When creating/updating an employee with a CostCenterId, the handler validates it belongs to the same company
- Deleting a cost center sets CostCenterId to null on all linked employees (SetNull FK)
- Terminate sets both TerminationDate and IsActive=false; Reactivate clears TerminationDate and sets IsActive=true

**User Story**: US010+011

---

## Upcoming

- US012 – Employee-Service Assignment (Cost Allocation)

---

## Frontend

## v0.4.0 — Cost Centers & Employees Frontend (USFW004 / US010+011)

**Date**: 2026-03-15

**Summary**: Full frontend for Cost Centers and Employees management. Includes paginated tables with search/filter, create/edit forms, activate/deactivate/terminate actions, and company sub-navigation tabs. Introduces reusable Pagination molecule and company layout with tab navigation.

**Changes**:
- Created TypeScript types for CostCenter and Employee DTOs, plus shared `PagedResult<T>` generic
- Created costCenterService with 7 API endpoint functions
- Created employeeService with 8 API endpoint functions
- Created React Query hooks for cost centers (6 hooks: query + 4 mutations)
- Created React Query hooks for employees (8 hooks: query + 6 mutations)
- Created Zod validation schemas for cost center and employee forms (including terminate schema)
- Created Pagination molecule (reusable Previous/Next with page info)
- Created CompanyNav component (tab navigation: Geral, Centros de Custo, Colaboradores)
- Created CostCenterForm (create/edit with code, name, description)
- Created CostCenterTable (sortable columns, status badges, activate/deactivate actions)
- Created EmployeeForm (create/edit with cost center dropdown populated from API)
- Created EmployeeTable (6 columns, terminate/reactivate/activate/deactivate actions)
- Created TerminateEmployeeDialog (inline form with date picker and confirmation)
- Created company layout at `/companies/[id]/layout.tsx` (header + nav tabs + children)
- Refactored company detail page (header/back moved to layout, page becomes "Geral" tab content)
- Created cost centers page with search, status filter, pagination, inline create/edit
- Created employees page with search, cost center filter, status filter, pagination, inline create/edit, terminate dialog

**Files Created**:
- `src/types/costCenter.ts` — CostCenter DTOs
- `src/types/employee.ts` — Employee DTOs
- `src/types/common.ts` — PagedResult<T> generic
- `src/app/services/costCenterService.ts` — CostCenter API service (7 endpoints)
- `src/app/services/employeeService.ts` — Employee API service (8 endpoints)
- `src/app/hooks/useCostCenters.ts` — React Query hooks (6 hooks)
- `src/app/hooks/useEmployees.ts` — React Query hooks (8 hooks)
- `src/app/constants/costCenterSchemas.ts` — Zod schemas
- `src/app/constants/employeeSchemas.ts` — Zod schemas
- `src/app/components/ui/molecules/Pagination.tsx` — Reusable pagination
- `src/app/components/companies/CompanyNav.tsx` — Company tab navigation
- `src/app/components/cost-centers/CostCenterForm.tsx` — Create/Edit form
- `src/app/components/cost-centers/CostCenterTable.tsx` — List table
- `src/app/components/employees/EmployeeForm.tsx` — Create/Edit form
- `src/app/components/employees/EmployeeTable.tsx` — List table
- `src/app/components/employees/TerminateEmployeeDialog.tsx` — Terminate inline dialog
- `src/app/(protected)/companies/[id]/layout.tsx` — Company layout with nav
- `src/app/(protected)/companies/[id]/cost-centers/page.tsx` — Cost centers page
- `src/app/(protected)/companies/[id]/employees/page.tsx` — Employees page

**Files Modified**:
- `src/app/components/ui/molecules/index.ts` — Added Pagination export
- `src/app/(protected)/companies/[id]/page.tsx` — Refactored (header moved to layout)

**User-Facing Changes**:
- `/companies/[id]` — Now shows tab navigation (Geral, Centros de Custo, Colaboradores)
- `/companies/[id]/cost-centers` — List, create, edit, activate/deactivate cost centers
- `/companies/[id]/employees` — List, create, edit, terminate/reactivate, activate/deactivate employees
- Pagination on all list views
- Search + filter by status/cost center
- Employee form has cost center dropdown

**User Story**: USFW004 (backend: US010+011)

---

## v0.3.0 — Authentication Frontend (USFW003 / US002)

**Date**: 2026-03-09

**Summary**: Full authentication UI — login, register, logout, auth context, route guards, and token management. Connects to US002 JWT backend endpoints.

**Changes**:
- Created auth types matching backend DTOs (`UserDto`, `AuthResponse`, `LoginRequest`, `RegisterRequest`, `RefreshTokenRequest`)
- Created auth service with login, register, refresh, and logout API calls
- Updated apiClient with 401 response interceptor (clears tokens, redirects to `/login`)
- Created AuthContext with AuthProvider (user state, localStorage persistence, login/register/logout actions)
- Created `useAuth` hook with context validation
- Created Zod validation schemas (`loginSchema`, `registerSchema`) matching backend rules
- Created LoginForm component (React Hook Form + Zod, loading spinner, API error display)
- Created RegisterForm component (responsive 2-col name fields, password complexity validation)
- Created auth layout with centered card, app branding, authenticated redirect
- Created protected layout with auth guard (redirect to `/login` if unauthenticated)
- Wrapped app with AuthProvider in providers.tsx
- Updated root page to redirect to `/companies` (auth guard handles the rest)

**Files Created**:
- `src/types/auth.ts` — Auth DTOs
- `src/app/services/authService.ts` — Auth API service (4 endpoints)
- `src/app/context/AuthContext.tsx` — Auth provider with state management
- `src/app/hooks/useAuth.ts` — Auth hook
- `src/app/constants/authSchemas.ts` — Zod schemas for login/register
- `src/app/components/auth/LoginForm.tsx` — Login form organism
- `src/app/components/auth/RegisterForm.tsx` — Register form organism
- `src/app/(auth)/layout.tsx` — Auth pages layout (centered, branded)
- `src/app/(auth)/login/page.tsx` — Login page
- `src/app/(auth)/register/page.tsx` — Register page
- `src/app/(protected)/layout.tsx` — Protected routes guard

**Files Modified**:
- `src/app/services/apiClient.ts` — Added 401 interceptor
- `src/app/providers.tsx` — Added AuthProvider wrapper
- `src/app/page.tsx` — Changed redirect from `/login` to `/companies`

**User-Facing Changes**:
- `/login` — Login page with email/password form
- `/register` — Registration page with name, email, password
- Protected routes redirect to `/login` if not authenticated
- Auth pages redirect to `/companies` if already authenticated

**User Story**: USFW003 (backend: US002)

---

## v0.2.0 — Company Management UI (USFW002 / US003)

**Date**: 2026-03-06

**Summary**: Frontend implementation for Company Management & Multi-Tenant UI. Includes API client, company service, React Query hooks, Zod validation schemas, pages, and form components.

**Changes**:
- Created TypeScript types for Company DTOs (`src/types/company.ts`)
- Created Axios API client with token interceptor (`src/app/services/apiClient.ts`)
- Created company service with all 7 API endpoints (`src/app/services/companyService.ts`)
- Created React Query hooks for companies (`src/app/hooks/useCompanies.ts`)
- Created Zod validation schemas (`src/app/constants/companySchemas.ts`)
- Created My Companies page with list, loading, error, empty states
- Created Company Detail page with users table and management actions
- Created CompanyForm component (create + edit modes with React Hook Form + Zod)
- Created AddUserToCompanyForm component
- All forms responsive (single column mobile, inline desktop)

**Files Created**:
- `src/types/company.ts` — Company DTOs
- `src/app/services/apiClient.ts` — Axios instance with auth interceptor
- `src/app/services/companyService.ts` — Company API service
- `src/app/hooks/useCompanies.ts` — React Query hooks (7 hooks)
- `src/app/constants/companySchemas.ts` — Zod schemas
- `src/app/(protected)/companies/page.tsx` — My Companies page
- `src/app/(protected)/companies/[id]/page.tsx` — Company Detail page
- `src/app/components/companies/CompanyForm.tsx` — Create/Edit form
- `src/app/components/companies/AddUserToCompanyForm.tsx` — Add user form

**User-Facing Changes**:
- `/companies` — List user's companies, create new
- `/companies/[id]` — View company details, edit, manage users

**User Story**: USFW002 (backend: US003)

---

## v0.1.0 — Frontend Project Scaffolding (USFW001)

**Date**: 2026-03-06

**Summary**: Complete frontend scaffolding with Next.js 16, TypeScript 5, Tailwind CSS 4, React Query 5, and Atomic Design folder structure.

**Changes**:
- Initialized Next.js 16 project with App Router, TypeScript, Tailwind CSS 4
- Installed all dependencies: axios, react-query, zod, react-hook-form, lucide-react, sonner
- Configured TypeScript (strict, path aliases `@/*`)
- Configured PostCSS with `@tailwindcss/postcss` (Tailwind v4)
- Created global CSS with base styles and scrollbar utilities
- Created root layout (Server Component, pt-BR, Allocare metadata)
- Created Providers wrapper (QueryClient + Toaster)
- Created root page with redirect to `/login`
- Created middleware with route matcher placeholder
- Created Atomic Design folder structure (atoms, molecules, auth, layout, etc.)
- Created barrel exports for UI components
- Created env files (.env.example, .env.local)
- Created README with project documentation

**Files Created**:
- `package.json` — project config with all dependencies
- `tsconfig.json` — TypeScript strict config
- `postcss.config.mjs` — Tailwind v4 PostCSS
- `next.config.ts` — minimal Next.js config
- `eslint.config.mjs` — ESLint flat config
- `.env.example` — env template
- `.gitignore` — standard Next.js ignores
- `src/app/globals.css` — Tailwind v4 styles
- `src/app/layout.tsx` — root layout
- `src/app/providers.tsx` — client providers
- `src/app/page.tsx` — root redirect
- `src/middleware.ts` — route middleware
- `src/app/components/ui/atoms/index.ts` — barrel
- `src/app/components/ui/molecules/index.ts` — barrel
- `src/app/components/ui/index.ts` — aggregated barrel
- `README.md` — project docs
- 15 placeholder directories with `.gitkeep`

**User-Facing Changes**:
- App starts on `http://localhost:3000` and redirects to `/login`

**User Story**: USFW001
