#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import json
from pathlib import Path

# Translations map (Source Text -> Translated Text)
TRANSLATIONS = {
    "HERO OF THE REBELLION": "HERÓI DA REBELIÃO",
    "Perform a {P} attack using 1 red and 1 yellow die. This attack gains Pierce 3.": "Realize um ataque {P} usando 1 dado vermelho e 1 amarelo. Este ataque ganha Pierce 3.",
    "While another friendly figure within 3 spaces is attacking, it may reroll 1 die.": "Enquanto outra figura aliada a até 3 espaços estiver atacando, ela pode rerrolar 1 dado.",
    "If you have not exited your space during this activation, apply +1 {H} and +2 Accuracy to your attack results.": "Se você não saiu do seu espaço durante esta ativação, aplique +1 {H} e +2 Accuracy aos resultados do seu ataque.",
    "You can trigger the same {B} ability up to twice per attack.": "Você pode acionar a mesma habilidade {B} até duas vezes por ataque.",
    "LOYAL WOOKIEE": "WOOKIEE LEAL",
    "Choose 1 adjacent hostile figure and roll 1 red die. That figure suffers {H} equal to the {H} results. Then, if it is a small figure, you may push it 1 space.": "Escolha 1 figura inimiga adjacente e role 1 dado vermelho. Essa figura sofre {H} igual aos resultados de {H}. Então, se for uma figura pequena, você pode empurrá-la 1 espaço.",
    "While a friendly figure is defending, and you are adjacent to the targeted space, apply +1 {G} to the defense results. Limit 1 \"Protector\"ability used per attack.": "Enquanto uma figura aliada estiver defendendo, e você estiver adjacente ao espaço alvo, aplique +1 {G} aos resultados de defesa. Limite de 1 habilidade \"Protector\" usada por ataque.",
    "SCOUNDREL": "CANALHA",
    "After an attack targeting you is resolved, if you did not suffer any {H}, you can interrupt to perform an attack targeting that attacker. Limit once per round.": "Após um ataque visando você ser resolvido, se você não sofreu nenhum {H}, você pode interromper para realizar um ataque visando aquele atacante. Limite de uma vez por rodada.",
    "While a friendly figure is defending, and you are adjacent to the targeted space, apply +1 {F} to the defense results.": "Enquanto uma figura aliada estiver defendendo, e você estiver adjacente ao espaço alvo, aplique +1 {F} aos resultados de defesa.",
    "While defending, apply +1 {G} to the defense results for each {F} result.": "Enquanto defende, aplique +1 {G} aos resultados de defesa para cada resultado {F}.",
    "HUMAN-CYBORG RELATIONS": "RELAÇÕES HUMANO-CIBORGUE",
    "Choose an adjacent friendly figure. That figure becomes Focused.": "Escolha uma figura aliada adjacente. Essa figura torna-se Focada.",
    "While defending, while adjacent to a friendly figure, you may reroll 1 defense die.": "Enquanto defende, enquanto adjacente a uma figura aliada, você pode rerrolar 1 dado de defesa.",
    "You cannot attack.": "Você não pode atacar.",
    "LOYAL ASTROMECH": "ASTROMECH LEAL",
    "You or an adjacent friendly DROID or VEHICLE recovers 1 {H}.": "Você ou um DROID ou VEHICLE aliado adjacente recupera 1 {H}.",
    "You can perform {I} tests on objects on which heroes can perform {I} tests(elite figures receive 1 success).": "Você pode realizar testes de {I} em objetos nos quais heróis podem realizar testes de {I} (figuras de elite recebem 1 sucesso).",
    "While defending, if you roll a blank result, add +1 {E} to the defense results.": "Enquanto defende, se você rolar um resultado em branco, adicione +1 {E} aos resultados de defesa.",
    "REBEL COMMANDER": "COMANDANTE REBELDE",
    "Perform an attack, then choose another friendly figure within 3 spaces. That figure may interrupt to perform an attack with the same target.": "Realize um ataque, então escolha outra figura aliada a até 3 espaços. Essa figura pode interromper para realizar um ataque com o mesmo alvo.",
    "Place 1 strain token on any enemy deployment group. The next time this group activates, discard the token and ignore its Bonus Effect.": "Coloque 1 ficha de tensão em qualquer grupo de posicionamento inimigo. A próxima vez que este grupo ativar, descarte a ficha e ignore seu Efeito Bônus.",
    "While attacking, if the target is within 3 spaces of you, you may replace 1 blue die in your attack pool with 1 red die.": "Enquanto ataca, se o alvo estiver a até 3 espaços de você, você pode substituir 1 dado azul na sua parada de ataque por 1 dado vermelho.",
    "You ignore additional movement point costs for difficult terrain and hostile figures.": "Você ignora custos adicionais de pontos de movimento para terreno difícil e figuras inimigas.",
    "CHARMING GAMBLER": "JOGADOR CHARMOSO",
    "While attacking or defending, you may reroll 1 of your attack or defense dice.": "Enquanto ataca ou defende, você pode rerrolar 1 dos seus dados de ataque ou defesa.",
    "Before you reroll a die, you may replace it with another die of the same type. After rolling, the new die is considered rerolled.": "Antes de rerrolar um dado, você pode substituí-lo por outro dado do mesmo tipo. Após rolar, o novo dado é considerado rerrolado.",
    "JEDI KNIGHT": "CAVALEIRO JEDI",
    "After a {O} attack targeting you or an adjacent friendly figure resolves, a hostile figure of your choice in your line of site suffers 1 {H}.": "Após um ataque {O} visando você ou uma figura aliada adjacente ser resolvido, uma figura inimiga de sua escolha em sua linha de visão sofre 1 {H}.",
    "Once during your activation, you may perform an attack without spending an action.": "Uma vez durante sua ativação, você pode realizar um ataque sem gastar uma ação.",
    "After you resolve an attack, if the defender was defeated, become Hidden.": "Após você resolver um ataque, se o defensor foi derrotado, torne-se Escondido.",
    "While attacking, if the target space is 5 or more spaces away, you may reroll 1 attack die.": "Enquanto ataca, se o espaço alvo estiver a 5 ou mais espaços de distância, você pode rerrolar 1 dado de ataque.",
    "REBEL INSTIGATOR": "INSTIGADORA REBELDE",
    "Place your figure in an empty space within 6 spaces.": "Posicione sua figura em um espaço vazio a até 6 espaços.",
    "At the start of your activation, you may gain 2 movement points or 1 POWER-{F}.": "No início da sua ativação, você pode receber 2 pontos de movimento ou 1 POWER-{F}.",
    "While attacking, you may reroll all attack dice or force the defender to reroll all defense dice.": "Enquanto ataca, você pode rerrolar todos os dados de ataque ou forçar o defensor a rerrolar todos os dados de defesa.",
    "SPECTRE-6": "SPECTRE-6",
    "At the start of each round, move up to 4 spaces.": "No início de cada rodada, mova até 4 espaços.",
    "While attacking, choose a hero within 3 spaces. For each non-red die in ist {J} pool, you may reroll 1 attack die.": "Enquanto ataca, escolha um herói a até 3 espaços. Para cada dado não vermelho na parada de {J} dele, você pode rerrolar 1 dado de ataque.",
    "SPECTRE-1": "SPECTRE-1",
    "After this activation, instead of having the app decide the next Imperial activation randomly, select an enemy group and manually activate it. This counts as the next Imperial activation.": "Após esta ativação, em vez de o aplicativo decidir a próxima ativação Imperial aleatoriamente, selecione um grupo inimigo e ative-o manualmente. Isso conta como a próxima ativação Imperial.",
    "While a friendly figure within 3 spaces is defending, it may reroll 1 defense die. If it does, convert each {E} result to 2 {G} and 1 {F} and, if that figure does not have a {P} weapon or attack type, you suffer 1 {H}.": "Enquanto uma figura aliada a até 3 espaços estiver defendendo, ela pode rerrolar 1 dado de defesa. Se fizer isso, converta cada resultado {E} em 2 {G} e 1 {F} e, se aquela figura não tiver uma arma ou tipo de ataque {P}, você sofre 1 {H}.",
    "SPECTRE-5": "SPECTRE-5",
    "Move up to 2 spaces, then recover 2 {H}.": "Mova-se até 2 espaços, depois recupere 2 {H}.",
    "Once during your activation, you may choose a space within 2 spaces and roll 1 green die. Each other figure and object on or adjacent to that space suffers {H} equal to the {H} results.": "Uma vez durante sua ativação, você pode escolher um espaço a até 2 espaços e rolar 1 dado verde. Cada outra figura e objeto naquele espaço ou adjacente a ele sofre {H} igual aos resultados de {H}.",
    "SPECTRE-4": "SPECTRE-4",
    "Once during your activation, you may perform a {P} attack using 2 red dice without spending an action.": "Uma vez durante sua ativação, você pode realizar um ataque {P} usando 2 dados vermelhos sem gastar uma ação."
}

def main():
    batch_path = Path("WORK/traduzindo/batch_0.json")
    output_path = Path("WORK/traduzindo/batch_0_translated.json")
    
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
