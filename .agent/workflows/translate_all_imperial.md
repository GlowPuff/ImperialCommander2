---
description: Orquestra tradução completa do Imperial Commander 2 seguindo o PLAN, com validação e relatório final
---

# /translate_all_imperial - Tradução Completa Orquestrada

$ARGUMENTS (opcional: fase específica, ex: "fase0", "fase1", "all")

---

## 🎯 OBJETIVO

Executar a tradução completa do projeto Imperial Commander 2 seguindo o PLAN-traducao-ptbr.md, validando cada arquivo e gerando relatório final.

---

## 🔴 REGRAS CRÍTICAS

1. **Seguir ordem do PLAN** — Fase 0 → Fase 1 → Fase 2 → Fase 3 → Fase 4
2. **Validar cada arquivo** após tradução
3. **Parar em caso de erro crítico** (JSON inválido)
4. **NÃO CRIAR SCRIPTS** — Usar apenas os scripts existentes em `WORK/scripts/`
5. **Arquivos temporários** — Devem ficar em `WORK/scripts/traduzindo/` e ser excluídos após uso

---

## Passo 0: Carregar Plano e Recursos

Leia os arquivos de referência:

```
WORK/PLAN-traducao-ptbr.md           # Plano de execução
WORK/PRD.md                          # Especificações
WORK/dicionarios/dicionario.json     # Glossário
.agent/agents/imperial-translator.md # Instruções do agente
```

---

## Passo 1: Preparação (Se ainda não feito)

// turbo
```bash
# Verificar se diretório Br existe
ls -la ImperialCommander2/Assets/Resources/Languages/Br 2>/dev/null || echo "⚠️ Diretório Br não existe - executar cópia primeiro"
```

Se não existir, copiar:
```bash
cp -r ImperialCommander2/Assets/Resources/Languages/En ImperialCommander2/Assets/Resources/Languages/Br
```

---

## Passo 2: Fase 0 - Arquivos Base (16 arquivos)

### 2.1 DeploymentGroups (3 arquivos)
> **OBRIGATÓRIO:** Usar `/translate_imperial` para garantir integridade.

| # | Arquivo | Comando | Status |
|---|---------|---------|--------|
| 0.1 | `Languages/Br/DeploymentGroups/allies.json` | `/translate_imperial` | [ ] |
| 0.2 | `Languages/Br/DeploymentGroups/enemies.json` | `/translate_imperial` | [ ] |
| 0.3 | `Languages/Br/DeploymentGroups/villains.json` | `/translate_imperial` | [ ] |

### 2.2 MissionCardText (8 arquivos)

| # | Arquivo | Comando | Status |
|---|---------|---------|--------|
| 0.4 | `Languages/Br/MissionCardText/core.json` | `/translate_imperial` | [ ] |
| 0.5 | `Languages/Br/MissionCardText/bespin.json` | `/translate_imperial` | [ ] |
| 0.6 | `Languages/Br/MissionCardText/empire.json` | `/translate_imperial` | [ ] |
| 0.7 | `Languages/Br/MissionCardText/hoth.json` | `/translate_imperial` | [ ] |
| 0.8 | `Languages/Br/MissionCardText/jabba.json` | `/translate_imperial` | [ ] |
| 0.9 | `Languages/Br/MissionCardText/lothal.json` | `/translate_imperial` | [ ] |
| 0.10 | `Languages/Br/MissionCardText/other.json` | `/translate_imperial` | [ ] |
| 0.11 | `Languages/Br/MissionCardText/twin.json` | `/translate_imperial` | [ ] |

### 2.3 Arquivos de Sistema (5 arquivos)

| # | Arquivo | Comando | Status |
|---|---------|---------|--------|
| 0.12 | `Languages/Br/bonuseffects.json` | `/translate_imperial` | [ ] |
| 0.13 | `Languages/Br/events.json` | `/translate_imperial` | [ ] |
| 0.14 | `Languages/Br/help.json` | `/translate_imperial` | [ ] |
| 0.15 | `Languages/Br/instructions.json` | `/translate_imperial` | [ ] |
| 0.16 | `Languages/Br/ui.json` | `/translate_imperial` | [ ] |

