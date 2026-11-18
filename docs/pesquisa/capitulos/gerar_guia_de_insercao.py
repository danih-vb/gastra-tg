"""Gera o guia de inserção do TG II e atualiza o documento de referências, sem tocar no `Gastra II.docx`.

    pip install python-docx
    python docs/pesquisa/capitulos/gerar_guia_de_insercao.py

Saídas:
- `docs/pesquisa/GASTRA_TGII_Guia_de_Insercao.docx`: o que falta no documento principal, conferido contra o modelo
  oficial da Fatec, com o texto pronto de cada parte e a instrução de onde e como colocar;
- `docs/pesquisa/referencias/GASTRA_Referencias_Bibliograficas.docx`: a lista completa de referências, em ordem.

O guia nasce dos estilos do próprio `Gastra II.docx` (Título 2, Título 3, Legenda, Normal): o que for colado no
documento principal chega com a mesma formatação, e as legendas continuam com numeração automática.
"""

from __future__ import annotations

import copy
import re
import unicodedata
from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor

from levar_para_docx import blocos, escrever, estilo, sem_recuo, texto_com_marcas

PASTA = Path(__file__).resolve().parent
PESQUISA = PASTA.parent
PRINCIPAL = PESQUISA / "Gastra II.docx"
SAIDA = PESQUISA / "GASTRA_TGII_Guia_de_Insercao.docx"
REFERENCIAS = PESQUISA / "referencias" / "GASTRA_Referencias_Bibliograficas.docx"
REVISOES = PASTA / "Revisoes_Capitulos_1_a_3.md"
MANUAL = PESQUISA.parent / "manual-usuario" / "GASTRA_Manual_do_Usuario.md"
CAPITULOS = [
    ("4", "ENGENHARIA DE SOFTWARE E PROJETO DO SISTEMA", PASTA / "Cap4_Engenharia_de_Software.md"),
    ("5", "DESENVOLVIMENTO, RESULTADOS E TESTES", PASTA / "Cap5_Desenvolvimento_Resultados_Testes.md"),
    ("6", "CONSIDERAÇÕES FINAIS", PASTA / "Cap6_Consideracoes_Finais.md"),
]
CITACAO = re.compile(r" \([A-ZÁÉÍÓÚÂÊÔÃÕÇ][^()]*?\d{4}[a-z]?\)")


# ---------------------------------------------------------------- leitura do documento principal

def secoes_do_principal() -> dict[str, list[str]]:
    """Texto dos parágrafos de cada seção do documento principal, pelo título."""
    documento = Document(str(PRINCIPAL))
    secoes: dict[str, list[str]] = {}
    atual = None
    for p in documento.paragraphs:
        texto = p.text.strip()
        if p.style.name.startswith("Heading"):
            atual = texto
            secoes[atual] = []
        elif atual and texto:
            secoes[atual].append(texto)
    return secoes


def secao(secoes: dict[str, list[str]], inicio: str) -> list[str]:
    return next(v for k, v in secoes.items() if k.startswith(inicio))


# ---------------------------------------------------------------- leitura das revisões em Markdown

def partes_das_revisoes() -> dict[str, str]:
    texto = REVISOES.read_text(encoding="utf-8")
    partes = re.split(r"(?m)^(?=#{2,3} )", texto)
    return {p.split("\n", 1)[0].lstrip("#").strip(): p.split("\n", 1)[1] if "\n" in p else "" for p in partes[1:]}


def paragrafos_md(texto: str) -> list[str]:
    return [" ".join(l.strip() for l in bloco.splitlines()) for bloco in re.split(r"\n\s*\n", texto.strip()) if bloco.strip()]


def sem_marcas(texto: str) -> str:
    return re.sub(r"^\*\*\[[^\]]*\]\*\*\s*", "", texto).strip()


def mesclar(paragrafos_revisados: list[str], originais: list[str]) -> list[str]:
    """Junta o revisado com o original: parágrafo "sem alteração" vem do documento; "resto sem alteração" é
    completado com o final do parágrafo original."""
    saida = []
    for paragrafo in paragrafos_revisados:
        paragrafo = sem_marcas(paragrafo)
        if "resto do parágrafo sem alteração" in paragrafo:
            novo = paragrafo.split("*(")[0].rstrip()
            sem_citacoes = CITACAO.sub("", novo)
            ancora = sem_citacoes.rstrip(" .")[-25:]
            # O parágrafo certo é o que começa igual: o mesmo final de frase pode aparecer em mais de um.
            original = next(o for o in originais if o.startswith(sem_citacoes[:30]))
            assert ancora in original, ancora
            resto = original[original.index(ancora) + len(ancora):]
            saida.append((novo.rstrip(" .") + resto) if not novo.endswith(".") else novo + resto.split(".", 1)[1])
        elif "parágrafo sem alteração" in paragrafo:
            inicio = paragrafo.split("*(")[0].strip()
            saida.append(next(o for o in originais if o.startswith(inicio)))
        else:
            saida.append(paragrafo)
    return saida


