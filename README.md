# GASTRA — Gestão Analítica de Restaurantes

**Trabalho de Graduação (TG)** — Análise e Desenvolvimento de Sistemas, FATEC Araraquara
**Autores:** Daniel (Danih) e Pedro
**Orientador:** Prof. Me. Leonardo José de Lima Ferrucci

---

## Sobre o projeto

GASTRA é um sistema de apoio à decisão para gestão de restaurantes, combinando quatro blocos
analíticos com um módulo operacional de comandas que alimenta esses blocos com dados reais:

| Bloco | O que faz | Status de escopo |
|---|---|---|
| **BI** (Business Intelligence) | Relatórios agregados de faturamento por praça/garçom | 🟢 Validado — escopo original |
| **Ciência de Dados** | Clusterização e regras de associação para recomendação de pratos | 🟢 Validado — escopo original |
| **Programação Linear** | Otimização da alocação/distribuição de garçons por praça | 🟢 Validado — escopo original |
| **LGPD** | Conformidade e minimização de dados pessoais em todo o sistema | 🟢 Validado — escopo original |
| **Módulo de Comandas** | Abertura/fechamento de mesa, registro de pedido (front-of-house) | 🟡 Pendente de validação formal |
| **Cardápio Digital (opção B)** | Consulta via QR/tablet, sem função de pedido | 🟠 Pendente de validação formal |

> ⚠️ **Atenção ao escopo:** o projeto de pesquisa formal ([`docs/pesquisa/Gastra.pdf`](docs/pesquisa/Gastra.pdf))
> delimita o GASTRA como **não** incluindo sistemas de pedidos ou cardápios digitais. Os itens 🟡 e 🟠
> acima foram decididos entre a dupla mas **ainda não foram validados com o orientador** — ver o
> conflito detalhado em [`docs/GASTRA_STATUS.md`](docs/GASTRA_STATUS.md#1-escopo-do-projeto). Nenhum
> código ou texto do TG deve tratar esses itens como escopo fechado até a validação ocorrer.

## Stack tecnológica

Definida no projeto de pesquisa formal:

- **Backend:** ASP.NET Core
- **Frontend:** Angular
- **Análise de dados:** Python (clusterização, regras de associação, programação linear)

## Estrutura do repositório

```
gastra/
├── backend/                 # API ASP.NET Core
├── frontend/                 # Aplicação Angular
├── data-science/             # Python — BI, clusterização, regras de associação, PL
│   ├── notebooks/            # Exploração e prototipagem (Jupyter)
│   ├── src/                  # Código de produção dos algoritmos
│   └── data/
│       ├── raw/              # NUNCA versionado (dados pessoais/LGPD) — ver docs/requisitos
│       └── processed/        # Dados tratados/anonimizados, versionáveis
├── docs/
│   ├── GASTRA_STATUS.md      # Documento vivo — status, decisões, checklist, roadmap
│   ├── pesquisa/              # Projeto de pesquisa formal (Gastra.pdf)
│   ├── requisitos/            # RF/RNF/RN e Matriz de Rastreabilidade
│   ├── modelagem/
│   │   ├── mer/               # Modelo de Entidade-Relacionamento (linguagem natural, primeiro)
│   │   └── der/               # Diagrama formal (a partir do MER)
│   ├── diagramas/             # Diagramas de apoio (fluxos, casos de uso)
│   └── assets/logo/           # Identidade visual do GASTRA
├── scripts/                   # Scripts de apoio (ex.: criação de issues em lote)
├── .github/                   # Templates de issue/PR, workflows
├── CONTRIBUTING.md            # Fluxo de contribuição (GitFlow), commits, segurança
├── LICENSE
└── README.md
```

## Documentação

- **Status vivo do projeto:** [`docs/GASTRA_STATUS.md`](docs/GASTRA_STATUS.md) — escopo, decisões,
  checklist de tarefas, resultados preliminares dos questionários.
- **Requisitos e Regras de Negócio:** [`docs/requisitos/GASTRA_Requisitos_RN.docx`](docs/requisitos/GASTRA_Requisitos_RN.docx)
- **Matriz de Rastreabilidade:** [`docs/requisitos/GASTRA_Matriz_Rastreabilidade.docx`](docs/requisitos/GASTRA_Matriz_Rastreabilidade.docx)
- **Projeto de pesquisa formal:** [`docs/pesquisa/Gastra.pdf`](docs/pesquisa/Gastra.pdf)
- **Setup do GitHub (Projects, milestones, labels):** [`docs/GASTRA_GITHUB_SETUP.md`](docs/GASTRA_GITHUB_SETUP.md)
- **Quadro de tarefas (GitHub Projects):** [GASTRA - TG](https://github.com/users/danih-vb/projects/3)

## Como rodar localmente

Cada módulo terá seu próprio `README.md` com instruções de setup assim que o código for iniciado:

- `backend/README.md` — a criar junto com o primeiro commit de código do ASP.NET Core
- `frontend/README.md` — a criar junto com o primeiro commit de código do Angular
- `data-science/README.md` — a criar junto com os primeiros notebooks/scripts Python

## Fluxo de contribuição

Este repositório segue **GitFlow**. Antes de commitar, leia [`CONTRIBUTING.md`](CONTRIBUTING.md) —
ele cobre branches, convenção de commits, processo de revisão entre a dupla e regras de segurança
para dados sensíveis (LGPD).

## Privacidade e LGPD

Dados de questionários e qualquer dado que possa identificar uma pessoa (cliente ou garçom
entrevistado) **não são versionados** neste repositório — apenas dados agregados/anonimizados em
`data-science/data/processed/`. Ver detalhes em [`CONTRIBUTING.md`](CONTRIBUTING.md#segurança-e-integridade-de-dados).

## Licença

Este projeto está sob a licença definida em [`LICENSE`](LICENSE). **Atenção:** por ser um Trabalho
de Graduação, confirmem com o Prof. Ferrucci e a FATEC se há alguma exigência institucional sobre
licenciamento/propriedade intelectual antes da defesa — ver nota em `LICENSE`.
