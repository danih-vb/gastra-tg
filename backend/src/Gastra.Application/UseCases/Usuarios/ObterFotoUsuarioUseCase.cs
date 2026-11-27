using Gastra.Domain.Enums;
using Gastra.Domain.Servicos;
using Gastra.Domain.Seguranca;

namespace Gastra.Application.UseCases.Usuarios;

/// <summary>
/// Lê a foto de um funcionário. O Gerente vê a de todos (é quem cuida das contas); os demais, só a própria, que é a do
/// crachá no cabeçalho. Ninguém mais tem motivo para ver a foto de uma pessoa (LGPD, minimização).
/// </summary>
public interface IObterFotoUsuarioUseCase
{
    /// <returns>O arquivo, ou nulo se não existir ou se quem pediu não puder vê-lo.</returns>
    Task<Stream?> Executar(string nome);
}

public class ObterFotoUsuarioUseCase(
    IUsuarioLogado usuarioLogado,
    IArmazenamentoDeFotosDeFuncionarios armazenamento) : IObterFotoUsuarioUseCase
{
    public async Task<Stream?> Executar(string nome)
    {
        if (usuarioLogado.ObterPapel() != PapelUsuario.Gerente)
        {
            var eu = await usuarioLogado.Obter();
            if (FotoDoFuncionario.Nome(eu.Foto) != nome)
                return null;
        }

        return armazenamento.Abrir(nome);
    }
}