# ---------------------------------------------------------------- escrita do guia

class Guia:
    def __init__(self) -> None:
        self.documento = Document(str(PRINCIPAL))
        corpo = self.documento.element.body
        for filho in list(corpo):
            if filho.tag != qn("w:sectPr"):
                corpo.remove(filho)
        self.ancora = self.documento.add_paragraph()._p

    def _novo(self):
        p = self.documento.add_paragraph()
        self.ancora.addnext(p._p)
        self.ancora = p._p
        return p

    def quebra(self) -> None:
        self._novo().add_run().add_break(WD_BREAK.PAGE)

    def parte(self, titulo: str) -> None:
        p = self._novo()
        estilo(p, "Ttulo1")
        p.add_run(titulo)

    def subtitulo(self, titulo: str) -> None:
        p = self._novo()
        estilo(p, "Ttulo2")
        p.add_run(titulo)

    def instrucao(self, texto: str, rotulo: str = "COMO INSERIR") -> None:
        """Caixa amarela: orientação para quem monta o documento. Não vai para o documento principal."""
        p = self._novo()
        sem_recuo(p, WD_ALIGN_PARAGRAPH.LEFT)
        p.paragraph_format.line_spacing = 1.15
        p.paragraph_format.space_before = Pt(6)
        p.paragraph_format.space_after = Pt(6)
        ppr = p._p.get_or_add_pPr()
        sombra = OxmlElement("w:shd")
        sombra.set(qn("w:val"), "clear")
        sombra.set(qn("w:fill"), "FFF2CC")
        ppr.append(sombra)
        bordas = OxmlElement("w:pBdr")
        for lado in ("top", "left", "bottom", "right"):
            borda = OxmlElement(f"w:{lado}")
            borda.set(qn("w:val"), "single")
            borda.set(qn("w:sz"), "6")
            borda.set(qn("w:space"), "4")
            borda.set(qn("w:color"), "BF9000")
            bordas.append(borda)
        ppr.append(bordas)
        cabeca = p.add_run(f"{rotulo}: ")
        cabeca.bold = True
        cabeca.font.color.rgb = RGBColor(0x7F, 0x60, 0x00)
        texto_com_marcas(p, texto, tamanho=Pt(10))
        cabeca.font.size = Pt(10)

    def texto(self, conteudo: str, centralizado: bool = False, negrito: bool = False, sem_recuo_inicial: bool = False) -> None:
        p = self._novo()
        if centralizado:
            sem_recuo(p)
        elif sem_recuo_inicial:
            sem_recuo(p, WD_ALIGN_PARAGRAPH.JUSTIFY)
        texto_com_marcas(p, f"**{conteudo}**" if negrito else conteudo)

    def citacao_longa(self, conteudo: str) -> None:
        """Citação direta com mais de três linhas (ABNT NBR 10520): recuo de 4 cm, fonte 10, espaçamento simples."""
        p = self._novo()
        sem_recuo(p, WD_ALIGN_PARAGRAPH.JUSTIFY)
        p.paragraph_format.left_indent = Cm(4)
        p.paragraph_format.line_spacing = 1.0
        p.paragraph_format.space_before = Pt(12)
        p.paragraph_format.space_after = Pt(12)
        texto_com_marcas(p, conteudo, tamanho=Pt(10))

    def markdown(self, conteudo: str, pasta: Path) -> None:
        self.ancora = escrever(self.documento, self.ancora, blocos(conteudo), pasta)

    def tabela(self, linhas: list[list[str]], larguras_cm: list[float]) -> None:
        tabela = self.documento.add_table(rows=len(linhas), cols=len(linhas[0]))
        tabela.style = self.documento.styles["Table Grid"]
        tabela.autofit = False
        for i, linha in enumerate(linhas):
            for j, celula_texto in enumerate(linha):
                celula = tabela.cell(i, j)
                celula.width = Cm(larguras_cm[j])
                p = celula.paragraphs[0]
                sem_recuo(p, WD_ALIGN_PARAGRAPH.LEFT)
                p.paragraph_format.line_spacing = 1.0
                texto_com_marcas(p, f"**{celula_texto}**" if i == 0 else celula_texto, tamanho=Pt(9))
        self.ancora.addnext(tabela._tbl)
        self.ancora = tabela._tbl
        self._novo()

    def salvar(self) -> None:
        self.documento.save(str(SAIDA))


