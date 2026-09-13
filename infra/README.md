# Ambiente de Banco de Dados — GASTRA

Ambiente de desenvolvimento/testes via Docker Compose, com MySQL containerizado.

## Pré-requisitos

- Docker Desktop instalado ([docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop))
- No Windows: WSL2 habilitado (o próprio instalador do Docker Desktop orienta isso)

## Subindo o ambiente

1. Copie `.env.example` para `.env` e preencha com valores reais (nunca commitar o `.env`):
```bash
   cp .env.example .env
```
2. Suba o container:
```bash
   docker compose up -d
```
3. Confirme que subiu com sucesso:
```bash
   docker compose ps
```
   O status deve aparecer como `Up`/`healthy`.

## Conectando via MySQL Workbench

- Host: `localhost`
- Porta: o valor de `MYSQL_PORT` do seu `.env` (padrão `3307`)
- Usuário/senha: valores de `MYSQL_USER`/`MYSQL_PASSWORD` do seu `.env`

> O container usa a porta 3307 no host por padrão para não colidir com uma
> instalação local de MySQL, que normalmente ocupa a 3306. Se a 3307 também
> estiver em uso na sua máquina, altere `MYSQL_PORT` no `.env` e suba de novo
> com `docker compose up -d` — não é necessário editar o `docker-compose.yml`.

## Resetando o ambiente

Se precisar recriar o banco do zero (ex.: mudou o `.env` e quer que os novos
valores sejam aplicados):
```bash
docker compose down -v
docker compose up -d
```
⚠️ O `-v` apaga o volume — só use se não houver dado de teste importante ainda.