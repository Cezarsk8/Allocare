Fase Backend do desenvolvimento: escolhe a história, cria branch, revisa, implementa, testa, documenta e commita.

## Passo 1 — Escolher história

1. Leia o Roadmap: `docs/roadmap.md`
2. Identifique a primeira história **não concluída** no Tier de menor número.
   - Concluída = todas as checklists marcadas ou listada no Development History.
   - Histórias futuras: `docs/user-stories/future/` (e subpastas).
3. Se $ARGUMENTS for fornecido, use como identificador direto da história (caminho ou número).
4. Leia o arquivo completo da história.
5. Apresente: número, título, tier, esforço, dependências.

> Anote o **caminho do arquivo** e o **número da história** (ex: US004) — serão usados nos passos seguintes.

---

## Passo 2 — Criar branch

1. Crie uma branch com nome `USXXX-NomeDaStory` a partir da raiz do monorepo:
   ```
   git checkout -b USXXX-NomeDaStory
   ```
2. Confirme que o repo está na nova branch.

---

## Passo 3 — Deep Review

Execute a review completa da user story **antes** de implementar, seguindo os critérios abaixo.

### Workflow obrigatório

1. Leia a story end-to-end.
2. Inspecione o contexto relevante do codebase:
   - Inspecione o codebase diretamente para entender a estrutura e convenções
   - `docs/development-history.md` (seção Backend) — check for related past work or known issues
   - `docs/product-vision.md` — verify alignment with domain model and business rules
   - Related domain models in `Allocore-backend/src/Allocore.Domain/Entities/`
   - Related handlers, validators in `Allocore-backend/src/Allocore.Application/Features/`
   - Related repositories, EF configurations in `Allocore-backend/src/Allocore.Infrastructure/`
   - Controllers and middleware in `Allocore-backend/src/Allocore.API/`
   - Error model and API conventions
3. Perform the review using the criteria below.
4. Auto-apply low-risk improvements to the user story text.

Assume the story WILL be implemented exactly as written — so ambiguity is a defect.

### Critérios de Review

**B) Structural Validation (CRITICAL)**
- Description clearly states: who, what, why
- Each step targets a layer (Domain/Infrastructure/Application/API/Tests)
- Tasks are concrete, actionable, and ordered correctly (Domain → Infrastructure → Application → API → Tests)
- Dependencies are explicit (no "magic implied steps")
- File paths reference actual project structure

**C) Step-by-Step Technical Soundness**
For EACH step, validate:
- Domain Layer: Invariants explicit, entities extend `Entity`, responsibilities correctly placed
- Infrastructure Layer: Migrations safe, EF configurations follow patterns, DI registration in `DependencyInjection.cs`
- Application Layer: CQRS pattern via MediatR, FluentValidation validators, side effects explicit, DTOs complete
- API Layer: Contracts explicit, error behavior predictable, API versioning follows `/api/v{version}/[controller]`

**D) Allocore Domain-Specific Validation**
- Multi-Tenancy: All new entities scoped to CompanyId, queries filter by CompanyId, UserCompany validated
- Cost Allocation Integrity (if applicable): Allocation percentages sum to 100%, types explicit, currency/period handling specified
- Provider Management (if applicable): Contacts model, contract tracking, service catalog scoped

**E) Contract & Semantics Integrity**
- No field silently changes meaning
- Derived values have a single owner (server OR client, never both)

**F) Legacy/Fallback Audit (Evolving App Mode)**
- Challenge fallback logic, hardcoded defaults, backwards-compatible dual mode, soft-deprecations
- Default bias: remove legacy paths unless concrete safety/migration reason

**G) Data & Migration Safety**
- Column types, precision, defaults, constraints specified
- FK relationships with correct delete behavior
- Rollback considerations noted when relevant

**H) Edge Cases & Boundary Enforcement**
- Edge cases listed AND enforced
- Boundary values: zero/null/max/rounding/concurrency
- Multi-tenancy edge cases

