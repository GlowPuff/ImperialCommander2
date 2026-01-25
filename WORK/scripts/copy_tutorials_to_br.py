#!/usr/bin/env python3
"""
Copia tutoriais de En para Br.
Uso: python copy_tutorials_to_br.py [--dry-run]
"""
import shutil
import sys
from pathlib import Path

BASE = Path(__file__).parent.parent.parent / "ImperialCommander2/Assets/Resources/SagaTutorials"
SRC = BASE / "En"
DST = BASE / "Br"

def main():
    dry = "--dry-run" in sys.argv
    if dry: print("🔍 DRY-RUN\n")
    
    if not SRC.exists():
        print(f"Erro: {SRC}")
        sys.exit(1)
    
    if not dry:
        DST.mkdir(exist_ok=True)
    
    for f in sorted(SRC.glob("*.json")):
        dest = DST / f.name
        if dest.exists():
            print(f"⏭️  {f.name}")
        else:
            if not dry:
                shutil.copy2(f, dest)
            print(f"✅ {f.name}")

if __name__ == "__main__":
    main()
