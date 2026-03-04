Merge, archive e push: finaliza a feature mergeando para main, arquivando histórias e fazendo push dos 3 repos.

## Pré-requisitos

Antes de executar, verifique:
1. `/dev-be` foi concluído (backend implementado e commitado)
2. `/dev-fe` foi concluído **ou** a história é backend-only (sem impacto frontend)

---

## Passo 1 — Verificar estado dos repos

1. Identifique a branch atual em cada repo:
   ```bash
   cd Allocore-backend && git branch --show-current
   cd Allocore-frontend && git branch --show-current
   ```
2. Confirme que ambos (ou apenas `Allocore-backend/` se backend-only) estão na feature branch.
3. Verifique que não há mudanças não commitadas:
   ```bash
   cd Allocore-backend && git status --short
   cd Allocore-frontend && git status --short
   ```
4. Se houver mudanças pendentes, **PARE** e peça ao usuário para commitar ou descartar.

> Extraia o identificador da história (USXXX) do nome da branch.

---

## Passo 2 — Verificação Final (Testes)

Execute todos os testes antes do merge para garantir que nada quebrou:

### Allocore Backend
```bash
cd Allocore-backend && dotnet build && dotnet test
```

### Allocore Frontend (se aplicável)
```bash
cd Allocore-frontend && npm run type-check && npm run build && npm test
```

- Se **qualquer teste falhar**: **PARE**. Corrija antes de fazer merge.
- Se a história é backend-only, pule os testes frontend.

---

## Passo 3 — Merge para main

Faça merge da feature branch para main em cada repo:

```bash
# Allocore Backend
cd Allocore-backend && git checkout main && git merge USXXX-NomeDaStory

# Allocore Frontend (se aplicável)
cd Allocore-frontend && git checkout main && git merge USXXX-NomeDaStory
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

## Passo 5 — Commit no repo root

```bash
git add docs/
git commit -m "docs: archive USXXX stories and update roadmap

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

---

## Passo 6 — Push de todos os repos

Antes de fazer push, peça confirmação do usuário listando o que será pushado:

```
Pronto para push:
  - Allocore-backend (main) → origin/main
  - Allocore-frontend (main) → origin/main  [se aplicável]
  - Root (main) → origin/main

Confirma? (s/n)
```

Após confirmação:

```bash
cd Allocore-backend && git push origin main
cd Allocore-frontend && git push origin main
cd .. && git push origin main
```

---

## Passo 7 — Limpar feature branches

Delete as feature branches local e remotamente:

```bash
# Allocore Backend
cd Allocore-backend && git branch -d USXXX-NomeDaStory && git push origin --delete USXXX-NomeDaStory

# Allocore Frontend (se aplicável)
cd Allocore-frontend && git branch -d USXXX-NomeDaStory && git push origin --delete USXXX-NomeDaStory
```

> Se a branch remota não existir (nunca foi pushada), ignore o erro do `--delete`.

---

## Conclusão

Apresente o resumo:

```
✅ /ship concluído: [Título da história]
──────────────────────────────────────────
Branch:       USXXX-NomeDaStory (deletada)
Merge BE:     ✅ main ← USXXX-NomeDaStory
Merge FE:     ✅ main ← USXXX-NomeDaStory / ⏭️ Skipped (backend-only)
Archive:      ✅ Histórias movidas para history/
Roadmap:      ✅ Atualizado
Push:         ✅ Allocore-backend + Allocore-frontend + Root
Cleanup:      ✅ Feature branches deletadas
──────────────────────────────────────────
BE version:   vX.Y.0
FE version:   vX.Y.0

Próximo: /dev-be para iniciar a história seguinte
```
