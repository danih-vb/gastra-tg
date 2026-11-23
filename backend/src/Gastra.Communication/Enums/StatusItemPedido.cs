namespace Gastra.Communication.Enums;

public enum StatusItemPedido
{
    Pendente,
    Entregue,
    Cancelado,

    /// <summary>Lançado pelo Metre ou pelo Gerente: espera o garçom da mesa confirmar.</summary>
    AConfirmar,
}
