# PROMPT DE COMANDO: Tradução e Edição "In-Place" via Python

**Arquivo Alvo:** `ImperialCommander2/Assets/Resources/SagaTutorials/Br/TUTORIAL02.json`
*(Nota: Este arquivo deve ser traduzido diretamente no sistema de arquivos)*

**Referências Obrigatórias:**
1. **Vocabulário:** Use estritamente as traduções definidas em `dicionario.json`.
2. **Regras:** Siga as diretrizes gramaticais e de proteção de código do `SYSTEM.md`.
3. **Contexto:** Use o `manual.txt` apenas para desambiguação de sentido (entender a lógica da frase), mas nunca sobreponha o vocabulário do JSON de referência.

---

**MECÂNICA DE EXECUÇÃO (Python Obrigatório):**
Você aturará como um script de automação. Você **NÃO** deve tentar reescrever o arquivo de texto manualmente na janela de chat. Você deve usar **Python** para manipular a estrutura de dados JSON com segurança.

Execute o seguinte plano cíclico até que o arquivo esteja 100% traduzido:

### Passo 1: Análise e Carga (Python)
1. Crie e execute um script Python que carregue o arquivo JSON alvo usando `json.load`.
2. Identifique o total de objetos na lista principal ou a estrutura das chaves que contêm texto.
3. Defina um tamanho de lote seguro para processamento (ex: 50 itens por vez para evitar timeout).

### Passo 2: O Loop de Tradução e Edição (Iterativo)
Para cada lote sequencial (ex: 0-50, 51-100, etc.), faça o seguinte via script:
1.  **Ler:** Acesse os itens desse intervalo específico na memória.
2.  **Filtrar:** Selecione para tradução **APENAS** os valores das chaves de texto legível, como:
    * `saveDate` > 1/18/2026
    * `languageID` > Portuguese Brazilian (BR)
   
   **Chaves de Missão:**
    * `missionDescription`
    * `additionalMissionInfo`
    * `startingObjective`
    * `missionInfo`
    * `theText`
    * `eventText`
    * `choiceText`
    * `buttonText`
    * `customInstructions`
    * `descriptionText`
    * `bonusText`
    * `imperialRewardText`
    * `rebelsRewardText`

    **Chaves de Efeito Bonus:**
    * `effects` (texto de regras de efeitos)

    **Chaves de Evento:**
    * `eventFlavor`
    * `content`
    
    **Chaves de ajuda:**
    * `helpText`

    **Chaves de instruções:**
    * `instruction`

    **Chaves de Dados (Aliados/Inimigos/Itens):**
    * `subname`
    * `text` (dentro de objetos de habilidade)
    
3.  **Verificar Preservação:** Antes de traduzir, verifique via código se o termo ou nome próprio existe na chave `lista_de_preservacao` do arquivo de referência. Se existir, **não toque**.
4.  **Traduzir:** Traduza os valores filtrados para Português do Brasil, mantendo intactas todas as tags (`{val}`, `<tag>`, `\n`).
5.  **Atualizar:** Atualize os valores no objeto JSON carregado em memória.
6.  **Salvar (Commit):** Use `json.dump` (com `ensure_ascii=False` e `indent=4`) para reescrever o arquivo JSON no disco com os dados atualizados deste lote imediatamente.

### Passo 3: Continuidade Autônoma
* Após salvar um lote com sucesso, informe no chat apenas: "Lote X (Itens Y a Z) traduzido e salvo."
* **Prossiga automaticamente** para o próximo lote sem esperar confirmação do usuário, repetindo o ciclo até o final do arquivo.

**Inicie a execução do Passo 1 agora.**