using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using Gastra.Domain.Entidades;
using Gastra.Infrastructure.DataAccess;

// Gera, a partir do código compilado:
//   docs/diagramas/_fontes/GASTRA_Classe_Dominio.drawio    entidades, enums, regras e valores do domínio
//   docs/diagramas/_fontes/GASTRA_Classe_Contratos.drawio  interfaces do domínio e implementações da infraestrutura
//   docs/arquitetura/GASTRA_Diagrama_Classes_Codigo.md     a mesma informação em tabelas
// Os PNGs saem do draw.io por linha de comando (ver tools/GeradorDiagramaClasses/README.md).

var docs = Path.GetFullPath(args.Length > 0 ? args[0] : Path.Combine("..", "docs"));
var dominio = typeof(Comanda).Assembly;
var infraestrutura = typeof(GastraDbContext).Assembly;

var entidades = Tipos(dominio, "Gastra.Domain.Entidades").Where(t => t.IsClass).ToList();
var enums = Tipos(dominio, "Gastra.Domain.Enums").Where(t => t.IsEnum).ToList();
var regras = Tipos(dominio, "Gastra.Domain.Alocacoes").Concat(Tipos(dominio, "Gastra.Domain.Indicadores"))
    .Concat(Tipos(dominio, "Gastra.Domain.Servicos").Where(t => t.IsClass)).ToList();
var interfaces = Tipos(dominio, "Gastra.Domain.Repositorios").Concat(Tipos(dominio, "Gastra.Domain.Servicos"))
    .Concat(Tipos(dominio, "Gastra.Domain.Seguranca")).Where(t => t.IsInterface).ToList();
var implementacoes = infraestrutura.GetTypes()
    .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true })
    .SelectMany(t => t.GetInterfaces().Where(interfaces.Contains).Select(i => (Interface: i, Classe: t)))
    .ToList();

const int LarguraMaxima = 2400;

var fontes = Path.Combine(docs, "diagramas", "_fontes");
File.WriteAllText(Path.Combine(fontes, "GASTRA_Classe_Dominio.drawio"), DiagramaDominio());
File.WriteAllText(Path.Combine(fontes, "GASTRA_Classe_Contratos.drawio"), DiagramaContratos());
File.WriteAllText(Path.Combine(docs, "arquitetura", "GASTRA_Diagrama_Classes_Codigo.md"), Markdown());

Console.WriteLine($"{entidades.Count} entidades, {enums.Count} enums, {regras.Count} regras e valores, " +
                  $"{interfaces.Count} interfaces, {implementacoes.Count} implementações.");
return;

// ---------------------------------------------------------------- leitura por reflection

static IEnumerable<Type> Tipos(Assembly assembly, string ns) =>
    assembly.GetTypes()
        .Where(t => t.Namespace == ns && t.IsPublic && !t.IsDefined(typeof(CompilerGeneratedAttribute)))
        .OrderBy(t => t.Name);

static string NomeDoTipo(Type tipo) => NomeComNulidade(tipo, null);

// Com a informação de nulidade do compilador, "Task<Comanda?>" aparece como no código, e não "Task<Comanda>".
static string NomeComNulidade(Type tipo, NullabilityInfo? nulidade)
{
    var anulavel = Nullable.GetUnderlyingType(tipo);
    if (anulavel is not null)
        return NomeComNulidade(anulavel, null) + "?";

    var sufixo = !tipo.IsValueType && nulidade?.ReadState == NullabilityState.Nullable ? "?" : "";

    if (tipo.IsGenericType)
    {
        var nome = tipo.Name[..tipo.Name.IndexOf('`')];
        var argumentos = tipo.GetGenericArguments()
            .Select((a, i) => NomeComNulidade(a, nulidade is not null && i < nulidade.GenericTypeArguments.Length ? nulidade.GenericTypeArguments[i] : null));
        if (nome == "ValueTuple")
            return "(" + string.Join(", ", argumentos) + ")" + sufixo;
        return $"{nome}<{string.Join(", ", argumentos)}>{sufixo}";
    }

    if (tipo.IsArray)
        return NomeComNulidade(tipo.GetElementType()!, nulidade?.ElementType) + "[]" + sufixo;

    return tipo.FullName switch
    {
        "System.Int32" => "int",
        "System.String" => "string",
        "System.Decimal" => "decimal",
        "System.Double" => "double",
        "System.Boolean" => "bool",
        "System.Void" => "void",
        "System.Object" => "object",
        _ => tipo.Name,
    } + sufixo;
}

