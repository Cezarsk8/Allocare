# USFW004 – Cost Centers & Employees — Frontend

> **Project:** Allocore-frontend
> **Backend Story:** US010+011
> **Status:** Pending

**Priority:** High
**Dependencies:** USFW002 — Company Management Frontend
**Estimated effort:** 2 sessions

## Description

As an **Admin user** managing a company in Allocare,
I want to view, create, edit, activate/deactivate Cost Centers and Employees through the web interface,
so that I can organize my workforce by department and prepare for cost allocation.

## Step 0: Responsive Baseline (MANDATORY)

- [ ] All tables use `overflow-x-auto` for mobile scrolling
- [ ] Forms use `flex flex-col gap-4` with `sm:flex-row` where appropriate
- [ ] List pages use single-column on mobile, expanding on `sm:` breakpoint
- [ ] Buttons stack vertically on mobile, align horizontally on `sm:`
- [ ] Company sub-navigation uses horizontal scroll on mobile

## Step 1: Types

- [ ] Create `src/types/costCenter.ts`:
  ```typescript
  export interface CostCenterDto {
    id: string;
    companyId: string;
    code: string;
    name: string;
    description: string | null;
    isActive: boolean;
    employeeCount: number;
    createdAt: string;
    updatedAt: string | null;
  }

  export interface CostCenterListItemDto {
    id: string;
    code: string;
    name: string;
    isActive: boolean;
    employeeCount: number;
  }

  export interface CreateCostCenterRequest {
    code: string;
    name: string;
    description?: string | null;
  }

  export interface UpdateCostCenterRequest {
    code: string;
    name: string;
    description?: string | null;
  }

  export interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
  }
  ```

- [ ] Create `src/types/employee.ts`:
  ```typescript
  export interface EmployeeDto {
    id: string;
    companyId: string;
    name: string;
    email: string;
    costCenterId: string | null;
    costCenterName: string | null;
    costCenterCode: string | null;
    jobTitle: string | null;
    hireDate: string | null;
    terminationDate: string | null;
    isActive: boolean;
    createdAt: string;
    updatedAt: string | null;
  }

  export interface EmployeeListItemDto {
    id: string;
    name: string;
    email: string;
    costCenterName: string | null;
    jobTitle: string | null;
    isActive: boolean;
  }

  export interface CreateEmployeeRequest {
    name: string;
    email: string;
    costCenterId?: string | null;
    jobTitle?: string | null;
    hireDate?: string | null;
  }

  export interface UpdateEmployeeRequest {
    name: string;
    email: string;
    costCenterId?: string | null;
    jobTitle?: string | null;
    hireDate?: string | null;
  }

  export interface TerminateEmployeeRequest {
    terminationDate: string;
  }
  ```

## Step 2: Services

- [ ] Create `src/app/services/costCenterService.ts`:
  - `getByCompany(companyId, params)` → GET `/companies/{companyId}/cost-centers` (paginated, filterable)
  - `getById(companyId, id)` → GET `/companies/{companyId}/cost-centers/{id}`
  - `create(companyId, data)` → POST `/companies/{companyId}/cost-centers`
  - `update(companyId, id, data)` → PUT `/companies/{companyId}/cost-centers/{id}`
  - `deactivate(companyId, id)` → PATCH `/companies/{companyId}/cost-centers/{id}/deactivate`
  - `activate(companyId, id)` → PATCH `/companies/{companyId}/cost-centers/{id}/activate`
  - `getEmployees(companyId, id, params)` → GET `/companies/{companyId}/cost-centers/{id}/employees`

- [ ] Create `src/app/services/employeeService.ts`:
  - `getByCompany(companyId, params)` → GET `/companies/{companyId}/employees` (paginated, filterable)
  - `getById(companyId, id)` → GET `/companies/{companyId}/employees/{id}`
  - `create(companyId, data)` → POST `/companies/{companyId}/employees`
  - `update(companyId, id, data)` → PUT `/companies/{companyId}/employees/{id}`
  - `terminate(companyId, id, data)` → PATCH `/companies/{companyId}/employees/{id}/terminate`
  - `reactivate(companyId, id)` → PATCH `/companies/{companyId}/employees/{id}/reactivate`
  - `deactivate(companyId, id)` → PATCH `/companies/{companyId}/employees/{id}/deactivate`
  - `activate(companyId, id)` → PATCH `/companies/{companyId}/employees/{id}/activate`

## Step 3: Hooks

- [ ] Create `src/app/hooks/useCostCenters.ts`:
  - `useCostCenters(companyId, params)` — paginated query with search/filter
  - `useCostCenter(companyId, id)` — single item query
  - `useCreateCostCenter()` — mutation, invalidates list
  - `useUpdateCostCenter()` — mutation, invalidates list + detail
  - `useDeactivateCostCenter()` — mutation, invalidates list + detail
  - `useActivateCostCenter()` — mutation, invalidates list + detail

