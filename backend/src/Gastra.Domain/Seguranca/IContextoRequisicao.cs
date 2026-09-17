namespace Gastra.Domain.Seguranca;

/// <summary>Dados técnicos da requisição atual, usados só pela auditoria.</summary>
public interface IContextoRequisicao
{
    /// <summary>IP de quem chamou. Só vai para a auditoria em eventos de autenticação (política de log, 4.1).</summary>
    string? ObterIp();

    /// <summary>Liga o registro de auditoria ao log técnico da mesma requisição.</summary>
    string? ObterIdCorrelacao();
}
