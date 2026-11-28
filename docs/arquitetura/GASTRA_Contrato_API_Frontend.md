# Contrato API ↔ frontend — GASTRA

Issue #161: os tipos que o frontend usa (`frontend/src/app/core/api/modelos.ts` e `core/sessao/modelos.ts`) foram
comparados ao contrato da API (o documento OpenAPI de `/openapi/v1.json`) em 28/11/2026, depois da release v1.0.

## Como foi comparado

O script [`comparar_contrato.py`](comparar_contrato.py) faz a conferência:

```bash
curl http://localhost:5019/openapi/v1.json -o openapi.json   # API em Development (Docker ou dotnet run)
python docs/arquitetura/comparar_contrato.py openapi.json
```

Para cada interface do frontend, ele acha o esquema da API com os mesmos campos e aponta três tipos de problema:

| Problema | Por que importa |
|---|---|
| Campo que o frontend espera e a API não manda | Chegaria `undefined` e a tela mostraria vazio ou quebraria |
| Campo que a API pode mandar nulo e o frontend tipa como não nulo | O TypeScript não avisaria, e a tela quebraria com o nulo |
| Enum com valores diferentes | Um status novo da API cairia num `switch` que não o conhece |

Os enums vão pelo nome (`"AConfirmar"`, e não `3`), por causa do `JsonStringEnumConverter` da API. Como o OpenAPI do
.NET os descreve só como texto, sem a lista de valores, o script lê os valores direto de
`backend/src/Gastra.Communication/Enums/`.

## Resultado

| Item | Resultado |
|---|---|
| Interfaces do frontend casadas com esquemas da API | 37 de 37 |
| Campos esperados pelo frontend que a API não manda | nenhum |
| Campos anuláveis na API (39) tipados como não nulos no frontend | nenhum |
| Enums comparados (12) com valores diferentes | nenhum |
| Formato de erro | igual: a API responde `{ "erros": ["..."] }` (`ErroResponse`) e o frontend lê `erros` (`shared/erros.ts`) |

## Decisão: manter os tipos escritos à mão

Os tipos continuam escritos à mão, conferidos por este script, e não gerados do OpenAPI. Motivos:

1. **O gerado seria pior para usar.** A API aceita números escritos como texto (o padrão web do ASP.NET Core,
   `JsonNumberHandling.AllowReadingFromString`), então o OpenAPI
   descreve cada número como `integer | string`. Um gerador produziria `number | string` em todo campo numérico, e
   cada conta na tela precisaria de conversão.
2. **Os tipos à mão carregam o que o OpenAPI não diz:** os valores de cada enum como união de textos e comentários
   sobre quando um campo vem nulo.
3. **Os nomes já seguem o backend** (o próprio `modelos.ts` diz isso), e a conferência acima mostrou que estão em dia.

A troca vale revisitar se a API passar a ter outro cliente além das telas Angular. Até lá, ao mudar um DTO em
`Gastra.Communication`, rode o script antes do PR.
