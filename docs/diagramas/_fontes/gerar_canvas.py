"""Gera o GASTRA_Business_Model_Canvas.drawio com os nove blocos em tópicos curtos e letra grande.

    python docs/diagramas/_fontes/gerar_canvas.py

A versão anterior (modelo da Strategyzer com parágrafos inteiros em cada bloco) ficava ilegível ao entrar no
documento na largura da página. O texto completo de cada bloco está na seção 3.1 do Gastra II; aqui fica só o
essencial. Depois, exportar o PNG com o draw.io (ver README.md desta pasta).
"""

from pathlib import Path
from xml.sax.saxutils import escape

PASTA = Path(__file__).resolve().parent
SAIDA = PASTA / "GASTRA_Business_Model_Canvas.drawio"

LARGURA = 1200
TOPO, ALTURA_CIMA, ALTURA_BAIXO = 70, 560, 230
COLUNA = LARGURA / 5

BLOCOS = [
    # (título, tópicos, x, y, largura, altura, cor de fundo, cor da borda)
    ("Parcerias principais", ["Restaurantes piloto", "Provedores de nuvem", "Abrasel e associações do setor",
                              "Fornecedores de maquininhas (POS)"], 0, 0, 1, 1, "#EAF2FD", "#6C9EEB"),
    ("Atividades principais", ["Desenvolver e manter o sistema", "Calibrar os modelos com dado real",
                               "Implantar em cada restaurante"], 1, 0, 1, 0.5, "#EAF2FD", "#6C9EEB"),
    ("Recursos principais", ["Equipe .NET, Angular e Python", "Dados da operação do restaurante",
                             "Infraestrutura em nuvem"], 1, 0.5, 1, 0.5, "#EAF2FD", "#6C9EEB"),
    ("Proposta de valor", ["Praças distribuídas com critério (programação linear)", "Sugestão de prato na hora certa",
                           "Índice de desempenho composto", "Relatórios para decidir com dado",
                           "Cliente nunca identificado (LGPD)"], 2, 0, 1, 1, "#FDECEC", "#E06666"),
    ("Relacionamento", ["Acompanhamento próximo na implantação", "Suporte contínuo",
                        "Relatórios de resultado"], 3, 0, 1, 0.5, "#FFF8D6", "#E0B400"),
    ("Canais", ["Venda consultiva ao gestor", "Piloto gratuito ou com desconto", "Abrasel e eventos do setor"],
     3, 0.5, 1, 0.5, "#FFF8D6", "#E0B400"),
    ("Segmentos de clientes", ["Restaurantes à la carte de médio porte", "Atendimento por garçom, em praças",
                               "Decisões hoje no costume e no “feeling”"], 4, 0, 1, 1, "#FFF8D6", "#E0B400"),
]
BASE = [
    ("Estrutura de custos", ["Equipe técnica", "Hospedagem e processamento em nuvem",
                             "Suporte à implantação e ao piloto", "Conformidade com a LGPD"], 0, 2.5, "#EAF2FD", "#6C9EEB"),
    ("Fontes de receita", ["Assinatura mensal (SaaS) pelo porte do restaurante", "Piloto que vira assinatura",
                           "Suporte e customização"], 2.5, 2.5, "#FFF8D6", "#E0B400"),
]


def celula(id_, valor, estilo, x, y, w, h):
    return (f'<mxCell id="{id_}" value="{escape(valor, {chr(34): "&quot;"})}" style="{estilo}" vertex="1" parent="1">'
            f'<mxGeometry x="{x:.0f}" y="{y:.0f}" width="{w:.0f}" height="{h:.0f}" as="geometry"/></mxCell>')


def bloco(i, titulo, topicos, x, y, w, h, fundo, borda):
    itens = "".join(f"<li>{escape(t)}</li>" for t in topicos)
    html = (f'<div style="font-size:21px;font-weight:bold;color:#1C2738;margin-bottom:8px">{escape(titulo)}</div>'
            f'<ul style="margin:0;padding-left:20px;font-size:19px;line-height:1.3;color:#243042">{itens}</ul>')
    estilo = (f"rounded=1;arcSize=3;whiteSpace=wrap;html=1;align=left;verticalAlign=top;spacing=12;spacingTop=10;"
              f"fillColor={fundo};strokeColor={borda};strokeWidth=2;fontFamily=Arial;")
    return celula(f"bloco{i}", html, estilo, x, y, w, h)


def main() -> None:
    celulas = [celula("titulo", "GASTRA – Business Model Canvas",
                      "text;html=1;align=left;verticalAlign=middle;fontSize=26;fontStyle=1;fontFamily=Arial;fontColor=#1C2738;",
                      0, 10, LARGURA, 44)]
    margem = 6
    for i, (titulo, topicos, coluna, linha, largura, altura, fundo, borda) in enumerate(BLOCOS):
        celulas.append(bloco(i, titulo, topicos, coluna * COLUNA + margem, TOPO + linha * ALTURA_CIMA + margem,
                             largura * COLUNA - 2 * margem, altura * ALTURA_CIMA - 2 * margem, fundo, borda))
    for j, (titulo, topicos, coluna, largura, fundo, borda) in enumerate(BASE):
        celulas.append(bloco(len(BLOCOS) + j, titulo, topicos, coluna * COLUNA + margem, TOPO + ALTURA_CIMA + margem,
                             largura * COLUNA - 2 * margem, ALTURA_BAIXO - 2 * margem, fundo, borda))
    xml = ('<mxfile host="drawio"><diagram name="GASTRA_Business_Model_Canvas" id="canvas">'
           '<mxGraphModel grid="0" page="0" math="0" shadow="0"><root><mxCell id="0"/><mxCell id="1" parent="0"/>'
           + "".join(celulas) + "</root></mxGraphModel></diagram></mxfile>")
    SAIDA.write_text(xml, encoding="utf-8")
    print(SAIDA.name)


if __name__ == "__main__":
    main()
