using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace GastraSemeador;

/// <summary>
/// Histórico de turnos já encerrados: alocações confirmadas e comandas fechadas (#182).
///
/// É o que faz as views de BI, o índice de desempenho e as regras de associação terem o que mostrar.
/// Os perfis de consumo, as probabilidades e as combinações de itens são os mesmos do <c>simulador.py</c>, para
/// que a recomendação treine sobre o mesmo padrão que os notebooks usaram — inclusive os dois gabaritos da
/// validação: a combinação plantada "moqueca puxa arroz de coco" (regras de associação, #185) e os três perfis
/// (clusterização).
/// </summary>
internal static class Historico
{
    /// <summary>
    /// 70 dias: passa dos 60 pedidos na issue e cobre com folga a janela de 30 dias que a RN03 usa para o
    /// faturamento médio por turno, de modo que o fator já entra estabilizado.
    /// </summary>
    private const int DiasDeHistorico = 70;

    /// <summary>Chance de cada garçom aparecer no turno — os mesmos 85% da calibração da RN03.</summary>
    private const double ChanceDePresenca = 0.85;

    /// <summary>Uma parte pequena dos itens é cancelada, como acontece no salão.</summary>
    private const double ChanceDeCancelamento = 0.04;

    /// <summary>
    /// Movimento de cada praça por turno, em comandas. É propriedade da **praça**, e não do garçom: quem
    /// estiver alocado ali divide esse movimento. Com ticket médio perto de R$ 115, os números reproduzem a
    /// proporção 3:2:1 da calibração da RN03 (R$ 1.800 / 1.200 / 600 por turno).
    /// </summary>
    private static readonly Dictionary<string, int> ComandasPorTurno = new() { ["A"] = 15, ["B"] = 10, ["C"] = 5 };

    /// <summary>
    /// Perfis de consumo plantados, os mesmos do <c>simulador.py</c>: cada comanda nasce de um perfil, que dá a chance
    /// de cada item. É o gabarito da clusterização (RF09): o algoritmo recebe só os itens pedidos e precisa
    /// redescobrir estes três grupos. Item fora do perfil entra com <see cref="ChanceForaDoPerfil"/>.
    /// </summary>
    private static readonly Dictionary<string, (string Nome, double Chance)[]> Perfis = new()
    {
        ["executivo"] =
        [
            ("Salada da horta", 0.55), ("Risoto de cogumelos", 0.50), ("Suco natural de laranja", 0.45),
            ("Água com gás", 0.40), ("Café coado", 0.45),
        ],
        ["frutos_do_mar"] =
        [
            ("Moqueca de peixe", 0.60), ("Bobó de camarão", 0.35), ("Bolinho de bacalhau (6 un.)", 0.50),
            ("Caipirinha", 0.45), ("Petit gâteau", 0.35),
        ],
        ["familia"] =
        [
            ("Porção infantil de frango", 0.75), ("Suco natural de laranja", 0.40), ("Arroz de coco", 0.30),
            ("Pudim de leite", 0.45), ("Sorvete de tapioca", 0.45),
        ],
    };

    /// <summary>Chance de um item que não é do perfil entrar na comanda: ruído, como no salão de verdade.</summary>
    private const double ChanceForaDoPerfil = 0.03;

    /// <summary>
    /// Quem senta à mesa muda o perfil mais provável, como no <c>simulador.py</c>. Período e pessoas escolhem o perfil,
    /// mas não entram na clusterização (RN05): servem só para interpretar os grupos que ela encontra.
    /// </summary>
    private static string SortearPerfil(Random sorteio, PeriodoAlocacao periodo, int pessoas)
    {
        int[] pesos = (periodo, pessoas <= 2) switch
        {
            (PeriodoAlocacao.Almoco, true) => [65, 25, 10],
            (PeriodoAlocacao.Almoco, false) => [15, 25, 60],
            (_, true) => [15, 75, 10],
            _ => [5, 45, 50],
        };
        return Sortear(sorteio, ["executivo", "frutos_do_mar", "familia"], pesos);
    }

    /// <summary>
    /// Fator de venda de cada garçom, do menos ao mais vendedor. É o que dá ao índice de desempenho algo para
    /// diferenciar: com o rodízio de praças perfeitamente justo, sem isso todo mundo faturaria igual e a RN03
    /// ficaria sem contraste. Na prática representa quem sugere entrada, sobremesa e a segunda rodada.
    /// </summary>
    private static double FatorDeVenda(int posicao) => 0.70 + 0.12 * posicao;

    /// <summary>Combinações que o salão observa: se o primeiro entra, o segundo tende a entrar junto.</summary>
    private static readonly (string Gatilho, string Acompanhamento, double Chance)[] Combinacoes =
    [
        ("Moqueca de peixe", "Arroz de coco", 0.85),
        ("Moqueca de peixe", "Caipirinha", 0.60),
        ("Pudim de leite", "Café coado", 0.70),
        ("Porção infantil de frango", "Suco natural de laranja", 0.65),
    ];

