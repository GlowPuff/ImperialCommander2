# Automação de Issues para Jules - Tradução Imperial Commander 2

Este diretório contém templates de issues otimizados e **scripts de automação** para dividir o trabalho de tradução em tarefas atômicas e paralelas para o agente Jules.

> **Para entender o ciclo de vida completo (Issue -> PR -> Revisão -> Merge), leia:** [AGENT.md](../../AGENT.md).

## 🚀 Como Usar (Automação)

A melhor forma de criar as issues é usando os scripts Python. Eles geram issues individuais ou em lote, garantindo que o contexto seja isolado e evitando gargalos em Pull Requests gigantes.

### Pré-requisitos
1. **GitHub CLI (`gh`)** instalado e autenticado.
2. **Branch de Destino:** `jules-translate` deve existir.
3. **Secret `JULES_PAT` (Personal Access Token):** Configurado nos Settings do repositório.
    *   Necessário para que os commits do Jules disparem novos workflows (validação recursiva).
    *   Permissões: `repo` (full control) e `workflow`.

```bash
git checkout main
git pull
git checkout -b jules-translate
git push -u origin jules-translate
```

### 🛠️ Scripts Disponíveis

Todos os scripts estão em `WORK/jules-issues/scripts/`.

#### 1. Arquivos de Sistema (Fase 0)
Gera 16 issues separadas para arquivos críticos do sistema (`ui.json`, `events.json`, etc).

```bash
./WORK/jules-issues/scripts/generate_fase0_issues.py
```

#### 2. Tutoriais (Fase 1)
Gera 3 issues para os tutoriais, com regras específicas de metadados (`languageID`).

```bash
./WORK/jules-issues/scripts/generate_fase1_issues.py
```

#### 3. MissionText (Fase 2)
Gera issues para os arquivos de texto (`.txt`) das missões.
*Atenção:* São ~276 arquivos. Recomenda-se gerar em lotes.

```bash
# Gerar apenas os primeiros 10
./WORK/jules-issues/scripts/generate_fase2_issues.py --limit 10

# Gerar do 11 ao 20
./WORK/jules-issues/scripts/generate_fase2_issues.py --offset 10 --limit 10
```

#### 3. Missões Core (Fase 3)
Gera 32 issues para as missões da campanha base.

```bash
# Gerar todas
./WORK/jules-issues/scripts/generate_fase3_issues.py

# Testar apenas as primeiras 5 (Offset 0, Limit 5)
./WORK/jules-issues/scripts/generate_fase3_issues.py --limit 5
```

#### 4. Expansões (Fases 4-10)
Script mestre que lê os arquivos `ISSUE_FASE*_*.md` (Jabba, Hoth, Empire, etc.) e gera todas as issues correspondentes (~66 issues).

```bash
./WORK/jules-issues/scripts/generate_expansion_issues.py
```

---

## 📂 Estrutura dos Templates

Os templates Markdown (`ISSUE_FASE*.md`) foram refatorados para serem **"Clean Templates"**.

Eles contêm apenas:
1. **Contexto Específico:** Qual arquivo traduzir.
2. **Instruções de Branch:** Apontando para `jules-translate`.
3. **Links de Referência:** Apontando para `.agent/agents/imperial-translator.md` como a "Fonte da Verdade" para regras de tradução.

Isso evita redundância e garante que, se as regras mudarem, basta atualizar o arquivo do agente, e não 10 templates de issue.

| Template/Fase | Conteúdo | Status |
|---------------|----------|--------|
| `ISSUE_FASE0.md` | Arquivos Base | ✅ Automatizado |
| `ISSUE_FASE1.md` | Tutoriais | ✅ Automatizado |
| `ISSUE_FASE2.md` | MissionText | ✅ Automatizado (com lotes) |
| `ISSUE_FASE3_CORE.md` | Missões Core | ✅ Automatizado |
| `ISSUE_FASE4_JABBA.md` | Jabba's Realm | ✅ Automatizado |
| `ISSUE_FASE5_HOTH.md` | Return to Hoth | ✅ Automatizado |
| `ISSUE_FASE6_EMPIRE.md` | Heart of the Empire | ✅ Automatizado |
| `ISSUE_FASE7_LOTHAL.md` | Tyrants of Lothal | ✅ Automatizado |
| `ISSUE_FASE8_TWIN.md` | Twin Shadows | ✅ Automatizado |
| `ISSUE_FASE9_BESPIN.md` | Bespin Gambit | ✅ Automatizado |
| `ISSUE_FASE10_OTHER.md` | Outras Missões | ✅ Automatizado |
