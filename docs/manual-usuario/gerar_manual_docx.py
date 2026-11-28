"""Gera o GASTRA_Manual_do_Usuario.docx a partir do GASTRA_Manual_do_Usuario.md, com a formatação do Gastra II
(o mesmo texto do Apêndice A, com a numeração própria do manual).

    pip install python-docx pillow
    python docs/manual-usuario/gerar_manual_docx.py

Depois, no Word: Ctrl+A e F9 (numeração das figuras), e salvar.
"""

from __future__ import annotations

import re
import sys
import tempfile
from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn

PASTA = Path(__file__).resolve().parent
sys.path.insert(0, str(PASTA.parents[0] / "pesquisa" / "capitulos"))
from inserir_apendice_a import com_legendas  # noqa: E402
from levar_para_docx import blocos, escrever, estilo, sem_recuo  # noqa: E402

MODELO = PASTA.parents[0] / "pesquisa" / "Gastra II.docx"
MANUAL = PASTA / "GASTRA_Manual_do_Usuario.md"
SAIDA = PASTA / "GASTRA_Manual_do_Usuario.docx"


def manual_em_markdown() -> str:
    """O manual sem o título e o sumário próprios (o Word monta o dele), sem links e citações do GitHub."""
    texto = MANUAL.read_text(encoding="utf-8")
    texto = re.sub(r"(?ms)^## Sumário.*?(?=^## )", "", texto)
    texto = re.sub(r"(?m)^# .*$", "", texto)
    texto = re.sub(r"\[([^\]]+)\]\((?:https?://|\.\./|#)[^)]*\)", r"\1", texto)
    texto = re.sub(r"(?m)^> ?", "", texto)
    texto = re.sub(r"(?m)^---\s*$", "", texto)
    return texto


def main() -> None:
    documento = Document(str(MODELO))
    corpo = documento.element.body
    # Usa o Gastra II só pelos estilos e pela página: o corpo sai inteiro, menos a configuração de seção.
    for elemento in list(corpo.iterchildren()):
        if elemento.tag != qn("w:sectPr"):
            corpo.remove(elemento)

    titulo = documento.add_paragraph()
    estilo(titulo, "Ttulo1")
    sem_recuo(titulo, WD_ALIGN_PARAGRAPH.CENTER)
    titulo.add_run("GASTRA – MANUAL DO USUÁRIO")

    with tempfile.TemporaryDirectory() as temporaria:
        markdown = com_legendas(manual_em_markdown(), 1, Path(temporaria))
        escrever(documento, titulo._p, blocos(markdown), MANUAL.parent)
        # As imagens do Gastra II que ficaram sem uso sairiam junto no arquivo: tira as ligações delas.
        usadas = set(corpo.xpath(".//@r:embed"))
        for rid, ligacao in list(documento.part.rels.items()):
            if ligacao.reltype.endswith("/image") and rid not in usadas:
                documento.part.drop_rel(rid)
        documento.save(str(SAIDA))
    print(f"{SAIDA.name} gerado. No Word: Ctrl+A e F9.")


if __name__ == "__main__":
    main()
