# Allocore-frontend Development History

## v0.3.0 — Authentication Frontend (USFW003 / US002)

**Date**: 2026-03-09

**Summary**: Full authentication UI — login, register, logout, auth context, route guards, and token management. Connects to US002 JWT backend endpoints.

**Changes**:
- ✅ Created auth types matching backend DTOs (`UserDto`, `AuthResponse`, `LoginRequest`, `RegisterRequest`, `RefreshTokenRequest`)
- ✅ Created auth service with login, register, refresh, and logout API calls
- ✅ Updated apiClient with 401 response interceptor (clears tokens, redirects to `/login`)
- ✅ Created AuthContext with AuthProvider (user state, localStorage persistence, login/register/logout actions)
- ✅ Created `useAuth` hook with context validation
- ✅ Created Zod validation schemas (`loginSchema`, `registerSchema`) matching backend rules
- ✅ Created LoginForm component (React Hook Form + Zod, loading spinner, API error display)
- ✅ Created RegisterForm component (responsive 2-col name fields, password complexity validation)
- ✅ Created auth layout with centered card, app branding, authenticated redirect
- ✅ Created protected layout with auth guard (redirect to `/login` if unauthenticated)
- ✅ Wrapped app with AuthProvider in providers.tsx
- ✅ Updated root page to redirect to `/companies` (auth guard handles the rest)

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
- ✅ Created TypeScript types for Company DTOs (`src/types/company.ts`)
- ✅ Created Axios API client with token interceptor (`src/app/services/apiClient.ts`)
- ✅ Created company service with all 7 API endpoints (`src/app/services/companyService.ts`)
- ✅ Created React Query hooks for companies (`src/app/hooks/useCompanies.ts`)
- ✅ Created Zod validation schemas (`src/app/constants/companySchemas.ts`)
- ✅ Created My Companies page with list, loading, error, empty states
- ✅ Created Company Detail page with users table and management actions
- ✅ Created CompanyForm component (create + edit modes with React Hook Form + Zod)
- ✅ Created AddUserToCompanyForm component
- ✅ All forms responsive (single column mobile, inline desktop)

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
- ✅ Initialized Next.js 16 project with App Router, TypeScript, Tailwind CSS 4
- ✅ Installed all dependencies: axios, react-query, zod, react-hook-form, lucide-react, sonner
- ✅ Configured TypeScript (strict, path aliases `@/*`)
- ✅ Configured PostCSS with `@tailwindcss/postcss` (Tailwind v4)
- ✅ Created global CSS with base styles and scrollbar utilities
- ✅ Created root layout (Server Component, pt-BR, Allocare metadata)
- ✅ Created Providers wrapper (QueryClient + Toaster)
- ✅ Created root page with redirect to `/login`
- ✅ Created middleware with route matcher placeholder
- ✅ Created Atomic Design folder structure (atoms, molecules, auth, layout, etc.)
- ✅ Created barrel exports for UI components
- ✅ Created env files (.env.example, .env.local)
- ✅ Created README with project documentation

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
