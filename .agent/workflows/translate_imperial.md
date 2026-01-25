---
description: Invocar o agente imperial-translator para traduzir arquivos do Imperial Commander 2 EN→PT-BR
---

# /translate_imperial - Tradução Imperial Commander 2

! IMPORTANTE: Você deve atuar como o agente [imperial-translator](file:///.agent/agents/imperial-translator.md). Leia e interiorize todas as suas regras antes de prosseguir.

$ARGUMENTS

---

## 🔴 REGRAS CRÍTICAS

1. **Carregar recursos obrigatoriamente** antes de traduzir
2. **Seguir hierarquia de decisão** rigorosamente
3. **Preservar código** (tags, variáveis, escapes)
4. **Validar JSON** após cada alteração

---

## Passo 1: Carregar Recursos

Antes de qualquer tradução, leia os arquivos de referência:

```bash
# Carregar dicionário
cat WORK/dicionarios/dicionario.json

# Verificar estrutura do arquivo alvo
python WORK/scripts/translate_mission.py --list-keys $ARGUMENTS
```

Recursos obrigatórios:
- `WORK/dicionarios/dicionario.json` → Glossário e lista de preservação
- `WORK/manual.txt` → Regras de contexto (consultar quando necessário)
- `WORK/PRD.md` → Especificações do projeto
- `.agent/agents/imperial-translator.md` → Instruções do agente

---

## Passo 2: Hierarquia de Decisão

Para cada termo encontrado, siga ESTA ORDEM:

| Prioridade | Verificação | Ação |
|:----------:|-------------|------|
| 1 | Termo em `lista_de_preservacao`? | 🔒 MANTER INGLÊS |
| 2 | Termo em `glossario_de_traducao`? | 📖 Usar tradução exata |
| 3 | Contexto em `manual.txt`? | 🌐 Desambiguação |
| 4 | Tradução natural | ✍️ Traduzir com fluência |

---

## Passo 3: Elementos NUNCA Traduzir

| Tipo | Exemplo | Preservar |
|------|---------|:---------:|
| Variáveis | `{amount}`, `{hero}` | ✅ |
| Tags | `<red>`, `</b_blue>` | ✅ |
| Escapes | `\n`, `\r` | ✅ |
| Colchetes | `[source]` | ✅ |

---

## Passo 4: Executar Tradução

### Para arquivos JSON:

**1. Extrair Lote**
```bash
# Extrair o primeiro lote (ou iterar se houver mais)
# O arquivo será criado em: WORK/scripts/traduzindo/batch_0.json
python WORK/scripts/extract_batch_universal.py $ARGUMENTS 0
```

**2. Traduzir o Lote (CRÍTICO)**
1. Leia o arquivo `WORK/scripts/traduzindo/batch_0.json`.
2. Para cada item na lista `"items"`, crie o campo `"translated"`.
3. Preencha `"translated"` com a tradução do valor em `"value"`.
4. **NÃO crie scripts python** para fazer isso. Edite o JSON intelectualmente.
5. Salve o resultado como `WORK/scripts/traduzindo/batch_0_translated.json`.

**3. Aplicar Tradução**
```bash
python WORK/scripts/apply_batch_universal.py $ARGUMENTS WORK/scripts/traduzindo/batch_0_translated.json
```

**4. Validar Preservação (Obrigatório)**
// turbo
```bash
# Se este passo falhar, você DEVE corrigir a tradução antes de prosseguir
python WORK/scripts/check_preservation.py Languages/En/$(basename $ARGUMENTS) $ARGUMENTS
```

**5. Limpeza (Obrigatória)**
// turbo
```bash
rm WORK/scripts/traduzindo/batch_0.json WORK/scripts/traduzindo/batch_0_translated.json WORK/scripts/traduzindo/batch_0.txt
```

### Para arquivos TXT:

```bash
# Analisar estrutura
python WORK/scripts/translate_txt.py --analyze $ARGUMENTS
```

---

## Passo 5: Atualizar Metadados

Após traduzir, atualizar campos:

```json
{
  "languageID": "Portuguese Brazilian (BR)",
  "saveDate": "4/26/2024"
}
```

---

## Passo 6: Validar

// turbo
```bash
python -m json.tool $ARGUMENTS > /dev/null && echo "✅ JSON válido" || echo "❌ JSON inválido"
```

---

## Uso

```
/translate_imperial Languages/Br/ui.json
/translate_imperial SagaTutorials/Br/TUTORIAL01.json
/translate_imperial Languages/Br/DeploymentGroups/allies.json
```

---

## Exemplo de Tradução Correta

```
EN: "Move {amount} spaces and <red>attack</red> the Terminal."
PT: "Mova {amount} espaços e <red>ataque</red> o Terminal."
     ^^^^^^^^                 ^^^^^^^^^^^^^
     Variável preservada      Tag preservada
```