# ---------------------------------------------------------------- conteúdo

SITUACAO = [
    ["Elemento do modelo oficial", "Situação no Gastra II.docx", "O que fazer", "Parte deste guia"],
    ["Capa", "Não há capa separada: a primeira página mistura capa e folha de rosto", "Criar a capa como no modelo", "3.1"],
    ["Folha de rosto", "Texto do TG I (\"Projeto de pesquisa…\")", "Trocar pelo texto do modelo", "3.2"],
    ["Dedicatória e epígrafe", "Não há", "Opcionais: a dupla decide", "—"],
    ["Agradecimentos", "Não há", "Incluir (o modelo não marca como opcional)", "3.3"],
    ["Resumo e palavras-chave", "Não há", "Incluir", "3.4"],
    ["Abstract e keywords", "Não há", "Incluir", "3.5"],
    ["Listas de ilustrações e de tabelas", "Existem, só com o capítulo 3", "Atualizar no Word (F9) depois de inserir; incluir os quadros", "8"],
    ["Lista de abreviaturas e siglas", "Existe, com 10 siglas", "Substituir pela lista completa", "3.6"],
    ["Sumário", "Existe (campo automático)", "Atualizar no Word (F9)", "8"],
    ["1.1 a 1.4", "Prontas", "Nada", "—"],
    ["1.5 Metodologia", "Desatualizada (sem integração contínua, prototipação e Nielsen)", "Substituir a seção", "4.1"],
    ["1.6 Estrutura do trabalho", "Cita o título antigo do capítulo 4", "Substituir o parágrafo", "4.2"],
    ["2.1 Conceitos e regras de negócio", "Pronta", "Nada", "—"],
    ["2.2 Tecnologias e arquitetura", "Sem as fontes oficiais; sem Docker, JWT e OWASP; sem os fundamentos dos blocos", "Substituir a seção", "4.3"],
    ["2.3 Trabalhos correlatos", "Saipos citado sem referência", "Uma correção", "4.4"],
    ["3.1 Canvas", "Ano do Strategyzer diferente da referência", "Uma correção", "4.4"],
    ["3.2 Viabilidade e custos", "Linha colada do navegador no meio do texto", "Apagar a linha", "4.4"],
    ["4 Engenharia de software", "Só o título", "Inserir o capítulo", "5"],
    ["5 Desenvolvimento, resultados e testes", "Só o título", "Inserir o capítulo; faltam os testes operacionais, o Nielsen e as telas reais", "6"],
    ["6 Considerações finais", "Só o título", "Inserir o capítulo; a dupla reescreve com a própria voz", "7"],
    ["Referências", "23 obras; faltam as citadas nos capítulos 2, 4 e 5", "Substituir a lista completa (parte 9)", "9"],
    ["Apêndice A – Manual do usuário", "Não há", "Inserir", "10"],
]

ENTREGAS = [
    ["Etapa", "Prazo", "Entrega do cronograma", "Situação", "Onde está"],
    ["1", "26/08 a 13/09", "Documentação inicial atualizada", "Feita", "Capítulos 1 a 3 do Gastra II.docx; as correções que faltam estão na parte 4"],
    ["2", "14/09 a 04/10", "Projeto de software", "Feita no repositório; falta passar ao documento", "Capítulo 4 (parte 5); diagramas em docs/diagramas e docs/modelagem"],
    ["3", "05/10 a 25/10", "API e banco de dados funcionais", "Feita", "backend/ e infra/; 325 testes do backend aprovados"],
    ["4", "26/10 a 15/11", "Sistema concluído, com testes operacionais", "Sistema pronto; faltam os testes operacionais e o Nielsen", "docs/testes/GASTRA_Plano_Testes_Operacionais.md e docs/ux-ui/GASTRA_Avaliacao_Heuristica.md"],
    ["5", "16/11 a 22/11", "Minuta completa", "Texto pronto neste guia, menos as seções que dependem da etapa 4", "Partes 3 a 10"],
    ["6", "23/11 a 28/11", "Arquivo final (ABNT e orientador)", "A fazer", "Parte 8 (lista de conferência)"],
    ["7", "29/11", "Documentação final e código-fonte", "A fazer", "Parte 11"],
    ["8", "30/11 a 05/12", "Banca", "A fazer", "—"],
]

