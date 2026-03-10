# Allocore Development History

## Version History

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
- ✅ Domain: Created `Contract` entity with 19 fields (title, status, dates, billing, financial, legal)
- ✅ Domain: Created `ContractService` join entity (service line items with pricing)
- ✅ Domain: Created `ContractStatus` enum (8 states: Draft → Active → Expired/Cancelled/Terminated)
- ✅ Domain: Created `BillingFrequency` enum (Monthly, Quarterly, SemiAnnual, Annual, OneOff, Custom)
- ✅ Infrastructure: EF Core configurations with indexes, filtered unique constraint on ContractNumber
- ✅ Infrastructure: `ContractRepository` with paged queries, filtering, expiring/renewal queries
- ✅ Application: 8 DTOs (Contract, ContractService, ListItem, Create/Update requests)
- ✅ Application: 3 FluentValidation validators (CreateContract, UpdateContract, CreateContractService)
- ✅ Application: 6 CQRS commands (Create, Update, UpdateStatus, AddService, UpdateService, RemoveService)
- ✅ Application: 4 CQRS queries (GetById, GetPaged, GetExpiring, GetByProvider)
- ✅ Application: Shared `ContractMapper` for DTO mapping
- ✅ API: `ContractsController` with 10 endpoints nested under `/companies/{companyId}/contracts`
- ✅ Migration: `AddContracts` (Contracts + ContractServices tables)

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

## Upcoming

- US006 – Notes System
- US007 – Asset & Inventory Management
