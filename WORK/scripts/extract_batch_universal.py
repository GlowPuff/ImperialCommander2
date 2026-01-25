#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Extrator de Lotes para Tradução - Imperial Commander 2
Extrai lotes de texto traduzível de arquivos JSON para processamento.

Uso:
    python extract_batch_universal.py <arquivo_json> [lote_numero]
    
Exemplo:
    python extract_batch_universal.py ../Assets/Resources/SagaTutorials/Br/TUTORIAL01.json 0
    
Output:
    Cria arquivo batch_<N>.json com os itens do lote para tradução.
"""

import json
import sys
from datetime import datetime
from pathlib import Path

# Configurações
BATCH_SIZE = 50
WORK_DIR = Path(__file__).parent.parent
WORK_DIR = Path(__file__).parent.parent
DICIONARIO_PATH = WORK_DIR / "dicionarios" / "dicionario.json"
OUTPUT_DIR = WORK_DIR / "traduzindo"
OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

# Chaves traduzíveis
TRANSLATABLE_KEYS = {
    "missionName", "campaignName", "missionDescription", "additionalMissionInfo",
    "startingObjective", "missionInfo", "customInstructions",
    "eventText", "theText", "buttonText", "choiceText",
    "descriptionText", "bonusText", "imperialRewardText", "rebelsRewardText",
    "effects", "eventFlavor", "content", "helpText", "instruction",
    "subname", "text"
}


def load_dicionario() -> dict:
    """Carrega o dicionário de referência."""
    if not DICIONARIO_PATH.exists():
        return {}
    with open(DICIONARIO_PATH, "r", encoding="utf-8") as f:
        return json.load(f)


def get_preservation_list(dicionario: dict) -> set:
    """Extrai lista de termos a preservar."""
    preservation = set()
    lista = dicionario.get("lista_de_preservacao", {})
    for key, values in lista.items():
        if isinstance(values, list):
            preservation.update(values)
    return preservation


def extract_items(data, path="", parent_guid=""):
    """Extrai recursivamente itens traduzíveis com seus caminhos."""
    items = []
    
    if isinstance(data, dict):
        # Capturar GUID do contexto atual se existir
        current_guid = data.get("GUID", parent_guid)
        
        for key, value in data.items():
            current_path = f"{path}.{key}" if path else key
            
            if key in TRANSLATABLE_KEYS and isinstance(value, str) and value.strip():
                items.append({
                    "path": current_path,
                    "key": key,
                    "value": value,
                    "context_guid": current_guid,
                    "parent_name": data.get("name", "")
                })
            elif isinstance(value, (dict, list)):
                items.extend(extract_items(value, current_path, current_guid))
    
    elif isinstance(data, list):
        for idx, item in enumerate(data):
            current_path = f"{path}[{idx}]"
            items.extend(extract_items(item, current_path, parent_guid))
    
    return items


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    
    filepath = Path(sys.argv[1])
    batch_num = int(sys.argv[2]) if len(sys.argv) > 2 else None
    
    if not filepath.exists():
        print(f"Erro: Arquivo não encontrado: {filepath}")
        sys.exit(1)
    
    # Carregar arquivo
    print(f"📂 Carregando: {filepath.name}")
    with open(filepath, "r", encoding="utf-8") as f:
        data = json.load(f)
    
    # Extrair itens
    items = extract_items(data)
    total_items = len(items)
    total_batches = (total_items + BATCH_SIZE - 1) // BATCH_SIZE
    
    print(f"📊 Total de itens traduzíveis: {total_items}")
    print(f"📦 Total de lotes: {total_batches}")
    
    if batch_num is None:
        # Listar todos os lotes disponíveis
        print("\n📋 Lotes disponíveis:")
        for i in range(total_batches):
            start = i * BATCH_SIZE
            end = min(start + BATCH_SIZE, total_items)
            print(f"   Lote {i}: itens {start} a {end-1}")
        print(f"\n💡 Execute: python {Path(__file__).name} {filepath} <numero_lote>")
        sys.exit(0)
    
    if batch_num >= total_batches:
        print(f"Erro: Lote {batch_num} não existe. Máximo: {total_batches - 1}")
        sys.exit(1)
    
    # Extrair lote específico
    start = batch_num * BATCH_SIZE
    end = min(start + BATCH_SIZE, total_items)
    batch_items = items[start:end]
    
    # Criar arquivo de lote
    output_file = OUTPUT_DIR / f"batch_{batch_num}.json"
    batch_data = {
        "source_file": str(filepath),
        "batch_number": batch_num,
        "items_range": f"{start}-{end-1}",
        "total_items": len(batch_items),
        "extracted_at": datetime.now().isoformat(),
        "items": batch_items
    }
    
    with open(output_file, "w", encoding="utf-8") as f:
        json.dump(batch_data, f, ensure_ascii=False, indent=2)
    
    print(f"\n✅ Lote {batch_num} extraído: {output_file.name}")
    print(f"   Itens: {start} a {end-1} ({len(batch_items)} total)")
    
    # Criar arquivo TXT para visualização/tradução manual
    txt_file = OUTPUT_DIR / f"batch_{batch_num}.txt"
    with open(txt_file, "w", encoding="utf-8") as f:
        f.write(f"# Lote {batch_num} - {filepath.name}\n")
        f.write(f"# Itens {start} a {end-1}\n")
        f.write("=" * 60 + "\n\n")
        
        for i, item in enumerate(batch_items):
            f.write(f"[{start + i}] {item['key']}\n")
            f.write(f"Path: {item['path']}\n")
            if item.get('parent_name'):
                f.write(f"Context: {item['parent_name']}\n")
            f.write(f"Original:\n{item['value']}\n")
            f.write(f"\nTradução:\n\n")
            f.write("-" * 40 + "\n\n")
    
    print(f"📝 Arquivo de tradução: {txt_file.name}")


if __name__ == "__main__":
    main()
