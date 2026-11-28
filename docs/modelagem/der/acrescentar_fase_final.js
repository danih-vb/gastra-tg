// Acrescenta ao DER conceitual o que entrou na fase final (21 a 26/11/2026), com as classes do próprio brModelo:
// - DestaqueDoCardapio (ENT12), ligado ao ItemDoCardapio por "destaca" (REL12);
// - MencaoHonrosa (ENT13), ligada ao Usuario por "recebe" (REL13, o garçom) e "concede" (REL14, o gerente);
// - em ItemDoPedido, os atributos da cortesia, do ajuste de preço e do lançamento para confirmar.
// uso (Java 8, que traz o jjs): jjs -cp brModelo.jar acrescentar_fase_final.js -- GASTRA_DER.brM3 GASTRA_DER.brM3 GASTRA_DER.png [der.xml]
// Já aplicado em 26/11/2026. Fica aqui como registro; rodar de novo duplicaria as entidades.
var entrada = arguments[0], saidaBrm = arguments[1], saidaPng = arguments[2], saidaXml = arguments[3];
var C = Packages.diagramas.conceitual;
var CARD = Packages.desenho.preAnyDiagrama.PreCardinalidade.TiposCard;

var fp = new Packages.principal.FramePrincipal();
var d = Packages.controlador.Diagrama.LoadFromFile(new java.io.File(entrada), fp.getEditor());
var itens = d.getListaDeItens();

// "perto" escolhe, entre formas com o mesmo texto (há duas "categoria"), a que fica perto daquele ponto.
var acha = function (tipo, texto, perto) {
    var melhor = null, distancia = 1e9;
    for (var i = 0; i < itens.size(); i++) {
        var f = itens.get(i);
        if (f.getClass().getSimpleName() != tipo || f.getTexto() != texto) continue;
        var dd = perto ? Math.abs(f.getLeft() - perto[0]) + Math.abs(f.getTop() - perto[1]) : 0;
        if (dd < distancia) { melhor = f; distancia = dd; }
    }
    if (melhor == null) throw "nao achei " + tipo + " " + texto;
    return melhor;
};
// Como se o rótulo da cardinalidade tivesse sido arrastado com o mouse: com MovimentacaoManual, o brModelo não
// recalcula a posição.
var moverRotulo = function (linha, x, y) {
    var rotulo = linha.getCard(), campo = null;
    for (var c = rotulo.getClass(); c != null && campo == null; c = c.getSuperclass()) {
        try { campo = c.getDeclaredField("MovimentacaoManual"); } catch (e) { }
    }
    campo.setAccessible(true);
    campo.setBoolean(rotulo, true);
    rotulo.SetBounds(x, y, rotulo.getWidth(), rotulo.getHeight());
};
var P = function (x, y) { return new java.awt.Point(x, y); };
var ligar = function (a, pa, b, pb, card) {
    var linha = new C.Ligacao(d);
    linha.FormasALigar = Java.to([a, b], "desenho.formas.Forma[]");
    linha.SuperInicie(0, pb, pa);
    linha.Ligar();
    if (card) {
        linha.PrepareCardinalidade();
        linha.getCard().setCard(card);
    }
    return linha;
};
var entidade = function (nome, x, y, largura) {
    var e = new C.Entidade(d, nome);
    e.setTexto(nome);
    e.SetBounds(x, y, largura, 58);
    return e;
};
var relacionamento = function (nome, x, y, largura) {
    var r = new C.Relacionamento(d, nome);
    r.setTexto(nome);
    r.SetBounds(x, y, largura, 50);
    return r;
};
// Atributos em escada acima da entidade, como nas outras; "desloc" afasta a escada da borda esquerda.
var escadaAcima = function (e, lista, desloc) {
    var x0 = e.getLeft() + desloc, y0 = e.getTop() - 19 * lista.length - 5;
    lista.forEach(function (a, i) {
        var at = new C.Atributo(d, a.nome);
        at.setTexto(a.nome);
        at.SetBounds(x0 + 24 + 14 * i, y0 + 19 * i, 20 + 8 * a.nome.length, 14);
        if (a.id) at.setIdentificador(true);
        if (a.opcional) at.setOpcional(true);
        ligar(at, P(at.getLeft() + 1, at.getTop() + 7), e, P(x0 + 10 + 14 * i, e.getTop()), null);
    });
};
// Escada descendo abaixo da entidade, para quem já tem a de cima cheia.
var escadaAbaixo = function (e, lista, desloc) {
    var x0 = e.getLeft() + desloc, base = e.getTop() + e.getHeight();
    lista.forEach(function (a, i) {
        var at = new C.Atributo(d, a.nome);
        at.setTexto(a.nome);
        at.SetBounds(x0 + 24 + 14 * i, base + 14 + 19 * i, 20 + 8 * a.nome.length, 14);
        if (a.opcional) at.setOpcional(true);
        ligar(at, P(at.getLeft() + 1, at.getTop() + 7), e, P(x0 + 10 + 14 * i, base), null);
    });
};

