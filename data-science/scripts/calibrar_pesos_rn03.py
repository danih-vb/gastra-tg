"""Roda a calibração dos pesos da RN03 (issue #55) e grava os resultados em docs/analises/.

    python scripts/calibrar_pesos_rn03.py            # simula tudo de novo (~20 minutos)
    python scripts/calibrar_pesos_rn03.py --reusar   # só refaz a escolha e o gráfico a partir do CSV

Demora alguns minutos: cada peso é simulado em 20 sementes de 120 turnos, e cada turno resolve a
programação linear de verdade.
"""

import csv
import sys
import time
from pathlib import Path

RAIZ = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(RAIZ / "src"))

import matplotlib  # noqa: E402

matplotlib.use("Agg")
import matplotlib.pyplot as plt  # noqa: E402

from gastra_analitica.alocacao.calibracao import (  # noqa: E402
    CenarioSimulado,
    ResultadoDaPolitica,
    calibrar,
    escolher_peso,
)

SAIDA = RAIZ.parent / "docs" / "analises"


def main() -> None:
    SAIDA.mkdir(parents=True, exist_ok=True)
    cenario = CenarioSimulado()

    arquivo_csv = SAIDA / "calibracao_rn03.csv"
    if "--reusar" in sys.argv and arquivo_csv.exists():
        # Refaz só a escolha e o gráfico a partir da simulação já gravada (a simulação leva ~20 minutos).
        with open(arquivo_csv, encoding="utf-8") as arquivo:
            resultados = [
                ResultadoDaPolitica(linha["politica"], float(linha["peso_desequilibrio"]) if linha["peso_desequilibrio"] else None,
                                    float(linha["gini_medio"]), float(linha["espera_maxima_media"]))
                for linha in csv.DictReader(arquivo)
            ]
    else:
        inicio = time.time()
        resultados = calibrar(cenario=cenario)
        print(f"Calibração em {time.time() - inicio:.0f} s")
        with open(arquivo_csv, "w", newline="", encoding="utf-8") as arquivo:
            escritor = csv.writer(arquivo)
            escritor.writerow(["politica", "peso_desequilibrio", "gini_medio", "espera_maxima_media"])
            for r in resultados:
                escritor.writerow([r.nome, r.peso_desequilibrio if r.peso_desequilibrio is not None else "",
                                   f"{r.gini_medio:.4f}", f"{r.espera_maxima_media:.2f}"])

    escolhido = escolher_peso(resultados)

    pesos = [r for r in resultados if r.peso_desequilibrio is not None]
    referencias = {r.nome: r for r in resultados if r.peso_desequilibrio is None}

    figura, (eixo_gini, eixo_espera) = plt.subplots(1, 2, figsize=(11, 4.2))
    x = [r.peso_desequilibrio for r in pesos]
    for eixo, atributo, titulo in (
        (eixo_gini, "gini_medio", "Desigualdade (Gini do faturamento por turno)"),
        (eixo_espera, "espera_maxima_media", "Espera máxima sem praça de alto potencial (turnos)"),
    ):
        eixo.plot(x, [getattr(r, atributo) for r in pesos], marker="o", color="#1f5f99", label="Programação linear")
        for nome, estilo in (("fixa (sem regra)", "--"), ("rodízio simples", ":")):
            eixo.axhline(getattr(referencias[nome], atributo), color="#7a7a7a", linestyle=estilo, label=nome)
        eixo.axvline(escolhido.peso_desequilibrio, color="#c0392b", linewidth=1)
        eixo.set_title(titulo, fontsize=10)
        eixo.set_xlabel("w1 (peso do equilíbrio de faturamento); w2 = 1 − w1")
        eixo.grid(alpha=0.3)
    eixo_gini.legend(fontsize=8)
    figura.suptitle(f"Calibração da RN03 — escolhido w1 = {escolhido.peso_desequilibrio:.1f}", fontsize=11)
    figura.tight_layout()
    figura.savefig(SAIDA / "calibracao_rn03.png", dpi=150)

    print(f"{'política':<20} {'Gini':>8} {'espera':>8}")
    for r in resultados:
        marca = "  <- escolhido" if r is escolhido else ""
        print(f"{r.nome:<20} {r.gini_medio:>8.4f} {r.espera_maxima_media:>8.2f}{marca}")


if __name__ == "__main__":
    main()
