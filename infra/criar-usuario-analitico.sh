#!/usr/bin/env bash
# Cria (ou atualiza) o usuário MySQL somente leitura da camada analítica (decisão D10 da arquitetura).
#
# O usuário só recebe SELECT nas views vw_*: não lê tabelas e não grava nada. As views não têm dado
# pessoal, e quem grava no banco é sempre o backend em C#.
#
# Uso, dentro de infra/, depois de `docker compose up -d` e das migrations (as views precisam existir):
#   ./criar-usuario-analitico.sh
#
# A senha vem de MYSQL_ANALITICA_PASSWORD no infra/.env, que nunca é versionado.
set -euo pipefail
cd "$(dirname "$0")"

set -a
. ./.env
set +a

if [ -z "${MYSQL_ANALITICA_PASSWORD:-}" ]; then
  echo "Defina MYSQL_ANALITICA_PASSWORD no infra/.env (veja o .env.example)." >&2
  exit 1
fi

USUARIO=gastra_analitica
BANCO="${MYSQL_DATABASE}"

VIEWS=$(docker exec gastra-mysql sh -c 'mysql -N -B -uroot -p"$MYSQL_ROOT_PASSWORD" -e "SELECT table_name FROM information_schema.views WHERE table_schema = \"$MYSQL_DATABASE\" AND table_name LIKE \"vw\_%\""' 2>/dev/null)

if [ -z "$VIEWS" ]; then
  echo "Nenhuma view vw_* encontrada em $BANCO. Rode as migrations do backend antes." >&2
  exit 1
fi

# A senha vai pela entrada padrão do mysql, e não pela linha de comando (não aparece na lista de processos).
{
  echo "CREATE USER IF NOT EXISTS '$USUARIO'@'%' IDENTIFIED BY '$MYSQL_ANALITICA_PASSWORD';"
  echo "ALTER USER '$USUARIO'@'%' IDENTIFIED BY '$MYSQL_ANALITICA_PASSWORD';"
  for VIEW in $VIEWS; do
    echo "GRANT SELECT ON \`$BANCO\`.\`$VIEW\` TO '$USUARIO'@'%';"
  done
} | docker exec -i gastra-mysql sh -c 'mysql -uroot -p"$MYSQL_ROOT_PASSWORD"' 2>&1 | grep -v "Using a password" || true

echo "Usuário $USUARIO com SELECT em:"
echo "$VIEWS" | sed 's/^/  - /'
