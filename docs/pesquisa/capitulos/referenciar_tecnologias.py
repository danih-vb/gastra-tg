"""Revisão pedida pela dupla em 21/11/2026: da Introdução em diante, a primeira vez que aparece o nome de uma
tecnologia, de um algoritmo ou de um método estatístico mais avançado, ela vem com referência. Linguagem, framework
e ferramenta citam o site oficial; método estatístico cita livro ou artigo.

    pip install python-docx
    python docs/pesquisa/capitulos/referenciar_tecnologias.py

Mexe só nos capítulos 1 a 4 do Gastra II.docx (o capítulo 5 vem do Markdown) e nas referências. Depois, no Word:
Ctrl+A e F9. Rodar uma segunda vez não duplica nada: cada troca confere se a citação já está lá.
"""

import copy
import re
import unicodedata
from pathlib import Path

from docx import Document
from docx.oxml.ns import qn

DOCX = Path(__file__).resolve().parent.parent / "Gastra II.docx"
ACESSO = "Acesso em: 21 nov. 2026."

# (início do parágrafo, trecho atual, trecho novo). O início identifica o parágrafo; o trecho atual precisa
# aparecer uma única vez nele.
TROCAS = [
    ("Restaurantes produzem diariamente", "Business Intelligence (BI),",
     "Business Intelligence (BI) (SANTOS; GIBERTONI, 2022),"),
    ("Desenvolver o GASTRA", "distribuição de garçons entre praças por programação linear,",
     "distribuição de garçons entre praças por programação linear (HILLIER; LIEBERMAN, 2006),"),
    ("Desenvolver a aplicação web", "backend em ASP.NET Core e frontend em Angular",
     "backend em ASP.NET Core (MICROSOFT, 2026a) e frontend em Angular (GOOGLE, 2026)"),
    ("Aplicar técnicas de ciência de dados", "(clusterização e regras de associação)",
     "(clusterização e regras de associação) (TAN; STEINBACH; KUMAR, 2006)"),
    ("O desenvolvimento do GASTRA exige", "para o tratamento de dados sob a LGPD.",
     "para o tratamento de dados sob a LGPD (BRASIL, 2018)."),
    ("Do ponto de vista técnico", "como arquitetura em camadas com",
     "como arquitetura em camadas (PRESSMAN; MAXIM, 2011) com"),
    ("Do ponto de vista técnico", "o backend em C#/.NET e o serviço analítico em Python — comunicando-se por HTTP.",
     "o backend em C# (MICROSOFT, 2026d) sobre a plataforma .NET (MICROSOFT, 2026c) e o serviço analítico em Python "
     "(PYTHON SOFTWARE FOUNDATION, 2026) — comunicando-se por HTTP (FIELDING; NOTTINGHAM; RESCHKE, 2022)."),
    ("Do ponto de vista técnico", "do ecossistema Python (mlxtend, scikit-learn, PuLP) sem",
     "do ecossistema Python, como mlxtend (RASCHKA, 2018), scikit-learn (PEDREGOSA et al., 2011) e PuLP "
     "(MITCHELL; O'SULLIVAN; DUNNING, 2011), sem"),
    ("A modelagem e a implementação do banco", "migrations do Entity Framework Core e",
     "migrations do Entity Framework Core (MICROSOFT, 2026b) e"),
    ("A validação do sistema ocorreu", "a silhueta da segmentação",
     "a silhueta da segmentação (ROUSSEEUW, 1987)"),
    ("Este trabalho está organizado", "Business Model Canvas", "Business Model Canvas (STRATEGYZER, [20--])"),
    ("Este trabalho está organizado", "a modelagem do sistema por casos de uso,",
     "a modelagem do sistema por casos de uso (JACOBSON et al., 1992),"),
    ("Este trabalho está organizado", "Modelo de Entidade-Relacionamento",
     "Modelo de Entidade-Relacionamento (CHEN, 1976)"),
    ("O frontend foi desenvolvido em Angular", "Por usar TypeScript,", "Por usar TypeScript (MICROSOFT, 2026e),"),
    ("Os quatro componentes — frontend", "sem instalar .NET, Node.js ou Python.",
     "sem instalar .NET, Node.js (OPENJS FOUNDATION, 2026) ou Python."),
    ("A autenticação utiliza o JSON Web Token", "o código de um aplicativo autenticador",
     "o código de um aplicativo autenticador, gerado pelo algoritmo TOTP (M'RAIHI et al., 2011)"),
    ("A recomendação de pratos do GASTRA", "ou diminui (abaixo de 1) a chance de levar B.",
     "ou diminui (abaixo de 1) a chance de levar B (TAN; STEINBACH; KUMAR, 2006)."),
    ("Já o trabalho de Moreira (2018)", "via leitura de QR Code em sua mesa",
     "via leitura de QR Code (INTERNATIONAL ORGANIZATION FOR STANDARDIZATION, 2015) em sua mesa"),
    ("A stack do GASTRA", "serviços gerenciados da Microsoft Azure,",
     "serviços gerenciados da Microsoft Azure (MICROSOFT, 2026f),"),
    ("A stack do GASTRA", "O certificado SSL para conexões seguras",
     "O certificado SSL/TLS (RESCORLA, 2018) para conexões seguras"),
    ("Ferramentas de desenvolvimento e modelagem",
     "(Visual Studio Community, VS Code, brModelo, draw.io/Lucidchart)",
     "— Visual Studio Community (MICROSOFT, 2026g), VS Code (MICROSOFT, 2026h), brModelo (BRMODELO, 2026), "
     "draw.io (JGRAPH, 2026) e Lucidchart (LUCID SOFTWARE, 2026) —"),
    ("Este capítulo apresenta os artefatos", "as histórias de usuário que os detalham",
     "as histórias de usuário (COHN, 2004) que os detalham"),
    ("Os requisitos funcionais foram detalhados", "na notação da UML",
     "na notação da UML (OBJECT MANAGEMENT GROUP, 2017)"),
    ("A Figura 7 apresenta o diagrama de implantação", "com o servidor nginx,",
     "com o servidor nginx (NGINX, 2026),"),
    ("A segunda técnica é a clusterização", "que elas compartilham (similaridade do cosseno), e não",
     "que elas compartilham, a chamada similaridade do cosseno (TAN; STEINBACH; KUMAR, 2006), e não"),
    ("A segunda técnica é a clusterização", "considera-se que não há perfis de fato.",
     "considera-se que não há perfis de fato (KAUFMAN; ROUSSEEUW, 1990)."),
    ("Antes das telas definitivas", "protótipo navegável em HTML,", "protótipo navegável em HTML (WHATWG, 2026),"),
    ("Antes das telas definitivas", "importadas no Figma.", "importadas no Figma (FIGMA, 2026)."),
]

