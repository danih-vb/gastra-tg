"""Registra no Gastra II o que entrou depois de 23/11/2026: menção honrosa, QR codes das mesas e o app instalável no
celular. Também atualiza a Tabela 2 e as contagens de testes a partir de docs/testes/GASTRA_Cenarios_Testes_Executados.md.

    pip install python-docx
    python docs/pesquisa/capitulos/registrar_entrega_final.py

Rodar de novo não duplica nada. Depois, no Word: Ctrl+A e F9.
"""

import copy
import re
from pathlib import Path

from docx import Document

from registrar_fase_final import acrescentar_linha, escrever_na_celula, teste, trocar_texto

RAIZ = Path(__file__).resolve().parents[3]
DOCX = RAIZ / "docs" / "pesquisa" / "Gastra II.docx"
RELATORIO = RAIZ / "docs" / "testes" / "GASTRA_Cenarios_Testes_Executados.md"

RF30 = ["RF30", "Gerar os QR codes fixos das mesas, prontos para imprimir, que abrem o cardápio digital da mesa",
        "Gerente", "Decisão da dupla (26/11)"]
US29 = [
    "US29 — Como Gerente, quero imprimir um QR code para cada mesa, para o cliente abrir o cardápio pelo celular (RF30)",
    "Uma etiqueta por mesa, com o número e a praça, filtrável por praça; o QR abre o cardápio da mesa, e não a conta, "
    "que continua pelo QR que o garçom mostra; o endereço do link é editável",
    "qr-das-mesas.spec.ts: uma etiqueta por mesa, em ordem de praça e número, com o QR do cardápio daquela mesa; "
    "qr-das-mesas.spec.ts: filtra pela praça e usa o endereço digitado no link",
]
MENCAO_NO_RF29 = ("já registrados", "já registrados, e as menções honrosas dadas pelo Gerente")
MENCAO_NA_US28 = (
    "a consulta é registrada na auditoria",
    "a consulta é registrada na auditoria; o Gerente dá menções honrosas com ícone e cor de uma lista, e o texto delas "
    "não vai para o log",
)
TESTES_DA_MENCAO = "; ".join([
    teste("IndicadoresControllerTests", "Mencao_PeloGerente_ApareceParaOGarcomJuntoDasMedalhas_ESemOTextoNoLog"),
    teste("IndicadoresControllerTests", "Mencao_PeloGarcom_Retorna403"),
])

TEXTOS = [
    # Contagens
    ("Foram especificados 29 requisitos funcionais, dos quais 28 foram aprovados",
     "Foram especificados 30 requisitos funcionais, dos quais 29 foram aprovados"),
    ("Os requisitos funcionais foram detalhados em 29 casos de uso", "Os requisitos funcionais foram detalhados em 30 casos de uso"),
    ("Os requisitos também foram escritos como 28 histórias de usuário",
     "Os requisitos também foram escritos como 29 histórias de usuário"),
    ("e o UC29, das medalhas do garçom.", "o UC29, das medalhas do garçom, e o UC30, dos QR codes das mesas."),
    ("Da mesma fase são o UC27,", "Da mesma fase são o UC27,"),
    ("formalizado em 28 requisitos funcionais aprovados", "formalizado em 29 requisitos funcionais aprovados"),
    # 5.1
    ("O Garçom usa o sistema no celular (Figura 18).",
     "O Garçom usa o sistema no celular (Figura 18), pelo navegador ou instalado na tela de início como aplicativo, que "
     "guarda no aparelho só a aparência do sistema e busca mesas, comandas e contas sempre no servidor."),
    ("e as medalhas do mês, com quanto falta para cada uma e as conquistas dos meses anteriores (RF29).",
     "e as medalhas do mês, com quanto falta para cada uma, as conquistas dos meses anteriores e as menções honrosas "
     "dadas pelo Gerente (RF29)."),
    ("as medalhas e o histórico de conquistas; o Coordenador",
     "as medalhas, o histórico de conquistas e as menções honrosas, que o Gerente cria escolhendo ícone, cor, título e "
     "descrição, para reconhecer o que o sistema não mede. Na tela do salão, ele imprime os QR codes das mesas (RF30), "
     "que abrem o cardápio digital da mesa; o Coordenador"),
    # 6.3
    ("tornar o aplicativo do garçom instalável no celular e capaz de funcionar com conexão instável;",
     "permitir que o aplicativo, que já pode ser instalado no celular, continue funcionando com conexão instável, "
     "guardando os lançamentos até a rede voltar;"),
    ("publicar o sistema em um provedor de nuvem, cujos custos foram estimados na seção 3.2.",
     "publicar o sistema em um provedor de nuvem, cujos custos foram estimados na seção 3.2, com HTTPS, o que também "
     "dispensa o ajuste no navegador hoje necessário para instalar o aplicativo no celular pela rede local."),
]
TRABALHO_FEITO = "• gerar, pelo próprio sistema, os códigos QR fixos de cada mesa para impressão;"

