# GASTRA — Guia do Ambiente de Desenvolvimento

Objetivo: permitir que qualquer integrante monte o ambiente do zero e rode o projeto sem
precisar perguntar ao outro (issue #81).

Sistema de referência: **Windows 11**. As versões da coluna "Testado com" foram as usadas para
validar o projeto em 14/09/2026.

---

## 1. Ferramentas

### 1.1 Obrigatórias

| Ferramenta | Testado com | Para quê | Instalação |
|---|---|---|---|
| Git | 2.48.1 | versionamento | https://git-scm.com |
| GitHub CLI (`gh`) | 2.100.0 | PRs, issues e board pelo terminal | `winget install GitHub.cli` |
| Docker Desktop (engine) | 29.7.2 | roda o MySQL | https://www.docker.com/products/docker-desktop |
| Docker Compose | 5.5.1 | sobe o `infra/docker-compose.yml` | vem com o Docker Desktop |
| WSL | 2.7.14 | backend Linux do Docker Desktop | `wsl --install` (ver seção 5) |
| .NET SDK | **10.0.401** | backend (fixado pelo `backend/global.json`) | `winget install Microsoft.DotNet.SDK.10` |
| dotnet-ef | 10.0.8 | migrations do Entity Framework | `dotnet tool install --global dotnet-ef` |
| Node.js | 24.16.0 | frontend | https://nodejs.org |
| npm | 11.13.0 | pacotes do frontend | vem com o Node.js |
| Python | 3.13.3 | camada analítica | https://www.python.org |

### 1.2 Editores

| Ferramenta | Testado com | Observação |
|---|---|---|
| Visual Studio Code | 1.137.0 | suficiente para as três camadas (backend, frontend, Python) |
| Visual Studio Community | **2026** | opcional, para o backend. **A versão 2022 não suporta projetos .NET 10** |
| MySQL Workbench | 8.0.41 CE | opcional, para inspecionar o banco |

### 1.3 Extensões do VS Code

| Extensão | ID | Para quê |
|---|---|---|
| C# Dev Kit | `ms-dotnettools.csdevkit` | backend (compilar, depurar, testes) |
| C# | `ms-dotnettools.csharp` | linguagem C# |
| Python | `ms-python.python` | camada analítica |
| Pylance | `ms-python.vscode-pylance` | autocompletar Python |
| Jupyter | `ms-toolsai.jupyter` | notebooks em `data-science/notebooks/` |
| Angular Language Service | `angular.ng-template` | autocompletar nos templates HTML do Angular |

Para instalar todas de uma vez:

```bash
code --install-extension ms-dotnettools.csdevkit --install-extension ms-dotnettools.csharp --install-extension ms-python.python --install-extension ms-python.vscode-pylance --install-extension ms-toolsai.jupyter --install-extension angular.ng-template
```

### 1.4 Angular CLI: não instalar globalmente

O projeto usa o Angular CLI **da própria pasta** `frontend/` (`npx ng ...` ou `npm run ...`).
Um `ng` global antigo não suporta Node 24 e gera avisos de incompatibilidade.

---

## 2. Montando o projeto do zero

### 2.1 Clonar

```bash
git clone https://github.com/danih-vb/gastra-tg.git
cd gastra-tg
```

A branch padrão é `dev`. Nunca trabalhe direto nela — ver `CONTRIBUTING.md`.

### 2.2 Banco de dados (`infra/`)

```bash
cd infra
cp .env.example .env
```

Edite o `.env` e **troque as senhas**. O arquivo nunca é versionado.

```bash
docker compose up -d --wait
```

O `--wait` só retorna quando o healthcheck do MySQL passa. Detalhes de conexão em
`infra/README.md`.

### 2.3 Camada analítica (`data-science/`)

```bash
cd data-science
python -m venv .venv
.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

Detalhes (inclusive o uso do solver do PuLP) em `data-science/README.md`.

### 2.4 Backend (`backend/`)

```bash
cd backend
dotnet --version
dotnet test
```

O `dotnet --version` dentro de `backend/` deve mostrar `10.0.x`. Detalhes em `backend/README.md`.

### 2.5 Frontend (`frontend/`)

```bash
cd frontend
npm install
npm test -- --watch=false
```

Detalhes em `frontend/README.md`.

---

## 3. Checklist de verificação

Com tudo instalado, cada linha abaixo deve dar o resultado esperado.

| Verificação | Comando | Esperado |
|---|---|---|
| .NET fixado | `dotnet --version` (dentro de `backend/`) | `10.0.x` |
| Migrations disponíveis | `dotnet ef --version` | `10.x` |
| Docker ativo | `docker version` | seções *Client* e *Server* sem erro |
| MySQL de pé | `docker compose ps` (dentro de `infra/`) | `gastra-mysql` com status `healthy` |
| Testes do backend | `dotnet test` (dentro de `backend/`) | todos aprovados |
| API respondendo | `dotnet run --project src/Gastra.Api` e abrir `http://localhost:5019/health` | `Healthy` |
| Testes do frontend | `npm test -- --watch=false` (dentro de `frontend/`) | todos aprovados |
| Frontend no navegador | `npm start` e abrir `http://localhost:4200` | tela com o cabeçalho GASTRA |
| Python | `python -c "import pandas, sklearn, mlxtend, pulp"` (venv ativado) | nenhum erro |
| GitHub CLI | `gh auth status` | `Logged in to github.com` |

---

## 4. Portas usadas

| Serviço | Porta | Onde é definida |
|---|---|---|
| MySQL (no host) | 3307 | `infra/.env` (`MYSQL_PORT`) |
| API | 5019 | `backend/src/Gastra.Api/Properties/launchSettings.json` |
| Frontend | 4200 | padrão do Angular CLI |

---

## 5. Problemas conhecidos

### 5.1 Docker Desktop: "Virtualization support not detected"

**Sintoma:** o Docker Desktop não inicia; `wsl --status` diz que a virtualização não está
habilitada, mesmo com a virtualização ligada na BIOS.

**Diagnóstico** (PowerShell):

```powershell
(Get-CimInstance Win32_Processor).VirtualizationFirmwareEnabled
(Get-CimInstance Win32_ComputerSystem).HypervisorPresent
Test-Path "$env:windir\System32\vmcompute.exe"
```

| Resultado | Causa | Correção |
|---|---|---|
| 1ª linha `False` | virtualização desligada na BIOS/UEFI | ativar Intel VT-x / AMD-V na BIOS |
| 2ª linha `False` | componente "Plataforma de Máquina Virtual" desligado | passos abaixo |
| 2ª linha `True` e 3ª linha `False` | componente instalado pela metade: falta o serviço `vmcompute` | passos abaixo, **incluindo o reparo** |

**Correção** (PowerShell **como administrador**, nesta ordem):

```powershell
DISM /Online /Cleanup-Image /RestoreHealth
dism /online /disable-feature /featurename:VirtualMachinePlatform /norestart
```

Reinicie o computador (Iniciar → Reiniciar, não "Desligar").

```powershell
dism /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart
```

Reinicie de novo e abra o Docker Desktop.

### 5.2 "Node.js version ... is not supported" ao usar `ng`

Um Angular CLI global antigo está sendo usado. Use `npx ng` ou os scripts `npm run` dentro de
`frontend/` (seção 1.4).

### 5.3 Aviso "LF will be replaced by CRLF" no Git

Não é erro: o Git no Windows converte o fim de linha automaticamente. Pode ser ignorado.

### 5.4 PowerShell bloqueia `Activate.ps1`

Mensagem de "execução de scripts desabilitada neste sistema". Libera scripts locais apenas para
o usuário atual:

```powershell
Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
```
