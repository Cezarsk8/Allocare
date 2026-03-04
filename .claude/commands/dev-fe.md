Fase Frontend do desenvolvimento: escreve a FE story, revisa, implementa, testa, documenta e commita na branch existente.

## Passo 1 — Identificar a história backend implementada

1. Se $ARGUMENTS for fornecido, use como identificador direto da história (caminho, número ou título).
2. Caso contrário, detecte a branch atual:
   ```bash
   cd Allocore-backend && git branch --show-current
   ```
   - Extraia o identificador da história do nome da branch (ex: `US004-NomeDaStory` → US004).
3. Localize o arquivo da história em `docs/user-stories/future/`.
4. Leia o arquivo completo da história backend.
5. Confirme que o backend já foi implementado (steps marcados como `✅ DONE` ou commit existente).

> Se o backend **não** foi implementado, **PARE** e instrua o usuário a rodar `/dev-be` primeiro.

---

## Passo 2 — Garantir branch no Allocore-frontend

1. Verifique se a branch `USXXX-NomeDaStory` já existe no `Allocore-frontend/`:
   ```bash
   cd Allocore-frontend && git branch --list USXXX-NomeDaStory
   ```
2. Se **não existir**, crie-a:
   ```bash
   cd Allocore-frontend && git checkout -b USXXX-NomeDaStory
   ```
3. Se **já existir**, faça checkout:
   ```bash
   cd Allocore-frontend && git checkout USXXX-NomeDaStory
   ```
4. Confirme que `Allocore-frontend/` está na branch correta.

---

## Passo 3 — Entender os contratos de API

1. Leia o código backend implementado para mapear os contratos:
   - Controllers: endpoints, rotas, métodos HTTP
   - DTOs: request/response shapes
   - Validações: regras de negócio aplicadas
2. Leia as referências do frontend:
   - `Allocore-frontend/docs/system/design-system.md`
   - `Allocore-frontend/docs/system/project_structure.md`
   - `Allocore-frontend/docs/system/development-history.md`

---

## Passo 4 — Escrever a história de frontend

1. Leia o template: `Allocore-frontend/docs/prompts/newUserStory.md`
2. Inspecione o codebase frontend para entender padrões existentes.
3. Escreva a história de frontend correspondente seguindo **exatamente** o template.
4. Salve em `docs/user-stories/future/[USXXX]-[titulo]-Frontend.md`
5. Apresente a história criada e aguarde confirmação do usuário antes de prosseguir.

> Se o usuário rejeitar ou pedir ajustes, aplique e reapresente.

---

## Passo 5 — Deep Review Frontend

1. Leia o prompt de review: `Allocore-frontend/docs/prompts/Deep_Review.md`
2. Inspecione os arquivos relevantes do codebase frontend.
3. Execute a review completa seguindo o prompt ao pé da letra.
4. Auto-aplique melhorias de baixo risco diretamente no arquivo da história.
5. Emita o veredicto:
   - ✅ Ready → continue para o Passo 6
   - ⚠️ Minor corrections → aplique e continue para o Passo 6
   - ❌ Blocking issues → **PARE**. Liste os problemas e aguarde o usuário.

---

## Passo 6 — Implementação Frontend

1. Leia o prompt: `Allocore-frontend/docs/prompts/Proceed_with_Implementation.md`
2. Inspecione os arquivos do codebase **antes** de alterar.
3. Implemente step-by-step na ordem da história:
   - Types → Services → Hooks → Components → Pages → Tests
   - Marque cada step como `✅ DONE` no arquivo da história.
4. Verifique `npm run type-check` + `npm run build` após cada step.

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

1. Execute `git diff --stat` em `Allocore-frontend/` para ver os arquivos alterados.
2. Inspecione o código **implementado** (não a story).
3. Adicione uma nova entrada em `Allocore-frontend/docs/system/development-history.md`:
   - No topo do arquivo (ordem reversa cronológica).
   - Incremente a versão minor.
   - Siga o formato existente: Summary, Changes (com ✅), Files Created/Modified, User-Facing Changes, User Story.

---

## Passo 9 — Commit

1. Stage e commit no `Allocore-frontend/`:
   ```bash
   cd Allocore-frontend && git add [arquivos relevantes]
   git commit -m "feat: implement USXXX [título] frontend

   [descrição breve das mudanças]

   Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
   ```
2. Stage e commit a story frontend + story backend atualizada no repo root:
   ```bash
   cd .. && git add docs/ && git commit -m "docs: add USXXX frontend story and update status"
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
