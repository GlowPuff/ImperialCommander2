## Traduzir Imperial Commander 2 → PT-BR: Fase 2 - MissionText

### 📋 Contexto

Traduzir arquivo de texto da missão: `{filename}`.

### 📚 Recursos Obrigatórios (LER PRIMEIRO)
1. **Instruções do tradutor:** `.agent/agents/imperial-translator.md`
2. **Glossário e preservação:** `WORK/dicionarios/dicionario.json`
3. **Plano de execução:** `WORK/PLAN-traducao-ptbr.md`

### ⚠️ Instrução de Branch (IMPORTANTE)
**O Pull Request gerado deve obrigatoriamente ter como base (target) o branch `jules-translate` e NÃO o `main`.**

### 🎯 Tarefa
**Passo 1:** Garantir diretório de destino
```bash
mkdir -p ImperialCommander2/Assets/Resources/Languages/Br/MissionText
```

**Passo 2:** Traduzir arquivo
- Origem: `ImperialCommander2/Assets/Resources/Languages/En/MissionText/{filename}`
- Destino: `ImperialCommander2/Assets/Resources/Languages/Br/MissionText/{filename}`
- Manter encoding UTF-8.

### 🔴 Regras Críticas
**Siga estritamente as diretrizes definidas em:** `.agent/agents/imperial-translator.md`

### 📝 Commit
```
feat(i18n): traduzir {filename} para PT-BR (Fase 2)
```

### 🏷️ Labels
`translation`, `pt-br`, `fase-2`, `mission-text`
