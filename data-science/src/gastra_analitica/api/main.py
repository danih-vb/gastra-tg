"""Serviço analítico do GASTRA.

Recebe os dados do backend em C#, calcula e devolve o resultado. Não grava nada: quem persiste é
sempre a API ASP.NET Core (decisão D2 da arquitetura).
"""

from fastapi import FastAPI, HTTPException

from gastra_analitica.alocacao.programacao_linear import (
    AlocacaoInviavelError,
    GarcomDisponivel,
    PracaDoTurno,
    sugerir_alocacao,
)
from gastra_analitica.api.modelos import (
    DesignacaoSugerida,
    ItemSugerido,
    PedidoDeAlocacao,
    PedidoDeRecomendacao,
    RespostaDeAlocacao,
    RespostaDeRecomendacao,
)
from gastra_analitica.dados.simulador import gerar_historico
from gastra_analitica.recomendacao.regras_associacao import ModeloRecomendacao, treinar

app = FastAPI(
    title="GASTRA — Camada Analítica",
    description="Recomendação de pratos e alocação de garçons, consumida pelo backend em C#.",
    version="0.2.0",
)

# Modelo de apoio enquanto o banco não tem movimento real: treinado uma vez, no primeiro uso.
_modelo_simulado: ModeloRecomendacao | None = None


def _modelo_do_historico_simulado() -> ModeloRecomendacao:
    global _modelo_simulado
    if _modelo_simulado is None:
        _modelo_simulado = treinar(gerar_historico().transacoes)
    return _modelo_simulado


@app.get("/health", tags=["Saúde"])
def health() -> dict[str, str]:
    """Indica que o serviço está no ar."""
    return {"status": "Healthy"}


@app.post("/recomendacao/combinacoes", tags=["Recomendação"], response_model=RespostaDeRecomendacao)
def sugerir_combinacoes(pedido: PedidoDeRecomendacao) -> RespostaDeRecomendacao:
    """
    RF09 — sugere itens a partir do que já foi pedido na comanda.

    Usa só padrão de consumo observável: nenhum atributo pessoal do cliente entra no cálculo (RN05).
    """
    if pedido.historico is None:
        modelo = _modelo_do_historico_simulado()
        origem = "simulado"
    else:
        modelo = treinar(pedido.historico)
        origem = "informado"

    sugestoes = modelo.sugerir(pedido.itens, pedido.limite, pedido.itens_disponiveis)

    return RespostaDeRecomendacao(
        sugestoes=[ItemSugerido(item_id=s.item_id, confianca=s.confianca, lift=s.lift) for s in sugestoes],
        regras_consideradas=len(modelo),
        origem_do_historico=origem,
    )


@app.post("/alocacao/sugestao", tags=["Alocação"], response_model=RespostaDeAlocacao)
def sugerir_alocacao_do_turno(pedido: PedidoDeAlocacao) -> RespostaDeAlocacao:
    """RF06 e RN03 — distribui os garçons do turno entre as praças por programação linear."""
    if abs(pedido.peso_desequilibrio + pedido.peso_espera - 1) > 1e-9:
        raise HTTPException(status_code=422, detail="Os pesos precisam somar 1.")

    garcons = [
        GarcomDisponivel(g.id, g.faturamento_por_turno, g.turnos_desde_praca_de_alto_potencial)
        for g in pedido.garcons
    ]
    pracas = [PracaDoTurno(p.id, p.vagas, p.faturamento_medio_historico) for p in pedido.pracas]

    try:
        resultado = sugerir_alocacao(garcons, pracas, pedido.peso_desequilibrio, pedido.peso_espera)
    except AlocacaoInviavelError as erro:
        raise HTTPException(status_code=422, detail=str(erro)) from erro

    return RespostaDeAlocacao(
        designacoes=[
            DesignacaoSugerida(garcom_id=d.garcom_id, praca_id=d.praca_id, custo=d.custo)
            for d in resultado.designacoes
        ],
        custo_total=resultado.custo_total,
        peso_desequilibrio=resultado.peso_desequilibrio,
        peso_espera=resultado.peso_espera,
    )
