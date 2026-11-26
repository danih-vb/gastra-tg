"""Coloca o manual do usuário no Gastra II como APÊNDICE A, depois das referências.

    pip install python-docx pillow
    python docs/pesquisa/capitulos/inserir_apendice_a.py

O texto vem de `docs/manual-usuario/GASTRA_Manual_do_Usuario.md`: seções renumeradas como A.1, A.2…, cada imagem com
legenda e fonte. Onde existe captura do sistema em execução (`docs/ux-ui/telas-reais/`), ela entra no lugar da tela do
protótipo. Rodar de novo apaga o apêndice anterior e refaz, sem duplicar. Depois, no Word: Ctrl+A e F9.
"""

from __future__ import annotations

import re
import tempfile
from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.oxml.ns import qn
from docx.shared import Cm
from PIL import Image

from levar_para_docx import blocos, escrever, estilo, sem_recuo

PASTA = Path(__file__).resolve().parent
DOCX = PASTA.parent / "Gastra II.docx"
MANUAL = PASTA.parents[1] / "manual-usuario" / "GASTRA_Manual_do_Usuario.md"
TELAS_REAIS = PASTA.parents[1] / "ux-ui" / "telas-reais"
TITULO = "APÊNDICE A – MANUAL DO USUÁRIO"

# Captura de página inteira vira a primeira dobra: 932 px no celular e 900 no computador, na escala 2 das capturas.
DOBRA_CELULAR, DOBRA_COMPUTADOR = 1864, 1800


def manual_em_markdown() -> str:
    """O manual sem o título e o sumário próprios, com as seções numeradas A.1, A.1.1…"""
    texto = MANUAL.read_text(encoding="utf-8")
    texto = re.sub(r"(?ms)^## Sumário.*?(?=^## )", "", texto)
    texto = re.sub(r"(?m)^# .*$", "", texto)
    texto = re.sub(r"(?m)^## (\d+)\. ", r"## A.\1 ", texto)
    texto = re.sub(r"(?m)^### (\d+)\.(\d+) ", r"### A.\1.\2 ", texto)
    texto = re.sub(r"seção (\d+)\.(\d+)", r"seção A.\1.\2", texto)
    texto = re.sub(r"seções (\d+)\.(\d+) e (\d+)\.(\d+)", r"seções A.\1.\2 e A.\3.\4", texto)
    texto = re.sub(r"\[([^\]]+)\]\((?:https?://|\.\./|#)[^)]*\)", r"\1", texto)
    texto = re.sub(r"(?m)^> ?", "", texto)
    texto = re.sub(r"(?m)^---\s*$", "", texto)
    return texto


def imagem_para_o_apendice(caminho: str, pasta_temporaria: Path) -> Path:
    """A captura real, se houver, cortada na primeira dobra; senão, a tela do protótipo."""
    original = (MANUAL.parent / caminho).resolve()
    real = TELAS_REAIS / original.name
    if not real.exists():
        return original
    imagem = Image.open(real)
    dobra = DOBRA_CELULAR if imagem.width < 1000 else DOBRA_COMPUTADOR
    if imagem.height <= dobra:
        return real
    cortada = pasta_temporaria / real.name
    imagem.crop((0, 0, imagem.width, dobra)).save(cortada)
    return cortada


def com_legendas(texto: str, primeira: int, pasta_temporaria: Path) -> str:
    numero = primeira

    def troca(m):
        nonlocal numero
        caminho, alt = m.group(1), m.group(2)
        arquivo = imagem_para_o_apendice(caminho, pasta_temporaria)
        bloco = f"\n**Figura {numero} – {alt}**\n\n![{alt}]({arquivo.as_posix()})\n\nFonte: elaborado pelos autores.\n"
        numero += 1
        return bloco

    return re.sub(r'<img src="([^"]+)" alt="([^"]*)"[^>]*>', troca, texto)


def texto(elemento) -> str:
    return "".join(elemento.xpath(".//w:t/text()")).strip()


def eh_titulo_1(elemento) -> bool:
    return elemento.xpath("string(./w:pPr/w:pStyle/@w:val)") == "Ttulo1"


def main() -> None:
    documento = Document(str(DOCX))
    corpo = documento.element.body

    # Rodar de novo: apaga o apêndice anterior (do título até o fim do corpo).
    anterior = next((p for p in corpo.iterchildren(qn("w:p")) if eh_titulo_1(p) and texto(p) == TITULO), None)
    if anterior is not None:
        quebra = anterior.getprevious()
        elemento = anterior
        while elemento is not None and elemento.tag != qn("w:sectPr"):
            seguinte = elemento.getnext()
            corpo.remove(elemento)
            elemento = seguinte
        if quebra is not None and quebra.xpath(".//w:br[@w:type='page']") and not texto(quebra):
            corpo.remove(quebra)

    figuras = sum(1 for p in documento.paragraphs
                  if p.style.name == "Caption" and p.text.startswith("Figura"))

    ultimo = [e for e in corpo.iterchildren() if e.tag != qn("w:sectPr")][-1]

    quebra = documento.add_paragraph()
    quebra.add_run().add_break(WD_BREAK.PAGE)
    ultimo.addnext(quebra._p)

    titulo = documento.add_paragraph()
    estilo(titulo, "Ttulo1")
    sem_recuo(titulo, WD_ALIGN_PARAGRAPH.CENTER)
    titulo.add_run(TITULO)
    quebra._p.addnext(titulo._p)

    with tempfile.TemporaryDirectory() as temporaria:
        markdown = com_legendas(manual_em_markdown(), figuras + 1, Path(temporaria))
        escrever(documento, titulo._p, blocos(markdown), MANUAL.parent)
        documento.save(str(DOCX))
    print(f"Apêndice A inserido; figuras a partir da {figuras + 1}. No Word: Ctrl+A e F9.")


if __name__ == "__main__":
    main()
