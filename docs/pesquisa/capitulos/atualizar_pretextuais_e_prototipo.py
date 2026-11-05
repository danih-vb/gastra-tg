"""Atualiza o Gastra II.docx (depois da revisão do Daniel, PR #257): elementos pré-textuais, seção 4.4.5, ajustes do
capítulo 2, negrito das referências e correções pontuais. Rodar uma vez, em cima da versão do PR #257.

    pip install python-docx
    python docs/pesquisa/capitulos/atualizar_pretextuais_e_prototipo.py

Depois, no Word: Ctrl+A e F9, para atualizar sumário, listas e numeração das legendas.
"""

from __future__ import annotations

import copy
from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.oxml.ns import qn
from docx.shared import Cm, Pt

from levar_para_docx import blocos, escrever

PASTA = Path(__file__).resolve().parent
DOCX = PASTA.parent / "Gastra II.docx"
TITULO = "GASTRA: SISTEMA DE APOIO À DECISÃO PARA A GESTÃO DE RESTAURANTES À LA CARTE"


# ---------------------------------------------------------------- texto em runs

def texto_de(elemento) -> str:
    return "".join(elemento.xpath("./w:r/w:t/text()"))


def trocar_no_paragrafo(paragrafo, velho: str, novo: str) -> None:
    """Troca dentro de um único run quando dá; senão junta tudo no primeiro run, com a formatação dele."""
    for run in paragrafo.runs:
        if velho in run.text:
            run.text = run.text.replace(velho, novo)
            return
    texto = paragrafo.text
    assert velho in texto, (velho, texto[:80])
    runs = paragrafo.runs
    runs[0].text = texto.replace(velho, novo)
    for run in runs[1:]:
        run._r.getparent().remove(run._r)


def paragrafo_com(documento, inicio: str):
    return next(p for p in documento.paragraphs if p.text.strip().startswith(inicio))


def substituir_texto(paragrafo, novo: str) -> None:
    runs = paragrafo.runs
    runs[0].text = novo
    for run in runs[1:]:
        run._r.getparent().remove(run._r)


# ---------------------------------------------------------------- ajustes de texto

