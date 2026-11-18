// Engenharia reversa (issue #84), passo 3: monta o modelo lógico com as classes do próprio brModelo.
// uso: jjs -cp brModelo.jar gerar_logico.js -- fisico.json layout.json saida.brM3 saida.png saida.xml
var entrada = arguments[0], layoutArq = arguments[1], saidaBrm = arguments[2], saidaPng = arguments[3], saidaXml = arguments[4];
var ler = function (p) { return new java.lang.String(java.nio.file.Files.readAllBytes(java.nio.file.Paths.get(p)), "UTF-8"); };
var esquema = JSON.parse(ler(entrada)), layout = JSON.parse(ler(layoutArq));
var L = Packages.diagramas.logico;
var TIPO = L.Constraint.CONSTRAINT_TIPO;
var CARD = Packages.desenho.preAnyDiagrama.PreCardinalidade.TiposCard;
var fp = new Packages.principal.FramePrincipal();
var editor = fp.getEditor();
var d = editor.Novo(Packages.controlador.Diagrama.TipoDeDiagrama.tpLogico);
d.setPrefixo("");

var tabelas = {};
esquema.tabelas.forEach(function (t) {
    var pos = layout[t.nome];
    var tab = new L.Tabela(d, t.nome);
    tab.setTexto(t.nome);
    tab.SetBounds(pos[0], pos[1], 300, 34 + 22 * t.colunas.length);
    var campos = {};
    t.colunas.forEach(function (c) {
        var campo = tab.Add(c.nome);
        campo.setTipo(c.tipo.toUpperCase());
        campo.setComplemento(c.nulo ? "" : "NOT NULL");
        campos[c.nome] = campo;
    });
    t.pk.forEach(function (n) { campos[n].setKey(true); });
    tabelas[t.nome] = { tab: tab, campos: campos };
});

esquema.uniques.forEach(function (u) {
    var t = tabelas[u.tabela];
    var cons = new L.Constraint(t.tab);
    cons.setTipo(TIPO.tpUNIQUE);
    cons.setNome(u.nome);
    cons.setNomeada(true);
    u.colunas.forEach(function (n) { t.campos[n].SetUnique(true); cons.Add(t.campos[n], null); });
});

var centro = function (tab) { return new java.awt.Point(tab.getLeft() + tab.getWidth() / 2, tab.getTop() + tab.getHeight() / 2); };
esquema.fks.forEach(function (fk) {
    var filho = tabelas[fk.tabela], pai = tabelas[fk.ref];
    var pk = null;
    pai.tab.getConstraints().forEach(function (c) { if (c.getTipo() == TIPO.tpPK) pk = c; });
    var linha = new L.LogicoLinha(d);
    linha.FormasALigar = Java.to([pai.tab, filho.tab], "desenho.formas.Forma[]");
    linha.SuperInicie(0, centro(filho.tab), centro(pai.tab));
    linha.Ligar();
    var cons = new L.Constraint(filho.tab);
    cons.setTipo(TIPO.tpFK);
    cons.setNome(fk.nome);
    cons.setDdlOnDelete("ON DELETE " + fk.on_delete);
    cons.Add(pai.campos[fk.ref_coluna], filho.campos[fk.coluna], linha, pk);
    filho.campos[fk.coluna].SetFkey(true);
    var nulo = false;
    esquema.tabelas.forEach(function (t) { if (t.nome == fk.tabela) t.colunas.forEach(function (c) { if (c.nome == fk.coluna) nulo = c.nulo; }); });
    linha.getCardA().setCard(nulo ? CARD.C01 : CARD.C11);
    linha.getCardB().setCard(CARD.C0N);
    linha.ajusteSeta();
});

d.OrganizeTabelas();
d.getListaDeTabelas().forEach(function (t) { t.OrganizeDiagrama(); });

var w = new java.io.OutputStreamWriter(new java.io.FileOutputStream(saidaXml), "UTF-8");
w.write(Packages.util.XMLGenerate.GeraXMLtoSaveFrom(d, false).toString()); w.close();
d.setArquivo("");
var guarda = new Packages.controlador.apoios.GuardaPadraoBrM(d);
guarda.versaoDiagrama = "3.2.0";
var oos = new java.io.ObjectOutputStream(new java.io.FileOutputStream(saidaBrm));
oos.writeObject(guarda); oos.close();
var img = Packages.util.ImageGenerate.geraImagem(d);
javax.imageio.ImageIO.write(img, "PNG", new java.io.File(saidaPng));
print("ok: " + img.getWidth() + "x" + img.getHeight());
java.lang.System.exit(0);