static bool EhRecord(Type tipo) => tipo.GetMethod("<Clone>$") is not null;

static List<string> Atributos(Type tipo)
{
    const BindingFlags publicos = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    var constantes = tipo.GetFields(publicos)
        .Where(f => f.IsLiteral || f.IsInitOnly)
        .Select(f => Uml.Marcar(
            $"+{f.Name}: {NomeDoTipo(f.FieldType)}{ValorConstante(f)}",
            f.IsStatic));

    var propriedades = tipo.GetProperties(publicos)
        .Where(p => p.GetMethod is { IsPublic: true } && !(EhRecord(tipo) && p.Name == "EqualityContract"))
        .Select(p => Uml.Marcar(
            $"+{p.Name}: {NomeComNulidade(p.PropertyType, new NullabilityInfoContext().Create(p))}",
            p.GetMethod!.IsStatic));

    return constantes.Concat(propriedades).ToList();
}

// `const decimal` não é literal para a reflection: o compilador o transforma em static readonly com
// [DecimalConstant]. Sem isto, justamente PercentualTaxaServico (RF04) aparecia sem o valor.
static string ValorConstante(FieldInfo campo)
{
    if (campo.IsLiteral)
        return " = " + Formatar(campo.GetRawConstantValue());

    var decimalConstante = campo.GetCustomAttribute<System.Runtime.CompilerServices.DecimalConstantAttribute>();
    return decimalConstante is null ? "" : " = " + Formatar(decimalConstante.Value);
}

static string Formatar(object? valor) => valor switch
{
    string texto => $"\"{texto}\"",
    double numero => numero.ToString(System.Globalization.CultureInfo.InvariantCulture),
    decimal numero => numero.ToString(System.Globalization.CultureInfo.InvariantCulture),
    _ => valor?.ToString() ?? "null",
};

static List<string> Metodos(Type tipo)
{
    string[] geradosPeloRecord = ["Equals", "GetHashCode", "ToString", "PrintMembers", "Deconstruct"];

    return tipo.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(m => !m.IsSpecialName && !m.Name.StartsWith('<'))
        .Where(m => !(EhRecord(tipo) && geradosPeloRecord.Contains(m.Name)))
        .OrderBy(m => m.Name)
        .Select(m =>
        {
            var contexto = new NullabilityInfoContext();
            var parametros = string.Join(", ", m.GetParameters().Select(p => $"{p.Name}: {NomeComNulidade(p.ParameterType, contexto.Create(p))}"));
            return Uml.Marcar($"+{m.Name}({parametros}): {NomeComNulidade(m.ReturnType, contexto.Create(m.ReturnParameter))}", m.IsStatic);
        })
        .ToList();
}

// ---------------------------------------------------------------- draw.io

