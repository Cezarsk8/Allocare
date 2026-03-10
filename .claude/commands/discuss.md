Discussão crítica pré-implementação: analisa a próxima história com olho de engenheiro sênior, identifica problemas, inconsistências, oportunidades de reutilização e riscos antes de começar a codar.

## Objetivo

Este comando é a fase de **"pensa antes de fazer"**. Antes de `/dev-be`, roda `/discuss` para:
- Questionar se a história faz sentido como está
- Identificar dados/componentes que a história assume mas não existem
- Encontrar oportunidades de reutilização na codebase
- Sugerir simplificações ou reordenamento
- Dar um veredicto honesto sobre ROI e viabilidade

**Seja brutalmente honesto.** Se a história é overengineering, diga. Se está mal escrita, diga. Se o esforço não vale, diga.

---

## Passo 1 — Identificar a história

1. Se $ARGUMENTS for fornecido, use como identificador da história (número, caminho ou nome).
2. Senão, leia `docs/roadmap.md` e identifique a **próxima história não concluída**.
3. Leia o arquivo completo da história em `docs/user-stories/future/`.
4. Apresente: número, título, esforço estimado, dependências.

---

## Passo 2 — Análise da Codebase (paralelo)

Lance sub-agentes para investigar em paralelo:

### 2a — Backend (`Allocore-backend/`)
- Entidades de domínio mencionadas ou impactadas
- Repositórios existentes e seus métodos (o que já existe vs. o que precisa ser criado)
- Services existentes que podem ser reutilizados ou estendidos
- Validators existentes (FluentValidation)
- Migrations existentes e schema atual
- Padrões Clean Architecture + CQRS atuais (Domain → Application → Infrastructure → API)

### 2b — Frontend (`Allocore-frontend/`)
- Tipos TypeScript existentes que serão impactados
- Componentes existentes reutilizáveis (Atomic Design: atoms, molecules, organisms)
- Hooks e services existentes
- Padrões de UI/UX já estabelecidos

### 2c — Dados & Infraestrutura
- Verificar se os dados que a história assume existem de fato (tabelas, migrations)
- Se a feature depende de serviços externos, validar se a infra está pronta
- Identificar se a feature vai funcionar com dados reais ou só em cenário idealizado
- Checar se Docker/docker-compose precisa de alteração

---

## Passo 3 — Análise Crítica

Avalie cada aspecto e dê um veredicto claro:

### 3.1 — Consistência da História
- [ ] Os acceptance criteria estão completos e não-ambíguos?
- [ ] Os DTOs/endpoints propostos seguem os padrões existentes (REST, naming)?
- [ ] As dependências listadas estão realmente satisfeitas?
- [ ] A estimativa de esforço é realista? (compare com histórias similares já feitas)
- [ ] Existem referências a componentes/métodos que **não existem**?

### 3.2 — Arquitetura & Engenharia
- [ ] O design segue Clean Architecture + CQRS como o resto da codebase?
- [ ] Existe reutilização máxima de repositórios, services, DTOs?
- [ ] Precisa de nova entidade ou basta estender as existentes?
- [ ] O endpoint REST está semanticamente correto?
- [ ] Existem riscos de performance (N+1, queries pesadas)?

### 3.3 — Frontend & UX
- [ ] Segue Atomic Design (atom vs molecule vs organism)?
- [ ] Os componentes propostos são reutilizáveis ou single-use?
- [ ] Respeita o Design System existente?
- [ ] A navegação/routing faz sentido no app flow?

### 3.4 — Valor vs. Esforço (ROI)
- [ ] Essa feature resolve um problema real do domínio de gestão de provedores/custos?
- [ ] O esforço é proporcional ao valor entregue?
- [ ] Existe uma versão mais simples que entrega 80% do valor com 30% do esforço?
- [ ] Faz sentido fazer agora ou existem histórias com melhor ROI?

---

## Passo 4 — Veredicto

Emita **um** dos veredictos:

### ✅ Pronta para implementação
A história está sólida. Pode seguir para `/dev-be`.
- Liste ajustes menores se houver.

### ⚠️ Precisa de ajustes
A história tem problemas corrigíveis. Liste:
- O que precisa mudar na história
- Sugestões de simplificação
- Pergunte ao usuário se quer aplicar os ajustes

### ❌ Repensar
A história tem problemas fundamentais (ROI ruim, dados inexistentes, scope creep, etc.).
- Explique por que
- Sugira alternativas
- Pergunte ao usuário o que prefere fazer

---

## Formato de Saída

```
## Discuss: USXXX — Título
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

**Esforço**: X | **Dependências**: USYYY (✅), USZZZ (⏳)

### Consistência da História
[análise com ✅/⚠️/❌ por item]

### Arquitetura & Engenharia
[análise com foco em reutilização, Clean Architecture + CQRS]

### Frontend & UX
[análise com foco em componentes, design system]

### Valor vs. Esforço
[análise honesta de ROI]

### Reutilização Identificada
| O que existe | Onde | Como usar |
|---|---|---|
| ... | ... | ... |

### Problemas Encontrados
1. **[Severidade]** Descrição do problema → Sugestão

### Veredicto: ✅/⚠️/❌
[Explicação + próximos passos]
```

## Regras

- **Não seja gentil** — o objetivo é encontrar problemas ANTES de gastar horas implementando.
- **Leia código real** — não assuma que algo existe, verifique na codebase.
- **Compare com histórias passadas** — use `docs/user-stories/history/` para calibrar esforço e complexidade.
- **Sugira simplificações agressivas** — "80% do valor com 30% do esforço" é sempre melhor.
- **Contexto do app** — Allocare é uma plataforma de gestão de provedores e controle de custos. Poucos usuários, foco em estrutura e auditabilidade. Progresso > perfeição.
