import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { API, sessaoDoGarcom } from '../../core/api/testes';
import { ARMAZENAMENTO_DA_SESSAO } from '../../core/configuracao';
import { autenticacaoInterceptor } from '../../core/sessao/autenticacao.interceptor';
import { Avatar } from './avatar';

@Component({
  imports: [Avatar],
  template: `@if (visivel()) {
    <app-avatar nome="Carla Mendes" [foto]="foto()" />
  }`,
})
class Cracha {
  readonly foto = signal<string | null>(null);
  readonly visivel = signal(true);
}

describe('Avatar (foto do funcionário)', () => {
  let http!: HttpTestingController;
  let fixture: ComponentFixture<Cracha>;
  const criados: string[] = [];
  const liberados: string[] = [];
  const originais = { criar: URL.createObjectURL, liberar: URL.revokeObjectURL };

  beforeEach(() => {
    // O jsdom não implementa endereços blob:.
    criados.length = 0;
    liberados.length = 0;
    URL.createObjectURL = () => {
      const endereco = `blob:teste-${criados.length + 1}`;
      criados.push(endereco);
      return endereco;
    };
    URL.revokeObjectURL = (endereco: string) => void liberados.push(endereco);
  });

  afterEach(() => {
    http.verify();
    URL.createObjectURL = originais.criar;
    URL.revokeObjectURL = originais.liberar;
  });

  async function renderizar(foto: string | null): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [Cracha],
      providers: [
        provideHttpClient(withInterceptors([autenticacaoInterceptor])),
        provideHttpClientTesting(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: sessaoDoGarcom() },
      ],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(Cracha);
    fixture.componentInstance.foto.set(foto);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  async function atualizar(): Promise<void> {
    await fixture.whenStable();
    fixture.detectChanges();
    await fixture.whenStable();
  }

  it('sem foto mostra as iniciais e não chama a API', async () => {
    const pagina = await renderizar(null);

    expect(pagina.querySelector('img')).toBeNull();
    expect(pagina.textContent?.trim()).toBe('CM');
  });

  it('busca a foto com o token, porque um <img> sozinho não passaria do login', async () => {
    const pagina = await renderizar('/api/usuarios/fotos/abc.png');

    const requisicao = http.expectOne(`${API}/api/usuarios/fotos/abc.png`);
    expect(requisicao.request.responseType).toBe('blob');
    expect(requisicao.request.headers.get('Authorization')).toBe('Bearer token-de-teste');
    expect(pagina.textContent?.trim()).toBe('CM');

    requisicao.flush(new Blob([new Uint8Array([1, 2, 3])], { type: 'image/png' }));
    await atualizar();

    expect(pagina.querySelector('img')!.getAttribute('src')).toBe('blob:teste-1');
  });

  it('se a API não entregar a foto, ficam as iniciais', async () => {
    const pagina = await renderizar('/api/usuarios/fotos/apagada.png');

    http
      .expectOne(`${API}/api/usuarios/fotos/apagada.png`)
      .flush(null, { status: 404, statusText: 'Not Found' });
    await atualizar();

    expect(pagina.querySelector('img')).toBeNull();
    expect(pagina.textContent?.trim()).toBe('CM');
  });

  it('libera a foto da memória ao trocar, ao tirar e ao sair da tela', async () => {
    const pagina = await renderizar('/api/usuarios/fotos/primeira.png');
    http.expectOne(`${API}/api/usuarios/fotos/primeira.png`).flush(new Blob(['a']));
    await atualizar();

    fixture.componentInstance.foto.set('/api/usuarios/fotos/segunda.png');
    await atualizar();
    http.expectOne(`${API}/api/usuarios/fotos/segunda.png`).flush(new Blob(['b']));
    await atualizar();
    expect(pagina.querySelector('img')!.getAttribute('src')).toBe('blob:teste-2');
    expect(liberados).toEqual(['blob:teste-1']);

    fixture.componentInstance.foto.set(null);
    await atualizar();
    expect(pagina.querySelector('img')).toBeNull();
    expect(liberados).toEqual(['blob:teste-1', 'blob:teste-2']);

    fixture.componentInstance.foto.set('/api/usuarios/fotos/terceira.png');
    await atualizar();
    http.expectOne(`${API}/api/usuarios/fotos/terceira.png`).flush(new Blob(['c']));
    await atualizar();
    fixture.componentInstance.visivel.set(false);
    await atualizar();
    expect(liberados).toEqual(['blob:teste-1', 'blob:teste-2', 'blob:teste-3']);
  });
});
