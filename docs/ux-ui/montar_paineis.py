"""Junta as 33 telas do protótipo (docs/ux-ui/telas) em painéis por perfil, para a seção 4.4.5 do TG II.

    pip install pillow
    python docs/ux-ui/montar_paineis.py

Cada painel vira uma figura do documento, com as telas identificadas por letra: (a), (b)… Celular é cortado na
altura da tela (932 px) e computador na altura da janela (900 px), para as telas do mesmo painel ficarem alinhadas;
o conteúdo abaixo da dobra aparece no manual do usuário.
"""

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

PASTA = Path(__file__).resolve().parent
TELAS = PASTA / "telas"
SAIDA = TELAS / "paineis"
FONTE = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 30) if Path("C:/Windows/Fonts/arial.ttf").exists() \
    else ImageFont.load_default()

# nome do painel: (colunas, [(arquivo, rótulo)])
PAINEIS = {
    "acesso": (4, [("acesso-login", "Entrar"), ("acesso-login-erro", "Senha incorreta"),
                   ("acesso-configurar-2fa", "Vincular o autenticador"), ("acesso-codigo", "Código do autenticador")]),
    "garcom-1": (4, [("garcom-mesas", "Mesas"), ("garcom-abrir-mesa", "Abrir mesa"),
                     ("garcom-comanda", "Comanda"), ("garcom-lancar-item", "Lançar item")]),
    "garcom-2": (4, [("garcom-restricao", "Restrição alimentar"), ("garcom-sugestoes-indisponiveis", "Sem sugestões"),
                     ("garcom-cancelar", "Cancelar item"), ("garcom-pendencias", "A entregar")]),
    "garcom-3": (3, [("garcom-fechar", "Fechar conta"), ("garcom-fechar-bloqueado", "Fechamento bloqueado"),
                     ("garcom-desempenho", "Desempenho")]),
    "metre": (2, [("metre-presenca", "Presença"), ("metre-sugestao", "Sugestão de alocação"),
                  ("metre-troca", "Troca de praça"), ("metre-confirmada", "Alocação confirmada"),
                  ("metre-servico-indisponivel", "Serviço de análise fora"), ("metre-salao", "Salão agora")]),
    "gerente-1": (2, [("gerente-analises", "Análises"), ("gerente-cardapio", "Cardápio"),
                      ("gerente-preco", "Alterar preço"), ("gerente-promocoes", "Promoções")]),
    "gerente-2": (2, [("gerente-nova-promocao", "Nova promoção"), ("gerente-erro-formulario", "Erro no formulário"),
                      ("gerente-salao", "Salão: praças e mesas"), ("gerente-usuarios", "Usuários")]),
    "cliente": (4, [("cliente-inicio", "Início"), ("cliente-cardapio", "Cardápio digital"),
                    ("cliente-filtro", "Filtro por restrição"), ("cliente-conta", "Conta")]),
}

# Telas reais do sistema em execução (capturar_telas_reais.py), para a seção 5.1. Capturadas em escala 2: o celular já
# vem na altura da tela, e o computador é cortado na altura da janela (900 px × 2).
PAINEIS_REAIS = {
    "garcom-1": (4, [("garcom-mesas", "Mesas"), ("garcom-abrir-mesa", "Abrir mesa"),
                     ("garcom-comanda", "Comanda e sugestões"), ("garcom-lancar-item", "Lançar item")]),
    "garcom-2": (3, [("garcom-pendencias", "A entregar"), ("garcom-fechar", "Fechar conta"),
                     ("garcom-desempenho", "Desempenho")]),
    "metre": (2, [("metre-presenca", "Presença"), ("metre-sugestao", "Sugestão de alocação"),
                  ("metre-salao", "Salão agora")]),
    "gerente": (2, [("gerente-analises", "Análises"), ("gerente-cardapio", "Cardápio"),
                    ("gerente-salao", "Salão"), ("gerente-usuarios", "Usuários")]),
    "cliente": (4, [("cliente-inicio", "Início"), ("cliente-cardapio", "Cardápio digital"),
                    ("cliente-filtro", "Filtro sem glúten"), ("cliente-conta", "Conta")]),
}

LARGURA_DO_PAINEL = 1600
MARGEM = 16
ALTURA_DO_ROTULO = 50


def montar(colunas: int, telas: list[tuple[str, str]], pasta: Path = TELAS, escala: int = 1) -> Image.Image:
    largura_da_celula = (LARGURA_DO_PAINEL - MARGEM * (colunas + 1)) // colunas
    imagens = []
    for arquivo, _ in telas:
        imagem = Image.open(pasta / f"{arquivo}.png").convert("RGB")
        dobra = (932 if imagem.width < 800 * escala else 900) * escala
        imagem = imagem.crop((0, 0, imagem.width, min(imagem.height, dobra)))
        altura = round(imagem.height * largura_da_celula / imagem.width)
        imagens.append(imagem.resize((largura_da_celula, altura), Image.LANCZOS))
    altura_da_celula = max(i.height for i in imagens) + ALTURA_DO_ROTULO
    linhas = -(-len(imagens) // colunas)
    painel = Image.new("RGB", (LARGURA_DO_PAINEL, MARGEM + linhas * (altura_da_celula + MARGEM)), "white")
    desenho = ImageDraw.Draw(painel)
    for indice, (imagem, (_, rotulo)) in enumerate(zip(imagens, telas)):
        x = MARGEM + (indice % colunas) * (largura_da_celula + MARGEM)
        y = MARGEM + (indice // colunas) * (altura_da_celula + MARGEM)
        painel.paste(imagem, (x, y))
        desenho.rectangle((x, y, x + imagem.width - 1, y + imagem.height - 1), outline="#c8c8c8", width=2)
        texto = f"({chr(97 + indice)}) {rotulo}"
        largura_do_texto = desenho.textlength(texto, font=FONTE)
        desenho.text((x + (largura_da_celula - largura_do_texto) / 2, y + imagem.height + 10), texto, fill="black", font=FONTE)
    return painel


def main() -> None:
    SAIDA.mkdir(exist_ok=True)
    for nome, (colunas, telas) in PAINEIS.items():
        montar(colunas, telas).save(SAIDA / f"{nome}.png", optimize=True)
        print(f"{nome}.png: {len(telas)} telas")
    reais = PASTA / "telas-reais"
    (reais / "paineis").mkdir(exist_ok=True)
    for nome, (colunas, telas) in PAINEIS_REAIS.items():
        montar(colunas, telas, reais, escala=2).save(reais / "paineis" / f"{nome}.png", optimize=True)
        print(f"telas-reais/paineis/{nome}.png: {len(telas)} telas")


if __name__ == "__main__":
    main()
