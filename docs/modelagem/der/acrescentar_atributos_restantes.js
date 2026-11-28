// Acrescenta ao DER conceitual dois atributos que o banco já tinha e o DER não (achados pela engenharia reversa
// de 26/11/2026): Usuario.foto e Comanda.atendimento_chamado_em, ambos opcionais. Sobe a escada de atributos de
// cada entidade uma linha (DoMove leva as linhas junto) e põe o novo no degrau de baixo.
// uso: jjs -cp brModelo.jar acrescentar_atributos_restantes.js -- GASTRA_DER.brM3 GASTRA_DER.brM3 GASTRA_DER.png [der.xml]
// Já aplicado em 26/11/2026; rodar de novo duplicaria os atributos.
var entrada = arguments[0], saidaBrm = arguments[1], saidaPng = arguments[2], saidaXml = arguments[3];
var C = Packages.diagramas.conceitual;
var fp = new Packages.principal.FramePrincipal();
var d = Packages.controlador.Diagrama.LoadFromFile(new java.io.File(entrada), fp.getEditor());
var itens = d.getListaDeItens();
var P = function (x, y) { return new java.awt.Point(x, y); };

var entidade = function (nome) {
    for (var i = 0; i < itens.size(); i++) {
        var f = itens.get(i);
        if (f.getClass().getSimpleName() == "Entidade" && f.getTexto() == nome) return f;
    }
    throw "nao achei " + nome;
};
// A escada da entidade: atributos logo acima dela, entre a borda esquerda e 300 px à direita.
var escada = function (e) {
    var lista = [];
    for (var i = 0; i < itens.size(); i++) {
        var f = itens.get(i);
        if (f.getClass().getSimpleName() != "Atributo") continue;
        if (f.getLeft() >= e.getLeft() && f.getLeft() <= e.getLeft() + 300 && f.getTop() < e.getTop() && f.getTop() > e.getTop() - 250)
            lista.push(f);
    }
    return lista.sort(function (x, y) { return x.getTop() - y.getTop(); });
};
var acrescentar = function (e, nome) {
    var degraus = escada(e);
    degraus.forEach(function (a) { a.DoMove(0, -19); });
    var ultimo = degraus[degraus.length - 1];
    var at = new C.Atributo(d, nome);
    at.setTexto(nome);
    at.SetBounds(ultimo.getLeft() + 14, ultimo.getTop() + 19, 20 + 8 * nome.length, 14);
    at.setOpcional(true);
    var linha = new C.Ligacao(d);
    linha.FormasALigar = Java.to([at, e], "desenho.formas.Forma[]");
    linha.SuperInicie(0, P(ultimo.getLeft(), e.getTop()), P(at.getLeft() + 1, at.getTop() + 7));
    linha.Ligar();
    print(e.getTexto() + ": " + degraus.length + " degraus + " + nome);
};
acrescentar(entidade("Usuario"), "foto");
acrescentar(entidade("Comanda"), "atendimento_chamado_em");

var arq = new java.io.File(saidaBrm);
d.setArquivo(arq.getAbsolutePath());
print("salvo: " + d.Salvar(arq, false));
var img = Packages.util.ImageGenerate.geraImagem(d);
javax.imageio.ImageIO.write(img, "PNG", new java.io.File(saidaPng));
if (saidaXml) {
    var w = new java.io.OutputStreamWriter(new java.io.FileOutputStream(saidaXml), "UTF-8");
    w.write(Packages.util.XMLGenerate.GeraXMLtoSaveFrom(d, false).toString()); w.close();
}
java.lang.System.exit(0);
