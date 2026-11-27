namespace Gastra.Domain.Servicos;

/// <summary>
/// Onde ficam as fotos do cardápio enviadas pelo computador. São públicas: o cardápio digital mostra ao cliente, que
/// não faz login. O domínio só conhece o contrato; o disco fica na infraestrutura. O nome do arquivo é sempre gerado
/// aqui, nunca o que veio do usuário.
/// </summary>
public interface IArmazenamentoDeFotos
{
    /// <summary>Grava a foto e devolve o nome gerado para ela.</summary>
    Task<string> Salvar(byte[] conteudo, string extensao);

    /// <summary>Abre a foto para leitura; nulo se o nome não for de uma foto guardada.</summary>
    Stream? Abrir(string nome);

    /// <summary>Apaga a foto do disco. Nome que não é de foto guardada é ignorado.</summary>
    void Apagar(string nome);
}
