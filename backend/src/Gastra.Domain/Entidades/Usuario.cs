using Gastra.Domain.Enums;

namespace Gastra.Domain.Entidades;

public class Usuario : EntidadeBase
{
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public PapelUsuario Papel { get; private set; }
    public bool Ativo { get; private set; }

    /// <summary>Segredo do app autenticador, já protegido (criptografado). Nulo até a configuração.</summary>
    public string? SegredoTotp { get; private set; }

    /// <summary>
    /// Todo token de acesso carrega esta chave. Ao trocá-la, todos os tokens emitidos antes deixam de
    /// valer — é o que faz o logoff (RF17) e a inativação (RF18) valerem imediatamente.
    /// </summary>
    public Guid ChaveSessao { get; private set; }

    public bool SegundoFatorConfigurado => SegredoTotp is not null;

    // Usado pelo Entity Framework ao ler do banco.
    private Usuario()
    {
    }

    public Usuario(string nome, string email, string senhaHash, PapelUsuario papel)
    {
        Nome = nome;
        Email = NormalizarEmail(email);
        SenhaHash = senhaHash;
        Papel = papel;
        Ativo = true;
        ChaveSessao = Guid.NewGuid();
    }

    /// <summary>RF16: Gerente e Coordenador precisam do segundo fator.</summary>
    public bool ExigeSegundoFator() => Papel is PapelUsuario.Gerente or PapelUsuario.Coordenador;

    /// <summary>RN07: o segredo é gerado uma única vez, na vinculação do app autenticador.</summary>
    public void DefinirSegredoTotp(string segredoProtegido)
    {
        if (SegundoFatorConfigurado)
            throw new InvalidOperationException("O segredo do segundo fator já foi definido.");

        SegredoTotp = segredoProtegido;
    }

    /// <summary>RF17: invalida todos os tokens de acesso emitidos até agora.</summary>
    public void EncerrarSessoes() => ChaveSessao = Guid.NewGuid();

    /// <summary>
    /// UC04: o Gerente define uma senha nova para quem perdeu a sua (#141). As sessões abertas caem junto, para o
    /// acesso antigo não continuar valendo.
    /// </summary>
    public void RedefinirSenha(string senhaHash)
    {
        SenhaHash = senhaHash;
        EncerrarSessoes();
    }

    /// <summary>
    /// UC04: desfaz a vinculação do app autenticador de quem perdeu o celular (#141). O próximo login volta a pedir a
    /// configuração, com um segredo novo — o antigo nunca é reexibido (RN07).
    /// </summary>
    public void ReiniciarSegundoFator()
    {
        if (!ExigeSegundoFator())
            throw new InvalidOperationException("Este papel não usa segundo fator.");

        SegredoTotp = null;
        EncerrarSessoes();
    }

    /// <summary>
    /// RF18: edição da conta. O papel vai dentro do token; se ele muda, os tokens antigos deixam de
    /// valer para ninguém continuar com as permissões anteriores.
    /// </summary>
    public void AtualizarDados(string nome, string email, PapelUsuario papel)
    {
        if (Papel != papel)
            EncerrarSessoes();

        Nome = nome;
        Email = NormalizarEmail(email);
        Papel = papel;
    }

    /// <summary>RF18: inativação (soft delete) com perda imediata de acesso.</summary>
    public void Inativar()
    {
        Ativo = false;
        EncerrarSessoes();
    }

    /// <summary>Desfaz a inativação: a conta volta com a mesma senha e o mesmo autenticador.</summary>
    public void Reativar() => Ativo = true;

    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
