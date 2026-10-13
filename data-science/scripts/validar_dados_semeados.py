"""Valida os blocos analíticos sobre um banco preenchido pelo semeador (issues #185 e da clusterização).

Lê só as views, como o serviço (D10), e imprime em Markdown os números de
docs/analises/GASTRA_Validacao_Dados_Simulados.md:

1. clusterização: quantidade de perfis, silhueta, itens marcantes e o perfil cruzado com período e composição;
2. recomendação: taxa de acerto das regras gerais contra a segmentada, e as sugestões para a moqueca;
3. alocação: faturamento por turno dos últimos 30 dias, praça designada pela RN03 e o Gini.

Uso, com o MySQL no ar e um banco semeado (de preferência separado do de desenvolvimento):

    GASTRA_VALIDACAO_URL="mysql+pymysql://usuario:senha@localhost:3307/gastra_validacao" \
        python scripts/validar_dados_semeados.py
"""

import os
import sys
from collections import Counter, defaultdict
from datetime import timedelta
from pathlib import Path

RAIZ = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(RAIZ / "src"))

from sqlalchemy import create_engine, text  # noqa: E402

from gastra_analitica.alocacao.calibracao import gini  # noqa: E402
from gastra_analitica.alocacao.programacao_linear import (  # noqa: E402
    GarcomDisponivel,
    PracaDoTurno,
    sugerir_alocacao,
)
from gastra_analitica.recomendacao.regras_associacao import treinar  # noqa: E402
from gastra_analitica.recomendacao.segmentada import taxa_de_acerto, treinar_segmentado  # noqa: E402

PESO_DESEQUILIBRIO, PESO_ESPERA = 0.6, 0.4
JANELA_DA_RN03 = 30


def ler(conexao, sql: str, **parametros) -> list:
    return list(conexao.execute(text(sql), parametros))


