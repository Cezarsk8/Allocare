# USFW003 – Authentication — Frontend

> **Project:** Allocore-frontend
> **Backend Story:** US002
> **Status:** Implemented

---

**Priority:** Critical
**Dependencies:** USFW001 — Frontend Scaffolding, USFW002 — Company Management (apiClient already exists)
**Estimated effort:** 1 session

## Description

As a **user**, I want to register, log in, and log out of Allocare so that I can securely access my companies and providers.

## UX Goals

- Clean, centered auth forms on a minimal page
- Client-side validation matching backend rules (instant feedback)
- After login, redirect to `/companies`
- After logout, redirect to `/login`
- If unauthenticated and accessing protected routes, redirect to `/login`

---

## Step 0: Responsive Baseline (MANDATORY)

- [x] Auth forms: single centered card, max-width `sm` (24rem), full width on mobile ✅ DONE
- [x] All inputs and buttons full-width inside the card ✅ DONE
- [x] Works on mobile (320px+), tablet (768px+), desktop (1024px+) ✅ DONE

## Step 1: Types

- [x] Create `src/types/auth.ts` with: ✅ DONE
  ```typescript
  export interface UserDto {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
    isEmailVerified: boolean;
    isActive: boolean;
    locale: string | null;
    createdAt: string;
  }

  export interface AuthResponse {
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
    user: UserDto;
  }

  export interface LoginRequest {
    email: string;
    password: string;
  }

  export interface RegisterRequest {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
  }

  export interface RefreshTokenRequest {
    refreshToken: string;
  }
  ```

## Step 2: Services

- [x] Create `src/app/services/authService.ts`: ✅ DONE
  - `login(data: LoginRequest): Promise<AuthResponse>` → POST `/auth/login`
  - `register(data: RegisterRequest): Promise<AuthResponse>` → POST `/auth/register`
  - `refreshToken(data: RefreshTokenRequest): Promise<AuthResponse>` → POST `/auth/refresh`
  - `logout(data: RefreshTokenRequest): Promise<void>` → POST `/auth/logout`
- [x] Update `src/app/services/apiClient.ts`: ✅ DONE
  - Add 401 response interceptor that clears tokens and redirects to `/login`

## Step 3: Auth Context & Hooks

- [x] Create `src/app/context/AuthContext.tsx`: ✅ DONE
  - `AuthProvider` wrapping children
  - State: `user`, `isAuthenticated`, `isLoading`
  - On mount: check localStorage for tokens, validate with `/auth/refresh` if expired
  - `login(email, password)` → calls authService, stores tokens + user in localStorage
  - `register(email, password, firstName, lastName)` → calls authService, stores tokens + user
  - `logout()` → calls authService, clears localStorage, redirects to `/login`
  - Token storage keys: `accessToken`, `refreshToken`, `user`
- [x] Create `src/app/hooks/useAuth.ts`: ✅ DONE
  - `useAuth()` — returns AuthContext (user, isAuthenticated, isLoading, login, register, logout)
  - Throws if used outside AuthProvider

## Step 4: Validation Schemas

- [x] Create `src/app/constants/authSchemas.ts`: ✅ DONE
  - `loginSchema`: email (required, valid email), password (required)
  - `registerSchema`: email (required, valid email, max 256), password (required, min 8, must contain uppercase + lowercase + digit + special char), firstName (required, max 100), lastName (required, max 100)

## Step 5: Components

### 5.1 Organisms

- [x] Create `src/app/components/auth/LoginForm.tsx`: ✅ DONE
  - React Hook Form + Zod (loginSchema)
  - Fields: email, password
  - Submit button with loading spinner
  - Error message display (from API)
  - Link to `/register`
  - Uses `useAuth().login`

- [x] Create `src/app/components/auth/RegisterForm.tsx`: ✅ DONE
  - React Hook Form + Zod (registerSchema)
  - Fields: firstName, lastName, email, password
  - Submit button with loading spinner
  - Error message display (from API)
  - Link to `/login`
  - Uses `useAuth().register`

