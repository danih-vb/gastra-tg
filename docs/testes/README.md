# Testes — GASTRA

| Arquivo | Conteúdo |
|---|---|
| [`GASTRA_Cenarios_Testes_Executados.md`](GASTRA_Cenarios_Testes_Executados.md) | Tabela de cenários de teste **executados**, por bloco do TG e por camada, com o resultado de cada um (issue #48) |
| `gerar_relatorio_testes.py` | Gera a tabela a partir dos resultados do `dotnet test` (.trx), do `pytest` e do `ng test` (JUnit XML) |

## Como atualizar a tabela

Para nenhum teste ficar ignorado, suba o MySQL (`infra/`) e o serviço Python (`data-science/`) antes, e crie o
usuário analítico somente leitura com `infra/criar-usuario-analitico.sh`.

1. **Backend**, dentro de `backend/`:
   ```bash
   GASTRA_TESTES_MYSQL="Server=localhost;Port=3307;User ID=root;Password=<senha>" \
   GASTRA_TESTES_ANALITICA=http://localhost:8000 \
   dotnet test --logger trx --results-directory ../resultados
   ```
2. **Camada analítica**, dentro de `data-science/`:
   ```bash
   GASTRA_ANALITICA_BANCO_SENHA=<senha do usuário analítico> \
   pytest --junitxml=../resultados/pytest.xml
   ```
3. **Frontend**, dentro de `frontend/`:
   ```bash
   ng test --watch=false --reporters junit --output-file ../resultados/vitest.xml
   ```
4. **Gerar a tabela**, na raiz:
   ```bash
   python docs/testes/gerar_relatorio_testes.py resultados/*.trx resultados/pytest.xml resultados/vitest.xml --commit $(git rev-parse --short HEAD)
   ```

A pasta `resultados/` é temporária e não deve ser versionada. Apague os resultados de execuções antigas antes de
gerar: o script soma tudo o que recebe, e um `.trx` esquecido mistura duas execuções na mesma tabela.

Uma classe de teste nova que não esteja no mapeamento `BLOCOS` (backend e Python) ou `BLOCOS_DO_FRONTEND` (telas)
cai no bloco "Outros", e o relatório avisa no fim — é o sinal para atualizar o script.

Os cenários **operacionais** de ponta a ponta, com o navegador contra a API e o banco reais, entram no marco M6.
