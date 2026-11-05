using Mapster;

namespace Gastra.Application.Mapeamento;

public static class MapeamentoConfig
{
    /// <summary>
    /// Configura o Mapster. Os enums da Communication e do Domain são tipos diferentes com os mesmos
    /// nomes: a conversão é feita pelo nome, e não pelo número, para que reordenar um enum não troque
    /// valores silenciosamente.
    /// </summary>
    public static void Registrar()
    {
        TypeAdapterConfig.GlobalSettings.Default.EnumMappingStrategy(EnumMappingStrategy.ByName);
    }
}
