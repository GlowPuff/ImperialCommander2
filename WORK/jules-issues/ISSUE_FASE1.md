## Traduzir Imperial Commander 2 → PT-BR: Fase 1 - Tutoriais

### 📋 Contexto

Traduzir os 3 tutoriais do Imperial Commander 2 de inglês para português brasileiro.

### 📚 Recursos Obrigatórios (LER PRIMEIRO)

1. **Instruções do tradutor:** `.agent/agents/imperial-translator.md`
2. **Glossário e preservação:** `WORK/dicionarios/dicionario.json`
3. **Plano de execução:** `WORK/PLAN-traducao-ptbr.md`

### ⚠️ Instrução de Branch (IMPORTANTE)

**O Pull Request gerado deve obrigatoriamente ter como base (target) o branch `jules-translate` e NÃO o `main`.**
*Certifique-se de que este branch existe no repositório antes de iniciar.*

### 🎯 Tarefa

**Passo 1:** Copiar tutoriais para Br (se não existir)
```bash
cp -r ImperialCommander2/Assets/Resources/SagaTutorials/En ImperialCommander2/Assets/Resources/SagaTutorials/Br
```

**Passo 2:** Traduzir os 3 arquivos

### 📁 Arquivos a Traduzir

| # | Arquivo |
|---|---------|
| 1 | `SagaTutorials/Br/TUTORIAL01.json` |
| 2 | `SagaTutorials/Br/TUTORIAL02.json` |
| 3 | `SagaTutorials/Br/TUTORIAL03.json` |

### 🔑 Campos a Traduzir

```
missionDescription, additionalMissionInfo, missionInfo,
theText, eventText, buttonText, choiceText
```

### 🔄 Metadados a Atualizar

Em cada arquivo, atualizar:
```json
{
  "languageID": "Portuguese Brazilian (BR)",
  "saveDate": "4/26/2024"
}
```

### 🔴 Regras Críticas

**Siga estritamente as diretrizes definidas em:** `.agent/agents/imperial-translator.md`

### ✅ Validação

Para cada arquivo:
```bash
python -m json.tool <arquivo> > /dev/null && echo "✅ JSON válido"
grep '"languageID"' <arquivo>  # Deve mostrar "Portuguese Brazilian (BR)"
```

### 📝 Commit

```
feat(i18n): traduzir tutoriais para PT-BR (Fase 1)

- TUTORIAL01.json
- TUTORIAL02.json  
- TUTORIAL03.json
```

### 🏷️ Labels

`translation`, `pt-br`, `fase-1`, `tutorials`
