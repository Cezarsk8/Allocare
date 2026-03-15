# US012 – Employee Service Assignment & Cost Allocation

## Description

**As** an Admin user managing a company in Allocore,
**I need** to assign contract services (licenses/seats) to employees and calculate per-employee costs,
**So that** I can track exactly what each employee costs in terms of provider services, enabling cost allocation to cost centers and departmental budgeting.

This story builds on US010+011 (Cost Centers & Employees) and US005 (Provider Contracts with Services). It introduces the `EmployeeService` junction entity that links an Employee to a ContractService, with start/end dates and prorated cost calculation.

**Priority**: High
**Effort**: Large
**Dependencies**: US010+011 (Employees & Cost Centers), US005 (Provider Contracts)

---

## Scope

### In Scope
- EmployeeService entity (Employee ↔ ContractService assignment)
- Assignment lifecycle: assign, unassign, update dates
- Per-employee cost calculation (total service cost / assigned employees)
- Prorating based on assignment start/end dates within a billing period
- List services per employee, list employees per service
- Cost summary per employee, per cost center

### Out of Scope
- Bulk assignment (assign service to multiple employees at once) — future enhancement
- Budget limits or alerts on cost centers
- Historical cost snapshots / versioning
- Invoice generation

---

## Key Entities

### EmployeeService (new)
- EmployeeId (required FK → Employee)
- ContractServiceId (required FK → ContractService)
- StartDate (required — when the employee started using this service)
- EndDate (nullable — null means currently active)
- CompanyId (required, denormalized for tenant isolation)
- Unique constraint: (EmployeeId, ContractServiceId) — one assignment per employee per service

### Key Calculations
- **Per-seat cost**: ContractService.Price / count of active EmployeeService for that ContractService
- **Prorated cost**: Per-seat cost × (days in period / total days in billing cycle)
- **Employee total cost**: Sum of all assigned service costs for that employee
- **Cost center total**: Sum of employee total costs for all employees in the cost center

---

## API Endpoints

### EmployeeServicesController (`/api/v1/companies/{companyId}/employees/{employeeId}/services`)

| Method | Route | Action |
|--------|-------|--------|
| GET | / | List services assigned to employee (with cost) |
| POST | / | Assign service to employee |
| DELETE | /{contractServiceId} | Unassign service from employee |
| PUT | /{contractServiceId} | Update assignment dates |

### Additional endpoints on existing controllers

| Controller | Method | Route | Action |
|-----------|--------|-------|--------|
| ContractServices | GET | /{id}/employees | List employees assigned to this service |
| Employees | GET | /{id}/cost-summary | Cost breakdown for employee |
| CostCenters | GET | /{id}/cost-summary | Cost breakdown for cost center |

---

## Notes

This story will be implemented after US010+011 is complete and validated. The exact implementation details (steps, file list) will be written during the `/discuss` phase before implementation begins.
