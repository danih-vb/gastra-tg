# GASTRA — Gestão Analítica de Restaurantes

**Trabalho de Graduação (TG)** — Análise e Desenvolvimento de Sistemas, FATEC Araraquara
**Autores:** Daniel (Danih) e Pedro
**Orientador:** Prof. Me. Leonardo José de Lima Ferrucci

---

## Sobre o projeto

GASTRA é um sistema de apoio à decisão para gestão de restaurantes, combinando quatro blocos
analíticos com um módulo operacional de comandas que alimenta esses blocos com dados reais.

**Atualização de escopo (21/08/2026, validada com o orientador):** os quatro blocos analíticos
continuam sendo o foco e a entrega central do TG. O módulo de comandas foi aprovado como
**adicional necessário** — não substitui nem compete com o foco, apenas fornece os dados
operacionais que os blocos analíticos consomem. Alguns itens do módulo de comandas são **extras
condicionados a sobrar tempo**, e um item foi formalmente excluído do TG.

| Bloco | O que faz | Status de escopo |
|---|---|---|
| **BI** (Business Intelligence) | Relatórios agregados de faturamento por praça/garçom, KPIs definidos | 🟢 Validado — escopo original, foco do TG |
| **Ciência de Dados** | Clusterização e regras de associação para recomendação de pratos | 🟢 Validado — escopo original, foco do TG |
| **Programação Linear** | Otimização da alocação de garçons por praça (critério RN03) | 🟢 Validado — escopo original, foco do TG |
| **LGPD** | Conformidade e minimização de dados pessoais em todo o sistema | 🟢 Validado — escopo original, foco do TG |
| **Núcleo do Módulo de Comandas** | Abertura/fechamento de mesa, registro de pedido (front-of-house) | 🟢 Validado como **adicional** — necessário, mas não é o foco |
| **Cardápio Digital** (consulta) | Consulta via QR/tablet, sem função de pedido | 🟠 Validado como **extra opcional** — só se sobrar tempo |
| **Comanda em tempo real** (consulta pelo cliente) | Cliente acompanha itens e valor parcial da própria comanda | 🟠 Validado como **extra opcional** — só se sobrar tempo |
| **Lista de pendências do garçom** | Itens do pedido ainda não entregues, como lembrete ativo | 🟠 Validado como **extra opcional** — simples, se sobrar tempo |
| **Integração com a cozinha** | Acesso a pedidos, confirmação de preparo | ⚫ **Fora do escopo do TG** — feature futura, pós-defesa |

