# US010+011 – Cost Centers & Employees

## Description

**As** an Admin user managing a company in Allocore,
**I need** to register and manage Cost Centers and Employees,
**So that** I can organize my workforce by department/cost center and have a foundation for allocating provider costs to specific people and areas.

Currently, Allocore has Providers → Contracts → Services with pricing, but no way to allocate those costs to people or departments. This story introduces CostCenter and Employee entities (company-scoped) with full CRUD operations. Employees can optionally belong to a CostCenter. These entities prepare the ground for future cost allocation stories (US012+).

**Priority**: High
**Dependencies**: US003 – Company & UserCompany (Multi-Tenant Core)

---

## Step 1: Domain Layer — CostCenter Entity

### 1.1 Create CostCenter entity

- [x] Create `Allocore.Domain/Entities/CostCenters/CostCenter.cs`: ✅ DONE
  ```csharp
  namespace Allocore.Domain.Entities.CostCenters;

  using Allocore.Domain.Common;

  public class CostCenter : Entity
  {
      public Guid CompanyId { get; private set; }
      public string Code { get; private set; } = string.Empty;
      public string Name { get; private set; } = string.Empty;
      public string? Description { get; private set; }
      public bool IsActive { get; private set; } = true;

      private CostCenter() { } // EF Core

      public static CostCenter Create(
          Guid companyId,
          string code,
          string name,
          string? description = null)
      {
          return new CostCenter
          {
              CompanyId = companyId,
              Code = code.Trim().ToUpperInvariant(),
              Name = name.Trim(),
              Description = description?.Trim(),
              IsActive = true
          };
      }

      public void Update(string code, string name, string? description)
      {
          Code = code.Trim().ToUpperInvariant();
          Name = name.Trim();
          Description = description?.Trim();
          UpdatedAt = DateTime.UtcNow;
      }

      public void Deactivate()
      {
          IsActive = false;
          UpdatedAt = DateTime.UtcNow;
      }

      public void Activate()
      {
          IsActive = true;
          UpdatedAt = DateTime.UtcNow;
      }
  }
  ```
  - **Business rules**: Code is uppercase-normalized. Unique per company.

---

## Step 2: Domain Layer — Employee Entity

### 2.1 Create Employee entity

- [x] Create `Allocore.Domain/Entities/Employees/Employee.cs`:
  ```csharp
  namespace Allocore.Domain.Entities.Employees;

  using Allocore.Domain.Common;
  using Allocore.Domain.Entities.CostCenters;

  public class Employee : Entity
  {
      public Guid CompanyId { get; private set; }
      public string Name { get; private set; } = string.Empty;
      public string Email { get; private set; } = string.Empty;
      public Guid? CostCenterId { get; private set; }
      public string? JobTitle { get; private set; }
      public DateTime? HireDate { get; private set; }
      public DateTime? TerminationDate { get; private set; }
      public bool IsActive { get; private set; } = true;

      public CostCenter? CostCenter { get; private set; }

      private Employee() { } // EF Core

      public static Employee Create(
          Guid companyId,
          string name,
          string email,
          Guid? costCenterId = null,
          string? jobTitle = null,
          DateTime? hireDate = null)
      {
          return new Employee
          {
              CompanyId = companyId,
              Name = name.Trim(),
              Email = email.Trim().ToLowerInvariant(),
              CostCenterId = costCenterId,
              JobTitle = jobTitle?.Trim(),
              HireDate = hireDate,
              IsActive = true
          };
      }

      public void Update(
          string name,
          string email,
          Guid? costCenterId,
          string? jobTitle,
          DateTime? hireDate)
      {
          Name = name.Trim();
          Email = email.Trim().ToLowerInvariant();
          CostCenterId = costCenterId;
          JobTitle = jobTitle?.Trim();
          HireDate = hireDate;
          UpdatedAt = DateTime.UtcNow;
      }

      public void Terminate(DateTime terminationDate)
      {
          TerminationDate = terminationDate;
          IsActive = false;
          UpdatedAt = DateTime.UtcNow;
      }

      public void Reactivate()
      {
          TerminationDate = null;
          IsActive = true;
          UpdatedAt = DateTime.UtcNow;
      }

      public void Deactivate()
      {
          IsActive = false;
          UpdatedAt = DateTime.UtcNow;
      }

      public void Activate()
      {
          IsActive = true;
          UpdatedAt = DateTime.UtcNow;
      }
  }
  ```
  - **Business rules**: Email is lowercase-normalized, unique per company. CostCenterId is nullable (employee can exist without a cost center). Terminate sets both TerminationDate and IsActive=false. Reactivate clears TerminationDate and sets IsActive=true.

### 2.2 Extend NoteEntityType enum

- [x] Update `Allocore.Domain/Entities/Notes/NoteEntityType.cs` — add `Employee = 2` and `CostCenter = 3`.

