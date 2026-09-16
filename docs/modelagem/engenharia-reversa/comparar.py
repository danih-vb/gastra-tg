"""
Engenharia reversa (issue #84), passos 2 e 4.

  python comparar.py schema.tsv der_conceitual.xml

1. Converte o TSV do catálogo em fisico.json (entrada do gerar_logico.js).
2. Lê o DER conceitual exportado do brModelo em XML.
3. Compara entidade × tabela, atributo × coluna e relacionamento × chave estrangeira e imprime as
   diferenças. A justificativa de cada uma está em GASTRA_Validacao_Modelo_Fisico.md.
"""

import json
import sys
import xml.etree.ElementTree as ET
from collections import OrderedDict

TSV, XML = sys.argv[1], sys.argv[2]

# Entidade do DER -> tabela do banco. Promoção ainda não foi implementada (UC08/UC09).
MAPA = {"Praca": "praca", "Mesa": "mesa", "Usuario": "usuario", "Comanda": "comanda",
        "ItemDoPedido": "item_pedido", "ItemDoCardapio": "item_cardapio", "Alocacao": "alocacao",
        "RestricaoAlimentar": "restricao_alimentar", "RegistroAuditoria": "registro_auditoria",
        "Promocao": None}
CARD = {"0": "(1,1)", "1": "(0,1)", "2": "(1,n)", "3": "(0,n)"}  # códigos do brModelo


def ler_fisico():
    colunas, fks, uniques, pks = OrderedDict(), [], [], {}
    for linha in open(TSV, encoding="utf-8"):
        p = linha.rstrip("\n").split("\t")
        if p[0] == "COL":
            colunas.setdefault(p[1], []).append({"nome": p[2], "tipo": p[3], "nulo": p[4] == "YES"})
        elif p[0] == "FK":
            fks.append({"tabela": p[1], "coluna": p[2], "ref": p[3], "ref_coluna": p[4], "on_delete": p[5], "nome": p[6]})
        elif p[0] == "UQ":
            uniques.append({"tabela": p[1], "nome": p[2], "colunas": p[3].split(",")})
        elif p[0] == "PK":
            pks[p[1]] = p[2].split(",")
    fisico = {"tabelas": [{"nome": t, "colunas": c, "pk": pks[t]} for t, c in colunas.items()],
              "fks": fks, "uniques": uniques}
    json.dump(fisico, open("fisico.json", "w", encoding="utf-8"), ensure_ascii=False, indent=1)
    return fisico


def ler_conceitual():
    raiz = ET.parse(XML).getroot()
    por_id = {e.get("ID"): e for e in raiz.iter() if e.tag in ("Entidade", "Atributo", "Relacionamento") and e.get("ID")}
    entidades = {i: {"nome": e.findtext("Texto"), "atributos": []} for i, e in por_id.items() if e.tag == "Entidade"}
    relacionamentos = {i: {"nome": e.findtext("Texto"), "pontas": []} for i, e in por_id.items() if e.tag == "Relacionamento"}
    for lig in raiz.iter("Ligacao"):
        pontas = lig.find("Ligacoes")
        a, b = pontas.get("PontaA"), pontas.get("PontaB")
        for x, y in ((a, b), (b, a)):
            ex, ey = por_id.get(x), por_id.get(y)
            if ex is None or ey is None:
                continue
            if ex.tag == "Atributo" and ey.tag == "Entidade":
                entidades[y]["atributos"].append({"nome": ex.findtext("Texto"),
                                                  "identificador": ex.find("Identificador").get("Valor") == "true",
                                                  "opcional": ex.find("Opcional").get("Valor") == "true",
                                                  "multivalorado": ex.find("Multivalorado").get("Valor") == "true"})
            if ex.tag == "Relacionamento" and ey.tag == "Entidade" and lig.find("Cardinalidade") is not None:
                relacionamentos[x]["pontas"].append((entidades[y]["nome"], CARD[lig.find("Cardinalidade/Card").get("Valor")]))
    return list(entidades.values()), list(relacionamentos.values())


fisico = ler_fisico()
entidades, relacionamentos = ler_conceitual()
tabelas = {t["nome"]: t for t in fisico["tabelas"]}
fk_por_coluna = {(k["tabela"], k["coluna"]): k for k in fisico["fks"]}

print("## Entidades e atributos")
for e in entidades:
    tabela = MAPA[e["nome"]]
    if tabela is None:
        print(f"- {e['nome']}: sem tabela")
        continue
    colunas = {c["nome"]: c for c in tabelas[tabela]["colunas"]}
    for a in e["atributos"]:
        nome = a["nome"]
        if a["identificador"] and "id" in colunas:
            continue  # identificador: id_<entidade> no DER vira "id" na tabela (convenção)
        if "(derivado)" in nome:
            print(f"- {e['nome']}.{nome}: derivado, não é coluna (calculado em view)")
        elif a["multivalorado"]:
            print(f"- {e['nome']}.{nome}: multivalorado, não é coluna (virou tabela própria)")
        elif nome not in colunas:
            print(f"- {e['nome']}.{nome}: só no conceitual")
        elif colunas[nome]["nulo"] != a["opcional"]:
            print(f"- {e['nome']}.{nome}: opcional no conceitual = {a['opcional']}, nulo no físico = {colunas[nome]['nulo']}")
    nomes = {a["nome"] for a in e["atributos"]}
    for c in colunas.values():
        if c["nome"] not in nomes and c["nome"] != "id":
            origem = "chave estrangeira" if (tabela, c["nome"]) in fk_por_coluna else "sem atributo correspondente"
            print(f"- {tabela}.{c['nome']}: só no físico ({origem})")

print("\n## Relacionamentos x chaves estrangeiras")
inverso = {v: k for k, v in MAPA.items() if v}
for r in relacionamentos:
    (e1, c1), (e2, c2) = r["pontas"]
    achou = False
    for filho, card_filho, pai, card_pai in ((e1, c1, e2, c2), (e2, c2, e1, c1)):
        for k in fisico["fks"]:
            if inverso.get(k["tabela"]) == filho and inverso.get(k["ref"]) == pai:
                nulo = next(c["nulo"] for c in tabelas[k["tabela"]]["colunas"] if c["nome"] == k["coluna"])
                esperado = "(0,1)" if nulo else "(1,1)"
                situacao = "confere" if card_filho == esperado else f"DIVERGE (físico indica {esperado})"
                print(f"- {filho} {card_filho} — {r['nome']} — {pai} {card_pai}: {k['tabela']}.{k['coluna']} "
                      f"{'NULL' if nulo else 'NOT NULL'} -> {situacao}")
                achou = True
    if not achou:
        print(f"- {e1} {c1} — {r['nome']} — {e2} {c2}: sem chave estrangeira")

tabelas_mapeadas = set(MAPA.values())
for t in tabelas:
    if t not in tabelas_mapeadas:
        print(f"\n- tabela {t}: sem entidade correspondente")
