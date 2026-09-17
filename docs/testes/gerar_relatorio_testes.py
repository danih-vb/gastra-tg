"""Gera a tabela de cenários de teste executados (issue #48) a partir dos resultados reais dos testes.

    python gerar_relatorio_testes.py <resultados .trx do dotnet test> <pytest.xml> [--commit HASH]

Como obter os arquivos de entrada (com MySQL e serviço Python no ar, para nenhum teste ficar ignorado):

    backend/     dotnet test --logger trx --results-directory <pasta>
    data-science/ pytest --junitxml=<pasta>/pytest.xml

Os cenários são agrupados por bloco do TG. Um cenário com vários casos (teoria do xUnit ou parametrize do
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
    ("LGPD, auditoria e segurança", ["AuditoriaTests", "EliminacaoAuditoriaTests", "AutenticacaoControllerTests",
                                      "UsuarioControllerTests", "UsuarioTests"]),
    ("Banco de dados (views, triggers e restrições)", ["ObjetosDoBancoTests"]),
    ("Integração backend ↔ Python", ["ServicoAnaliticoHttpTests", "ContratoComPythonTests", "RegistroServicoAnaliticoTests"]),
    ("Arquitetura e infraestrutura", ["ArquiteturaTests", "DocumentacaoOpenApiTests", "HealthCheckTests",
                                       "test_arquitetura", "test_health"]),
])
BLOCO_DA_CLASSE = {classe: bloco for bloco, classes in BLOCOS.items() for classe in classes}


@dataclass
class Cenario:
    bloco: str
    origem: str  # classe ou módulo de teste
    nome: str
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
        registrar(cenarios, classe, metodo, situacao)


def ler_junit(caminho: Path, cenarios: dict) -> None:
    for caso in ET.parse(caminho).getroot().iter("testcase"):
        modulo = caso.get("classname").split(".")[-1]
        nome = re.sub(r"\[.*\]$", "", caso.get("name"))
        if caso.find("failure") is not None or caso.find("error") is not None:
            situacao = "Falhou"
        elif caso.find("skipped") is not None:
            situacao = "Ignorado"
        else:
            situacao = "Passou"
        registrar(cenarios, modulo, nome, situacao)


def registrar(cenarios: dict, origem: str, metodo: str, situacao: str) -> None:
    chave = (origem, metodo)
    if chave not in cenarios:
        cenarios[chave] = Cenario(BLOCO_DA_CLASSE.get(origem, "Outros"), origem, legivel(metodo))
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
        "  .NET 10 e Python 3.13.",
        f"- **Resultado:** {len(cenarios)} cenários e {len(casos)} casos executados: {casos.count('Passou')} passaram,"
        f" {casos.count('Falhou')} falharam e {casos.count('Ignorado')} foram ignorados.",
        "",
        "Cobre o critério da issue #48 para os **testes automatizados**: pelo menos um cenário por bloco (comandas,",
        "alocação por programação linear, recomendação e LGPD), com o resultado executado. Os cenários operacionais",
        "de ponta a ponta com o frontend entram no marco M6.",
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
        linhas += ["", f"## {bloco}", "", "| # | Cenário | Origem | Casos | Resultado |", "|---|---|---|---|---|"]
        for indice, cenario in enumerate(sorted(por_bloco[bloco], key=lambda c: (c.origem, c.nome)), start=1):
            linhas.append(f"| {indice} | {cenario.nome} | `{cenario.origem}` | {cenario.casos} | {cenario.resultado} |")

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
