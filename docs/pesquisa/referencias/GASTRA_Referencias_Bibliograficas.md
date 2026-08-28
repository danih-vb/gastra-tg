# GASTRA — Referências Bibliográficas (em construção)

TG — FATEC Araraquara | Autores: Daniel Velluto Bento e Pedro Luis Otrente de Campos | Orientador: Prof. Me. Leonardo José de Lima Ferrucci
Versão: 27/08/2026

---

## 1. Sobre este documento

Este arquivo reúne referências bibliográficas coletadas **durante o desenvolvimento** do GASTRA —
fontes usadas para embasar decisões técnicas específicas (ex.: escolha de formulação de PL,
referencial de metas de usabilidade), que vão além da revisão de literatura já fechada no
`Gastra.pdf` (projeto de pesquisa formal). Este documento não substitui a lista de referências final
do TG — é um repositório de trabalho, para não perder a fonte de uma decisão entre o momento em que
ela é tomada e a redação final do texto (Etapa 5, M7).

Cada entrada segue norma ABNT (NBR 6023) e traz uma frase de ponte explicando a conexão específica
com o GASTRA — não um resumo genérico da obra.

---

## 2. Referências por bloco temático

### 2.1 Bloco de Programação Linear (RN03, RF06–RF08)

**HILLIER, Frederick S.; LIEBERMAN, Gerald J. Introduction to Operations Research. 10. ed. New
York: McGraw-Hill, 2015.**

Conexão com o GASTRA: referência clássica para a formulação do Problema de Designação (Assignment
Problem), usada para embasar a estrutura final do RN03 — a alocação de garçons a praças como caso
particular de Programação Linear, com a propriedade de unimodularidade total que permite resolver
o problema via Simplex sem precisar de um solver de Programação Linear Inteira dedicado. Complementa
(não substitui) as referências já citadas no projeto de pesquisa formal para o mesmo bloco (ZANIOL,
2011; COELHO, 2021; VIEIRA et al., 2015).

*Nota:* confirmar edição exata disponível para consulta e ajustar a entrada ABNT completa (local de
publicação, ISBN) na redação final.

### 2.2 Bloco de Usabilidade / RNF01–RNF02

**NIELSEN, Jakob. Response Time: The 3 Important Limits. Nielsen Norman Group, 1993.**

Conexão com o GASTRA: base para a definição dos valores iniciais de RNF01 (tempo de reflexo do
pedido na comanda, meta de 2 segundos) e RNF02 (número de toques para abrir mesa e registrar
pedido, meta de 5 toques). Os limiares clássicos de Nielsen (resposta percebida como instantânea,
limite para manter o fluxo de atenção do usuário sem interrupção, e limite de perda de atenção)
justificam por que a meta de RNF01 não foi escolhida arbitrariamente — e por que o valor final
ainda precisa ser validado empiricamente nos testes de usabilidade (Etapa 4, M6), já que o
referencial original foi pensado para interfaces locais, não para um backend em rede.

*Nota:* localizar a URL/edição exata do artigo (Nielsen Norman Group) para a entrada ABNT completa
na redação final; confirmar se a edição consultada foi a de 1993 (original) ou uma revisão
posterior do mesmo grupo.

---

## 3. Como usar este documento

- Toda vez que uma decisão técnica for embasada numa fonte externa nova (não citada no
  `Gastra.pdf`), adicionar uma entrada aqui, na seção do bloco temático correspondente, com a
  frase de ponte explicando a conexão específica — não só o resumo da obra.
- Antes da redação final (Etapa 5, M7), cruzar este documento com o `Gastra.pdf` para consolidar a
  lista de referências definitiva do TG, sem duplicar entradas.
- Fontes sem edição/ISBN/URL confirmados ficam marcadas com *Nota* até serem fechadas.
