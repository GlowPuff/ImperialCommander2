import json
import re
import os

def atualizar_dicionario_completo():
    # 1. Carregar ou criar o dicionário JSON base
    arquivo_json = 'dicionario.json'
    if os.path.exists(arquivo_json):
        with open(arquivo_json, 'r', encoding='utf-8') as f:
            data = json.load(f)
    else:
        data = {}

    # Garantir chaves principais
    if "lista_de_preservacao" not in data: data["lista_de_preservacao"] = {}
    if "glossario_de_traducao" not in data: data["glossario_de_traducao"] = {}

    preservacao = data["lista_de_preservacao"]
    glossario = data["glossario_de_traducao"]

    # Sets temporários para evitar duplicatas na preservação
    sets_preservacao = {
        "nomes_proprios_e_entidades": set(preservacao.get("nomes_proprios_e_entidades", [])),
        "habilidades_e_efeitos": set(preservacao.get("habilidades_e_efeitos", [])),
        "palavras_chave_e_surges": set(preservacao.get("palavras_chave_e_surges", [])),
        "itens_e_equipamentos": set(preservacao.get("itens_e_equipamentos", [])),
        "outros_termos": set(preservacao.get("outros_termos", []))
    }

    # Contadores
    novos_preservados = 0
    novas_traducoes = 0

    # ---------------------------------------------------------
    # PARTE A: Processar dicio_termos_categorizados.txt (Só Preservação)
    # ---------------------------------------------------------
    try:
        with open('dicio_termos_categorizados.txt', 'r', encoding='utf-8') as f:
            for linha in f:
                linha = linha.strip()
                if not linha or "TIPO - CATEGORIA" in linha: continue
                
                partes = linha.split(' - ')
                if len(partes) < 3: continue
                
                termo = partes[-1].strip()
                cat_raw = partes[1].strip().lower()

                if cat_raw in ["name", "subname", "villains", "allies", "heroes"]:
                    sets_preservacao["nomes_proprios_e_entidades"].add(termo)
                elif cat_raw in ["abilities", "ability"]:
                    sets_preservacao["habilidades_e_efeitos"].add(termo)
                elif cat_raw in ["surges", "traits", "keywords", "surge"]:
                    sets_preservacao["palavras_chave_e_surges"].add(termo)
                elif cat_raw in ["items", "equipment", "weapons"]:
                    sets_preservacao["itens_e_equipamentos"].add(termo)
                else:
                    sets_preservacao["outros_termos"].add(termo)
                novos_preservados += 1
    except FileNotFoundError:
        print("Aviso: dicio_termos_categorizados.txt não encontrado.")

    # ---------------------------------------------------------
    # PARTE B: Processar dicionario_imperial_assault_termos.txt (Tradução e Preservação)
    # ---------------------------------------------------------
    # Mapeamento de categorias do arquivo txt para as chaves do JSON
    mapa_cat_glossario = {
        "Ações": "Ações e Mecânicas",
        "Atributos": "Ações e Mecânicas",
        "Palavras-chaves": "Termos de Jogo",
        "Cartas": "Cartas e Componentes",
        "Tokens": "Cartas e Componentes",
        "Condições": "Condições (Estados)",
        "Cenário": "Termos de Jogo"
    }

    try:
        with open('dicionario_imperial_assault_termos.txt', 'r', encoding='utf-8') as f:
            linhas = f.readlines()
            
        for linha in linhas:
            linha = linha.strip()
            # Ignora linhas vazias ou cabeçalhos
            if not linha or "Categoria:" in linha or "Notas Explicativas" in linha: continue
            
            # Tenta separar por tabulação múltipla ou espaços longos
            # Regex busca: (Categoria) espaço (Termo Original) espaço (Tradução)
            # Como o formato é visual, vamos tentar splitar por 2+ espaços ou tabs
            partes = re.split(r'\s{2,}|\t+', linha)
            
            if len(partes) >= 3:
                categoria_txt = partes[0].strip()
                termo_ing = partes[1].strip()
                termo_pt = partes[2].strip()
                
                # CASO 1: Tradução é "-" -> Vai para PRESERVAÇÃO
                if termo_pt == "-":
                    sets_preservacao["palavras_chave_e_surges"].add(termo_ing)
                    novos_preservados += 1
                
                # CASO 2: Tradução Válida -> Vai para GLOSSÁRIO
                else:
                    chave_json = mapa_cat_glossario.get(categoria_txt, "Outros Termos")
                    if chave_json not in glossario:
                        glossario[chave_json] = {}
                    
                    # Só adiciona se não existir ou atualiza
                    glossario[chave_json][termo_ing] = termo_pt
                    novas_traducoes += 1
                    
    except FileNotFoundError:
        print("Aviso: dicionario_imperial_assault_termos.txt não encontrado.")

    # 3. Consolidação Final
    for key, valor_set in sets_preservacao.items():
        data["lista_de_preservacao"][key] = sorted(list(valor_set))
    
    data["glossario_de_traducao"] = glossario

    # 4. Salvar
    with open('dicionario.json', 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

    print(f"PROCESSAMENTO CONCLUÍDO:")
    print(f"- Termos verificados para preservação (Total): {sum(len(s) for s in sets_preservacao.values())}")
    print(f"- Termos de tradução inseridos/atualizados: {novas_traducoes}")
    print(f"- Arquivo 'dicionario.json' atualizado com sucesso.")

if __name__ == "__main__":
    atualizar_dicionario_completo()