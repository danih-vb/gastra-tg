"""Leva para os documentos de requisitos o que entrou na fase final (21 a 26/11/2026): RF26 a RF30, UC26 a UC30 e
US25 a US29. O texto é o mesmo dos quadros do Gastra II, que é a fonte desde a fase final.

    pip install python-docx
    python docs/requisitos/registrar_fase_final.py

Rodar de novo não duplica nada.
"""

import copy
import sys
from pathlib import Path

from docx import Document

PASTA = Path(__file__).resolve().parent
sys.path.insert(0, str(PASTA.parent / "pesquisa" / "capitulos"))
from registrar_fase_final import escrever_na_celula  # noqa: E402

RF = [
    ("RF26", "lançar item de cortesia na comanda e ajustar o preço de um item já lançado, sempre com motivo",
     "Metre e Gerente (cortesia); Gerente (ajuste)", "Decisão da dupla (21/11)", "US25; UC26; ENT06"),
    ("RF27", "lançar item na comanda pelo Metre ou pelo Gerente, que só entra na conta depois de confirmado pelo garçom",
     "Metre e Gerente (lançamento); Garçom (confirmação)", "Decisão da dupla (23/11)", "US26; UC27; ENT06"),
    ("RF28", "definir destaques do dia e metas do mês entre os itens do cardápio, exibidos à equipe com o andamento de "
             "cada meta",
     "Gerente e Coordenador (definição); Garçom, Metre e Gerente (consulta)", "Decisão da dupla (23/11)",
     "US27; UC28; ENT12; REL12"),
    ("RF29", "exibir as medalhas do mês e o histórico de conquistas do garçom, calculados a partir dos indicadores já "
             "registrados, e as menções honrosas dadas pelo Gerente",
     "Garçom (as próprias); Gerente (de qualquer garçom, e dá as menções)", "Decisão da dupla (23/11)",
     "US28; UC29; ENT13; REL13; REL14"),
    ("RF30", "gerar os QR codes fixos das mesas, prontos para imprimir, que abrem o cardápio digital da mesa",
     "Gerente", "Decisão da dupla (26/11)", "US29; UC30; ENT01"),
]

US = [
    ("US25", "Como Metre ou Gerente, quero oferecer uma cortesia à mesa e, como Gerente, ajustar o preço de um item "
             "lançado", "RF26", "US23"),
    ("US26", "Como Metre ou Gerente, quero lançar um item na mesa e, como Garçom, confirmar ou recusar o que foi "
             "lançado por eles", "RF27", "US23"),
    ("US27", "Como Gerente ou Coordenador, quero marcar destaques do dia e metas do mês e, como Garçom, vê-los ao "
             "lançar os itens", "RF28", "US21"),
    ("US28", "Como Garçom, quero ver as minhas medalhas do mês e as conquistas anteriores e, como Gerente, as de cada "
             "garçom, e dar menções honrosas", "RF29", "US06"),
    ("US29", "Como Gerente, quero imprimir um QR code para cada mesa, para o cliente abrir o cardápio pelo celular",
     "RF30", "US22"),
]

UC = [
    ("UC26", "Lançar Cortesia e Ajustar Preço", "Metre, Gerente", "UC23"),
    ("UC27", "Lançar Item para Confirmação do Garçom", "Metre, Gerente, Garçom", "UC23"),
    ("UC28", "Definir Destaque do Dia ou Meta do Mês", "Gerente, Coordenador", "UC09"),
    ("UC29", "Visualizar Medalhas e Histórico de Conquistas", "Garçom, Gerente", "UC22"),
    ("UC30", "Gerar QR Codes das Mesas", "Gerente", "UC24"),
]


def linha_que_comeca(documento, prefixo):
    for tabela in documento.tables:
        for linha in tabela.rows:
            if linha.cells[0].text.strip().startswith(prefixo):
                return tabela, linha
    return None, None


def inserir_depois(referencia, valores) -> None:
    nova = copy.deepcopy(referencia._tr)
    referencia._tr.addnext(nova)
    from docx.table import _Row
    linha = _Row(nova, referencia._parent)
    for celula, valor in zip(linha.cells, valores):
        escrever_na_celula(celula, valor)


def em_sequencia(documento, itens, ancora_de):
    """Cada item entra logo depois da âncora (ou do item anterior do mesmo grupo), se ainda não existir."""
    feitos = 0
    for item in itens:
        if linha_que_comeca(documento, item[0])[1] is not None:
            continue
        anterior = f"{item[0][:2]}{int(item[0][2:]) - 1:02d}"
        _, referencia = linha_que_comeca(documento, anterior)
        _, ancora = linha_que_comeca(documento, ancora_de(item))
        alvo = referencia if referencia is not None and _mesma_tabela(documento, referencia, ancora) else ancora
        inserir_depois(alvo, valores_de(item))
        feitos += 1
    return feitos


def _mesma_tabela(documento, a, b):
    return a._tr.getparent() is b._tr.getparent()


def valores_de(item):
    return VALORES[item[0][:2]](item)


VALORES = {
    "US": lambda i: [i[0], i[1], i[2]],
    "UC": lambda i: [i[0], i[1], i[2]],
}


def main() -> None:
    feitos = 0

    requisitos = Document(str(PASTA / "GASTRA_Requisitos_RN.docx"))
    _, rf25 = linha_que_comeca(requisitos, "RF25")
    referencia = rf25
    for codigo, descricao, atores, _, _ in RF:
        _, existente = linha_que_comeca(requisitos, codigo)
        if existente is None:
            inserir_depois(referencia, [codigo, f"O sistema deve permitir {descricao}.", atores])
            _, existente = linha_que_comeca(requisitos, codigo)
            feitos += 1
        referencia = existente
    requisitos.save(str(PASTA / "GASTRA_Requisitos_RN.docx"))

    matriz = Document(str(PASTA / "GASTRA_Matriz_Rastreabilidade.docx"))
    _, referencia = linha_que_comeca(matriz, "RF25")
    for codigo, descricao, _, fonte, artefatos in RF:
        _, existente = linha_que_comeca(matriz, codigo)
        if existente is None:
            inserir_depois(referencia, [codigo, f"O sistema deve permitir {descricao}.", fonte, artefatos,
                                        "Decisão interna + teste automatizado", "Aprovado"])
            _, existente = linha_que_comeca(matriz, codigo)
            feitos += 1
        referencia = existente
    matriz.save(str(PASTA / "GASTRA_Matriz_Rastreabilidade.docx"))

    for arquivo, itens in (("GASTRA_User_Stories.docx", US), ("GASTRA_Casos_de_Uso.docx", UC)):
        documento = Document(str(PASTA / arquivo))
        feitos += em_sequencia(documento, itens, lambda item: item[-1])
        documento.save(str(PASTA / arquivo))

    print(f"{feitos} linhas novas nos documentos de requisitos.")


if __name__ == "__main__":
    main()
