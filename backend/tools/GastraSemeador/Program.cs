using GastraSemeador;

// Semeia o banco de desenvolvimento com dados simulados (épico #179).
//
// Esta primeira parte cria os cadastros base (#181): contas, praças, mesas e cardápio. O histórico de
// comandas fechadas (#182) e o estado "ao vivo" da demonstração (#183) entram em seguida, sobre esses
// mesmos cadastros.
//
// Uso, dentro de backend/:
//   GASTRA_AMBIENTE=Development \
//   GASTRA_SEMEADOR_CONEXAO="Server=localhost;Port=3307;Database=gastra_dev;User ID=gastra_app;Password=..." \
//   GASTRA_SEMEADOR_SENHA="..." \
//   dotnet run --project tools/GastraSemeador

var configuracao = Configuracao.Ler(args, Console.Error);
if (configuracao is null)
    return 1;

try
{
    await using var contexto = configuracao.AbrirContexto();

    if (!await contexto.Database.CanConnectAsync())
    {
        Console.Error.WriteLine("Não consegui conectar no banco. Ele está no ar e a conexão está certa?");
        return 1;
    }

    Console.WriteLine($"Semeando com a semente {configuracao.Semente}.");
    await CadastrosBase.Semear(contexto, configuracao, Console.Out);
    Console.WriteLine("Pronto. Os dados são fictícios e não representam nenhum restaurante real.");
    return 0;
}
catch (Exception excecao)
{
    Console.Error.WriteLine($"O semeador parou: {excecao.Message}");
    return 1;
}
