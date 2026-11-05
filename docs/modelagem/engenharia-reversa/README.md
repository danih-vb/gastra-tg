# Engenharia reversa do banco (issue #84)

Gera o modelo lógico do brModelo a partir do banco real e compara com o DER conceitual. A análise
dos resultados está em [`../GASTRA_Validacao_Modelo_Fisico.md`](../GASTRA_Validacao_Modelo_Fisico.md).

## Pré-requisitos
- Contêiner do MySQL de pé, com as migrations aplicadas.
- Python 3.
- brModelo 3.31 e o `jjs` do Java 8, que roda JavaScript sobre as classes do brModelo.

## Passo a passo
Rode tudo dentro desta pasta.

1. **Extrair o catálogo do banco:**
   ```bash
   docker exec -i gastra-mysql sh -c 'mysql -N -B -u"$MYSQL_USER" -p"$MYSQL_PASSWORD" gastra_dev' < extrair_schema.sql > schema.tsv
   ```
2. **Exportar o DER conceitual para XML.** Abra `../der/GASTRA_DER.brM3` no brModelo e salve como
   XML em `der_conceitual.xml`.
3. **Comparar** (também gera `fisico.json`):
   ```bash
   python comparar.py schema.tsv der_conceitual.xml > comparacao.txt
   ```
4. **Montar o modelo lógico no brModelo:**
   ```bash
   jjs -cp brModelo.jar gerar_logico.js -- fisico.json layout.json GASTRA_Logico_Engenharia_Reversa.brM3 GASTRA_Logico_Engenharia_Reversa.png logico.xml
   ```

`layout.json` guarda a posição de cada tabela no desenho. `schema.tsv`, `fisico.json` e os XMLs
são arquivos intermediários e não são versionados.
