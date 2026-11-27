"""Registra no MER o que entrou no banco na fase final (21 a 26/11/2026), já desenhado no DER:

- atributos novos em Usuário (foto), Comanda (atendimento_chamado_em) e ItemDoPedido (cortesia e ajuste de preço,
  lançamento para o garçom confirmar);
- ENT12 DestaqueDoCardapio com REL12, e ENT13 MencaoHonrosa com REL13 e REL14.

    pip install python-docx
    python docs/modelagem/mer/registrar_fase_final.py

Rodar de novo não duplica nada.
"""

import copy
from pathlib import Path

from docx import Document

MER = Path(__file__).resolve().parent / "GASTRA_MER.docx"

ATRIBUTOS = {
    "ENT03": ["foto (opcional; caminho da foto do funcionário, posta pelo Gerente e exibida na lista e no cabeçalho)"],
    "ENT04": ["atendimento_chamado_em (opcional; quando o cliente chamou o garçom pela conta digital; volta a vazio "
              "quando o garçom atende ou a comanda fecha)"],
    "ENT06": [
        "cortesia (verdadeiro quando o Metre ou o Gerente oferece o item: entra na conta por R$ 0,00 e não conta como "
        "venda; RF26)",
        "preco_original (opcional; o preço antes da cortesia ou do ajuste do Gerente; vazio se o preço nunca mudou)",
        "motivo_ajuste (opcional; obrigatório na cortesia e no ajuste de preço, para o caixa e a gestão entenderem)",
        "lancado_por (opcional; papel de quem lançou quando não foi o garçom da mesa, Metre ou Gerente; o item só "
        "entra na conta depois que o garçom confirma; RF27)",
    ],
}

SECOES = [
    ("7. Destaques e metas", [
        ("ENT12 – DestaqueDoCardapio", [
            "id_destaque (PK)",
            "tipo (destaque do dia | meta do mês)",
            "data_inicio",
            "data_fim (no destaque do dia, igual à data_inicio; na meta, o último dia do mês)",
            "meta (opcional; unidades a vender no mês; vazio no destaque do dia)",
        ]),
        ("REL12 – DestaqueDoCardapio destaca um ItemDoCardápio", [
            "Cardinalidade: N:1 (um item pode ser destacado ou ter meta em vários dias e meses; cada destaque é de "
            "exatamente um item)",
            "Participação: opcional para ItemDoCardápio; obrigatória para DestaqueDoCardapio",
            "Entidades envolvidas: ENT12, ENT05",
            "Regra de negócio: RF28. Definidos pelo Gerente ou pelo Coordenador; a equipe vê os destaques do dia e o "
            "andamento de cada meta, contado pelas vendas do mês.",
        ]),
    ]),
    ("8. Menções honrosas", [
        ("ENT13 – MencaoHonrosa", [
            "id_mencao (PK)",
            "titulo (até 60 caracteres)",
            "descricao (opcional; até 200 caracteres)",
            "icone (lista fechada: troféu, estrela, coração, faísca, alvo, relógio, pessoas, check)",
            "cor (lista fechada: laranja, marinho, verde, vermelho, roxo, dourado)",
            "mes (o mês a que se refere, guardado como o primeiro dia dele)",
            "data_hora_concessao",
        ]),
        ("REL13 – Usuário (papel Garçom) recebe uma MencaoHonrosa", [
            "Cardinalidade: 1:N (um garçom recebe várias menções; cada menção é de exatamente um garçom)",
            "Participação: opcional para Usuário; obrigatória para MencaoHonrosa",
            "Entidades envolvidas: ENT13, ENT03",
            "Regra de negócio: o Usuário referenciado deve ter papel = Garçom e estar ativo (validação de aplicação)",
        ]),
        ("REL14 – Usuário (papel Gerente) concede uma MencaoHonrosa", [
            "Cardinalidade: 1:N (um gerente concede várias menções; cada menção é concedida por exatamente um gerente)",
            "Participação: opcional para Usuário; obrigatória para MencaoHonrosa",
            "Entidades envolvidas: ENT13, ENT03",
            "Regra de negócio: RF29. Reconhece o que o sistema não mede; o título e a descrição não vão para o "
            "registro de auditoria, que guarda só quem recebeu, o ícone, a cor e o mês.",
        ]),
    ]),
]


def main() -> None:
    documento = Document(str(MER))
    paragrafos = documento.paragraphs
    textos = [p.text for p in paragrafos]
    feitos = 0

    # Atributos: entram depois do último item da lista de atributos da entidade.
    for entidade, novos in ATRIBUTOS.items():
        inicio = next(i for i, t in enumerate(textos) if t.startswith(entidade))
        fim = inicio + 2
        while fim < len(paragrafos) and paragrafos[fim].style.name == "List Paragraph":
            fim += 1
        ultimo = paragrafos[fim - 1]
        existentes = {paragrafos[i].text.split(" (")[0] for i in range(inicio + 2, fim)}
        for texto in reversed(novos):
            if texto.split(" (")[0] in existentes:
                continue
            novo = copy.deepcopy(ultimo._p)
            ultimo._p.addnext(novo)
            from docx.text.paragraph import Paragraph
            p = Paragraph(novo, ultimo._parent)
            for r in p.runs[1:]:
                r._r.getparent().remove(r._r)
            p.runs[0].text = texto
            feitos += 1

    # Seções novas no fim, copiando a formatação das que já existem.
    if not any(t.startswith("ENT12") for t in textos):
        modelo_titulo = next(p for p in paragrafos if p.style.name == "Heading 1")
        modelo_ent = next(p for p in paragrafos if p.text.startswith("ENT11"))
        modelo_rotulo = next(p for p in paragrafos if p.text == "Atributos:")
        modelo_item = next(p for p in paragrafos if p.style.name == "List Paragraph")
        modelo_rel = next(p for p in paragrafos if p.text.startswith("REL11"))
        modelo_linha = next(p for p in paragrafos if p.text.startswith("Cardinalidade:"))
        corpo = documento.element.body
        sect = corpo.find("{http://schemas.openxmlformats.org/wordprocessingml/2006/main}sectPr")

        def acrescentar(modelo, texto):
            novo = copy.deepcopy(modelo._p)
            for r in novo.findall("{http://schemas.openxmlformats.org/wordprocessingml/2006/main}r")[1:]:
                novo.remove(r)
            sect.addprevious(novo)
            from docx.text.paragraph import Paragraph
            Paragraph(novo, modelo._parent).runs[0].text = texto

        for titulo, blocos in SECOES:
            acrescentar(modelo_titulo, titulo)
            for nome, linhas in blocos:
                if nome.startswith("ENT"):
                    acrescentar(modelo_ent, nome)
                    acrescentar(modelo_rotulo, "Atributos:")
                    for linha in linhas:
                        acrescentar(modelo_item, linha)
                else:
                    acrescentar(modelo_rel, nome)
                    for linha in linhas:
                        acrescentar(modelo_linha, linha)
                feitos += 1

    documento.save(str(MER))
    print(f"{feitos} alterações no MER.")


if __name__ == "__main__":
    main()
