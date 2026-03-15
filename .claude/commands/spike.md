Spike técnico: pesquisa profunda (negócio + técnica) e validação prática de uma hipótese antes de comprometer com a implementação. Busca fontes externas, constrói conhecimento, testa com scripts descartáveis e documenta o resultado com veredicto.

## Objetivo

Este comando é a fase de **"pesquisa e prova antes de implementar"**. Use `/spike` quando:
- Precisa entender um domínio de negócio antes de modelar
- Precisa testar uma API externa ou serviço de terceiros
- Quer validar se uma abordagem técnica funciona na prática
- Tem dúvida sobre performance, compatibilidade ou viabilidade
- Precisa comparar alternativas (lib A vs lib B, API X vs API Y)
- A história tem risco técnico ou de negócio que não se resolve só com análise

**O spike tem duas entregas: conhecimento e evidência.** O código é descartável — o que fica é o documento com o que foi aprendido.

---

## Passo 1 — Definir a Hipótese

1. Se $ARGUMENTS for fornecido, use como tema do spike.
2. Senão, pergunte ao usuário: **"O que você quer validar?"**
3. Formule a hipótese como uma pergunta clara e testável:
   - "A API do Serper.dev retorna preços em BRL para produtos brasileiros?"
   - "O Hangfire consegue agendar jobs recorrentes com cron no .NET 9?"
   - "Qual o modelo de precificação mais comum em apps de gestão de academia no Brasil?"
4. Classifique o spike:
   - **Técnico**: validar API, lib, performance, integração
   - **Negócio**: entender domínio, validar modelo, pesquisar mercado/concorrência
   - **Híbrido**: os dois (ex: "qual API de NF-e tem melhor custo-benefício para nosso caso?")
5. Liste as **perguntas-chave** que o spike precisa responder (3-7 perguntas).
6. Defina o **critério de sucesso**: o que precisa ser verdade para o veredicto ser PROCEED.

Apresente ao usuário:

```
## Spike: [Título]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

**Tipo**: Técnico / Negócio / Híbrido
**Hipótese**: [pergunta clara]

**Perguntas-chave**:
1. [pergunta]
2. [pergunta]
...

**Critério de sucesso**: [o que precisa ser verdade]
```

Aguarde confirmação do usuário antes de prosseguir.

---

## Passo 2 — Pesquisa Profunda

Este é o passo mais importante do spike. **Pesquise ANTES de escrever código.** O objetivo é construir um entendimento sólido do problema a partir de fontes externas reais.

Lance sub-agentes em paralelo para maximizar a cobertura da pesquisa:

### 2a — Pesquisa de Negócio (MANDATÓRIA)

Use WebSearch e WebFetch para buscar fontes externas sobre o domínio:

- **Como o mercado resolve isso?** Pesquise como empresas, apps ou serviços similares abordam o mesmo problema. Busque por cases, artigos, análises de mercado.
- **Existe padrão de indústria?** Normas, convenções, boas práticas estabelecidas no setor (ex: padrões de NF-e, protocolos de saúde, convenções de e-commerce).
- **Quem são os players?** Identifique concorrentes, ferramentas ou serviços que já resolvem parte do problema. Analise como eles fazem.
- **Quais as armadilhas conhecidas?** Busque por post-mortems, erros comuns, limitações que outros encontraram ao implementar algo similar.
- **Regulação e compliance** (se aplicável): leis, regulamentações, requisitos legais que impactem a implementação (ex: LGPD, regras da Receita Federal, normas ANVISA).

Fontes recomendadas:
- Artigos técnicos e blog posts de empresas que resolveram o mesmo problema
- Documentação oficial de padrões da indústria
- Fóruns e discussões (Stack Overflow, Reddit, comunidades específicas)
- Repositórios open-source que implementam soluções similares
- Documentação governamental/regulatória quando relevante

### 2b — Pesquisa Técnica (MANDATÓRIA)

Use WebSearch e WebFetch para buscar fontes externas sobre a solução técnica:

- **Documentação oficial**: leia a documentação da API, lib ou serviço em questão. Não assuma — leia.
- **Comparação de alternativas**: pesquise pelo menos 2-3 alternativas. Para cada uma:
  - Pricing e limites (free tier, rate limits, quotas)
  - Qualidade da documentação e suporte
  - Maturidade e adoção (GitHub stars, npm downloads, NuGet downloads)
  - Última atualização e manutenção ativa
  - Compatibilidade com a stack do projeto
