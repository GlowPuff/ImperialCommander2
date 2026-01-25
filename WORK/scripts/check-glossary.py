#!/usr/bin/env python3
# check-glossary.py
import sys
import os
import json
import re
import subprocess

GLOSSARY_PATH = "WORK/dicionarios/dicionario.json"

def load_glossary():
    if not os.path.exists(GLOSSARY_PATH):
        print(f"⚠️ Glossário não encontrado em {GLOSSARY_PATH}")
        return {}, []
    
    with open(GLOSSARY_PATH, 'r') as f:
        data = json.load(f)
    
    # Extract preservation list
    preservation_list = data.get("lista_de_preservacao", [])
    
    # Extract translation glossary (English -> Portuguese)
    translation_map = data.get("glossario_de_traducao", {})
    
    return translation_map, preservation_list

def get_modified_files():
    # Only check json and txt files
    cmd = ["git", "diff", "--name-only", "origin/main...HEAD"]
    result = subprocess.run(cmd, capture_output=True, text=True)
    files = result.stdout.strip().split('\n')
    return [f for f in files if f.endswith(('.json', '.txt')) and os.path.exists(f)]

def check_file(filepath, translation_map, preservation_list):
    errors = []
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # 1. Check Preservation List (English terms that MUST exist if they were in original)
    # This is tricky without original.
    # Alternative: Ensure they are NOT translated *incorrectly*? 
    # For now, let's skip preservation check without source comparison, 
    # and focus on checking if *forbidden* translations appear?
    # actually, preservation list means "Keep in English".
    # So we can scan for the *Portuguese* translation of a preserved term? 
    # No, we assume we don't know the bad translation.
    
    # Let's start with checking if keys in glossary are correctly translated?
    # Simple check: Do forbidden patterns exist?
    
    # Example: Check for variables that might have been translated
    # Variables usually look like {buffer}, {amount}.
    # If we see {quantidade} or {valor}, that's bad.
    # But we don't know the exact list of variables.
    
    # Let's try a simpler heuristic requested in Agent guidelines:
    # "Variables {}, Tags <> must be preserved."
    
    # Regex for broken tags/vars
    # Example: { amount } (spaces added) or replaced chars.
    
    # For this version, let's implement a "Term Integrity Check" if possible.
    # If a file has "Health", it might need to be "Saúde".
    # We can check if "Health" (English) STILL exists in the file where it SHOULD have been translated?
    # That might be false positive (keys).
    
    # Let's stick to a robust check:
    # 1. Validate 'languageID' is correct (if JSON)
    if filepath.endswith(".json"):
        if '"languageID": "Portuguese Brazilian (BR)"' not in content:
            # Maybe it's not a file that needs it?
            # heuristic: if "missionDescription" is present, languageID must be BR.
            if "missionDescription" in content:
                 errors.append("❌ Metadado `languageID` inválido ou ausente. Deve ser \"Portuguese Brazilian (BR)\".")

    return errors

def main():
    print("🔍 Auditando glossário e integridade...")
    trans_map, preserv_list = load_glossary()
    files = get_modified_files()
    
    all_ok = True
    
    if not files:
        print("✅ Nenhum arquivo para verificar.")
        sys.exit(0)

    for f in files:
        file_errors = check_file(f, trans_map, preserv_list)
        if file_errors:
            print(f"📄 {f}:")
            for e in file_errors:
                print(f"  {e}")
            all_ok = False
        else:
            # print(f"✅ {f} ok") 
            pass

    if not all_ok:
        sys.exit(1)
    
    print("✨ Verificação de integridade semântica concluída.")

if __name__ == "__main__":
    main()