def ajustes_de_texto(documento) -> None:
    # Título 2 sem o número do capítulo.
    trocar_no_paragrafo(paragrafo_com(documento, ".3 MODELAGEM DE DADOS"), ".3 MODELAGEM", "4.3 MODELAGEM")
    # A referência do Strategyzer não tem ano.
    trocar_no_paragrafo(paragrafo_com(documento, "Fonte: elaborado pelos autores com base no template de Strategyzer"),
                        "Strategyzer (2026)", "Strategyzer ([20--])")
    # Legendas do capítulo 3 com travessão, como as dos demais capítulos e o modelo.
    for paragrafo in documento.paragraphs:
        if paragrafo.style.name == "Caption" and paragrafo.text.startswith(("Figura 1 - ", "Tabela 1 - ")):
            trocar_no_paragrafo(paragrafo, " - ", " – ")

    # Justificativas próprias, em vez de "definido no projeto de pesquisa".
    substituir_texto(paragrafo_com(documento, "O backend foi desenvolvido em ASP.NET Core"), (
        "O backend foi desenvolvido em ASP.NET Core (MICROSOFT, 2026a), framework de código aberto e multiplataforma, "
        "que roda em contêineres Linux sem custo de licença. Pesaram na escolha a tipagem estática do C#, que leva para "
        "a compilação uma parte dos erros que em linguagens dinâmicas só apareceriam em execução — relevante em um "
        "sistema cujas regras de negócio, como o fechamento da comanda e o cálculo da taxa de serviço, precisam estar "
        "corretas —, e o fato de a própria plataforma oferecer autenticação por token, injeção de dependência e acesso "
        "a dados, sem depender de bibliotecas de terceiros para o essencial. É o backend que concentra as regras de "
        "negócio, a autenticação e a gravação no banco de dados. O acesso a dados utiliza o Entity Framework Core "
        "(MICROSOFT, 2026b) como mapeador objeto-relacional, com a abordagem Code First, na qual o esquema do banco de "
        "dados é gerado a partir das classes do domínio por meio de migrations versionadas — em vez de escrito "
        "manualmente em SQL —, o que mantém o histórico de alterações do banco de dados rastreável junto ao histórico "
        "de código da aplicação."))
    substituir_texto(paragrafo_com(documento, "O frontend foi desenvolvido em Angular"), (
        "O frontend foi desenvolvido em Angular (GOOGLE, 2026), framework que traz na própria plataforma o roteamento "
        "entre telas, os formulários com validação, o cliente HTTP e a estrutura de testes, em vez de exigir a escolha e "
        "a integração de uma biblioteca diferente para cada uma dessas partes. Por usar TypeScript, permite descrever no "
        "frontend os mesmos contratos de dados da API, de modo que uma mudança no formato de uma resposta aparece como "
        "erro de compilação, e não como falha na tela. A aplicação é de página única (Single-Page Application), "
        "organizada por módulo funcional, de modo que cada grupo de telas (comandas, alocação, análises, cliente) possa "
        "evoluir de forma independente, sem interferir nos demais."))
    paragrafo_mysql = paragrafo_com(documento, "O banco de dados relacional utiliza MySQL")
    resto = paragrafo_mysql.text.split("sobre o qual foram criadas views", 1)[1]
    substituir_texto(paragrafo_mysql, (
        "O banco de dados relacional utiliza MySQL (ORACLE, 2026), sistema gerenciador de código aberto, amplamente "
        "adotado e oferecido como serviço gerenciado pelos principais provedores de nuvem — inclusive o considerado na "
        "estimativa de custos da seção 3.2 —, o que simplifica levar o sistema para produção. Sobre ele foram criadas "
        "views" + resto))
    trocar_no_paragrafo(paragrafo_com(documento, "Os requisitos do GASTRA resultam do cruzamento"),
                        "com o escopo definido no projeto de pesquisa", "dentro do escopo definido pelos objetivos da seção 1.3")
    assert not any("projeto de pesquisa" in p.text for p in documento.paragraphs[30:]), "sobrou remissão ao projeto de pesquisa"


# ---------------------------------------------------------------- referências: destaque da NBR 6023

DESTAQUES = {
    "AGRAWAL": "Proceedings [...]",
    "ASSOCIAÇÃO BRASILEIRA DE BARES E RESTAURANTES (ABRASEL). Bares": "Bares e restaurantes faturaram mais em 2025, diz pesquisa de serviços",
    "ASSOCIAÇÃO BRASILEIRA DE BARES E RESTAURANTES (ABRASEL). Salário": "Salário em bares e restaurantes segue crescendo, mas contratação ainda é desafio",
    "BRASIL": "Lei nº 13.709, de 14 de agosto de 2018",
    "CAVIQUE": "Revista Portuguesa e Brasileira de Gestão",
    "COELHO": "Programação matemática aplicada a casos reais de designação de entregadores em uma empresa de food delivery",
    "DOCKER": "Docker Docs",
    "DRIESSEN": "A successful Git branching model",
    "GINI": "Variabilità e mutabilità",
    "GITHUB": "GitHub Actions documentation",
    "GONÇALVES": "Anais [...]",
    "GOOGLE": "Angular",
    "HILLIER": "Introdução à pesquisa operacional",
    "HUALLPA": "Anais [...]",
    "JONES": "JSON Web Token (JWT)",
    "KUBERNS": "Azure Pricing 2026",
    "MACQUEEN": "Proceedings [...]",
    "MARTÍNEZ": "Condiciones óptimas para implementar Business Intelligence en la productividad de MIPYMES",
    "MICROSOFT. ASP": "ASP.NET Core documentation",
    "MICROSOFT. Entity": "Entity Framework Core",
    "MITCHELL": "PuLP",
    "MOREIRA": "Desenvolvimento de sistema de feedback de consumo integrado a software de restaurante",
    "NIELSEN, Jakob. Heuristic": "Usability inspection methods",
    "NIELSEN, Jakob. Response": "Response Time",
    "ORACLE": "MySQL 8.4 reference manual",
    "OWASP": "OWASP Top 10:2021",
    "PEDREGOSA": "Journal of Machine Learning Research",
    "PRESSMAN": "Engenharia de software",
    "PUMP.CO": "Azure Database for MySQL",
    "RAMÍREZ": "FastAPI",
    "RAPÔSO": "RACE – Revista de Administração do Cesmac",
    "RASCHKA": "Journal of Open Source Software",
    "REGISTRO.BR": "Sobre domínios",
    "ROUSSEEUW": "Journal of Computational and Applied Mathematics",
    "SAIPOS": "Sistema de gestão da Saipos",
    "SANTOS": "Interface Tecnológica",
    "SCHWABER": "O Guia do Scrum",
    "SOMMERVILLE": "Engenharia de software",
    "STRATEGYZER": "Business Model Canvas",
    "SYED": "Procedia – Social and Behavioral Sciences",
    "VIEIRA": "Produção em Foco",
    "ZANIOL": "Resumos [...]",
}


