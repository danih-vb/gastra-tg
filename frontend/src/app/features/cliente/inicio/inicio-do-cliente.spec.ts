import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { InicioDoCliente } from './inicio-do-cliente';

describe('Início do cliente', () => {
  it('leva ao cardápio, sem login', async () => {
    await TestBed.configureTestingModule({
      imports: [InicioDoCliente],
      providers: [provideRouter([])],
    }).compileComponents();

    const fixture = TestBed.createComponent(InicioDoCliente);
    await fixture.whenStable();
    const pagina = fixture.nativeElement as HTMLElement;

    expect(pagina.querySelector('a')!.getAttribute('href')).toBe('/cliente/cardapio');
  });
});