string DiagramaDominio()
{
    var d = new Desenho();
    d.Titulo("GASTRA - Diagrama de Classes: Domínio (gerado do código)", 40, 10);

    // Entidades em linhas pensadas para aproximar quem se relaciona.
    string[][] linhas =
    [
        ["Praca", "Mesa", "Comanda", "ItemDoPedido"],
        ["Alocacao", "Usuario", "RestricaoAlimentar", "ItemDoCardapio", "ItemCardapioFlag"],
        ["RegistroAuditoria", "EntidadeBase", "Promocao", "PromocaoItem"],
    ];

    var porNome = entidades.ToDictionary(t => t.Name);
    var y = 70;
    foreach (var linha in linhas)
    {
        var x = 40;
        var alturaDaLinha = 0;
        foreach (var nome in linha.Where(porNome.ContainsKey))
        {
            var tipo = porNome[nome];
            var (_, _, w, h) = d.Classe(nome, nome, x, y, Atributos(tipo), Metodos(tipo),
                tipo.IsAbstract ? "abstrata" : "dominio", tipo.IsAbstract ? "abstract" : null);
            x += w + 90;
            alturaDaLinha = Math.Max(alturaDaLinha, h);
        }
        y += alturaDaLinha + 110;
    }

    // Entidades novas que ainda não estão no arranjo acima entram numa linha extra, para nada ficar de fora.
    var faltando = entidades.Where(t => !linhas.SelectMany(l => l).Contains(t.Name)).ToList();
    if (faltando.Count > 0)
    {
        var x = 40;
        foreach (var tipo in faltando)
            x += d.Classe(tipo.Name, tipo.Name, x, y, Atributos(tipo), Metodos(tipo), "dominio").W + 90;
        y += 400;
    }

    // Relacionamentos lidos do código: coleção de entidade (composição) e propriedade <Entidade>Id (associação).
    var apelidos = new Dictionary<string, string> { ["Garcom"] = "Usuario", ["ItemCardapio"] = "ItemDoCardapio" };
    foreach (var tipo in entidades)
    {
        const BindingFlags todos = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        foreach (var campo in tipo.GetFields(todos).Where(f => f.FieldType.IsGenericType))
        {
            var elemento = campo.FieldType.GetGenericArguments()[0];
            if (entidades.Contains(elemento))
                d.Ligacao(tipo.Name, elemento.Name, "composicao", "", "1", "0..*");
        }

        foreach (var propriedade in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (!propriedade.Name.EndsWith("Id") || propriedade.Name == "Id")
                continue;
            var alvo = propriedade.Name[..^2];
            alvo = apelidos.GetValueOrDefault(alvo, alvo);
            if (!porNome.ContainsKey(alvo) || alvo == tipo.Name || d.JaLigados(alvo, tipo.Name))
                continue;
            var opcional = Nullable.GetUnderlyingType(propriedade.PropertyType) is not null;
            d.Ligacao(tipo.Name, alvo, "associacao", propriedade.Name, "0..*", opcional ? "0..1" : "1");
        }
    }

    d.Nota("nota-heranca",
        "Todas as entidades herdam de <b>EntidadeBase</b> (Id). As setas de herança foram omitidas para não poluir o desenho.<br>" +
        "Associações vêm das propriedades <i>&lt;Entidade&gt;Id</i>; composições, das coleções privadas de entidades.",
        40, y, 620, 60);
    y += 100;

    d.Titulo("Enumerações", 40, y);
    y += 36;
    var xEnum = 40;
    var alturaEnums = 0;
    foreach (var tipo in enums)
    {
        if (xEnum > LarguraMaxima)
        {
            (xEnum, y, alturaEnums) = (40, y + alturaEnums + 30, 0);
        }
        var (_, _, w, h) = d.Classe(tipo.Name, tipo.Name, xEnum, y, Enum.GetNames(tipo).ToList(), [], "enum", "enumeration", semMetodos: true);
        xEnum += w + 30;
        alturaEnums = Math.Max(alturaEnums, h);
    }
    y += alturaEnums + 70;

    d.Titulo("Regras de negócio e objetos de valor", 40, y);
    y += 36;
    var xRegra = 40;
    var alturaRegras = 0;
    foreach (var tipo in regras)
    {
        if (xRegra > LarguraMaxima)
        {
            (xRegra, y, alturaRegras) = (40, y + alturaRegras + 40, 0);
        }
        var estereotipo = tipo.IsAbstract && tipo.IsSealed ? "static" : EhRecord(tipo) ? "record" : typeof(Exception).IsAssignableFrom(tipo) ? "exception" : null;
        var (_, _, w, h) = d.Classe(tipo.Name, tipo.Name, xRegra, y, Atributos(tipo), Metodos(tipo), "valor", estereotipo);
        xRegra += w + 40;
        alturaRegras = Math.Max(alturaRegras, h);
    }

    return d.Xml("GASTRA_Classe_Dominio");
}

string DiagramaContratos()
{
    var d = new Desenho();
    d.Titulo("GASTRA - Diagrama de Classes: Contratos (Domain) e Implementações (Infrastructure) — gerado do código", 40, 10);

    var y = 70;
    foreach (var interfaceDoDominio in interfaces.OrderBy(i => i.Namespace).ThenBy(i => i.Name))
    {
        var (_, _, w, h) = d.Classe(interfaceDoDominio.Name, interfaceDoDominio.Name, 40, y, [], Metodos(interfaceDoDominio), "interface", "interface");
        var classes = implementacoes.Where(p => p.Interface == interfaceDoDominio).Select(p => p.Classe).Distinct().ToList();
        var xClasse = Math.Max(40 + w + 160, 760);
        foreach (var classe in classes)
        {
            d.Classe(classe.Name, classe.Name, xClasse, y + Math.Max(0, h / 2 - 20), [], [], "implementacao", null, semMetodos: true);
            d.Ligacao(classe.Name, interfaceDoDominio.Name, "realizacao");
            xClasse += 260;
        }

        if (classes.Count == 0)
            d.Nota("sem-" + interfaceDoDominio.Name, "sem implementação na Infrastructure", xClasse, y + 10, 240, 40);

        y += h + 40;
    }

    d.Nota("legenda",
        "Verde: interface em Gastra.Domain. Amarelo: implementação em Gastra.Infrastructure.<br>" +
        "Seta tracejada com triângulo vazado = realização (a classe implementa a interface).",
        40, y + 10, 620, 50);

    return d.Xml("GASTRA_Classe_Contratos");
}