---

## Step 3: Infrastructure Layer — EF Core Configurations

### 3.1 CostCenter Configuration

- [x] Create `Infrastructure/Persistence/Configurations/CostCenterConfiguration.cs`:
  - Table: `CostCenters`
  - Properties: CompanyId (required), Code (required, max 50), Name (required, max 200), Description (max 2000), IsActive (required, default true)
  - Unique index: `(CompanyId, Code)`
  - Index: `CompanyId`

### 3.2 Employee Configuration

- [x] Create `Infrastructure/Persistence/Configurations/EmployeeConfiguration.cs`:
  - Table: `Employees`
  - Properties: CompanyId (required), Name (required, max 200), Email (required, max 300), CostCenterId (nullable), JobTitle (max 200), HireDate, TerminationDate, IsActive (required, default true)
  - Unique index: `(CompanyId, Email)`
  - Indexes: `CompanyId`, `CostCenterId`
  - FK: CostCenterId → CostCenters.Id with `DeleteBehavior.SetNull`

### 3.3 Update ApplicationDbContext

- [x] Add `DbSet<CostCenter> CostCenters` and `DbSet<Employee> Employees` to `ApplicationDbContext.cs`.

---

## Step 4: Infrastructure Layer — Repositories

### 4.1 ICostCenterRepository

- [x] Create `Application/Abstractions/Persistence/ICostCenterRepository.cs`:
  ```csharp
  public interface ICostCenterRepository : IReadRepository<CostCenter>, IWriteRepository<CostCenter>
  {
      Task<bool> ExistsByCodeInCompanyAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
      Task<bool> ExistsByCodeInCompanyExcludingAsync(Guid companyId, string code, Guid excludeId, CancellationToken cancellationToken = default);
      Task<(IEnumerable<CostCenter> CostCenters, int TotalCount)> GetPagedByCompanyAsync(
          Guid companyId, int page, int pageSize,
          bool? isActiveFilter = null,
          string? searchTerm = null,
          CancellationToken cancellationToken = default);
      Task<int> GetEmployeeCountAsync(Guid costCenterId, CancellationToken cancellationToken = default);
  }
  ```

### 4.2 IEmployeeRepository

- [x] Create `Application/Abstractions/Persistence/IEmployeeRepository.cs`:
  ```csharp
  public interface IEmployeeRepository : IReadRepository<Employee>, IWriteRepository<Employee>
  {
      Task<Employee?> GetByIdWithCostCenterAsync(Guid id, CancellationToken cancellationToken = default);
      Task<bool> ExistsByEmailInCompanyAsync(Guid companyId, string email, CancellationToken cancellationToken = default);
      Task<bool> ExistsByEmailInCompanyExcludingAsync(Guid companyId, string email, Guid excludeId, CancellationToken cancellationToken = default);
      Task<(IEnumerable<Employee> Employees, int TotalCount)> GetPagedByCompanyAsync(
          Guid companyId, int page, int pageSize,
          Guid? costCenterIdFilter = null,
          bool? isActiveFilter = null,
          string? searchTerm = null,
          CancellationToken cancellationToken = default);
      Task<(IEnumerable<Employee> Employees, int TotalCount)> GetPagedByCostCenterAsync(
          Guid costCenterId, int page, int pageSize,
          CancellationToken cancellationToken = default);
  }
  ```

### 4.3 Implement CostCenterRepository

- [x] Create `Infrastructure/Persistence/Repositories/CostCenterRepository.cs` following ProviderRepository pattern.

### 4.4 Implement EmployeeRepository

- [x] Create `Infrastructure/Persistence/Repositories/EmployeeRepository.cs` following ProviderRepository pattern. Include `.Include(e => e.CostCenter)` for queries that need cost center info.

### 4.5 Register repositories

- [x] Update `Infrastructure/DependencyInjection.cs` — register `ICostCenterRepository` and `IEmployeeRepository`.

---

## Step 5: Application Layer — CostCenter DTOs & Validators

### 5.1 CostCenter DTOs

- [x] Create `Application/Features/CostCenters/DTOs/CostCenterDto.cs`
- [x] Create `Application/Features/CostCenters/DTOs/CostCenterListItemDto.cs`
- [x] Create `Application/Features/CostCenters/DTOs/CreateCostCenterRequest.cs`
- [x] Create `Application/Features/CostCenters/DTOs/UpdateCostCenterRequest.cs`

### 5.2 CostCenter Validators

- [x] Create `Application/Features/CostCenters/Validators/CreateCostCenterRequestValidator.cs`
- [x] Create `Application/Features/CostCenters/Validators/UpdateCostCenterRequestValidator.cs`

### 5.3 CostCenter Mapper

- [x] Create `Application/Features/CostCenters/CostCenterMapper.cs`

---

## Step 6: Application Layer — Employee DTOs & Validators