**Ao completar Fase 0:** Validar todos com `/validate_imperial`, gerar relatório parcial e fazer backup.

---

## Passo 3: Fase 1 - Tutoriais (3 arquivos)

// turbo
```bash
cd WORK/scripts && python copy_tutorials_to_br.py
```

| # | Arquivo | Status |
|---|---------|--------|
| 1.1 | `SagaTutorials/Br/TUTORIAL01.json` | [ ] |
| 1.2 | `SagaTutorials/Br/TUTORIAL02.json` | [ ] |
| 1.3 | `SagaTutorials/Br/TUTORIAL03.json` | [ ] |

---

## Passo 4: Fase 2 - MissionText (276 arquivos)

// turbo
```bash
cd WORK/scripts && python copy_missiontext_to_br.py
```

Traduzir em lotes de 10-20 arquivos.

---

## Passo 5: Fase 3 - Missões Core (32 arquivos)

// turbo
```bash
cd WORK/scripts && python copy_missions_to_br.py
```

| Lote | Arquivos | Status |
|------|----------|--------|
| 3.1 | CORE1 a CORE8 | [ ] |
| 3.2 | CORE9 a CORE16 | [ ] |
| 3.3 | CORE17 a CORE24 | [ ] |
| 3.4 | CORE25 a CORE32 | [ ] |

---

## Passo 6: Fase 4 - Expansões (106 arquivos)

### Prioridade P1
| Expansão | Arquivos | Status |
|----------|:--------:|--------|
| Jabba | 16 | [ ] |
| Hoth | 16 | [ ] |
| Empire | 16 | [ ] |

### Prioridade P2
| Expansão | Arquivos | Status |
|----------|:--------:|--------|
| Lothal | 6 | [ ] |
| Twin | 6 | [ ] |
| Bespin | 6 | [ ] |

### Prioridade P3
| Expansão | Arquivos | Status |
|----------|:--------:|--------|
| Other | 40 | [ ] |

---

## Passo 7: Gerar Relatório Final

Após completar todas as fases, criar relatório em `WORK/TRANSLATION_REPORT.md`:

```markdown
# Relatório de Tradução Imperial Commander 2

**Data:** [data atual]
**Total de arquivos:** 433

## Resumo por Fase

| Fase | Arquivos | Traduzidos | Validados | Erros |
|------|:--------:|:----------:|:---------:|:-----:|
| Fase 0 | 16 | X | X | X |
| Fase 1 | 3 | X | X | X |
| Fase 2 | 276 | X | X | X |
| Fase 3 | 32 | X | X | X |
| Fase 4 | 106 | X | X | X |

## Erros Encontrados

[Lista de erros, se houver]

## Próximos Passos

[Ações pendentes, se houver]
```

---

## Uso

```
# Traduzir tudo
/translate_all_imperial all

# Traduzir apenas Fase 0
/translate_all_imperial fase0

# Traduzir apenas Fase 1
/translate_all_imperial fase1

# Continuar de onde parou
/translate_all_imperial continue
```

---

## ⚠️ Notas Importantes

1. **Trabalhe em sessões** — Não é necessário traduzir tudo de uma vez
2. **Salve progresso** — Marque status no PLAN após cada arquivo
3. **Revise traduções** — Qualidade > Velocidade
4. **Faça backup** antes de cada fase

---

## Fluxo de Trabalho por Arquivo

```mermaid
flowchart TD
    A[Selecionar arquivo] --> B[/translate_imperial arquivo]
    B --> C[/validate_imperial arquivo]
    C --> D{Válido?}
    D -->|Sim| E[Marcar ✅ no PLAN]
    D -->|Não| F[Corrigir erros]
    F --> B
    E --> G[Próximo arquivo]
```
