# USFW001 – Frontend Project Scaffolding & Foundation

> **Project:** Allocore-frontend (Next.js 15 + Atomic Design)
> **Phase:** 1 — Foundation
> **Status:** Pending

---

**Priority:** Critical
**Dependencies:** None (backend US001 + US002 already completed, but not required for scaffolding)
**Estimated effort:** 1 session

## Description

As a **developer**, I want the Allocore-frontend Next.js project fully bootstrapped with Atomic Design folder structure, Tailwind CSS v4, all core dependencies, and project configuration so that subsequent feature stories (USFW002+) can begin immediately without setup overhead.

Currently, the `Allocore-frontend` directory contains only empty placeholder directories (`docs/`, `src/app/`, `src/components/`, etc.) and a `.gitkeep` file. There is no `package.json`, no configuration files, no source code. This story creates the complete project skeleton from scratch, following the proven patterns from the OvertimeWeb reference project and adapting them for the Allocare domain.

## UX Goals

- N/A — this is a scaffolding story with no user-facing UI beyond a redirect page
- Establish the design foundation (Tailwind CSS v4, base styles) for all future stories
- Ensure `npm run dev` starts cleanly with no errors

---

## Step 0: Responsive Baseline (MANDATORY)

- [x] ✅ DONE N/A for this story — no user-facing pages are created
- [x] ✅ DONE **Note:** The `globals.css` base styles and Tailwind configuration established here will be the responsive foundation for all future stories

## Step 1: Project Initialization & Dependencies

### 1.1 Initialize Next.js 15 project

- [x] ✅ DONE Run `npx create-next-app@latest . --typescript --tailwind --eslint --app --src-dir --no-git --import-alias "@/*"` in `C:\Users\cezar\Projects\Allocare\Allocore-frontend`
  - **Note:** The `--src-dir` flag creates the `src/` directory. The `--app` flag uses App Router.
  - **Note:** `--no-git` prevents reinitializing git since `docs/` and `.gitkeep` already exist.
  - If `create-next-app` scaffolds default files (e.g., `src/app/page.tsx`, `src/app/layout.tsx`, `src/app/globals.css`), they will be **overwritten** in subsequent steps.

### 1.2 Install production dependencies

- [x] ✅ DONE Install all required packages:
  ```bash
  npm install axios @tanstack/react-query@^5 lucide-react sonner zod react-hook-form @hookform/resolvers
  ```
  - **Note:** `next`, `react`, `react-dom`, `typescript`, and `tailwindcss` are already installed by `create-next-app`.
  - **Note:** `react-hook-form` + `@hookform/resolvers` are included for form handling with Zod validation (used in USFW002+).

### 1.3 Install dev dependencies

- [x] ✅ DONE Install dev tooling:
  ```bash
  npm install -D @tailwindcss/forms @tailwindcss/postcss @types/node @types/react @types/react-dom autoprefixer postcss eslint-config-next
  ```
  - **Note:** Some of these may already be present from `create-next-app`. The command is idempotent.

### 1.4 Verify `package.json`

- [x] ✅ DONE File: `package.json`
- [x] ✅ DONE Confirm `name` is `"allocore-frontend"` and `"private": true`
- [x] ✅ DONE Confirm the following scripts exist (add if missing):
  ```json
  {
    "name": "allocore-frontend",
    "private": true,
    "scripts": {
      "dev": "next dev",
      "build": "next build",
      "start": "next start",
      "lint": "next lint",
      "type-check": "tsc --noEmit"
    }
  }
  ```
- [x] ✅ DONE Confirm `engines` field:
  ```json
  {
    "engines": {
      "node": ">=18.0.0",
      "npm": ">=9.0.0"
    }
  }
  ```

### Expected `package.json` dependencies (reference)

