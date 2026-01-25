#!/bin/bash
# validate-json.sh
# Validates JSON syntax for all modified .json files in the PR/Commit

echo "🔍 Buscando arquivos JSON modificados..."

# Get modified .json files (diff relative to main or current changes)
# In PR context, we likely want to check files changed from main
MODIFIED_FILES=$(git diff --name-only origin/main...HEAD | grep ".json$")

if [ -z "$MODIFIED_FILES" ]; then
    echo "✅ Nenhum arquivo JSON modificado encontrado."
    exit 0
fi

ERRORS=0

for file in $MODIFIED_FILES; do
    if [ -f "$file" ]; then
        echo -n "Checking $file... "
        if python -m json.tool "$file" > /dev/null 2>&1; then
            echo "✅ OK"
        else
            echo "❌ ERRO DE SINTAXE"
            python -m json.tool "$file" # Show error
            ERRORS=$((ERRORS+1))
        fi
    fi
done

if [ $ERRORS -gt 0 ]; then
    echo "🚫 FALHA: $ERRORS arquivo(s) com JSON inválido."
    exit 1
fi

echo "✨ Todos os arquivos JSON estão válidos."
exit 0
