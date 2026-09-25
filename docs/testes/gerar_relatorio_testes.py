"""Gera a tabela de cenários de teste executados (issue #48) a partir dos resultados reais dos testes.

    python gerar_relatorio_testes.py <.trx do dotnet test> <pytest.xml> <vitest.xml> [--commit HASH]

Como obter os arquivos de entrada (com MySQL e serviço Python no ar, para nenhum teste ficar ignorado):

    backend/      dotnet test --logger trx --results-directory <pasta>
    data-science/ pytest --junitxml=<pasta>/pytest.xml
    frontend/     ng test --watch=false --reporters junit --output-file <pasta>/vitest.xml

Os cenários são agrupados por bloco do TG e, dentro do bloco, por camada (backend, analítica, telas). Um cenário com vários casos (teoria do xUnit ou parametrize do
pytest) aparece uma vez, com a quantidade de casos.
"""

from __future__ import annotations

import re
import sys
import xml.etree.ElementTree as ET
from collections import OrderedDict, defaultdict
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path

TRX = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"

# Classe de teste -> bloco do TG. Uma classe nova sem bloco cai em "Outros", e o relatório avisa.
BLOCOS = OrderedDict([
    ("Núcleo de comandas", ["ComandaControllerTests", "ComandaTests", "SalaoControllerTests"]),
    ("Cardápio e promoções", ["CardapioControllerTests", "ItemDoCardapioTests", "PromocaoControllerTests", "PromocaoTests"]),
    ("Alocação de garçons (programação linear)", ["AlocacaoControllerTests", "AlocacaoTests", "RegraDeDistribuicaoTests",
                                                   "test_alocacao", "test_calibracao"]),
    ("Recomendação de pratos (ciência de dados)", ["SugestoesComandaTests", "test_recomendacao", "test_provedor_modelo",
                                                    "test_historico_banco", "test_api_analitica"]),
    ("BI e índice de desempenho", ["IndicadoresControllerTests", "IndiceDeDesempenhoTests", "RepositorioIndicadoresTests"]),
    ("LGPD, auditoria e segurança", ["AuditoriaTests", "LimiteDeRequisicoesTests", "EliminacaoAuditoriaTests", "AutenticacaoControllerTests",
                                      "UsuarioControllerTests", "UsuarioTests"]),
    ("Banco de dados (views, triggers e restrições)", ["ObjetosDoBancoTests"]),
    ("Integração backend ↔ Python", ["ServicoAnaliticoHttpTests", "ContratoComPythonTests", "RegistroServicoAnaliticoTests"]),
    ("Arquitetura e infraestrutura", ["ArquiteturaTests", "CorsTests", "DocumentacaoOpenApiTests", "HealthCheckTests",
                                       "test_arquitetura", "test_health"]),
])
BLOCO_DA_CLASSE = {classe: bloco for bloco, classes in BLOCOS.items() for classe in classes}

# Trecho do caminho do .spec.ts -> bloco do TG. A ordem importa: vence o primeiro trecho que casar.
BLOCOS_DO_FRONTEND = [
    ("features/comandas", "Núcleo de comandas"),
    ("features/salao", "Núcleo de comandas"),
    ("features/cliente/conta", "Núcleo de comandas"),
    ("features/cliente/inicio", "Núcleo de comandas"),
    ("features/cardapio", "Cardápio e promoções"),
    ("features/cliente/cardapio-digital", "Cardápio e promoções"),
    ("features/alocacao", "Alocação de garçons (programação linear)"),
    ("features/analises", "BI e índice de desempenho"),
    ("features/usuarios", "LGPD, auditoria e segurança"),
    ("features/acesso", "LGPD, auditoria e segurança"),
    ("core/sessao", "LGPD, auditoria e segurança"),
    ("app.spec.ts", "LGPD, auditoria e segurança"),  # o menu por papel é controle de acesso
    ("shared/rotulos", "Núcleo de comandas"),  # composicaoSugerida e os rótulos de RN01
    ("shared/", "Arquitetura e infraestrutura"),
]

BACKEND, ANALITICA, FRONTEND = "Backend .NET", "Analítica Python", "Frontend Angular"


@dataclass
class Cenario:
    bloco: str
    origem: str  # classe, módulo ou tela de teste
    nome: str
    camada: str = BACKEND
    casos: int = 0
    resultados: list[str] = field(default_factory=list)

    @property
    def resultado(self) -> str:
        if any(r == "Falhou" for r in self.resultados):
            return "❌ Falhou"
        if all(r == "Ignorado" for r in self.resultados):
            return "⏭ Ignorado"
        return "✅ Passou"