def negrito_nas_referencias(documento) -> int:
    paragrafos = documento.paragraphs
    inicio = next(i for i, p in enumerate(paragrafos) if p.text.strip() == "REFERÊNCIAS" and p.style.name.startswith("Heading"))
    feitos = 0
    for paragrafo in paragrafos[inicio + 1:]:
        texto = paragrafo.text.strip()
        if not texto:
            continue
        if paragrafo.style.name.startswith("Heading"):
            break
        chave = max((c for c in DESTAQUES if texto.startswith(c)), key=len, default=None)
        assert chave, f"referência sem destaque definido: {texto[:60]}"
        destaque = DESTAQUES[chave]
        # Em artigo e evento o destaque vem depois do título: procura a partir do fim do nome do autor.
        posicao = texto.index(destaque, texto.index(". ") + 2 if destaque.startswith(("Proceedings", "Anais", "Resumos")) else 0)
        molde = copy.deepcopy(paragrafo.runs[0]._r)
        for run in paragrafo.runs:
            run._r.getparent().remove(run._r)
        for pedaco, negrito in ((texto[:posicao], False), (destaque, True), (texto[posicao + len(destaque):], False)):
            if not pedaco:
                continue
            run = copy.deepcopy(molde)
            for t in run.findall(qn("w:t"))[1:]:
                run.remove(t)
            run.find(qn("w:t")).text = pedaco
            run.find(qn("w:t")).set(qn("xml:space"), "preserve")
            rpr = run.find(qn("w:rPr"))
            if rpr is None:
                rpr = run.makeelement(qn("w:rPr"), {})
                run.insert(0, rpr)
            for b in rpr.findall(qn("w:b")):
                rpr.remove(b)
            if negrito:
                rpr.append(rpr.makeelement(qn("w:b"), {}))
            paragrafo._p.append(run)
        feitos += 1
    return feitos


# ---------------------------------------------------------------- pré-textuais

