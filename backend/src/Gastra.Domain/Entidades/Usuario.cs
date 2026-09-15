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

    /// <summary>RF18: inativação (soft delete) com perda imediata de acesso.</summary>
    public void Inativar()
    {
        Ativo = false;
        EncerrarSessoes();
    }

    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
