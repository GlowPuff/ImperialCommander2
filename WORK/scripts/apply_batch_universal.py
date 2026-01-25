#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Aplicador de Lotes Traduzidos - Imperial Commander 2
Aplica traduções de um lote de volta ao arquivo JSON original.

Uso:
    python apply_batch_universal.py <arquivo_json> <lote_traduzido.json>
    
Exemplo:
    python apply_batch_universal.py ../Assets/Resources/SagaTutorials/Br/TUTORIAL01.json batch_0_translated.json

O arquivo de lote traduzido deve ter a mesma estrutura do batch extraído,
mas com o campo "translated" adicionado em cada item.
"""

import json
import sys
from datetime import datetime
from pathlib import Path


def set_value_at_path(data, path: str, value: str):
    """Define um valor no JSON seguindo o caminho especificado."""
    parts = []
    current = ""
    i = 0
    
    while i < len(path):
        if path[i] == '.':
            if current:
                parts.append(current)
                current = ""
        elif path[i] == '[':
            if current:
                parts.append(current)
                current = ""
            # Encontrar o índice
            j = i + 1
            while j < len(path) and path[j] != ']':
                j += 1
            parts.append(int(path[i+1:j]))
            i = j
        else:
            current += path[i]
        i += 1
    
    if current:
        parts.append(current)
    
    # Navegar até o penúltimo elemento
    obj = data
    for part in parts[:-1]:
        obj = obj[part]
    
    # Definir o valor
    obj[parts[-1]] = value


def main():
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(1)
    
    target_file = Path(sys.argv[1])
    batch_file = Path(sys.argv[2])
    
    if not target_file.exists():
        print(f"Erro: Arquivo alvo não encontrado: {target_file}")
        sys.exit(1)
    
    if not batch_file.exists():
        print(f"Erro: Arquivo de lote não encontrado: {batch_file}")
        sys.exit(1)
    
    # Carregar arquivo alvo
    print(f"📂 Carregando arquivo alvo: {target_file.name}")
    with open(target_file, "r", encoding="utf-8") as f:
        data = json.load(f)
    
    # Carregar lote traduzido
    print(f"📦 Carregando lote traduzido: {batch_file.name}")
    with open(batch_file, "r", encoding="utf-8") as f:
        batch = json.load(f)
    
    items = batch.get("items", [])
    applied = 0
    skipped = 0
    
    print(f"\n🔄 Aplicando {len(items)} traduções...")
    
    for item in items:
        path = item.get("path")
        translated = item.get("translated")
        
        if not translated:
            skipped += 1
            continue
        
        try:
            set_value_at_path(data, path, translated)
            applied += 1
        except Exception as e:
            print(f"⚠️  Erro ao aplicar {path}: {e}")
            skipped += 1
    
    # Atualizar metadados
    if "languageID" in data:
        data["languageID"] = "Portuguese Brazilian (BR)"
        print("✅ languageID atualizado")
    
    if "saveDate" in data:
        data["saveDate"] = datetime.now().strftime("%-m/%-d/%Y")
        print("✅ saveDate atualizado")
    
    # Salvar arquivo
    with open(target_file, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    
    print(f"\n✅ Arquivo salvo: {target_file.name}")
    print(f"📊 Aplicados: {applied} | Ignorados: {skipped}")


if __name__ == "__main__":
    main()
