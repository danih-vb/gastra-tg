using Gastra.Application.Fotos;
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
    public const int TamanhoMaximo = FormatoDeFoto.TamanhoMaximo;

    /// <summary>Rota pública que serve as fotos do cardápio.</summary>
    public const string Prefixo = "/api/cardapio/fotos/";

    public async Task<FotoEnviadaResponse> Executar(byte[] conteudo)
    {
        var extensao = FormatoDeFoto.Extensao(conteudo)
                       ?? throw new ErroValidacaoException([MensagensErro.FotoInvalida]);

        var nome = await armazenamento.Salvar(conteudo, extensao);
        return new FotoEnviadaResponse { Imagem = Prefixo + nome };
    }
}