> ℹ️ **Sobre o escopo:** o projeto de pesquisa formal ([`docs/pesquisa/Gastra.pdf`](docs/pesquisa/Gastra.pdf))
> delimitava originalmente o GASTRA como não incluindo sistemas de pedidos ou cardápios digitais.
> Essa delimitação foi revisada com o orientador em 21/08/2026 — o histórico completo da decisão e
> o racional de cada item estão em [`docs/GASTRA_STATUS.md`](docs/GASTRA_STATUS.md#1-escopo-do-projeto-revisado-em-21082026--validado-com-o-orientador).
> Itens 🟠 não são compromisso de entrega: só entram em desenvolvimento depois que o núcleo
> (comandas + os 4 blocos analíticos) tiver um MVP apresentável.

## Stack tecnológica

Definida no projeto de pesquisa formal:

- **Backend:** ASP.NET Core
- **Frontend:** Angular
- **Análise de dados:** Python (clusterização, regras de associação, programação linear)

## Notação de diagramas

Notação confirmada com o orientador em 21/08/2026: **UML** (casos de uso, classes,
sequência, atividades). MER/DER e Business Model Canvas ficam para a Sprint 2 — não
fazem parte do escopo da Sprint 1.

## Estrutura do repositório

```
gastra/
├── backend/                   # API ASP.NET Core
├── frontend/                  # Aplicação Angular
├── data-science/              # Python — BI, clusterização, regras de associação, PL
│   ├── notebooks/             # Exploração e prototipagem (Jupyter)
│   ├── src/                   # Código de produção dos algoritmos
│   └── data/
│       ├── raw/                # NUNCA versionado (dados pessoais/LGPD) — ver docs/requisitos
│       └── processed/          # Dados tratados/anonimizados, versionáveis
├── docs/
│   ├── GASTRA_STATUS.md       # Documento vivo — status, decisões, checklist, roadmap
│   ├── CHECKLIST_REVISAO_PR.md # Roteiro para quem revisa um Pull Request
│   ├── pesquisa/               # Projeto de pesquisa formal (Gastra.pdf)
│   │   └── referencias/        # Material institucional de apoio (guia de orientação da FATEC, etc.)
│   ├── negocio/                 # Business Model Canvas
│   ├── requisitos/             # RF/RNF/RN, Matriz de Rastreabilidade, User Stories
│   │   └── entrevistas/        # Roteiros de entrevista (nunca gravação/transcrição bruta)
│   ├── modelagem/
│   │   ├── mer/                 # Modelo de Entidade-Relacionamento (linguagem natural, primeiro)
│   │   └── der/                 # Diagrama formal (a partir do MER)
│   ├── arquitetura/             # Definição de arquitetura do sistema (parte teórica do TG)
│   ├── ux-ui/                   # Wireframes e protótipo navegável
│   ├── testes/                  # Cenários de teste executados
│   ├── manual-usuario/          # Manual de uso do usuário
│   ├── diagramas/               # Diagramas de apoio (fluxos, casos de uso)
│   └── assets/logo/             # Identidade visual do GASTRA
├── scripts/                   # Scripts de apoio (criação de issues em lote, etc.)
├── .github/                   # Templates de issue/PR, workflows
├── CONTRIBUTING.md            # Fluxo de contribuição (GitFlow), commits, segurança
├── LICENSE
└── README.md
```

## Documentação

- **Status vivo do projeto:** [`docs/GASTRA_STATUS.md`](docs/GASTRA_STATUS.md) — escopo, decisões,
  checklist de tarefas, milestones, sprints, resultados preliminares dos questionários e da
  entrevista.
- **Requisitos e Regras de Negócio:** [`docs/requisitos/GASTRA_Requisitos_RN.docx`](docs/requisitos/GASTRA_Requisitos_RN.docx)
- **Matriz de Rastreabilidade:** [`docs/requisitos/GASTRA_Matriz_Rastreabilidade.docx`](docs/requisitos/GASTRA_Matriz_Rastreabilidade.docx)
- **Roteiro de entrevista (Cocobambu):** [`docs/requisitos/entrevistas/GASTRA_Roteiro_Entrevista_Cocobambu.docx`](docs/requisitos/entrevistas/GASTRA_Roteiro_Entrevista_Cocobambu.docx)
- **Projeto de pesquisa formal:** [`docs/pesquisa/Gastra.pdf`](docs/pesquisa/Gastra.pdf)
- **Checklist de revisão de PR:** [`docs/CHECKLIST_REVISAO_PR.md`](docs/CHECKLIST_REVISAO_PR.md)
- **User Stories:** [`docs/requisitos/GASTRA_User_Stories.docx`](docs/requisitos/GASTRA_User_Stories.docx)
- **Quadro de tarefas (GitHub Projects):** [GASTRA - TG](https://github.com/users/danih-vb/projects/3)
- *A criar (Sprint 2):* Business Model Canvas, MER/DER, diagramas de apoio (UML),
  definição de arquitetura, protótipo UX/UI, cenários de teste, manual do usuário —
  ver checklist em `docs/GASTRA_STATUS.md`.

## Como rodar localmente

Cada módulo terá seu próprio `README.md` com instruções de setup assim que o código for iniciado:

- `backend/README.md` — a criar junto com o primeiro commit de código do ASP.NET Core
- `frontend/README.md` — a criar junto com o primeiro commit de código do Angular
- `data-science/README.md` — a criar junto com os primeiros notebooks/scripts Python

## Fluxo de contribuição

Este repositório segue **GitFlow**. Antes de commitar, leia [`CONTRIBUTING.md`](CONTRIBUTING.md) —
ele cobre branches, convenção de commits, fluxo de release, uso do quadro de tarefas (Milestones,
Sprints/Iterations e Status), processo de revisão entre a dupla e regras de segurança para dados
sensíveis (LGPD).

## Privacidade e LGPD

Este repositório é **público**, mas nenhum dado pessoal identificável de cliente ou garçom
(entrevistado ou respondente de questionário) é versionado nele — nem em texto, nem em planilha,
nem em gravação/transcrição. Apenas dados agregados/anonimizados entram em
`data-science/data/processed/`, e mesmo assim com cautela: amostras muito pequenas (ex.: n=2) não
são estruturadas como dataset, só como texto narrativo, porque um "agregado" de amostra pequena
pode reidentificar a resposta individual. Roteiros de entrevista (perguntas, estrutura) são
versionados normalmente em `docs/requisitos/entrevistas/`, mas a gravação ou transcrição literal de
qualquer entrevista realizada nunca é commitada. Ver detalhes completos em
[`CONTRIBUTING.md`](CONTRIBUTING.md#6-segurança-e-integridade-de-dados) e em
[`docs/GASTRA_STATUS.md`](docs/GASTRA_STATUS.md), seção "Dados do questionário e da entrevista".

## Licença

Este projeto está sob a licença definida em [`LICENSE`](LICENSE). **Atenção:** por ser um Trabalho
de Graduação, confirmem com o Prof. Ferrucci e a FATEC se há alguma exigência institucional sobre
licenciamento/propriedade intelectual antes da defesa — ver `CONTRIBUTING.md`, seção 8.
