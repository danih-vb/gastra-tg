import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { ARMAZENAMENTO_DA_SESSAO } from './core/configuracao';
import { Papel } from './core/sessao/modelos';
import { ArmazenamentoEmMemoria } from './core/sessao/testes';

describe('App', () => {
  async function renderizar(papel: Papel | null): Promise<HTMLElement> {
    const armazenamento = new ArmazenamentoEmMemoria();
    if (papel) {
      armazenamento.setItem(
        'gastra.sessao',
        JSON.stringify({ token: 't', nome: 'Ana', papel, expiraEm: Date.now() + 60_000 }),
      );
    }
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: armazenamento },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function itensDoMenu(pagina: HTMLElement): string[] {
    return [...pagina.querySelectorAll('nav a')].map((a) => (a.textContent ?? '').trim());
  }

  it('exibe o nome do sistema no cabeçalho', async () => {
    const pagina = await renderizar(null);

    expect(pagina.querySelector('.barra')?.textContent).toContain('GASTRA');
  });

  it('sem login, não mostra menu nem botão de sair', async () => {
    const pagina = await renderizar(null);

    expect(pagina.querySelector('nav')).toBeNull();
    expect(pagina.querySelector('.sair')).toBeNull();
  });

  it('o garçom vê só as telas dele', async () => {
    const pagina = await renderizar('Garcom');

    expect(itensDoMenu(pagina)).toEqual(['Comandas', 'A entregar', 'Desempenho']);
    expect(pagina.querySelector('.barra')?.textContent).toContain('Ana · Garçom');
  });

  it('o metre vê a alocação e o salão, e não as telas do garçom', async () => {
    const pagina = await renderizar('Metre');

    expect(itensDoMenu(pagina)).toEqual(['Alocação', 'Salão agora']);
  });

  it('o gerente vê a gestão, mas não a alocação do metre', async () => {
    const pagina = await renderizar('Gerente');

    expect(itensDoMenu(pagina)).toEqual(['Cardápio', 'Análises', 'Salão', 'Usuários']);
  });
});
