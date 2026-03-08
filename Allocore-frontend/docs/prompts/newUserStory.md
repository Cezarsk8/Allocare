# Template — Frontend User Story

Use este template para escrever user stories de frontend correspondentes a histórias backend implementadas.

---

```markdown
# USFWXXX – [Título da Feature] — Frontend

> **Project:** Allocore-frontend
> **Backend Story:** USXXX
> **Status:** Pending

---

**Priority:** [Critical / High / Medium / Low]
**Dependencies:** [USFWXXX — descrição]
**Estimated effort:** [1 session / 2 sessions]

## Description

As a **[role]**, I want [goal] so that [benefit].

## UX Goals

- [Goal 1]
- [Goal 2]

---

## Step 0: Responsive Baseline (MANDATORY)

- [ ] Define mobile behavior for all components in this story
- [ ] Ensure all new components work on mobile (320px+), tablet (768px+), desktop (1024px+)

## Step 1: Types

- [ ] Create/update types in `src/types/`
- [ ] Types must match backend DTOs exactly

## Step 2: Services

- [ ] Create API service functions in `src/app/services/`
- [ ] Use `apiClient` (Axios instance)

## Step 3: Hooks

- [ ] Create custom hooks in `src/app/hooks/`
- [ ] Use React Query for server state

## Step 4: Components

### 4.1 Atoms
- [ ] Create atoms in `src/app/components/ui/atoms/`

### 4.2 Molecules
- [ ] Create molecules in `src/app/components/ui/molecules/`

### 4.3 Organisms
- [ ] Create organisms in `src/app/components/{feature}/`

## Step 5: Pages

- [ ] Create route pages

## Step 6: Barrel Exports

- [ ] Update all `index.ts` barrel exports

---

## Technical Details

### Backend Endpoints Used

| Action | Method | Endpoint | Auth | Response |
|--------|--------|----------|------|----------|
| ... | ... | ... | ... | ... |

### Project Structure — Affected Files

| Layer | File | Change |
|-------|------|--------|
| ... | ... | ... |

---

## Acceptance Criteria

- [ ] [Criterion 1]
- [ ] [Criterion 2]
- [ ] `npm run type-check` passes
- [ ] `npm run build` passes

---

## What is explicitly NOT changing?

- [Item 1]
```
