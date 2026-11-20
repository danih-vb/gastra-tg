import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { API, itemCardapio } from '../../../core/api/testes';
import { provedoresDeLocalizacao } from '../../../core/localizacao';
import { CardapioDigital } from './cardapio-digital';

describe('Cardápio digital (UC19)', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<CardapioDigital>;

  async function renderizar(): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [CardapioDigital],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provedoresDeLocalizacao(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(CardapioDigital);
    await fixture.whenStable();

    // A rota do cliente é a pública: só itens disponíveis, sem login.
    http.expectOne(`${API}/api/cardapio/digital`).flush([
      itemCardapio({
        id: 1,
        nome: 'Risoto de cogumelos',
        preco: 68,
        precoPromocional: 57.8,
        flagsDieteticas: ['Vegetariano', 'SemGluten'],
      }),
      itemCardapio({
        id: 2,
        nome: 'Picanha na chapa',
        preco: 119,
        flagsDieteticas: ['SemGluten'],
      }),
      itemCardapio({
        id: 3,
        nome: 'Pudim de leite',
        categoria: 'Sobremesa',
        preco: 19,
        flagsDieteticas: ['Vegetariano'],
      }),
    ]);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function texto(pagina: HTMLElement): string {
    return (pagina.textContent ?? '').replace(/ /g, ' ');
  }

  function chip(pagina: HTMLElement, rotulo: string): HTMLButtonElement {
    return [...pagina.querySelectorAll<HTMLButtonElement>('.chip')].find((c) =>
      c.textContent?.includes(rotulo),
    )!;
  }

  afterEach(() => http.verify());

  it('agrupa por categoria e mostra o preço promocional junto do cheio', async () => {
    const pagina = await renderizar();

    expect([...pagina.querySelectorAll('section.cartao h2')].map((h) => h.textContent)).toEqual([
      'Pratos',
      'Sobremesas',
    ]);
    expect(texto(pagina)).toContain('R$ 68,00');
    expect(texto(pagina)).toContain('R$ 57,80');
    expect(texto(pagina)).toContain('Promoção');
  });

  it('avisa que o pedido é com o garçom e que alergia precisa ser confirmada', async () => {
    const pagina = await renderizar();

    expect(texto(pagina)).toContain('Para pedir, chame o garçom');
    expect(texto(pagina)).toContain('Tem alergia?');
  });

  it('tocar no item abre a foto e os detalhes', async () => {
    const pagina = await renderizar();

    [...pagina.querySelectorAll<HTMLButtonElement>('button.abrir')]
      .find((b) => b.textContent?.includes('Pudim'))!
      .click();
    await fixture.whenStable();

    expect(pagina.querySelector('app-folha')?.textContent).toContain('Pudim de leite');
    expect(texto(pagina.querySelector('app-folha')!)).toContain('R$ 19,00');
  });

  it('monta a lista "Quero pedir" no próprio celular, com o total', async () => {
    localStorage.removeItem('gastra.queroPedir');
    const pagina = await renderizar();
    const coracao = (nome: string) =>
      pagina.querySelector<HTMLButtonElement>(`.favorito[aria-label="Quero pedir: ${nome}"]`)!;

    coracao('Picanha na chapa').click();
    coracao('Pudim de leite').click();
    await fixture.whenStable();

    expect(texto(pagina)).toContain('Quero pedir (2)');
    expect(JSON.parse(localStorage.getItem('gastra.queroPedir')!)).toEqual([2, 3]);

    pagina.querySelector<HTMLButtonElement>('.barra-lista button')!.click();
    await fixture.whenStable();
    expect(texto(pagina.querySelector('app-folha')!)).toContain('R$ 138,00');

    coracao('Pudim de leite').click();
    await fixture.whenStable();
    expect(texto(pagina)).toContain('Quero pedir (1)');
    localStorage.removeItem('gastra.queroPedir');
  });

  it('os filtros combinam entre si', async () => {
    const pagina = await renderizar();

    chip(pagina, 'Vegetariano').click();
    await fixture.whenStable();
    expect(pagina.querySelectorAll('.prato').length).toBe(2);

    chip(pagina, 'Sem glúten').click();
    await fixture.whenStable();

    expect(pagina.querySelectorAll('.prato').length).toBe(1);
    expect(texto(pagina)).toContain('Risoto de cogumelos');
    expect(texto(pagina)).toContain('1 opção com todos os filtros');
  });

  it('sem resultado, explica e oferece limpar os filtros', async () => {
    const pagina = await renderizar();

    chip(pagina, 'Vegano').click();
    await fixture.whenStable();

    expect(texto(pagina)).toContain('Nenhum prato com todos esses filtros');

    [...pagina.querySelectorAll<HTMLButtonElement>('button')]
      .find((b) => b.textContent?.includes('Limpar filtros'))!
      .click();
    await fixture.whenStable();

    expect(pagina.querySelectorAll('.prato').length).toBe(3);
  });
});
