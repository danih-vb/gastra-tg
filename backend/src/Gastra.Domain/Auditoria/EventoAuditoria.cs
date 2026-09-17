namespace Gastra.Domain.Auditoria;

/// <summary>
/// Códigos fixos dos eventos de auditoria (GASTRA_Politica_Log_Auditoria.md, seção 4). Texto fixo, e não
/// enum, porque fica gravado no banco e precisa continuar legível mesmo se um evento deixar de existir.
/// </summary>
public static class EventoAuditoria
{
    // 4.1 Autenticação e contas
    public const string LoginSucesso = "LOGIN_SUCESSO";
    public const string LoginFalha = "LOGIN_FALHA";
    public const string AutenticadorVinculado = "AUTENTICADOR_VINCULADO";
    public const string SegundoFatorConfirmado = "SEGUNDO_FATOR_CONFIRMADO";
    public const string SegundoFatorRecusado = "SEGUNDO_FATOR_RECUSADO";
    public const string Logoff = "LOGOFF";
    public const string ContaCriada = "CONTA_CRIADA";
    public const string ContaEditada = "CONTA_EDITADA";
    public const string ContaInativada = "CONTA_INATIVADA";
    public const string ContaReativada = "CONTA_REATIVADA";

    // 4.2 Cardápio e salão
    public const string ItemCardapioCadastrado = "ITEM_CARDAPIO_CADASTRADO";
    public const string PrecoAlterado = "PRECO_ALTERADO";
    public const string DisponibilidadeAlterada = "DISPONIBILIDADE_ALTERADA";
    public const string PracaCadastrada = "PRACA_CADASTRADA";
    public const string PracaEditada = "PRACA_EDITADA";
    public const string MesaCadastrada = "MESA_CADASTRADA";
    public const string MesaEditada = "MESA_EDITADA";

    // 4.3 Comandas
    public const string ComandaAberta = "COMANDA_ABERTA";
    public const string ComposicaoAjustada = "COMPOSICAO_AJUSTADA";
    public const string ItemRegistrado = "ITEM_REGISTRADO";
    public const string ItemCancelado = "ITEM_CANCELADO";
    public const string RestricaoRegistrada = "RESTRICAO_REGISTRADA";
    public const string TaxaServicoRemovida = "TAXA_SERVICO_REMOVIDA";
    public const string ComandaFechada = "COMANDA_FECHADA";

    // 4.5 Alocação de garçons
    public const string SugestaoAlocacaoGerada = "SUGESTAO_ALOCACAO_GERADA";
    public const string AlocacaoAjustada = "ALOCACAO_AJUSTADA";
    public const string AlocacaoConfirmada = "ALOCACAO_CONFIRMADA";

    // 4.5 Análises
    public const string RelatorioBiConsultado = "RELATORIO_BI_CONSULTADO";
    public const string IndiceDesempenhoConsultado = "INDICE_DESEMPENHO_CONSULTADO";

    // Seção 7: retenção
    public const string AuditoriaEliminadaPorPrazo = "AUDITORIA_ELIMINADA_POR_PRAZO";
}