def capa_e_folha_de_rosto(documento) -> None:
    corpo = documento.element.body
    elementos = list(corpo)
    fim = next(i for i, el in enumerate(elementos) if el.tag == qn("w:p") and el.xpath(".//w:br[@w:type='page']"))
    pagina = elementos[: fim + 1]

    # Capa: cópia da primeira página, sem o bloco da natureza do trabalho.
    capa = [copy.deepcopy(el) for el in pagina]
    for el in capa:
        texto = texto_de(el)
        if texto.startswith("Projeto de pesquisa apresentado"):
            for run in el.findall(qn("w:r")):
                el.remove(run)
    for el in reversed(capa):
        elementos[0].addprevious(el)
        elementos[0] = el
    for el in capa:
        _ajustar_titulo_local_ano(el)

    # Folha de rosto: sem as linhas da instituição (o modelo não as tem), com a natureza do trabalho do modelo.
    for el in pagina:
        texto = texto_de(el)
        if texto in ("CENTRO PAULA SOUZA", "FACULDADE DE TECNOLOGIA DE ARARAQUARA",
                     "CURSO DE GRADUAÇÃO EM ANÁLISE E DESENVOLVIMENTO DE SISTEMAS"):
            for run in el.findall(qn("w:r")):
                el.remove(run)
        elif texto.startswith("Projeto de pesquisa apresentado"):
            runs = el.findall(qn("w:r"))
            molde = next(r for r in runs if r.find(qn("w:t")) is not None)
            for run in runs:
                el.remove(run)
            for pedaco in ("Trabalho apresentado como exigência parcial para a conclusão do Curso de Graduação em "
                           "Análise e Desenvolvimento de Sistemas pela Faculdade de Tecnologia de Araraquara – FATEC "
                           "Araraquara.", None, None, "Orientador: Prof. Me. Leonardo José de Lima Ferrucci"):
                run = copy.deepcopy(molde)
                for filho in list(run):
                    if filho.tag != qn("w:rPr"):
                        run.remove(filho)
                if pedaco is None:
                    run.append(run.makeelement(qn("w:br"), {}))
                else:
                    t = run.makeelement(qn("w:t"), {})
                    t.text = pedaco
                    run.append(t)
                el.append(run)
        _ajustar_titulo_local_ano(el)


def _ajustar_titulo_local_ano(el) -> None:
    texto = texto_de(el)
    trocas = {"GASTRA": TITULO, "Araraquara": "Araraquara – SP", "6º semestre/2026": "2º semestre / 2026"}
    if texto in trocas:
        runs = [r for r in el.findall(qn("w:r")) if r.find(qn("w:t")) is not None]
        runs[0].find(qn("w:t")).text = trocas[texto]
        for run in runs[1:]:
            el.remove(run)


def paginas_pre_textuais(documento) -> None:
    """Dedicatória, agradecimentos, epígrafe, resumo e abstract, cada um em página própria, antes das listas."""
    lista = paragrafo_com(documento, "LISTA DE ILUSTRAÇÕES")
    ancora = lista._p

    def novo(texto="", alinhamento=None, negrito=False, italico=False, recuo_esquerdo=None, antes=None, recuo_primeira=None):
        p = documento.add_paragraph()
        if alinhamento is not None:
            p.alignment = alinhamento
        formato = p.paragraph_format
        formato.first_line_indent = Cm(0) if recuo_primeira is None else recuo_primeira
        if recuo_esquerdo is not None:
            formato.left_indent = recuo_esquerdo
        if antes is not None:
            formato.space_before = antes
        if texto:
            run = p.add_run(texto)
            run.bold = negrito
            run.italic = italico
        ancora.addprevious(p._p)
        return p

    def quebra():
        novo().add_run().add_break(WD_BREAK.PAGE)

    def titulo(texto):
        novo(texto, WD_ALIGN_PARAGRAPH.CENTER, negrito=True)
        novo()

    # Dedicatória e epígrafe: no pé da página, à direita, como no modelo.
    novo("Às nossas famílias.", WD_ALIGN_PARAGRAPH.RIGHT, italico=True, antes=Pt(500))
    quebra()
    titulo("AGRADECIMENTOS")
    for texto in (
        "Às nossas famílias, pela paciência e pelo apoio durante todo o curso.",
        "Ao Prof. Me. Leonardo José de Lima Ferrucci, nosso orientador, pela orientação ao longo dos dois semestres "
        "do trabalho de graduação.",
        "Ao Prof. Me. Wdson de Oliveira, pela condução das disciplinas de trabalho de graduação e pelas orientações "
        "sobre a estrutura deste trabalho.",
    ):
        novo(texto, WD_ALIGN_PARAGRAPH.JUSTIFY, recuo_primeira=Cm(1.5))
    quebra()
    novo("“A melhor maneira de prever o futuro é inventá-lo.”", WD_ALIGN_PARAGRAPH.RIGHT, italico=True,
         recuo_esquerdo=Cm(8), antes=Pt(500))
    novo("Alan Kay", WD_ALIGN_PARAGRAPH.RIGHT)
    quebra()
    titulo("RESUMO")
    novo(RESUMO, WD_ALIGN_PARAGRAPH.JUSTIFY)
    novo()
    p = novo(alinhamento=WD_ALIGN_PARAGRAPH.JUSTIFY)
    p.add_run("Palavras-chave: ").bold = True
    p.add_run("Análise e Desenvolvimento de Sistemas. Engenharia de Software. Business Intelligence. Programação linear.")
    quebra()
    titulo("ABSTRACT")
    novo(ABSTRACT, WD_ALIGN_PARAGRAPH.JUSTIFY)
    novo()
    p = novo(alinhamento=WD_ALIGN_PARAGRAPH.JUSTIFY)
    p.add_run("Keywords: ").bold = True
    p.add_run("Systems Analysis and Development. Software Engineering. Business Intelligence. Linear programming.")
    quebra()


