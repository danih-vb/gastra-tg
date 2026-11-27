"""Registra no Gastra II o que entrou na fase final (23/11/2026): lançamento pelo Metre ou Gerente com confirmação
do garçom, destaques do dia e metas do mês, e medalhas do garçom.

    pip install python-docx pillow
    python docs/pesquisa/capitulos/registrar_fase_final.py

Acrescenta RF27 a RF29, UC27 a UC29 e US26 a US28, atualiza as contagens, o texto da 5.1, os painéis das
Figuras 18 a 21, a Tabela 2 com a execução de 23/11 e o capítulo 6. Rodar de novo não duplica nada. Depois, no
Word: Ctrl+A e F9.
"""

import copy
from pathlib import Path

from docx import Document
from docx.oxml.ns import qn
from PIL import Image

RAIZ = Path(__file__).resolve().parents[3]
DOCX = RAIZ / "docs" / "pesquisa" / "Gastra II.docx"
PAINEIS = RAIZ / "docs" / "ux-ui" / "telas-reais" / "paineis"
Z = "​"  # quebra opcional nos nomes de teste, como nas outras linhas do quadro

RFS = [
    ["RF27", "Lançar item na comanda pelo Metre ou pelo Gerente, que só entra na conta depois de confirmado pelo "
             "garçom", "Metre e Gerente (lançamento); Garçom (confirmação)", "Decisão da dupla (23/11)"],
    ["RF28", "Definir destaques do dia e metas do mês entre os itens do cardápio, exibidos à equipe com o andamento "
             "de cada meta", "Gerente e Coordenador (definição); Garçom, Metre e Gerente (consulta)",
     "Decisão da dupla (23/11)"],
    ["RF29", "Exibir as medalhas do mês e o histórico de conquistas do garçom, calculados a partir dos indicadores "
             "já registrados", "Garçom (as próprias); Gerente (de qualquer garçom)", "Decisão da dupla (23/11)"],
]


def teste(classe: str, metodo: str) -> str:
    return f"{classe}.{Z}" + f"_{Z}".join(metodo.split("_"))


USS = [
    ["US26 — Como Metre ou Gerente, quero lançar um item na mesa e, como Garçom, confirmar ou recusar o que foi "
     "lançado por eles (RF27)",
     "O item entra a confirmar, fora da conta e da tela do cliente, e impede o fechamento até ser confirmado ou "
     "recusado; a recusa cancela o item com motivo; o Metre não confirma",
     "; ".join([teste("ComandaControllerTests", "LancadoPeloMetre_EsperaOGarcom_ForaDaContaEDoCliente"),
                teste("ComandaControllerTests", "GarcomConfirma_ItemLancadoPeloGerente_EntraNaConta"),
                teste("ComandaControllerTests", "GarcomRecusa_ItemSaiComMotivo"),
                teste("ComandaControllerTests", "Confirmacao_PeloMetre_Retorna403")])],
    ["US27 — Como Gerente ou Coordenador, quero marcar destaques do dia e metas do mês e, como Garçom, vê-los ao "
     "lançar os itens (RF28)",
     "O destaque do dia vale só no dia escolhido; a meta do mês vale o mês inteiro e exige uma quantidade; o "
     "andamento conta as unidades vendidas pela equipe, sem cortesias; o Garçom não cria destaques",
     "; ".join([teste("DestaqueControllerTests", "DestaqueDoDia_PeloGerente_ApareceParaOGarcom"),
                teste("DestaqueControllerTests", "MetaDoMes_ContaAsUnidadesVendidas_SemCortesia"),
                teste("DestaqueControllerTests", "MetaSemQuantidade_Retorna400_EDiaPassado_Retorna400"),
                teste("DestaqueControllerTests", "Criar_PeloGarcom_Retorna403")])],
    ["US28 — Como Garçom, quero ver as minhas medalhas do mês e as conquistas anteriores e, como Gerente, as de "
     "cada garçom (RF29)",
     "Seis medalhas calculadas a partir dos indicadores, com o progresso das que faltam e o histórico de seis "
     "meses; o Garçom só vê as próprias; a consulta é registrada na auditoria",
     "; ".join([teste("IndicadoresControllerTests", "Medalhas_DoGarcom_MostraAsDoMesEOHistorico_ERegistraAConsulta"),
                teste("IndicadoresControllerTests", "Medalhas_OGarcomNaoVeAsDeOutro"),
                teste("IndicadoresControllerTests", "Medalhas_OGerenteVeAsDeQualquerGarcom_EsoDeGarcom"),
                teste("MedalhasTests", "BemAvaliado_ExigeOMinimoDeAvaliacoesEAMedia")])],
]

CASOS_DE_USO = [
    ("UC12", "UC27 Lançar item para confirmação do garçom"),
    ("UC05", "UC28 Definir destaque do dia ou meta do mês"),
    ("UC15", "UC29 Visualizar medalhas e histórico de conquistas"),
]