- [ ] Create `src/app/hooks/useEmployees.ts`:
  - `useEmployees(companyId, params)` — paginated query with search/filter
  - `useEmployee(companyId, id)` — single item query
  - `useCreateEmployee()` — mutation, invalidates list
  - `useUpdateEmployee()` — mutation, invalidates list + detail
  - `useTerminateEmployee()` — mutation, invalidates list + detail
  - `useReactivateEmployee()` — mutation, invalidates list + detail
  - `useDeactivateEmployee()` — mutation, invalidates list + detail
  - `useActivateEmployee()` — mutation, invalidates list + detail

## Step 4: Validation Schemas

- [ ] Create `src/app/constants/costCenterSchemas.ts`:
  - `createCostCenterSchema`: code (required, max 50), name (required, max 200), description (optional, max 2000)
  - `updateCostCenterSchema`: same as create

- [ ] Create `src/app/constants/employeeSchemas.ts`:
  - `createEmployeeSchema`: name (required, max 200), email (required, valid email, max 300), costCenterId (optional), jobTitle (optional, max 200), hireDate (optional)
  - `updateEmployeeSchema`: same as create
  - `terminateEmployeeSchema`: terminationDate (required)

## Step 5: Components

### 5.1 Pagination Molecule
- [ ] Create `src/app/components/ui/molecules/Pagination.tsx`:
  - Props: `page`, `totalPages`, `onPageChange`
  - Previous/Next buttons + page info
  - Disabled states at boundaries
  - Reusable across all paginated tables

### 5.2 Company Sub-Navigation
- [ ] Create `src/app/components/companies/CompanyNav.tsx`:
  - Tab-style navigation: Geral | Centros de Custo | Colaboradores
  - Active tab highlight based on current route
  - Links to: `/companies/{id}`, `/companies/{id}/cost-centers`, `/companies/{id}/employees`

### 5.3 Cost Center Components
- [ ] Create `src/app/components/cost-centers/CostCenterForm.tsx`:
  - Props: `companyId`, `costCenterId?`, `defaultValues?`, `onSuccess?`, `onCancel?`
  - Fields: Code, Name, Description (textarea)
  - React Hook Form + Zod validation
  - Create/Edit mode based on `costCenterId`

- [ ] Create `src/app/components/cost-centers/CostCenterTable.tsx`:
  - Props: `costCenters`, `onToggleStatus`
  - Columns: Code, Name, Employees (count), Status (badge), Actions
  - Row click → navigate to detail/edit inline
  - Activate/Deactivate action button

### 5.4 Employee Components
- [ ] Create `src/app/components/employees/EmployeeForm.tsx`:
  - Props: `companyId`, `employeeId?`, `defaultValues?`, `onSuccess?`, `onCancel?`
  - Fields: Name, Email, Job Title, Cost Center (dropdown), Hire Date (date input)
  - Cost Center dropdown populated from `useCostCenters` (active only)
  - React Hook Form + Zod validation

- [ ] Create `src/app/components/employees/EmployeeTable.tsx`:
  - Props: `employees`, `onToggleStatus`, `onTerminate`
  - Columns: Name, Email, Cost Center, Job Title, Status, Actions
  - Actions: Activate/Deactivate, Terminate/Reactivate

- [ ] Create `src/app/components/employees/TerminateEmployeeDialog.tsx`:
  - Props: `companyId`, `employeeId`, `employeeName`, `onSuccess`, `onCancel`
  - Inline form with termination date picker
  - Confirmation message

## Step 6: Pages & Layout

- [ ] Create `src/app/(protected)/companies/[id]/layout.tsx`:
  - Fetches company data
  - Shows company header (name, status, role)
  - Renders `CompanyNav` tabs
  - Wraps `{children}`

- [ ] Refactor `src/app/(protected)/companies/[id]/page.tsx`:
  - Remove company header (moved to layout)
  - Keep: Edit form, Users section
  - Becomes the "Geral" tab content

- [ ] Create `src/app/(protected)/companies/[id]/cost-centers/page.tsx`:
  - Search input + active/inactive filter
  - Inline create form (toggle)
  - Paginated table
  - Inline edit form (toggle per row or detail section)
  - Loading / Empty / Error states

- [ ] Create `src/app/(protected)/companies/[id]/employees/page.tsx`:
  - Search input + cost center filter dropdown + active/inactive filter
  - Inline create form (toggle)
  - Paginated table
  - Inline edit form
  - Terminate/Reactivate actions
  - Loading / Empty / Error states

## Step 7: Barrel Exports

- [ ] Update `src/app/components/ui/index.ts` — add Pagination export
- [ ] Create barrel exports if needed for new component directories

## Acceptance Criteria

- [ ] `npm run type-check` passes
- [ ] `npm run build` passes
- [ ] All 7 CostCenter API endpoints integrated
- [ ] All 8 Employee API endpoints integrated
- [ ] Pagination works on both list pages
- [ ] Search/filter works on both list pages
- [ ] Forms validate before submit (Zod)
- [ ] Toast notifications on success/error
- [ ] Responsive on mobile (tables scroll, forms stack)
- [ ] Company navigation shows active tab
- [ ] Cost center dropdown on Employee form loads from API
