"""Compara os tipos do frontend (frontend/src/app/core/api/modelos.ts) com o contrato da API (OpenAPI), issue #161.

    curl http://localhost:5019/openapi/v1.json -o openapi.json   # API em Development (Docker ou dotnet run)
    python docs/arquitetura/comparar_contrato.py openapi.json

Para cada interface do frontend, acha o esquema da API com os mesmos campos e aponta:
- campo que o frontend espera e a API não manda (erro: viria undefined);
- campo que a API pode mandar nulo e o frontend tipa como não nulo (erro: a tela quebraria);
- valores de enum diferentes.
Campos que a API manda e o frontend não usa são só contados: não quebram nada.
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

RAIZ = Path(__file__).resolve().parents[2]
MODELOS = RAIZ / "frontend" / "src" / "app" / "core" / "api" / "modelos.ts"


def interfaces_do_frontend(texto: str) -> dict[str, dict[str, tuple[bool, str]]]:
    """{interface: {campo: (aceita_nulo_ou_opcional, tipo)}}"""
    resultado = {}
    sem_comentarios = re.sub(r"/\*\*.*?\*/", "", texto, flags=re.S)
    for m in re.finditer(r"export interface (\w+)(?: extends [\w, ]+)? \{(.*?)\n\}", sem_comentarios, re.S):
        campos, profundidade, aberto = {}, 0, None
        for linha in m.group(2).splitlines():
            # Só os campos do primeiro nível: um tipo declarado dentro de outro (itens: { ... }[]) fica de fora.
            if profundidade == 0:
                c = re.match(r"\s*(\w+)(\?)?:\s*(.+?);?\s*$", linha)
                if c:
                    tipo = c.group(3)
                    campos[c.group(1)] = (bool(c.group(2)) or "null" in tipo or "undefined" in tipo, tipo)
            profundidade += linha.count("{") - linha.count("}")
        resultado[m.group(1)] = campos
    return resultado


def tipos_do_frontend(texto: str) -> dict[str, set[str]]:
    return {m.group(1): set(re.findall(r"'(\w+)'", m.group(2)))
            for m in re.finditer(r"export type (\w+) = ([^;]+);", texto)}


def esquemas_da_api(openapi: dict) -> tuple[dict, dict]:
    esquemas = openapi["components"]["schemas"]
    objetos, enums = {}, {}
    for nome, esquema in esquemas.items():
        if "enum" in esquema:
            enums[nome] = {str(v) for v in esquema["enum"]}
            continue
        props = esquema.get("properties", {})
        if not props:
            continue
        campos = {}
        for campo, p in props.items():
            tipo = p.get("type")
            anulavel = (isinstance(tipo, list) and "null" in tipo) or p.get("nullable", False) \
                or any(x.get("type") == "null" for x in p.get("oneOf", []) + p.get("anyOf", []))
            campos[campo] = anulavel
        objetos[nome] = campos
    return objetos, enums


def enums_do_backend() -> dict[str, set[str]]:
    enums = {}
    for arquivo in (RAIZ / "backend" / "src" / "Gastra.Communication" / "Enums").glob("*.cs"):
        sem_comentarios = re.sub(r"//.*", "", arquivo.read_text(encoding="utf-8"))
        for m in re.finditer(r"public enum (\w+)\s*\{(.*?)\}", sem_comentarios, re.S):
            enums[m.group(1)] = {v.split("=")[0].strip() for v in m.group(2).split(",") if v.strip()}
    return enums


def main() -> None:
    openapi = json.loads(Path(sys.argv[1]).read_text(encoding="utf-8"))
    texto = MODELOS.read_text(encoding="utf-8")
    frontend = interfaces_do_frontend(texto)
    objetos, enums = esquemas_da_api(openapi)

    erros, avisos, casadas = [], [], 0
    for nome, campos in sorted(frontend.items()):
        candidatos = sorted(objetos.items(), key=lambda kv: -len(set(kv[1]) & set(campos)) / max(len(set(kv[1]) | set(campos)), 1))
        esquema, da_api = candidatos[0]
        semelhanca = len(set(da_api) & set(campos)) / max(len(set(da_api) | set(campos)), 1)
        if semelhanca < 0.5:
            avisos.append(f"{nome}: sem esquema parecido na API (melhor: {esquema}, {semelhanca:.0%}); tipo só do frontend?")
            continue
        casadas += 1
        for campo, (aceita_nulo, tipo) in campos.items():
            if campo not in da_api:
                erros.append(f"{nome}.{campo}: o frontend espera e a API ({esquema}) não manda")
            elif da_api[campo] and not aceita_nulo:
                erros.append(f"{nome}.{campo}: a API ({esquema}) pode mandar nulo e o frontend tipa como `{tipo}`")
        sobra = set(da_api) - set(campos)
        if sobra:
            avisos.append(f"{nome} ({esquema}): a API manda e a tela não usa: {', '.join(sorted(sobra))}")

    # O OpenAPI do .NET manda os enums como texto, sem a lista de valores: a lista vem do próprio C#.
    if not enums:
        enums = enums_do_backend()
    tipos = tipos_do_frontend(texto + (RAIZ / "frontend" / "src" / "app" / "core" / "sessao" / "modelos.ts").read_text(encoding="utf-8"))
    tipos["PapelUsuario"] = tipos.pop("Papel", set())
    for nome, valores in sorted(tipos.items()):
        if nome in enums and valores and valores != enums[nome]:
            erros.append(f"enum {nome}: frontend {sorted(valores)} x API {sorted(enums[nome])}")

    print(f"{casadas} interfaces casadas com esquemas da API; {len(erros)} divergências; {len(avisos)} avisos.\n")
    print("## Divergências\n" + ("\n".join(f"- {e}" for e in erros) or "- nenhuma"))
    print("\n## Avisos\n" + ("\n".join(f"- {a}" for a in avisos) or "- nenhum"))


if __name__ == "__main__":
    main()
