using Gastra.Domain.Servicos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gastra.Infrastructure.Armazenamento;

/// <summary>
/// Fotos dos funcionários: noutra pasta, que nenhuma rota pública lê. Se um dia alguém servir a pasta do cardápio
/// direto pelo nginx, estas continuam de fora.
/// </summary>
public class ArmazenamentoDeFotosDeFuncionariosEmDisco(
    IConfiguration configuration,
    ILogger<ArmazenamentoDeFotosDeFuncionariosEmDisco> logger)
    : FotosEmDisco(
            configuration["Fotos:DiretorioFuncionarios"] ?? Path.Combine(AppContext.BaseDirectory, "fotos-funcionarios"),
            logger),
        IArmazenamentoDeFotosDeFuncionarios;
