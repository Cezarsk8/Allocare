Referência do workflow de desenvolvimento do Allocare. Use os comandos individuais para executar.

## Sistema de 3 Fases

O desenvolvimento de cada história segue 3 fases sequenciais, cada uma com seu próprio comando:

```
/dev-be  →  /dev-fe  →  /ship
```

### `/dev-be` — Fase Backend
Escolhe história → Cria branch → Review → Implementa → Testa → Documenta → Commita

### `/dev-fe` — Fase Frontend
Lê contratos BE → Escreve FE story → Review → Implementa → Testa → Documenta → Commita

### `/ship` — Merge & Release
Merge para main → Arquiva histórias → Push dos 3 repos → Deleta feature branches

---

## Fluxo Normal (full-stack)

```
1. /dev-be              → Backend completo na feature branch
2. /dev-fe              → Frontend completo na mesma branch
3. /ship                → Merge, archive, push
```

## Fluxo Backend-Only

Para histórias sem impacto no frontend (ex: limpeza interna, refactoring):

```
1. /dev-be              → Backend completo na feature branch
2. /ship                → Merge, archive, push (pula frontend)
```

---

## Outros Comandos

| Comando | Uso |
|---------|-----|
| `/roadmap` | Analisa e reordena o roadmap |
| `/start` | Roda ambos os projetos localmente |
