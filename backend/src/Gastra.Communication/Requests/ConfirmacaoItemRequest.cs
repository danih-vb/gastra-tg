namespace Gastra.Communication.Requests;

/// <summary>Resposta do garçom a um item lançado pelo Metre ou pelo Gerente.</summary>
public class ConfirmacaoItemRequest
{
    public bool Aceitar { get; set; }
}
