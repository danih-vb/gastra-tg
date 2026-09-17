using System.Text.Json;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;

namespace Gastra.Application.Auditoria;

/// <summary>
/// Monta e adiciona o registro de auditoria. Não grava sozinho: o registro entra no mesmo Commit da
/// operação auditada, e por isso não existe registro de uma operação que falhou ao gravar.
/// </summary>
public interface IRegistradorAuditoria
{
    /// <param name="evento">Código de <see cref="Gastra.Domain.Auditoria.EventoAuditoria"/>.</param>
    /// <param name="alvo">Registro afetado, quando houver.</param>
    /// <param name="detalhes">
    /// Só os campos permitidos pela política de log (seção 4). Nunca senha, código TOTP, token, código de
    /// acesso do cliente, categoria ou observação da restrição, nome ou e-mail.
    /// </param>
    /// <param name="ator">Quem agiu, quando ainda não há token (login). Sem ele, vem do token da requisição.</param>
    /// <param name="comIp">Só em eventos de autenticação (política de log, 4.1).</param>
    Task Registrar(
        string evento,
        ResultadoAuditoria resultado = ResultadoAuditoria.Sucesso,
        (string Entidade, int Id)? alvo = null,
        object? detalhes = null,
        Usuario? ator = null,
        bool comIp = false);
}

public class RegistradorAuditoria(
    IRepositorioAuditoria repositorio,
    IUsuarioLogado usuarioLogado,
    IContextoRequisicao contexto) : IRegistradorAuditoria
{
    // Mesmo formato do exemplo da política de log: {"praca_sugerida": 2}.
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    public Task Registrar(
        string evento,
        ResultadoAuditoria resultado = ResultadoAuditoria.Sucesso,
        (string Entidade, int Id)? alvo = null,
        object? detalhes = null,
        Usuario? ator = null,
        bool comIp = false)
    {
        var identificacao = ator is null ? usuarioLogado.ObterIdentificacao() : (ator.Id, ator.Papel);

        var registro = new RegistroAuditoria(
            evento,
            resultado,
            identificacao?.Id,
            identificacao?.Papel,
            alvo?.Entidade,
            alvo?.Id,
            detalhes is null ? null : JsonSerializer.Serialize(detalhes, Json),
            comIp ? contexto.ObterIp() : null,
            contexto.ObterIdCorrelacao());

        return repositorio.Adicionar(registro);
    }
}