# (autor, título em negrito, resto). Entram em ordem alfabética; as da Microsoft vão depois da 2026b, na ordem
# das letras.
NOVAS = [
    ("BRMODELO. ", "brModelo", f". [S. l.], 2026. Disponível em: https://www.brmodeloweb.com/. {ACESSO}"),
    ("CHEN, Peter Pin-Shan. The entity-relationship model: toward a unified view of data. ",
     "ACM Transactions on Database Systems", ", New York, v. 1, n. 1, p. 9-36, 1976."),
    ("COHN, Mike. ", "User stories applied", ": for agile software development. Boston: Addison-Wesley, 2004."),
    ("FIELDING, Roy T.; NOTTINGHAM, Mark; RESCHKE, Julian (ed.). ", "HTTP semantics",
     f". RFC 9110. [S. l.]: Internet Engineering Task Force, 2022. Disponível em: "
     f"https://www.rfc-editor.org/rfc/rfc9110. {ACESSO}"),
    ("FIGMA. ", "Figma", f". [S. l.], 2026. Disponível em: https://www.figma.com/. {ACESSO}"),
    ("INTERNATIONAL ORGANIZATION FOR STANDARDIZATION. ", "ISO/IEC 18004",
     ": information technology: automatic identification and data capture techniques: QR Code bar code symbology "
     "specification. 3. ed. Geneva: ISO, 2015."),
    ("JACOBSON, Ivar et al. ", "Object-oriented software engineering",
     ": a use case driven approach. Wokingham: Addison-Wesley, 1992."),
    ("JGRAPH. ", "draw.io", f". [S. l.], 2026. Disponível em: https://www.drawio.com/. {ACESSO}"),
    ("KAUFMAN, Leonard; ROUSSEEUW, Peter J. ", "Finding groups in data",
     ": an introduction to cluster analysis. New York: John Wiley & Sons, 1990."),
    ("LUCID SOFTWARE. ", "Lucidchart", f". [S. l.], 2026. Disponível em: https://www.lucidchart.com/. {ACESSO}"),
    ("M'RAIHI, David et al. ", "TOTP",
     f": time-based one-time password algorithm. RFC 6238. [S. l.]: Internet Engineering Task Force, 2011. "
     f"Disponível em: https://www.rfc-editor.org/rfc/rfc6238. {ACESSO}"),
    ("NGINX. ", "nginx documentation", f". [S. l.], 2026. Disponível em: https://nginx.org/en/docs/. {ACESSO}"),
    ("OBJECT MANAGEMENT GROUP. ", "OMG Unified Modeling Language (OMG UML)",
     f": version 2.5.1. Needham: OMG, 2017. Disponível em: https://www.omg.org/spec/UML/2.5.1/. {ACESSO}"),
    ("OPENJS FOUNDATION. ", "Node.js", f". [S. l.], 2026. Disponível em: https://nodejs.org/. {ACESSO}"),
    ("PYTHON SOFTWARE FOUNDATION. ", "Python", f". [S. l.], 2026. Disponível em: https://www.python.org/. {ACESSO}"),
    ("RESCORLA, Eric. ", "The Transport Layer Security (TLS) protocol version 1.3",
     f". RFC 8446. [S. l.]: Internet Engineering Task Force, 2018. Disponível em: "
     f"https://www.rfc-editor.org/rfc/rfc8446. {ACESSO}"),
    ("TAN, Pang-Ning; STEINBACH, Michael; KUMAR, Vipin. ", "Introduction to data mining",
     ". Boston: Pearson Addison Wesley, 2006."),
    ("WHATWG. ", "HTML living standard", f". [S. l.], 2026. Disponível em: https://html.spec.whatwg.org/. {ACESSO}"),
]
MICROSOFT = [
    (".NET documentation", "2026c", "https://learn.microsoft.com/dotnet/"),
    ("C# documentation", "2026d", "https://learn.microsoft.com/dotnet/csharp/"),
    ("TypeScript", "2026e", "https://www.typescriptlang.org/"),
    ("Azure documentation", "2026f", "https://learn.microsoft.com/azure/"),
    ("Visual Studio", "2026g", "https://visualstudio.microsoft.com/"),
    ("Visual Studio Code", "2026h", "https://code.visualstudio.com/"),
]


