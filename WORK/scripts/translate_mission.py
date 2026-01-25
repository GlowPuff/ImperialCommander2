#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script Universal de Tradução - Imperial Commander 2
Traduz arquivos JSON de missões/tutoriais do Imperial Assault para PT-BR.

Uso:
    python translate_mission.py <arquivo_json>
    python translate_mission.py --list-keys <arquivo_json>  # Lista chaves traduzíveis

Exemplo:
    python translate_mission.py ../Assets/Resources/SagaTutorials/Br/TUTORIAL01.json
"""

import json
import sys
import os
import re
from datetime import datetime
from pathlib import Path

# Caminho para o diretório de trabalho
WORK_DIR = Path(__file__).parent.parent
DICIONARIO_PATH = WORK_DIR / "dicionarios" / "dicionario.json"

# Chaves que contêm texto traduzível
TRANSLATABLE_KEYS = {
    # Propriedades de missão
    "missionName", "campaignName", "missionDescription", "additionalMissionInfo",
    "startingObjective", "missionInfo", "customInstructions",
    # Textos de evento
    "eventText", "theText", "buttonText", "choiceText",
    "descriptionText", "bonusText", "imperialRewardText", "rebelsRewardText",
    # Efeitos e eventos
    "effects", "eventFlavor", "content",
    # Ajuda e instruções
    "helpText", "instruction",
    # Dados de entidades
    "subname", "text"
}

# Chaves de metadados a atualizar
METADATA_UPDATES = {
    "languageID": "Portuguese Brazilian (BR)",
}


class TranslationProcessor:
    """Processador de tradução com suporte ao dicionário e preservação de tags."""
    
    def __init__(self, dicionario_path: Path):
        self.dicionario = self._load_dicionario(dicionario_path)
        self.preservation_list = self._build_preservation_set()
        self.glossary = self._build_glossary()
        self.stats = {"total": 0, "translated": 0, "preserved": 0, "skipped": 0}
    
    def _load_dicionario(self, path: Path) -> dict:
        """Carrega o dicionário de referência."""
        if not path.exists():
            print(f"⚠️  Dicionário não encontrado: {path}")
            return {}
        with open(path, "r", encoding="utf-8") as f:
            return json.load(f)
    
    def _build_preservation_set(self) -> set:
        """Constrói set de termos a preservar em inglês."""
        preservation = set()
        lista = self.dicionario.get("lista_de_preservacao", {})
        for key, values in lista.items():
            if isinstance(values, list):
                preservation.update(values)
            elif isinstance(values, str):
                preservation.add(values)
        return preservation
    
    def _build_glossary(self) -> dict:
        """Constrói dicionário de traduções (inglês → português)."""
        glossary = {}
        glossario = self.dicionario.get("glossario_de_traducao", {})
        for category, terms in glossario.items():
            if isinstance(terms, dict):
                glossary.update(terms)
        return glossary
    
    def should_preserve(self, text: str) -> bool:
        """Verifica se o texto contém termos a preservar."""
        for term in self.preservation_list:
            if term in text:
                return True
        return False
    
    def apply_glossary(self, text: str) -> str:
        """Aplica traduções do glossário ao texto."""
        result = text
        # Ordenar por tamanho decrescente para evitar substituições parciais
        for en, pt in sorted(self.glossary.items(), key=lambda x: -len(x[0])):
            # Substituição case-insensitive preservando case original
            pattern = re.compile(re.escape(en), re.IGNORECASE)
            result = pattern.sub(pt, result)
        return result
    
    def extract_translatable_values(self, data: dict, path: str = "") -> list:
        """Extrai todos os valores traduzíveis do JSON com seus caminhos."""
        results = []
        
        if isinstance(data, dict):
            for key, value in data.items():
                current_path = f"{path}.{key}" if path else key
                if key in TRANSLATABLE_KEYS and isinstance(value, str) and value.strip():
                    results.append({
                        "path": current_path,
                        "key": key,
                        "value": value,
                        "needs_translation": True
                    })
                elif isinstance(value, (dict, list)):
                    results.extend(self.extract_translatable_values(value, current_path))
        
        elif isinstance(data, list):
            for idx, item in enumerate(data):
                current_path = f"{path}[{idx}]"
                results.extend(self.extract_translatable_values(item, current_path))
        
        return results
    
    def count_translatable_items(self, data: dict) -> int:
        """Conta quantos itens traduzíveis existem no arquivo."""
        return len(self.extract_translatable_values(data))


def update_metadata(data: dict) -> dict:
    """Atualiza metadados do arquivo (languageID, saveDate)."""
    # Atualizar languageID
    if "languageID" in data:
        data["languageID"] = METADATA_UPDATES["languageID"]
    
    # Atualizar saveDate
    if "saveDate" in data:
        data["saveDate"] = datetime.now().strftime("%-m/%-d/%Y")
    
    return data


def analyze_file(filepath: Path) -> dict:
    """Analisa um arquivo JSON e retorna estatísticas."""
    with open(filepath, "r", encoding="utf-8") as f:
        data = json.load(f)
    
    processor = TranslationProcessor(DICIONARIO_PATH)
    items = processor.extract_translatable_values(data)
    
    stats = {
        "total_lines": sum(1 for _ in open(filepath, "r", encoding="utf-8")),
        "translatable_items": len(items),
        "keys_found": {},
    }
    
    for item in items:
        key = item["key"]
        stats["keys_found"][key] = stats["keys_found"].get(key, 0) + 1
    
    return stats


def list_keys_mode(filepath: Path):
    """Modo de listagem de chaves traduzíveis."""
    stats = analyze_file(filepath)
    
    print(f"\n📄 Arquivo: {filepath.name}")
    print(f"📊 Total de linhas: {stats['total_lines']}")
    print(f"🔤 Itens traduzíveis: {stats['translatable_items']}")
    print("\n📋 Chaves encontradas:")
    
    for key, count in sorted(stats["keys_found"].items(), key=lambda x: -x[1]):
        print(f"   {key}: {count}")
    
    # Calcular lotes recomendados
    batch_size = 50
    total_batches = (stats["translatable_items"] + batch_size - 1) // batch_size
    print(f"\n📦 Lotes recomendados (~{batch_size} itens): {total_batches}")


def main():
    """Função principal do script."""
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    
    # Modo de listagem de chaves
    if sys.argv[1] == "--list-keys":
        if len(sys.argv) < 3:
            print("Erro: Especifique o arquivo JSON")
            sys.exit(1)
        filepath = Path(sys.argv[2])
        if not filepath.exists():
            print(f"Erro: Arquivo não encontrado: {filepath}")
            sys.exit(1)
        list_keys_mode(filepath)
        sys.exit(0)
    
    # Modo normal - análise do arquivo
    filepath = Path(sys.argv[1])
    if not filepath.exists():
        print(f"Erro: Arquivo não encontrado: {filepath}")
        sys.exit(1)
    
    print(f"\n🔍 Analisando: {filepath.name}")
    
    # Verificar dicionário
    if not DICIONARIO_PATH.exists():
        print(f"⚠️  Dicionário não encontrado: {DICIONARIO_PATH}")
    else:
        print(f"✅ Dicionário carregado: {DICIONARIO_PATH.name}")
    
    # Analisar arquivo
    stats = analyze_file(filepath)
    print(f"📊 Linhas: {stats['total_lines']}")
    print(f"🔤 Itens traduzíveis: {stats['translatable_items']}")
    
    # Mostrar resumo de chaves
    print("\n📋 Resumo de chaves:")
    for key, count in sorted(stats["keys_found"].items(), key=lambda x: -x[1])[:10]:
        print(f"   {key}: {count}")
    
    # Calcular lotes
    batch_size = 50
    total_batches = (stats["translatable_items"] + batch_size - 1) // batch_size
    print(f"\n📦 Total de lotes necessários: {total_batches}")
    print(f"\n💡 Para iniciar tradução, execute o script de extração de lotes.")


if __name__ == "__main__":
    main()
