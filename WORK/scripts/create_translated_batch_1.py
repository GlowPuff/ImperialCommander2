#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import json
from pathlib import Path

# Translations map (Source Text -> Translated Text)
TRANSLATIONS = {
    "While another friendly figure within 3 spaces is performing an attribute test, apply +1 {B} to the results.": "Enquanto outra figura aliada a até 3 espaços estiver realizando um teste de atributo, aplique +1 {B} aos resultados.",
    "Once during your activation, you may choose up to 2 hostile figures within 4 spaces of you. Push each of those figures 1 space.": "Uma vez durante sua ativação, você pode escolher até 2 figuras inimigas a até 4 espaços de você. Empurre cada uma dessas figuras 1 espaço.",
    "SPECTRE-2": "SPECTRE-2",
    "While another friendly figure within 3 spaces is attacking, you may apply +2 Accuracy, +1 {H}, or +1 {B} to the attack results. Limit once per round.": "Enquanto outra figura aliada a até 3 espaços estiver atacando, você pode aplicar +2 Accuracy, +1 {H} ou +1 {B} aos resultados do ataque. Limite de uma vez por rodada.",
    "After deployment, you and each adjacent friendly figure gains 1 movement point.": "Após o posicionamento, você e cada figura aliada adjacente ganham 1 ponto de movimento.",
    "\"CHOPPER\"": "\"CHOPPER\"",
    "Move up to 2 spaces, then choose an adjacent figure. If that figure is hostile, roll 1 green die. It suffers {H} equal to the {H} results. Then, if that figure is small, push it up to 1 space.": "Mova-se até 2 espaços, então escolha uma figura adjacente. Se essa figura for inimiga, role 1 dado verde. Ela sofre {H} igual aos resultados de {H}. Então, se for uma figura pequena, empurre-a até 1 espaço.",
    "Use while on or adjacent to a terminal. Choose a figure on or adjacent to any terminal. That figure suffers 2 {H} and 1 {C}.": "Use enquanto estiver em ou adjacente a um terminal. Escolha uma figura em ou adjacente a qualquer terminal. Essa figura sofre 2 {H} e 1 {C}.",
    "Perform an interact, then move up to 2 spaces.": "Realize uma interação, então mova-se até 2 espaços.",
    "While defending, apply -2 Accuracy to the attack results. After an attack targeting you resolves, gain 2 movement points.": "Enquanto defende, aplique -2 Accuracy aos resultados do ataque. Após um ataque visando você ser resolvido, ganhe 2 pontos de movimento.",
    "Perform an interact, then move up to 3 spaces.": "Realize uma interação, então mova-se até 3 espaços.",
    "While attacking, if you have suffered 5 or more {H}, apply +1 {B} to the attack results.": "Enquanto ataca, se você tiver sofrido 5 ou mais {H}, aplique +1 {B} aos resultados do ataque.",
    "After you resolve an attack, if the defender was defeated, become Hidden.": "Após você resolver um ataque, se o defensor foi derrotado, torne-se Escondido.",
    "While attacking, if the target space is 5 or more spaces away, you may reroll 1 attack die.": "Enquanto ataca, se o espaço alvo estiver a 5 ou mais espaços de distância, você pode rerrolar 1 dado de ataque.",
    "You can trigger the same {B} ability up to twice per attack.": "Você pode acionar a mesma habilidade {B} até duas vezes por ataque.",
    "Figures do not block line of sight for this figure's attacks.": "Figuras não bloqueiam a linha de visão para os ataques desta figura.",
    "If you have not exited your space during this activation, apply +1 {H} and +2 Accuracy to your attack results.": "Se você não saiu do seu espaço durante esta ativação, aplique +1 {H} e +2 Accuracy aos resultados do seu ataque.",
    "Gain 4 movement points and become Focused.": "Ganhe 4 pontos de movimento e torne-se Focado."
}

def main():
    batch_path = Path("WORK/traduzindo/batch_1.json")
    output_path = Path("WORK/traduzindo/batch_1_translated.json")
    
    with open(batch_path, "r", encoding="utf-8") as f:
        data = json.load(f)
        
    for item in data["items"]:
        original = item["value"]
        if original in TRANSLATIONS:
            item["translated"] = TRANSLATIONS[original]
        else:
            print(f"WARNING: No translation found for: {original}")
            # Use original if no translation provided (or mark with TODO)
            item["translated"] = original
            
    with open(output_path, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        
    print(f"Created {output_path}")

if __name__ == "__main__":
    main()
