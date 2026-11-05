"""Garante que os algoritmos não dependem da API.

A importação roda num interpretador novo: dentro do pytest a API já foi importada por outros
testes, e o resultado ficaria contaminado.
"""

import os
import subprocess
import sys
from pathlib import Path

import pytest

SRC = Path(__file__).resolve().parents[1] / "src"


@pytest.mark.parametrize("modulo", ["gastra_analitica.recomendacao", "gastra_analitica.alocacao"])
def test_algoritmo_nao_importa_fastapi(modulo):
    codigo = f"import sys; import {modulo}; print('fastapi' in sys.modules)"

    resultado = subprocess.run(
        [sys.executable, "-c", codigo],
        capture_output=True,
        text=True,
        env={**os.environ, "PYTHONPATH": str(SRC)},
        check=True,
    )

    assert resultado.stdout.strip() == "False"
