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

    /// <summary>RN09: quantas tentativas de acesso erradas seguidas bloqueiam a conta.</summary>
    public const int MaximoTentativasFalhas = 5;

    /// <summary>RN09: por quanto tempo a conta fica bloqueada.</summary>
    public static readonly TimeSpan DuracaoBloqueio = TimeSpan.FromMinutes(15);

    /// <summary>Tentativas erradas (senha ou código do segundo fator) desde o último acesso completo.</summary>
    public int TentativasFalhas { get; private set; }

    /// <summary>Até quando a conta recusa qualquer tentativa, mesmo com a senha certa (RN09). Nulo = livre.</summary>
    public DateTime? BloqueadaAte { get; private set; }

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

    /// <summary>RN09: bloqueada enquanto o prazo não passar.</summary>
    public bool EstaBloqueada(DateTime agora) => BloqueadaAte > agora;

    /// <summary>
    /// RN09: conta uma tentativa errada, de senha ou de código. A quinta seguida bloqueia a conta e recomeça a contagem,
    /// para o próximo bloqueio também exigir cinco erros. Durante o bloqueio nada é contado: senão cada tentativa de um
    /// atacante empurraria o fim do bloqueio e o dono não entraria nunca mais.
    /// </summary>
    /// <returns>Verdadeiro quando esta tentativa bloqueou a conta.</returns>
    public bool RegistrarTentativaFalha(DateTime agora)
    {
        if (EstaBloqueada(agora))
            return false;

        TentativasFalhas++;
        if (TentativasFalhas < MaximoTentativasFalhas)
            return false;

        TentativasFalhas = 0;
        BloqueadaAte = agora.Add(DuracaoBloqueio);
        return true;
    }

    /// <summary>
    /// RN09: só um acesso completo zera a contagem — com segundo fator, depois do código. Acertar só a senha não zera,
    /// senão bastaria logar de novo para ganhar mais cinco palpites de código.
    /// </summary>
    public void RegistrarAcessoCompleto()
    {
        TentativasFalhas = 0;
        BloqueadaAte = null;
    }

    /// <summary>RF17: invalida todos os tokens de acesso emitidos até agora.</summary>
    public void EncerrarSessoes() => ChaveSessao = Guid.NewGuid();

    /// <summary>
    /// UC04: o Gerente define uma senha nova para quem perdeu a sua (#141). As sessões abertas caem junto, para o
    /// acesso antigo não continuar valendo, e um bloqueio por tentativas (RN09) é desfeito: quem esqueceu a senha
    /// costuma ser justamente quem errou cinco vezes.
    /// </summary>
    public void RedefinirSenha(string senhaHash)
    {
        SenhaHash = senhaHash;
        RegistrarAcessoCompleto();
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
