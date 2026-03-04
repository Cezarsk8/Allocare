Analise todas as user stories futuras e otimize o roadmap, reordenando por importância e relevância.

## Passos obrigatórios

1. **Leia o estado atual do projeto**:
   - Roadmap atual: `docs/roadmap.md`
   - Development History backend: `Allocore-backend/Docs/System/DevelopmentHistory.md` (últimas versões)
   - Arquitetura backend: `Allocore-backend/Docs/System/ProjectStructure.md`
   - Product Vision: `Allocore-backend/Docs/System/ProductVision.md`

2. **Leia TODAS as user stories futuras**:
   - Liste todos os arquivos em `docs/user-stories/future/` (incluindo subpastas)
   - Leia cada arquivo de história na íntegra para entender escopo, dependências, esforço e impacto

3. **Identifique histórias que já foram implementadas**:
   - Compare as histórias futuras com o Development History
   - Se uma história já foi implementada, mova-a para `docs/user-stories/history/backend/[domain]/` ou `docs/user-stories/history/frontend/[domain]/`
   - Remova-a do roadmap

4. **Avalie cada história restante** nos 5 critérios (score 1-5):

   | Critério | Peso | Descrição |
   |----------|------|-----------|
   | **Risk Reduction** | 25% | Corrige falhas de segurança, bugs ou dívida técnica |
   | **User Value** | 25% | Impacto direto na experiência do usuário final |
   | **Effort Efficiency** | 15% | Razão valor/esforço (pouco esforço, grande impacto = alto) |
   | **Unblocking Power** | 15% | Habilita ou simplifica histórias futuras |
   | **Differentiation** | 20% | Torna o Allocare único; gera recomendação boca-a-boca |

5. **Reorganize os Tiers** considerando:
   - Dependências entre histórias (não colocar uma história antes de suas dependências)
   - Agrupamento temático (histórias que se complementam ficam no mesmo Tier)
   - Balanceamento de esforço por Tier (evitar Tiers gigantes ou microscópicos)
   - Histórias bloqueadas por dependências externas ficam no último Tier

6. **Reescreva o arquivo `docs/roadmap.md`** mantendo a mesma estrutura:
   - Tabela de inventário atualizada
   - Scoring matrix atualizada
   - Tiers reordenados com PM Commentary para cada decisão
   - Dependency graph atualizado
   - Quarterly roadmap suggestion atualizada (a partir da data atual)
   - Seção "What Changed" explicando as diferenças vs. versão anterior
   - Atualize a data de "Last updated" e "Previous version"

7. **Apresente um resumo** das mudanças:
   - Histórias adicionadas/removidas/movidas
   - Mudanças de Tier
   - Nova ordem de prioridade top 5
   - Total de esforço atualizado

## Regras

- **Seja opinionado**: O roadmap deve refletir decisões de produto claras, não apenas uma lista neutra.
- **PM perspective**: Priorize como um Product Manager — engagement > polish, security > features.
- **Não invente histórias**: Só reorganize o que já existe. Se identificar gaps, mencione na seção de commentary.
- **Respeite dependências**: Nunca coloque uma história antes de suas dependências no ordering.
- **Marque histórias implementadas**: Se uma história já foi feita, remova do backlog e atualize contadores.

Se $ARGUMENTS for fornecido, use como contexto adicional (ex: `/roadmap focus on engagement` prioriza engajamento).
