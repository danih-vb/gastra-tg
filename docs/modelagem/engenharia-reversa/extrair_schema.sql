-- Engenharia reversa (issue #84), passo 1: lê o schema físico direto do catálogo do MySQL.
-- Saída em TSV, uma linha por coluna (COL), chave estrangeira (FK), índice único (UQ) e chave primária (PK).
-- Uso (dentro de infra/): docker exec gastra-mysql sh -c 'mysql -N -B -u"$MYSQL_USER" -p"$MYSQL_PASSWORD" gastra_dev' < ../docs/modelagem/engenharia-reversa/extrair_schema.sql > schema.tsv

SELECT 'COL', c.table_name, c.column_name, c.column_type, c.is_nullable, c.column_key, c.ordinal_position
FROM information_schema.columns c
JOIN information_schema.tables t ON t.table_schema = c.table_schema AND t.table_name = c.table_name
WHERE c.table_schema = DATABASE() AND t.table_type = 'BASE TABLE' AND c.table_name <> '__EFMigrationsHistory'
ORDER BY c.table_name, c.ordinal_position;

SELECT 'FK', k.table_name, k.column_name, k.referenced_table_name, k.referenced_column_name, r.delete_rule, k.constraint_name
FROM information_schema.key_column_usage k
JOIN information_schema.referential_constraints r
  ON r.constraint_schema = k.constraint_schema AND r.constraint_name = k.constraint_name
WHERE k.table_schema = DATABASE() AND k.referenced_table_name IS NOT NULL;

SELECT 'UQ', table_name, index_name, GROUP_CONCAT(column_name ORDER BY seq_in_index)
FROM information_schema.statistics
WHERE table_schema = DATABASE() AND non_unique = 0 AND index_name <> 'PRIMARY'
GROUP BY table_name, index_name;

SELECT 'PK', table_name, GROUP_CONCAT(column_name ORDER BY seq_in_index)
FROM information_schema.statistics
WHERE table_schema = DATABASE() AND index_name = 'PRIMARY' AND table_name <> '__EFMigrationsHistory'
GROUP BY table_name;