def main() -> None:
    url = os.environ.get("GASTRA_VALIDACAO_URL")
    if not url:
        sys.exit("Defina GASTRA_VALIDACAO_URL com a conexão do banco semeado.")
    engine = create_engine(url)

    with engine.connect() as conexao:
        itens_por_comanda = defaultdict(list)
        for comanda_id, item_id in ler(conexao, "SELECT comanda_id, item_cardapio_id FROM vw_itens_por_comanda "
                                                 "ORDER BY comanda_id, item_cardapio_id"):
            itens_por_comanda[comanda_id].append(item_id)
        nomes = dict(ler(conexao, "SELECT DISTINCT item_cardapio_id, "
                                  "(SELECT nome FROM item_cardapio WHERE id = item_cardapio_id) "
                                  "FROM vw_faturamento_item_cardapio"))
        contexto = {c: (p, comp) for c, p, comp in ler(conexao, "SELECT comanda_id, periodo, composicao "
                                                                "FROM vw_comanda_faturamento")}
        ultimo_dia = ler(conexao, "SELECT MAX(data) FROM vw_comanda_faturamento")[0][0]
        desde = ultimo_dia - timedelta(days=JANELA_DA_RN03 - 1)
        faturamento = dict(ler(conexao, "SELECT garcom_id, AVG(faturamento) FROM vw_desempenho_garcom_turno "
                                        "WHERE data >= :desde GROUP BY garcom_id", desde=desde))
        pracas = ler(conexao, "SELECT v.praca_id, p.quantidade_garcons, v.faturamento_medio_por_turno "
                              "FROM vw_faturamento_medio_praca v JOIN praca p ON p.id = v.praca_id ORDER BY 1")
        historico = ler(conexao, "SELECT garcom_id, praca_id FROM alocacao WHERE confirmada = 1 "
                                 "ORDER BY data DESC, periodo DESC")

    comandas = [itens_por_comanda[c] for c in sorted(itens_por_comanda)]
    ids = sorted(itens_por_comanda)
    print(f"# Validação sobre o banco semeado — {len(comandas)} comandas com itens\n")

    # 1. Clusterização
    modelo = treinar_segmentado(comandas)
    perfis = modelo.perfis
    print("## Clusterização\n")
    print(f"Silhueta por quantidade de perfis: {perfis.silhuetas_testadas}; escolhida: {len(perfis.perfis)} "
          f"(silhueta {perfis.silhueta}).\n")
    print("| Perfil | Comandas | Participação | Itens marcantes (presença; destaque) | Almoço | 3+ pessoas |")
    print("|---|---:|---:|---|---:|---:|")
    for perfil in perfis.perfis:
        membros = [ids[i] for i, rotulo in enumerate(perfis.rotulos) if rotulo == perfil.id]
        almoco = sum(contexto[c][0] == "Almoco" for c in membros if c in contexto) / len(membros)
        composicoes = Counter(contexto[c][1] for c in membros if c in contexto)
        grandes = sum(n for comp, n in composicoes.items() if comp in ("Familia", "GrupoPequeno", "GrupoGrande"))
        marcantes = "; ".join(f"{nomes.get(i.item_id, i.item_id)} ({i.presenca:.0%}; {i.destaque:.1f}x)"
                              for i in perfil.itens_marcantes)
        print(f"| {perfil.id} | {perfil.comandas} | {perfil.participacao:.0%} | {marcantes} | {almoco:.0%} | "
              f"{grandes / len(membros):.0%} |")

    # 2. Recomendação
    corte = int(len(comandas) * 0.7)
    geral = taxa_de_acerto(treinar, comandas[:corte], comandas[corte:])
    segmentada = taxa_de_acerto(treinar_segmentado, comandas[:corte], comandas[corte:])
    print("\n## Recomendação\n")
    print(f"Taxa de acerto no item escondido (top 3, 70% treino / 30% teste): regras gerais {geral:.1%}, "
          f"segmentada {segmentada:.1%} ({(segmentada - geral) * 100:+.1f} pontos).\n")
    moqueca = next(i for i, n in nomes.items() if n.startswith("Moqueca"))
    print("| Sugestão para a moqueca | Confiança | Lift |")
    print("|---|---:|---:|")
    for sugestao in treinar(comandas).sugerir([moqueca]):
        print(f"| {nomes[sugestao.item_id]} | {sugestao.confianca:.3f} | {sugestao.lift:.3f} |")

    # 3. Alocação
    media = sum(p[2] for p in pracas if p[2] > 0) / sum(1 for p in pracas if p[2] > 0)
    alto = {p[0] for p in pracas if p[2] > media}
    espera: dict[int, int] = {}
    parados: set[int] = set()
    for garcom, praca in historico:
        if garcom in parados:
            continue
        if praca in alto:
            parados.add(garcom)
        else:
            espera[garcom] = espera.get(garcom, 0) + 1
    garcons = [GarcomDisponivel(g, float(f), espera.get(g, 0)) for g, f in faturamento.items()]
    resultado = sugerir_alocacao(garcons, [PracaDoTurno(p[0], p[1], float(p[2])) for p in pracas],
                                 PESO_DESEQUILIBRIO, PESO_ESPERA)
    designada = {d.garcom_id: d.praca_id for d in resultado.designacoes}
    potencial = {p[0]: float(p[2]) for p in pracas}
    print(f"\n## Alocação (últimos {JANELA_DA_RN03} dias até {ultimo_dia:%d/%m})\n")
    print("| Garçom | Faturamento por turno (R$) | Turnos sem praça de alto potencial | Praça designada | "
          "Potencial da praça (R$ por turno) |")
    print("|---|---:|---:|---|---:|")
    for garcom in sorted(faturamento, key=lambda g: faturamento[g]):
        praca = designada[garcom]
        print(f"| {garcom} | {float(faturamento[garcom]):.2f} | {espera.get(garcom, 0)} | {praca} | "
              f"{potencial[praca]:.0f} |")
    print(f"\nGini do faturamento médio por turno: {gini([float(v) for v in faturamento.values()]):.4f}")


if __name__ == "__main__":
    main()