// ---------------------------------------------------------------- markdown

string Markdown()
{
    var md = new StringBuilder();
    md.AppendLine("# GASTRA — Diagrama de Classes gerado do código");
    md.AppendLine();
    md.AppendLine("> **Arquivo gerado automaticamente** por `backend/tools/GeradorDiagramaClasses` a partir dos assemblies");
    md.AppendLine("> compilados. Não edite à mão: rode a ferramenta de novo (ver o README dela). A especificação original,");
    md.AppendLine("> de antes da implementação, continua em `GASTRA_Diagrama_Classes_Camadas.md`.");
    md.AppendLine();
    md.AppendLine("Diagramas: [domínio](../diagramas/classe/GASTRA_Classe_Dominio.png) · [contratos e implementações](../diagramas/classe/GASTRA_Classe_Contratos.png).");
    md.AppendLine();
    md.AppendLine($"Resumo: {entidades.Count} entidades, {enums.Count} enumerações, {regras.Count} regras e objetos de valor, {interfaces.Count} interfaces e {implementacoes.Select(p => p.Classe).Distinct().Count()} implementações.");
    md.AppendLine();

    md.AppendLine("## 1. Entidades (`Gastra.Domain.Entidades`)");
    md.AppendLine();
    foreach (var tipo in entidades)
        Tabela(md, tipo);

    md.AppendLine("## 2. Enumerações (`Gastra.Domain.Enums`)");
    md.AppendLine();
    md.AppendLine("| Enumeração | Valores |");
    md.AppendLine("|---|---|");
    foreach (var tipo in enums)
        md.AppendLine($"| `{tipo.Name}` | {string.Join(", ", Enum.GetNames(tipo))} |");
    md.AppendLine();

    md.AppendLine("## 3. Regras de negócio e objetos de valor");
    md.AppendLine();
    foreach (var tipo in regras)
        Tabela(md, tipo);

    md.AppendLine("## 4. Contratos e implementações");
    md.AppendLine();
    md.AppendLine("| Interface (Domain) | Implementação (Infrastructure) | Métodos |");
    md.AppendLine("|---|---|---|");
    foreach (var interfaceDoDominio in interfaces.OrderBy(i => i.Namespace).ThenBy(i => i.Name))
    {
        var classes = implementacoes.Where(p => p.Interface == interfaceDoDominio).Select(p => $"`{p.Classe.Name}`").Distinct();
        var metodos = string.Join("<br>", Metodos(interfaceDoDominio).Select(m => $"`{Escapar(m)}`"));
        md.AppendLine($"| `{interfaceDoDominio.Name}` | {string.Join(", ", classes)} | {metodos} |");
    }

    return md.ToString();

    static void Tabela(StringBuilder md, Type tipo)
    {
        md.AppendLine($"### {tipo.Name}");
        md.AppendLine();
        var atributos = Atributos(tipo);
        var metodos = Metodos(tipo);
        md.AppendLine("| Atributos | Métodos |");
        md.AppendLine("|---|---|");
        md.AppendLine($"| {Lista(atributos)} | {Lista(metodos)} |");
        md.AppendLine();
    }

    static string Lista(List<string> itens) => itens.Count == 0 ? "—" : string.Join("<br>", itens.Select(i => $"`{Escapar(i)}`"));

    static string Escapar(string texto) => Uml.ParaTexto(texto).Replace("|", "\\|");
}

// ---------------------------------------------------------------- escrita do XML do draw.io

internal sealed class Desenho
{
    private const int Linha = 18;
    private const int Cabecalho = 26;
    private const int Folga = 8;

    private static readonly Dictionary<string, (string Fill, string Stroke)> Cores = new()
    {
        ["dominio"] = ("#dae8fc", "#6c8ebf"),
        ["abstrata"] = ("#e1d5e7", "#9673a6"),
        ["enum"] = ("#f5f5f5", "#666666"),
        ["valor"] = ("#fff2cc", "#d6b656"),
        ["interface"] = ("#d5e8d4", "#82b366"),
        ["implementacao"] = ("#fff2cc", "#d6b656"),
        ["nota"] = ("#fff9e6", "#b3a100"),
    };

