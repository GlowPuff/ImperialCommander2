---
name: imperial-code-integrity
description: Preservação de código durante tradução. Proteção de tags XML, variáveis, escapes e estruturas que não devem ser traduzidas.
allowed-tools: Read, Grep, Glob
---

# Imperial Code Integrity Skill

Skill para garantir a integridade do código durante traduções no Imperial Commander 2.

---

## 🚫 Elementos NUNCA Traduzir

### 1. Variáveis (Chaves)

| Padrão | Exemplo | Regex |
|--------|---------|-------|
| `{...}` | `{amount}`, `{hero}`, `{ENEMYNAME}` | `\{[^}]+\}` |

```
✅ "Move {amount} spaces" → "Mova {amount} espaços"
❌ "Move {amount} spaces" → "Mova {quantidade} espaços"
```

---

### 2. Tags XML/HTML

| Padrão | Exemplo | Regex |
|--------|---------|-------|
| `<...>` | `<red>`, `</b_blue>`, `<size=120%>` | `<[^>]+>` |

```
✅ "<red>Warning!</red>" → "<red>Aviso!</red>"
❌ "<red>Warning!</red>" → "<vermelho>Aviso!</vermelho>"
```

---

### 3. Escapes

| Padrão | Exemplo | Regex |
|--------|---------|-------|
| `\n` | Nova linha | `\\n` |
| `\r` | Retorno | `\\r` |
| `\t` | Tab | `\\t` |

```
✅ "Line 1\nLine 2" → "Linha 1\nLinha 2"
❌ "Line 1\nLine 2" → "Linha 1
Linha 2"
```

---

### 4. Colchetes

| Padrão | Exemplo | Regex |
|--------|---------|-------|
| `[...]` | `[source]`, `[target]` | `\[[^\]]+\]` |

```
✅ "Deal [damage] to [target]" → "Cause [damage] a [target]"
❌ "Deal [damage] to [target]" → "Cause [dano] a [alvo]"
```

---

## 🔍 Validação de Integridade

### Script de Verificação

```bash
# Contar variáveis antes e depois
ORIG=$(grep -oE '\{[^}]+\}' original.json | sort | uniq -c)
TRAD=$(grep -oE '\{[^}]+\}' traduzido.json | sort | uniq -c)

# Comparar
diff <(echo "$ORIG") <(echo "$TRAD")
```

### Verificação de Tags

```bash
# Extrair todas as tags
grep -oE '<[^>]+>' original.json | sort | uniq > tags_orig.txt
grep -oE '<[^>]+>' traduzido.json | sort | uniq > tags_trad.txt

# Comparar
diff tags_orig.txt tags_trad.txt
```

---

## ✅ Regex de Proteção Combinado

Para identificar TODOS os elementos que não devem ser traduzidos:

```regex
(\{[^}]+\}|<[^>]+>|\\[nrt]|\[[^\]]+\])
```

---

## 📋 Checklist de Integridade

Antes de salvar qualquer tradução:

- [ ] Variáveis `{...}` preservadas exatamente
- [ ] Tags `<...>` preservadas exatamente
- [ ] Escapes `\n`, `\r`, `\t` preservados
- [ ] Colchetes `[...]` preservados
- [ ] Quantidade de elementos igual ao original
- [ ] JSON válido após alterações

---

## ⚠️ Erros Comuns

### Tradução de Variáveis

```
❌ {amount} → {quantidade}
❌ {hero} → {herói}
❌ {ENEMYNAME} → {NOME_INIMIGO}
```

### Alteração de Tags

```
❌ <red> → <vermelho>
❌ </b_blue> → </b_azul>
❌ <size=120%> → <tamanho=120%>
```

### Quebra de Escapes

```
❌ \n → (nova linha real)
❌ "texto\ntexto" → "texto
texto"
```

---

## 🛠️ Ferramentas de Validação

| Comando | Propósito |
|---------|-----------|
| `python -m json.tool <file>` | Validar JSON |
| `jq '.' <file>` | Formatar e validar JSON |
| `grep -P '\{[^}]+\}' <file>` | Listar variáveis |
| `grep -P '<[^>]+>' <file>` | Listar tags |
