"""Captura as telas reais do GASTRA em execução, para a seção 5.1 do TG II e para a avaliação heurística.

Usa um Edge aberto com depuração remota (o login é feito por uma pessoa, na própria janela; o script não lê nem
guarda senha) e salva cada tela em docs/ux-ui/telas-reais/<nome>.png.

    msedge --remote-debugging-port=9222 --user-data-dir=<pasta temporária> http://localhost:4200/acesso/login
    pip install websocket-client
    python docs/ux-ui/capturar_telas_reais.py cliente <código da mesa>
    python docs/ux-ui/capturar_telas_reais.py garcom      # depois de entrar como garçom
    python docs/ux-ui/capturar_telas_reais.py metre       # depois de entrar como Metre
    python docs/ux-ui/capturar_telas_reais.py gerente     # depois de entrar como Gerente
"""

from __future__ import annotations

import base64
import json
import sys
import time
import urllib.request
from pathlib import Path

import websocket

BASE = "http://localhost:4200"
SAIDA = Path(__file__).resolve().parent / "telas-reais"
CELULAR = (390, 844)
COMPUTADOR = (1366, 900)


class Navegador:
    def __init__(self) -> None:
        abas = json.load(urllib.request.urlopen("http://localhost:9222/json"))
        aba = next(a for a in abas if a["type"] == "page" and a["url"].startswith(BASE))
        self.ws = websocket.create_connection(aba["webSocketDebuggerUrl"], timeout=30, suppress_origin=True)
        self.id = 0

    def enviar(self, metodo: str, **parametros):
        self.id += 1
        self.ws.send(json.dumps({"id": self.id, "method": metodo, "params": parametros}))
        while True:
            resposta = json.loads(self.ws.recv())
            if resposta.get("id") == self.id:
                if "error" in resposta:
                    raise RuntimeError(resposta["error"])
                return resposta.get("result", {})

    def tamanho(self, largura: int, altura: int) -> None:
        self.enviar("Emulation.setDeviceMetricsOverride", width=largura, height=altura, deviceScaleFactor=2,
                    mobile=largura < 800)

    def ir(self, rota: str, espera: float = 2.5) -> None:
        self.enviar("Page.navigate", url=BASE + rota)
        time.sleep(espera)

    def js(self, codigo: str):
        return self.enviar("Runtime.evaluate", expression=codigo, awaitPromise=True, returnByValue=True)["result"].get("value")

    def clicar(self, texto: str, seletor: str = "button, a, [role=button]", espera: float = 1.5) -> bool:
        achou = self.js(f"""(() => {{
            const el = [...document.querySelectorAll({json.dumps(seletor)})]
                .find(e => e.offsetParent !== null && e.textContent.replace(/\\s+/g, ' ').trim().includes({json.dumps(texto)}));
            if (!el) return false;
            el.click();
            return true;
        }})()""")
        time.sleep(espera)
        return bool(achou)

    def foto(self, nome: str, pagina_inteira: bool = False) -> None:
        parametros = {"format": "png"}
        if pagina_inteira:
            altura = self.js("Math.min(document.documentElement.scrollHeight, 3000)")
            largura = self.js("document.documentElement.clientWidth")
            parametros |= {"captureBeyondViewport": True,
                           "clip": {"x": 0, "y": 0, "width": largura, "height": altura, "scale": 1}}
        dados = self.enviar("Page.captureScreenshot", **parametros)["data"]
        SAIDA.mkdir(exist_ok=True)
        (SAIDA / f"{nome}.png").write_bytes(base64.b64decode(dados))
        print("  ", nome)


def cliente(n: Navegador, codigo: str) -> None:
    n.tamanho(*CELULAR)
    n.ir("/cliente")
    n.foto("cliente-inicio")
    n.ir("/cliente/cardapio")
    n.foto("cliente-cardapio")
    n.clicar("Sem glúten", ".chip")
    n.foto("cliente-filtro")
    n.ir(f"/cliente/conta/{codigo}")
    n.foto("cliente-conta", pagina_inteira=True)


def garcom(n: Navegador) -> None:
    n.tamanho(*CELULAR)
    n.ir("/comandas")
    n.foto("garcom-mesas", pagina_inteira=True)
    livre = n.js("""(() => { const m = [...document.querySelectorAll('.mesa')].find(b => b.textContent.includes('Livre'));
        if (!m) return false; m.click(); return true; })()""")
    time.sleep(1.2)
    if livre:
        n.foto("garcom-abrir-mesa")
        n.js("document.querySelector('app-folha button[aria-label], app-folha .fechar')?.click()")
    n.ir("/comandas")
    aberta = n.js("""(() => { const m = [...document.querySelectorAll('.mesa.minha')][0];
        if (!m) return null; m.click(); return location.pathname; })()""")
    time.sleep(2.5)
    n.foto("garcom-comanda", pagina_inteira=True)
    n.clicar("Bebidas", "button")
    item = n.js("""(() => { const b = [...document.querySelectorAll('.lista.cardapio li button')].find(x => !x.disabled);
        if (!b) return false; b.click(); return true; })()""")
    time.sleep(1.2)
    if item:
        n.foto("garcom-lancar-item")
    n.ir("/a-entregar")
    n.foto("garcom-pendencias", pagina_inteira=True)
    if aberta:
        n.ir(aberta + "/fechar")
        n.foto("garcom-fechar", pagina_inteira=True)
    n.ir("/desempenho")
    n.foto("garcom-desempenho", pagina_inteira=True)


def metre(n: Navegador) -> None:
    n.tamanho(*COMPUTADOR)
    n.ir("/alocacao", 3)
    n.foto("metre-presenca")
    n.clicar("Gerar sugestão", "button", espera=4)
    n.foto("metre-sugestao", pagina_inteira=True)
    n.ir("/salao-agora", 3)
    n.foto("metre-salao", pagina_inteira=True)


def gerente(n: Navegador) -> None:
    n.tamanho(*COMPUTADOR)
    n.ir("/analises", 5)
    n.foto("gerente-analises", pagina_inteira=True)
    n.ir("/cardapio", 3)
    n.foto("gerente-cardapio", pagina_inteira=True)
    n.ir("/salao", 3)
    n.foto("gerente-salao", pagina_inteira=True)
    n.ir("/usuarios", 3)
    n.foto("gerente-usuarios", pagina_inteira=True)


if __name__ == "__main__":
    papel = sys.argv[1]
    navegador = Navegador()
    if papel == "cliente":
        cliente(navegador, sys.argv[2])
    else:
        {"garcom": garcom, "metre": metre, "gerente": gerente}[papel](navegador)