def legivel(nome: str) -> str:
    """Fechar_ComItemPendente_Retorna422 -> "Fechar com item pendente retorna 422"."""
    nome = re.sub(r"^test_", "", nome)
    partes = []
    for trecho in nome.split("_"):
        trecho = re.sub(r"(?<=[a-zà-ú])(?=[A-Z0-9])|(?<=[0-9])(?=[A-Za-z])|(?<=[A-Z])(?=[A-Z][a-z])", " ", trecho)
        partes.append(trecho)
    texto = " ".join(p for p in partes if p).strip()
    palavras = texto.split(" ")
    # Siglas (UC15, RN03, CHECK) ficam como estão; o resto vai para minúsculas, menos a primeira letra.
    # Duas maiúsculas sem número são duas palavras de uma letra ("EO" em ContaEOMotivo), e não sigla.
    palavras = [" ".join(p.lower()) if re.fullmatch(r"[A-Z]{2}", p) else p for p in palavras]
    palavras = [p if (p.isupper() and len(p) > 2) or re.match(r"^[A-Z]{2,}\d+$", p) else p.lower() for p in palavras]
    texto = " ".join(palavras)
    return texto[:1].upper() + texto[1:]


def ler_trx(caminho: Path, cenarios: dict) -> None:
    raiz = ET.parse(caminho).getroot()
    definicoes = {}
    for teste in raiz.iter(f"{TRX}UnitTest"):
        metodo = teste.find(f"{TRX}TestMethod")
        definicoes[teste.get("id")] = (metodo.get("className").split(".")[-1], metodo.get("name"))

    for resultado in raiz.iter(f"{TRX}UnitTestResult"):
        classe, metodo = definicoes[resultado.get("testId")]
        situacao = {"Passed": "Passou", "Failed": "Falhou", "NotExecuted": "Ignorado"}.get(resultado.get("outcome"), "Falhou")
        registrar(cenarios, classe, metodo, situacao, BACKEND)


def ler_junit(caminho: Path, cenarios: dict) -> None:
    """JUnit XML do pytest e do vitest. O vitest identifica o caso pelo arquivo .spec.ts."""
    for caso in ET.parse(caminho).getroot().iter("testcase"):
        classname = caso.get("classname")
        situacao = situacao_do_junit(caso)
        if classname.endswith(".spec.ts"):
            registrar_tela(cenarios, classname, caso.get("name"), situacao)
        else:
            registrar(cenarios, classname.split(".")[-1], re.sub(r"\[.*\]$", "", caso.get("name")), situacao, ANALITICA)


def situacao_do_junit(caso: ET.Element) -> str:
    if caso.find("failure") is not None or caso.find("error") is not None:
        return "Falhou"
    return "Ignorado" if caso.find("skipped") is not None else "Passou"


def registrar(cenarios: dict, origem: str, metodo: str, situacao: str, camada: str = BACKEND) -> None:
    chave = (origem, metodo)
    if chave not in cenarios:
        cenarios[chave] = Cenario(BLOCO_DA_CLASSE.get(origem, "Outros"), origem, legivel(metodo), camada)
    cenarios[chave].casos += 1
    cenarios[chave].resultados.append(situacao)


def registrar_tela(cenarios: dict, arquivo: str, nome: str, situacao: str) -> None:
    """No vitest, `name` vem como "describe > it" e já está escrito em português: não passa pelo legivel()."""
    caminho = arquivo.replace("\\", "/")
    bloco = next((b for trecho, b in BLOCOS_DO_FRONTEND if trecho in caminho), "Outros")
    describe, _, teste = nome.rpartition(" > ")
    origem = describe.split(" > ")[0] if describe else Path(caminho).name
    chave = (origem, teste)
    if chave not in cenarios:
        cenarios[chave] = Cenario(bloco, origem, teste[:1].upper() + teste[1:], FRONTEND)
    cenarios[chave].casos += 1
    cenarios[chave].resultados.append(situacao)


