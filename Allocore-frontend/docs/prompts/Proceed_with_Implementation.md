# Proceed with Implementation — Frontend

Implemente a user story de frontend step-by-step, seguindo rigorosamente a ordem definida.

## Regras de Implementação

### Antes de Tocar Qualquer Arquivo
1. **Leia** o arquivo existente antes de modificar
2. **Inspecione** padrões do codebase (imports, naming, structure)
3. **Verifique** se o arquivo/componente já existe

### Ordem de Implementação
1. **Types** — TypeScript interfaces/types em `src/types/`
2. **Services** — API calls em `src/app/services/`
3. **Hooks** — Custom hooks em `src/app/hooks/`
4. **Atoms** — Primitive components em `src/app/components/ui/atoms/`
5. **Molecules** — Composed components em `src/app/components/ui/molecules/`
6. **Organisms** — Feature components em `src/app/components/{feature}/`
7. **Pages** — Route pages em `src/app/(auth)/` ou `src/app/(protected)/`
8. **Barrel exports** — Atualizar todos os `index.ts`

### Após Cada Step
- Marque `✅ DONE` na checklist da história
- Execute `npm run type-check` para verificar tipos
- Execute `npm run build` ao fim do último step

### Padrões Obrigatórios
- Use `'use client'` apenas quando necessário (hooks, state, event handlers)
- Use o path alias `@/*` para imports
- Use Zod para validação de formulários
- Use React Hook Form para state de formulários
- Use React Query para server state
- Use Sonner para toasts (`toast.success()`, `toast.error()`)
- Use Lucide React para ícones

### O que NÃO Fazer
- Não adicione features além do escopo da história
- Não refatore código que não foi tocado pela história
- Não adicione dark mode (será feito em história futura)
- Não pule a verificação de type-check entre steps
