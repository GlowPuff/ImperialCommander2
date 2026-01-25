#!/usr/bin/env python3
import subprocess
import argparse

# List of files to translate in Fase 1
FILES = [
    "SagaTutorials/Br/TUTORIAL01.json",
    "SagaTutorials/Br/TUTORIAL02.json",
    "SagaTutorials/Br/TUTORIAL03.json"
]

TEMPLATE = """
## Traduzir Imperial Commander 2 → PT-BR: Fase 1 - {basename}

### 📋 Contexto
Traduzir o arquivo `{filename}` (Tutorial) do Imperial Commander 2 de inglês para português brasileiro.

### 📚 Recursos Obrigatórios (LER PRIMEIRO)
1. **Instruções do tradutor:** `.agent/agents/imperial-translator.md`
2. **Glossário e preservação:** `WORK/dicionarios/dicionario.json`

### ⚠️ Instrução de Branch (IMPORTANTE)
**O Pull Request gerado deve obrigatoriamente ter como base (target) o branch `jules-translate` e NÃO o `main`.**

### 🎯 Tarefa
1. Traduzir o arquivo: `{filename}`
2. **Atualizar Metadados (CRÍTICO):**
   - `languageID`: "Portuguese Brazilian (BR)"
   - `saveDate`: data atual (ex: "4/26/2024")

### 🔑 Campos Principais
`missionDescription`, `additionalMissionInfo`, `missionInfo`, `theText`, `eventText`, `buttonText`, `choiceText`

### 🔴 Regras Críticas
**Siga estritamente as diretrizes definidas em:** `.agent/agents/imperial-translator.md`

### ✅ Validação
Após a tradução, execute:
```bash
python -m json.tool {filename} > /dev/null && echo "✅ JSON válido"
grep '"languageID"' {filename}  # Deve mostrar "Portuguese Brazilian (BR)"
```

### 📝 Formato do Commit
```
feat(i18n): traduzir {basename} para PT-BR (Fase 1)
```
"""

def main():
    parser = argparse.ArgumentParser(description='Generate Github Issues for Fase 1')
    parser.add_argument('--dry-run', action='store_true', help='Print commands instead of executing')
    parser.add_argument('--limit', type=int, default=0, help='Limit number of issues to create (0 for all)')
    parser.add_argument('--offset', type=int, default=0, help='Skip the first N items')
    args = parser.parse_args()
    
    files_to_process = FILES[args.offset : args.offset + args.limit if args.limit > 0 else None]

    print(f"Preparing to create {len(files_to_process)} issues for Fase 1...")

    for i, filename in enumerate(files_to_process, args.offset + 1):
        basename = filename.split('/')[-1]
        title = f"Traduzir {basename} (Fase 1)"
        body = TEMPLATE.format(filename=filename, basename=basename)
        
        cmd = [
            "gh", "issue", "create",
            "--title", title,
            "--body", body,
            "--label", "translation,pt-br,fase-1,tutorials,auto-generated,jules",
            "--assignee", "@me" 
        ]

        if args.dry_run:
            print(f"\n--- [DRY RUN] Issue {i}/{len(FILES)}: {title} ---")
            print("Command:", " ".join(cmd))
            print("Body Preview:")
            print(body[:300] + "...")
        else:
            print(f"Creating issue {i}/{len(FILES)}: {title}...", end=" ", flush=True)
            try:
                result = subprocess.run(cmd, capture_output=True, text=True, check=True)
                print(f"✅ Success! {result.stdout.strip()}")
            except subprocess.CalledProcessError as e:
                print(f"❌ Error: {e.stderr}")

if __name__ == "__main__":
    main()
