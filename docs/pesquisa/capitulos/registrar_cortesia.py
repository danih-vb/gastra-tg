"""Registra no Gastra II o requisito novo de 21/11/2026: cortesia (Metre e Gerente) e ajuste de preço (Gerente).

    pip install python-docx
    python docs/pesquisa/capitulos/registrar_cortesia.py

Acrescenta o RF26 ao Quadro 1, o UC26 ao Quadro 4, a US25 ao quadro das histórias de usuário e atualiza as
contagens do texto. Rodar de novo não duplica nada. Depois, no Word: Ctrl+A e F9.
"""

import copy
from pathlib import Path

from docx import Document
from docx.oxml.ns import qn

DOCX = Path(__file__).resolve().parent.parent / "Gastra II.docx"
Z = "​"  # quebra opcional nos nomes de teste, como nas outras linhas do quadro

RF26 = ["RF26", "Lançar item de cortesia na comanda e ajustar o preço de um item já lançado, sempre com motivo",
        "Metre e Gerente (cortesia); Gerente (ajuste)", "Decisão da dupla (21/11)"]
US25 = [
    "US25 — Como Metre ou Gerente, quero oferecer uma cortesia à mesa e, como Gerente, ajustar o preço de um item "
    "lançado (RF26)",
    "A cortesia entra por R$ 0,00 e guarda o preço do cardápio, mesmo para item fora de venda; o ajuste guarda o "
    "preço original; o motivo é obrigatório e não vai para o log; o Garçom não faz nenhum dos dois",
    f"ComandaControllerTests.{Z}Cortesia_{Z}PeloMetre_{Z}EntraPorZeroMesmoComItemIndisponivel; "
    f"ComandaControllerTests.{Z}Cortesia_{Z}PeloGarcom_{Z}Retorna403; "
    f"ComandaControllerTests.{Z}AlterarPreco_{Z}PeloGerente_{Z}MudaAContaEOClienteVeOValorOriginal; "
    f"ComandaControllerTests.{Z}AlterarPreco_{Z}PeloMetre_{Z}Retorna403",
]
TEXTOS = [
    ("Foram especificados 25 requisitos funcionais, dos quais 24 foram aprovados",
     "Foram especificados 26 requisitos funcionais, dos quais 25 foram aprovados"),
    ("Os requisitos funcionais foram detalhados em 25 casos de uso", "Os requisitos funcionais foram detalhados em 26 casos de uso"),
    ("e o UC25 surgiu com a avaliação do atendimento (RF25).",
     "o UC25 surgiu com a avaliação do atendimento (RF25), e o UC26 surgiu com o RF26, na fase final, para que o "
     "Metre e o Gerente pudessem oferecer uma cortesia e o Gerente pudesse ajustar o preço de um item lançado, "
     "sempre com motivo registrado."),
]


def escrever_na_celula(celula, texto: str) -> None:
    paragrafos = celula.paragraphs
    for extra in paragrafos[1:]:
        extra._p.getparent().remove(extra._p)
    runs = paragrafos[0].runs
    for extra in runs[1:]:
        extra._r.getparent().remove(extra._r)
    if runs:
        runs[0].text = texto
    else:
        paragrafos[0].add_run(texto)


def acrescentar_linha(tabela, valores) -> bool:
    if any(linha.cells[0].text.startswith(valores[0][:4]) for linha in tabela.rows):
        return False
    nova = copy.deepcopy(tabela.rows[-1]._tr)
    tabela.rows[-1]._tr.addnext(nova)
    for celula, valor in zip(tabela.rows[-1].cells, valores):
        escrever_na_celula(celula, valor)
    return True


def trocar_texto(paragrafo, atual: str, novo: str) -> bool:
    if novo in paragrafo.text or atual not in paragrafo.text:
        return False
    for run in paragrafo.runs:
        if atual in run.text:
            run.text = run.text.replace(atual, novo)
            return True
    raise SystemExit(f"Trecho dividido entre formatações, trocar à mão: {atual[:50]}")


def main() -> None:
    documento = Document(str(DOCX))
    quadro_rf = next(t for t in documento.tables if t.rows[0].cells[0].text == "ID"
                     and any(r.cells[0].text == "RF25" for r in t.rows))
    quadro_uc = next(t for t in documento.tables if t.rows[0].cells[0].text == "Grupo")
    quadro_us = next(t for t in documento.tables if t.rows[0].cells[0].text == "História")

    feitos = [acrescentar_linha(quadro_rf, RF26), acrescentar_linha(quadro_us, US25)]

    nucleo = next(r for r in quadro_uc.rows if "UC12" in r.cells[1].text)
    if "UC26" not in nucleo.cells[1].text:
        escrever_na_celula(nucleo.cells[1], nucleo.cells[1].text + "; UC26 Lançar cortesia e ajustar preço")
        atores = nucleo.cells[2].text
        for papel in ("Metre", "Gerente"):
            if papel not in atores:
                atores += ", " + papel
        escrever_na_celula(nucleo.cells[2], atores)
        feitos.append(True)

    for atual, novo in TEXTOS:
        feitos.append(any(trocar_texto(p, atual, novo) for p in documento.paragraphs))

    documento.save(str(DOCX))
    print(f"{sum(feitos)} alterações. No Word: Ctrl+A e F9.")


if __name__ == "__main__":
    main()
