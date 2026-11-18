"""Gera as imagens do protótipo (docs/ux-ui/telas) a partir das cenas de prototipo/assets/cenas.js.

Uso, na raiz do repositório:
    python docs/ux-ui/gerar_telas.py                # todas as cenas
    python docs/ux-ui/gerar_telas.py garcom metre   # só algumas personas

Precisa do Google Chrome ou do Microsoft Edge instalado. Não precisa de servidor: abre os arquivos direto do disco.
"""

from __future__ import annotations

import os
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

PASTA = Path(__file__).resolve().parent
PROTOTIPO = PASTA / "prototipo"
SAIDA = PASTA / "telas"

CELULAR = (430, 932)
TELA_LARGA = (1366, 900)

# persona -> (tamanho padrão, {cena: altura específica})
CENAS: dict[str, tuple[tuple[int, int], dict[str, int | None]]] = {
    "garcom": (CELULAR, {
        "mesas": None, "abrir-mesa": None, "comanda": 1500, "lancar-item": None, "restricao": None, "cancelar": None,
        "fechar-bloqueado": None, "fechar": None, "pendencias": None, "desempenho": None, "sugestoes-indisponiveis": 1200,
    }),
    "metre": (TELA_LARGA, {
        "presenca": None, "sugestao": 1000, "troca": 1000, "confirmada": None, "servico-indisponivel": 1000, "salao": 1400,
    }),
    "gerente": (TELA_LARGA, {
        "analises": 1750, "cardapio": 1300, "preco": None, "promocoes": None, "nova-promocao": 1100, "salao": 1250,
        "usuarios": None, "erro-formulario": None,
    }),
    "cliente": (CELULAR, {"inicio": None, "cardapio": None, "filtro": None, "conta": 1100}),
    "acesso": (CELULAR, {"login": 1050, "login-erro": None, "configurar-2fa": 1250, "codigo": None}),
}


def navegador() -> str:
    candidatos = [
        shutil.which("chrome"), shutil.which("google-chrome"), shutil.which("msedge"),
        r"C:\Program Files\Google\Chrome\Application\chrome.exe",
        r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
    ]
    for caminho in candidatos:
        if caminho and Path(caminho).exists():
            return caminho
    sys.exit("Chrome ou Edge não encontrado.")


def capturar(exe: str, persona: str, cena: str, largura: int, altura: int, perfil: str) -> Path:
    destino = SAIDA / f"{persona}-{cena}.png"
    url = (PROTOTIPO / f"{persona}.html").as_uri() + f"?captura&cena={cena}"
    subprocess.run(
        [
            exe, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
            f"--user-data-dir={perfil}", f"--window-size={largura},{altura}", "--virtual-time-budget=6000",
            f"--screenshot={destino}", url,
        ],
        check=True, capture_output=True, timeout=90,
    )
    return destino


def main() -> None:
    escolhidas = sys.argv[1:] or list(CENAS)
    SAIDA.mkdir(exist_ok=True)
    exe = navegador()
    with tempfile.TemporaryDirectory() as perfil:
        for persona in escolhidas:
            (largura, altura_padrao), cenas = CENAS[persona]
            for cena, altura in cenas.items():
                arquivo = capturar(exe, persona, cena, largura, altura or altura_padrao, perfil)
                print(f"{arquivo.relative_to(PASTA)}  {os.path.getsize(arquivo) // 1024} KB")


if __name__ == "__main__":
    main()