| Package | Version | Purpose |
|---------|---------|---------|
| `next` | `^15.0.0` | Framework |
| `react` | `^18.3.0` or `^19.0.0` | UI library |
| `react-dom` | `^18.3.0` or `^19.0.0` | React DOM |
| `typescript` | `^5.3.0` | Type system |
| `tailwindcss` | `^4.0.0` | Styling |
| `axios` | `^1.6.0` | HTTP client |
| `@tanstack/react-query` | `^5.0.0` | Server state management |
| `lucide-react` | `^0.300.0` | Icons |
| `sonner` | `^2.0.0` | Toast notifications |
| `zod` | `^3.22.0` | Validation |
| `react-hook-form` | `^7.50.0` | Form handling |
| `@hookform/resolvers` | `^3.3.0` | RHF + Zod integration |

---

## Step 2: TypeScript Configuration

- [x] ✅ DONE File: `tsconfig.json`
- [x] ✅ DONE Ensure the following configuration (overwrite if `create-next-app` generated a different one):

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "lib": ["dom", "dom.iterable", "esnext"],
    "allowJs": true,
    "skipLibCheck": true,
    "strict": true,
    "noEmit": true,
    "esModuleInterop": true,
    "module": "esnext",
    "moduleResolution": "bundler",
    "resolveJsonModule": true,
    "isolatedModules": true,
    "jsx": "preserve",
    "incremental": true,
    "forceConsistentCasingInFileNames": true,
    "plugins": [
      {
        "name": "next"
      }
    ],
    "paths": {
      "@/*": ["./src/*"]
    }
  },
  "include": ["next-env.d.ts", "**/*.ts", "**/*.tsx", ".next/types/**/*.ts"],
  "exclude": ["node_modules"]
}
```

- **Key:** The `@/*` → `./src/*` path alias enables clean imports like `import { Button } from '@/app/components/ui/atoms'` across the entire project.

---

## Step 3: Tailwind CSS v4 Configuration

### 3.1 PostCSS configuration

- [x] ✅ DONE File: `postcss.config.mjs`
- [x] ✅ DONE Content:
  ```js
  const config = {
    plugins: {
      "@tailwindcss/postcss": {},
    },
  };
  export default config;
  ```
  - **Note:** Tailwind CSS v4 uses `@tailwindcss/postcss` instead of the legacy `tailwindcss` PostCSS plugin. There is **no `tailwind.config.js`** in v4 — configuration is done via CSS.

### 3.2 Global CSS

- [x] ✅ DONE File: `src/app/globals.css`
- [x] ✅ DONE Content:
  ```css
  @import "tailwindcss";

  @layer base {
    html {
      font-feature-settings: normal;
      font-variation-settings: normal;
      -webkit-font-smoothing: antialiased;
      -moz-osx-font-smoothing: grayscale;
    }

    body {
      background-color: rgb(249 250 251);
      color: rgb(17 24 39);
    }
  }

  @layer utilities {
    /* Custom scrollbar */
    .scrollbar-thin::-webkit-scrollbar {
      width: 8px;
      height: 8px;
    }

    .scrollbar-thin::-webkit-scrollbar-track {
      background-color: rgb(243 244 246);
    }

    .scrollbar-thin::-webkit-scrollbar-thumb {
      background-color: rgb(209 213 219);
      border-radius: 9999px;
    }

    .scrollbar-thin::-webkit-scrollbar-thumb:hover {
      background-color: rgb(156 163 175);
    }
  }
  ```
  - **Note:** Tailwind v4 uses `@import "tailwindcss"` instead of `@tailwind base/components/utilities` directives.
  - **Note:** Tailwind v4 includes `sr-only` as a built-in utility — no custom definition needed.
  - **Note:** Dark mode CSS variables will be added in a future polish story. For now, the base light theme is sufficient.

---

## Step 4: Next.js Configuration

### 4.1 Next.js config

- [x] ✅ DONE File: `next.config.ts`
- [x] ✅ DONE Content:
  ```typescript
  import type { NextConfig } from 'next';

  const nextConfig: NextConfig = {
    // API calls go directly to the backend via Axios (no Next.js rewrites needed)
    // The backend URL is configured via NEXT_PUBLIC_API_BASE_URL env var
    // CORS is handled by the backend for development
  };

  export default nextConfig;
  ```
  - **Note:** No API rewrites are needed. The frontend calls the Allocore backend directly via Axios using the env var.

### 4.2 ESLint configuration

- [x] ✅ DONE File: `.eslintrc.json`
- [x] ✅ DONE Content (use whatever `create-next-app` generates, ensure it includes):
  ```json
  {
    "extends": "next/core-web-vitals"
  }
  ```

---

## Step 5: Environment Configuration

### 5.1 Environment example file

- [x] ✅ DONE File: `.env.example`
- [x] ✅ DONE Content:
  ```env
  # Allocore Backend API
  NEXT_PUBLIC_API_BASE_URL=http://localhost:5103/api/v1
  ```

### 5.2 Local environment file

- [x] ✅ DONE File: `.env.local`
- [x] ✅ DONE Content:
  ```env
  NEXT_PUBLIC_API_BASE_URL=http://localhost:5103/api/v1
  ```
  - **Note:** `.env.local` is gitignored by default. The `NEXT_PUBLIC_` prefix makes this variable available in the browser.
  - **Business rule:** The backend URL `http://localhost:5103/api/v1` matches the Allocore API's `launchSettings.json` configuration.

