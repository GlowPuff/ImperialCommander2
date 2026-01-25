#!/usr/bin/env python3
import subprocess
import argparse
import sys

# List of files to translate in Fase 0
# Format: (filename, description/notes)
FILES = [
    ("Languages/Br/DeploymentGroups/allies.json", "Traduzir descrições"),
    ("Languages/Br/DeploymentGroups/enemies.json", "Traduzir descrições"),
    ("Languages/Br/DeploymentGroups/villains.json", "Traduzir descrições"),
    ("Languages/Br/MissionCardText/core.json", "Textos de cartas"),
    ("Languages/Br/MissionCardText/bespin.json", "Textos de cartas"),
    ("Languages/Br/MissionCardText/empire.json", "Textos de cartas"),
    ("Languages/Br/MissionCardText/hoth.json", "Textos de cartas"),
    ("Languages/Br/MissionCardText/jabba.json", "Textos de cartas"),
    ("Languages/Br/MissionCardText/lothal.json", "Textos de cartas"),
    ("Languages/Br/MissionCardText/other.json", "Textos de cartas"),
    ("Languages/Br/MissionCardText/twin.json", "Textos de cartas"),
    ("Languages/Br/bonuseffects.json", "⚠️ VER REGRA ESPECIAL: Manter chaves em inglês antes dos dois pontos"),
    ("Languages/Br/events.json", "Traduzir descrições"),
    ("Languages/Br/help.json", "Traduzir textos de ajuda"),
    ("Languages/Br/instructions.json", "Traduzir instruções"),
    ("Languages/Br/ui.json", "Traduzir interface"),
]

TEMPLATE = """
## Traduzir Imperial Commander 2 → PT-BR: Fase 0 - {filename}

### 📋 Contexto
Traduzir o arquivo `{filename}` do Imperial Commander 2 de inglês para português brasileiro.
Este arquivo faz parte da "Fase 0" (Arquivos Base do Sistema).

### 📚 Recursos Obrigatórios (LER PRIMEIRO)
1. **Instruções do tradutor:** `.agent/agents/imperial-translator.md`
2. **Glossário e preservação:** `WORK/dicionarios/dicionario.json`
3. **Plano de execução:** `WORK/PLAN-traducao-ptbr.md`

### ⚠️ Instrução de Branch (IMPORTANTE)
**O Pull Request gerado deve obrigatoriamente ter como base (target) o branch `jules-translate` e NÃO o `main`.**

### 🎯 Tarefa
1. Traduzir o arquivo: `{filename}`
2. Garantir validação JSON.
3. Seguir rigorosamente as regras abaixo.

### 🔴 Regras Críticas
**Siga estritamente as diretrizes definidas em:** `.agent/agents/imperial-translator.md`

{special_rules}


### ✅ Validação
Após a tradução, execute:
```bash
python -m json.tool {filename}
```

### 📝 Formato do Commit
```
feat(i18n): traduzir {basename} para PT-BR (Fase 0)
```
"""

BONUS_EFFECTS_RULE = """
#### Regra Especial: bonuseffects.json
Manter o nome da habilidade em inglês antes dos dois pontos:
```
✅ "CHARGING UP: Depois que esta figura atacar..."
❌ "CARREGANDO: Depois que esta figura atacar..."
```
"""

def generate_body(filename, note):
    basename = filename.split('/')[-1]
    
    special_rules = ""
    if "bonuseffects.json" in filename:
        special_rules = BONUS_EFFECTS_RULE

    return TEMPLATE.format(
        filename=filename,
        basename=basename,
        special_rules=special_rules
    )

def main():
    parser = argparse.ArgumentParser(description='Generate Github Issues for Fase 0')
    parser.add_argument('--dry-run', action='store_true', help='Print commands instead of executing')
    parser.add_argument('--limit', type=int, default=0, help='Limit number of issues to create (0 for all)')
    parser.add_argument('--offset', type=int, default=0, help='Skip the first N items')
    args = parser.parse_args()

    files_to_process = FILES[args.offset : args.offset + args.limit if args.limit > 0 else None]
    
    print(f"Preparing to create {len(files_to_process)} issues (from total {len(FILES)})...")

    for i, (filename, note) in enumerate(files_to_process, args.offset + 1):
        title = f"Traduzir {filename} (Fase 0)"
        body = generate_body(filename, note)
        
        cmd = [
            "gh", "issue", "create",
            "--title", title,
            "--body", body,
            "--label", "translation,pt-br,fase-0,auto-generated,jules",
            "--assignee", "@me" 
        ]

        if args.dry_run:
            print(f"\n--- [DRY RUN] Issue {i}/{len(FILES)}: {title} ---")
            print("Command:", " ".join(cmd))
            print("Body Preview (first 200 chars):")
            print(body[:200] + "...")
        else:
            print(f"Creating issue {i}/{len(FILES)}: {title}...", end=" ", flush=True)
            try:
                result = subprocess.run(cmd, capture_output=True, text=True, check=True)
                print(f"✅ Success! {result.stdout.strip()}")
            except subprocess.CalledProcessError as e:
                print(f"❌ Error: {e.stderr}")

if __name__ == "__main__":
    main()