## Step 6: Pages

- [x] Create `src/app/(auth)/login/page.tsx`: ✅ DONE
  - Centered layout with app title + LoginForm
  - If already authenticated, redirect to `/companies`

- [x] Create `src/app/(auth)/register/page.tsx`: ✅ DONE
  - Centered layout with app title + RegisterForm
  - If already authenticated, redirect to `/companies`

- [x] Create `src/app/(auth)/layout.tsx`: ✅ DONE
  - Simple centered layout for auth pages (flex, items-center, justify-center, min-h-screen)

## Step 7: Auth Guard & Integration

- [x] Create `src/app/(protected)/layout.tsx`: ✅ DONE
  - Checks `useAuth().isAuthenticated`
  - If loading: show spinner
  - If not authenticated: redirect to `/login`
  - If authenticated: render children

- [x] Update `src/app/providers.tsx`: ✅ DONE
  - Wrap children with `AuthProvider`

- [x] Update `src/app/page.tsx`: ✅ DONE
  - Redirect to `/companies` instead of `/login` (AuthGuard handles the rest)

## Step 8: Barrel Exports

- [x] N/A — direct imports used, no barrel exports needed ✅ DONE

---

## Technical Details

### Backend Endpoints Used

| Action | Method | Endpoint | Auth | Response |
|--------|--------|----------|------|----------|
| Register | POST | `/api/v1/auth/register` | No | `AuthResponse` |
| Login | POST | `/api/v1/auth/login` | No | `AuthResponse` |
| Refresh Token | POST | `/api/v1/auth/refresh` | No | `AuthResponse` |
| Logout | POST | `/api/v1/auth/logout` | Yes | `204 No Content` |

### Token Strategy

- Store `accessToken`, `refreshToken`, and `user` in localStorage
- apiClient attaches `Authorization: Bearer <accessToken>` on every request (already implemented)
- 401 interceptor clears storage and redirects to `/login`

### Project Structure — Affected Files

| Layer | File | Change |
|-------|------|--------|
| Types | `src/types/auth.ts` | Create |
| Service | `src/app/services/authService.ts` | Create |
| Service | `src/app/services/apiClient.ts` | Modify (add 401 interceptor) |
| Context | `src/app/context/AuthContext.tsx` | Create |
| Hook | `src/app/hooks/useAuth.ts` | Create |
| Schema | `src/app/constants/authSchemas.ts` | Create |
| Component | `src/app/components/auth/LoginForm.tsx` | Create |
| Component | `src/app/components/auth/RegisterForm.tsx` | Create |
| Page | `src/app/(auth)/login/page.tsx` | Create |
| Page | `src/app/(auth)/register/page.tsx` | Create |
| Layout | `src/app/(auth)/layout.tsx` | Create |
| Layout | `src/app/(protected)/layout.tsx` | Create |
| Provider | `src/app/providers.tsx` | Modify (add AuthProvider) |
| Page | `src/app/page.tsx` | Modify (redirect to /companies) |

---

## Acceptance Criteria

- [x] User can register with email, password, first name, last name
- [x] User can log in with email and password
- [x] User can log out
- [x] Unauthenticated users are redirected to `/login` on protected routes
- [x] Authenticated users on `/login` or `/register` are redirected to `/companies`
- [x] Form validation matches backend rules (email format, password complexity)
- [x] API errors are displayed in the form
- [x] Tokens are persisted in localStorage
- [x] 401 responses clear auth state and redirect to `/login`
- [x] `npm run type-check` passes
- [x] `npm run build` passes

---

## What is explicitly NOT changing?

- No forgot-password / reset-password UI (future story)
- No httpOnly cookie token strategy (future security hardening)
- No server-side middleware token validation (future)
- No user profile page
- Companies pages remain unchanged (they just become protected by the layout guard)
