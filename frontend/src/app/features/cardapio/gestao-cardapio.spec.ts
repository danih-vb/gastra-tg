import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Promocao } from '../../core/api/modelos';
import { API, itemCardapio } from '../../core/api/testes';
import { provedoresDeLocalizacao } from '../../core/localizacao';
import { AvisoService } from '../../shared/aviso/aviso.service';
import { GestaoCardapio, lerValor, precoComDesconto } from './gestao-cardapio';

const ITENS = [
  itemCardapio({ id: 1, nome: 'Risoto de cogumelos', preco: 68, precoPromocional: 57.8 }),
  itemCardapio({ id: 2, nome: 'Caipirinha de limão', categoria: 'Bebida', preco: 24, flagsDieteticas: ['Vegano'] }),
];

const PROMOCAO: Promocao = {
  id: 7,
  descricao: 'Quinta do risoto',
  tipoDesconto: 'Percentual',
  valorDesconto: 15,
  dataInicio: '2026-09-01',
  dataFim: '2099-09-30',
  ativa: true,
  vigenteHoje: true,
  itens: [{ itemCardapioId: 1, nome: 'Risoto de cogumelos', precoOriginal: 68, precoComDesconto: 57.8 }],
};

describe('contas do cardápio', () => {
  it('lê valores em reais do jeito que o gerente digita', () => {
    expect(lerValor('12,50')).toBe(12.5);
    expect(lerValor('12.50')).toBe(12.5);
    expect(lerValor('1.234,50')).toBe(1234.5);
    expect(lerValor('')).toBeNaN();
  });

  it('calcula a prévia da promoção como o domínio: percentual, fixo e mínimo de R$ 0,01', () => {
    expect(precoComDesconto(24, 'Percentual', 20)).toBe(19.2);
    expect(precoComDesconto(29, 'ValorFixo', 5)).toBe(24);
    expect(precoComDesconto(5, 'ValorFixo', 10)).toBe(0.01);
  });
});

