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
    public static string ImagemInvalida => Obter(nameof(ImagemInvalida));
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
    public static string RedefinirPropriaSenha => Obter(nameof(RedefinirPropriaSenha));
    public static string ReiniciarProprioSegundoFator => Obter(nameof(ReiniciarProprioSegundoFator));
    public static string PapelSemSegundoFator => Obter(nameof(PapelSemSegundoFator));
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
    public static string PracaNaoEncontrada => Obter(nameof(PracaNaoEncontrada));
    public static string CodigoPracaObrigatorio => Obter(nameof(CodigoPracaObrigatorio));
    public static string CodigoPracaMuitoLongo => Obter(nameof(CodigoPracaMuitoLongo));
    public static string CodigoPracaJaCadastrado => Obter(nameof(CodigoPracaJaCadastrado));
    public static string QuantidadeGarconsInvalida => Obter(nameof(QuantidadeGarconsInvalida));
    public static string NumeroMesaObrigatorio => Obter(nameof(NumeroMesaObrigatorio));
    public static string NumeroMesaMuitoLongo => Obter(nameof(NumeroMesaMuitoLongo));
    public static string NumeroMesaJaCadastrado => Obter(nameof(NumeroMesaJaCadastrado));
    public static string CapacidadeMesaInvalida => Obter(nameof(CapacidadeMesaInvalida));
    public static string AlocacaoSemGarcons => Obter(nameof(AlocacaoSemGarcons));
    public static string AlocacaoGarcomInvalido => Obter(nameof(AlocacaoGarcomInvalido));
    public static string AlocacaoSemPracas => Obter(nameof(AlocacaoSemPracas));
    public static string AlocacaoSemVagas => Obter(nameof(AlocacaoSemVagas));
    public static string AlocacaoJaConfirmada => Obter(nameof(AlocacaoJaConfirmada));
    public static string AlocacaoNaoEncontrada => Obter(nameof(AlocacaoNaoEncontrada));
    public static string PracaSemVaga => Obter(nameof(PracaSemVaga));
    public static string TrocaComOProprioGarcom => Obter(nameof(TrocaComOProprioGarcom));
    public static string GarcomDaTrocaForaDaPraca => Obter(nameof(GarcomDaTrocaForaDaPraca));
    public static string TrocaNaMesmaPraca => Obter(nameof(TrocaNaMesmaPraca));
    public static string PeriodoInvalido => Obter(nameof(PeriodoInvalido));
    public static string DataAlocacaoObrigatoria => Obter(nameof(DataAlocacaoObrigatoria));
    public static string PeriodoRelatorioInvalido => Obter(nameof(PeriodoRelatorioInvalido));
    public static string PeriodoRelatorioLongoDemais => Obter(nameof(PeriodoRelatorioLongoDemais));
    public static string DescricaoPromocaoObrigatoria => Obter(nameof(DescricaoPromocaoObrigatoria));
    public static string DescricaoPromocaoMuitoLonga => Obter(nameof(DescricaoPromocaoMuitoLonga));
    public static string TipoDescontoInvalido => Obter(nameof(TipoDescontoInvalido));
    public static string ValorDescontoInvalido => Obter(nameof(ValorDescontoInvalido));
    public static string PeriodoPromocaoInvalido => Obter(nameof(PeriodoPromocaoInvalido));
    public static string PromocaoSemItens => Obter(nameof(PromocaoSemItens));
    public static string DescontoFixoMaiorQuePreco => Obter(nameof(DescontoFixoMaiorQuePreco));
    public static string PromocaoNaoEncontrada => Obter(nameof(PromocaoNaoEncontrada));
    public static string PromocaoJaDesativada => Obter(nameof(PromocaoJaDesativada));
    public static string NotaAvaliacaoInvalida => Obter(nameof(NotaAvaliacaoInvalida));
    public static string ComentarioMuitoLongo => Obter(nameof(ComentarioMuitoLongo));
    public static string ComandaJaAvaliada => Obter(nameof(ComandaJaAvaliada));
    public static string AvaliacaoForaDoPrazo => Obter(nameof(AvaliacaoForaDoPrazo));

    private static string Obter(string chave) =>
        Recursos.GetString(chave, CultureInfo.CurrentUICulture) ?? chave;
}
