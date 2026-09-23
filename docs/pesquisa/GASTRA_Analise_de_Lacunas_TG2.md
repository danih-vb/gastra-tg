# GASTRA — Análise de lacunas do documento de pesquisa (TG II)

Compara o documento de pesquisa como ele está hoje com a estrutura oficial do TG II e com o que o
repositório já tem pronto, para dizer **o que falta escrever, de onde vem o conteúdo e em que ordem**.
Issue #199.

- **Documento analisado:** `docs/pesquisa/Gastra II.docx`, na versão da PR #154.
- **Prazo da Etapa 2 no cronograma oficial:** 04/10.

## Como a medição foi feita

O texto foi extraído do `word/document.xml` de cada arquivo e medido por seção: para cada título, quantos
caracteres e quantos parágrafos de corpo existem até o título seguinte. É contagem bruta, e serve só para
separar "escrito" de "vazio" — não julga qualidade.

## 1. Situação seção a seção

| Seção | Corpo hoje | Situação | De onde vem o que falta | Issue |
|---|---:|---|---|---|
| 1.1 Contextualização | 974 | Escrita | — | — |
| 1.2 Problematização | 1.564 | Escrita | — | — |
| 1.3 Objetivos | 1.744 | Escrita | — | — |
| 1.4 Justificativa | 3.379 | Escrita | — | — |
| 1.5 Metodologia | 3.615 | **Escrita, mas desatualizada** | Como o trabalho foi conduzido de fato: Scrum com sprints, GitFlow, PR com revisão do outro integrante, CI em quatro jobs | #206 |
| 1.6 Estrutura do trabalho | 1.750 | Escrita | — | — |
| 2.1 Conceitos e regras de negócio | 2.275 | Escrita | — | — |
| 2.2 Tecnologias e arquitetura | 3.736 | **Escrita, faltam citações** | `docs/arquitetura/GASTRA_Arquitetura.md` (decisões D1–D11) | #204 |
| 2.3 Trabalhos correlatos | 3.347 | Escrita | — | — |
| 3.1 Modelo de negócios (Canvas) | 5.402 | Escrita | — | — |
| 3.2 Viabilidade financeira | 3.660 | Escrita | — | — |
| **4 Engenharia de software** | **0** | **Vazia** | ver seção 2 abaixo | #200–#203 |
| **5 Desenvolvimento, resultados e testes** | **0** | **Vazia, e sem issue** | ver seção 3 abaixo | **nenhuma** |
| **6 Considerações finais** | **0** | **Vazia, e sem issue** | redação final | **nenhuma** |
| Referências | 6.457 | Escrita | — | — |

### O achado principal

O capítulo 4 é o que o épico #198 mira, e está de fato vazio. Mas **os capítulos 5 e 6 também estão
vazios, e não existe issue nenhuma para eles**. O planejamento da Sprint 4 cobre o capítulo 4 e para ali.

Isso importa porque o capítulo 5 é justamente onde cabe quase tudo o que foi construído nas últimas
semanas — e que hoje está em arquivos do repositório que ninguém ainda trouxe para o documento.

## 2. Capítulo 4 — Engenharia de software e projeto do sistema

Todo o conteúdo já existe no repositório. O trabalho é de redação e formatação (terceira pessoa, ABNT,
figura e tabela com título acima e fonte abaixo), não de investigação.

| Subseção | Fonte pronta no repositório | Issue |
|---|---|---|
| 4.1 Requisitos | `docs/requisitos/GASTRA_Requisitos_RN.docx` — 24 RF, 5 RNF, 8 RN; `GASTRA_Matriz_Rastreabilidade.docx` | #200 |
| 4.2 Casos de uso e user stories | `GASTRA_Casos_de_Uso.docx`, `GASTRA_User_Stories.docx`, 6 diagramas em `docs/diagramas/casos-de-uso/` | #201 |
| 4.3 Modelagem de dados | `docs/modelagem/`: MER, DER, `GASTRA_Schema.sql`, `GASTRA_Validacao_Modelo_Fisico.md`, `GASTRA_Objetos_Banco.md` | #202 |
| 4.4 Arquitetura e prototipação | `GASTRA_Arquitetura.md` (D1–D11), 4 diagramas de classes, 3 de sequência, 2 de atividade, 2 de arquitetura; `docs/ux-ui/` com o protótipo e 33 telas | #203 |

