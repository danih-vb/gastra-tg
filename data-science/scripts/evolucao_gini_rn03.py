"""Curva do Gini turno a turno partindo do banco semeado (issue #221). Grava o CSV e o gráfico em docs/analises/.

    GASTRA_VALIDACAO_URL="mysql+pymysql://usuario:senha@localhost:3307/gastra_validacao" \
        python scripts/evolucao_gini_rn03.py

    python scripts/evolucao_gini_rn03.py --reusar   # só refaz o gráfico a partir do CSV

Demora cerca de 20 minutos: cada política roda 20 sementes de 120 turnos, e as da RN03 resolvem a programação
linear de verdade a cada turno.
"""

import csv
import os
import sys
from datetime import timedelta
from pathlib import Path

RAIZ = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(RAIZ / "src"))

import matplotlib  # noqa: E402

matplotlib.use("Agg")
import matplotlib.pyplot as plt  # noqa: E402
from sqlalchemy import create_engine, text  # noqa: E402

from gastra_analitica.alocacao.calibracao import (  # noqa: E402
    politica_fixa,
    politica_fixa_invertida,
    politica_programacao_linear,
    politica_rodizio,
)
from gastra_analitica.alocacao.evolucao import (  # noqa: E402
    JANELA_EM_TURNOS,
    EstadoInicial,
    GarcomInicial,
    PracaInicial,
    curva_media,
)

SAIDA = RAIZ.parent / "docs" / "analises"
SEMENTES = tuple(range(20))
TURNOS = 120

POLITICAS = [
    ("RN03 do sistema (w1 = 0,6)", politica_programacao_linear(0.6)),
    ("Só a espera (w1 = 0)", politica_programacao_linear(0.0)),
    ("Rodízio simples", politica_rodizio),
    # Sem regra, o resultado depende de quem calhou de ficar na praça forte: as duas ordens mostram o intervalo.
    ("Sem regra, quem vende menos fixo na praça forte", politica_fixa),
    ("Sem regra, quem vende mais fixo na praça forte", politica_fixa_invertida),
]