    private readonly StringBuilder _celulas = new();
    private readonly Dictionary<string, (string Id, int X, int Y, int W, int H)> _caixas = new();
    private readonly HashSet<(string, string)> _ligados = [];
    private int _proximoId = 2;

    private string NovoId() => $"c{_proximoId++}";

    private static string Html(string texto) => SecurityElement.Escape(texto);

    private static int Largura(IEnumerable<string> textos, int minimo = 180) =>
        Math.Max(minimo, (int)(textos.DefaultIfEmpty("").Max(t => t.Length) * 6.6) + 24);

    public void Titulo(string texto, int x, int y) =>
        _celulas.Append($"<mxCell id=\"{NovoId()}\" value=\"{Html(texto)}\" style=\"text;html=1;fontSize=16;fontStyle=1;align=left;verticalAlign=middle;\" vertex=\"1\" parent=\"1\"><mxGeometry x=\"{x}\" y=\"{y}\" width=\"1400\" height=\"30\" as=\"geometry\"/></mxCell>");

    public (int X, int Y, int W, int H) Classe(string chave, string nome, int x, int y, List<string> atributos, List<string> metodos,
        string tipo, string? estereotipo = null, bool semMetodos = false)
    {
        var (fill, stroke) = Cores[tipo];
        var cabecalho = estereotipo is null ? Html(nome) : $"«{estereotipo}»&lt;br&gt;{Html(nome)}";
        var alturaCabecalho = estereotipo is null ? Cabecalho : Cabecalho + 14;
        var w = Largura(atributos.Concat(metodos).Append(nome));
        var hAtributos = atributos.Count > 0 ? atributos.Count * Linha + Folga : (semMetodos ? 0 : 10);
        var hMetodos = semMetodos ? 0 : (metodos.Count > 0 ? metodos.Count * Linha + Folga : 10);
        var separador = semMetodos || atributos.Count == 0 && metodos.Count > 0 ? 0 : 8;
        if (atributos.Count == 0 && metodos.Count > 0)
            hAtributos = 0;
        var h = alturaCabecalho + hAtributos + separador + hMetodos;

        var id = NovoId();
        _celulas.Append($"<mxCell id=\"{id}\" value=\"{cabecalho}\" style=\"swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize={alturaCabecalho};horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=0;marginBottom=0;whiteSpace=wrap;html=1;fillColor={fill};strokeColor={stroke};swimlaneFillColor=#ffffff;\" vertex=\"1\" parent=\"1\"><mxGeometry x=\"{x}\" y=\"{y}\" width=\"{w}\" height=\"{h}\" as=\"geometry\"/></mxCell>");

        const string estiloTexto = "text;strokeColor=none;fillColor=none;align=left;verticalAlign=top;spacingLeft=6;spacingRight=4;overflow=hidden;rotatable=0;whiteSpace=wrap;html=1;fontSize=12;";
        var posicao = alturaCabecalho;
        if (hAtributos > 0)
        {
            Texto(id, atributos, posicao, w, hAtributos, estiloTexto);
            posicao += hAtributos;
        }
        if (separador > 0)
        {
            _celulas.Append($"<mxCell id=\"{NovoId()}\" value=\"\" style=\"line;strokeWidth=1;fillColor=none;align=left;verticalAlign=middle;rotatable=0;points=[];portConstraint=eastwest;strokeColor=inherit;\" vertex=\"1\" parent=\"{id}\"><mxGeometry y=\"{posicao}\" width=\"{w}\" height=\"8\" as=\"geometry\"/></mxCell>");
            posicao += separador;
        }
        if (hMetodos > 0)
            Texto(id, metodos, posicao, w, hMetodos, estiloTexto);

        _caixas[chave] = (id, x, y, w, h);
        return (x, y, w, h);
    }

    private void Texto(string pai, List<string> linhas, int y, int w, int h, string estilo)
    {
        var valor = Html(string.Join("<br>", linhas.Select(Uml.ParaHtml)));
        _celulas.Append($"<mxCell id=\"{NovoId()}\" value=\"{valor}\" style=\"{estilo}\" vertex=\"1\" parent=\"{pai}\"><mxGeometry y=\"{y}\" width=\"{w}\" height=\"{h}\" as=\"geometry\"/></mxCell>");
    }

