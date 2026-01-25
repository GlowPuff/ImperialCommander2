#!/usr/bin/env python3
"""
Copia MissionText de En para Br.
Uso: python copy_missiontext_to_br.py [--dry-run]
"""
import shutil
import sys
from pathlib import Path

BASE = Path(__file__).parent.parent.parent / "ImperialCommander2/Assets/Resources/Languages"
SRC = BASE / "En" / "MissionText"
DST = BASE / "Br" / "MissionText"

def main():
    dry = "--dry-run" in sys.argv
    if dry: print("🔍 DRY-RUN\n")
    
    if not SRC.exists():
        print(f"Erro: {SRC}")
        sys.exit(1)
    
    if not dry:
        DST.mkdir(parents=True, exist_ok=True)
    
    for f in sorted(SRC.glob("*.txt")):
        dest = DST / f.name
        if dest.exists():
            print(f"⏭️  {f.name}")
        else:
            if not dry:
                shutil.copy2(f, dest)
            print(f"✅ {f.name}")
    
    print(f"\n📊 Total: {len(list(SRC.glob('*.txt')))}")

if __name__ == "__main__":
    main()