TEXTOS = [
    # Contagens
    ("Foram especificados 26 requisitos funcionais, dos quais 25 foram aprovados",
     "Foram especificados 29 requisitos funcionais, dos quais 28 foram aprovados"),
    ("Os requisitos funcionais foram detalhados em 26 casos de uso", "Os requisitos funcionais foram detalhados em 29 casos de uso"),
    ("Os requisitos também foram escritos como 24 histórias de usuário",
     "Os requisitos também foram escritos como 28 histórias de usuário"),
    ("sempre com motivo registrado. Pelo mesmo motivo do RF12",
     "sempre com motivo registrado. Da mesma fase são o UC27, para que o Metre e o Gerente lancem itens que o garçom "
     "confirma, o UC28, dos destaques do dia e das metas do mês, e o UC29, das medalhas do garçom. Pelo mesmo motivo "
     "do RF12"),
    ("formalizado em 25 requisitos funcionais aprovados", "formalizado em 28 requisitos funcionais aprovados"),
    ("mais de 500 casos de teste", "mais de 600 casos de teste"),
    ("more than 500 automated", "more than 600 automated"),
    ("validado por 576 casos de teste", "validado por 615 casos de teste"),
    # 5.1 — Garçom
    ("A tela inicial mostra as mesas do salão",
     "A tela inicial mostra os destaques do dia e as metas do mês definidos pelo Gerente, com o andamento de cada "
     "meta, e as mesas do salão"),
    ("e lança cada item em dois toques.",
     "e lança cada item em dois toques; na lista, os destaques e as metas vêm primeiro, em cor."),
    ("marca como atrasado o que passou de 15 minutos (Figura 19).",
     "marca como atrasado o que passou de 15 minutos (Figura 19). Os itens lançados pelo Metre ou pelo Gerente "
     "aparecem na comanda em Para confirmar, sem entrar na conta, até que o garçom os confirme ou recuse (RF27)."),
    ("sem os valores dos colegas.",
     "sem os valores dos colegas, e as medalhas do mês, com quanto falta para cada uma e as conquistas dos meses "
     "anteriores (RF29)."),
    # 5.1 — Metre
    ("e pode lançar uma cortesia na mesa, por R$ 0,00 e com motivo (RF26).",
     "e pode lançar na mesa um item, que espera a confirmação do garçom (RF27), ou uma cortesia, por R$ 0,00 e com "
     "motivo (RF26), escolhendo o item na mesma lista usada pelo garçom."),
    # 5.1 — Gerente
    ("O Gerente também gerencia o cardápio, com fotos e promoções, as praças, as mesas e as contas de usuário; "
     "o Coordenador tem acesso somente ao cardápio e às promoções.",
     "O Gerente também gerencia o cardápio, com fotos, promoções, destaques do dia e metas do mês (RF28), as praças, "
     "as mesas e as contas de usuário, com a foto de cada funcionário e, no perfil de cada garçom, as medalhas e o "
     "histórico de conquistas; o Coordenador tem acesso somente ao cardápio, às promoções e aos destaques."),
    # Legendas
    ("Garçom: do salão ao lançamento do item", "Garçom: dos destaques do dia ao lançamento do item"),
    ("Garçom: busca, itens a entregar, fechamento e desempenho",
     "Garçom: busca, itens para confirmar e a entregar, fechamento, desempenho e medalhas"),
    ("Metre: alocação do turno, salão e cortesia", "Metre: alocação do turno, salão, lançamento e cortesia"),
    ("Gerente: análises, cardápio, ajuste de preço e usuários",
     "Gerente: análises, cardápio, destaques, ajuste de preço, usuários e medalhas"),
    # 5.2.2
    ("a partir da execução de 21/11/2026", "a partir da execução de 23/11/2026"),
    ("Os 576 casos foram aprovados", "Os 615 casos foram aprovados"),
    ("são 361 casos no backend, 68 na camada analítica e 147 no frontend",
     "são 394 casos no backend, 68 na camada analítica e 153 no frontend"),
    # 6.3
    ("ampliar a tela de desempenho do garçom com a evolução do índice ao longo das semanas e com metas do turno, "
     "sempre comparando",
     "ampliar a tela de desempenho do garçom com a evolução do índice ao longo das semanas e com metas individuais, "
     "além das medalhas e das metas da equipe já implementadas, sempre comparando"),
]

LIMITACAO = (
    "• As medalhas, os destaques e as metas do mês entraram na fase final e também não têm dado de uso; os alvos "
    "das medalhas, como 100 mesas ou 20 turnos no mês, foram definidos pela dupla e devem ser revistos com a "
    "operação real."
)

TABELA_2 = {
    "Núcleo de comandas": ("151", "160"),
    "Cardápio e promoções": ("75", "98"),
    "Alocação de garçons (programação linear)": ("61", "67"),
    "Recomendação de pratos (ciência de dados)": ("54", "60"),
    "BI e índice de desempenho": ("63", "69"),
    "LGPD, auditoria e segurança": ("114", "115"),
    "Banco de dados (views, triggers e restrições)": ("14", "14"),
    "Integração backend e serviço analítico": ("12", "15"),
    "Arquitetura e infraestrutura": ("13", "17"),
    "Total": ("557", "615"),
}

