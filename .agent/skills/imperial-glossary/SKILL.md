---
name: imperial-glossary
description: Gerenciamento de glossário e terminologia para tradução Imperial Assault. Hierarquia de decisão, lista de preservação e traduções consistentes.
allowed-tools: Read, Grep, Glob
---

# Imperial Glossary Skill

Skill para gerenciamento de terminologia e glossário do projeto Imperial Commander 2.

---

## 📁 Arquivos de Referência

| Arquivo | Caminho | Conteúdo |
|---------|---------|----------|
| **Dicionário Principal** | `WORK/dicionarios/dicionario.json` | Glossário + Preservação |
| **Manual de Contexto** | `WORK/manual.txt` | Regras gramaticais e contexto |

---

## 🔴 Hierarquia de Decisão (IMPERATIVA)

### Prioridade 1: Lista de Preservação

**SEMPRE verifique PRIMEIRO** se o termo está em `lista_de_preservacao`.

```json
{
  "lista_de_preservacao": {
    "palavras_chave_e_surges": ["Accuracy", "Blast", "Pierce", "Stun", ...],
    "nomes_proprios_e_entidades": ["Darth Vader", "Han Solo", "E-11", ...]
  }
}
```

> [!CAUTION]
> Termos na lista de preservação **NUNCA** são traduzidos, em nenhuma circunstância.

---

### Prioridade 2: Glossário de Tradução

Se NÃO está na preservação, verifique `glossario_de_traducao`:

```json
{
  "glossario_de_traducao": {
    "Ações e Mecânicas": {
      "Action": "Ação",
      "Activate": "Ativar",
      "Attack": "Atacar",
      "Move": "Mover",
      "Rest": "Descansar"
    },
    "Termos de Jogo": {
      "Strain": "Tensão",
      "Surge": "Impulso",
      "Deployment": "Posicionamento",
      "Figure": "Figura"
    }
  }
}
```

---

### Prioridade 3: Manual de Contexto

Para desambiguação, consulte `manual.txt`:

```json
{
  "notas_de_contexto": {
    "Strain": "Traduzir como 'Tensão'. Não usar 'Estresse' ou 'Esforço'.",
    "Claim": "Traduzir como 'Obter'. Não usar 'Reivindicar'.",
    "Shelf": "Traduzir como 'Compartimento' (contexto de naves).",
    "Deployment": "Pode ser 'Posicionamento' (substantivo) ou 'Posicionar' (verbo)."
  }
}
```

---

## 📋 Regras de Prioridade do Dicionário

```json
{
  "regras_de_prioridade": {
    "1_IMPERATIVO": "Verifique PRIMEIRO a 'lista_de_preservacao'. Se o termo estiver lá, MANTENHA O ORIGINAL em inglês.",
    "2_TRADUCAO": "Se não estiver na preservação, verifique o 'glossario_de_traducao'. Use o valor exato.",
    "3_SINTAXE": "Jamais traduza conteúdo entre chaves {}, colchetes [] ou tags <>.",
    "4_CAIXA": "Respeite a capitalização do termo original (ex: 'Action' -> 'Ação', 'action' -> 'ação')."
  }
}
```

---

## 🔍 Como Consultar

### Verificar se termo está na preservação:
```bash
grep -i '"termo"' WORK/dicionarios/dicionario.json | head -5
```

### Buscar tradução no glossário:
```bash
jq '.glossario_de_traducao | .. | objects | select(has("Termo"))' WORK/dicionarios/dicionario.json
```

### Consultar contexto no manual:
```bash
grep -i "termo" WORK/manual.txt -B2 -A2
```

---

## ✅ Exemplos

| Termo EN | Preservar? | Tradução PT |
|----------|:----------:|-------------|
| `Darth Vader` | ✅ | `Darth Vader` |
| `Pierce` | ✅ | `Pierce` |
| `Action` | ❌ | `Ação` |
| `Strain` | ❌ | `Tensão` |
| `E-11` | ✅ | `E-11` |
| `Figure` | ❌ | `Figura` |

---

## ⚠️ Casos Especiais

### Habilidades em bonuseffects.json

Manter nome da habilidade em inglês antes dos dois pontos:

```
✅ "CHARGING UP: Depois que esta figura atacar..."
❌ "CARREGANDO: Depois que esta figura atacar..."
```

### Capitalização

Respeitar a capitalização original:

```
"Action" → "Ação"
"action" → "ação"
"ACTION" → "AÇÃO"
```