describe('Gestão do cardápio (UC05–UC09)', () => {
  let http!: HttpTestingController;
  let fixture: ComponentFixture<GestaoCardapio>;
  let avisos: AvisoService;

  async function renderizar(): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [GestaoCardapio],
      providers: [provideHttpClient(), provideHttpClientTesting(), provedoresDeLocalizacao()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    avisos = TestBed.inject(AvisoService);
    fixture = TestBed.createComponent(GestaoCardapio);
    await fixture.whenStable();
    responderLista();
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  /** Depois de cada ação a tela recarrega itens e promoções. */
  function responderLista(): void {
    http.expectOne(`${API}/api/cardapio`).flush(ITENS);
    http.expectOne(`${API}/api/promocoes`).flush([PROMOCAO]);
  }

  function botao(raiz: ParentNode, texto: string): HTMLButtonElement {
    return [...raiz.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.trim().includes(texto))!;
  }

  function preencher(pagina: HTMLElement, seletor: string, valor: string): void {
    const campo = pagina.querySelector<HTMLInputElement | HTMLSelectElement>(seletor)!;
    campo.value = valor;
    campo.dispatchEvent(new Event(campo.tagName === 'SELECT' ? 'change' : 'input'));
  }

  function texto(pagina: HTMLElement): string {
    return (pagina.textContent ?? '').replace(/ /g, ' ');
  }

  afterEach(() => http.verify());

  it('lista os itens com preço promocional e filtra pela busca', async () => {
    const pagina = await renderizar();

    expect(pagina.querySelectorAll('tbody tr').length).toBe(2);
    expect(texto(pagina)).toContain('R$ 57,80');

    preencher(pagina, '#busca', 'caipi');
    await fixture.whenStable();

    expect(pagina.querySelectorAll('tbody tr').length).toBe(1);
  });

  it('cadastro mostra dentro do painel as mensagens da API', async () => {
    const pagina = await renderizar();

    botao(pagina, 'Novo item').click();
    await fixture.whenStable();
    preencher(pagina, '#item-categoria', 'Bebida');
    await fixture.whenStable();
    pagina.querySelector('app-folha form')!.dispatchEvent(new Event('submit'));

    const requisicao = http.expectOne({ method: 'POST', url: `${API}/api/cardapio` });
    expect(requisicao.request.body.categoria).toBe('Bebida');
    requisicao.flush({ erros: ['O nome é obrigatório.', 'O preço deve ser maior que zero.'] }, { status: 400, statusText: 'Bad Request' });
    await fixture.whenStable();

    const alerta = pagina.querySelector('app-folha [role="alert"]')!;
    expect(alerta.textContent).toContain('O nome é obrigatório.');
    expect(alerta.textContent).toContain('O preço deve ser maior que zero.');
  });

  it('cadastra o item com preço em reais, flags e foto', async () => {
    const pagina = await renderizar();

    botao(pagina, 'Novo item').click();
    await fixture.whenStable();
    preencher(pagina, '#item-nome', 'Limonada suíça');
    preencher(pagina, '#item-categoria', 'Bebida');
    preencher(pagina, '#item-preco', '14,50');
    preencher(pagina, '#item-foto', '/fotos/limonada.jpg');
    botao(pagina.querySelector('app-folha')!, 'Vegano').click();
    await fixture.whenStable();
    pagina.querySelector('app-folha form')!.dispatchEvent(new Event('submit'));

    const requisicao = http.expectOne({ method: 'POST', url: `${API}/api/cardapio` });
    expect(requisicao.request.body).toEqual({
      nome: 'Limonada suíça',
      categoria: 'Bebida',
      preco: 14.5,
      descricao: '',
      flagsDieteticas: ['Vegano'],
      imagem: '/fotos/limonada.jpg',
    });
    requisicao.flush(itemCardapio({ id: 3, nome: 'Limonada suíça' }));
    responderLista();
    await fixture.whenStable();

    expect(pagina.querySelector('app-folha')).toBeNull();
    expect(avisos.aviso()?.mensagem).toBe('Limonada suíça cadastrado');
  });

  it('tirar do ar oferece desfazer, que devolve a disponibilidade', async () => {
    const pagina = await renderizar();

    pagina.querySelector<HTMLInputElement>('tbody tr .interruptor input')!.click();
    const tirar = http.expectOne(`${API}/api/cardapio/1/disponibilidade`);
    expect(tirar.request.body).toEqual({ disponivel: false });
    tirar.flush(null);
    responderLista();
    await fixture.whenStable();

    avisos.desfazer();
    const voltar = http.expectOne(`${API}/api/cardapio/1/disponibilidade`);
    expect(voltar.request.body).toEqual({ disponivel: true });
    voltar.flush(null);
    responderLista();
    await fixture.whenStable();
  });

  it('cria a promoção com a prévia dos preços e os itens escolhidos', async () => {
    const pagina = await renderizar();

    botao(pagina, 'Promoções').click();
    await fixture.whenStable();
    botao(pagina, 'Nova promoção').click();
    await fixture.whenStable();
    preencher(pagina, '#promo-descricao', 'Happy hour');
    preencher(pagina, '#promo-valor', '20');
    [...pagina.querySelectorAll<HTMLLabelElement>('.grade-itens label')].find((l) => l.textContent?.includes('Caipirinha'))!
      .querySelector('input')!
      .click();
    await fixture.whenStable();

    expect(texto(pagina.querySelector('app-folha') as HTMLElement)).toContain('R$ 19,20');

    pagina.querySelector('app-folha form')!.dispatchEvent(new Event('submit'));
    const requisicao = http.expectOne({ method: 'POST', url: `${API}/api/promocoes` });
    expect(requisicao.request.body).toMatchObject({
      descricao: 'Happy hour',
      tipoDesconto: 'Percentual',
      valorDesconto: 20,
      itemCardapioIds: [2],
    });
    requisicao.flush(PROMOCAO);
    responderLista();
    await fixture.whenStable();
  });

  it('remover promoção pede confirmação e chama a API', async () => {
    const pagina = await renderizar();

    botao(pagina, 'Promoções').click();
    await fixture.whenStable();
    botao(pagina.querySelector('.promocao')!, 'Remover').click();
    await fixture.whenStable();

    expect(texto(pagina)).toContain('Itens já lançados mantêm o preço cobrado');
    botao(pagina.querySelector('app-folha')!, 'Remover').click();
    http.expectOne({ method: 'DELETE', url: `${API}/api/promocoes/7` }).flush(null);
    responderLista();
    await fixture.whenStable();

    expect(avisos.aviso()?.mensagem).toContain('removida');
  });
});
