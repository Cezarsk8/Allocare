# USFW002 – Company Management & Multi-Tenant UI — Frontend

> **Project:** Allocore-frontend
> **Backend Story:** US003
> **Status:** Pending

---

**Priority:** Critical
**Dependencies:** USFW001 — Frontend Project Scaffolding ✅
**Estimated effort:** 1 session

## Description

As an **Admin user**, I want to manage companies (create, view, update) and manage user-company associations through the frontend, so that I can set up the multi-tenant structure for Allocare.

As an **authenticated user**, I want to see which companies I belong to via a "My Companies" page so I can navigate between them.

## UX Goals

- Admin can create/edit companies via forms with validation
- Admin/Owner can add/remove users from companies
- Any authenticated user can view their companies
- Toast notifications for success/error feedback
- Responsive: forms and tables work on mobile

---

## Step 0: Responsive Baseline (MANDATORY)

- [x] ✅ DONE All forms: single column on mobile, wider on desktop
- [x] ✅ DONE Company users table: horizontal scroll on mobile
- [x] ✅ DONE Action buttons: full width on mobile, inline on desktop

## Step 1: Types

- [x] ✅ DONE Create `src/types/company.ts`:
  ```typescript
  export interface CompanyDto {
    id: string;
    name: string;
    legalName: string | null;
    taxId: string | null;
    isActive: boolean;
    createdAt: string;
    userRole: string | null;
  }

  export interface CreateCompanyRequest {
    name: string;
    legalName?: string | null;
    taxId?: string | null;
  }

  export interface UpdateCompanyRequest {
    name: string;
    legalName?: string | null;
    taxId?: string | null;
  }

  export interface AddUserToCompanyRequest {
    userId: string;
    roleInCompany: string;
  }

  export interface UserCompanyDto {
    userId: string;
    userEmail: string;
    userFullName: string;
    companyId: string;
    companyName: string;
    roleInCompany: string;
    joinedAt: string;
  }
  ```

## Step 2: API Service & Client

- [x] ✅ DONE Create `src/app/services/apiClient.ts`:
  ```typescript
  import axios from 'axios';

  const apiClient = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_BASE_URL,
    headers: { 'Content-Type': 'application/json' },
  });

  // Request interceptor: attach token from localStorage
  apiClient.interceptors.request.use((config) => {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('accessToken');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    return config;
  });

  export default apiClient;
  ```

- [x] ✅ DONE Create `src/app/services/companyService.ts`:
  ```typescript
  import apiClient from './apiClient';
  import type { CompanyDto, CreateCompanyRequest, UpdateCompanyRequest, AddUserToCompanyRequest, UserCompanyDto } from '@/types/company';

  export const companyService = {
    getMyCompanies: () =>
      apiClient.get<CompanyDto[]>('/my/companies').then(r => r.data),

    getCompanyById: (id: string) =>
      apiClient.get<CompanyDto>(`/companies/${id}`).then(r => r.data),

    createCompany: (data: CreateCompanyRequest) =>
      apiClient.post<CompanyDto>('/companies', data).then(r => r.data),

    updateCompany: (id: string, data: UpdateCompanyRequest) =>
      apiClient.put<CompanyDto>(`/companies/${id}`, data).then(r => r.data),

    getCompanyUsers: (companyId: string) =>
      apiClient.get<UserCompanyDto[]>(`/companies/${companyId}/users`).then(r => r.data),

    addUserToCompany: (companyId: string, data: AddUserToCompanyRequest) =>
      apiClient.post<UserCompanyDto>(`/companies/${companyId}/users`, data).then(r => r.data),

    removeUserFromCompany: (companyId: string, userId: string) =>
      apiClient.delete(`/companies/${companyId}/users/${userId}`),
  };
  ```

## Step 3: Hooks

- [x] ✅ DONE Create `src/app/hooks/useCompanies.ts`:
  ```typescript
  import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
  import { companyService } from '@/app/services/companyService';
  import type { CreateCompanyRequest, UpdateCompanyRequest, AddUserToCompanyRequest } from '@/types/company';

  export function useMyCompanies() {
    return useQuery({
      queryKey: ['my-companies'],
      queryFn: companyService.getMyCompanies,
    });
  }

  export function useCompany(id: string) {
    return useQuery({
      queryKey: ['company', id],
      queryFn: () => companyService.getCompanyById(id),
      enabled: !!id,
    });
  }

  export function useCompanyUsers(companyId: string) {
    return useQuery({
      queryKey: ['company-users', companyId],
      queryFn: () => companyService.getCompanyUsers(companyId),
      enabled: !!companyId,
    });
  }

  export function useCreateCompany() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (data: CreateCompanyRequest) => companyService.createCompany(data),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['my-companies'] }),
    });
  }

  export function useUpdateCompany() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ id, data }: { id: string; data: UpdateCompanyRequest }) =>
        companyService.updateCompany(id, data),
      onSuccess: (_, { id }) => {
        queryClient.invalidateQueries({ queryKey: ['my-companies'] });
        queryClient.invalidateQueries({ queryKey: ['company', id] });
      },
    });
  }

  export function useAddUserToCompany() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ companyId, data }: { companyId: string; data: AddUserToCompanyRequest }) =>
        companyService.addUserToCompany(companyId, data),
      onSuccess: (_, { companyId }) =>
        queryClient.invalidateQueries({ queryKey: ['company-users', companyId] }),
    });
  }

  export function useRemoveUserFromCompany() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ companyId, userId }: { companyId: string; userId: string }) =>
        companyService.removeUserFromCompany(companyId, userId),
      onSuccess: (_, { companyId }) =>
        queryClient.invalidateQueries({ queryKey: ['company-users', companyId] }),
    });
  }
  ```

