"""Contratos de entrada e saída do serviço analítico (o que o backend em C# envia e recebe)."""

from __future__ import annotations

from pydantic import BaseModel, Field


class PedidoDeRecomendacao(BaseModel):
    """
    `itens` são os ids do cardápio já lançados na comanda. `historico` é o conjunto de comandas
    anteriores; quando não vem, o serviço lê o histórico real do banco (D10) ou, sem movimento suficiente,
    usa o simulado.
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
    origem_do_historico: str  # "banco", "simulado" ou "informado"
    motivo_do_simulado: str | None = None
    perfil: int | None = None  # perfil de consumo usado; None quando só as regras gerais valeram


class ItemMarcanteDoPerfil(BaseModel):
    item_id: int
    presenca: float  # fração das comandas do perfil que levaram o item
    destaque: float  # quantas vezes mais ele aparece no perfil do que no restaurante inteiro


class PerfilDeConsumo(BaseModel):
    id: int
    comandas: int
    participacao: float
    itens_marcantes: list[ItemMarcanteDoPerfil]


class RespostaDePerfis(BaseModel):
    perfis: list[PerfilDeConsumo]
    silhueta: float | None
    silhuetas_testadas: dict[int, float]
    segmenta_a_recomendacao: bool
    comandas_analisadas: int
    origem_do_historico: str
    motivo_do_simulado: str | None = None


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
