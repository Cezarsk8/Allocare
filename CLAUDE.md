# Allocare Project — Contexto para Claude

## O que é o Allocare

Allocare é uma plataforma de gestão de provedores e controle de custos para empresas.
Centraliza dados de provedores, contratos, serviços e custos em um sistema estruturado e auditável — permitindo que gestores de operações, finanças e procurement tenham visibilidade completa sobre seus fornecedores.

Domínio principal: **Provider Registry → Contract Management → Cost Tracking → Cost Allocation → Reporting**.

## Estrutura do Repositório

```
Allocare/
├── docs/                    → Documentação centralizada (roadmap, user stories)
│   ├── roadmap.md
│   └── user-stories/
│       ├── future/          → Histórias pendentes (backend + frontend)
│       └── history/         → Histórias implementadas
│           ├── backend/
│           └── frontend/
├── Allocore-backend/        → Backend (.NET 8 Web API + PostgreSQL, Clean Architecture + CQRS)
└── Allocore-frontend/       → Frontend (Next.js 15 + TypeScript + Tailwind CSS 4 — placeholder)
```

Cada projeto tem seu próprio repositório git e suas próprias pastas de docs técnicos (`Allocore-backend/Docs/`, `Allocore-frontend/docs/`).
User stories e roadmap vivem centralizados em `docs/` na raiz.

---

## Workflow de Desenvolvimento

Cada feature segue este processo **obrigatório**, nesta ordem:

### 1. Escolher a história
Consulte o roadmap para pegar a próxima história do topo:
`docs/roadmap.md`

As histórias futuras estão em `docs/user-stories/future/` (e subpastas).

A ordem de prioridade é: **Tier 1 → Tier 2 → Tier 3...** (nunca pular tiers sem motivo).

### 2. Deep Review → `/review`
Revisão completa da user story **antes** de qualquer código.
- Detecta inconsistências, edge cases ausentes, ambiguidades
- Aplica melhorias de baixo risco automaticamente (atualiza o arquivo da história)
- Emite veredicto: ✅ Ready / ⚠️ Minor corrections / ❌ Blocking issues

**Prompts de review:**
- Backend: `Allocore-backend/Docs/Prompts/Deep_Review.md`
- Frontend / Full-Stack: `Allocore-frontend/docs/prompts/Deep_Review.md`

### 3. Implementação → `/implement`
Implementa a história **exatamente como especificada**, step by step.
- Inspeciona o código existente antes de tocar qualquer arquivo
- Segue os padrões do projeto sem inventar novos
- Marca cada step como `✅ DONE` na checklist da história conforme conclui
- Executa build/type-check ao fim de cada step

**Prompts de implementação:**
- Backend: `Allocore-backend/Docs/Prompts/Proceed_with_Implementation.md`
- Frontend / Full-Stack: `Allocore-frontend/docs/prompts/Proceed_with_Implementation.md`

### 4. Documentação → `/document`
Atualiza o Development History de cada repo tocado pela história.
- Inspeciona o código implementado (não a story original)
- Backend: adiciona entrada em `Allocore-backend/Docs/System/DevelopmentHistory.md` (cronológico)
- Frontend: adiciona entrada em `Allocore-frontend/docs/system/development-history.md` (reverso cronológico)

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
| `docs/user-stories/future/` | Histórias pendentes de implementação |
| `docs/user-stories/history/` | Histórias já implementadas (backend/ e frontend/) |
| `Allocore-backend/Docs/System/SystemArchitecture.md` | Arquitetura do backend |
| `Allocore-backend/Docs/System/DevelopmentHistory.md` | Histórico de desenvolvimento backend |
| `Allocore-backend/Docs/System/ProductVision.md` | Visão do produto |
| `Allocore-frontend/docs/system/project_structure.md` | Estrutura de pastas do frontend |
| `Allocore-frontend/docs/system/design-system.md` | Design system (cores, componentes, dialogs) |
| `Allocore-frontend/docs/system/development-history.md` | Histórico de desenvolvimento frontend |