    public static async Task Semear(GastraDbContext contexto, Cenario cenario, Configuracao configuracao, TextWriter saida)
    {
        if (await contexto.Comandas.AnyAsync())
        {
            saida.WriteLine("Histórico: já existem comandas no banco, então não mexo em nada.");
            return;
        }

        var sorteio = new Random(configuracao.Semente);
        var garcons = cenario.Garcons;
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3));
        var comandas = 0;
        var itens = 0;

        // Do dia mais antigo para o mais recente, parando ontem: o turno de hoje é o estado "ao vivo" (#183).
        for (var recuo = DiasDeHistorico; recuo >= 1; recuo--)
        {
            var dia = hoje.AddDays(-recuo);

            foreach (var periodo in new[] { PeriodoAlocacao.Almoco, PeriodoAlocacao.Jantar })
            {
                var presentes = garcons.Where(_ => sorteio.NextDouble() < ChanceDePresenca).ToList();
                if (presentes.Count == 0)
                    continue;

                var turno = Distribuir(presentes, cenario.Pracas, dia, periodo).ToList();

                foreach (var (garcom, praca) in turno)
                {
                    var alocacao = new Alocacao(dia, periodo, garcom.Id, praca.Id);
                    alocacao.Confirmar();
                    contexto.Alocacoes.Add(alocacao);
                }

                // O movimento é da praça; quem está alocado nela divide as comandas do turno.
                foreach (var grupo in turno.GroupBy(t => t.Praca))
                {
                    var mesas = cenario.Mesas.Where(m => m.PracaId == grupo.Key.Id).ToList();
                    var doTurno = ComandasPorTurno[grupo.Key.Codigo] + sorteio.Next(-2, 3);
                    var equipe = grupo.Select(t => t.Garcom).ToList();

                    for (var i = 0; i < doTurno; i++)
                    {
                        var garcom = equipe[i % equipe.Count];
                        var fator = FatorDeVenda(garcons.IndexOf(garcom));
                        var comanda = CriarComanda(contexto, cenario, sorteio, garcom, mesas, dia, periodo, fator);
                        comandas++;
                        itens += comanda.Itens.Count;
                    }
                }
            }

            // Salva dia a dia: segura a memória e deixa o progresso visível em banco grande.
            await contexto.SaveChangesAsync();
            contexto.ChangeTracker.Clear();
        }

        saida.WriteLine($"Histórico: {DiasDeHistorico} dias, {comandas} comandas fechadas e {itens} itens.");
    }

    /// <summary>
    /// Distribui os presentes pelas praças respeitando as vagas. Primeiro **uma pessoa em cada praça**, para
    /// que nenhuma fique fechada, e só depois as vagas que sobraram, na ordem das praças. A fila gira a cada
    /// turno: assim a praça de alto potencial não cai sempre na mesma pessoa, e é esse rodízio que dá à RN03
    /// um histórico com variação para explicar.
    /// </summary>
    private static IEnumerable<(Usuario Garcom, Praca Praca)> Distribuir(
        List<Usuario> presentes,
        List<Praca> pracas,
        DateOnly dia,
        PeriodoAlocacao periodo)
    {
        var giro = dia.DayNumber * 2 + (periodo == PeriodoAlocacao.Jantar ? 1 : 0);
        var deslocamento = giro % presentes.Count;
        var fila = presentes.Skip(deslocamento).Concat(presentes.Take(deslocamento)).ToList();
        var ordenadas = pracas.OrderBy(p => p.Codigo).ToList();

        var indice = 0;
        var ocupadas = ordenadas.ToDictionary(p => p.Id, _ => 0);

        foreach (var praca in ordenadas)
        {
            if (indice >= fila.Count)
                yield break;

            ocupadas[praca.Id]++;
            yield return (fila[indice++], praca);
        }

        foreach (var praca in ordenadas)
        {
            while (ocupadas[praca.Id] < praca.QuantidadeGarcons && indice < fila.Count)
            {
                ocupadas[praca.Id]++;
                yield return (fila[indice++], praca);
            }
        }
    }

    private static Comanda CriarComanda(
        GastraDbContext contexto,
        Cenario cenario,
        Random sorteio,
        Usuario garcom,
        List<Mesa> mesas,
        DateOnly dia,
        PeriodoAlocacao periodo,
        double fatorDeVenda)
    {
        var mesa = mesas[sorteio.Next(mesas.Count)];
        var pessoas = Math.Min(mesa.Capacidade, Sortear(sorteio, [1, 2, 3, 4, 5, 6], [10, 35, 20, 20, 10, 5]));

        var comanda = new Comanda(mesa.Id, garcom.Id, pessoas);
        contexto.Comandas.Add(comanda);

        var escolhidos = EscolherItens(cenario.Cardapio, sorteio, fatorDeVenda, SortearPerfil(sorteio, periodo, pessoas));
        foreach (var item in escolhidos)
        {
            var quantidade = sorteio.NextDouble() < 0.25 * fatorDeVenda ? 2 : 1;
            var pedido = comanda.AdicionarItem(item, quantidade);

            if (sorteio.NextDouble() < ChanceDeCancelamento)
                pedido.Cancelar(SortearMotivo(sorteio));
            else
                pedido.MarcarEntregue();
        }

        // A taxa de serviço é retirada de vez em quando, como o cliente pede de vez em quando.
        if (sorteio.NextDouble() < 0.07)
            comanda.RemoverTaxaServico();

        comanda.Fechar();
        AjustarDatas(contexto, comanda, sorteio, dia, periodo);
        return comanda;
    }

    /// <summary>
    /// O construtor de <c>Comanda</c> carimba <c>DateTime.UtcNow</c> e não aceita data injetada (lacuna
    /// registrada no README). Aqui as datas são corrigidas pela API de propriedades do EF, que enxerga o
    /// setter privado — sem reflexão e sem afrouxar o domínio.
    ///
    /// Os horários são escolhidos em Brasília e gravados em UTC. A view converte de volta com
    /// <c>CONVERT_TZ</c>, e o corte do turno é às 17h locais: por isso o almoço fica entre 11h e 15h30 e o
    /// jantar entre 18h30 e 21h30, longe do corte e sem virar o dia na conversão.
    /// </summary>
    private static void AjustarDatas(
        GastraDbContext contexto,
        Comanda comanda,
        Random sorteio,
        DateOnly dia,
        PeriodoAlocacao periodo)
    {
        var minutoInicial = periodo == PeriodoAlocacao.Almoco
            ? 11 * 60 + sorteio.Next(0, 270)   // 11h00 às 15h29
            : 18 * 60 + 30 + sorteio.Next(0, 180); // 18h30 às 21h29

        var aberturaLocal = dia.ToDateTime(TimeOnly.MinValue).AddMinutes(minutoInicial);
        var duracao = TimeSpan.FromMinutes(45 + sorteio.Next(0, 65));

        var abertura = DateTime.SpecifyKind(aberturaLocal.AddHours(3), DateTimeKind.Utc);
        var fechamento = abertura + duracao;

        contexto.Entry(comanda).Property(c => c.DataHoraAbertura).CurrentValue = abertura;
        contexto.Entry(comanda).Property(c => c.DataHoraFechamento).CurrentValue = fechamento;

        // Os itens vão entrando ao longo do atendimento, e não todos no mesmo instante.
        var passo = duracao.TotalMinutes / (comanda.Itens.Count + 1);
        var ordem = 1;
        foreach (var item in comanda.Itens)
        {
            contexto.Entry(item).Property(i => i.DataHoraRegistro).CurrentValue = abertura.AddMinutes(passo * ordem);
            ordem++;
        }
    }

    /// <summary>Sorteia os itens da comanda pelas chances do perfil e aplica as combinações.</summary>
    private static List<ItemDoCardapio> EscolherItens(
        List<ItemDoCardapio> cardapio, Random sorteio, double fatorDeVenda, string perfil)
    {
        var nomes = new HashSet<string>();
        var chances = Perfis[perfil].ToDictionary(p => p.Nome, p => p.Chance);

        foreach (var item in cardapio)
        {
            if (sorteio.NextDouble() < chances.GetValueOrDefault(item.Nome, ChanceForaDoPerfil) * fatorDeVenda)
                nomes.Add(item.Nome);
        }

        foreach (var (gatilho, acompanhamento, chance) in Combinacoes)
        {
            if (nomes.Contains(gatilho) && sorteio.NextDouble() < chance)
                nomes.Add(acompanhamento);
        }

        // Comanda vazia não existe: quem senta pede alguma coisa.
        if (nomes.Count == 0)
            nomes.Add(cardapio[sorteio.Next(cardapio.Count)].Nome);

        return cardapio.Where(i => nomes.Contains(i.Nome)).ToList();
    }

    private static MotivoCancelamento SortearMotivo(Random sorteio) =>
        Sortear(sorteio,
            [MotivoCancelamento.ErroDeLancamento, MotivoCancelamento.ClienteDesistiu, MotivoCancelamento.ItemEmFalta],
            [50, 25, 25]);

    private static T Sortear<T>(Random sorteio, T[] valores, int[] pesos)
    {
        var alvo = sorteio.Next(pesos.Sum());
        var acumulado = 0;
        for (var i = 0; i < valores.Length; i++)
        {
            acumulado += pesos[i];
            if (alvo < acumulado)
                return valores[i];
        }

        return valores[^1];
    }
}
