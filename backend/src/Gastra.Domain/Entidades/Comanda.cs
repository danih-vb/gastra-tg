using Gastra.Domain.Enums;

namespace Gastra.Domain.Entidades;

/// <summary>
/// Conta de uma mesa, da abertura ao fechamento (UC10–UC14). É a raiz do núcleo: os itens do pedido
/// e as restrições alimentares só existem dentro dela, e é por aqui que as regras passam.
/// </summary>
public class Comanda : EntidadeBase
{
    /// <summary>RF04: taxa de serviço de 10% sobre o subtotal, quando o cliente não pede a remoção.</summary>
    public const decimal PercentualTaxaServico = 0.10m;

    private readonly List<ItemDoPedido> _itens = [];
    private readonly List<RestricaoAlimentar> _restricoes = [];

    public int MesaId { get; private set; }
    public int GarcomId { get; private set; }
    public DateTime DataHoraAbertura { get; private set; }
    public DateTime? DataHoraFechamento { get; private set; }
    public StatusComanda Status { get; private set; }
    public int QuantidadePessoas { get; private set; }
    public ComposicaoMesa Composicao { get; private set; }
    public bool TaxaServicoRemovida { get; private set; }

    /// <summary>
    /// Verdadeiro quando o garçom trocou a composição sugerida por outra. A partir daí o sistema não
    /// reclassifica mais sozinho: quem está na mesa vê o que o sistema não vê (decisão da dupla, 16/09).
    /// </summary>
    public bool ComposicaoAjustadaManualmente { get; private set; }

    /// <summary>Token da consulta do cliente por QR code (UC20), sem login. Não expira no fechamento.</summary>
    public string CodigoAcessoCliente { get; private set; } = string.Empty;

    public IReadOnlyCollection<ItemDoPedido> Itens => _itens;
    public IReadOnlyCollection<RestricaoAlimentar> Restricoes => _restricoes;

    /// <summary>
    /// RN04: a restrição alimentar é dado do cliente e só pode ser vista por quem atende a mesa (Garçom
    /// ou Metre) enquanto a comanda está aberta. Gestão e comanda fechada recebem a lista vazia.
    /// </summary>
    public IReadOnlyCollection<RestricaoAlimentar> RestricoesVisiveisPara(PapelUsuario papel) =>
        Status == StatusComanda.Aberta && papel is (PapelUsuario.Garcom or PapelUsuario.Metre) ? _restricoes : [];

    // Usado pelo Entity Framework ao ler do banco.
    private Comanda()
    {
    }

