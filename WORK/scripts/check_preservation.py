#!/usr/bin/env python3
"""
Verificador de Preservação Estrutural - Imperial Commander 2
Verifica se termos protegidos foram mantidos no arquivo traduzido,
comparando chave por chave para evitar falsos positivos.

Uso:
    python check_preservation.py <arquivo_original_en.json> <arquivo_traduzido_br.json>
"""

import json
import sys
from pathlib import Path

WORK_DIR = Path(__file__).parent.parent
DICIONARIO_PATH = WORK_DIR / "dicionarios" / "dicionario.json"

def load_preservation_list():
    if not DICIONARIO_PATH.exists():
        return set()
    with open(DICIONARIO_PATH, "r", encoding="utf-8") as f:
        data = json.load(f)
    preservation = set()
    lista = data.get("lista_de_preservacao", {})
    if "palavras_chave_e_surges" in lista:
        preservation.update(lista["palavras_chave_e_surges"])
    if "nomes_proprios_e_entidades" in lista:
        preservation.update(lista["nomes_proprios_e_entidades"])
    return preservation

def find_violations_recursive(data_en, data_br, path, preservation_list):
    violations = []
    
    if isinstance(data_en, dict) and isinstance(data_br, dict):
        for key in data_en:
            if key in data_br:
                violations.extend(find_violations_recursive(data_en[key], data_br[key], f"{path}.{key}", preservation_list))
    
    elif isinstance(data_en, list) and isinstance(data_br, list):
        # Tentar parear por índices
        for i in range(min(len(data_en), len(data_br))):
            violations.extend(find_violations_recursive(data_en[i], data_br[i], f"{path}[{i}]", preservation_list))
            
    elif isinstance(data_en, str) and isinstance(data_br, str):
        # Checagem de preservação na string
        for term in preservation_list:
            if term in data_en:
                # Termo existe no original, deve existir na tradução
                # Case sensitive check
                if term not in data_br:
                    violations.append(f"Termo '{term}' perdido em '{path}'\n   EN: ...{data_en[:50]}...\n   BR: ...{data_br[:50]}...")
    
    return violations

def main():
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(1)
        
    file_en = Path(sys.argv[1])
    file_br = Path(sys.argv[2])
    
    if not file_en.exists() or not file_br.exists():
        print("Erro: Arquivos não encontrados.")
        sys.exit(1)
        
    print(f"🔍 Verificando preservação estrutural em: {file_br.name}")
    
    terms = load_preservation_list()
    print(f"📋 Termos protegidos: {len(terms)}")
    
    try:
        with open(file_en, "r", encoding="utf-8") as f:
            data_en = json.load(f)
        with open(file_br, "r", encoding="utf-8") as f:
            data_br = json.load(f)
    except json.JSONDecodeError:
        print("❌ Erro: Arquivo JSON inválido.")
        sys.exit(1)
        
    violations = find_violations_recursive(data_en, data_br, "root", terms)
    
    if violations:
        print("\n❌ ERROS DE PRESERVAÇÃO ENCONTRADOS:")
        for v in violations[:10]:
            print(f" - {v}")
        if len(violations) > 10:
            print(f" ... e mais {len(violations)-10} erros.")
        print("\n⚠️  A tradução removeu termos obrigatórios em campos específicos.")
        sys.exit(1)
    else:
        print("✅ Sucesso: Preservação verificada estrutura-a-estrutura.")
        sys.exit(0)

if __name__ == "__main__":
    main()
