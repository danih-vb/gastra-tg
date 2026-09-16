using Gastra.Domain.Enums;

namespace Gastra.Domain.Entidades;

/// <summary>
/// Registro de auditoria: quem fez o quê e quando (GASTRA_Politica_Log_Auditoria.md). A tabela é
/// somente de inserção — por isso a entidade não tem nenhum método que altere o registro depois de
/// criado. O conteúdo de <see cref="Detalhes"/> é limitado aos campos permitidos pela política:
/// nunca senha, segredo TOTP, token, código de acesso do cliente ou restrição alimentar.
/// </summary>
public class RegistroAuditoria : EntidadeBase
{
    public DateTime DataHoraUtc { get; private set; }
    public string Evento { get; private set; } = string.Empty;
    public ResultadoAuditoria Resultado { get; private set; }

    /// <summary>Nulo apenas em falha de login com e-mail que não existe (a política proíbe registrar o e-mail).</summary>
    public int? UsuarioId { get; private set; }

    public PapelUsuario? Papel { get; private set; }
    public string? Entidade { get; private set; }
    public int? IdEntidade { get; private set; }
    public string? Detalhes { get; private set; }

    /// <summary>Somente em eventos de autenticação (seção 4.1 da política).</summary>
    public string? Ip { get; private set; }

    /// <summary>Liga este evento ao log técnico da mesma requisição.</summary>
    public string? IdCorrelacao { get; private set; }

    // Usado pelo Entity Framework ao ler do banco.
    private RegistroAuditoria()
    {
    }

    public RegistroAuditoria(
        string evento,
        ResultadoAuditoria resultado,
        int? usuarioId = null,
        PapelUsuario? papel = null,
        string? entidade = null,
        int? idEntidade = null,
        string? detalhes = null,
        string? ip = null,
        string? idCorrelacao = null)
    {
        if (string.IsNullOrWhiteSpace(evento))
            throw new ArgumentException("O evento é obrigatório.", nameof(evento));

        DataHoraUtc = DateTime.UtcNow;
        Evento = evento;
        Resultado = resultado;
        UsuarioId = usuarioId;
        Papel = papel;
        Entidade = entidade;
        IdEntidade = idEntidade;
        Detalhes = detalhes;
        Ip = ip;
        IdCorrelacao = idCorrelacao;
    }
}
