## Traduzir Imperial Commander 2 → PT-BR: Fase 0 - Arquivos Base

### 📋 Contexto

Traduzir os arquivos base do sistema do Imperial Commander 2 (companion app para Star Wars: Imperial Assault) de inglês para português brasileiro.

### 📚 Recursos Obrigatórios (LER PRIMEIRO)

Antes de começar, leia e siga rigorosamente:

1. **Instruções do tradutor:** `.agent/agents/imperial-translator.md`
2. **Glossário e preservação:** `WORK/dicionarios/dicionario.json`
3. **Plano de execução:** `WORK/PLAN-traducao-ptbr.md`

### ⚠️ Instrução de Branch (IMPORTANTE)

**O Pull Request gerado deve obrigatoriamente ter como base (target) o branch `jules-translate` e NÃO o `main`.**
*Certifique-se de que este branch existe no repositório antes de iniciar.*

### 🎯 Tarefa

**Passo 1:** Copiar diretório base (se não existir)
```bash
cp -r ImperialCommander2/Assets/Resources/Languages/En ImperialCommander2/Assets/Resources/Languages/Br
```

**Passo 2:** Traduzir os 16 arquivos listados abaixo

### 📁 Arquivos a Traduzir

| # | Arquivo | Notas |
|---|---------|-------|
| 1 | `Languages/Br/DeploymentGroups/allies.json` | Traduzir descrições |
| 2 | `Languages/Br/DeploymentGroups/enemies.json` | Traduzir descrições |
| 3 | `Languages/Br/DeploymentGroups/villains.json` | Traduzir descrições |
| 4 | `Languages/Br/MissionCardText/core.json` | Textos de cartas |
| 5 | `Languages/Br/MissionCardText/bespin.json` | Textos de cartas |
| 6 | `Languages/Br/MissionCardText/empire.json` | Textos de cartas |
| 7 | `Languages/Br/MissionCardText/hoth.json` | Textos de cartas |
| 8 | `Languages/Br/MissionCardText/jabba.json` | Textos de cartas |
| 9 | `Languages/Br/MissionCardText/lothal.json` | Textos de cartas |
| 10 | `Languages/Br/MissionCardText/other.json` | Textos de cartas |
| 11 | `Languages/Br/MissionCardText/twin.json` | Textos de cartas |
| 12 | `Languages/Br/bonuseffects.json` | ⚠️ VER REGRA ESPECIAL |
| 13 | `Languages/Br/events.json` | Traduzir descrições |
| 14 | `Languages/Br/help.json` | Traduzir textos de ajuda |
| 15 | `Languages/Br/instructions.json` | Traduzir instruções |
| 16 | `Languages/Br/ui.json` | Traduzir interface |

### 🔴 Regras Críticas

**Siga estritamente as diretrizes definidas em:** `.agent/agents/imperial-translator.md`

#### Regra Especial: bonuseffects.json

Manter o nome da habilidade em inglês antes dos dois pontos:
```
✅ "CHARGING UP: Depois que esta figura atacar..."
❌ "CARREGANDO: Depois que esta figura atacar..."
```

### ✅ Validação

Após cada arquivo:
1. Verificar JSON válido: `python -m json.tool <arquivo>`
2. Verificar variáveis preservadas
3. Verificar tags preservadas

### 📝 Commit

Criar commit com mensagem:
```
feat(i18n): traduzir arquivos base do sistema para PT-BR (Fase 0)

- DeploymentGroups: allies, enemies, villains
- MissionCardText: 8 arquivos
- Sistema: bonuseffects, events, help, instructions, ui
```

### 🏷️ Labels

`translation`, `pt-br`, `fase-0`, `priority-high`
