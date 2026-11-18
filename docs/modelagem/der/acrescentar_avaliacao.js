// Acrescenta AvaliacaoAtendimento ao DER conceitual (#196), com as classes do próprio brModelo.
// 1. Abre uma faixa descendo a fileira de baixo com DoMove, que leva as linhas junto (mudar a posição direto não leva).
// 2. Põe o relacionamento "recebe" no corredor livre entre as linhas de "contém" e "possui", sem cruzar nenhuma.
// uso (Java 8, que traz o jjs): jjs -cp brModelo.jar acrescentar_avaliacao.js -- GASTRA_DER.brM3 GASTRA_DER.brM3 GASTRA_DER.png
// Já aplicado em 25/09/2026 (#196). Fica aqui como registro de como a mudança foi feita; rodar de novo duplicaria a entidade.
var entrada = arguments[0], saidaBrm = arguments[1], saidaPng = arguments[2];
var C = Packages.diagramas.conceitual;
var CARD = Packages.desenho.preAnyDiagrama.PreCardinalidade.TiposCard;

var fp = new Packages.principal.FramePrincipal();
var d = Packages.controlador.Diagrama.LoadFromFile(new java.io.File(entrada), fp.getEditor());
var itens = d.getListaDeItens();

var acha = function (tipo, texto) {
    for (var i = 0; i < itens.size(); i++) {
        var f = itens.get(i);
        if (f.getClass().getSimpleName() == tipo && f.getTexto() == texto) return f;
    }
    throw "nao achei " + tipo + " " + texto;
};
var comanda = acha("Entidade", "Comanda");

var LIMIAR = 736, D = 200;
var formas = [];
for (var i = 0; i < itens.size(); i++) {
    var f = itens.get(i);
    if (f.getClass().getSimpleName() != "Ligacao" && f.getTop() >= LIMIAR) formas.push(f);
}
formas.forEach(function (f) { f.DoMove(0, D); });

// Liga a (no ponto pa) a b (no ponto pb). Os pontos dizem por onde a linha encosta em cada forma; como no
// gerar_logico.js, vão na ordem inversa das formas.
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

// Relacionamento no corredor x ~316, logo abaixo da Comanda.
var recebe = new C.Relacionamento(d, "recebe");
recebe.setTexto("recebe");
recebe.SetBounds(241, 830, 150, 50);

var EX = 450, EY = 826;
var avaliacao = new C.Entidade(d, "AvaliacaoAtendimento");
avaliacao.setTexto("AvaliacaoAtendimento");
avaliacao.SetBounds(EX, EY, 150, 58);

// Atributos em escada acima da entidade, como nas outras.
[
    { nome: "id_avaliacao", id: true },
    { nome: "nota" },
    { nome: "comentario", opcional: true },
    { nome: "data_hora_envio" },
].forEach(function (a, i) {
    var at = new C.Atributo(d, a.nome);
    at.setTexto(a.nome);
    at.SetBounds(EX + 24 + 14 * i, EY - 91 + 19 * i, 20 + 8 * a.nome.length, 14);
    if (a.id) at.setIdentificador(true);
    if (a.opcional) at.setOpcional(true);
    // Haste subindo do topo da entidade até a altura do círculo, como nas outras entidades.
    ligar(at, P(at.getLeft() + 1, at.getTop() + 7), avaliacao, P(EX + 10 + 14 * i, EY), null);
});

// Uma comanda recebe no máximo uma avaliação; a avaliação é de exatamente uma comanda.
// Corredor x = 316: entre a linha de "contém" (x 298) e a de "possui" (x 335), sem cruzar nenhuma.
var daComanda = ligar(recebe, P(316, 830), comanda, P(316, comanda.getTop() + comanda.getHeight()), CARD.C01);
// Embaixo da Comanda já estão os (0,n) de "contém" e de "possui": o rótulo desce para o trecho livre do corredor.
var rotulo = daComanda.getCard();
// Como se tivesse sido arrastado com o mouse: com MovimentacaoManual, o brModelo não recalcula a posição.
var campo = null;
for (var c = rotulo.getClass(); c != null && campo == null; c = c.getSuperclass()) {
    try { campo = c.getDeclaredField("MovimentacaoManual"); } catch (e) { }
}
campo.setAccessible(true);
campo.setBoolean(rotulo, true);
rotulo.SetBounds(322, 716, rotulo.getWidth(), rotulo.getHeight());
ligar(recebe, P(391, 855), avaliacao, P(EX, EY + 29), CARD.C11);

print("formas descidas: " + formas.length);
var arq = new java.io.File(saidaBrm);
d.setArquivo(arq.getAbsolutePath());
print("salvo: " + d.Salvar(arq, false));
var img = Packages.util.ImageGenerate.geraImagem(d);
javax.imageio.ImageIO.write(img, "PNG", new java.io.File(saidaPng));
print("png: " + img.getWidth() + "x" + img.getHeight());
java.lang.System.exit(0);
