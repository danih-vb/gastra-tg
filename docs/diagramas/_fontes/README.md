# Arquivos-fonte dos diagramas

Os PNGs em `docs/diagramas/` e `docs/modelagem/der/` são **exportações**. O arquivo-fonte
editável de cada diagrama fica aqui, com o mesmo nome-base do PNG correspondente.

| PNG exportado | Fonte | Ferramenta |
|---|---|---|
| `casos-de-uso/GASTRA_UC_*.png` | `GASTRA_UC_*.drawio` | draw.io / diagrams.net |
| `classe/GASTRA_Classe.png` | `GASTRA_Classe.drawio` | draw.io / diagrams.net |
| `atividade/GASTRA_Atividade_*.png` | `GASTRA_Atividade_*.drawio` | draw.io / diagrams.net |
| `sequencia/GASTRA_Sequencia_*.png` | `GASTRA_Sequencia_*.drawio` | draw.io / diagrams.net |
| `../modelagem/der/GASTRA_DER.png` | `../modelagem/der/GASTRA_DER.brM3` | brModelo 3.2 |

**Regra:** todo PR que altera um diagrama tem que atualizar o PNG **e** o arquivo-fonte, no
mesmo commit. PNG sem fonte é diagrama que ninguém consegue mais corrigir.