def chave(texto: str) -> str:
    sem_acento = unicodedata.normalize("NFD", texto).encode("ascii", "ignore").decode()
    return re.sub(r"[^A-Z0-9 ]", "", sem_acento.upper())


def trocar_no_paragrafo(paragrafo, atual: str, novo: str) -> bool:
    texto = paragrafo.text
    if novo in texto:
        return False
    assert texto.count(atual) == 1, (texto[:60], atual)
    inicio = texto.index(atual)
    fim = inicio + len(atual)
    posicao = 0
    primeiro = None
    for run in paragrafo.runs:
        a, b = posicao, posicao + len(run.text)
        posicao = b
        if b <= inicio or a >= fim:
            continue
        dentro_ini, dentro_fim = max(inicio, a) - a, min(fim, b) - a
        if primeiro is None:
            primeiro = run
            run.text = run.text[:dentro_ini] + novo + run.text[dentro_fim:]
        else:
            run.text = run.text[:dentro_ini] + run.text[dentro_fim:]
    assert paragrafo.text == texto.replace(atual, novo), paragrafo.text[:80]
    return True


def nova_referencia(molde, autor: str, titulo: str, resto: str):
    novo = copy.deepcopy(molde._p)
    runs = novo.findall(qn("w:r"))
    for run in runs:
        novo.remove(run)
    for texto, negrito in ((autor, False), (titulo, True), (resto, False)):
        run = copy.deepcopy(runs[0])
        for filho in list(run):
            if filho.tag != qn("w:rPr"):
                run.remove(filho)
        rpr = run.find(qn("w:rPr"))
        if rpr is None:
            rpr = run.makeelement(qn("w:rPr"), {})
            run.insert(0, rpr)
        for b in rpr.findall(qn("w:b")):
            rpr.remove(b)
        if negrito:
            rpr.append(rpr.makeelement(qn("w:b"), {}))
        t = run.makeelement(qn("w:t"), {qn("xml:space"): "preserve"})
        t.text = texto
        run.append(t)
        novo.append(run)
    return novo