RESUMO = (
    "A distribuição de garçons entre as praças de um restaurante à la carte costuma seguir critérios informais, como a "
    "antiguidade, e a sugestão de pratos ao cliente depende da experiência individual de cada profissional, sem apoio "
    "nos dados que a própria operação gera. Este trabalho apresenta o GASTRA, um sistema web de apoio à decisão para a "
    "gestão de restaurantes, que registra a operação do salão — abertura de mesa, pedidos e fechamento da conta — e usa "
    "esse registro em três frentes: relatórios de Business Intelligence para o gerente, recomendação de pratos ao garçom "
    "e sugestão de alocação dos garçons às praças. A recomendação combina regras de associação, obtidas pelo algoritmo "
    "Apriori, com a clusterização das comandas em perfis de consumo pelo K-Means; a alocação é modelada como um problema "
    "de designação e resolvida por programação linear, ponderando a diferença de faturamento entre os garçons e o tempo "
    "desde a última praça de maior movimento. Os requisitos foram levantados por entrevista e questionários com garçons "
    "e clientes, e o sistema foi construído com ASP.NET Core, Angular, Python e MySQL, em conformidade com a Lei Geral de "
    "Proteção de Dados: o cliente não é identificado, e as análises usam apenas dados agregados. Na ausência de histórico "
    "real, os algoritmos foram validados com dados simulados. A recomendação segmentada por perfil elevou o acerto das "
    "sugestões de 76,2% para 83,1%, e a alocação, aplicada turno a turno, reduziu em 92% a desigualdade de faturamento "
    "entre os garçons em 30 dias, medida pelo coeficiente de Gini. O sistema foi verificado por mais de 500 casos de "
    "teste automatizados, todos aprovados.")

ABSTRACT = (
    "The assignment of waiters to the sections of an à la carte restaurant usually follows informal criteria, such as "
    "seniority, and suggesting dishes to customers depends on each professional's experience, with no support from the "
    "data the operation itself produces. This work presents GASTRA, a web-based decision support system for restaurant "
    "management that records the dining room operation — opening a table, placing orders and closing the bill — and "
    "uses this record in three ways: Business Intelligence reports for the manager, dish recommendations for the waiter "
    "and a suggested assignment of waiters to sections. The recommendation combines association rules, mined with the "
    "Apriori algorithm, with K-Means clustering of orders into consumption profiles; the assignment is modeled as an "
    "assignment problem and solved by linear programming, weighing the revenue gap between waiters and the time since "
    "each one last worked in a high-traffic section. Requirements were gathered through an interview and questionnaires "
    "with waiters and customers, and the system was built with ASP.NET Core, Angular, Python and MySQL, in compliance "
    "with the Brazilian General Data Protection Law (LGPD): customers are not identified, and the analyses use only "
    "aggregated data. In the absence of real historical data, the algorithms were validated on simulated data. "
    "Profile-based recommendation raised the hit rate of the suggestions from 76.2% to 83.1%, and the assignment, "
    "applied shift after shift, reduced revenue inequality among waiters by 92% in 30 days, as measured by the Gini "
    "coefficient. The system was verified by more than 500 automated test cases, all of which passed.")