- **Benchmarks e performance**: busque benchmarks existentes, relatos de performance em produção, limitações conhecidas.
- **Exemplos reais de uso**: busque exemplos de código, tutoriais, repos que usam a tecnologia no mesmo contexto.
- **Problemas conhecidos**: busque issues no GitHub, bugs reportados, limitações documentadas, breaking changes recentes.
- **Segurança**: verifique se há vulnerabilidades conhecidas, práticas de segurança recomendadas, auditorias.

### 2c — Pesquisa na Codebase

Investigue o que já existe no projeto que pode ser relevante:

- Patterns existentes que o spike pode reutilizar
- Integrações similares já feitas (como foram implementadas, o que funcionou)
- Credentials, configs, infraestrutura já disponível
- Código que seria impactado pela implementação futura

### 2d — Síntese da Pesquisa

Antes de partir para o código, apresente um **resumo estruturado** da pesquisa:

```
## Pesquisa: [Título do Spike]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

### Contexto de Negócio
[O que o mercado faz, padrões da indústria, regulações]

### Panorama Técnico
| Alternativa | Pricing | Limites | Maturidade | Compatibilidade |
|-------------|---------|---------|------------|-----------------|
| [opção A]   | ...     | ...     | ...        | ...             |
| [opção B]   | ...     | ...     | ...        | ...             |

### Riscos Identificados na Pesquisa
- [risco 1 — fonte]
- [risco 2 — fonte]

### Recomendação Preliminar
[Qual alternativa testar primeiro e por quê]

### Fontes Consultadas
- [link 1 — descrição]
- [link 2 — descrição]
- ...
```

Aguarde feedback do usuário antes de prosseguir para o teste prático.

---

## Passo 3 — Script de Teste

Crie um script descartável em `scripts/` para validar a hipótese na prática.

> **Nota:** Para spikes puramente de negócio (sem componente técnico para testar), pule este passo e vá direto para o Passo 4 — a pesquisa do Passo 2 já é a evidência.

### Regras do script de spike
- **Localização**: `scripts/spike-[nome-descritivo].js` (ou `.ts`, `.py`, `.cs` conforme o contexto)
- **Standalone**: o script deve rodar isolado, sem depender do projeto compilado
- **Sem polish**: código pragmático, console.log/print direto, sem error handling elaborado
- **Dados reais**: use dados reais ou realistas do projeto, não dados fictícios
- **Mensurável**: o output deve responder diretamente as perguntas-chave do Passo 1
- **Credenciais**: se precisar de API key, use variável de ambiente ou arquivo local (NUNCA hardcode)

### Estrutura recomendada do script
```
1. Setup (imports, config, credentials)
2. Test cases (3-5 cenários reais do projeto)
3. Execução dos testes
4. Output estruturado com resultados
5. Comparativo (se testando múltiplas alternativas)
```

Após escrever o script, execute-o e colete os resultados.

### Se o script falhar
- Não tente consertar infinitamente — 2 tentativas máximo
- Se falhar por problema de infra (rede, auth), documente como blocker
- Se falhar por abordagem errada, pivotar para alternativa (volte ao Passo 2b)

---

## Passo 4 — Análise de Resultados

Cruze os dados da pesquisa (Passo 2) com os resultados práticos (Passo 3) para responder cada pergunta-chave:

| Pergunta | Resposta | Evidência (pesquisa) | Evidência (teste) |
|----------|----------|---------------------|-------------------|
| [pergunta 1] | SIM/NÃO/PARCIAL | [o que a pesquisa mostrou] | [o que o teste confirmou] |
| [pergunta 2] | SIM/NÃO/PARCIAL | [o que a pesquisa mostrou] | [o que o teste confirmou] |
| ... | ... | ... | ... |

### O que funcionou bem
- [lista objetiva com dados]

### O que precisa de atenção
- [problemas encontrados, limitações, edge cases]

### O que a pesquisa revelou que o teste não cobriu
- [insights de negócio, regulações, limitações de longo prazo]

### Riscos identificados
- [riscos técnicos, de custo, de manutenção, de negócio]

---

## Passo 5 — Veredicto

Emita **um** dos veredictos:

### PROCEED
A hipótese foi validada. A abordagem funciona e o risco é aceitável.
- Liste a estratégia recomendada de implementação
- Inclua detalhes técnicos relevantes (endpoints, configs, limites)
- Estime impacto no custo/performance se aplicável
- Indique qual user story ou feature este spike habilita
- Documente decisões de arquitetura que o spike informou

