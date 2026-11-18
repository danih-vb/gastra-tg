using Gastra.Communication.Responses;
using Gastra.Domain.Servicos;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Cardapio;

// RF05 — foto do item importada do computador
public interface IEnviarFotoUseCase
{
    Task<FotoEnviadaResponse> Executar(byte[] conteudo);
}

public class EnviarFotoUseCase(IArmazenamentoDeFotos armazenamento) : IEnviarFotoUseCase
{
    public const int TamanhoMaximo = 5 * 1024 * 1024;

    public async Task<FotoEnviadaResponse> Executar(byte[] conteudo)
    {
        var extensao = ExtensaoPeloConteudo(conteudo);
        if (extensao is null || conteudo.Length > TamanhoMaximo)
            throw new ErroValidacaoException([MensagensErro.FotoInvalida]);

        var nome = await armazenamento.Salvar(conteudo, extensao);
        return new FotoEnviadaResponse { Imagem = $"/api/cardapio/fotos/{nome}" };
    }

    /// <summary>
    /// O tipo vem dos primeiros bytes do arquivo, e não do nome nem do que o navegador declara, que qualquer um troca.
    /// SVG fica de fora de propósito: é um formato que pode carregar script, e a foto vai para o navegador do cliente.
    /// </summary>
    private static string? ExtensaoPeloConteudo(byte[] c)
    {
        if (c.Length > 3 && c[0] == 0xFF && c[1] == 0xD8 && c[2] == 0xFF)
            return "jpg";
        if (c.Length > 8 && c[0] == 0x89 && c[1] == 0x50 && c[2] == 0x4E && c[3] == 0x47
            && c[4] == 0x0D && c[5] == 0x0A && c[6] == 0x1A && c[7] == 0x0A)
            return "png";
        if (c.Length > 12 && c[0] == 0x52 && c[1] == 0x49 && c[2] == 0x46 && c[3] == 0x46
            && c[8] == 0x57 && c[9] == 0x45 && c[10] == 0x42 && c[11] == 0x50)
            return "webp";
        return null;
    }
}
