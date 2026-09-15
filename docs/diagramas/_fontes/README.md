# Arquivos-fonte dos diagramas

Os PNGs em `docs/diagramas/` e `docs/modelagem/der/` são **exportações**. O arquivo-fonte
editável de cada diagrama fica aqui, com o mesmo nome-base do PNG correspondente.

| PNG exportado | Fonte | Ferramenta |
|---|---|---|
| `casos-de-uso/GASTRA_UC_*.png` | `GASTRA_UC_*.drawio` | draw.io / diagrams.net |
| `classe/GASTRA_Classe_*.png` | `GASTRA_Classe_*.drawio` | draw.io / diagrams.net |
| `atividade/GASTRA_Atividade_*.png` | `GASTRA_Atividade_*.drawio` | draw.io / diagrams.net |
| `sequencia/GASTRA_Sequencia_*.png` | `GASTRA_Sequencia_*.vsdx` | Lucidchart |
| `negocio/GASTRA_Business_Model_Canvas.png` | `GASTRA_Business_Model_Canvas.drawio` | draw.io / diagrams.net |
| `../modelagem/der/GASTRA_DER.png` | `../modelagem/der/GASTRA_DER.brM3` | brModelo 3.2 |

**Regra:** todo PR que altera um diagrama tem que atualizar o PNG **e** o arquivo-fonte, no
mesmo commit. PNG sem fonte é diagrama que ninguém consegue mais corrigir.

> **Nota (migração em andamento):** a migração de Lucidchart para draw.io começou pelos
> diagramas de Casos de Uso, Atividade, Business Model Canvas e Classe. Sequência
> ainda tem fonte em `.vsdx` (Lucidchart) — migrar quando houver tempo dedicado a isso,
> não como efeito colateral de outro PR.