    public Comanda(int mesaId, int garcomId, int quantidadePessoas)
    {
        if (quantidadePessoas < 1)
            throw new ArgumentOutOfRangeException(nameof(quantidadePessoas), "A mesa precisa ter pelo menos uma pessoa.");

        MesaId = mesaId;
        GarcomId = garcomId;
        QuantidadePessoas = quantidadePessoas;
        Composicao = SugerirComposicao(quantidadePessoas);
        DataHoraAbertura = DateTime.UtcNow;
        Status = StatusComanda.Aberta;
        CodigoAcessoCliente = Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// RN01: classifica a mesa pela quantidade de pessoas e pela presença de item infantil já pedido.
    /// Na abertura ainda não há itens, então a sugestão sai só pela quantidade — e vira Família
    /// assim que um item infantil é lançado numa mesa de 3 ou mais pessoas. O fato de já ter havido
    /// item infantil fica guardado na própria composição (Família), sem coluna extra no banco.
    /// </summary>
    public ComposicaoMesa SugerirComposicao(int quantidadePessoas) =>
        Classificar(quantidadePessoas, Composicao == ComposicaoMesa.Familia);

    /// <summary>
    /// RF02: o garçom confirma a sugestão ou ajusta, inclusive a quantidade de pessoas. Um ajuste manual
    /// passa a valer sobre a regra automática.
    /// </summary>
    public void ConfirmarComposicao(int quantidadePessoas, ComposicaoMesa composicao)
    {
        GarantirAberta();

        if (quantidadePessoas < 1)
            throw new ArgumentOutOfRangeException(nameof(quantidadePessoas), "A mesa precisa ter pelo menos uma pessoa.");

        if (!Enum.IsDefined(composicao))
            throw new ArgumentOutOfRangeException(nameof(composicao), "Composição inválida.");

        // Confirmar a sugestão não congela nada; trocar por outra, sim.
        ComposicaoAjustadaManualmente = composicao != SugerirComposicao(quantidadePessoas);
        QuantidadePessoas = quantidadePessoas;
        Composicao = composicao;
    }

    /// <summary>RF03: lança um item, copiando o preço vigente no cardápio.</summary>
    public ItemDoPedido AdicionarItem(ItemDoCardapio item, int quantidade)
    {
        GarantirAberta();

        if (!item.Disponivel)
            throw new InvalidOperationException($"O item \"{item.Nome}\" está indisponível.");

        var pedido = new ItemDoPedido(item.Id, quantidade, item.Preco);
        _itens.Add(pedido);

        // RN01: item infantil em mesa de 3 ou mais pessoas reclassifica a composição — a não ser que
        // o garçom já tenha ajustado a composição na mão.
        if (!ComposicaoAjustadaManualmente && item.FlagsDieteticas.Contains(FlagDietetica.OpcaoInfantil))
            Composicao = Classificar(QuantidadePessoas, true);

        return pedido;
    }

    /// <summary>RF14: restrição informada voluntariamente, vinculada à comanda ativa.</summary>
    public RestricaoAlimentar RegistrarRestricao(CategoriaRestricao categoria, string? observacaoLivre)
    {
        GarantirAberta();

        var restricao = new RestricaoAlimentar(categoria, observacaoLivre);
        _restricoes.Add(restricao);
        return restricao;
    }

    /// <summary>RF04: o cliente pode pedir a remoção da taxa antes do fechamento.</summary>
    public void RemoverTaxaServico()
    {
        GarantirAberta();
        TaxaServicoRemovida = true;
    }

    /// <summary>RN02: itens ainda pendentes impedem o fechamento e ficam visíveis como lembrete.</summary>
    public bool PossuiPendencias() => _itens.Any(i => i.Status == StatusItemPedido.Pendente);

    public decimal CalcularSubtotal() => _itens.Sum(i => i.CalcularValor());

    public decimal CalcularTaxaServico() =>
        TaxaServicoRemovida ? 0 : Math.Round(CalcularSubtotal() * PercentualTaxaServico, 2);

    public decimal CalcularTotal() => CalcularSubtotal() + CalcularTaxaServico();

    /// <summary>
    /// UC14: fecha a conta. Só é possível sem itens pendentes (RN02). No fechamento, a observação
    /// livre das restrições é apagada e sobra apenas a categoria (decisão de LGPD da dupla).
    /// </summary>
    public void Fechar()
    {
        GarantirAberta();

        if (PossuiPendencias())
            throw new InvalidOperationException("Há itens pendentes: entregue ou cancele cada um antes de fechar.");

        foreach (var restricao in _restricoes)
            restricao.ApagarObservacaoLivre();

        Status = StatusComanda.Fechada;
        DataHoraFechamento = DateTime.UtcNow;
    }

    private void GarantirAberta()
    {
        if (Status != StatusComanda.Aberta)
            throw new InvalidOperationException("A comanda não está aberta.");
    }

    private static ComposicaoMesa Classificar(int pessoas, bool possuiItemInfantil) => pessoas switch
    {
        1 => ComposicaoMesa.Solo,
        2 => ComposicaoMesa.Casal,
        >= 3 when possuiItemInfantil => ComposicaoMesa.Familia,
        3 or 4 => ComposicaoMesa.GrupoPequeno,
        _ => ComposicaoMesa.GrupoGrande,
    };
}