### 5.3 Verify `.gitignore`

- [x] ✅ DONE File: `.gitignore`
- [x] ✅ DONE Ensure the following entries exist (most are included by `create-next-app`):
  ```
  # dependencies
  /node_modules
  /.pnp
  .pnp.js

  # testing
  /coverage

  # next.js
  /.next/
  /out/

  # production
  /build

  # misc
  .DS_Store
  *.pem

  # debug
  npm-debug.log*

  # local env files
  .env*.local

  # vercel
  .vercel

  # typescript
  *.tsbuildinfo
  next-env.d.ts
  ```

---

## Step 6: Folder Structure Creation

Create the following directory structure. All directories should contain at minimum a `.gitkeep` file or an `index.ts` barrel export (as specified) so they are tracked by git.

### 6.1 Route groups

- [x] ✅ DONE `src/app/(auth)/` — Create directory (empty, placeholder for USFW004)
- [x] ✅ DONE `src/app/(protected)/` — Create directory (empty, placeholder for USFW005)

### 6.2 Component directories

- [x] ✅ DONE `src/app/components/ui/atoms/index.ts` — Empty barrel export:
  ```typescript
  // Atom components — re-export all atoms from here
  // Added in USFW004+
  ```
- [x] ✅ DONE `src/app/components/ui/molecules/index.ts` — Empty barrel export:
  ```typescript
  // Molecule components — re-export all molecules from here
  // Added in USFW005+
  ```
- [x] ✅ DONE `src/app/components/ui/index.ts` — Barrel export aggregating atoms + molecules:
  ```typescript
  // UI component barrel export
  export * from './atoms';
  export * from './molecules';
  ```
- [x] ✅ DONE `src/app/components/auth/` — Create directory (empty, placeholder for USFW004)
- [x] ✅ DONE `src/app/components/layout/` — Create directory (empty, placeholder for USFW005)
- [x] ✅ DONE `src/app/components/providers/` — Create directory (empty, placeholder for USFW006 — Provider Management UI)
- [x] ✅ DONE `src/app/components/contracts/` — Create directory (empty, placeholder for USFW007 — Contract Management UI)
- [x] ✅ DONE `src/app/components/dashboard/` — Create directory (empty, placeholder for USFW008 — Dashboard UI)
- [x] ✅ DONE `src/app/components/settings/` — Create directory (empty, placeholder for USFW009 — Settings UI)

### 6.3 Application directories

- [x] ✅ DONE `src/app/config/` — Create directory (empty, placeholder for app configuration)
- [x] ✅ DONE `src/app/constants/` — Create directory (empty, placeholder for USFW002)
- [x] ✅ DONE `src/app/context/` — Create directory (empty, placeholder for USFW004)
- [x] ✅ DONE `src/app/hooks/` — Create directory (empty, placeholder for USFW004+)
- [x] ✅ DONE `src/app/services/` — Create directory (empty, placeholder for USFW003)
- [x] ✅ DONE `src/app/utils/` — Create directory (empty, placeholder for USFW003)

### 6.4 Types directory

- [x] ✅ DONE `src/types/` — Create directory (empty, placeholder for USFW002)

