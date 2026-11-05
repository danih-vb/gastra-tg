# Gerador do diagrama de classes (issue #125)

Lê os assemblies compilados do backend por reflection e gera o diagrama de classes **a partir do código**.
Assim, o diagrama nunca fica diferente do que está implementado.

| Saída | Conteúdo |
|---|---|
| `docs/diagramas/_fontes/GASTRA_Classe_Dominio.drawio` | Entidades com atributos e métodos, composições (coleções de entidades) e associações (propriedades `<Entidade>Id`), enumerações, regras de negócio e objetos de valor |
| `docs/diagramas/_fontes/GASTRA_Classe_Contratos.drawio` | Interfaces de `Gastra.Domain` (repositórios, serviços, segurança) e as classes de `Gastra.Infrastructure` que as implementam |
| `docs/arquitetura/GASTRA_Diagrama_Classes_Codigo.md` | A mesma informação em tabelas |

## Como rodar

Dentro de `backend/`:

```bash
dotnet run --project tools/GeradorDiagramaClasses -- ../docs
```

Depois, dentro da raiz do repositório, exporte os PNGs com o draw.io desktop:

```bash
drawio --export --format png --scale 1.5 --border 30 --output docs/diagramas/classe/GASTRA_Classe_Dominio.png docs/diagramas/_fontes/GASTRA_Classe_Dominio.drawio
drawio --export --format png --scale 1.5 --border 30 --output docs/diagramas/classe/GASTRA_Classe_Contratos.png docs/diagramas/_fontes/GASTRA_Classe_Contratos.drawio
```

No Windows, o executável fica em `%LOCALAPPDATA%\Programs\draw.io\draw.io.exe`.

## Quando rodar

Ao fim de cada módulo e antes de cada entrega ao orientador (combinado de 16/09, #119).

## O que não é gerado

- `GASTRA_Classe_Pacotes` (camadas) e `GASTRA_Classe_AbrirComanda` (exemplo de um caso de uso atravessando as
  camadas) são desenhos conceituais, mantidos à mão.
- As setas de herança para `EntidadeBase` são omitidas, para não poluir o desenho. Uma nota no diagrama avisa isso.
- O projeto fica **fora da solução**: não entra no build nem nos testes da API.
