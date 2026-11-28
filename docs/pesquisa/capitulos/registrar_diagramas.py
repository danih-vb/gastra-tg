"""Atualiza no Gastra II os diagramas da fase final e o texto que os descreve:

- Figuras 2 e 3 (casos de uso do núcleo de comandas e dos blocos analíticos), com UC26, UC27 e UC29;
- Figura 5 (DER), com DestaqueDoCardapio, MencaoHonrosa e os atributos novos;
- Figura 6 (modelo lógico por engenharia reversa), com as 15 tabelas atuais;
- Quadro 6 (mapa de dados pessoais), com a foto do funcionário e a menção honrosa.

    pip install python-docx pillow
    python docs/pesquisa/capitulos/registrar_diagramas.py

Cada imagem é trocada no lugar, mantendo a largura da figura e ajustando a altura à proporção nova. Rodar de novo
não duplica nada. Depois, no Word: Ctrl+A e F9.
"""

from pathlib import Path

from docx import Document
from PIL import Image

from registrar_fase_final import acrescentar_linha, trocar_texto

RAIZ = Path(__file__).resolve().parents[3]
DOCX = RAIZ / "docs" / "pesquisa" / "Gastra II.docx"
LARGURA_DO_TEXTO = 5760000  # EMU: 16 cm, a largura útil da página.
ALTURA_MAXIMA = 4680000  # EMU: a altura que DER e lógico já usavam, para caberem na página com legenda e fonte.
# O DER é quase quadrado e tem letra pequena: ganha a largura toda da página, que ainda cabe com legenda e fonte.
ALTURA_MAXIMA_POR_FIGURA = {"Diagrama Entidade-Relacionamento conceitual do GASTRA": 5500000}

FIGURAS = {
    "Business Model Canvas do GASTRA": "docs/negocio/GASTRA_Business_Model_Canvas.png",
    "Diagrama de casos de uso do núcleo de comandas": "docs/diagramas/casos-de-uso/GASTRA_UC_NucleoComandas.png",
    "Diagrama de casos de uso dos blocos analíticos": "docs/diagramas/casos-de-uso/GASTRA_UC_BlocosAnaliticos.png",
    "Diagrama Entidade-Relacionamento conceitual do GASTRA": "docs/modelagem/der/GASTRA_DER.png",
    "Modelo lógico obtido por engenharia reversa do banco implementado":
        "docs/modelagem/engenharia-reversa/GASTRA_Logico_Engenharia_Reversa.png",
}

TEXTOS = [
    ("A Figura 2 apresenta o diagrama do núcleo de comandas, que concentra a operação diária do salão.",
     "A Figura 2 apresenta o diagrama do núcleo de comandas, que concentra a operação diária do salão. Nele também "
     "entram o Metre e o Gerente, que oferecem cortesias e ajustam preços (UC26) e lançam itens que só entram na "
     "conta depois que o garçom confirma (UC27)."),
    ("e os relatórios, o índice de desempenho e a sugestão de pratos.",
     "e os relatórios, o índice de desempenho, a sugestão de pratos e as medalhas do garçom (UC29), com as menções "
     "honrosas que o Gerente concede."),
    ("com 11 entidades e 11 relacionamentos", "com 13 entidades e 14 relacionamentos"),
    ("cardinalidade (1,1) do lado da avaliação e (0,1) do lado da comanda.",
     "cardinalidade (1,1) do lado da avaliação e (0,1) do lado da comanda. A menção honrosa liga-se duas vezes ao "
     "usuário, porque o garçom que a recebe e o gerente que a concede são papéis diferentes da mesma entidade. Já o "
     "destaque do cardápio é uma entidade própria, e não um atributo do item, porque o mesmo item pode ser destaque "
     "em vários dias e ter meta em vários meses."),
    ("o banco tem 13 tabelas: uma para cada uma das 11 entidades do modelo conceitual",
     "o banco tem 15 tabelas: uma para cada uma das 13 entidades do modelo conceitual"),
    ("As tabelas estão ligadas por 13 chaves estrangeiras.", "As tabelas estão ligadas por 16 chaves estrangeiras."),
    ("foi incorporada ao MER e ao DER.",
     "foi incorporada ao MER e ao DER. Na fase final, a comparação foi refeita depois das tabelas dos destaques e das "
     "menções honrosas e das colunas da cortesia, do ajuste de preço e do lançamento para confirmar, e tudo foi "
     "incorporado ao MER e ao DER. Ficaram só no banco as duas colunas do bloqueio por tentativas de acesso, que são "
     "um mecanismo de segurança, e não um dado do negócio."),
]


DADOS_PESSOAIS = [
    ["Foto do funcionário", "Pessoal", "Funcionário",
     "Opcional; só o Gerente põe ou tira; guardada fora das fotos públicas do cardápio e entregue só a quem está "
     "logado (o Gerente vê todas; os demais, a própria); não vai para relatório, view nem log; o arquivo é apagado "
     "quando a foto é trocada ou a conta é inativada"],
    ["Título e descrição da menção honrosa", "Pessoal, por vínculo a um garçom identificado", "Funcionário",
     "Só o Gerente concede e remove; o garçom vê só as próprias; o texto nunca vai para o registro de auditoria"],
]


def trocar_imagem(documento, legenda, caminho) -> bool:
    paragrafos = documento.paragraphs
    i = next(i for i, p in enumerate(paragrafos) if p.style.name == "Caption" and legenda in p.text)
    figura = paragrafos[i + 1]._p
    rid = figura.xpath(".//a:blip/@r:embed")[0]
    parte = documento.part.related_parts[rid]
    novo = (RAIZ / caminho).read_bytes()
    mudou = parte.blob != novo
    parte._blob = novo
    largura, altura = Image.open(RAIZ / caminho).size
    teto = ALTURA_MAXIMA_POR_FIGURA.get(legenda, ALTURA_MAXIMA)
    for extent in figura.xpath(".//wp:extent") + figura.xpath(".//a:ext"):
        cx = LARGURA_DO_TEXTO
        cy = round(cx * altura / largura)
        if cy > teto:
            cx, cy = round(teto * largura / altura), teto
        mudou = mudou or extent.get("cx") != str(cx)
        extent.set("cx", str(cx))
        extent.set("cy", str(cy))
    return mudou


def main() -> None:
    documento = Document(str(DOCX))
    feitos = [trocar_imagem(documento, legenda, caminho) for legenda, caminho in FIGURAS.items()]
    mapa = next(t for t in documento.tables if [c.text for c in t.rows[0].cells][:2] == ["Dado", "Classificação"])
    feitos += [acrescentar_linha(mapa, linha) for linha in DADOS_PESSOAIS]
    for atual, novo in TEXTOS:
        if not any(novo in p.text for p in documento.paragraphs):
            feitos.append(any(trocar_texto(p, atual, novo) for p in documento.paragraphs))
    documento.save(str(DOCX))
    print(f"{sum(bool(f) for f in feitos)} alterações. No Word: Ctrl+A e F9.")


if __name__ == "__main__":
    main()
