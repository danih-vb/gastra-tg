# Capítulos do TG II em revisão

Os capítulos 4, 5 e 6 do `Gastra II.docx` são escritos aqui, em Markdown, porque o `.docx` é binário: o GitHub não
mostra a diferença, não dá para comentar linha a linha e duas pessoas editando o mesmo arquivo geram um conflito
que o Git não resolve.

| Arquivo | Capítulo | Issues |
|---|---|---|
| `Cap4_Engenharia_de_Software.md` | 4 Engenharia de software e projeto do sistema | #200–#203 |
| `Cap5_Desenvolvimento_Resultados_Testes.md` | 5 Desenvolvimento, resultados e testes | #226 |
| `Cap6_Consideracoes_Finais.md` | 6 Considerações finais | #227 |

## Convenções

- Estrutura e títulos do modelo oficial da Fatec (Modelo_Estrutura_TGII_ADS_Atualizado): o capítulo 5 tem só
  5.1 Apresentação do produto e telas e 5.2 Testes e validação do software, e a avaliação heurística fica em
  5.2.5, porque avaliou as telas já construídas.
- Título de seção (`##`, `###`) no mesmo formato do documento: `4.1 LEVANTAMENTO E ESPECIFICAÇÃO DE REQUISITOS`,
  `4.1.1 Requisitos funcionais`.
- Legenda acima, em negrito: `**Quadro 1 – …**`, `**Tabela 2 – …**`, `**Figura 5 – …**`; fonte abaixo:
  `Fonte: …`. Quadro para conteúdo textual, tabela para números (ABNT). A numeração continua a do capítulo 3
  (Figura 1 e Tabela 1 já existem).
- Citações só de obras que já estão nas referências do documento.
- Trechos marcados com **[A PREENCHER]**, **[SUBSTITUIR]** ou **[ATUALIZAR]** dependem de trabalho ainda não feito.

## Guia de inserção

`gerar_guia_de_insercao.py` gera `docs/pesquisa/GASTRA_TGII_Guia_de_Insercao.docx` sem tocar no `Gastra II.docx`:
o que falta no documento em relação ao modelo oficial, as entregas do cronograma, e o texto pronto de cada parte
(pré-textuais, capítulos 1 a 6, referências e apêndice), com a instrução de onde colar. O guia usa os estilos do
próprio documento, então o texto colado chega formatado. O mesmo script atualiza
`docs/pesquisa/referencias/GASTRA_Referencias_Bibliograficas.docx` com a lista completa de referências.

```
python docs/pesquisa/capitulos/gerar_guia_de_insercao.py
```

## Depois da revisão

Um script leva o texto aprovado para o `Gastra II.docx`, com os estilos do documento (Título 2 e 3, legenda com
numeração automática, fonte abaixo) e as imagens. É feito uma vez, com os três capítulos aprovados, para não
haver duas versões do mesmo texto sendo editadas ao mesmo tempo.

## Fase final: o `.docx` passou a ser a fonte

Desde 21/11/2026 o texto é ajustado direto no `Gastra II.docx`, por scripts que trocam trechos exatos e podem rodar
de novo sem duplicar nada. Os capítulos em Markdown desta pasta ficaram como registro da revisão e **não** têm essas
mudanças: rodar `levar_para_docx.py`, `inserir_capitulo_5.py` ou `inserir_capitulo_6.py` agora apagaria o que veio
depois.

| Script | O que faz |
|---|---|
| `registrar_cortesia.py` | RF26, UC26 e US25 (cortesia e ajuste de preço) |
| `registrar_fase_final.py` | RF27 a RF29, UC27 a UC29, US26 a US28, figuras da 5.1, Tabela 2 de 23/11 |
| `registrar_entrega_final.py` | menção honrosa, RF30 (QR codes das mesas), app no celular, Tabela 2 lida do relatório de testes |
| `revisar_travessoes.py` | revisão de estilo: travessões do texto corrido trocados por vírgula, parênteses ou dois-pontos |
| `inserir_apendice_a.py` | Apêndice A, o manual do usuário, com as capturas reais no lugar das telas do protótipo |
| `registrar_diagramas.py` | Figuras 2, 3, 5 e 6 (casos de uso, DER e modelo lógico da fase final), contagens do MER e do banco, Quadro 6 com a foto e a menção honrosa |

Os diagramas vêm de fora desta pasta: `docs/diagramas/_fontes/atualizar_uc_fase_final.py` (casos de uso),
`docs/modelagem/der/acrescentar_fase_final.js` e `acrescentar_atributos_restantes.js` (DER, no brModelo) e
`docs/modelagem/engenharia-reversa/` (modelo lógico). Atualize-os antes de rodar `registrar_diagramas.py`.

Depois de qualquer um deles, no Word: Ctrl+A e F9, e exportar o PDF.
