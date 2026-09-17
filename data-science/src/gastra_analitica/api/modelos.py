"""Contratos de entrada e saída do serviço analítico (o que o backend em C# envia e recebe)."""

from __future__ import annotations

from pydantic import BaseModel, Field


class PedidoDeRecomendacao(BaseModel):
    """
    `itens` são os ids do cardápio já lançados na comanda. `historico` é o conjunto de comandas
    anteriores; quando não vem, o serviço usa o histórico simulado (o banco ainda não tem movimento).
    """

    itens: list[int] = Field(default_factory=list)
    historico: list[list[int]] | None = None
    itens_disponiveis: list[int] | None = None
    limite: int = Field(default=3, ge=1, le=10)


class ItemSugerido(BaseModel):
    item_id: int
    confianca: float
    lift: float


class RespostaDeRecomendacao(BaseModel):
    sugestoes: list[ItemSugerido]
    regras_consideradas: int
    origem_do_historico: str


class GarcomDoTurno(BaseModel):
    id: int
    faturamento_por_turno: float = Field(ge=0)
    turnos_desde_praca_de_alto_potencial: int = Field(default=0, ge=0)


class PracaDoTurno(BaseModel):
    id: int
    vagas: int = Field(ge=1)
    faturamento_medio_historico: float = Field(ge=0)


class PedidoDeAlocacao(BaseModel):
    garcons: list[GarcomDoTurno]
    pracas: list[PracaDoTurno]
    peso_desequilibrio: float = Field(default=0.6, ge=0, le=1)
    peso_espera: float = Field(default=0.4, ge=0, le=1)


class DesignacaoSugerida(BaseModel):
    garcom_id: int
    praca_id: int
    custo: float


class RespostaDeAlocacao(BaseModel):
    designacoes: list[DesignacaoSugerida]
    custo_total: float
    peso_desequilibrio: float
    peso_espera: float