PENDENCIAS = [
    "**Testes operacionais (seção 5.2.6):** executar os 43 passos de `docs/testes/GASTRA_Plano_Testes_Operacionais.md` e me passar o resultado; eu escrevo o texto da seção.",
    "**Avaliação heurística (seção 5.2.5):** decidir quem assina (a dupla, com a pré-avaliação feita por IA, ou declarar o uso de IA) e concluir o registro em `docs/ux-ui/GASTRA_Avaliacao_Heuristica.md`.",
    "**Telas reais (seção 5.1):** as figuras 11 a 14 ainda são do protótipo. Capturar as telas do sistema rodando, com os dados semeados, e trocar as imagens mantendo as legendas.",
    "**Resumo e abstract:** acrescentar uma frase com o resultado dos testes operacionais e do Nielsen.",
    "**Capítulo 6:** reescrever com a voz da dupla, principalmente a seção 6.4.",
    "**Agradecimentos:** o texto da parte 3.3 é um rascunho.",
    "**Funções novas (PRs #252 a #255):** item atrasado, aviso de restrição, dividir a conta, engenharia de cardápio e chamar o garçom ainda não estão nos capítulos. Depois do merge, citar na seção 5.1 e registrar o chamado do garçom como requisito novo (RF, caso de uso, matriz e DER).",
    "**Números dos testes:** os capítulos dizem 513 casos. Regenerar o relatório (`docs/testes/gerar_relatorio_testes.py`) depois dos últimos merges e trocar o número no resumo, no capítulo 5 e no capítulo 6.",
    "**Referência de Kasavana e Smith (1982):** conferir a cidade da editora.",
    "**CSV da pesquisa:** versionar o CSV anonimizado do questionário, como pede `docs/requisitos/GASTRA_Numeros_da_Pesquisa.md`, para os números da pesquisa poderem ser conferidos.",
]

CONFERENCIA_ABNT = [
    "Texto em terceira pessoa (\"desenvolve-se\", \"analisa-se\"), como pede o guia de orientação.",
    "Fonte 12 no texto e espaçamento 1,5; citações longas, legendas, fontes e notas em fonte menor e espaçamento simples.",
    "Margens: superior e esquerda 3 cm, inferior e direita 2 cm (o documento já está assim).",
    "Legenda acima de figuras, quadros e tabelas, no formato \"Figura 1 – Título\" (travessão, como no modelo); fonte abaixo. As legendas do capítulo 3 ainda usam hífen (\"Figura 1 - \"): trocar pelo travessão.",
    "Números de página no canto superior direito, contados desde a folha de rosto e mostrados a partir da introdução.",
    "Sumário, listas de ilustrações e de tabelas atualizados: no Word, Ctrl+A e F9. Os quadros (Quadro 1 a 7) entram na lista de ilustrações: em Referências > Inserir Índice de Ilustrações, escolher o rótulo \"Quadro\" e colar a lista gerada logo abaixo das figuras.",
    "Toda citação no texto tem a obra nas referências, e toda referência é citada no texto. Isso foi conferido em 30/10; conferir de novo depois de colar.",
    "Nenhum trecho marcado em amarelo, [A PREENCHER] ou [SUBSTITUIR] pode sobrar no documento final.",
    "Siglas da lista aparecem no texto; as que não aparecem saem da lista (conferir CI/CD, IA, SaaS e SSL).",
]

ENTREGA_FINAL = [
    "**Código-fonte:** gerar a versão de entrega a partir da branch `main`, por um release (GitFlow: `release/1.0` saindo de `dev`, revisada e integrada em `main`), e criar a tag `v1.0`. O GitHub gera o .zip da tag, que pode ir para a coordenação.",
    "**Documento:** exportar o Gastra II.docx para PDF depois do F9, conferir as listas e o sumário no PDF, e entregar os dois arquivos.",
    "**Banca:** o roteiro de demonstração é o mesmo dos testes operacionais; o ambiente sobe com `infra/resetar-ambiente.sh`.",
]


