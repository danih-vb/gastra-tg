#!/usr/bin/env bash
# Volta o ambiente completo ao mesmo estado inicial, para cada rodada dos testes operacionais (issue #210).
#
# Apaga o volume do MySQL, sobe os quatro serviços, espera a API aplicar as migrations, cria o usuário somente
# leitura da camada analítica e roda o semeador com a semente padrão. No fim, o banco tem o histórico simulado
# e o turno de hoje montado e não confirmado, e o semeador imprime o código de acesso de uma das mesas.
#
# Uso, dentro de infra/ (precisa do Docker e do SDK do .NET, por causa do semeador):
#   ./resetar-ambiente.sh          pede confirmação antes de apagar o banco
#   ./resetar-ambiente.sh --sim    não pede
#
# Tudo vem do infra/.env, que nunca é versionado: além das variáveis de sempre, precisa de
# MYSQL_ANALITICA_PASSWORD e GASTRA_SEMEADOR_SENHA (a senha de todas as contas fictícias).
set -euo pipefail
cd "$(dirname "$0")"

set -a
. ./.env
set +a

for variavel in MYSQL_DATABASE MYSQL_USER MYSQL_PASSWORD MYSQL_ANALITICA_PASSWORD GASTRA_SEMEADOR_SENHA; do
  if [ -z "${!variavel:-}" ]; then
    echo "Defina $variavel no infra/.env (veja o .env.example)." >&2
    exit 1
  fi
done

if [ "${1:-}" != "--sim" ]; then
  read -r -p "Isto apaga TODO o banco $MYSQL_DATABASE do Docker. Continuar? [s/N] " resposta
  [ "$resposta" = "s" ] || [ "$resposta" = "S" ] || { echo "Nada foi apagado."; exit 0; }
fi

echo "1/4 Derrubando os serviços e apagando o volume..."
docker compose --profile app down -v

echo "2/4 Subindo os quatro serviços (a API aplica as migrations antes de ficar saudável)..."
docker compose --profile app up -d --build --wait

echo "3/4 Criando o usuário somente leitura da camada analítica..."
./criar-usuario-analitico.sh

echo "4/4 Semeando os dados simulados..."
(
  cd ../backend
  GASTRA_AMBIENTE=Development \
  GASTRA_SEMEADOR_CONEXAO="Server=localhost;Port=${MYSQL_PORT:-3307};Database=$MYSQL_DATABASE;User ID=$MYSQL_USER;Password=$MYSQL_PASSWORD" \
  dotnet run --project tools/GastraSemeador
)

echo "Pronto. Abra http://localhost:${WEB_PORT:-4200}."
