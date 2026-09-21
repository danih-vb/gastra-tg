# Semeador de dados simulados

Preenche o banco de **desenvolvimento** com dados fictícios, para que as telas de análise, a alocação e a
recomendação tenham o que mostrar. Épico #179.

> **Os dados são simulados.** Não representam nenhum restaurante real, nem o estabelecimento entrevistado
> na pesquisa: os nomes das pessoas são inventados, as praças não são a planta de ninguém e os pratos são
> de cozinha brasileira comum, sem prato assinatura.

## Por que existe uma ferramenta em vez de usar a API

Toda comanda nasce com `DateTime.UtcNow` no construtor — não há como injetar uma data de abertura. Só que as
views de BI agregam por turno, dia e hora, e o fator de faturamento da RN03 olha os últimos 30 dias: sem
datas no passado, todas essas telas ficam vazias por mais comandas que se crie pela API.

SQL direto resolveria as datas, mas passaria por cima das regras do domínio (composição da mesa, taxa de
serviço, totais, hash de senha) e produziria dado que o sistema nunca geraria. A ferramenta grava pelo
`GastraDbContext`, com as entidades do domínio, e ajusta **só as datas** pela API de propriedades do EF, que
enxerga o setter privado. A decisão está registrada como **D11** em
[`docs/arquitetura/GASTRA_Arquitetura.md`](../../../docs/arquitetura/GASTRA_Arquitetura.md).

A limitação de testabilidade (`Comanda` fixa `UtcNow`) fica registrada aqui: se um dia a entidade aceitar a
data por parâmetro, esta ferramenta fica mais simples.

## Como rodar

Fica **fora da solução** de propósito: não entra no build nem nos testes da API, e nunca é publicado com ela.

Dentro de `backend/`:

```bash
GASTRA_AMBIENTE=Development GASTRA_SEMEADOR_CONEXAO="Server=localhost;Port=3307;Database=gastra_dev;User ID=gastra_app;Password=SUA_SENHA" GASTRA_SEMEADOR_SENHA="SENHA_DAS_CONTAS_FICTICIAS" dotnet run --project tools/GastraSemeador
```

| Variável | O que é |
|---|---|
| `GASTRA_AMBIENTE` | Precisa ser `Development`. Fora disso a ferramenta recusa rodar e explica o motivo |
| `GASTRA_SEMEADOR_CONEXAO` | Conexão do banco de desenvolvimento — os mesmos valores do `infra/.env` |
| `GASTRA_SEMEADOR_SENHA` | Senha inicial de **todas** as contas fictícias, 8 caracteres ou mais. Nunca fica no código |

| Argumento | O que faz |
|---|---|
| `--semente N` | Troca a semente do sorteio. O padrão é `42`, a mesma usada na calibração da RN03 |

A mesma semente gera exatamente os mesmos dados. Rodar duas vezes seguidas **não duplica nada**: contas,
praças, mesas e itens são identificados por e-mail, código, número e nome.

## Para começar do zero

A ferramenta não apaga nada. Para recomeçar, derrube o volume do MySQL e deixe a API aplicar as migrations:

```bash
docker compose -f infra/docker-compose.yml down -v
```

## O que gera

| Parte | Conteúdo | Issue |
|---|---|---|
| Cadastros base | 1 Gerente, 1 Coordenador, 1 Metre e 7 Garçons (`@gastra.local`); 3 praças com potenciais diferentes (vagas 3/3/2); 20 mesas com capacidades variadas; 14 itens cobrindo as quatro categorias, todas as marcações dietéticas e itens infantis | #181 |
| Histórico de comandas fechadas | *(a fazer)* | #182 |
| Estado "ao vivo" da demonstração | *(a fazer)* | #183 |

O cardápio inclui de propósito os itens que o `simulador.py` usa nas combinações — moqueca, arroz de coco,
caipirinha, pudim, café, porção infantil e suco. É o que permite plantar a combinação "moqueca puxa arroz de
coco" no histórico e depois conferir se a recomendação encontra a regra sozinha.