**I) Testing Adequacy**
- Tests cover highest-risk behavior (irreversible writes, financial correctness, multi-tenant isolation)
- In Evolving App Mode, require tests for critical invariants only

### Veredicto

- ✅ Ready → continue para o Passo 4
- ⚠️ Minor corrections → aplique e continue para o Passo 4
- ❌ Blocking issues → **PARE**. Liste os problemas e aguarde o usuário.

### Auto-Apply Rules

Apply improvements that are low-risk, unambiguous, and consistent with conventions:
- Purely clarifying (names, ordering, missing details, acceptance criteria, error cases)
- Removing unnecessary legacy/fallback paths (Evolving App Mode bias)
- Adding missing but required specifics (types/precision, response codes, validation rules)

Do NOT apply improvements that require product decisions or unknown tradeoffs — write those as explicit questions.

---

## Passo 4 — Implementação Backend

Implemente a user story step-by-step seguindo estas regras:

### Antes de tocar qualquer arquivo

1. Inspecione o codebase diretamente para entender estrutura e padrões
2. Leia `docs/development-history.md` (seção Backend) para contexto de trabalho anterior
3. Confirme como features similares foram implementadas e siga os mesmos padrões

### Contexto do Projeto

Allocore é uma API .NET 8 Clean Architecture para gestão de custos de provedores. Usa PostgreSQL + EF Core, MediatR (CQRS), FluentValidation, e JWT Bearer.

**Convenções obrigatórias:**
- CQRS via MediatR: `HTTP Request → Controller → MediatR.Send() → ValidationBehavior → Handler → Response`
- Entities extend `Entity` base class (Id, CreatedAt, UpdatedAt)
- `Result<T>` pattern for operation outcomes
- FluentValidation validators auto-registered via `AddValidatorsFromAssembly()`
- Generic repository interfaces in `Application/Abstractions/Persistence/`
- API versioning: URL-based `/api/v{version}/[controller]` (default v1)
- Controllers inject `IMediator` and use `Send()` for all operations
- Features organized as `Features/{FeatureName}/`
- Multi-tenancy: CompanyId filter on all queries + UserCompany mapping
- Database: PostgreSQL with EF Core, code-first migrations
- Error handling: Global exception handler — `ValidationException` → 400, unhandled → 500

### Workflow de implementação

1. Siga a ordem: Domain → Infrastructure → Application → API → Tests
2. Marque cada step como `✅ DONE` no arquivo da história
3. Ensure the code compiles after each layer
4. Do NOT add features beyond the user story
5. Do NOT refactor unrelated code

### Cleanup Rules

- You MAY remove clearly unused code ONLY if: it is in files you touched, provably unused, and removal does not change behavior
- Otherwise note as "optional tech debt"

### Quality Requirements

- Naming: PascalCase for C# types, camelCase for JSON
- Entity patterns: Extend `Entity` base class
- CQRS: All operations through MediatR. Controllers never call repositories directly
- Validation: Domain invariants in entity methods. Request validation via FluentValidation
- Multi-tenancy: All new entities MUST include CompanyId. All queries MUST filter by CompanyId
- Migrations: Safe and consistent
- DI registration: Infrastructure in `Allocore.Infrastructure/DependencyInjection.cs`, Application in `Allocore.Application/DependencyInjection.cs`

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

1. Execute `git diff --stat` para ver os arquivos alterados.
2. Inspecione o código **implementado** (não a story).
3. Adicione uma nova entrada na seção **Backend** de `docs/development-history.md`:
   - No final da seção Backend (ordem cronológica), antes da seção Frontend.
   - Incremente a versão minor.
   - Siga o formato existente: Summary, Changes, Files Created/Modified, Migration Notes, User Story.

---

## Passo 7 — Commit

1. Stage e commit:
   ```bash
   git add [arquivos relevantes]
   git commit -m "feat: implement USXXX [título]

   [descrição breve das mudanças]

   Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
   ```
2. Stage e commit docs:
   ```bash
   git add docs/ && git commit -m "docs: update USXXX story with review and implementation status"
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