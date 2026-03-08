# Deep Review — Frontend Story

Execute uma revisão completa da user story de frontend **antes** de implementar.

## Checklist de Review

### 1. Consistência com Backend
- [ ] Todos os endpoints referenciados existem no backend implementado?
- [ ] Os tipos (request/response) correspondem aos DTOs do backend?
- [ ] As rotas de API estão corretas (method, path, auth)?

### 2. Consistência com Codebase Frontend
- [ ] Os imports usam o path alias `@/*`?
- [ ] Os nomes de arquivos seguem o padrão existente?
- [ ] Os componentes estão na camada correta (atom/molecule/organism)?
- [ ] Os barrel exports estão atualizados?

### 3. Padrões do Projeto
- [ ] Client components têm `'use client'`?
- [ ] Server components NÃO têm `'use client'`?
- [ ] Hooks seguem o padrão `useXxx`?
- [ ] Services usam Axios via `apiClient`?
- [ ] Validações usam Zod schemas?

### 4. UX & Responsividade
- [ ] A história define comportamento mobile?
- [ ] Os estados de loading/error/empty estão definidos?
- [ ] As mensagens de erro são user-friendly?
- [ ] Os toasts usam Sonner?

### 5. Edge Cases
- [ ] Token expirado / não autenticado?
- [ ] Dados vazios (listas, campos opcionais)?
- [ ] Erros de validação (frontend + backend)?
- [ ] Permissões (admin vs user)?

## Veredicto

- ✅ **Ready** — história pode ser implementada como está
- ⚠️ **Minor corrections** — aplicar correções de baixo risco e continuar
- ❌ **Blocking issues** — problemas que impedem implementação

## Ações

1. Corrija automaticamente inconsistências de baixo risco no arquivo da história
2. Liste problemas encontrados com severidade
3. Emita o veredicto final
