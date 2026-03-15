Merge, archive e push: finaliza a feature mergeando para main, arquivando histórias e fazendo push.

**IMPORTANTE**: Execute tudo de uma vez, sem pausas para confirmação. Só pare se algo der errado (conflito, teste falhando, mudanças não commitadas).

## Pré-requisitos

Antes de executar, verifique:
1. `/dev-be` foi concluído (backend implementado e commitado)
2. `/dev-fe` foi concluído **ou** a história é backend-only (sem impacto frontend)

---

## Passo 1 — Verificar estado do repo

1. Identifique a branch atual:
   ```bash
   git branch --show-current && git status --short
   ```
2. Confirme que está na feature branch.
3. Se houver mudanças não commitadas, **PARE** e peça ao usuário para commitar ou descartar.

> Extraia o identificador da história (USXXX) do nome da branch.

---

## Passo 2 — Verificação Final (Testes)

Execute build e testes:

### Backend
```bash
cd Allocore-backend && dotnet build && dotnet test
```

### Frontend (se aplicável)
```bash
cd Allocore-frontend && npm run type-check && npm run build
```

- Se **qualquer teste/build falhar**: **PARE**. Corrija antes de continuar.
- Se a história é backend-only, pule o frontend.

---

## Passo 3 — Merge para main

Faça merge da feature branch para main:

```bash
git checkout main && git merge USXXX-NomeDaStory
```

- Se houver conflitos, **PARE** e resolva com o usuário.

---

## Passo 4 — Arquivar histórias

Mova as histórias de `future/` para `history/`:

1. **História backend**: `docs/user-stories/future/[arquivo]` → `docs/user-stories/history/backend/[domínio]/`
2. **História frontend** (se existir): `docs/user-stories/future/[arquivo]` → `docs/user-stories/history/frontend/[domínio]/`

> Domínios válidos: `infrastructure`, `authentication`, `multi-tenant`, `providers`, `contracts`, `notes`, `costs`, `projects`, `reporting`.

3. Atualize o `docs/roadmap.md`: marque a história como concluída ou remova da lista.

---

## Passo 5 — Revisão de Documentação

Revise e atualize os docs impactados pela feature. Inspecione o código implementado (não a story).

### 5a — Development History (`docs/development-history.md`)

1. Verifique se `/dev-be` e `/dev-fe` já adicionaram entradas para esta feature.
2. Se **faltam entradas**, crie-as agora baseado nos commits e código implementado:
   - Seção **Backend**: Summary, Changes (com ✅), Files Created/Modified, Migration Notes, User Story
   - Seção **Frontend**: Summary, Changes (com ✅), Files Created/Modified, User-Facing Changes, User Story
3. Se as entradas existem mas estão incompletas, complemente.

### 5b — Design System (`docs/design-system.md`)

- Se a feature introduziu **novos padrões UI** (componentes, cores, layouts não documentados), adicione ao design system.
- Se não houve novos padrões visuais: prossiga sem alteração.

### 5c — Product Vision (`docs/product-vision.md`)

- Se a feature impacta significativamente a visão de produto (nova persona, novo fluxo central, nova proposta de valor), atualize.
- Se não: prossiga sem alteração.

---

## Passo 6 — Release Notes

Crie um release note unificado (backend + frontend) em `docs/release-notes/`.

1. Leia a história completa (backend e frontend se aplicável).
2. Inspecione o código implementado (commits recentes).
3. Leia os últimos 2 release notes em `docs/release-notes/` para referência de estilo.
4. Crie o arquivo: `docs/release-notes/vYYYY.MM.DD_USXXX_Feature_Name.md`

### Formato

```markdown
# Release Notes — USXXX: [Feature Title]

**Date:** [Month Day, Year]
**User Story:** USXXX

---

## What's New
[Descrição user-facing das novas capacidades — sem jargão técnico interno]

---

## Technical Details

### Backend
[Endpoints novos/alterados, entidades, migrations, decisões arquiteturais]

### Frontend
[Páginas/componentes novos, integrações de API, design system, mobile]

---

## Breaking Changes
[O que mudou, o que precisa de ação — ou omitir a seção se não houver]

---

## Notes
[Limitações conhecidas, follow-ups, decisões intencionalmente adiadas]
```

Regras:
- Seções são opcionais — omita as que não se aplicam
- Se backend-only, omita a subseção Frontend dos Technical Details
- **Code is truth** — documente o que foi implementado, não o que a história planejava
- Tom: claro e conciso para "What's New", técnico para "Technical Details"

> **Nota:** Release notes históricos (pré-março/2026) têm prefixo `be-` ou `fe-`. Novos release notes usam o formato unificado `vYYYY.MM.DD_USXXX_*.md`.

---

## Passo 7 — Commit no repo root

```bash
git add docs/ && git commit -m "docs: archive USXXX stories, add release notes, update roadmap

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

---

## Passo 8 — Push

Antes de fazer push, peça confirmação do usuário listando o que será pushado:

```
Pronto para push:
  - main → origin/main

Confirma? (s/n)
```

Após confirmação:

```bash
git push origin main
```

---

## Passo 9 — Limpar feature branch

Delete a feature branch local:

```bash
git branch -d USXXX-NomeDaStory
```

---

## Conclusão

Apresente o resumo:

```
✅ /ship concluído: [Título da história]
──────────────────────────────────────────
Branch:       USXXX-NomeDaStory (deletada)
Merge:        ✅ main ← USXXX-NomeDaStory
Archive:      ✅ Histórias movidas para history/
Docs:         ✅ Dev history + design system + product vision revisados
Release Note: ✅ docs/release-notes/vYYYY.MM.DD_USXXX_*.md
Push:         ✅ origin/main
Cleanup:      ✅ Feature branch deletada
──────────────────────────────────────────

Próximo: /dev-be para iniciar a história seguinte
```