namespace Gastra.Communication.Responses;

/// <summary>
/// UC15 — garçom que pode entrar no turno. Traz só o necessário para a lista de presença do Metre: nada de e-mail
/// ou situação do segundo fator, que são assunto da gestão de contas (RNF03).
/// </summary>
public class GarcomDoTurnoResponse
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;
}
