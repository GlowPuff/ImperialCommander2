#!/usr/bin/env python3
import subprocess
import argparse

TEMPLATE = """
## Traduzir Imperial Commander 2 → PT-BR: Fase 3 - {basename}

### 📋 Contexto
Traduzir a missão Core `{source_filename}` para `{target_filename}`.

### 📚 Recursos Obrigatórios (LER PRIMEIRO)
1. **Instruções do tradutor:** `.agent/agents/imperial-translator.md`
2. **Glossário e preservação:** `WORK/dicionarios/dicionario.json`

### ⚠️ Instrução de Branch (IMPORTANTE)
**O Pull Request gerado deve obrigatoriamente ter como base (target) o branch `jules-translate` e NÃO o `main`.**

### 🎯 Tarefa
1. **Origem:** `SagaMissions/Core/{source_filename}`
2. **Destino:** `ImperialCommander2/Assets/Resources/Languages/Br/Missions/Core/{target_filename}`
   *(Se o arquivo de destino não existir, copie da origem e renomeie)*
3. **Traduzir** o conteúdo respeitando as regras.
4. **Atualizar Metadados:**
   - `languageID`: "Portuguese Brazilian (BR)"
   - `saveDate`: data atual

### 🔑 Campos Principais
`missionDescription`, `missionInfo`, `eventText`, `choiceText`, `buttonText`, `customInstructions`, `imperialRewardText`, `rebelsRewardText`

### 🔴 Regras Críticas
**Siga estritamente as diretrizes definidas em:** `.agent/agents/imperial-translator.md`

### ✅ Validação
Após a tradução, execute:
```bash
python -m json.tool ImperialCommander2/Assets/Resources/Languages/Br/Missions/Core/{target_filename} > /dev/null && echo "✅ JSON válido"
grep '"languageID"' ImperialCommander2/Assets/Resources/Languages/Br/Missions/Core/{target_filename}
```

### 📝 Formato do Commit
```
feat(i18n): traduzir {basename} para PT-BR (Fase 3)
```
"""

def main():
    parser = argparse.ArgumentParser(description='Generate Github Issues for Fase 3 (Core)')
    parser.add_argument('--dry-run', action='store_true', help='Print commands instead of executing')
    parser.add_argument('--limit', type=int, default=0, help='Limit number of issues to create (0 for all)')
    parser.add_argument('--offset', type=int, default=0, help='Skip the first N items')
    
    args = parser.parse_args()

    # CORE missions are 1-indexed (CORE1 to CORE32)
    # Total 32 missions.
    TOTAL_MISSIONS = 32
    
    # Calculate range
    start_index = args.offset # 0
    end_index = TOTAL_MISSIONS
    if args.limit > 0:
        end_index = min(start_index + args.limit, TOTAL_MISSIONS)
    
    # ID = index + 1
    
    print(f"Preparing to create issues for Core Missions (Index {start_index} to {end_index})...")

    # Iterate from start_index to end_index (exclusive of end_index in range, so we need +1 for range)
    # But effectively we want `start_index + 1` to `end_index` as IDs.
    
    # Ex: offset=0, limit=2 -> start=0, end=2. IDs: 1, 2. (range(0,2) -> 0,1 -> +1 -> 1,2)
    
    for i in range(start_index, end_index):
        mission_id = i + 1
        source_filename = f"CORE{mission_id}.json"
        basename = f"CORE{mission_id}_BR.json"
        title = f"Traduzir {basename} (Fase 3)"
        
        body = TEMPLATE.format(
            source_filename=source_filename,
            target_filename=basename,
            basename=basename
        )
        
        cmd = [
            "gh", "issue", "create",
            "--title", title,
            "--body", body,
            "--label", "translation,pt-br,fase-3,missions,core,auto-generated,jules",
            "--assignee", "@me" 
        ]

        if args.dry_run:
            print(f"\n--- [DRY RUN] Issue Core {mission_id}: {title} ---")
            print("Command:", " ".join(cmd))
            print("Body Preview:")
            print(body[:300] + "...")
        else:
            print(f"Creating issue Core {mission_id}: {title}...", end=" ", flush=True)
            try:
                result = subprocess.run(cmd, capture_output=True, text=True, check=True)
                print(f"✅ Success! {result.stdout.strip()}")
            except subprocess.CalledProcessError as e:
                print(f"❌ Error: {e.stderr}")
            except FileNotFoundError:
                 print("❌ Error: 'gh' command not found. Please install GitHub CLI.")

if __name__ == "__main__":
    main()