### 6.1 Employee DTOs

- [x] Create `Application/Features/Employees/DTOs/EmployeeDto.cs`
- [x] Create `Application/Features/Employees/DTOs/EmployeeListItemDto.cs`
- [x] Create `Application/Features/Employees/DTOs/CreateEmployeeRequest.cs`
- [x] Create `Application/Features/Employees/DTOs/UpdateEmployeeRequest.cs`
- [x] Create `Application/Features/Employees/DTOs/TerminateEmployeeRequest.cs`

### 6.2 Employee Validators

- [x] Create `Application/Features/Employees/Validators/CreateEmployeeRequestValidator.cs`
- [x] Create `Application/Features/Employees/Validators/UpdateEmployeeRequestValidator.cs`
- [x] Create `Application/Features/Employees/Validators/TerminateEmployeeRequestValidator.cs`

### 6.3 Employee Mapper

- [x] Create `Application/Features/Employees/EmployeeMapper.cs`

---

## Step 7: Application Layer — CostCenter CQRS (Commands & Queries)

### 7.1 Commands

- [x] Create `CreateCostCenterCommand` + `CreateCostCenterCommandHandler`
- [x] Create `UpdateCostCenterCommand` + `UpdateCostCenterCommandHandler`
- [x] Create `DeactivateCostCenterCommand` + `DeactivateCostCenterCommandHandler`
- [x] Create `ActivateCostCenterCommand` + `ActivateCostCenterCommandHandler`

### 7.2 Queries

- [x] Create `GetCostCenterByIdQuery` + `GetCostCenterByIdQueryHandler`
- [x] Create `GetCostCentersPagedQuery` + `GetCostCentersPagedQueryHandler`

---

## Step 8: Application Layer — Employee CQRS (Commands & Queries)

### 8.1 Commands

- [x] Create `CreateEmployeeCommand` + `CreateEmployeeCommandHandler`
- [x] Create `UpdateEmployeeCommand` + `UpdateEmployeeCommandHandler`
- [x] Create `TerminateEmployeeCommand` + `TerminateEmployeeCommandHandler`
- [x] Create `ReactivateEmployeeCommand` + `ReactivateEmployeeCommandHandler`
- [x] Create `DeactivateEmployeeCommand` + `DeactivateEmployeeCommandHandler`

### 8.2 Queries

- [x] Create `GetEmployeeByIdQuery` + `GetEmployeeByIdQueryHandler`
- [x] Create `GetEmployeesPagedQuery` + `GetEmployeesPagedQueryHandler`
- [x] Create `GetEmployeesByCostCenterQuery` + `GetEmployeesByCostCenterQueryHandler`

---

## Step 9: API Layer — Controllers

### 9.1 CostCentersController

- [x] Create `API/Controllers/v1/CostCentersController.cs`:
  - `GET /api/v1/companies/{companyId}/cost-centers` — List (paged, filterable)
  - `GET /api/v1/companies/{companyId}/cost-centers/{id}` — Detail with employee count
  - `POST /api/v1/companies/{companyId}/cost-centers` — Create
  - `PUT /api/v1/companies/{companyId}/cost-centers/{id}` — Update
  - `PATCH /api/v1/companies/{companyId}/cost-centers/{id}/deactivate` — Deactivate
  - `PATCH /api/v1/companies/{companyId}/cost-centers/{id}/activate` — Activate
  - `GET /api/v1/companies/{companyId}/cost-centers/{id}/employees` — Employees in cost center

### 9.2 EmployeesController

- [x] Create `API/Controllers/v1/EmployeesController.cs`:
  - `GET /api/v1/companies/{companyId}/employees` — List (paged, filterable)
  - `GET /api/v1/companies/{companyId}/employees/{id}` — Detail with cost center
  - `POST /api/v1/companies/{companyId}/employees` — Create
  - `PUT /api/v1/companies/{companyId}/employees/{id}` — Update
  - `PATCH /api/v1/companies/{companyId}/employees/{id}/terminate` — Terminate
  - `PATCH /api/v1/companies/{companyId}/employees/{id}/reactivate` — Reactivate
  - `PATCH /api/v1/companies/{companyId}/employees/{id}/deactivate` — Deactivate
  - `PATCH /api/v1/companies/{companyId}/employees/{id}/activate` — Activate

---

## Step 10: Migration & Verification

- [x] Create EF Core migration `AddCostCentersAndEmployees`
- [x] Apply migration to database
- [x] `dotnet build` — 0 errors
- [x] Verify all 15 endpoints appear in Swagger

---

## Review Notes (auto-applied)

- Added missing `activate` endpoint on EmployeesController (entity has `Activate()` method)
- Total endpoints: 15 (7 CostCenter + 8 Employee)
- CostCenter validation on Employee create/update: handler must verify CostCenterId belongs to same CompanyId
- Employee search includes both Name and Email fields
