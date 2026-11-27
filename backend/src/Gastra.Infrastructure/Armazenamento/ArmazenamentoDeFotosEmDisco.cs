using Gastra.Domain.Servicos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gastra.Infrastructure.Armazenamento;

/// <summary>Fotos do cardápio (públicas).</summary>
public class ArmazenamentoDeFotosEmDisco(IConfiguration configuration, ILogger<ArmazenamentoDeFotosEmDisco> logger)
    : FotosEmDisco(configuration["Fotos:Diretorio"] ?? Path.Combine(AppContext.BaseDirectory, "fotos"), logger),
        IArmazenamentoDeFotos;
