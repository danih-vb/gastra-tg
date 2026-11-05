using Gastra.Communication.Responses;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;

namespace Gastra.Application.UseCases.Alocacoes;

// UC15 — quem o Metre pode marcar como presente no turno (#147).
public interface IListarGarconsDoTurnoUseCase
{
    Task<List<GarcomDoTurnoResponse>> Executar();
}

public class ListarGarconsDoTurnoUseCase(IRepositorioUsuario repositorio) : IListarGarconsDoTurnoUseCase
{
    public async Task<List<GarcomDoTurnoResponse>> Executar()
    {
        var usuarios = await repositorio.ListarTodos();

        // Inativo não entra na alocação: a própria sugestão recusaria depois (AlocacaoGarcomInvalido).
        return usuarios
            .Where(u => u.Ativo && u.Papel == PapelUsuario.Garcom)
            .OrderBy(u => u.Nome)
            .Select(u => new GarcomDoTurnoResponse { Id = u.Id, Nome = u.Nome })
            .ToList();
    }
}
