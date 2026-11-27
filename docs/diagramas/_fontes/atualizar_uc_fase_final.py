"""Acrescenta aos diagramas de casos de uso os UC26 a UC30, da fase final (21 a 26/11/2026).

    python docs/diagramas/_fontes/atualizar_uc_fase_final.py

Edita os .drawio desta pasta: cria as elipses, os atores e as associações novas, e move o que precisa abrir espaço.
Rodar de novo não duplica nada. Depois, exportar os PNGs com o draw.io (ver README.md).
"""

from __future__ import annotations

import re
from pathlib import Path
from xml.sax.saxutils import escape

PASTA = Path(__file__).resolve().parent

ELIPSE = ("rounded=0;whiteSpace=wrap;html=1;ellipse;fillColor=#6c9eeb;strokeColor=#000000;fontColor=#000000;"
          "fontSize=14;verticalAlign=middle;")
ATOR = "shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;html=1;outlineConnect=0;fontSize=13;"
NOTA = ("shape=note;size=16;whiteSpace=wrap;html=1;align=left;spacingLeft=8;fillColor=#ffffff;strokeColor=#000000;"
        "fontSize=12;")
ASSOCIACAO = "edgeStyle=orthogonalEdgeStyle;rounded=1;html=1;endArrow=none;strokeColor=#000000;"
RETA = "rounded=0;html=1;endArrow=none;strokeColor=#000000;"


class Diagrama:
    def __init__(self, nome: str) -> None:
        self.nome = nome
        self.caminho = PASTA / f"GASTRA_UC_{nome}.drawio"
        self.xml = self.caminho.read_text(encoding="utf-8")

    def id(self, sufixo: str) -> str:
        return f"GASTRA_UC_{self.nome}-{sufixo}"

    def tem(self, sufixo: str) -> bool:
        return f'id="{self.id(sufixo)}"' in self.xml

    def _acrescentar(self, celula: str) -> None:
        self.xml = self.xml.replace("</root>", celula + "</root>", 1)

    def vertice(self, sufixo: str, texto: str, estilo: str, x: int, y: int, largura: int, altura: int) -> None:
        if self.tem(sufixo):
            return
        valor = escape(texto, {'"': "&quot;"}).replace("\n", "&lt;br&gt;")
        self._acrescentar(f'<mxCell id="{self.id(sufixo)}" value="{valor}" style="{estilo}" vertex="1" parent="1">'
                          f'<mxGeometry x="{x}" y="{y}" width="{largura}" height="{altura}" as="geometry"/></mxCell>')

    def aresta(self, sufixo: str, origem: str, destino: str, pontos=(), estilo: str = ASSOCIACAO,
               saida: str = "exitX=1;exitY=0.4;exitDx=0;exitDy=0;") -> None:
        if self.tem(sufixo):
            return
        lista = "".join(f'<mxPoint x="{x}" y="{y}"/>' for x, y in pontos)
        geometria = f'<Array as="points">{lista}</Array>' if pontos else ""
        self._acrescentar(f'<mxCell id="{self.id(sufixo)}" value="" style="{estilo}{saida}" edge="1" parent="1" '
                          f'source="{self.id(origem)}" target="{self.id(destino)}">'
                          f'<mxGeometry relative="1" as="geometry">{geometria}</mxGeometry></mxCell>')

    def geometria(self, sufixo: str, **valores: int) -> None:
        padrao = rf'(<mxCell id="{re.escape(self.id(sufixo))}"[^>]*>\s*<mxGeometry )([^>]*?)(/?>)'
        m = re.search(padrao, self.xml)
        assert m, sufixo
        atributos = m.group(2)
        for chave, valor in valores.items():
            atributos, n = re.subn(rf'\b{chave}="[^"]*"', f'{chave}="{valor}"', atributos)
            if not n:
                atributos = f'{chave}="{valor}" ' + atributos
        self.xml = self.xml[:m.start(2)] + atributos + self.xml[m.end(2):]

    def texto(self, sufixo: str, novo: str) -> None:
        valor = escape(novo, {'"': "&quot;"})
        self.xml = re.sub(rf'(<mxCell id="{re.escape(self.id(sufixo))}" value=")[^"]*(")', rf"\g<1>{valor}\2", self.xml)

    def salvar(self) -> None:
        self.caminho.write_text(self.xml, encoding="utf-8")
        print("  ", self.caminho.name)


