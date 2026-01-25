#!/usr/bin/env python3
"""
Copia missões originais para Languages/Br/Missions/ com sufixo _BR.
Ex: CORE1.json → CORE1_BR.json

Uso:
    python copy_missions_to_br.py [--dry-run]
"""

import shutil
import sys
from pathlib import Path

BASE_DIR = Path(__file__).parent.parent.parent
SAGA_MISSIONS = BASE_DIR / "ImperialCommander2" / "Assets" / "Resources" / "SagaMissions"
BR_MISSIONS = BASE_DIR / "ImperialCommander2" / "Assets" / "Resources" / "Languages" / "Br" / "Missions"


def main():
    dry_run = "--dry-run" in sys.argv
    
    if dry_run:
        print("🔍 Modo DRY-RUN\n")
    
    if not SAGA_MISSIONS.exists():
        print(f"Erro: {SAGA_MISSIONS}")
        sys.exit(1)
    
    if not dry_run:
        BR_MISSIONS.mkdir(parents=True, exist_ok=True)
    
    expansions = [d for d in SAGA_MISSIONS.iterdir() if d.is_dir()]
    total = 0
    
    for exp in sorted(expansions):
        dest_dir = BR_MISSIONS / exp.name
        json_files = list(exp.glob("*.json"))
        
        if not json_files:
            continue
        
        print(f"📁 {exp.name}/")
        
        if not dry_run:
            dest_dir.mkdir(exist_ok=True)
        
        for f in sorted(json_files):
            # Adicionar sufixo _BR antes da extensão
            new_name = f.stem + "_BR" + f.suffix
            dest_file = dest_dir / new_name
            
            if dest_file.exists():
                status = "⏭️  existe"
            else:
                if not dry_run:
                    shutil.copy2(f, dest_file)
                status = "✅" if not dry_run else "📋"
            
            print(f"   {status}: {f.name} → {new_name}")
            total += 1
    
    print(f"\n📊 Total: {total}")


if __name__ == "__main__":
    main()
