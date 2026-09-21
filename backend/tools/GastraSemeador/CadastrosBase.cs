using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Infrastructure.DataAccess;
using Gastra.Infrastructure.Seguranca;
using Microsoft.EntityFrameworkCore;

namespace GastraSemeador;

/// <summary>
/// Cadastros fictícios sobre os quais o histórico é construído: contas, praças, mesas e cardápio.
///
/// Nada aqui vem do restaurante entrevistado — nem o nome do estabelecimento, nem a planta do salão,
/// nem prato assinatura. Os pratos são de cozinha brasileira comum, e os nomes das pessoas são
/// inventados. O que se mantém fiel é a *forma* dos dados: três praças com potenciais diferentes,
/// cardápio com todas as categorias e marcações, e um quadro de sete garçons — os mesmos parâmetros
/// da calibração da RN03 (<c>docs/analises/GASTRA_Calibracao_Pesos_RN03.md</c>).
/// </summary>
internal static class CadastrosBase
{
    /// <summary>Praças com potenciais bem diferentes: é o que faz a RN03 ter o que distribuir.</summary>
    private static readonly (string Codigo, int Vagas, int Mesas)[] Pracas =
    [
        ("A", 3, 8),   // salão principal, alto potencial
        ("B", 3, 7),   // salão de baixo
        ("C", 2, 5),   // varanda, movimento menor
    ];

    private static readonly (string Nome, string Email, PapelUsuario Papel)[] Contas =
    [
        ("Roberta Lima", "roberta.gerente@gastra.local", PapelUsuario.Gerente),
        ("Marcos Tavares", "marcos.coordenador@gastra.local", PapelUsuario.Coordenador),
        ("Helena Prado", "helena.metre@gastra.local", PapelUsuario.Metre),
        ("Carla Mendes", "carla@gastra.local", PapelUsuario.Garcom),
        ("João Pereira", "joao@gastra.local", PapelUsuario.Garcom),
        ("Ana Souza", "ana@gastra.local", PapelUsuario.Garcom),
        ("Paulo Henrique", "paulo@gastra.local", PapelUsuario.Garcom),
        ("Beatriz Alves", "beatriz@gastra.local", PapelUsuario.Garcom),
        ("Rafael Nunes", "rafael@gastra.local", PapelUsuario.Garcom),
        ("Tânia Moreira", "tania@gastra.local", PapelUsuario.Garcom),
    ];

    /// <summary>
    /// Cardápio. Os quatro primeiros pratos e as bebidas correspondem aos itens que o
    /// <c>simulador.py</c> usa nas combinações — é o que permite plantar "moqueca puxa arroz de coco"
    /// no histórico e conferir depois se a recomendação encontra a regra.
    /// </summary>
    private static readonly ItemSemeado[] Cardapio =
    [
        new("Moqueca de peixe", CategoriaItemCardapio.PratoPrincipal, 89.00m,
            "Peixe branco, leite de coco e dendê, servida na panela de barro.", [FlagDietetica.SemGluten, FlagDietetica.SemLactose]),
        new("Arroz de coco", CategoriaItemCardapio.PratoPrincipal, 32.00m,
            "Acompanhamento tradicional da moqueca.", [FlagDietetica.Vegetariano, FlagDietetica.SemGluten]),
        new("Caipirinha", CategoriaItemCardapio.Bebida, 24.00m,
            "Cachaça, limão e açúcar.", [FlagDietetica.Vegano, FlagDietetica.SemGluten, FlagDietetica.SemLactose]),
        new("Pudim de leite", CategoriaItemCardapio.Sobremesa, 22.00m,
            "Receita da casa, com calda escura.", [FlagDietetica.Vegetariano, FlagDietetica.SemGluten]),
        new("Salada da horta", CategoriaItemCardapio.Entrada, 28.00m,
            "Folhas, tomate, pepino e molho de ervas.", [FlagDietetica.Vegano, FlagDietetica.SemGluten, FlagDietetica.SemLactose]),
        new("Porção infantil de frango", CategoriaItemCardapio.PratoPrincipal, 34.00m,
            "Iscas de frango com arroz e batata.", [FlagDietetica.OpcaoInfantil]),
        new("Suco natural de laranja", CategoriaItemCardapio.Bebida, 14.00m,
            "Feito na hora, sem açúcar.", [FlagDietetica.Vegano, FlagDietetica.SemGluten, FlagDietetica.SemLactose, FlagDietetica.OpcaoInfantil]),
        new("Café coado", CategoriaItemCardapio.Bebida, 9.00m,
            "Coado na hora.", [FlagDietetica.Vegano, FlagDietetica.SemGluten, FlagDietetica.SemLactose]),
        new("Bolinho de bacalhau (6 un.)", CategoriaItemCardapio.Entrada, 46.00m,
            "Bacalhau desfiado com batata.", [FlagDietetica.SemLactose]),
        new("Bobó de camarão", CategoriaItemCardapio.PratoPrincipal, 96.00m,
            "Camarão em creme de mandioca.", [FlagDietetica.SemGluten]),
        new("Risoto de cogumelos", CategoriaItemCardapio.PratoPrincipal, 72.00m,
            "Arroz arbóreo com cogumelos frescos.", [FlagDietetica.Vegetariano, FlagDietetica.SemGluten]),
        new("Petit gâteau", CategoriaItemCardapio.Sobremesa, 29.00m,
            "Bolo quente de chocolate com sorvete de creme.", [FlagDietetica.Vegetariano]),
        new("Água com gás", CategoriaItemCardapio.Bebida, 8.00m,
            "Garrafa de 500 ml.", [FlagDietetica.Vegano, FlagDietetica.SemGluten, FlagDietetica.SemLactose]),
        new("Sorvete de tapioca", CategoriaItemCardapio.Sobremesa, 24.00m,
            "Sorvete artesanal, sem leite de vaca.", [FlagDietetica.Vegano, FlagDietetica.SemLactose, FlagDietetica.OpcaoInfantil]),
    ];

