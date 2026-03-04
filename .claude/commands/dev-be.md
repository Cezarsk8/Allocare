Fase Backend do desenvolvimento: escolhe a história, cria branch, revisa, implementa, testa, documenta e commita.

## Passo 1 — Escolher história

1. Leia o Roadmap: `docs/roadmap.md`
2. Identifique a primeira história **não concluída** no Tier de menor número.
   - Concluída = todas as checklists `✅ DONE` ou listada no Development History.
   - Histórias futuras: `docs/user-stories/future/` (e subpastas).
3. Se $ARGUMENTS for fornecido, use como identificador direto da história (caminho ou número).
4. Leia o arquivo completo da história.
5. Apresente: número, título, tier, esforço, dependências.

> Anote o **caminho do arquivo** e o **número da história** (ex: US004) — serão usados nos passos seguintes.

---

## Passo 2 — Criar branch

1. Crie uma branch com nome `USXXX-NomeDaStory` no `Allocore-backend/`:
   ```
   cd Allocore-backend && git checkout -b USXXX-NomeDaStory
   ```
2. **Só se a história tiver impacto no frontend**, crie também no `Allocore-frontend/`:
   ```
   cd Allocore-frontend && git checkout -b USXXX-NomeDaStory
   ```
3. Confirme que o(s) repo(s) está(ão) na nova branch.

---

## Passo 3 — Deep Review

1. Leia o prompt de review: `Allocore-backend/Docs/Prompts/Deep_Review.md`
2. Inspecione os arquivos relevantes do codebase conforme indicado no prompt.
3. Execute a review completa seguindo o prompt ao pé da letra.
4. Auto-aplique melhorias de baixo risco diretamente no arquivo da história.
5. Emita o veredicto:
   - ✅ Ready → continue para o Passo 4
   - ⚠️ Minor corrections → aplique e continue para o Passo 4
   - ❌ Blocking issues → **PARE**. Liste os problemas e aguarde o usuário.

---

## Passo 4 — Implementação Backend

1. Leia o prompt: `Allocore-backend/Docs/Prompts/Proceed_with_Implementation.md`
2. Leia referências:
   - `Allocore-backend/Docs/System/ProjectStructure.md`
   - `Allocore-backend/Docs/System/DevelopmentHistory.md`
3. Inspecione os arquivos do codebase **antes** de alterar.
4. Implemente step-by-step na ordem da história:
   - Domain → Application → Infrastructure → API → Tests
   - Marque cada step como `✅ DONE` no arquivo da história.

---

## Passo 5 — Verificação

Execute os comandos de verificação no `Allocore-backend/`:

```bash
dotnet build
dotnet test
```

- Se **build falhar**: corrija antes de continuar.
- Se **testes falharem**: corrija os testes que quebraram pela mudança. Não ignore falhas.

---

## Passo 6 — Documentação Backend

1. Execute `git diff --stat` em `Allocore-backend/` para ver os arquivos alterados.
2. Inspecione o código **implementado** (não a story).
3. Adicione uma nova entrada em `Allocore-backend/Docs/System/DevelopmentHistory.md`:
   - No final do arquivo (ordem cronológica), antes do Template.
   - Incremente a versão minor.
   - Siga o formato existente: Summary, Changes, Files Created/Modified, Migration Notes, User Story.

---

## Passo 7 — Commit

1. Stage e commit no `Allocore-backend/`:
   ```bash
   git add [arquivos relevantes]
   git commit -m "feat: implement USXXX [título]

   [descrição breve das mudanças]

   Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
   ```
2. Stage e commit a story atualizada no repo root:
   ```bash
   cd .. && git add docs/ && git commit -m "docs: update USXXX story with review and implementation status"
   ```

---

## Conclusão

Apresente o resumo:

```
✅ /dev-be concluído: [Título da história]
──────────────────────────────────────────
Branch:       USXXX-NomeDaStory
Review:       ✅ Ready / ⚠️ Minor corrections
Implement:    ✅ X/Y steps done
Build:        ✅ / ❌
Tests:        ✅ X passed / ❌ Y failed
Document:     ✅ vX.Y.0 adicionado
Commit:       ✅ [hash curto]
──────────────────────────────────────────
Próximo: /dev-fe para a fase frontend
         /ship se a história não tem frontend
```
