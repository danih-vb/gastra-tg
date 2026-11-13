"""Captura as telas reais do GASTRA em execução, para a seção 5.1 do TG II e para a avaliação heurística.

Usa um Edge aberto com depuração remota (o login é feito por uma pessoa, na própria janela; o script não lê nem
guarda senha) e salva cada tela em docs/ux-ui/telas-reais/<nome>.png.

    msedge --remote-debugging-port=9222 --user-data-dir=<pasta temporária> http://localhost:4200/acesso/login
    pip install websocket-client
    python docs/ux-ui/capturar_telas_reais.py cliente <código da mesa>
    python docs/ux-ui/capturar_telas_reais.py garcom [e-mail]   # com o e-mail, entra antes com a conta de teste
    python docs/ux-ui/capturar_telas_reais.py metre [e-mail]
    python docs/ux-ui/capturar_telas_reais.py gerente [e-mail]

Com o e-mail, o script entra sozinho com as contas fictícias do semeador: a senha vem de GASTRA_SEMEADOR_SENHA
(infra/.env) e o código do segundo fator é calculado da chave que a tela de primeiro acesso mostra.
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
    # No celular, a captura é da tela visível: em página inteira, a barra de baixo (fixa) sai no meio da imagem.
    n.tamanho(*CELULAR)
    n.ir("/comandas")
    n.foto("garcom-mesas")
    livre = n.js("""(() => { const m = [...document.querySelectorAll('.mesa')].find(b => b.textContent.includes('Livre'));
        if (!m) return false; m.click(); return true; })()""")
    time.sleep(1.2)
    if livre:
        n.foto("garcom-abrir-mesa")
    n.ir("/comandas")
    n.js("[...document.querySelectorAll('.mesa.minha')][0]?.click()")
    time.sleep(3)
    aberta = n.js("location.pathname")
    n.foto("garcom-comanda")
    n.clicar("Bebidas", "button")
    item = n.js("""(() => { const b = [...document.querySelectorAll('.lista.cardapio li button')].find(x => !x.disabled);
        if (!b) return false; b.click(); return true; })()""")
    time.sleep(1.2)
    if item:
        n.foto("garcom-lancar-item")
        # Lança de fato, para a tela A entregar mostrar o item pendente.
        n.clicar("Lançar 1", "app-folha button", espera=2.5)
    n.ir("/a-entregar")
    n.foto("garcom-pendencias")
    if aberta.startswith("/comandas/"):
        n.ir(aberta + "/fechar", 3)
        n.foto("garcom-fechar")
    n.ir("/desempenho")
    n.foto("garcom-desempenho")


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




def codigo_totp(chave_base32: str, agora: float | None = None) -> str:
    """Código de 6 dígitos (RFC 6238), o mesmo que o aplicativo autenticador mostraria para a conta de teste."""
    import hashlib
    import hmac
    import struct

    chave = base64.b32decode(chave_base32.replace(" ", "").upper() + "=" * (-len(chave_base32.replace(" ", "")) % 8))
    contador = struct.pack(">Q", int((agora or time.time()) // 30))
    resumo = hmac.new(chave, contador, hashlib.sha1).digest()
    deslocamento = resumo[-1] & 0x0F
    numero = struct.unpack(">I", resumo[deslocamento:deslocamento + 4])[0] & 0x7FFFFFFF
    return f"{numero % 1_000_000:06d}"


def preencher(n: Navegador, seletor: str, valor: str) -> None:
    n.js(f"""(() => {{ const e = document.querySelector({json.dumps(seletor)}); e.value = {json.dumps(valor)};
        e.dispatchEvent(new Event('input', {{ bubbles: true }})); }})()""")


def entrar(n: Navegador, email: str) -> None:
    """Login com a conta de teste do semeador; a senha vem de GASTRA_SEMEADOR_SENHA (infra/.env) e não é exibida."""
    import os

    n.enviar("Emulation.clearDeviceMetricsOverride")
    n.ir("/acesso/login", 1.5)
    n.js("localStorage.clear(); sessionStorage.clear();")
    n.ir("/acesso/login", 1.5)
    preencher(n, "#email", email)
    preencher(n, "#senha", os.environ["GASTRA_SEMEADOR_SENHA"])
    n.js("document.querySelector('form').requestSubmit()")
    time.sleep(3)
    if n.js("location.pathname").startswith("/acesso/segundo-fator"):
        chave = n.js("document.querySelector('.chave-manual code')?.textContent?.trim() ?? null")
        if not chave:
            raise SystemExit("Segundo fator já configurado em outra sessão: sem a chave, não há como gerar o código.")
        preencher(n, "#codigo", codigo_totp(chave))
        n.js("document.querySelector('form').requestSubmit()")
        time.sleep(3)
    print("   entrou:", n.js("location.pathname"))


if __name__ == "__main__":
    papel = sys.argv[1]
    navegador = Navegador()
    if papel == "cliente":
        cliente(navegador, sys.argv[2])
    else:
        if len(sys.argv) > 2:
            entrar(navegador, sys.argv[2])
        {"garcom": garcom, "metre": metre, "gerente": gerente}[papel](navegador)