var usuario = acha("Entidade", "Usuario");
var itemDoCardapio = acha("Entidade", "ItemDoCardapio");
var itemDoPedido = acha("Entidade", "ItemDoPedido");

// 1. Abre um corredor embaixo do Usuario: a RestricaoAlimentar e os atributos dela vão 70 px para a esquerda.
//    DoMove leva as linhas junto (mudar a posição direto não leva).
var restricao = acha("Entidade", "RestricaoAlimentar");
[restricao, acha("Atributo", "id_restricao"), acha("Atributo", "categoria", [735, 602]), acha("Atributo", "observacao_livre")]
    .forEach(function (f) { f.DoMove(-70, 0); });

// 2. MencaoHonrosa: o garçom recebe (corredor x 816) e o gerente concede (desce à direita, antes de "é gerado por").
var mencao = entidade("MencaoHonrosa", 960, 880, 160);
escadaAcima(mencao, [
    { nome: "id_mencao", id: true },
    { nome: "titulo" },
    { nome: "descricao", opcional: true },
    { nome: "icone" },
    { nome: "cor" },
    { nome: "mes" },
    { nome: "data_hora_concessao" },
], 30);
var recebe = relacionamento("recebe", 751, 770, 130);
moverRotulo(ligar(recebe, P(816, 770), usuario, P(816, usuario.getTop() + usuario.getHeight()), CARD.C0N), 778, 574);
ligar(recebe, P(881, 795), mencao, P(960, 915), CARD.C11);
var concede = relacionamento("concede", 915, 660, 120);
moverRotulo(ligar(concede, P(975, 660), usuario, P(842, usuario.getTop() + usuario.getHeight()), CARD.C0N), 848, 574);
ligar(concede, P(975, 710), mencao, P(975, 880), CARD.C11);

// 3. DestaqueDoCardapio, abaixo do ItemDoCardapio.
var destaque = entidade("DestaqueDoCardapio", 880, 1310, 170);
escadaAcima(destaque, [
    { nome: "id_destaque", id: true },
    { nome: "tipo" },
    { nome: "data_inicio" },
    { nome: "data_fim" },
    { nome: "meta", opcional: true },
], 30);
var destaca = relacionamento("destaca", 692, 1225, 130);
ligar(destaca, P(757, 1225), itemDoCardapio, P(757, itemDoCardapio.getTop() + itemDoCardapio.getHeight()), CARD.C0N);
ligar(destaca, P(822, 1250), destaque, P(880, 1339), CARD.C11);

// 4. ItemDoPedido: cortesia, ajuste de preço e quem lançou (Metre ou Gerente, até o garçom confirmar).
escadaAbaixo(itemDoPedido, [
    { nome: "cortesia" },
    { nome: "preco_original", opcional: true },
    { nome: "motivo_ajuste", opcional: true },
    { nome: "lancado_por", opcional: true },
], 0);

var arq = new java.io.File(saidaBrm);
d.setArquivo(arq.getAbsolutePath());
print("salvo: " + d.Salvar(arq, false));
var img = Packages.util.ImageGenerate.geraImagem(d);
javax.imageio.ImageIO.write(img, "PNG", new java.io.File(saidaPng));
print("png: " + img.getWidth() + "x" + img.getHeight());
if (saidaXml) {
    var w = new java.io.OutputStreamWriter(new java.io.FileOutputStream(saidaXml), "UTF-8");
    w.write(Packages.util.XMLGenerate.GeraXMLtoSaveFrom(d, false).toString()); w.close();
    print("xml: " + saidaXml);
}
java.lang.System.exit(0);
