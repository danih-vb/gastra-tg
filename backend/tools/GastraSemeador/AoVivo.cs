using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace GastraSemeador;

/// <summary>
/// O salão como ele está agora (#183): o turno de hoje com a alocação **ainda não confirmada**, mesas
/// abertas há algum tempo, itens esperando entrega e uma mesa com restrição registrada.
///
/// Existe para a demonstração começar com o restaurante em movimento, e não com telas vazias. O roteiro
/// de ponta a ponta precisa poder ser percorrido sem ninguém cadastrar nada à mão: o metre confirma o
/// turno que já está montado, o garçom entra e encontra mesas abertas, o cliente abre a conta de uma
/// delas pelo código.
/// </summary>
internal static class AoVivo
{
    /// <summary>Quantas mesas ficam abertas em cada praça quando a demonstração começa.</summary>
    private static readonly Dictionary<string, int> MesasAbertas = new() { ["A"] = 3, ["B"] = 2, ["C"] = 1 };

    /// <summary>Parte dos itens que fica esperando entrega — é o que dá o que fazer na tela "A entregar".</summary>
    private const double ChanceDePendente = 0.40;

    public static async Task Semear(GastraDbContext contexto, Cenario cenario, Configuracao configuracao, TextWriter saida)
    {
        var agoraLocal = DateTime.UtcNow.AddHours(-3);
        var hoje = DateOnly.FromDateTime(agoraLocal);
        var periodo = agoraLocal.Hour < 17 ? PeriodoAlocacao.Almoco : PeriodoAlocacao.Jantar;

        if (await contexto.Alocacoes.AnyAsync(a => a.Data == hoje && a.Periodo == periodo))
        {
            saida.WriteLine("Ao vivo: o turno de hoje já está montado, então não mexo em nada.");
            return;
        }

        var sorteio = new Random(configuracao.Semente);
        var garcons = cenario.Garcons;

        // Todos presentes: no dia da demonstração ninguém falta.
        var turno = Distribuir(garcons, cenario.Pracas).ToList();
        foreach (var (garcom, praca) in turno)
            contexto.Alocacoes.Add(new Alocacao(hoje, periodo, garcom.Id, praca.Id));

        var abertas = 0;
        var pendentes = 0;
        var codigos = new List<string>();

        foreach (var grupo in turno.GroupBy(t => t.Praca))
        {
            var mesas = cenario.Mesas.Where(m => m.PracaId == grupo.Key.Id).OrderBy(_ => sorteio.Next()).ToList();
            var equipe = grupo.Select(t => t.Garcom).ToList();

            for (var i = 0; i < MesasAbertas[grupo.Key.Codigo] && i < mesas.Count; i++)
            {
                // A primeira mesa aberta leva a restrição, para o roteiro sempre ter uma para mostrar.
                var comanda = Abrir(contexto, cenario, sorteio, equipe[i % equipe.Count], mesas[i], agoraLocal,
                    comRestricao: abertas == 0);
                abertas++;
                pendentes += comanda.Itens.Count(x => x.Status == StatusItemPedido.Pendente);
                codigos.Add(comanda.CodigoAcessoCliente);
            }
        }

        await contexto.SaveChangesAsync();

        saida.WriteLine($"Ao vivo: turno de {periodo} de hoje montado e não confirmado, {abertas} mesas abertas "
                        + $"e {pendentes} itens esperando entrega.");
        saida.WriteLine($"Código de uma das mesas, para abrir a conta do cliente: {codigos[0]}");
    }

    /// <summary>Uma pessoa em cada praça primeiro, depois as vagas que sobram — como no histórico.</summary>
    private static IEnumerable<(Usuario Garcom, Praca Praca)> Distribuir(List<Usuario> garcons, List<Praca> pracas)
    {
        var ordenadas = pracas.OrderBy(p => p.Codigo).ToList();
        var ocupadas = ordenadas.ToDictionary(p => p.Id, _ => 0);
        var indice = 0;

        foreach (var praca in ordenadas)
        {
            if (indice >= garcons.Count)
                yield break;

            ocupadas[praca.Id]++;
            yield return (garcons[indice++], praca);
        }

        foreach (var praca in ordenadas)
        {
            while (ocupadas[praca.Id] < praca.QuantidadeGarcons && indice < garcons.Count)
            {
                ocupadas[praca.Id]++;
                yield return (garcons[indice++], praca);
            }
        }
    }

    private static Comanda Abrir(
        GastraDbContext contexto,
        Cenario cenario,
        Random sorteio,
        Usuario garcom,
        Mesa mesa,
        DateTime agoraLocal,
        bool comRestricao)
    {
        var pessoas = Math.Max(1, Math.Min(mesa.Capacidade, sorteio.Next(1, mesa.Capacidade + 1)));
        var comanda = new Comanda(mesa.Id, garcom.Id, pessoas);
        contexto.Comandas.Add(comanda);

        var escolhidos = cenario.Cardapio.OrderBy(_ => sorteio.Next()).Take(sorteio.Next(2, 5)).ToList();
        foreach (var item in escolhidos)
        {
            var pedido = comanda.AdicionarItem(item, sorteio.NextDouble() < 0.2 ? 2 : 1);
            if (sorteio.NextDouble() >= ChanceDePendente)
                pedido.MarcarEntregue();
        }

        // Restrição registrada: é o que o garçom e o metre enxergam, e o cliente não (RN04).
        if (comRestricao || sorteio.NextDouble() < 0.25)
            comanda.RegistrarRestricao(CategoriaRestricao.Alergia, "Alergia a camarão em um dos lugares.");

        // A mesa foi aberta há algum tempo: sem isso o salão pareceria ter acabado de abrir.
        var abertura = DateTime.SpecifyKind(agoraLocal.AddMinutes(-sorteio.Next(20, 100)).AddHours(3), DateTimeKind.Utc);
        contexto.Entry(comanda).Property(c => c.DataHoraAbertura).CurrentValue = abertura;

        var passo = 10;
        var ordem = 1;
        foreach (var item in comanda.Itens)
        {
            contexto.Entry(item).Property(i => i.DataHoraRegistro).CurrentValue = abertura.AddMinutes(passo * ordem);
            ordem++;
        }

        return comanda;
    }
}