## 3. Capítulo 5 — Desenvolvimento, resultados e testes

**Não tem issue, e é o capítulo com mais material pronto.** Sugestão de subseções, com a fonte de cada uma:

| Subseção sugerida | Fonte pronta no repositório |
|---|---|
| 5.1 Processo de desenvolvimento | Histórico do repositório: sprints, PRs com revisão, CI em quatro jobs. Conversa com a #206 |
| 5.2 Testes automatizados | `docs/testes/GASTRA_Cenarios_Testes_Executados.md` — 385 cenários e 432 casos, por bloco e por camada |
| 5.3 Calibração dos pesos da RN03 | `docs/analises/GASTRA_Calibracao_Pesos_RN03.md`, com o gráfico `calibracao_rn03.png` |
| 5.4 Validação sobre dados simulados | `docs/analises/GASTRA_Validacao_Dados_Simulados.md` — a regra plantada reencontrada pelo algoritmo, a alocação invertendo a ordem do faturamento, o Gini |
| 5.5 Resultado para o usuário | `docs/manual-usuario/GASTRA_Manual_do_Usuario.md` |
| 5.6 Testes operacionais | **Ainda não existe.** É a Sprint 5 (#207–#211) |

## 4. Capítulo 6 — Considerações finais

Também sem issue. É redação a partir do que os capítulos 4 e 5 concluírem: objetivos alcançados, limitações
assumidas e trabalhos futuros. As limitações já estão registradas espalhadas e valem ser reunidas:

- Nenhuma tela foi exercitada contra a API real (registrado em todas as PRs de frontend);
- Os cenários operacionais de ponta a ponta ficaram para o marco M6;
- O histórico usado nas análises é **simulado**, e não de operação real;
- A avaliação do atendimento (RF25) entrou no fim e não tem dado de uso.

## 5. Ordem de trabalho proposta

A ordem não é a numérica. Ela começa pelo que destrava os outros e pelo que tem material pronto:

1. **#205 — reconciliar os números.** Primeiro, porque todo o resto vai citar esses números. Feita junto
   com esta análise.
2. **#200 — §4.1 Requisitos.** É a base que 4.2, 4.3 e 4.4 referenciam.
3. **#201 — §4.2 Casos de uso**, que dependem dos requisitos.
4. **#202 — §4.3 Modelagem de dados.** Depende da revisão dos diagramas (#189–#197) estar feita, senão o
   capítulo cita figura que vai mudar.
5. **#203 — §4.4 Arquitetura e prototipação.** Mesma dependência dos diagramas.
6. **#204 — §2.2 citações** e **#206 — §1.5 metodologia**: retoques em capítulo já escrito, e podem sair a
   qualquer momento.
7. **Capítulo 5**, que precisa de issue. É o de maior retorno por hora: o material está pronto e é o que
   mostra resultado.
8. **Capítulo 6**, por último, porque conclui os anteriores.

## 6. Riscos

- **Os capítulos 5 e 6 não estão no planejamento.** Se o prazo de 04/10 vale para a Etapa 2 inteira, eles
  precisam entrar agora, não depois.
- **§4.3 e §4.4 dependem dos diagramas** (#189–#197). Escrever antes da revisão significa reescrever
  depois, porque o código mudou bastante desde que os diagramas foram desenhados.
- **O CSV do questionário não está versionado.** É a fonte de verdade dos números da pesquisa e não está
  no repositório — ver `docs/requisitos/GASTRA_Numeros_da_Pesquisa.md`.