    public void Nota(string chave, string htmlDaNota, int x, int y, int w, int h)
    {
        var (fill, stroke) = Cores["nota"];
        var id = NovoId();
        _celulas.Append($"<mxCell id=\"{id}\" value=\"{Html(htmlDaNota)}\" style=\"shape=note;whiteSpace=wrap;html=1;backgroundOutline=1;size=14;align=left;spacingLeft=8;spacingRight=8;fillColor={fill};strokeColor={stroke};fontSize=12;\" vertex=\"1\" parent=\"1\"><mxGeometry x=\"{x}\" y=\"{y}\" width=\"{w}\" height=\"{h}\" as=\"geometry\"/></mxCell>");
        _caixas[chave] = (id, x, y, w, h);
    }

    public bool JaLigados(string a, string b) => _ligados.Contains((a, b)) || _ligados.Contains((b, a));

    public void Ligacao(string origem, string destino, string tipo, string rotulo = "", string? multOrigem = null, string? multDestino = null)
    {
        if (!_caixas.ContainsKey(origem) || !_caixas.ContainsKey(destino))
            return;

        var estilo = tipo switch
        {
            "composicao" => "startArrow=diamondThin;startFill=1;startSize=16;endArrow=none;",
            "realizacao" => "endArrow=block;endFill=0;endSize=14;dashed=1;",
            _ => "endArrow=open;endFill=0;endSize=10;",
        };
        var id = NovoId();
        _celulas.Append($"<mxCell id=\"{id}\" value=\"{Html(rotulo)}\" style=\"edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;fontSize=10;labelBackgroundColor=#ffffff;{estilo}\" edge=\"1\" parent=\"1\" source=\"{_caixas[origem].Id}\" target=\"{_caixas[destino].Id}\"><mxGeometry relative=\"1\" as=\"geometry\"/></mxCell>");
        if (multOrigem is not null)
            _celulas.Append($"<mxCell id=\"{NovoId()}\" value=\"{Html(multOrigem)}\" style=\"edgeLabel;html=1;align=left;verticalAlign=bottom;fontSize=10;\" vertex=\"1\" connectable=\"0\" parent=\"{id}\"><mxGeometry x=\"-0.85\" relative=\"1\" as=\"geometry\"><mxPoint as=\"offset\"/></mxGeometry></mxCell>");
        if (multDestino is not null)
            _celulas.Append($"<mxCell id=\"{NovoId()}\" value=\"{Html(multDestino)}\" style=\"edgeLabel;html=1;align=right;verticalAlign=bottom;fontSize=10;\" vertex=\"1\" connectable=\"0\" parent=\"{id}\"><mxGeometry x=\"0.85\" relative=\"1\" as=\"geometry\"><mxPoint as=\"offset\"/></mxGeometry></mxCell>");
        _ligados.Add((origem, destino));
    }

    public string Xml(string nome) =>
        $"<mxfile host=\"drawio\"><diagram name=\"{nome}\" id=\"{nome}\"><mxGraphModel dx=\"1400\" dy=\"900\" grid=\"1\" gridSize=\"10\" guides=\"1\" tooltips=\"1\" connect=\"1\" arrows=\"1\" fold=\"1\" page=\"0\" pageScale=\"1\" background=\"#ffffff\" math=\"0\" shadow=\"0\"><root><mxCell id=\"0\"/><mxCell id=\"1\" parent=\"0\"/>{_celulas}</root></mxGraphModel></diagram></mxfile>";
}

/// <summary>
/// Membro estático (constante, campo ou método de classe). Na UML ele se desenha <b>sublinhado</b>. A marca
/// viaja no começo da linha e cada saída decide como mostrá-la: sublinhado no draw.io, e «static» por
/// extenso no Markdown, onde o texto fica dentro de crase e não aceita formatação.
/// </summary>
internal static class Uml
{
    private const char Estatico = '\u0001';

    public static string Marcar(string texto, bool estatico) => estatico ? Estatico + texto : texto;

    public static string ParaHtml(string linha) =>
        linha.StartsWith(Estatico) ? $"<u>{SecurityElement.Escape(linha[1..])}</u>" : SecurityElement.Escape(linha);

    public static string ParaTexto(string linha) => linha.StartsWith(Estatico) ? linha[1..] + " «static»" : linha;
}
