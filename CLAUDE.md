# Allocare Project — Contexto para Claude

## O que é o Allocare

Allocare é uma plataforma de gestão de provedores e controle de custos para empresas.
Centraliza dados de provedores, contratos, serviços e custos em um sistema estruturado e auditável — permitindo que gestores de operações, finanças e procurement tenham visibilidade completa sobre seus fornecedores.

Domínio principal: **Provider Registry → Contract Management → Cost Tracking → Cost Allocation → Reporting**.

## Stack

- **Backend**: .NET 8 (Clean Architecture, PostgreSQL, CQRS + MediatR)
- **Frontend**: Next.js 15 (TypeScript, Tailwind CSS 4, Atomic Design)

## Estrutura do Repositório

```
Allocare/
├── docs/                     → Documentação centralizada (TODA a documentação vive aqui)
│   ├── roadmap.md            → Priorização de histórias
│   ├── product-vision.md     → Visão de produto
│   ├── design-system.md      → Design system (cores, componentes)
│   ├── development-history.md → Histórico de dev unificado (seções Backend + Frontend)
│   ├── release-notes/        → Release notes unificados (criados pelo /ship)
│   └── user-stories/
│       ├── future/           → Histórias pendentes
│       └── history/          → Histórias implementadas
│           ├── backend/{domain}/
│           └── frontend/{domain}/
├── Allocore-backend/         → Backend (.NET 8 Web API + PostgreSQL, só código)
└── Allocore-frontend/        → Frontend (Next.js 15 + TypeScript + Tailwind CSS 4, só código)
```

É um monorepo com um único .git na raiz. **Toda a documentação vive centralizada em `docs/`** — os subprojetos contêm apenas código.

---

## Workflow de Desenvolvimento

Cada feature segue até 5 fases sequenciais:

```
/discuss  →  /dev-be  →  /dev-fe  →  /ship
```

Comandos auxiliares: `/spike` · `/roadmap` · `/workflow` · `/start`

### `/discuss` — Análise Pré-Implementação
Analisa a história com olho crítico → Verifica codebase → Identifica reutilização → Emite veredicto (✅ Pronta / ⚠️ Ajustar / ❌ Repensar)

### `/dev-be` — Fase Backend
Cria branch → Deep Review → Implementa → Testa → Documenta → Commita

### `/dev-fe` — Fase Frontend
Lê contratos BE → Escreve FE story → Deep Review → Implementa → Testa → Documenta → Commita

### `/ship` — Merge & Release
Merge para main → Arquiva histórias → Revisa docs → Release notes → Push → Deleta feature branch

### `/spike` — Pesquisa & Validação Técnica
Pesquisa profunda (negócio + técnica) → Script descartável → Testa hipótese → Documenta em `docs/spikes/` → Veredicto (PROCEED/PIVOT/ABORT)

### `/roadmap` — Otimização do Roadmap
Analisa e reordena o roadmap por importância e relevância

Para histórias **backend-only** (sem frontend): `/dev-be` → `/ship`

---

## Convenções Frontend (Allocore)

### File Conventions

| Layer | Location | Pattern |
|-------|----------|---------|
| Routes | `src/app/(auth)/` e `src/app/(protected)/` | App Router, route groups |
| Feature Components | `src/app/components/{feature}/` | Organisms por domínio (auth, companies, providers, contracts) |
| UI Components | `src/app/components/ui/atoms/` e `molecules/` | Atomic Design |
| Layout Components | `src/app/components/layout/` | Sidebar, Header |
| Services | `src/app/services/{feature}Service.ts` | API calls via Axios (`apiClient`) |
| Types | `src/types/{feature}.ts` | Interfaces, enums, DTOs |
| Hooks | `src/app/hooks/use{Entity}.ts` | React Query hooks |
| Context | `src/app/context/{Feature}Context.tsx` | AuthContext |
| Constants | `src/app/constants/{feature}Schemas.ts` | Zod validation schemas |

### Component Strategy

- **Atomic Design**: Atoms (Button, Input, Badge) → Molecules (FormField, Card) → Organisms (LoginForm, ProviderTable)
- `'use client'` apenas quando necessário (hooks, state, event handlers)
- Barrel exports via `index.ts` para UI components
- Path alias: `@/*` maps to `./src/*`

### API Integration

- `apiClient` (Axios) com interceptor para JWT e 401 redirect
- React Query para server state (hooks em `src/app/hooks/`)
- Zod schemas para validação de formulários (React Hook Form + Zod)
- Error handling: parse error response do backend

### Auth

- `AuthContext` → `useAuth` hook (token, user, login/register/logout)
- Route groups: `(auth)` para login/register, `(protected)` para rotas autenticadas
- 401 interceptor redireciona para `/login`

### Design System

- Tailwind CSS 4 (CSS-based config, no `tailwind.config.js`)
- Icons: Lucide React
- Toasts: Sonner
- Forms: React Hook Form + Zod
- Colors: Primary blue-600, Background gray-50
- Mobile-first responsive (sm/md/lg/xl breakpoints)
- Detalhes completos em `docs/design-system.md`

---

## Regras Gerais

- **Evolving App Mode**: o app tem poucos ou nenhum usuário externo. Prefira progresso a preservação de compatibilidade. Não adicione shims ou fallbacks desnecessários.
- **Não pular etapas**: nunca implementar sem review, nunca documentar sem implementar.
- **Scope bounded**: não adicione features além do que está na história. Não refatore código que não foi tocado.
- **Marcar progresso**: use `✅ DONE` nas checklists das histórias enquanto implementa.

---

## Referências Chave

| Arquivo | Propósito |
|---------|-----------|
| `docs/roadmap.md` | Priorização das próximas histórias |
| `docs/product-vision.md` | Visão de produto e proposta de valor |
| `docs/user-stories/future/` | Histórias pendentes de implementação |
| `docs/user-stories/history/` | Histórias já implementadas (backend/ e frontend/) |
| `docs/development-history.md` | Histórico de dev unificado (Backend + Frontend) |
| `docs/design-system.md` | Design system (cores, componentes, responsividade) |
| `docs/release-notes/` | Release notes unificados (criados pelo /ship) |