def manual_em_markdown() -> str:
    """O manual como apêndice: sem o sumário próprio, imagens com legenda e fonte, numeração A.1, A.2…"""
    texto = MANUAL.read_text(encoding="utf-8")
    texto = re.sub(r"(?ms)^## Sumário.*?(?=^## )", "", texto)
    texto = re.sub(r"(?m)^# .*$", "", texto)
    texto = re.sub(r"(?m)^## (\d+)\. ", r"## A.\1 ", texto)
    texto = re.sub(r"(?m)^### (\d+)\.(\d+) ", r"### A.\1.\2 ", texto)
    texto = re.sub(r"\[([^\]]+)\]\((?:https?://|\.\./|#)[^)]*\)", r"\1", texto)
    texto = re.sub(r"(?m)^> ?", "", texto)
    texto = re.sub(r"(?m)^---\s*$", "", texto)
    return texto


def imagens_com_legenda(texto: str, primeira: int) -> str:
    numero = primeira

    def troca(m):
        nonlocal numero
        alt, caminho = m.group(2), m.group(1)
        bloco = f"\n**Figura {numero} – {alt}**\n\n![{alt}]({caminho})\n\nFonte: elaborado pelos autores.\n"
        numero += 1
        return bloco

    return re.sub(r'<img src="([^"]+)" alt="([^"]*)"[^>]*>', troca, texto)


def chave_de_ordem(referencia: str) -> str:
    return "".join(c for c in unicodedata.normalize("NFD", referencia.upper()) if unicodedata.category(c) != "Mn")


def referencias_completas(secoes, revisoes) -> list[str]:
    existentes = secao(secoes, "REFER")
    novas = [r for r in paragrafos_md(revisoes["Referências novas"]) if re.match(r"^[A-Z]{2,}", r)]
    todas = [r.replace("[ano não informado]", "[20--]") for r in existentes] + novas
    unicas = list(dict.fromkeys(todas))
    return sorted(unicas, key=chave_de_ordem)


def siglas_completas(secoes, revisoes) -> list[str]:
    existentes = secao(secoes, "LISTA DE ABREVIATURAS")
    texto = paragrafos_md(revisoes["LISTA DE ABREVIATURAS E SIGLAS **[alterado: siglas que os capítulos 4 a 6 passaram a usar]**"])[1]
    novas = [s.strip().rstrip(".") for s in texto.split(";")]
    return sorted(dict.fromkeys(existentes + novas), key=lambda s: chave_de_ordem(s.split("–")[0]))


