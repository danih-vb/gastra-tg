namespace Gastra.Domain.Servicos;

/// <summary>
/// Onde ficam as fotos dos funcionários (crachá do sistema). Ficam longe das do cardápio de propósito: aquelas são
/// públicas, e a foto de uma pessoa é dado pessoal (LGPD), que só quem está logado pode ver. Cada arquivo pertence a
/// uma única conta e é apagado quando a foto é trocada ou tirada, e quando a conta é inativada.
/// </summary>
public interface IArmazenamentoDeFotosDeFuncionarios
{
    /// <summary>Grava a foto e devolve o nome gerado para ela.</summary>
    Task<string> Salvar(byte[] conteudo, string extensao);

    /// <summary>Abre a foto para leitura; nulo se o nome não for de uma foto guardada.</summary>
    Stream? Abrir(string nome);

    /// <summary>Apaga a foto do disco. Nome que não é de foto guardada é ignorado.</summary>
    void Apagar(string nome);
}
