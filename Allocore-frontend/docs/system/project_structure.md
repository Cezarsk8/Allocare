# Allocore-frontend Project Structure

## Overview

Next.js 16 App Router with Atomic Design pattern, TypeScript, Tailwind CSS 4.

## Directory Structure

```
Allocore-frontend/
├── docs/
│   ├── prompts/             → Claude prompts (Deep_Review, Implementation)
│   └── system/              → Design system, project structure, dev history
├── public/                  → Static assets
├── src/
│   ├── app/
│   │   ├── (auth)/          → Auth route group (login, register)
│   │   ├── (protected)/     → Protected route group (dashboard, etc.)
│   │   ├── components/
│   │   │   ├── ui/
│   │   │   │   ├── atoms/   → Primitive components (Button, Input, Badge)
│   │   │   │   └── molecules/ → Composed components (FormField, Card)
│   │   │   ├── auth/        → Auth organisms (LoginForm, RegisterForm)
│   │   │   ├── layout/      → Layout organisms (Sidebar, Header)
│   │   │   ├── providers/   → Provider management organisms
│   │   │   ├── contracts/   → Contract management organisms
│   │   │   ├── dashboard/   → Dashboard organisms
│   │   │   └── settings/    → Settings organisms
│   │   ├── config/          → App configuration
│   │   ├── constants/       → Enums, route constants
│   │   ├── context/         → React contexts (AuthContext, etc.)
│   │   ├── hooks/           → Custom hooks
│   │   ├── services/        → API service layer (Axios)
│   │   ├── utils/           → Utility functions
│   │   ├── globals.css      → Tailwind v4 CSS
│   │   ├── layout.tsx       → Root layout (Server Component)
│   │   ├── page.tsx         → Root page (redirect)
│   │   └── providers.tsx    → Client providers (QueryClient, Toaster)
│   ├── types/               → TypeScript type definitions
│   └── middleware.ts        → Route middleware
├── .env.example             → Environment template
├── eslint.config.mjs        → ESLint flat config
├── next.config.ts           → Next.js configuration
├── package.json             → Dependencies and scripts
├── postcss.config.mjs       → Tailwind v4 PostCSS
├── README.md                → Project documentation
└── tsconfig.json            → TypeScript configuration
```

## Key Conventions

- **Path alias**: `@/*` maps to `./src/*`
- **Barrel exports**: UI components re-exported via `@/app/components/ui`
- **Server vs Client**: Only add `'use client'` when needed (hooks, state, browser APIs)
- **Tailwind v4**: Config via CSS (`@import "tailwindcss"`), no `tailwind.config.js`
- **API calls**: Via Axios to `NEXT_PUBLIC_API_BASE_URL`, no Next.js rewrites