def montar() -> None:
    secoes = secoes_do_principal()
    revisoes = partes_das_revisoes()
    guia = Guia()

    # Capa do guia
    guia.texto("GASTRA – GUIA DE INSERÇÃO NO DOCUMENTO DO TG II", centralizado=True, negrito=True)
    guia.texto("O que falta no Gastra II.docx, o texto pronto de cada parte e onde colocar", centralizado=True)
    guia.instrucao(
        "Este guia não substitui o Gastra II.docx: ele diz o que colar e onde. As caixas amarelas como esta são "
        "instruções e **não vão para o documento**. Todo o resto foi gerado com os estilos do próprio Gastra II.docx: "
        "selecione o trecho, copie e cole no lugar indicado, e ele chega com a mesma formatação. Os títulos com número "
        "(como 4.1 ou 5.2.3) já vêm com o estilo de título do documento. As legendas de figura, quadro e tabela vêm com "
        "numeração automática: os números que aparecem aqui não são os finais, porque o guia não tem as figuras do "
        "capítulo 3 nem a mesma ordem; depois de colar tudo, pressione Ctrl+A e F9 no Gastra II.docx, e números, "
        "sumário e listas se acertam sozinhos. Trechos em amarelo vivo dentro do texto são pendências que dependem da dupla.",
        rotulo="COMO USAR",
    )
    guia.instrucao(
        "Conferido contra os três documentos da disciplina: o Guia de Orientação do TG II, o Modelo de Estrutura do "
        "TG II e o Cronograma de Entregas (Prof. Me. Wdson de Oliveira). Gerado de "
        "`docs/pesquisa/capitulos/gerar_guia_de_insercao.py`, a partir dos capítulos revisados no repositório: se um "
        "capítulo mudar, basta gerar o guia de novo.",
        rotulo="BASE",
    )

    # 1. Situação
    guia.quebra()
    guia.parte("1 SITUAÇÃO DO DOCUMENTO EM RELAÇÃO AO MODELO OFICIAL")
    guia.tabela(SITUACAO, [4.2, 5.0, 4.6, 2.2])

    # 2. Entregas
    guia.parte("2 ENTREGAS DO CRONOGRAMA")
    guia.tabela(ENTREGAS, [1.1, 2.2, 3.8, 4.4, 4.5])

    # 3. Pré-textuais
    guia.quebra()
    guia.parte("3 ELEMENTOS PRÉ-TEXTUAIS")
    guia.subtitulo("3.1 Capa")
    guia.instrucao("Nova primeira página, antes da folha de rosto atual. Cada linha abaixo, centralizada, em maiúsculas, como no modelo.")
    for linha in ("CENTRO PAULA SOUZA", "FACULDADE DE TECNOLOGIA DE ARARAQUARA",
                  "CURSO DE GRADUAÇÃO EM ANÁLISE E DESENVOLVIMENTO DE SISTEMAS", "", "DANIEL VELLUTO BENTO",
                  "PEDRO LUIS OTRENTE DE CAMPOS", "", "GASTRA: SISTEMA DE APOIO À DECISÃO PARA A GESTÃO DE RESTAURANTES À LA CARTE",
                  "", "Araraquara – SP", "2º semestre / 2026"):
        guia.texto(linha, centralizado=True, negrito=linha.startswith("GASTRA"))
    guia.subtitulo("3.2 Folha de rosto")
    guia.instrucao(
        "Na folha de rosto atual: trocar \"GASTRA\" pelo título com subtítulo da capa; trocar o bloco \"Projeto de "
        "pesquisa apresentado…\" pelo texto abaixo (recuado à direita, como já está); trocar \"Araraquara\" por "
        "\"Araraquara – SP\" e \"6º semestre/2026\" por \"2º semestre / 2026\". O título proposto é sugestão: a dupla "
        "pode mudar, desde que capa e folha de rosto fiquem iguais.")
    guia.texto("Trabalho apresentado como exigência parcial para a conclusão do Curso de Graduação em Análise e "
               "Desenvolvimento de Sistemas pela Faculdade de Tecnologia de Araraquara – FATEC Araraquara.", sem_recuo_inicial=True)
    guia.texto("Orientador: Prof. Me. Leonardo José de Lima Ferrucci", sem_recuo_inicial=True)
    guia.subtitulo("3.3 Agradecimentos")
    guia.instrucao("Página própria depois da folha de rosto, com o título AGRADECIMENTOS centralizado. Rascunho: a dupla reescreve com as próprias palavras.")
    agradecimentos = paragrafos_md(revisoes["AGRADECIMENTOS **[novo; o modelo não o marca como opcional]**"])
    guia.texto(sem_marcas(agradecimentos[0]).replace("**[Rascunho para a dupla reescrever com as próprias palavras.]** ", ""))
    for titulo, chave, palavras in (
        ("3.4 Resumo", "RESUMO **[novo, depois dos agradecimentos; parágrafo único, sem recuo]**", "Palavras-chave"),
        ("3.5 Abstract", "ABSTRACT **[novo, em página própria, depois do resumo]**", "Keywords"),
    ):
        guia.subtitulo(titulo)
        guia.instrucao(
            f"Página própria, com o título {titulo.split()[1].upper()} centralizado. Parágrafo único, sem recuo, entre 150 e "
            "500 palavras (o modelo pede esse intervalo). O trecho em amarelo vivo é a frase que falta, com o resultado "
            "dos testes operacionais e do Nielsen.")
        paragrafos = paragrafos_md(revisoes[chave])
        corpo = next(p for p in paragrafos if not p.startswith("**"))
        partes = re.split(r"(\[[^\]]*\])", corpo)
        p = guia._novo()
        sem_recuo(p, WD_ALIGN_PARAGRAPH.JUSTIFY)
        from docx.enum.text import WD_COLOR_INDEX
        for pedaco in partes:
            if pedaco.startswith("["):
                texto_com_marcas(p, pedaco, realce=WD_COLOR_INDEX.YELLOW)
            else:
                texto_com_marcas(p, pedaco)
        linha_palavras = next(p for p in paragrafos if p.startswith(f"**{palavras}:**"))
        guia.texto(linha_palavras, sem_recuo_inicial=True)
    guia.subtitulo("3.6 Lista de abreviaturas e siglas")
    guia.instrucao("Substituir toda a lista atual por esta, que já está em ordem alfabética e inclui as siglas dos capítulos 4 a 6.")
    for sigla in siglas_completas(secoes, revisoes):
        guia.texto(sigla, sem_recuo_inicial=True)

    # 4. Capítulos 1 a 3
    guia.quebra()
    guia.parte("4 CAPÍTULOS 1 A 3: SUBSTITUIÇÕES")
    guia.subtitulo("4.1 Seção 1.5 Metodologia da pesquisa e desenvolvimento")
    guia.instrucao("Apagar todos os parágrafos da seção 1.5, do primeiro até o anterior ao título 1.6, e colar os parágrafos abaixo no lugar. O título 1.5 continua o mesmo.")
    for paragrafo in mesclar(paragrafos_md(revisoes["1.5 METODOLOGIA DA PESQUISA E DESENVOLVIMENTO"]), secao(secoes, "1.5")):
        guia.texto(paragrafo)
    guia.subtitulo("4.2 Seção 1.6 Estrutura do trabalho")
    guia.instrucao("Substituir o parágrafo único da seção 1.6 por este.")
    estrutura = secao(secoes, "1.6")[0]
    for velho, novo in (
        ("a fundamentação teórica das tecnologias e da arquitetura escolhidas, e a comparação do GASTRA com trabalhos correlatos",
         "a fundamentação teórica das tecnologias, da arquitetura e de cada bloco do sistema — Business Intelligence, ciência de dados, programação linear e proteção de dados — e a comparação do GASTRA com trabalhos correlatos"),
        ("Engenharia de Software e Desenvolvimento do Produto", "Engenharia de Software e Projeto do Sistema"),
        ("com as principais telas do sistema, e os procedimentos e resultados dos testes realizados para validação do software",
         "com as telas do sistema em funcionamento, e os testes e a validação do software"),
    ):
        assert velho in estrutura, velho
        estrutura = estrutura.replace(velho, novo)
    guia.texto(estrutura)
    guia.subtitulo("4.3 Seção 2.2 Tecnologias e arquitetura")
    guia.instrucao(
        "Apagar todos os parágrafos da seção 2.2 e colar tudo o que vem abaixo, até a caixa da parte 4.4. Os títulos "
        "2.2.1 a 2.2.4 são novos: o modelo fixa a 2.3 como trabalhos correlatos, por isso os fundamentos de cada bloco "
        "entram como subseções da 2.2. A citação longa da 2.2.4 vai com recuo de 4 cm, fonte 10 e espaçamento simples.")
    trecho = revisoes["2.2 TECNOLOGIAS E ARQUITETURA"]
    for paragrafo in mesclar(paragrafos_md(trecho), secao(secoes, "2.2")):
        guia.texto(paragrafo)
    fundamentos = REVISOES.read_text(encoding="utf-8")
    fundamentos = fundamentos[fundamentos.index("### Fundamentos dos blocos do sistema"):fundamentos.index("## Correções pontuais")]
    fundamentos = fundamentos.split("\n", 1)[1]
    fundamentos = re.sub(r"(?s)^O modelo fixa a 2\.3.*?\n\n", "", fundamentos)
    citacao = re.search(r"(?m)((?:^>.*\n?)+)", fundamentos)
    antes, depois = fundamentos[:citacao.start()], fundamentos[citacao.end():]
    texto_da_citacao = " ".join(l.lstrip("> ").strip() for l in citacao.group(1).splitlines())
    texto_da_citacao = texto_da_citacao.replace("**[citação longa: recuo de 4 cm, fonte menor e espaçamento simples, como no TG I]** ", "")
    guia.markdown(antes, PASTA)
    guia.citacao_longa(texto_da_citacao)
    guia.markdown(depois, PASTA)
    guia.subtitulo("4.4 Correções pontuais nos capítulos 2 e 3")
    for item in re.split(r"\n(?=- )", revisoes["Correções pontuais"].strip()):
        guia.instrucao(" ".join(l.strip() for l in item.lstrip("- ").splitlines()), rotulo="CORRIGIR")

    # 5 a 7. Capítulos 4 a 6
    for numero_parte, (numero, titulo, markdown) in zip(("5", "6", "7"), CAPITULOS):
        guia.quebra()
        guia.parte(f"{numero_parte} CAPÍTULO {numero}: {titulo}")
        guia.instrucao(
            f"O título \"{numero} {titulo}\" já está no Gastra II.docx. Colar todo o conteúdo abaixo logo depois dele, até "
            f"a próxima caixa amarela. Figuras, quadros e tabelas já vêm com legenda e fonte. "
            + ("As figuras 11 a 14 da seção 5.1 são do protótipo e devem ser trocadas pelas telas reais (ver a parte 12). "
               if numero == "5" else "")
            + ("Este capítulo é um rascunho: a dupla reescreve com a própria voz. " if numero == "6" else ""))
        guia.markdown(markdown.read_text(encoding="utf-8"), markdown.parent)

    # 8. Conferência ABNT
    guia.quebra()
    guia.parte("8 CONFERÊNCIA FINAL (ABNT E MODELO)")
    guia.instrucao("Lista para a etapa 6 do cronograma (23/11 a 28/11), depois de colar tudo.", rotulo="QUANDO")
    for item in CONFERENCIA_ABNT:
        guia.markdown(f"- {item}", PASTA)

    # 9. Referências
    guia.quebra()
    guia.parte("9 REFERÊNCIAS")
    guia.instrucao(
        f"Substituir toda a lista de REFERÊNCIAS do Gastra II.docx por esta: {len(referencias_completas(secoes, revisoes))} obras em ordem alfabética, ABNT NBR 6023, "
        "alinhadas à esquerda, espaçamento simples e uma linha em branco entre elas. A mesma lista está em "
        "`docs/pesquisa/referencias/GASTRA_Referencias_Bibliograficas.docx`.")
    for referencia in referencias_completas(secoes, revisoes):
        p = guia._novo()
        sem_recuo(p, WD_ALIGN_PARAGRAPH.LEFT)
        p.paragraph_format.line_spacing = 1.0
        p.paragraph_format.space_after = Pt(12)
        texto_com_marcas(p, referencia)

    # 10. Apêndice
    guia.quebra()
    guia.parte("10 APÊNDICE A – MANUAL DO USUÁRIO")
    guia.instrucao(
        "Depois das REFERÊNCIAS, nova página com o título \"APÊNDICE A – MANUAL DO USUÁRIO\" no estilo de Título 1, e "
        "todo o conteúdo abaixo. As imagens do manual são do protótipo: trocar pelas telas reais quando forem "
        "capturadas, mantendo as legendas.")
    ultima_figura = max(int(n) for _, _, md in CAPITULOS for n in re.findall(r"\*\*Figura (\d+) ", md.read_text(encoding="utf-8")))
    guia.markdown(imagens_com_legenda(manual_em_markdown(), ultima_figura + 1), MANUAL.parent)

    # 11. Entrega e 12. Pendências
    guia.quebra()
    guia.parte("11 ENTREGA FINAL (29/11)")
    for item in ENTREGA_FINAL:
        guia.markdown(f"- {item}", PASTA)
    guia.parte("12 PENDÊNCIAS DA DUPLA")
    guia.instrucao("O que este guia não consegue resolver sozinho, em ordem de urgência.", rotulo="LEIA")
    for item in PENDENCIAS:
        guia.markdown(f"- {item}", PASTA)
    guia.salvar()


