Fase Frontend do desenvolvimento: escreve a FE story, revisa, implementa, testa, documenta e commita na branch existente.

## Passo 1 — Identificar a história backend implementada

1. Se $ARGUMENTS for fornecido, use como identificador direto da história (caminho, número ou título).
2. Caso contrário, detecte a branch atual:
   ```bash
   git branch --show-current
   ```
   - Extraia o identificador da história do nome da branch (ex: `US004-NomeDaStory` → US004).
3. Localize o arquivo da história em `docs/user-stories/future/`.
4. Leia o arquivo completo da história backend.
5. Confirme que o backend já foi implementado (steps marcados como `✅ DONE` ou commit existente).

> Se o backend **não** foi implementado, **PARE** e instrua o usuário a rodar `/dev-be` primeiro.

---

## Passo 2 — Garantir branch

1. Verifique se já está na branch `USXXX-NomeDaStory` (já criada pelo `/dev-be`).
2. Se não estiver, faça checkout.

---

## Passo 3 — Entender os contratos de API

1. Leia o código backend implementado para mapear os contratos:
   - Controllers: endpoints, rotas, métodos HTTP
   - DTOs: request/response shapes
   - Validações: regras de negócio aplicadas
2. Leia as referências do frontend:
   - `docs/design-system.md`
   - `docs/development-history.md` (seção Frontend)
   - Inspecione o codebase diretamente para entender a estrutura e padrões

---

## Passo 4 — Escrever a história de frontend

1. Inspecione o codebase frontend para entender padrões existentes.
2. Escreva a história de frontend correspondente seguindo o template abaixo.
3. Salve em `docs/user-stories/future/[USXXX]-[titulo]-Frontend.md`
4. Apresente a história criada e aguarde confirmação do usuário antes de prosseguir.

> Se o usuário rejeitar ou pedir ajustes, aplique e reapresente.

### Template de Frontend Story

```markdown
# USFWXXX – [Título da Feature] — Frontend

> **Project:** Allocore-frontend
> **Backend Story:** USXXX
> **Status:** Pending

**Priority:** [Critical / High / Medium / Low]
**Dependencies:** [USFWXXX — descrição]
**Estimated effort:** [1 session / 2 sessions]

## Description
As a **[role]**, I want [goal] so that [benefit].

## Step 0: Responsive Baseline (MANDATORY)
- [ ] Define mobile behavior for all components

## Step 1: Types
- [ ] Create/update types in src/types/

## Step 2: Services
- [ ] Create API service functions in src/app/services/

## Step 3: Hooks
- [ ] Create custom hooks in src/app/hooks/

## Step 4: Components
- [ ] Create atoms, molecules, organisms

## Step 5: Pages
- [ ] Create route pages

## Step 6: Barrel Exports
- [ ] Update all index.ts barrel exports

## Acceptance Criteria
- [ ] npm run type-check passes
- [ ] npm run build passes
```

---

## Passo 5 — Deep Review Frontend

Execute uma revisão completa da user story de frontend **antes** de implementar.

### Checklist de Review

#### 1. Consistência com Backend
- [ ] Todos os endpoints referenciados existem no backend implementado?
- [ ] Os tipos (request/response) correspondem aos DTOs do backend?
- [ ] As rotas de API estão corretas (method, path, auth)?

#### 2. Consistência com Codebase Frontend
- [ ] Os imports usam o path alias `@/*`?
- [ ] Os nomes de arquivos seguem o padrão existente?
- [ ] Os componentes estão na camada correta (atom/molecule/organism)?
- [ ] Os barrel exports estão atualizados?

#### 3. Padrões do Projeto
- [ ] Client components têm `'use client'`?
- [ ] Server components NÃO têm `'use client'`?
- [ ] Hooks seguem o padrão `useXxx`?
- [ ] Services usam Axios via `apiClient`?
- [ ] Validações usam Zod schemas?

#### 4. UX & Responsividade
- [ ] A história define comportamento mobile?
- [ ] Os estados de loading/error/empty estão definidos?
- [ ] As mensagens de erro são user-friendly?
- [ ] Os toasts usam Sonner?