### **Rule:** Empty directories that have no `index.ts` should contain a `.gitkeep` file to ensure git tracks them.

---

## Step 7: Root Application Files

### 7.1 Root layout

- [x] ✅ DONE File: `src/app/layout.tsx`
- [x] ✅ DONE **Type:** Server Component (no `'use client'`)
- [x] ✅ DONE Content pattern:
  ```typescript
  import type { Metadata, Viewport } from 'next';
  import { Providers } from './providers';
  import './globals.css';

  export const metadata: Metadata = {
    title: 'Allocare — Gestão de Provedores & Controle de Custos',
    description: 'Plataforma centralizada para gestão de provedores, contratos, serviços e controle de custos corporativos.',
    keywords: ['gestão de provedores', 'controle de custos', 'contratos', 'procurement', 'cost allocation'],
    authors: [{ name: 'Allocare Team' }],
  };

  export const viewport: Viewport = {
    width: 'device-width',
    initialScale: 1,
    themeColor: '#2563eb',
  };

  export default function RootLayout({
    children,
  }: {
    children: React.ReactNode;
  }) {
    return (
      <html lang="pt-BR">
        <body>
          <Providers>{children}</Providers>
        </body>
      </html>
    );
  }
  ```
  - **Note:** `lang="pt-BR"` — Allocare's UI is in Portuguese.
  - **Note:** Imports `Providers` from `./providers` and `globals.css` for Tailwind.

### 7.2 Providers wrapper

- [x] ✅ DONE File: `src/app/providers.tsx`
- [x] ✅ DONE **Type:** Client Component (`'use client'`)
- [x] ✅ DONE Content pattern:
  ```typescript
  'use client';

  import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
  import { useState } from 'react';
  import { Toaster } from 'sonner';

  export function Providers({ children }: { children: React.ReactNode }) {
    const [queryClient] = useState(
      () =>
        new QueryClient({
          defaultOptions: {
            queries: {
              staleTime: 60 * 1000, // 1 minute
              refetchOnWindowFocus: false,
            },
          },
        })
    );

    return (
      <QueryClientProvider client={queryClient}>
        {children}
        <Toaster position="top-right" duration={5000} />
      </QueryClientProvider>
    );
  }
  ```
  - **Note:** `AuthProvider` is NOT included yet — it will be added in USFW004 when `AuthContext` is created. For now, only `QueryClientProvider` and `Toaster` are wrapped.
  - **Note:** `staleTime: 60_000` and `refetchOnWindowFocus: false` match Allocare project conventions.

### 7.3 Root page (redirect)

- [x] ✅ DONE File: `src/app/page.tsx`
- [x] ✅ DONE **Type:** Server Component (no `'use client'`)
- [x] ✅ DONE Content pattern:
  ```typescript
  import { redirect } from 'next/navigation';

  export default function HomePage() {
    redirect('/login');
  }
  ```
  - **Note:** Simple server-side redirect. Once USFW004 (auth) is implemented, this can be enhanced to check auth status and redirect to `/dashboard` if authenticated.

### 7.4 Middleware (placeholder)

- [x] ✅ DONE File: `src/middleware.ts`
- [x] ✅ DONE Content pattern:
  ```typescript
  import { NextResponse } from 'next/server';
  import type { NextRequest } from 'next/server';

  const protectedRoutes = ['/dashboard', '/providers', '/contracts', '/settings', '/profile'];
  const authRoutes = ['/login', '/register', '/forgot-password', '/reset-password'];

  export function middleware(request: NextRequest) {
    const { pathname } = request.nextUrl;

    // Auth routes — allow through (client-side auth guard will handle redirect if already logged in)
    if (authRoutes.some((route) => pathname.startsWith(route))) {
      return NextResponse.next();
    }

    // Protected routes — allow through (ClientAuthGuard added in USFW004 will handle client-side)
    // TODO (USFW004): Add server-side token validation when using httpOnly cookies
    if (protectedRoutes.some((route) => pathname.startsWith(route))) {
      return NextResponse.next();
    }

    return NextResponse.next();
  }

  export const config = {
    matcher: [
      '/((?!api|_next/static|_next/image|favicon.ico).*)',
    ],
  };
  ```
  - **Note:** This is a passthrough placeholder. Actual route protection logic will be implemented in USFW004. The route lists are pre-populated based on the planned navigation structure from the Allocare domain.
  - **Note:** Auth routes match backend `AuthController` endpoints: login, register, forgot-password, reset-password.

