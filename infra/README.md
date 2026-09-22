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

> **Atualizou a branch e o `docker-compose.yml` mudou?** Rode `docker compose up -d` de novo. O
> contêiner é recriado com a nova configuração e o volume com os dados é mantido. Desde as views
> e triggers (issue #85), o MySQL sobe com `--log-bin-trust-function-creators=ON`. Sem isso, a
> migration que cria os triggers falha com o erro 1419.

## Usuário somente leitura da camada analítica (D10)

O serviço Python lê o histórico pelas views com um usuário que só tem `SELECT` nelas:

1. Defina `MYSQL_ANALITICA_PASSWORD` no `.env`.
2. Com o contêiner de pé e as migrations aplicadas, rode `./criar-usuario-analitico.sh`.

O script pode ser rodado de novo sem problema: ele atualiza a senha e dá acesso às views que forem
criadas depois. A senha vai para o MySQL pela entrada padrão, e não pela linha de comando.

## Sistema inteiro em Docker (MySQL + API + serviço analítico + telas)

Para apresentar ou avaliar o GASTRA sem instalar .NET, Python nem Node:

1. **No `.env`**, além das variáveis do MySQL, defina (ver `.env.example`):
   - `JWT_CHAVE_ASSINATURA`: 32 caracteres aleatórios ou mais;
   - `ADMIN_EMAIL` e `ADMIN_SENHA`: o primeiro Gerente, criado se o banco ainda não tiver usuário;
   - `API_AMBIENTE=Development`, só se quiser o Swagger;
   - `WEB_PORT`, se a porta 4200 estiver ocupada (o padrão é 4200).
2. **Suba tudo:**
   ```bash
   docker compose --profile app up -d --build
   ```
3. **Abra `http://localhost:4200`.** É o sistema inteiro: o nginx serve as telas e repassa `/api` para a API.
4. **Confira:** `docker compose --profile app ps` deve mostrar os quatro serviços `healthy`.
   - telas: `http://localhost:4200/saude`;
   - API: `http://localhost:5019/health` (Swagger em `/swagger`, se o ambiente for `Development`);
   - serviço analítico: `http://localhost:8000/health` e `http://localhost:8000/docs`.

O que acontece na subida:
- **Migrations:** a API aplica as pendentes sozinha (`Banco__AplicarMigrationsAoIniciar`). Se alguma falhar, a API
  não sobe.
- **Ordem:** a API só sobe depois do MySQL e do serviço analítico estarem saudáveis.
- **Histórico real (D10):** com `MYSQL_ANALITICA_PASSWORD` no `.env` e o usuário criado (seção acima), o serviço
  analítico lê as views; sem isso, usa o histórico simulado.
- **Imagens:** rodam com usuário sem privilégio de root, e nenhum segredo entra nelas; tudo vem do `.env`.
- **Mesma origem (D12):** as telas chamam `/api` no próprio endereço por onde a página foi aberta, e o nginx
  repassa para a API. Não há CORS para configurar, e **abrir pelo IP da máquina funciona no celular da mesma
  rede sem recompilar nada** — útil para demonstrar a parte do cliente num telefone de verdade.
- **Sem o perfil `app`:** `docker compose up -d` continua subindo só o MySQL, como antes.

Para desligar só as telas, a API e o serviço analítico, mantendo o MySQL:

```bash
docker compose --profile app rm -sf web api analitica
```

> A API avisa no log "Failed to determine the https port for redirect": no contêiner ela atende só HTTP, e o
> redirecionamento para HTTPS fica a cargo de um proxy na frente, num ambiente de produção.

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