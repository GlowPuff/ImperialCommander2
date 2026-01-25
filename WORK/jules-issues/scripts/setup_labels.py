#!/usr/bin/env python3
import subprocess

REQUIRED_LABELS = [
    # Core labeling
    {"name": "jules", "color": "0E8A16", "desc": "Auto-processed by Jules Agent"},
    {"name": "auto-generated", "color": "D4C5F9", "desc": "Created by automation scripts"},
    {"name": "translation", "color": "1D76DB", "desc": "Related to translation tasks"},
    {"name": "pt-br", "color": "5319E7", "desc": "Target language: Portuguese (BR)"},
    
    # Phase specific
    {"name": "fase-0", "color": "BFDADC", "desc": "Phase 0: System Files"},
    {"name": "fase-1", "color": "BFDADC", "desc": "Phase 1: Tutorials"},
    {"name": "fase-2", "color": "BFDADC", "desc": "Phase 2: Mission Text"},
    {"name": "fase-3", "color": "BFDADC", "desc": "Phase 3: Core Missions"},
    {"name": "fase-4", "color": "BFDADC", "desc": "Phase 4: Expansions"},
    
    # Content specific
    {"name": "tutorials", "color": "F9D0C4", "desc": "Tutorial files"},
    {"name": "mission-text", "color": "F9D0C4", "desc": "Mission text files"},
    {"name": "missions", "color": "F9D0C4", "desc": "Mission JSON files"},
    {"name": "core", "color": "D4C5F9", "desc": "Core campaign"},
    
    # Expansion specific
    {"name": "jabba", "color": "C2E0C6", "desc": "Jabba's Realm"},
    {"name": "hoth", "color": "C2E0C6", "desc": "Return to Hoth"},
    {"name": "bespin", "color": "C2E0C6", "desc": "Bespin Gambit"},
    {"name": "lothal", "color": "C2E0C6", "desc": "Tyrants of Lothal"},
    {"name": "twin", "color": "C2E0C6", "desc": "Twin Shadows"},
    {"name": "empire", "color": "C2E0C6", "desc": "Heart of the Empire"},
    {"name": "other", "color": "C2E0C6", "desc": "Other missions"},
]

def create_labels():
    print("🎨 Creating missing GitHub Labels...")
    
    existing_labels_output = subprocess.run(
        ["gh", "label", "list", "--limit", "100", "--json", "name"],
        capture_output=True, text=True
    ).stdout
    
    # Simple set check might be noisy, let's just try to create/edit
    for label in REQUIRED_LABELS:
        name = label["name"]
        color = label["color"]
        desc = label["desc"]
        
        # Check if exists (simple check)
        if f'"{name}"' not in existing_labels_output:
            print(f"Creating label: {name}...", end=" ")
            try:
                subprocess.run(
                    ["gh", "label", "create", name, "--color", color, "--description", desc, "--force"],
                    check=True, capture_output=True
                )
                print("✅")
            except subprocess.CalledProcessError:
                print("❌ Failed")
        else:
            print(f"Skipping {name} (already exists)")

if __name__ == "__main__":
    create_labels()
