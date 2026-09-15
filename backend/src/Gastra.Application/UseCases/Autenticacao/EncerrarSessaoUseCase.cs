using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;

namespace Gastra.Application.UseCases.Autenticacao;

// UC03 — Encerrar sessão (RF17)
public interface IEncerrarSessaoUseCase
{
    Task Executar();
}

public class EncerrarSessaoUseCase(IUsuarioLogado usuarioLogado, IUnitOfWork unitOfWork) : IEncerrarSessaoUseCase
{
    public async Task Executar()
    {
        var usuario = await usuarioLogado.Obter();

        // Troca a chave de sessão: o token usado nesta requisição (e qualquer outro) deixa de valer.
        usuario.EncerrarSessoes();
        await unitOfWork.Commit();
    }
}
