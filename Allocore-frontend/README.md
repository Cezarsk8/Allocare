# Allocore-frontend

Frontend para a plataforma Allocare de gestão de provedores e controle de custos.

## Tech Stack

| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| Next.js | 16 | Framework React com App Router |
| React | 19 | UI library |
| TypeScript | 5 | Type system |
| Tailwind CSS | 4 | Styling (CSS-based config) |
| React Query | 5 | Server state management |
| Axios | 1.x | HTTP client |
| Zod | 4.x | Validation |
| React Hook Form | 7.x | Form handling |
| Lucide React | 0.5x | Icons |
| Sonner | 2.x | Toast notifications |

## Pré-requisitos

- Node.js >= 18
- npm >= 9
- Allocore backend rodando em `localhost:5103`

## Setup

```bash
npm install
cp .env.example .env.local
npm run dev
```

Abrir `http://localhost:3000`

## Scripts

| Script | Descrição |
|--------|-----------|
| `npm run dev` | Servidor de desenvolvimento |
| `npm run build` | Build de produção |
| `npm run start` | Servidor de produção |
| `npm run lint` | ESLint |
| `npm run type-check` | TypeScript type checking |

## Estrutura do Projeto

```
src/
├── app/
│   ├── (auth)/              → Route group: login, register, forgot-password
│   ├── (protected)/         → Route group: dashboard, providers, contracts
│   ├── components/
│   │   ├── ui/
│   │   │   ├── atoms/       → Botões, inputs, badges
│   │   │   └── molecules/   → Cards, forms, dialogs
│   │   ├── auth/            → Componentes de autenticação
│   │   ├── layout/          → Sidebar, header, navigation
│   │   ├── providers/       → Provider management UI
│   │   ├── contracts/       → Contract management UI
│   │   ├── dashboard/       → Dashboard widgets
│   │   └── settings/        → Settings UI
│   ├── config/              → Configuração do app
│   ├── constants/           → Enums, rotas
│   ├── context/             → React contexts
│   ├── hooks/               → Custom hooks
│   ├── services/            → API service layer
│   └── utils/               → Funções utilitárias
├── types/                   → TypeScript types
└── middleware.ts             → Route middleware
```

## Atomic Design

- **Atoms**: Componentes primitivos (Button, Input, Badge, Label)
- **Molecules**: Composições de atoms (FormField, Card, Dialog)
- **Organisms**: Features completas (LoginForm, ProviderTable, Sidebar)
- **Pages**: Route-level components que compõem organisms

## Backend

O backend Allocore deve estar rodando na porta 5103. Configure a URL via `NEXT_PUBLIC_API_BASE_URL` no `.env.local`.