#### 5. Edge Cases
- [ ] Token expirado / não autenticado?
- [ ] Dados vazios (listas, campos opcionais)?
- [ ] Erros de validação (frontend + backend)?
- [ ] Permissões (admin vs user)?

### Veredicto

- ✅ **Ready** — história pode ser implementada como está
- ⚠️ **Minor corrections** — aplicar correções de baixo risco e continuar
- ❌ **Blocking issues** — problemas que impedem implementação

---

## Passo 6 — Implementação Frontend

Implemente a user story de frontend step-by-step, seguindo rigorosamente a ordem definida.

### Regras de Implementação

#### Antes de Tocar Qualquer Arquivo
1. **Leia** o arquivo existente antes de modificar
2. **Inspecione** padrões do codebase (imports, naming, structure)
3. **Verifique** se o arquivo/componente já existe

#### Ordem de Implementação
1. **Types** — TypeScript interfaces/types em `src/types/`
2. **Services** — API calls em `src/app/services/`
3. **Hooks** — Custom hooks em `src/app/hooks/`
4. **Atoms** — Primitive components em `src/app/components/ui/atoms/`
5. **Molecules** — Composed components em `src/app/components/ui/molecules/`
6. **Organisms** — Feature components em `src/app/components/{feature}/`
7. **Pages** — Route pages em `src/app/(auth)/` ou `src/app/(protected)/`
8. **Barrel exports** — Atualizar todos os `index.ts`

#### Após Cada Step
- Marque `✅ DONE` na checklist da história
- Execute `npm run type-check` para verificar tipos
- Execute `npm run build` ao fim do último step

#### Padrões Obrigatórios
- Use `'use client'` apenas quando necessário (hooks, state, event handlers)
- Use o path alias `@/*` para imports
- Use Zod para validação de formulários
- Use React Hook Form para state de formulários
- Use React Query para server state
- Use Sonner para toasts (`toast.success()`, `toast.error()`)
- Use Lucide React para ícones

#### O que NÃO Fazer
- Não adicione features além do escopo da história
- Não refatore código que não foi tocado pela história
- Não adicione dark mode (será feito em história futura)
- Não pule a verificação de type-check entre steps

---

## Passo 7 — Verificação

Execute os comandos de verificação no `Allocore-frontend/`:

```bash
cd Allocore-frontend && npm run type-check && npm run build && npm test
```

- Se **type-check falhar**: corrija antes de continuar.
- Se **build falhar**: corrija antes de continuar.
- Se **testes falharem**: corrija os testes que quebraram pela mudança. Não ignore falhas.

---

## Passo 8 — Documentação Frontend

1. Execute `git diff --stat` para ver os arquivos alterados.
2. Inspecione o código **implementado** (não a story).
3. Adicione uma nova entrada na seção **Frontend** de `docs/development-history.md`:
   - No topo da seção Frontend (ordem reversa cronológica).
   - Incremente a versão minor.
   - Siga o formato existente: Summary, Changes (com ✅), Files Created/Modified, User-Facing Changes, User Story.

---

## Passo 9 — Commit

1. Stage e commit:
   ```bash
   git add [arquivos relevantes]
   git commit -m "feat: implement USXXX [título] frontend

   [descrição breve das mudanças]

   Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
   ```
2. Stage e commit docs:
   ```bash
   git add docs/ && git commit -m "docs: add USXXX frontend story and update status"
   ```

---

## Conclusão

Apresente o resumo:

```
✅ /dev-fe concluído: [Título da história]
──────────────────────────────────────────
Branch:       USXXX-NomeDaStory
Story FE:     ✅ Criada em [caminho]
Review:       ✅ Ready / ⚠️ Minor corrections
Implement:    ✅ X/Y steps done
Type-check:   ✅ / ❌
Build:        ✅ / ❌
Document:     ✅ vX.Y.0 adicionado
Commit:       ✅ [hash curto]
──────────────────────────────────────────
Próximo: /ship para merge, archive e push
```