def gerar(cenarios: dict, commit: str) -> str:
    por_bloco = defaultdict(list)
    for cenario in cenarios.values():
        por_bloco[cenario.bloco].append(cenario)

    casos = [r for c in cenarios.values() for r in c.resultados]
    linhas = [
        "# GASTRA — Cenários de teste executados",
        "",
        "> **Gerado a partir da execução real dos testes automatizados** (`docs/testes/gerar_relatorio_testes.py`).",
        "> Não edite à mão: rode os testes e o script de novo.",
        "",
        f"- **Execução:** {datetime.now():%d/%m/%Y %H:%M}, commit `{commit}`.",
        "- **Ambiente:** MySQL 8.4 real (testes de views, triggers e restrições), serviço Python no ar (testes de contrato),",
        "  .NET 10, Python 3.13 e Angular 22 sobre Vitest.",
        f"- **Resultado:** {len(cenarios)} cenários e {len(casos)} casos executados: {casos.count('Passou')} passaram,"
        f" {casos.count('Falhou')} falharam e {casos.count('Ignorado')} foram ignorados.",
        "",
        "Cobre o critério da issue #48 para os **testes automatizados**: pelo menos um cenário por bloco (comandas,",
        "alocação por programação linear, recomendação e LGPD), com o resultado executado. Cada bloco aparece nas três",
        "camadas em que o GASTRA foi construído — backend .NET, camada analítica Python e as telas Angular —, de modo",
        "que a regra de negócio e a tela que a mostra ao usuário são verificadas no mesmo lugar.",
        "",
        "O que **não** está aqui: os cenários operacionais de ponta a ponta (navegador contra a API e o banco reais,",
        "sem dublê), que dependem de dados de operação e entram no marco M6.",
        "",
        "## Resumo por camada",
        "",
        "| Camada | Cenários | Casos | Passaram | Falharam | Ignorados |",
        "|---|---|---|---|---|---|",
    ]
    for camada in (BACKEND, ANALITICA, FRONTEND):
        da_camada = [c for c in cenarios.values() if c.camada == camada]
        resultados = [r for c in da_camada for r in c.resultados]
        linhas.append(f"| {camada} | {len(da_camada)} | {len(resultados)} | {resultados.count('Passou')} | "
                      f"{resultados.count('Falhou')} | {resultados.count('Ignorado')} |")

    linhas += [
        "",
        "## Resumo por bloco",
        "",
        "| Bloco | Cenários | Casos | Passaram | Falharam | Ignorados |",
        "|---|---|---|---|---|---|",
    ]
    ordem = list(BLOCOS) + [b for b in por_bloco if b not in BLOCOS]
    for bloco in ordem:
        if bloco not in por_bloco:
            continue
        resultados = [r for c in por_bloco[bloco] for r in c.resultados]
        linhas.append(f"| {bloco} | {len(por_bloco[bloco])} | {len(resultados)} | {resultados.count('Passou')} | "
                      f"{resultados.count('Falhou')} | {resultados.count('Ignorado')} |")

    for bloco in ordem:
        if bloco not in por_bloco:
            continue
        linhas += ["", f"## {bloco}", "", "| # | Cenário | Camada | Origem | Casos | Resultado |", "|---|---|---|---|---|---|"]
        ordem_camada = {BACKEND: 0, ANALITICA: 1, FRONTEND: 2}
        for indice, cenario in enumerate(
            sorted(por_bloco[bloco], key=lambda c: (ordem_camada[c.camada], c.origem, c.nome)), start=1
        ):
            linhas.append(f"| {indice} | {cenario.nome} | {cenario.camada} | `{cenario.origem}` | {cenario.casos} | "
                          f"{cenario.resultado} |")

    if "Outros" in por_bloco:
        linhas += ["", "> ⚠️ Há classes de teste sem bloco definido em `BLOCOS` no script. Atualize o mapeamento."]

    return "\n".join(linhas) + "\n"


def main() -> None:
    argumentos = sys.argv[1:]
    commit = "desconhecido"
    if "--commit" in argumentos:
        indice = argumentos.index("--commit")
        commit = argumentos[indice + 1]
        del argumentos[indice:indice + 2]

    cenarios: dict = OrderedDict()
    for caminho in map(Path, argumentos):
        (ler_trx if caminho.suffix == ".trx" else ler_junit)(caminho, cenarios)

    saida = Path(__file__).with_name("GASTRA_Cenarios_Testes_Executados.md")
    saida.write_text(gerar(cenarios, commit), encoding="utf-8")
    print(f"{saida.name}: {len(cenarios)} cenários")


if __name__ == "__main__":
    main()
