## Traduzir Imperial Commander 2 → PT-BR: Fase 8 - Expansão Twin

### 📋 Contexto
Traduzir as **6 missões** da expansão **Twin Shadows** do Imperial Commander 2 para português brasileiro.

### 📚 Recursos Obrigatórios (LER PRIMEIRO)
1. **Instruções do tradutor:** `.agent/agents/imperial-translator.md`
2. **Glossário e preservação:** `WORK/dicionarios/dicionario.json`
3. **Plano de execução:** `WORK/PLAN-traducao-ptbr.md`

### ⚠️ Instrução de Branch (IMPORTANTE)
**O Pull Request gerado deve obrigatoriamente ter como base (target) o branch `jules-translate` e NÃO o `main`.**

### 🎯 Tarefa
**Passo 1:** Copiar missões para Br
```bash
mkdir -p ImperialCommander2/Assets/Resources/Languages/Br/Missions/Twin
```

**Passo 2:** Para cada missão em `SagaMissions/Twin/`:
- Copiar para `Languages/Br/Missions/Twin/` com sufixo `_BR`
- Traduzir conteúdo respeitando validade JSON e regras abaixo.

### 🔴 Regras Críticas
**Siga estritamente as diretrizes definidas em:** `.agent/agents/imperial-translator.md`

### 📝 Commit
```
feat(i18n): traduzir missoes Twin para PT-BR (Fase 8)
```

### 🏷️ Labels
`translation`, `pt-br`, `fase-8`, `missions`, `twin`
