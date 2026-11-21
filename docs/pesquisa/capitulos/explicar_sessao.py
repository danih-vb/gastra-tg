"""Acrescenta, depois do parágrafo da autenticação (JWT), como a sessão e o acesso do cliente funcionam por dentro.

Pedido do orientador na reunião de 20/11/2026: ele esperava que, trocando o endereço a partir da conta do cliente
ou colando o endereço do garçom em outra aba, desse para entrar na conta do garçom, e quis entender por que não dá.

    python docs/pesquisa/capitulos/explicar_sessao.py

Rodar de novo não duplica o parágrafo.
"""

import copy
from pathlib import Path

from docx import Document
from docx.oxml.ns import qn

DOCX = Path(__file__).resolve().parent.parent / "Gastra II.docx"

TEXTO = (
    "No navegador, a sessão fica guardada no armazenamento da própria aba (sessionStorage), e não no armazenamento "
    "compartilhado entre as abas (WHATWG, 2026). Uma aba nova, mesmo aberta com o endereço de uma tela do garçom, "
    "não tem o token e é levada ao login por uma guarda de rota do Angular, que também confere se o papel do usuário "
    "pode abrir aquela tela. A guarda organiza a navegação, mas a proteção está no servidor: toda chamada à interface "
    "de programação leva o token, e o backend recusa a que chegar sem ele, com ele vencido ou com um papel sem "
    "permissão para a operação. O cliente, que não faz login, chega pelo QR code a um endereço com o código de acesso "
    "da comanda, um identificador aleatório de 128 bits, e esse código só libera as operações públicas daquela conta: "
    "consultar os itens, chamar o garçom e avaliar o atendimento, além do cardápio digital. Trocar o endereço a partir "
    "dessa página para uma tela do garçom leva ao login, como em qualquer aba sem sessão. Guardar a sessão por aba "
    "também reduz o tempo em que o token fica disponível em um computador compartilhado do salão, já que fechar a aba "
    "encerra o acesso."
)


def main() -> None:
    documento = Document(str(DOCX))
    if any(p.text.startswith(TEXTO[:60]) for p in documento.paragraphs):
        print("O parágrafo já está no documento.")
        return
    jwt = next(p for p in documento.paragraphs if p.text.startswith("A autenticação utiliza o JSON Web Token"))
    novo = copy.deepcopy(jwt._p)
    runs = novo.findall(qn("w:r"))
    for run in runs[1:]:
        novo.remove(run)
    for filho in list(runs[0]):
        if filho.tag not in (qn("w:rPr"),):
            runs[0].remove(filho)
    t = runs[0].makeelement(qn("w:t"), {qn("xml:space"): "preserve"})
    t.text = TEXTO
    runs[0].append(t)
    for outro in novo:
        if outro.tag not in (qn("w:pPr"), qn("w:r")):
            novo.remove(outro)
    jwt._p.addnext(novo)
    documento.save(str(DOCX))
    print("Parágrafo da sessão acrescentado depois do da autenticação.")


if __name__ == "__main__":
    main()
