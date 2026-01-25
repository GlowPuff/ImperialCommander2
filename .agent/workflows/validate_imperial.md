---
description: Validar traduções do Imperial Commander 2 - verificar JSON, variáveis, tags e glossário
---

# /validate_imperial - Validação de Traduções

$ARGUMENTS

---

## 🔴 OBJETIVO

Validar que um arquivo traduzido:
1. É um JSON válido
2. Tem todas as variáveis preservadas
3. Tem todas as tags preservadas
4. Usa o glossário corretamente
5. Tem metadados atualizados

---

## Passo 1: Validar JSON

// turbo
```bash
python -m json.tool $ARGUMENTS > /dev/null 2>&1 && echo "✅ JSON válido" || echo "❌ JSON INVÁLIDO - corrija antes de continuar"
```

---

## Passo 2: Verificar Metadados

// turbo
```bash
echo "=== Verificando languageID ==="
grep -o '"languageID"[^,]*' $ARGUMENTS || echo "⚠️ languageID não encontrado"
```

Esperado: `"languageID": "Portuguese Brazilian (BR)"`

---

## Passo 3: Contar Variáveis

// turbo
```bash
echo "=== Variáveis no arquivo ==="
grep -oE '\{[^}]+\}' $ARGUMENTS | sort | uniq -c | head -20
```

Compare com o arquivo original para garantir que nenhuma variável foi alterada.

---

## Passo 4: Contar Tags

// turbo
```bash
echo "=== Tags no arquivo ==="
grep -oE '<[^>]+>' $ARGUMENTS | sort | uniq -c | head -20
```

Compare com o arquivo original para garantir que nenhuma tag foi alterada.

---

## Passo 5: Verificar Termos Preservados (Automático)

Esta validação usa o script oficial para verificar se termos da `lista_de_preservacao` foram mantidos corretamente.

// turbo
```bash
# Validar preservação estrutural
python WORK/scripts/check_preservation.py Languages/En/$(basename $ARGUMENTS) $ARGUMENTS
```

> Se este passo falhar, o arquivo DEVE ser corrigido.

---

## Passo 6: Verificar Escapes

// turbo
```bash
echo "=== Verificando escapes ==="
grep -c '\\n' $ARGUMENTS && echo "Escapes \\n encontrados" || echo "Nenhum \\n (pode ser normal)"
```

---

## Passo 7: Comparar com Original (se disponível)

Para comparação completa, forneça o arquivo original:

```bash
# Variáveis no original
ORIG_VARS=$(grep -oE '\{[^}]+\}' <arquivo_original> | sort | uniq)
TRAD_VARS=$(grep -oE '\{[^}]+\}' $ARGUMENTS | sort | uniq)

# Comparar
diff <(echo "$ORIG_VARS") <(echo "$TRAD_VARS") && echo "✅ Variáveis idênticas" || echo "❌ DIFERENÇA NAS VARIÁVEIS"
```

---

## Relatório Final

Após executar todos os passos, gerar relatório:

| Verificação | Status |
|-------------|--------|
| JSON válido | ✅/❌ |
| languageID correto | ✅/❌ |
| Variáveis preservadas | ✅/❌ |
| Tags preservadas | ✅/❌ |
| Escapes intactos | ✅/❌ |

---

## Uso

```
/validate_imperial Languages/Br/ui.json
/validate_imperial SagaTutorials/Br/TUTORIAL01.json
/validate_imperial Languages/Br/bonuseffects.json
```

---

## Erros Comuns

| Erro | Causa | Solução |
|------|-------|---------|
| JSON inválido | Vírgula extra, aspas não fechadas | Usar `jq` para localizar erro |
| Variável traduzida | `{amount}` → `{quantidade}` | Restaurar variável original |
| Tag alterada | `<red>` → `<vermelho>` | Restaurar tag original |
| languageID errado | Não atualizado | Alterar para "Portuguese Brazilian (BR)" |