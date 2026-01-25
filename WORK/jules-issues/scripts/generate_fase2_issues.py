#!/usr/bin/env python3
import subprocess
import argparse
import os

TEMPLATE_FILE = "WORK/jules-issues/ISSUE_FASE2.md"
SOURCE_DIR = "ImperialCommander2/Assets/Resources/Languages/En/MissionText"

def read_template():
    with open(TEMPLATE_FILE, 'r') as f:
        return f.read()

def get_mission_files():
    files = []
    if not os.path.exists(SOURCE_DIR):
        print(f"❌ Error: Source directory {SOURCE_DIR} not found.")
        return []
        
    for f in sorted(os.listdir(SOURCE_DIR)):
        if f.endswith(".txt") and not f.endswith(".meta"):
            files.append(f)
    return files

def main():
    parser = argparse.ArgumentParser(description='Generate Github Issues for Fase 2 (MissionText)')
    parser.add_argument('--dry-run', action='store_true', help='Print commands instead of executing')
    parser.add_argument('--limit', type=int, default=0, help='Limit number of issues to create (0 for all)')
    parser.add_argument('--offset', type=int, default=0, help='Skip the first N items')
    args = parser.parse_args()

    template_content = read_template()
    files = get_mission_files()
    
    # Slice files based on batch arguments
    start = args.offset
    end = None
    if args.limit > 0:
        end = start + args.limit
    
    target_files = files[start:end]

    print(f"Found {len(files)} total text files.")
    print(f"Preparing to create {len(target_files)} issues (from index {start} to {end if end else len(files)})...")

    for i, filename in enumerate(target_files, start + 1):
        title = f"Traduzir {filename} (Fase 2)"
        
        # Replace placeholders in template
        body = template_content.replace("{filename}", filename)
        
        cmd = [
            "gh", "issue", "create",
            "--title", title,
            "--body", body,
            "--label", "translation,pt-br,fase-2,mission-text,auto-generated,jules",
            "--assignee", "@me" 
        ]

        if args.dry_run:
            print(f"\n--- [DRY RUN] Issue {i}: {title} ---")
            print("Command:", " ".join(cmd))
            print("Body Preview:")
            print(body[:200] + "...")
        else:
            print(f"Creating issue {i}: {title}...", end=" ", flush=True)
            try:
                subprocess.run(cmd, capture_output=True, text=True, check=True)
                print("✅ Success")
            except subprocess.CalledProcessError as e:
                print(f"❌ Error: {e.stderr}")
            except FileNotFoundError:
                print("❌ Error: 'gh' command not found.")

if __name__ == "__main__":
    main()