## Step 4: Zod Schemas

- [x] ✅ DONE Create `src/app/constants/companySchemas.ts`:
  ```typescript
  import { z } from 'zod/v4';

  export const createCompanySchema = z.object({
    name: z.string().min(1, 'Nome da empresa é obrigatório').max(200, 'Máximo 200 caracteres'),
    legalName: z.string().max(300, 'Máximo 300 caracteres').optional().or(z.literal('')),
    taxId: z.string().max(50, 'Máximo 50 caracteres').optional().or(z.literal('')),
  });

  export const updateCompanySchema = createCompanySchema;

  export const addUserToCompanySchema = z.object({
    userId: z.string().min(1, 'ID do usuário é obrigatório'),
    roleInCompany: z.enum(['Viewer', 'Manager', 'Owner'], { message: 'Selecione um papel válido' }),
  });

  export type CreateCompanyFormData = z.infer<typeof createCompanySchema>;
  export type UpdateCompanyFormData = z.infer<typeof updateCompanySchema>;
  export type AddUserToCompanyFormData = z.infer<typeof addUserToCompanySchema>;
  ```

## Step 5: Page — My Companies

- [x] ✅ DONE Create `src/app/(protected)/companies/page.tsx`:
  - List current user's companies using `useMyCompanies()`
  - Show company name, role, status
  - "Nova Empresa" button (for Admin users)
  - Links to company detail page
  - Empty state: "Você não está vinculado a nenhuma empresa"
  - Loading state with spinner
  - Error state with retry

## Step 6: Page — Company Detail

- [x] ✅ DONE Create `src/app/(protected)/companies/[id]/page.tsx`:
  - Show company details (name, legal name, tax ID, status)
  - Edit button (Owner/Admin)
  - Tab/section: Company Users list
  - "Adicionar Usuário" button (Owner/Admin)
  - Remove user button with confirmation
  - Uses `useCompany()` and `useCompanyUsers()`

## Step 7: Company Form Component

- [x] ✅ DONE Create `src/app/components/companies/CompanyForm.tsx`:
  - React Hook Form + Zod validation
  - Fields: name (required), legalName (optional), taxId (optional)
  - Mode: create or edit (pre-filled)
  - Submit handler calls `useCreateCompany()` or `useUpdateCompany()`
  - Toast on success/error
  - Cancel button

## Step 8: Add User to Company Component

- [x] ✅ DONE Create `src/app/components/companies/AddUserToCompanyForm.tsx`:
  - Fields: userId (text input), roleInCompany (select: Viewer/Manager/Owner)
  - Zod validation
  - Submit calls `useAddUserToCompany()`
  - Toast feedback

## Step 9: Barrel Exports & Cleanup

- [x] ✅ DONE Update barrel exports as needed
- [x] ✅ DONE Verify all imports use `@/*` alias

---

## Technical Details

### Backend Endpoints Used

| Action | Method | Endpoint | Auth | Response |
|--------|--------|----------|------|----------|
| List my companies | `GET` | `/my/companies` | Auth | `CompanyDto[]` |
| Get company | `GET` | `/companies/{id}` | Auth | `CompanyDto` |
| Create company | `POST` | `/companies` | Admin | `CompanyDto` |
| Update company | `PUT` | `/companies/{id}` | Owner/Admin | `CompanyDto` |
| List company users | `GET` | `/companies/{id}/users` | Auth | `UserCompanyDto[]` |
| Add user to company | `POST` | `/companies/{id}/users` | Owner/Admin | `UserCompanyDto` |
| Remove user | `DELETE` | `/companies/{id}/users/{userId}` | Owner/Admin | 204 |

### Project Structure — Affected Files

| Layer | File | Change |
|-------|------|--------|
| Types | `src/types/company.ts` | Create |
| Service | `src/app/services/apiClient.ts` | Create |
| Service | `src/app/services/companyService.ts` | Create |
| Hook | `src/app/hooks/useCompanies.ts` | Create |
| Constants | `src/app/constants/companySchemas.ts` | Create |
| Page | `src/app/(protected)/companies/page.tsx` | Create |
| Page | `src/app/(protected)/companies/[id]/page.tsx` | Create |
| Component | `src/app/components/companies/CompanyForm.tsx` | Create |
| Component | `src/app/components/companies/AddUserToCompanyForm.tsx` | Create |

---

## Acceptance Criteria

- [x] ✅ DONE My Companies page lists the current user's companies
- [x] ✅ DONE Admin can create a company via form with validation
- [x] ✅ DONE Owner/Admin can edit a company
- [x] ✅ DONE Company detail page shows company info and linked users
- [x] ✅ DONE Owner/Admin can add users to a company
- [x] ✅ DONE Owner/Admin can remove users from a company
- [x] ✅ DONE Toast notifications for all success/error operations
- [x] ✅ DONE `npm run type-check` passes
- [x] ✅ DONE `npm run build` passes

---

## What is explicitly NOT changing?

- No AuthContext/login flow (USFW004 — not yet implemented)
- No sidebar/layout (USFW005)
- No dark mode
- Token handling is basic (localStorage) — will be improved in auth story
