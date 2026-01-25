#!/usr/bin/env python3
import subprocess
import argparse
import os
import re

# Mapping of Phase Files to Labels
# We can extract labels from the file content or define them here.
# Extracting is robust.

PHASE_FILES_DIR = "../" # Relative to scripts/

def get_phase_files():
    files = []
    # List files in WORK/jules-issues/ matching ISSUE_FASE*.md
    # but exclude FASE0, FASE1, FASE3 which we handled separately (or maybe we can handle them here too if adapted, but let's stick to expansions for now as requested)
    # Actually, let's look for ISSUE_FASE*_*.md where * is an expansion name
    
    try:
        all_files = os.listdir(os.path.join(os.path.dirname(__file__), PHASE_FILES_DIR))
    except FileNotFoundError:
        # Fallback if running from proper root
        all_files = os.listdir("WORK/jules-issues/")

    for f in sorted(all_files):
        if f.startswith("ISSUE_FASE") and f.endswith(".md") and "TEMPLATE" not in f and "CORE" not in f and "FASE0" not in f and "FASE1.md" not in f:
            files.append(f)
    return files

def parse_labels(content):
    # wrapper to find `translation`, `pt-br`...
    match = re.search(r'`(translation.*)`', content)
    if match:
        return match.group(1).replace('`', '').replace(' ', '')
    return "translation,pt-br,auto-generated"

def parse_title(content):
    # Extract first header
    lines = content.split('\n')
    for line in lines:
        if line.startswith("# "):
            return line.replace("# ", "").strip()
    return "Translation Issue"

def main():
    parser = argparse.ArgumentParser(description='Generate Github Issues for Expansions (Fase 4-10)')
    parser.add_argument('--dry-run', action='store_true', help='Print commands instead of executing')
    parser.add_argument('--limit', type=int, default=0, help='Limit number of issues to create (0 for all)')
    parser.add_argument('--offset', type=int, default=0, help='Skip the first N items')
    args = parser.parse_args()

    script_dir = os.path.dirname(os.path.abspath(__file__))
    issues_dir = os.path.abspath(os.path.join(script_dir, PHASE_FILES_DIR))

    files = get_phase_files()
    
    files_to_process = files[args.offset : args.offset + args.limit if args.limit > 0 else None]
    
    print(f"Found {len(files)} total expansion phase files.")
    print(f"Processing {len(files_to_process)} files (offset={args.offset}, limit={args.limit}).")

    for filename in files_to_process:
        filepath = os.path.join(issues_dir, filename)
        with open(filepath, 'r') as f:
            content = f.read()

        title = parse_title(content)
        labels = parse_labels(content)
        # Add auto-generated label
        if "auto-generated" not in labels:
            labels += ",auto-generated,jules"
        elif "jules" not in labels:
            labels += ",jules"

        cmd = [
            "gh", "issue", "create",
            "--title", title,
            "--body", content,
            "--label", labels,
            "--assignee", "@me"
        ]

        if args.dry_run:
            print(f"\n--- [DRY RUN] File: {filename} ---")
            print(f"Title: {title}")
            print(f"Labels: {labels}")
            print("Command:", " ".join(cmd))
        else:
            print(f"Creating issue for {filename}...", end=" ", flush=True)
            try:
                subprocess.run(cmd, check=True, capture_output=True)
                print("✅ Success")
            except subprocess.CalledProcessError as e:
                print(f"❌ Error: {e}")
            except FileNotFoundError:
                print("❌ Error: 'gh' CLI not found.")

if __name__ == "__main__":
    main()