    /// <summary>
    /// Cria o que ainda não existe e devolve o que está no banco. É idempotente de propósito: rodar duas
    /// vezes seguidas não duplica praça nem conta, e quem já semeou pode rodar de novo sem medo.
    /// </summary>
    public static async Task<Cenario> Semear(GastraDbContext contexto, Configuracao configuracao, TextWriter saida)
    {
        var criptografia = new CriptografiaSenha();

        var usuarios = await contexto.Usuarios.ToListAsync();
        foreach (var (nome, email, papel) in Contas)
        {
            if (usuarios.Any(u => u.Email == email.ToLowerInvariant()))
                continue;

            var usuario = new Usuario(nome, email, criptografia.GerarHash(configuracao.SenhaPadrao), papel);
            contexto.Usuarios.Add(usuario);
            usuarios.Add(usuario);
        }

        var pracas = await contexto.Pracas.ToListAsync();
        foreach (var (codigo, vagas, _) in Pracas)
        {
            if (pracas.Any(p => p.Codigo == codigo))
                continue;

            var praca = new Praca(codigo, vagas);
            contexto.Pracas.Add(praca);
            pracas.Add(praca);
        }

        await contexto.SaveChangesAsync();

        var mesas = await contexto.Mesas.ToListAsync();
        var numero = 1;
        foreach (var (codigo, _, quantidade) in Pracas)
        {
            var praca = pracas.First(p => p.Codigo == codigo);
            for (var i = 0; i < quantidade; i++, numero++)
            {
                var rotulo = numero.ToString("00");
                if (mesas.Any(m => m.Numero == rotulo))
                    continue;

                // Capacidades variadas: mesa de dois, de quatro e mesa grande de família.
                var capacidade = i % 5 == 0 ? 6 : i % 3 == 0 ? 2 : 4;
                var mesa = new Mesa(rotulo, capacidade, praca.Id);
                contexto.Mesas.Add(mesa);
                mesas.Add(mesa);
            }
        }

        var itens = await contexto.ItensCardapio.ToListAsync();
        foreach (var item in Cardapio)
        {
            if (itens.Any(i => i.Nome == item.Nome))
                continue;

            var novo = new ItemDoCardapio(item.Nome, item.Categoria, item.Preco, item.Descricao, item.Flags);
            contexto.ItensCardapio.Add(novo);
            itens.Add(novo);
        }

        await contexto.SaveChangesAsync();

        saida.WriteLine($"Cadastros base: {usuarios.Count} contas, {pracas.Count} praças, {mesas.Count} mesas, "
                        + $"{itens.Count} itens de cardápio.");

        return new Cenario(usuarios, pracas, mesas, itens);
    }

    private sealed record ItemSemeado(
        string Nome,
        CategoriaItemCardapio Categoria,
        decimal Preco,
        string Descricao,
        FlagDietetica[] Flags);
}

/// <summary>O que o histórico e o estado "ao vivo" precisam ter em mãos depois dos cadastros base.</summary>
internal sealed record Cenario(
    List<Usuario> Usuarios,
    List<Praca> Pracas,
    List<Mesa> Mesas,
    List<ItemDoCardapio> Cardapio)
{
    public List<Usuario> Garcons => Usuarios.Where(u => u.Papel == PapelUsuario.Garcom).ToList();
}