### PIVOT
A hipótese foi parcialmente validada. Funciona, mas não como esperado.
- Explique o que funcionou e o que não funcionou
- Proponha abordagem alternativa baseada nos achados
- Sugira um novo spike se necessário

### ABORT
A hipótese foi refutada. A abordagem não é viável.
- Explique por que não funciona (com dados da pesquisa E dos testes)
- Liste alternativas se existirem
- Se não há alternativa viável, recomende descartar a feature/ideia

---

## Passo 6 — Documentação

Crie o documento do spike em `docs/spikes/`:

### Nome do arquivo
`docs/spikes/spike-[nome-descritivo].md`

### Template do documento

```markdown
# Spike: [Título]

**Date:** [YYYY-MM-DD]
**Type:** Technical / Business / Hybrid
**Status:** COMPLETED
**Verdict:** [PROCEED/PIVOT/ABORT] — [resumo de uma linha]

---

## Summary

[1-2 parágrafos descrevendo o que foi pesquisado, testado e o resultado principal]

## Research

### Business Context
[O que a pesquisa de negócio revelou: mercado, concorrência, padrões, regulações]

### Technical Landscape
[Alternativas avaliadas, comparativo, maturidade, compatibilidade]

| Alternative | Pricing | Limits | Maturity | Verdict |
|-------------|---------|--------|----------|---------|
| ... | ... | ... | ... | ... |

### Sources
- [link — descrição da fonte e o que foi extraído]
- [link — descrição da fonte e o que foi extraído]
- ...

## Test Results

[Tabela com resultados dos testes práticos — omitir se spike puramente de negócio]

## Key Questions Answered

| Question | Answer | Evidence |
|----------|--------|----------|
| ... | ... | ... |

## Observations

### What works well
- ...

### What needs attention
- ...

## Recommendation

**[VEREDICTO]**

[Estratégia recomendada com detalhes técnicos e de negócio]

### Implementation Notes
[Detalhes técnicos relevantes para a implementação futura: endpoints, configs, limites, patterns recomendados]

## Files
- Script: `scripts/spike-[nome].js` (temporary spike script)
- [outros arquivos criados]
```

Crie a pasta `docs/spikes/` se não existir.

---

## Passo 7 — Commit & Cleanup

### O que commitar
- O documento do spike (`docs/spikes/spike-*.md`)
- Scripts de spike (`scripts/spike-*`) — para referência futura

### Commit
```bash
git add docs/spikes/ scripts/spike-*
git commit -m "spike: [título do spike]

[veredicto em uma linha]

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

> Scripts de spike são commitados para referência, mas NÃO devem ser importados ou referenciados pelo código do projeto.

---

## Conclusão

Apresente o resumo:

```
Spike concluído: [Título]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Tipo:         Técnico / Negócio / Híbrido
Hipótese:     [pergunta original]
Veredicto:    PROCEED / PIVOT / ABORT
Pesquisa:     X fontes consultadas
Perguntas:    X/Y respondidas
Documento:    docs/spikes/spike-[nome].md
Script:       scripts/spike-[nome].js (ou N/A)
Commit:       [hash curto]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Se PROCEED]: Próximo: criar/atualizar user story para implementação
[Se PIVOT]:   Próximo: novo spike com abordagem revisada
[Se ABORT]:   Próximo: descartar ou buscar alternativa
```

---

## Regras

- **Pesquisa é obrigatória** — todo spike DEVE incluir pesquisa externa (web search). Não pule direto pro código. Entender o problema é mais valioso que testar a solução.
- **Fontes reais** — cite links, documentação, artigos. Conhecimento sem fonte é opinião, não pesquisa.
- **Negócio + técnica** — mesmo spikes técnicos devem considerar o contexto de negócio (custo, mercado, regulação). Mesmo spikes de negócio devem considerar viabilidade técnica.
- **Spike NÃO é implementação** — o objetivo é aprender, não entregar. Código descartável é o esperado.
- **Dados reais** — teste com dados do projeto, não com "hello world". O spike só vale se os dados são representativos.
- **Time-boxed** — se o spike está demorando demais, o veredicto provavelmente é PIVOT ou ABORT. Não force.
- **Documente TUDO** — o valor do spike é o conhecimento, não o código. Se não documentou, não fez spike.
- **Sem vergonha de ABORT** — descobrir que algo não funciona é um resultado válido e valioso. Evita semanas de implementação desperdiçada.
- **Credenciais seguras** — nunca hardcode API keys. Use env vars ou arquivos locais no .gitignore.
