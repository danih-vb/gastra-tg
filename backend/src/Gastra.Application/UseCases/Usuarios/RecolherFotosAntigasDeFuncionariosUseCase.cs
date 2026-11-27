using Gastra.Application.UseCases.Cardapio;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Servicos;

namespace Gastra.Application.UseCases.Usuarios;

/// <summary>
/// Até a versão 1.0, a foto do funcionário era enviada pela mesma rota das fotos do cardápio e ficava pública
/// (/api/cardapio/fotos/...). Ao subir a API, as contas que ainda apontam para lá ganham uma cópia na pasta das fotos
/// dos funcionários, e o arquivo público é apagado, a não ser que algum item do cardápio use a mesma imagem.
/// Se o arquivo não estiver na pasta (por exemplo, a API rodando fora do Docker, com outra pasta de fotos), a conta
/// fica como está: apagar a referência aí perderia a foto que existe no servidor de verdade.
/// </summary>
public interface IRecolherFotosAntigasDeFuncionariosUseCase
{
    /// <returns>Quantas fotos foram tiradas da pasta pública.</returns>
    Task<int> Executar();
}

public class RecolherFotosAntigasDeFuncionariosUseCase(
    IRepositorioUsuario usuarios,
    IRepositorioItemCardapio itens,
    IArmazenamentoDeFotos fotosDoCardapio,
    IArmazenamentoDeFotosDeFuncionarios fotosDosFuncionarios,
    IUnitOfWork unitOfWork) : IRecolherFotosAntigasDeFuncionariosUseCase
{
    public async Task<int> Executar()
    {
        var ids = (await usuarios.ListarTodos())
            .Where(u => u.Foto?.StartsWith(EnviarFotoUseCase.Prefixo, StringComparison.Ordinal) == true)
            .Select(u => u.Id)
            .ToList();
        if (ids.Count == 0)
            return 0;

        var usadasNoCardapio = (await itens.ListarTodos()).Select(i => i.Imagem).OfType<string>().ToHashSet();
        var copiadas = new List<string>();
        var ajustadas = 0;
        var publicasParaApagar = new HashSet<string>();

        foreach (var id in ids)
        {
            var usuario = (await usuarios.ObterPorId(id))!;
            var endereco = usuario.Foto!;
            var nome = endereco[EnviarFotoUseCase.Prefixo.Length..];

            await using var arquivo = fotosDoCardapio.Abrir(nome);
            if (arquivo is null)
                continue;

            using var memoria = new MemoryStream();
            await arquivo.CopyToAsync(memoria);
            var copia = await fotosDosFuncionarios.Salvar(memoria.ToArray(), Path.GetExtension(nome).TrimStart('.'));
            copiadas.Add(copia);
            usuario.DefinirFoto(FotoDoFuncionario.Endereco(copia));
            ajustadas++;

            if (!usadasNoCardapio.Contains(endereco))
                publicasParaApagar.Add(nome);
        }

        if (ajustadas == 0)
            return 0;

        try
        {
            await unitOfWork.Commit();
        }
        catch
        {
            foreach (var copia in copiadas)
                fotosDosFuncionarios.Apagar(copia);
            throw;
        }

        foreach (var nome in publicasParaApagar)
            fotosDoCardapio.Apagar(nome);

        return ajustadas;
    }
}
