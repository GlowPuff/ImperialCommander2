#!/usr/bin/env python3
"""
Tradutor de arquivos MissionText (*.txt) - Imperial Commander 2
Aplica glossário e preserva tags HTML.

Uso:
    python translate_txt.py <arquivo.txt>
    python translate_txt.py --analyze <arquivo.txt>
"""

import json
import re
import sys
from pathlib import Path

WORK_DIR = Path(__file__).parent.parent
DICIONARIO_PATH = WORK_DIR / "dicionarios" / "dicionario.json"


def load_dicionario():
    if not DICIONARIO_PATH.exists():
        return {}, set()
    with open(DICIONARIO_PATH, "r", encoding="utf-8") as f:
        data = json.load(f)
    
    # Glossário
    glossary = {}
    for category, terms in data.get("glossario_de_traducao", {}).items():
        if isinstance(terms, dict):
            glossary.update(terms)
    
    # Preservação
    preservation = set()
    for key, values in data.get("lista_de_preservacao", {}).items():
        if isinstance(values, list):
            preservation.update(values)
    
    return glossary, preservation


def extract_tags(text):
    """Extrai tags HTML para proteção."""
    return re.findall(r'<[^>]+>', text)


def analyze_file(filepath):
    """Analisa arquivo TXT."""
    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()
    
    lines = content.split('\n')
    tags = extract_tags(content)
    
    print(f"\n📄 {filepath.name}")
    print(f"📊 Linhas: {len(lines)}")
    print(f"🏷️  Tags encontradas: {len(set(tags))}")
    for tag in sorted(set(tags)):
        print(f"   {tag}")


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    
    if sys.argv[1] == "--analyze":
        if len(sys.argv) < 3:
            print("Erro: especifique arquivo")
            sys.exit(1)
        analyze_file(Path(sys.argv[2]))
    else:
        filepath = Path(sys.argv[1])
        if not filepath.exists():
            print(f"Erro: {filepath}")
            sys.exit(1)
        
        glossary, preservation = load_dicionario()
        print(f"📖 Glossário: {len(glossary)} termos")
        print(f"🔒 Preservação: {len(preservation)} termos")
        analyze_file(filepath)


if __name__ == "__main__":
    main()