def ler_estado(url: str) -> EstadoInicial:
    with create_engine(url).connect() as conexao:
        def ler(sql: str, **parametros):
            return list(conexao.execute(text(sql), parametros))

        ultimo = ler("SELECT MAX(data) FROM vw_desempenho_garcom_turno")[0][0]
        desde = ultimo - timedelta(days=JANELA_EM_TURNOS // 2 - 1)
        turnos = ler("SELECT garcom_id, data, periodo, faturamento FROM vw_desempenho_garcom_turno "
                     "WHERE data >= :desde ORDER BY data, periodo", desde=desde)
        tickets = dict(ler("SELECT garcom_id, AVG(faturamento) FROM vw_comanda_faturamento GROUP BY garcom_id"))
        ticket_geral = float(ler("SELECT AVG(faturamento) FROM vw_comanda_faturamento")[0][0])
        pracas = ler("SELECT t.praca_id, p.quantidade_garcons, AVG(t.comandas), STDDEV_SAMP(t.comandas) "
                     "FROM vw_faturamento_praca_turno t JOIN praca p ON p.id = t.praca_id GROUP BY t.praca_id, "
                     "p.quantidade_garcons ORDER BY t.praca_id")
        medio = dict(ler("SELECT praca_id, faturamento_medio_por_turno FROM vw_faturamento_medio_praca"))
        alocacoes = ler("SELECT garcom_id, praca_id FROM alocacao WHERE confirmada = 1 ORDER BY data DESC, periodo DESC")

    # Turno -1 é o jantar do último dia; quanto mais antigo, mais negativo.
    def indice(data, periodo) -> int:
        return -((ultimo - data).days * 2 + (1 if periodo == "Almoco" else 0)) - 1

    historicos: dict[int, list[tuple[int, float]]] = {}
    for garcom, data, periodo, faturamento in turnos:
        historicos.setdefault(garcom, []).append((indice(data, periodo), float(faturamento)))

    media_das_pracas = sum(float(v) for v in medio.values() if v) / sum(1 for v in medio.values() if v)
    alto = {praca for praca, valor in medio.items() if valor and float(valor) > media_das_pracas}
    espera: dict[int, int] = {}
    parados: set[int] = set()
    for garcom, praca in alocacoes:
        if garcom in parados:
            continue
        if praca in alto:
            parados.add(garcom)
        else:
            espera[garcom] = espera.get(garcom, 0) + 1

    return EstadoInicial(
        garcons=tuple(
            GarcomInicial(g, float(tickets[g]) / ticket_geral, tuple(sorted(h)), espera.get(g, 0))
            for g, h in sorted(historicos.items())
        ),
        pracas=tuple(PracaInicial(p, vagas, float(media), float(desvio or 0)) for p, vagas, media, desvio in pracas),
        ticket_medio=ticket_geral,
    )


def resumir(curvas: dict[str, list[float]]) -> None:
    for nome, curva in curvas.items():
        print(f"{nome}: Gini {curva[0]:.4f} no início, {curva[60]:.4f} no turno 60, {curva[-1]:.4f} no turno {TURNOS}")


def desenhar(curvas: dict[str, list[float]]) -> None:
    fig, eixo = plt.subplots(figsize=(8.5, 5.6))
    estilos = ["-", "--", "-.", ":", (0, (1, 4))]
    for (nome, curva), estilo in zip(curvas.items(), estilos):
        eixo.plot(range(len(curva)), curva, linestyle=estilo, linewidth=2.2 if "RN03" in nome else 1.4, label=nome)
    eixo.axvline(JANELA_EM_TURNOS, color="gray", linewidth=0.8)
    eixo.text(JANELA_EM_TURNOS + 1, 0.168, "janela renovada (30 dias)", fontsize=8, color="gray")
    eixo.set_xlabel("Turnos alocados pela política, a partir do banco semeado")
    eixo.set_ylabel("Gini do faturamento médio por turno (30 dias)")
    eixo.set_title(f"Desigualdade entre os garçons, turno a turno (média de {len(SEMENTES)} sementes)")
    eixo.set_ylim(0, 0.175)
    eixo.grid(alpha=0.3)
    eixo.legend(fontsize=8, loc="upper center", bbox_to_anchor=(0.5, -0.14), ncol=2, frameon=False)
    fig.tight_layout()
    fig.savefig(SAIDA / "evolucao_gini_rn03.png", dpi=150)


def ler_csv() -> dict[str, list[float]]:
    with open(SAIDA / "evolucao_gini_rn03.csv", encoding="utf-8") as arquivo:
        linhas = list(csv.reader(arquivo))
    return {nome: [float(linha[i + 1]) for linha in linhas[1:]] for i, nome in enumerate(linhas[0][1:])}


def main() -> None:
    if "--reusar" in sys.argv:
        curvas = ler_csv()
        resumir(curvas)
        desenhar(curvas)
        return

    url = os.environ.get("GASTRA_VALIDACAO_URL")
    if not url:
        sys.exit("Defina GASTRA_VALIDACAO_URL com a conexão do banco semeado.")
    inicio = ler_estado(url)

    print("Estado inicial lido do banco:")
    for g in inicio.garcons:
        print(f"  garçom {g.id}: fator de venda {g.fator_de_venda:.2f}, {len(g.historico)} turnos na janela, "
              f"espera {g.turnos_desde_alto_potencial}")
    for p in inicio.pracas:
        print(f"  praça {p.id}: {p.vagas} vagas, {p.comandas_por_turno:.1f} ± {p.desvio_de_comandas:.1f} comandas por turno")
    print(f"  ticket médio: R$ {inicio.ticket_medio:.2f}" + chr(10))

    curvas = {nome: curva_media(politica, inicio, SEMENTES, TURNOS) for nome, politica in POLITICAS}
    resumir(curvas)

    SAIDA.mkdir(parents=True, exist_ok=True)
    with open(SAIDA / "evolucao_gini_rn03.csv", "w", newline="", encoding="utf-8") as arquivo:
        escritor = csv.writer(arquivo)
        escritor.writerow(["turno", *curvas])
        for turno in range(TURNOS + 1):
            escritor.writerow([turno, *(f"{curvas[nome][turno]:.5f}" for nome in curvas)])
    desenhar(curvas)
    print(chr(10) + f"Gravados {SAIDA / 'evolucao_gini_rn03.csv'} e {SAIDA / 'evolucao_gini_rn03.png'}")


if __name__ == "__main__":
    main()