def gestao_cardapio() -> None:
    d = Diagrama("GestaoCardapio")
    d.geometria("2", height=680)
    d.vertice("uc28", "Definir Destaque do Dia\nou Meta do Mês (UC28)", ELIPSE, 400, 660, 260, 80)
    d.aresta("uc28-gerente", "3", "uc28", [(330, 240)])
    d.aresta("uc28-coordenador", "4", "uc28", [(330, 450)])
    d.geometria("20", y=670)
    d.salvar()


def nucleo_comandas() -> None:
    d = Diagrama("NucleoComandas")
    d.geometria("1", width=1300)
    d.geometria("2", width=920)
    d.vertice("uc26", "Lançar Cortesia e\nAjustar Preço (UC26)", ELIPSE, 820, 300, 270, 72)
    d.vertice("uc27", "Lançar Item para Confirmação\ndo Garçom (UC27)", ELIPSE, 820, 490, 270, 86)
    d.vertice("gerente", "Gerente", ATOR, 1290, 306, 30, 60)
    d.vertice("metre", "Metre", ATOR, 1290, 503, 30, 60)
    saida_esquerda = "exitX=0;exitY=0.5;exitDx=0;exitDy=0;"
    d.aresta("uc26-gerente", "gerente", "uc26", estilo=RETA, saida=saida_esquerda)
    d.aresta("uc27-gerente", "gerente", "uc27", estilo=RETA, saida=saida_esquerda)
    d.aresta("uc26-metre", "metre", "uc26", estilo=RETA, saida=saida_esquerda)
    d.aresta("uc27-metre", "metre", "uc27", estilo=RETA, saida=saida_esquerda)
    d.aresta("uc27-garcom", "3", "uc27", [(250, 416), (250, 533)],
             saida="exitX=1;exitY=0.4;exitDx=0;exitDy=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0;")
    d.texto("16", "Pré-condição de UC12, UC13, UC14, UC23, UC26 e UC27: comanda aberta na mesa (RF01).")
    d.geometria("16", height=100)
    d.geometria("17", y=620)
    d.vertice("nota-uc27", "No UC27, o item lançado pelo Metre ou pelo Gerente só entra na conta depois que o "
              "garçom da mesa confirma (RF27). Cortesia e ajuste de preço exigem motivo (RF26).",
              NOTA, 760, 760, 340, 80)
    d.geometria("2", height=820)
    d.salvar()


def blocos_analiticos() -> None:
    d = Diagrama("BlocosAnaliticos")
    d.geometria("2", height=720)
    d.vertice("uc29", "Visualizar Medalhas e Histórico\nde Conquistas (UC29)", ELIPSE, 680, 580, 260, 80)
    d.aresta("uc29-gerente", "4", "uc29", [(1020, 216), (1020, 620)])
    d.aresta("uc29-garcom", "5", "uc29", [(1020, 466), (1020, 620)])
    d.geometria("21", y=700)
    d.geometria("22", y=700, width=300)
    d.vertice("nota-uc29", "No UC29, o Gerente também dá menções honrosas ao garçom, com ícone e cor de uma lista "
              "pronta (RF29).", NOTA, 670, 690, 270, 80)
    d.salvar()


def configuracao_salao() -> None:
    d = Diagrama("ConfiguracaoSalao")
    d.geometria("2", height=300)
    d.vertice("uc30", "Gerar QR Codes\ndas Mesas (UC30)", ELIPSE, 360, 270, 280, 80)
    d.aresta("uc30-gerente", "3", "uc30", [(280, 196), (280, 310)])
    d.geometria("6", y=410)
    d.texto("6", "Toda mesa pertence a uma praça (REL01). A quantidade de garçons por praça é entrada da alocação "
            "(RN03). O QR do UC30 é fixo e abre o cardápio digital da mesa (UC19), e não a conta (RF30).")
    d.geometria("6", width=320, height=110)
    d.salvar()


if __name__ == "__main__":
    gestao_cardapio()
    nucleo_comandas()
    blocos_analiticos()
    configuracao_salao()
