# GASTRA — Gestão Analítica de Restaurantes

**Trabalho de Graduação (TG)** — Análise e Desenvolvimento de Sistemas, FATEC Araraquara
**Autores:** Daniel Velluto Bento e Pedro Luis Otrente de Campos
**Orientador:** Prof. Me. Leonardo José de Lima Ferrucci

---

## Sobre o projeto

GASTRA é um sistema de apoio à decisão para gestão de restaurantes, combinando quatro blocos
analíticos — Business Intelligence, Ciência de Dados, Programação Linear e conformidade com a
LGPD — com um módulo operacional de comandas que alimenta esses blocos com dados reais de
atendimento.

## Escopo

| Bloco | O que faz | Enquadramento |
|---|---|---|
| **BI** (Business Intelligence) | Relatórios agregados de faturamento por praça/garçom, KPIs definidos | Escopo original, foco do TG |
| **Ciência de Dados** | Clusterização e regras de associação para recomendação de pratos | Escopo original, foco do TG |
| **Programação Linear** | Otimização da alocação de garçons por praça (critério RN03) | Escopo original, foco do TG |
| **LGPD** | Conformidade e minimização de dados pessoais em todo o sistema | Escopo original, foco do TG |
| **Núcleo do Módulo de Comandas** | Abertura/fechamento de mesa, registro de pedido (front-of-house) | Adicional — necessário para alimentar os blocos analíticos, mas não é o foco |
| **Cardápio Digital** (consulta) | Consulta via QR/tablet, sem função de pedido | Extra opcional — condicionado a sobrar tempo |
| **Comanda em tempo real** (consulta pelo cliente) | Cliente acompanha itens e valor parcial da própria comanda | Extra opcional — condicionado a sobrar tempo |
| **Lista de pendências do garçom** | Itens do pedido ainda não entregues, como lembrete ativo | Extra opcional — condicionado a sobrar tempo |
| **Integração com a cozinha** | Acesso a pedidos, confirmação de preparo | Fora do escopo do TG — feature futura, pós-defesa |
| **Programa de fidelização** | Cadastro voluntário do cliente, identificador persistente, histórico entre visitas | Fora do escopo do TG — feature futura, pós-defesa |

## Stack tecnológica

Definida no projeto de pesquisa formal:

- **Backend:** ASP.NET Core
- **Frontend:** Angular
- **Análise de dados:** Python (clusterização, regras de associação, programação linear)
- **Banco de dados:** MySQL, containerizado via Docker Compose no ambiente de desenvolvimento

## Notação de diagramas

**UML** para os diagramas de apoio (casos de uso, classes, sequência, atividades). O
modelo de dados segue MER em linguagem natural, formalizado em seguida como DER. O modelo de
negócio é representado em Business Model Canvas.

## Estrutura do repositório

```
gastra-tg/
├── backend/                       # API ASP.NET Core
├── frontend/                      # Aplicação Angular
├── data-science/                  # Python — BI, clusterização, regras de associação, PL
│   ├── notebooks/                 # Exploração e prototipagem (Jupyter)
│   ├── src/                       # Código de produção dos algoritmos
│   └── data/
│       ├── raw/                   # Nunca versionado (dados pessoais/LGPD)
│       └── processed/             # Dados tratados/anonimizados, versionáveis
│           └── GASTRA_Dados_Processados.docx
├── infra/                         # Ambiente de banco via Docker Compose (dev/testes)
│   ├── docker-compose.yml         # Serviço MySQL containerizado
│   └── .env.example               # Variáveis necessárias, sem valor real (.env não é versionado)
├── docs/
│   ├── pesquisa/                  # Projeto de pesquisa formal
│   │   ├── Gastra.pdf
│   │   └── referencias/           # Referências bibliográficas e material de apoio
│   │       └── GASTRA_Referencias_Bibliograficas.docx
│   ├── negocio/                   # Business Model Canvas
│   │   └── GASTRA_Business_Model_Canvas.png
│   ├── requisitos/                # RF/RNF/RN, Matriz de Rastreabilidade, User Stories, KPIs
│   │   ├── GASTRA_Requisitos_RN.docx
│   │   ├── GASTRA_Matriz_Rastreabilidade.docx
│   │   ├── GASTRA_User_Stories.docx
│   │   ├── GASTRA_KPIs_Criterios_Analiticos.docx
│   │   ├── entrevistas/           # Roteiros de entrevista (nunca gravação/transcrição bruta)
│   │   │   └── GASTRA_Roteiro_Entrevista_Restaurante.docx
│   │   └── questionarios/         # Instrumento de coleta (Jotform)
│   │       └── GASTRA_Instrumento_Coleta.docx
│   ├── modelagem/
│   │   ├── mer/                   # Modelo de Entidade-Relacionamento (linguagem natural)
│   │   │   └── GASTRA_MER.docx
│   │   └── der/                   # Diagrama formal (notação de Chen), a partir do MER
│   │       ├── GASTRA_DER.brm3
│   │       └── GASTRA_DER.png
│   ├── arquitetura/                # Definição de arquitetura do sistema
│   ├── ux-ui/                      # Wireframes e protótipo navegável
│   ├── testes/                     # Cenários de teste executados
│   ├── manual-usuario/             # Manual de uso do usuário
│   ├── diagramas/                  # Diagramas de apoio (UML)
│   │   ├── casos-de-uso/           # Diagramas UML de Casos de Uso
│   │   ├── classe/                 # Diagrama de Classes do domínio
│   │   ├── atividade/              # Diagramas UML de Atividade
│   │   └── sequencia/              # Diagramas UML de Sequência
│   └── assets/
│       └── logo/                   # Identidade visual do GASTRA
├── scripts/                        # Scripts de apoio ao repositório
├── .github/                        # Templates de issue/PR, workflows
├── CONTRIBUTING.md
├── LICENSE
└── README.md
```

