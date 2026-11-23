"""Leva o capítulo 6 (Markdown) para o Gastra II.docx.

    pip install python-docx
    python docs/pesquisa/capitulos/inserir_capitulo_6.py

Depois, no Word: Ctrl+A e F9, para atualizar o sumário.
"""

from pathlib import Path

from docx import Document

from levar_para_docx import substituir_capitulo

PASTA = Path(__file__).resolve().parent
DOCX = PASTA.parent / "Gastra II.docx"


def main() -> None:
    documento = Document(str(DOCX))
    removidos = substituir_capitulo(documento, "6", PASTA / "Cap6_Consideracoes_Finais.md")
    documento.save(str(DOCX))
    print(f"Capítulo 6 inserido ({removidos} elementos antigos substituídos). No Word: Ctrl+A e F9.")


if __name__ == "__main__":
    main()
