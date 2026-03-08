# Allocore-frontend Development History

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