def main() -> None:
    documento = Document(str(DOCX))
    paragrafos = documento.paragraphs
    inicio = next(i for i, p in enumerate(paragrafos) if p.text.strip() == "1 INTRODUÇÃO")
    cap5 = next(i for i, p in enumerate(paragrafos) if p.text.strip().startswith("5 DESENVOLVIMENTO"))
    corpo = paragrafos[inicio:cap5]

    trocadas = 0
    for comeco, atual, novo in TROCAS:
        alvo = [p for p in corpo if p.text.startswith(comeco)]
        assert len(alvo) == 1, (comeco, len(alvo))
        trocadas += trocar_no_paragrafo(alvo[0], atual, novo)

    # A entrada é "DOCKER INC.": a citação tem de usar o mesmo nome.
    for p in paragrafos[inicio:]:
        if "(DOCKER, 2026)" in p.text:
            trocar_no_paragrafo(p, "(DOCKER, 2026)", "(DOCKER INC., 2026)")

    titulo = next(i for i, p in enumerate(paragrafos) if p.text.strip() == "REFERÊNCIAS" and i > inicio)
    referencias = [p for p in paragrafos[titulo + 1:] if p.text.strip()]
    molde = next(p for p in referencias if p.text.startswith("JONES"))
    existentes = {chave(p.text)[:40] for p in referencias}

    acrescentadas = 0
    for autor, titulo_ref, resto in NOVAS:
        if chave(autor + titulo_ref)[:40] in existentes:
            continue
        seguinte = next((p for p in referencias if chave(p.text) > chave(autor + titulo_ref)), None)
        elemento = nova_referencia(molde, autor, titulo_ref, resto)
        if seguinte is None:
            referencias[-1]._p.addnext(elemento)
        else:
            seguinte._p.addprevious(elemento)
        acrescentadas += 1

    ultima = next(p for p in referencias if p.text.startswith("MICROSOFT. Entity Framework Core"))._p
    for titulo_ref, ano, url in MICROSOFT:
        if any(p.text.startswith(f"MICROSOFT. {titulo_ref}. [S. l.], {ano}") for p in referencias):
            continue
        elemento = nova_referencia(molde, "MICROSOFT. ", titulo_ref,
                                   f". [S. l.], {ano}. Disponível em: {url}. {ACESSO}")
        ultima.addnext(elemento)
        ultima = elemento
        acrescentadas += 1

    documento.save(str(DOCX))
    print(f"{trocadas} citações inseridas, {acrescentadas} referências novas. No Word: Ctrl+A e F9.")


if __name__ == "__main__":
    main()
