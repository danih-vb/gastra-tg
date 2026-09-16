using System.Globalization;
using System.Resources;

namespace Gastra.Exceptions;

/// <summary>
/// Mensagens de erro traduzidas. O idioma vem de <see cref="CultureInfo.CurrentUICulture"/>, definido
/// por requisição a partir do cabeçalho Accept-Language: MensagensErro.resx (pt-BR, padrão) ou
/// MensagensErro.en.resx (inglês).
/// </summary>
public static class MensagensErro
{
    private static readonly ResourceManager Recursos =
        new("Gastra.Exceptions.MensagensErro", typeof(MensagensErro).Assembly);

    public static string NomeObrigatorio => Obter(nameof(NomeObrigatorio));
    public static string NomeMuitoLongo => Obter(nameof(NomeMuitoLongo));
    public static string DescricaoMuitoLonga => Obter(nameof(DescricaoMuitoLonga));
    public static string PrecoInvalido => Obter(nameof(PrecoInvalido));
    public static string CategoriaInvalida => Obter(nameof(CategoriaInvalida));
    public static string FlagDieteticaInvalida => Obter(nameof(FlagDieteticaInvalida));
    public static string ItemCardapioNaoEncontrado => Obter(nameof(ItemCardapioNaoEncontrado));
    public static string RequisicaoInvalida => Obter(nameof(RequisicaoInvalida));
    public static string ErroInesperado => Obter(nameof(ErroInesperado));
    public static string EmailObrigatorio => Obter(nameof(EmailObrigatorio));
    public static string SenhaObrigatoria => Obter(nameof(SenhaObrigatoria));
    public static string CredenciaisInvalidas => Obter(nameof(CredenciaisInvalidas));
    public static string TokenSegundoFatorInvalido => Obter(nameof(TokenSegundoFatorInvalido));
    public static string CodigoSegundoFatorInvalido => Obter(nameof(CodigoSegundoFatorInvalido));
    public static string SegundoFatorJaConfigurado => Obter(nameof(SegundoFatorJaConfigurado));
    public static string SegundoFatorNaoConfigurado => Obter(nameof(SegundoFatorNaoConfigurado));
    public static string NaoAutenticado => Obter(nameof(NaoAutenticado));
    public static string AcessoNegado => Obter(nameof(AcessoNegado));
    public static string EmailInvalido => Obter(nameof(EmailInvalido));
    public static string EmailMuitoLongo => Obter(nameof(EmailMuitoLongo));
    public static string SenhaCurta => Obter(nameof(SenhaCurta));
    public static string SenhaLonga => Obter(nameof(SenhaLonga));
    public static string PapelInvalido => Obter(nameof(PapelInvalido));
    public static string EmailJaCadastrado => Obter(nameof(EmailJaCadastrado));
    public static string UsuarioNaoEncontrado => Obter(nameof(UsuarioNaoEncontrado));
    public static string AlterarProprioPapel => Obter(nameof(AlterarProprioPapel));
    public static string InativarPropriaConta => Obter(nameof(InativarPropriaConta));
    public static string ComandaNaoEncontrada => Obter(nameof(ComandaNaoEncontrada));
    public static string MesaNaoEncontrada => Obter(nameof(MesaNaoEncontrada));
    public static string ItemPedidoNaoEncontrado => Obter(nameof(ItemPedidoNaoEncontrado));
    public static string ComandaNaoEstaAberta => Obter(nameof(ComandaNaoEstaAberta));
    public static string QuantidadePessoasInvalida => Obter(nameof(QuantidadePessoasInvalida));
    public static string QuantidadeItemInvalida => Obter(nameof(QuantidadeItemInvalida));
    public static string ComposicaoInvalida => Obter(nameof(ComposicaoInvalida));
    public static string CategoriaRestricaoInvalida => Obter(nameof(CategoriaRestricaoInvalida));
    public static string ObservacaoMuitoLonga => Obter(nameof(ObservacaoMuitoLonga));
    public static string SituacaoItemInvalida => Obter(nameof(SituacaoItemInvalida));
    public static string MotivoCancelamentoObrigatorio => Obter(nameof(MotivoCancelamentoObrigatorio));
    public static string MotivoCancelamentoInvalido => Obter(nameof(MotivoCancelamentoInvalido));
    public static string ItemNaoEstaPendente => Obter(nameof(ItemNaoEstaPendente));
    public static string ItemCardapioIndisponivel => Obter(nameof(ItemCardapioIndisponivel));
    public static string ComandaComItensPendentes => Obter(nameof(ComandaComItensPendentes));

    private static string Obter(string chave) =>
        Recursos.GetString(chave, CultureInfo.CurrentUICulture) ?? chave;
}
