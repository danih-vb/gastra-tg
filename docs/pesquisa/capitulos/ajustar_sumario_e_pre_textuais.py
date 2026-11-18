"""Ajustes finos depois de atualizar_pretextuais_e_prototipo.py, vistos no PDF gerado pelo Word.

- Dedicatória e epígrafe no pé da página: o Word ignora o espaço antes no topo de página nova, então o espaço vem de
  parágrafos vazios.
- O sumário não lista os elementos pré-textuais (ABNT NBR 14724): os títulos das listas passam a um estilo igual ao
  Título 1, mas fora dos níveis do sumário.
- O título do sumário em maiúsculas e centralizado, como os demais títulos sem número.
"""

from pathlib import Path

from docx import Document
from docx.enum.style import WD_STYLE_TYPE
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.shared import Pt

DOCX = Path(__file__).resolve().parent.parent / "Gastra II.docx"
LINHAS_ATE_O_PE = 18
LISTAS = ("LISTA DE ILUSTRAÇÕES", "LISTA DE QUADROS", "LISTA DE TABELAS", "LISTA DE ABREVIATURAS E SIGLAS")


def main() -> None:
    documento = Document(str(DOCX))

    for inicio in ("Às nossas famílias.", "“A melhor maneira de prever o futuro"):
        paragrafo = next(p for p in documento.paragraphs if p.text.startswith(inicio))
        paragrafo.paragraph_format.space_before = Pt(0)
        for _ in range(LINHAS_ATE_O_PE):
            paragrafo._p.addprevious(documento.add_paragraph()._p)

    if "Título pré-textual" not in [s.name for s in documento.styles]:
        estilo = documento.styles.add_style("Título pré-textual", WD_STYLE_TYPE.PARAGRAPH)
        estilo.base_style = documento.styles["Heading 1"]
        nivel = estilo.element.get_or_add_pPr().makeelement(qn("w:outlineLvl"), {qn("w:val"): "9"})
        estilo.element.get_or_add_pPr().append(nivel)
    for paragrafo in documento.paragraphs:
        if paragrafo.text.strip() in LISTAS and paragrafo.style.name == "Heading 1":
            paragrafo.style = documento.styles["Título pré-textual"]

    for titulo in documento.element.body.iter(qn("w:p")):
        estilos = titulo.xpath("./w:pPr/w:pStyle/@w:val")
        if estilos and estilos[0] in ("TOCHeading", "CabealhodoSumrio") and "".join(titulo.xpath(".//w:t/text()")).strip() == "Sumário":
            textos = titulo.xpath(".//w:t")
            textos[0].text = "SUMÁRIO"
            for t in textos[1:]:
                t.text = ""
            jc = titulo.get_or_add_pPr().makeelement(qn("w:jc"), {qn("w:val"): "center"})
            titulo.get_or_add_pPr().append(jc)
            for run in titulo.xpath(".//w:r"):
                rpr = run.find(qn("w:rPr"))
                if rpr is None:
                    rpr = run.makeelement(qn("w:rPr"), {})
                    run.insert(0, rpr)
                rpr.append(rpr.makeelement(qn("w:b"), {}))
    documento.save(str(DOCX))
    print("ok")


if __name__ == "__main__":
    main()
