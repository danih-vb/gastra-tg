import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { InicioDoCliente } from './inicio-do-cliente';

describe('Início do cliente (QR code da mesa)', () => {
  let fixture: ComponentFixture<InicioDoCliente>;
  let router: Router;

  async function renderizar(): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [InicioDoCliente],
      providers: [provideRouter([])],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(InicioDoCliente);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  async function digitar(pagina: HTMLElement, codigo: string): Promise<void> {
    const campo = pagina.querySelector<HTMLInputElement>('#codigo')!;
    campo.value = codigo;
    campo.dispatchEvent(new Event('input'));
    await fixture.whenStable();
    pagina.querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  }

  it('leva para a conta da mesa com o código digitado, em maiúsculas', async () => {
    const pagina = await renderizar();

    await digitar(pagina, 'm3r8td');

    expect(router.navigate).toHaveBeenCalledWith(['/cliente/conta', 'M3R8TD']);
  });

  it('código incompleto não chama a API: avisa no próprio campo', async () => {
    const pagina = await renderizar();

    await digitar(pagina, 'M3R');

    expect(router.navigate).not.toHaveBeenCalled();
    expect(pagina.textContent).toContain('O código tem 6 caracteres.');
  });

  it('deixa claro que o pedido é feito com o garçom', async () => {
    const pagina = await renderizar();

    expect(pagina.textContent).toContain('Pedidos são feitos com o garçom');
  });
});