---

## Step 8: README

- [x] ✅ DONE File: `README.md` (project root of Allocore-frontend)
- [x] ✅ DONE Content should include:
  - **Project name:** Allocore-frontend
  - **Description:** Frontend para a plataforma Allocare de gestão de provedores e controle de custos
  - **Tech stack table** (Next.js 15, TypeScript, Tailwind CSS 4, React Query v5, Axios, Zod, React Hook Form, Lucide, Sonner)
  - **Prerequisites:** Node.js ≥ 18, npm ≥ 9, Allocore backend running at `localhost:5103`
  - **Setup instructions:**
    1. `npm install`
    2. Copy `.env.example` to `.env.local`
    3. `npm run dev`
    4. Open `http://localhost:3000`
  - **Available scripts:** `dev`, `build`, `start`, `lint`, `type-check`
  - **Project structure** overview (abbreviated folder tree)
  - **Backend dependency:** Allocore backend must be running on port 5103
  - **Atomic Design rules** (brief summary: atoms → molecules → organisms → pages)

---

## Step 9: Verification

- [x] ✅ DONE Run `npm run dev` — confirm the app starts on `http://localhost:3000` with no errors
- [x] ✅ DONE Run `npm run type-check` — confirm zero TypeScript errors
- [x] ✅ DONE Run `npm run lint` — confirm zero ESLint errors
- [x] ✅ DONE Run `npm run build` — confirm production build succeeds
- [x] ✅ DONE Navigate to `http://localhost:3000` — confirm redirect to `/login` (404 page is expected since login page doesn't exist yet)
- [x] ✅ DONE Verify all directories exist in the expected structure
- [x] ✅ DONE Verify `.env.local` is NOT committed (check `.gitignore`)

---

## Technical Details

### Backend Endpoints Used

| Action | Method | Endpoint | Auth | Notes |
|--------|--------|----------|------|-------|
| Ping / Health check | `GET` | `/api/v1/ping` | None | Returns `PingResponse`. Not consumed in this story but available for USFW002+ (Dashboard / health check). |

### Project Structure — Affected Files

| Layer | File | Change |
|-------|------|--------|
| **Config** | `package.json` | **Create** — all dependencies and scripts |
| **Config** | `tsconfig.json` | **Create** — strict mode, path aliases |
| **Config** | `postcss.config.mjs` | **Create** — Tailwind v4 PostCSS plugin |
| **Config** | `next.config.ts` | **Create** — minimal Next.js config |
| **Config** | `.eslintrc.json` | **Create** — Next.js ESLint config |
| **Config** | `.env.example` | **Create** — `NEXT_PUBLIC_API_BASE_URL` |
| **Config** | `.env.local` | **Create** — local env (gitignored) |
| **Config** | `.gitignore` | **Create** — standard Next.js ignores |
| **CSS** | `src/app/globals.css` | **Create** — Tailwind v4 import, base styles, scrollbar utilities |
| **Layout** | `src/app/layout.tsx` | **Create** — root layout (Server Component) |
| **Provider** | `src/app/providers.tsx` | **Create** — QueryClient + Toaster (Client Component) |
| **Page** | `src/app/page.tsx` | **Create** — redirect to `/login` |
| **Middleware** | `src/middleware.ts` | **Create** — route matcher placeholder |
| **Barrel** | `src/app/components/ui/atoms/index.ts` | **Create** — empty barrel |
| **Barrel** | `src/app/components/ui/molecules/index.ts` | **Create** — empty barrel |
| **Barrel** | `src/app/components/ui/index.ts` | **Create** — aggregated barrel |
| **Docs** | `README.md` | **Create** — project overview and setup |

### Directories Created (empty placeholders)

| Directory | Purpose | Populated in |
|-----------|---------|-------------|
| `src/app/(auth)/` | Auth route group | USFW004 |
| `src/app/(protected)/` | Protected route group | USFW005 |
| `src/app/components/auth/` | Auth organisms | USFW004 |
| `src/app/components/layout/` | Layout organisms | USFW005 |
| `src/app/components/providers/` | Provider management organisms | USFW006 |
| `src/app/components/contracts/` | Contract management organisms | USFW007 |
| `src/app/components/dashboard/` | Dashboard organisms | USFW008 |
| `src/app/components/settings/` | Settings organisms | USFW009 |
| `src/app/config/` | App configuration | USFW002+ |
| `src/app/constants/` | Enums, routes | USFW002 |
| `src/app/context/` | React contexts | USFW004 |
| `src/app/hooks/` | Custom hooks | USFW004+ |
| `src/app/services/` | API service layer | USFW003 |
| `src/app/utils/` | Utility functions | USFW003 |
| `src/types/` | TypeScript types | USFW002 |

---

## Acceptance Criteria

- [x] ✅ DONE `npm install` completes with zero errors
- [x] ✅ DONE `npm run dev` starts the dev server on `http://localhost:3000` with no compilation errors
- [x] ✅ DONE `npm run type-check` passes with zero errors
- [x] ✅ DONE `npm run lint` passes with zero errors
- [x] ✅ DONE `npm run build` succeeds (production build)
- [x] ✅ DONE Navigating to `http://localhost:3000` redirects to `/login`
- [x] ✅ DONE `package.json` contains `"name": "allocore-frontend"` and `"private": true`
- [x] ✅ DONE `package.json` contains all 12 required dependencies (next, react, react-dom, typescript, tailwindcss, axios, @tanstack/react-query, lucide-react, sonner, zod, react-hook-form, @hookform/resolvers)
- [x] ✅ DONE `tsconfig.json` has `@/*` → `./src/*` path alias
- [x] ✅ DONE `.env.example` contains `NEXT_PUBLIC_API_BASE_URL=http://localhost:5103/api/v1`
- [x] ✅ DONE `.env.local` is present and gitignored
- [x] ✅ DONE All 15 placeholder directories exist under `src/`
- [x] ✅ DONE Barrel exports exist at `src/app/components/ui/atoms/index.ts`, `molecules/index.ts`, and `ui/index.ts`
- [x] ✅ DONE `src/app/providers.tsx` wraps children in `QueryClientProvider` and renders `<Toaster />`
- [x] ✅ DONE `src/middleware.ts` exists with route matcher configuration
- [x] ✅ DONE `README.md` exists with setup instructions

---

## What is explicitly NOT changing?

- **No `AuthContext` or `AuthProvider`** — deferred to USFW004
- **No `apiClient` (Axios instance)** — deferred to USFW003
- **No `tokenStorage` utility** — deferred to USFW003
- **No TypeScript type definitions** — deferred to USFW002
- **No UI components** (atoms, molecules, organisms) — deferred to USFW004+
- **No route pages** (login, dashboard, etc.) — deferred to USFW004/005
- **No dark mode implementation** — deferred to future polish story
- **No `tailwind.config.js`** — Tailwind v4 does not use a JS config file; configuration is CSS-based
- **No API rewrites in `next.config.ts`** — direct Axios calls to backend; CORS handled separately if needed
- **No tests** — testing infrastructure is not part of this scaffolding story
- **`docs/` directory** — already exists, untouched by this story

---

## Follow-ups (Intentionally Deferred)

| Item | Reason | Related Story |
|------|--------|---------------|
| `AuthProvider` in `providers.tsx` | Requires `AuthContext` which depends on API service layer | USFW004 |
| CORS / API rewrite configuration | May not be needed; evaluate when API integration begins | USFW003 |
| Dark mode CSS variables | Polish phase, not needed for initial development | Future |
| Server-side token validation in middleware | Requires auth system to be built first | USFW004 |
| `favicon.ico` and Open Graph metadata | Polish phase | Future |
| Design system documentation (`docs/system/design-system.md`) | Created after initial components are built | USFW004+ |
| Frontend prompt files (`Deep_Review.md`, `Proceed_with_Implementation.md`) | Created alongside first frontend implementation story | USFW002 |
