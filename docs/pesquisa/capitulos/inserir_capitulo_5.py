"""Leva o capítulo 5 (Markdown) para o Gastra II.docx e acrescenta a referência que ele passou a citar.

    pip install python-docx
    python docs/pesquisa/capitulos/inserir_capitulo_5.py

Depois, no Word: Ctrl+A e F9, para atualizar sumário, listas e numeração das legendas.
"""

import copy
from pathlib import Path

from docx import Document
from docx.oxml.ns import qn

from levar_para_docx import substituir_capitulo

PASTA = Path(__file__).resolve().parent
DOCX = PASTA.parent / "Gastra II.docx"

KASAVANA = ("KASAVANA, Michael L.; SMITH, Donald I. ", "Menu engineering",
            ": a practical guide to menu analysis. Lansing: Hospitality Publications, 1982.")


def acrescentar_kasavana(documento) -> None:
    referencias = documento.paragraphs
    if any(p.text.startswith("KASAVANA") for p in referencias):
        return
    seguinte = next(p for p in referencias if p.text.startswith("KUBERNS"))
    novo = copy.deepcopy(seguinte._p)
    runs = novo.findall(qn("w:r"))
    molde = next(r for r in runs if r.find(qn("w:rPr")) is None or r.find(qn("w:rPr")).find(qn("w:b")) is None)
    for run in runs:
        novo.remove(run)
    for texto, negrito in zip(KASAVANA, (False, True, False)):
        run = copy.deepcopy(molde)
        for filho in list(run):
            if filho.tag != qn("w:rPr"):
                run.remove(filho)
        rpr = run.find(qn("w:rPr"))
        if rpr is None:
            rpr = run.makeelement(qn("w:rPr"), {})
            run.insert(0, rpr)
        for b in rpr.findall(qn("w:b")):
            rpr.remove(b)
        if negrito:
            rpr.append(rpr.makeelement(qn("w:b"), {}))
        t = run.makeelement(qn("w:t"), {qn("xml:space"): "preserve"})
        t.text = texto
        run.append(t)
        novo.append(run)
    seguinte._p.addprevious(novo)


def main() -> None:
    documento = Document(str(DOCX))
    removidos = substituir_capitulo(documento, "5", PASTA / "Cap5_Desenvolvimento_Resultados_Testes.md")
    acrescentar_kasavana(documento)
    documento.save(str(DOCX))
    print(f"Capítulo 5 inserido ({removidos} elementos antigos substituídos). No Word: Ctrl+A e F9.")


if __name__ == "__main__":
    main()