# ---------------------------------------------------------------- 4.4.5 Prototipação

PROTOTIPO = """### 4.4.5 Prototipação

Antes das telas definitivas, foi construído um protótipo navegável em HTML, executado diretamente no navegador,
com dados fictícios e as mesmas regras, listas fechadas, mensagens de erro e permissões já implementadas no
backend. O protótipo cobre as quatro personas — Garçom e Cliente no formato de celular, Metre em tablet ou
computador e Gerente em computador — além do acesso, e suas telas foram capturadas em 33 imagens e importadas no
Figma. Ele serviu a três propósitos: validar o fluxo com antecedência, medir o RNF02 com um contador de toques
embutido e servir de base para as telas em Angular.

As figuras 10 a 17 apresentam as 33 telas, agrupadas por perfil de usuário: o acesso ao sistema (Figura 10), o
Garçom (figuras 11 a 13), o Metre (Figura 14), o Gerente (figuras 15 e 16) e o Cliente (Figura 17). Cada tela mostra
a área visível sem rolagem; as imagens completas estão no repositório do projeto.

**Figura 10 – Protótipo: acesso ao sistema**

![Protótipo: acesso ao sistema](../../ux-ui/telas/paineis/acesso.png)

Fonte: elaborado pelos autores.

**Figura 11 – Protótipo: Garçom, do salão ao lançamento do item**

![Protótipo: Garçom, do salão ao lançamento do item](../../ux-ui/telas/paineis/garcom-1.png)

Fonte: elaborado pelos autores.

**Figura 12 – Protótipo: Garçom, restrições, sugestões e entregas**

![Protótipo: Garçom, restrições, sugestões e entregas](../../ux-ui/telas/paineis/garcom-2.png)

Fonte: elaborado pelos autores.

**Figura 13 – Protótipo: Garçom, fechamento da conta e desempenho**

![Protótipo: Garçom, fechamento da conta e desempenho](../../ux-ui/telas/paineis/garcom-3.png)

Fonte: elaborado pelos autores.

**Figura 14 – Protótipo: Metre, alocação do turno e salão**

![Protótipo: Metre, alocação do turno e salão](../../ux-ui/telas/paineis/metre.png)

Fonte: elaborado pelos autores.

**Figura 15 – Protótipo: Gerente, análises e cardápio**

![Protótipo: Gerente, análises e cardápio](../../ux-ui/telas/paineis/gerente-1.png)

Fonte: elaborado pelos autores.

**Figura 16 – Protótipo: Gerente, promoções, salão e usuários**

![Protótipo: Gerente, promoções, salão e usuários](../../ux-ui/telas/paineis/gerente-2.png)

Fonte: elaborado pelos autores.

**Figura 17 – Protótipo: Cliente**

![Protótipo: Cliente](../../ux-ui/telas/paineis/cliente.png)

Fonte: elaborado pelos autores.
"""


def secao_prototipo(documento) -> None:
    corpo = list(documento.element.body)
    capitulo5 = next(i for i, el in enumerate(corpo)
                     if el.tag == qn("w:p") and el.xpath("./w:pPr/w:pStyle/@w:val") == ["Ttulo1"]
                     and texto_de(el).startswith("5 "))
    i = capitulo5 - 1
    while i > 0 and corpo[i].tag == qn("w:p") and (not texto_de(corpo[i]).strip()) \
            and not corpo[i].xpath(".//w:drawing"):
        i -= 1
    escrever(documento, corpo[i], blocos(PROTOTIPO), PASTA)


def main() -> None:
    documento = Document(str(DOCX))
    ajustes_de_texto(documento)
    print("referências com destaque:", negrito_nas_referencias(documento))
    secao_prototipo(documento)
    capa_e_folha_de_rosto(documento)
    paginas_pre_textuais(documento)
    documento.save(str(DOCX))
    print("Gravado. No Word: Ctrl+A e F9.")


if __name__ == "__main__":
    main()