def atualizar_referencias() -> None:
    """Troca a lista do documento de referências pela completa, mantendo o cabeçalho e a formatação dele."""
    secoes = secoes_do_principal()
    revisoes = partes_das_revisoes()
    documento = Document(str(REFERENCIAS))
    paragrafos = documento.paragraphs
    titulo = next(p for p in paragrafos if p.text.strip() == "Referências")
    modelo = next(p for p in paragrafos if p.text.startswith("ASSOCIAÇÃO BRASILEIRA"))
    molde = copy.deepcopy(modelo._p)
    for p in paragrafos:
        if p._p is not titulo._p and paragrafos.index(p) > paragrafos.index(titulo):
            p._p.getparent().remove(p._p)
    ancora = titulo._p
    for referencia in referencias_completas(secoes, revisoes):
        novo = copy.deepcopy(molde)
        for run in novo.findall(qn("w:r"))[1:]:
            novo.remove(run)
        primeiro = novo.find(qn("w:r"))
        for t in primeiro.findall(qn("w:t"))[1:]:
            primeiro.remove(t)
        texto = primeiro.find(qn("w:t"))
        texto.text = referencia
        texto.set(qn("xml:space"), "preserve")
        ancora.addnext(novo)
        ancora = novo
    documento.save(str(REFERENCIAS))


if __name__ == "__main__":
    montar()
    atualizar_referencias()
    print(f"Guia: {SAIDA}")
    print(f"Referências: {REFERENCIAS}")