# Legenda da figura -> painel que vai no lugar da imagem
FIGURAS = {
    "Garçom: dos destaques do dia ao lançamento do item": "garcom-1",
    "Garçom: busca, itens para confirmar e a entregar": "garcom-2",
    "Metre: alocação do turno, salão, lançamento e cortesia": "metre",
    "Gerente: análises, cardápio, destaques, ajuste de preço": "gerente",
}
ALTURA_MAXIMA = 7_920_000  # 22 cm em EMU: cabe na página com a legenda e a fonte


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


def juntar_runs(paragrafo) -> None:
    """Junta os runs de um parágrafo de formatação única, para trocar um trecho que o Word partiu em vários."""
    runs = paragrafo.runs
    if len(runs) < 2 or paragrafo._p.xpath(".//w:fldChar|.//w:fldSimple|.//w:hyperlink|.//w:drawing"):
        return
    formatos = {str(r._r.rPr.xml) if r._r.rPr is not None else "" for r in runs}
    if len(formatos) > 1:
        return
    runs[0].text = "".join(r.text for r in runs)
    for extra in runs[1:]:
        extra._r.getparent().remove(extra._r)


def trocar_texto(paragrafo, atual: str, novo: str) -> bool:
    if novo in paragrafo.text or atual not in paragrafo.text:
        return False
    if paragrafo.style.name.lower().startswith(("table of figures", "toc")):
        return False  # índices: o Word refaz com F9
    if not any(atual in r.text for r in paragrafo.runs):
        juntar_runs(paragrafo)
    for run in paragrafo.runs:
        if atual in run.text:
            run.text = run.text.replace(atual, novo)
            return True
    raise SystemExit(f"Trecho dividido entre formatações, trocar à mão: {atual[:50]}")


def trocar_figuras(documento) -> int:
    trocadas = 0
    paragrafos = documento.paragraphs
    for i, paragrafo in enumerate(paragrafos):
        if paragrafo.style.name != "Caption":
            continue
        painel = next((p for legenda, p in FIGURAS.items() if legenda in paragrafo.text), None)
        if not painel:
            continue
        figura = next(p for p in paragrafos[i + 1:i + 3] if p._p.xpath(".//a:blip"))
        blip = figura._p.xpath(".//a:blip")[0]
        parte = documento.part.related_parts[blip.get(qn("r:embed"))]
        arquivo = PAINEIS / f"{painel}.png"
        parte._blob = arquivo.read_bytes()
        largura_px, altura_px = Image.open(arquivo).size
        extensao = figura._p.xpath(".//wp:extent")[0]
        cx = int(extensao.get("cx"))
        cy = round(cx * altura_px / largura_px)
        if cy > ALTURA_MAXIMA:
            cx, cy = round(cx * ALTURA_MAXIMA / cy), ALTURA_MAXIMA
        for elemento in figura._p.xpath(".//wp:extent|.//a:xfrm/a:ext"):
            elemento.set("cx", str(cx))
            elemento.set("cy", str(cy))
        trocadas += 1
    return trocadas


def main() -> None:
    documento = Document(str(DOCX))
    quadro_rf = next(t for t in documento.tables if t.rows[0].cells[0].text == "ID"
                     and any(r.cells[0].text == "RF25" for r in t.rows))
    quadro_uc = next(t for t in documento.tables if t.rows[0].cells[0].text == "Grupo")
    quadro_us = next(t for t in documento.tables if t.rows[0].cells[0].text == "História")
    tabela_2 = next(t for t in documento.tables if t.rows[0].cells[0].text == "Bloco")

    feitos = [acrescentar_linha(quadro_rf, rf) for rf in RFS] + [acrescentar_linha(quadro_us, us) for us in USS]

    for vizinho, caso in CASOS_DE_USO:
        linha = next(r for r in quadro_uc.rows if vizinho in r.cells[1].text)
        if caso[:4] not in linha.cells[1].text:
            escrever_na_celula(linha.cells[1], linha.cells[1].text + "; " + caso)
            feitos.append(True)

    for linha in tabela_2.rows[1:]:
        cenarios, casos = TABELA_2[linha.cells[0].text]
        if linha.cells[2].text != casos:
            escrever_na_celula(linha.cells[1], cenarios)
            escrever_na_celula(linha.cells[2], casos)
            escrever_na_celula(linha.cells[3], casos)
            feitos.append(True)

    for atual, novo in TEXTOS:
        feitos.append(any(trocar_texto(p, atual, novo) for p in documento.paragraphs))

    if not any(LIMITACAO[:40] in p.text for p in documento.paragraphs):
        avaliacao = next(p for p in documento.paragraphs if p.text.startswith("• A avaliação do atendimento pelo cliente"))
        nova = copy.deepcopy(avaliacao._p)
        avaliacao._p.addnext(nova)
        paragrafo = next(p for p in documento.paragraphs if p._p is nova)
        juntar_runs(paragrafo)
        paragrafo.runs[0].text = LIMITACAO
        feitos.append(True)

    feitos.append(trocar_figuras(documento) == len(FIGURAS))

    documento.save(str(DOCX))
    print(f"{sum(feitos)} alterações. No Word: Ctrl+A e F9.")


if __name__ == "__main__":
    main()
