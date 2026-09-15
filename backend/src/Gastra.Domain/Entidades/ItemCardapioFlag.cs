using Gastra.Domain.Enums;

namespace Gastra.Domain.Entidades;

/// <summary>
/// Flag dietética de um item do cardápio. Existe para que o atributo multivalorado
/// <c>flags_dieteticas</c> seja gravado em tabela própria (primeira forma normal).
/// </summary>
public class ItemCardapioFlag
{
    public FlagDietetica Flag { get; private set; }

    private ItemCardapioFlag()
    {
    }

    internal ItemCardapioFlag(FlagDietetica flag)
    {
        Flag = flag;
    }
}
