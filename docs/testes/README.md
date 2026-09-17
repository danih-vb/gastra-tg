# Testes — GASTRA

| Arquivo | Conteúdo |
|---|---|
| [`GASTRA_Cenarios_Testes_Executados.md`](GASTRA_Cenarios_Testes_Executados.md) | Tabela de cenários de teste **executados**, por bloco do TG, com o resultado de cada um (issue #48) |
| `gerar_relatorio_testes.py` | Gera a tabela a partir dos arquivos de resultado do `dotnet test` (.trx) e do `pytest` (JUnit XML) |

## Como atualizar a tabela

Para nenhum teste ficar ignorado, suba o MySQL (`infra/`) e o serviço Python (`data-science/`) antes.

1. **Backend**, dentro de `backend/`:
   ```bash
   GASTRA_TESTES_MYSQL="Server=localhost;Port=3307;User ID=root;Password=<senha>" \
   GASTRA_TESTES_ANALITICA=http://localhost:8000 \
   dotnet test --logger trx --results-directory ../resultados
   ```
2. **Python**, dentro de `data-science/`:
   ```bash
   pytest --junitxml=../resultados/pytest.xml
   ```
3. **Gerar a tabela**, na raiz:
   ```bash
   python docs/testes/gerar_relatorio_testes.py resultados/*.trx resultados/pytest.xml --commit $(git rev-parse --short HEAD)
   ```

A pasta `resultados/` é temporária e não deve ser versionada.

Os cenários **operacionais** de ponta a ponta, com o frontend, entram no marco M6 e completam a issue #48.
