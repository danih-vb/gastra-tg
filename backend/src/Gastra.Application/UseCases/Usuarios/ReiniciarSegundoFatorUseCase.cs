using Gastra.Application.Auditoria;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Usuarios;

// UC04 — O Gerente desfaz a vinculação do app autenticador de quem perdeu o celular (#141).
public interface IReiniciarSegundoFatorUseCase
{
    Task Executar(int id);
}

public class ReiniciarSegundoFatorUseCase(
    IRepositorioUsuario repositorio,
    IUsuarioLogado usuarioLogado,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IReiniciarSegundoFatorUseCase
{
    public async Task Executar(int id)
    {
        var usuario = await repositorio.ObterPorId(id)
                      ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        if (!usuario.ExigeSegundoFator())
            throw new RegraDeNegocioException(MensagensErro.PapelSemSegundoFator);

        // Quem já está com a sessão aberta poderia se desvincular e vincular outro aparelho sozinho; isso enfraquece
        // o segundo fator. Outro gerente faz, e fica o registro de quem fez.
        var gerenteLogado = await usuarioLogado.Obter();
        if (gerenteLogado.Id == usuario.Id)
            throw new RegraDeNegocioException(MensagensErro.ReiniciarProprioSegundoFator);

        // RN07: o segredo antigo é descartado e nunca é reexibido. O próximo login gera um novo na configuração.
        usuario.ReiniciarSegundoFator();

        await auditoria.Registrar(EventoAuditoria.SegundoFatorReiniciado, alvo: (nameof(Usuario), usuario.Id));
        await unitOfWork.Commit();
    }
}
