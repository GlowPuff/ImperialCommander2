## Traduzir Imperial Commander 2 → PT-BR: Fase 4 - Expansão {EXPANSION}

### 📋 Contexto

Traduzir as missões da expansão **{EXPANSION}** do Imperial Commander 2 para português brasileiro.

### 📚 Recursos Obrigatórios (LER PRIMEIRO)

1. **Instruções do tradutor:** `.agent/agents/imperial-translator.md`
2. **Glossário e preservação:** `WORK/dicionarios/dicionario.json`
3. **Plano de execução:** `WORK/PLAN-traducao-ptbr.md`

### ⚠️ Instrução de Branch (IMPORTANTE)

**O Pull Request gerado deve obrigatoriamente ter como base (target) o branch `jules-translate` e NÃO o `main`.**
*Certifique-se de que este branch existe no repositório antes de iniciar.*

### 🎯 Tarefa

**Passo 1:** Copiar missões para Br
```bash
mkdir -p ImperialCommander2/Assets/Resources/Languages/Br/Missions/{EXPANSION}
```

**Passo 2:** Para cada missão em `SagaMissions/{EXPANSION}/`:
- Copiar para `Languages/Br/Missions/{EXPANSION}/` com sufixo `_BR`
- Traduzir conteúdo

### 📁 Arquivos a Traduzir

Expansão: **{EXPANSION}**
Quantidade: **{COUNT}** arquivos

| Original | Destino |
|----------|---------|
| `SagaMissions/{EXPANSION}/*.json` | `Languages/Br/Missions/{EXPANSION}/*_BR.json` |

### 🔴 Regras Críticas

**Siga estritamente as diretrizes definidas em:** `.agent/agents/imperial-translator.md`

### 📝 Commit

```
feat(i18n): traduzir missões {EXPANSION} para PT-BR (Fase 4)

- {COUNT} missões traduzidas
- Aplicado glossário oficial
```

### 🏷️ Labels

`translation`, `pt-br`, `fase-4`, `missions`, `{expansion-lower}`

---

## 📋 Expansões Disponíveis

Substituir `{EXPANSION}` e `{COUNT}`:

| Expansão | Arquivos | Prioridade |
|----------|:--------:|:----------:|
| Jabba | 16 | P1 |
| Hoth | 16 | P1 |
| Empire | 16 | P1 |
| Lothal | 6 | P2 |
| Twin | 6 | P2 |
| Bespin | 6 | P2 |
| Other | 40 | P3 |