## Documentação

- **Projeto de pesquisa formal:** [`docs/pesquisa/Gastra.pdf`](docs/pesquisa/Gastra.pdf)
- **Referências bibliográficas:** [`docs/pesquisa/referencias/GASTRA_Referencias_Bibliograficas.docx`](docs/pesquisa/referencias/GASTRA_Referencias_Bibliograficas.docx)
- **Requisitos e Regras de Negócio:** [`docs/requisitos/GASTRA_Requisitos_RN.docx`](docs/requisitos/GASTRA_Requisitos_RN.docx)
- **Matriz de Rastreabilidade:** [`docs/requisitos/GASTRA_Matriz_Rastreabilidade.docx`](docs/requisitos/GASTRA_Matriz_Rastreabilidade.docx)
- **User Stories:** [`docs/requisitos/GASTRA_User_Stories.docx`](docs/requisitos/GASTRA_User_Stories.docx)
- **KPIs e Critérios Analíticos:** [`docs/requisitos/GASTRA_KPIs_Criterios_Analiticos.docx`](docs/requisitos/GASTRA_KPIs_Criterios_Analiticos.docx)
- **Roteiro de entrevista (restaurante colaborador):** [`docs/requisitos/entrevistas/GASTRA_Roteiro_Entrevista_Restaurante.docx`](docs/requisitos/entrevistas/GASTRA_Roteiro_Entrevista_Restaurante.docx)
- **Instrumento de coleta (questionários Jotform):** [`docs/requisitos/questionarios/GASTRA_Instrumento_Coleta.docx`](docs/requisitos/questionarios/GASTRA_Instrumento_Coleta.docx)
- **Dados processados (questionários + entrevista):** [`data-science/data/processed/GASTRA_Dados_Processados.docx`](data-science/data/processed/GASTRA_Dados_Processados.docx)
- **Fluxo de contribuição:** [`CONTRIBUTING.md`](CONTRIBUTING.md)
- **Ambiente de banco de dados (Docker):** [`infra/README.md`](infra/README.md)
- **Quadro de tarefas (GitHub Projects):** [GASTRA - TG](https://github.com/users/danih-vb/projects/3)
- **Business Model Canvas:** [`docs/negocio/GASTRA_Business_Model_Canvas.png`](docs/negocio/GASTRA_Business_Model_Canvas.png)
- **Casos de Uso:** [`docs/requisitos/GASTRA_Casos_de_Uso.docx`](docs/requisitos/GASTRA_Casos_de_Uso.docx)
- **Diagrama de Atividade — Núcleo de Comandas:** [`docs/diagramas/atividade/GASTRA_Atividade_NucleoComandas.png`](docs/diagramas/atividade/GASTRA_Atividade_NucleoComandas.png)
- **Diagrama de Atividade — Alocação de Garçons:** [`docs/diagramas/atividade/GASTRA_Atividade_AlocacaoGarcons.png`](docs/diagramas/atividade/GASTRA_Atividade_AlocacaoGarcons.png)
- **Diagrama de Sequência — Núcleo de Comandas:** [`docs/diagramas/sequencia/GASTRA_Sequencia_NucleoComandas.png`](docs/diagramas/sequencia/GASTRA_Sequencia_NucleoComandas.png)
- **Diagrama de Sequência — Recomendação de Pratos:** [`docs/diagramas/sequencia/GASTRA_Sequencia_RecomendacaoPratos.png`](docs/diagramas/sequencia/GASTRA_Sequencia_RecomendacaoPratos.png)
- **Modelo de Entidade-Relacionamento (MER):** [`docs/modelagem/mer/GASTRA_MER.docx`](docs/modelagem/mer/GASTRA_MER.docx)
- **Diagrama de Entidade-Relacionamento (DER):** [`docs/modelagem/der/GASTRA_DER.png`](docs/modelagem/der/GASTRA_DER.png)
- **Diagrama de Classes:** [`docs/diagramas/classe/GASTRA_Classe.png`](docs/diagramas/classe/GASTRA_Classe.png)

- *A criar:* definição de arquitetura, protótipo UX/UI, cenários de teste, manual do usuário.

## Privacidade e LGPD

Este repositório é **público**, mas nenhum dado pessoal identificável de cliente ou garçom
(entrevistado ou respondente de questionário) é versionado nele — nem em texto, nem em
planilha, nem em gravação/transcrição. Apenas dados agregados/anonimizados e interpretados
entram em `data-science/data/processed/` (hoje, `GASTRA_Dados_Processados.docx`) — mesmo assim
com cautela: amostras muito pequenas nunca são apresentadas como recorte estruturado por
resposta, só como síntese narrativa, porque um "agregado" de amostra tão pequena pode
reidentificar a resposta individual. Roteiros de entrevista (perguntas, estrutura) são
versionados normalmente em `docs/requisitos/entrevistas/`, mas a gravação ou transcrição
literal de qualquer entrevista realizada nunca é commitada. Ver detalhes completos em
[`CONTRIBUTING.md`](CONTRIBUTING.md#4-segurança-de-dados-e-lgpd).

## Licença

Este projeto está sob a licença definida em [`LICENSE`](LICENSE).
