namespace Gastra.Domain.Servicos;

/// <summary>Garçom presente no turno, com os dois fatores da RN03.</summary>
public record GarcomParaAlocacao(int GarcomId, decimal FaturamentoAcumulado, int TurnosDesdePracaDeAltoPotencial);

/// <summary>Praça do turno: quantos garçons comporta e quanto costuma faturar.</summary>
public record PracaParaAlocacao(int PracaId, int Vagas, decimal FaturamentoMedioHistorico);

/// <summary>Resultado da programação linear: um garçom numa praça.</summary>
public record DesignacaoSugerida(int GarcomId, int PracaId);