NOMES_DOS_BLOCOS = {"Integração backend ↔ Python": "Integração backend e serviço analítico"}


def numeros_do_relatorio():
    texto = RELATORIO.read_text(encoding="utf-8")
    data = re.search(r"\*\*Execução:\*\* (\d{2}/\d{2}/\d{4})", texto).group(1)
    camadas = {m.group(1): int(m.group(2)) for m in re.finditer(r"^\| (Backend \.NET|Analítica Python|Frontend Angular) \| \d+ \| (\d+) \|", texto, re.M)}
    resumo = texto.split("## Resumo por bloco", 1)[1].split("\n## ", 1)[0]
    blocos = {}
    for linha in resumo.splitlines():
        partes = [p.strip() for p in linha.strip("|").split("|")]
        if len(partes) >= 3 and partes[1].isdigit():
            blocos[NOMES_DOS_BLOCOS.get(partes[0], partes[0])] = (partes[1], partes[2])
    blocos["Total"] = (str(sum(int(c) for c, _ in blocos.values())), str(sum(int(k) for _, k in blocos.values())))
    return data, camadas, blocos


def main() -> None:
    documento = Document(str(DOCX))
    tabelas = documento.tables
    quadro_rf = next(t for t in tabelas if t.rows[0].cells[0].text == "ID" and any(r.cells[0].text == "RF25" for r in t.rows))
    quadro_uc = next(t for t in tabelas if t.rows[0].cells[0].text == "Grupo")
    quadro_us = next(t for t in tabelas if t.rows[0].cells[0].text == "História")
    tabela_2 = next(t for t in tabelas if t.rows[0].cells[0].text == "Bloco")

    feitos = [acrescentar_linha(quadro_rf, RF30), acrescentar_linha(quadro_us, US29)]

    rf29 = next(r for r in quadro_rf.rows if r.cells[0].text == "RF29")
    if MENCAO_NO_RF29[1] not in rf29.cells[1].text:
        escrever_na_celula(rf29.cells[1], rf29.cells[1].text.replace(*MENCAO_NO_RF29))
        escrever_na_celula(rf29.cells[2], "Garçom (as próprias); Gerente (de qualquer garçom, e dá as menções)")
        feitos.append(True)

    us28 = next(r for r in quadro_us.rows if r.cells[0].text.startswith("US28"))
    if MENCAO_NA_US28[1] not in us28.cells[1].text:
        escrever_na_celula(us28.cells[1], us28.cells[1].text.replace(*MENCAO_NA_US28))
        escrever_na_celula(us28.cells[2], us28.cells[2].text + "; " + TESTES_DA_MENCAO)
        feitos.append(True)

    salao = next(r for r in quadro_uc.rows if "UC24" in r.cells[1].text)
    if "UC30" not in salao.cells[1].text:
        escrever_na_celula(salao.cells[1], salao.cells[1].text + "; UC30 Gerar QR codes das mesas")
        feitos.append(True)

    data, camadas, blocos = numeros_do_relatorio()
    for linha in tabela_2.rows[1:]:
        cenarios, casos = blocos[linha.cells[0].text]
        if linha.cells[2].text != casos:
            escrever_na_celula(linha.cells[1], cenarios)
            escrever_na_celula(linha.cells[2], casos)
            escrever_na_celula(linha.cells[3], casos)
            feitos.append(True)

    total = blocos["Total"][1]
    textos = TEXTOS + [
        (r"a partir da execução de \d{2}/\d{2}/2026", f"a partir da execução de {data}"),
        (r"Os \d+ casos foram aprovados", f"Os {total} casos foram aprovados"),
        (r"são \d+ casos no backend, \d+ na camada analítica e \d+ no frontend",
         f"são {camadas['Backend .NET']} casos no backend, {camadas['Analítica Python']} na camada analítica e "
         f"{camadas['Frontend Angular']} no frontend"),
        (r"validado por \d+ casos de teste", f"validado por {total} casos de teste"),
    ]
    for atual, novo in textos:
        if atual.startswith(("a partir", "Os ", "são ", "validado")) and "\\d" in atual:
            for p in documento.paragraphs:
                m = re.search(atual, p.text)
                if m and m.group(0) != novo:
                    feitos.append(trocar_texto(p, m.group(0), novo))
        elif atual != novo:
            feitos.append(any(trocar_texto(p, atual, novo) for p in documento.paragraphs))

    feito = next((p for p in documento.paragraphs if p.text == TRABALHO_FEITO), None)
    if feito is not None:
        feito._p.getparent().remove(feito._p)
        feitos.append(True)

    documento.save(str(DOCX))
    print(f"{sum(bool(f) for f in feitos)} alterações (testes de {data}: {total} casos). No Word: Ctrl+A e F9.")


if __name__ == "__main__":
    main()
