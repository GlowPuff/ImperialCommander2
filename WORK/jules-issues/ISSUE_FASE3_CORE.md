## Traduzir Imperial Commander 2 → PT-BR: Fase 3 - Missões Core

### 📋 Contexto

Traduzir as 32 missões Core do Imperial Commander 2 para português brasileiro.

### 📚 Recursos Obrigatórios (LER PRIMEIRO)

1. **Instruções do tradutor:** `.agent/agents/imperial-translator.md`
2. **Glossário e preservação:** `WORK/dicionarios/dicionario.json`
3. **Plano de execução:** `WORK/PLAN-traducao-ptbr.md`

### ⚠️ Instrução de Branch (IMPORTANTE)

**O Pull Request gerado deve obrigatoriamente ter como base (target) o branch `jules-translate` e NÃO o `main`.**
*Certifique-se de que este branch existe no repositório antes de iniciar.*

### 🎯 Tarefa

**Passo 1:** Copiar missões para Br (se não existir)
```bash
mkdir -p ImperialCommander2/Assets/Resources/Languages/Br/Missions/Core
```

**Passo 2:** Para cada missão em `SagaMissions/Core/`:
- Copiar para `Languages/Br/Missions/Core/` com sufixo `_BR`
- Traduzir conteúdo

### 📁 Arquivos a Traduzir

| Lote | Missões | 
|------|---------|
| 1 | CORE1_BR.json a CORE8_BR.json |
| 2 | CORE9_BR.json a CORE16_BR.json |
| 3 | CORE17_BR.json a CORE24_BR.json |
| 4 | CORE25_BR.json a CORE32_BR.json |

### 🔑 Campos a Traduzir

```
missionDescription, additionalMissionInfo, startingObjective, missionInfo,
theText, eventText, choiceText, buttonText, customInstructions,
descriptionText, bonusText, imperialRewardText, rebelsRewardText,
effects, eventFlavor, content, helpText, instruction, subname, text
```

### 🔄 Metadados a Atualizar

```json
{
  "languageID": "Portuguese Brazilian (BR)",
  "saveDate": "4/26/2024"
}
```

### 🔴 Regras Críticas

**Siga estritamente as diretrizes definidas em:** `.agent/agents/imperial-translator.md`

### 📝 Commit

```
feat(i18n): traduzir missões Core para PT-BR (Fase 3)

- 32 missões: CORE1_BR a CORE32_BR
- Aplicado glossário oficial
- Variáveis e tags preservadas
```

### 🏷️ Labels

`translation`, `pt-br`, `fase-3`, `missions`, `core`
