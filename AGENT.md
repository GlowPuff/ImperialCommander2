# Jules Agent Workflow: Imperial Translator

Este documento define o ciclo de vida completo da automação do agente Jules no projeto Imperial Commander 2.

## 🔄 Visão Geral do Ciclo

```mermaid
graph TD
    A[Script: generate_issues.py] -->|Cria Issue + Assign| B(GitHub Issue)
    B -->|Trigger: Assigned| C[Workflow: jules-worker.yml]
    C -->|Jules Traduz + Commit| D[Novo Pull Request]
    D -->|Trigger: PR Open| E[Workflow: jules-reviewer.yml]
    E -->|Jules Valida + Corrige| E
    E -->|Tag: [JULES-READY]| F[Auto-Merge]
    F -->|Merge| G[Branch: jules-translate]
```

---

## 🎭 Papéis do Agente

### 1. Jules Worker (Tradutor)
*   **Workflow:** `.github/workflows/jules-worker.yml`
*   **Gatilho:** Issue com label `jules`.
*   **Input:** Descrição da Issue (que contém instruções e links para documentação).
*   **Ação:**
    *   Lê a issue.
    *   Traduz o arquivo solicitado.
    *   Cria um commit/PR.

### 2. Jules Reviewer (Corretor)
*   **Workflow:** `.github/workflows/jules-reviewer.yml`
*   **Gatilho:** Mensagem de `pull_request` (abertura ou novos commits).
*   **Input:** Código do PR atual.
*   **Responsabilidade:**
    *   Atuar como *Gatekeeper* de qualidade.
    *   Executar scripts de validação (`WORK/scripts/validate-json.sh`).
    *   Consultar glossário e regras em `.agent/agents/imperial-translator.md`.
*   **Comportamento de Loop:**
    *   **Se encontrar erro:** Corrige o arquivo, faz commit. (Isso re-aciona a Action automaticamente).
    *   **Se estiver perfeito:** Faz commit com a tag `[JULES-READY]`.

---

## 🤖 Automação (GitHub Actions)

### 1. Tradução (`jules-translator.yml`)
Ocorre quando você roda os scripts python.
*   Você -> Script -> Issue (Assign `jules-imperial`) -> **Action Dispara** -> Jules Code.

### 2. Revisão (`jules-full-automation.yml`)
Ocorre quando o Jules (ou você) abre um PR.
*   PR Aberto -> **Action Dispara** -> Validação -> Correção/Merge.

---

## 🛡️ Diretrizes Globais

1.  **Fonte da Verdade:** O arquivo `.agent/agents/imperial-translator.md` sobrepõe qualquer alucinação.
2.  **Tokens:** O segredo `JULES_PAT` é fundamental para conectar o passo 1 ao passo 2. Sem ele, o PR criado pelo Jules não dispararia a revisão (GitHub impede loops de actions baseadas em GITHUB_TOKEN padrão).

---

## Checklist para Criar Novas Issues

Ao criar issues manualmente ou via script, garanta que:
- [ ] O branch alvo é `jules-translate`.
- [ ] O assignee é `jules-imperial`.
