# API do GASTRA (issue #135). Construída a partir de backend/:
#   docker build -t gastra-api backend
# Nenhum segredo entra na imagem: chave do JWT, conexão com o banco e administrador inicial vêm de variáveis
# de ambiente na execução (infra/docker-compose.yml, perfil "app").

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaura primeiro só com os .csproj: a camada de pacotes fica em cache enquanto as dependências não mudam.
COPY global.json Gastra.slnx ./
COPY src/Gastra.Api/Gastra.Api.csproj src/Gastra.Api/
COPY src/Gastra.Application/Gastra.Application.csproj src/Gastra.Application/
COPY src/Gastra.Communication/Gastra.Communication.csproj src/Gastra.Communication/
COPY src/Gastra.Domain/Gastra.Domain.csproj src/Gastra.Domain/
COPY src/Gastra.Exceptions/Gastra.Exceptions.csproj src/Gastra.Exceptions/
COPY src/Gastra.Infrastructure/Gastra.Infrastructure.csproj src/Gastra.Infrastructure/
RUN dotnet restore src/Gastra.Api/Gastra.Api.csproj

COPY src/ src/
RUN dotnet publish src/Gastra.Api/Gastra.Api.csproj --configuration Release --no-restore --output /publicado

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS execucao
WORKDIR /app

# curl só para a verificação de saúde do contêiner.
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /publicado .

# Usuário sem privilégio de root, que já vem na imagem oficial.
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=5 \
  CMD curl -fs http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Gastra.Api.dll"]
