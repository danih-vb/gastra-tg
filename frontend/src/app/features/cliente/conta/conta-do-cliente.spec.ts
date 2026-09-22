import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ComandaDoCliente } from '../../../core/api/modelos';
import { API } from '../../../core/api/testes';
import { provedoresDeLocalizacao } from '../../../core/localizacao';
import { ContaDoCliente } from './conta-do-cliente';

const CONTA: ComandaDoCliente = {
  mesa: '12',
  dataHoraAbertura: '2026-09-17T19:38:00',
  fechada: false,
  itens: [
    { nome: 'Kids: mini hambúrguer com fritas', quantidade: 2, valor: 72, situacao: 'Pendente' },
    { nome: 'Suco de laranja 400 ml', quantidade: 1, valor: 13, situacao: 'Cancelado' },
    { nome: 'Picanha na chapa', quantidade: 1, valor: 119, situacao: 'Entregue' },
  ],
  subtotal: 191,
  taxaServico: 19.1,
  total: 210.1,
  podeAvaliar: false,
  avaliacaoEnviada: false,
};

/** Conta fechada e dentro do prazo: o estado em que a API libera avaliar (RF25). */
const FECHADA_AVALIAVEL: ComandaDoCliente = { ...CONTA, fechada: true, podeAvaliar: true };

describe('Conta do cliente (UC20)', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<ContaDoCliente>;

  async function renderizar(resposta: ComandaDoCliente | 'nao-encontrada', codigo = 'M3R8TD'): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [ContaDoCliente],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), provedoresDeLocalizacao()],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(ContaDoCliente);
    fixture.componentRef.setInput('codigo', codigo);
    await fixture.whenStable();

    const pedido = http.expectOne(`${API}/api/comandas/consulta/${codigo}`);
    if (resposta === 'nao-encontrada') {
      pedido.flush({ erros: ['Comanda não encontrada.'] }, { status: 404, statusText: 'Not Found' });
    } else {
      pedido.flush(resposta);
    }
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function texto(pagina: HTMLElement): string {
    return (pagina.textContent ?? '').replace(/ /g, ' ');
  }

  afterEach(() => {
    http.verify();
    fixture.destroy(); // encerra a atualização automática
  });

  it('mostra os itens com a situação de cada um e os totais', async () => {
    const pagina = await renderizar(CONTA);

    expect(texto(pagina)).toContain('Sua conta · Mesa 12');
    expect(texto(pagina)).toContain('A caminho');
    expect(texto(pagina)).toContain('Entregue');
    expect(texto(pagina)).toContain('Cancelado · não cobrado');
    expect(texto(pagina)).toContain('R$ 191,00');
    expect(texto(pagina)).toContain('R$ 19,10');
    expect(texto(pagina)).toContain('Total parcial');
    expect(texto(pagina)).toContain('R$ 210,10');
  });

  it('explica que a taxa é opcional e que nada pessoal aparece (RN04)', async () => {
    const pagina = await renderizar(CONTA);

    expect(texto(pagina)).toContain('A taxa de serviço é opcional');
    expect(texto(pagina)).toContain('Nenhum dado pessoal é exibido');
  });

  it('com a conta fechada, agradece e mostra o total final', async () => {
    const pagina = await renderizar({ ...CONTA, fechada: true });

    expect(texto(pagina)).toContain('Conta fechada');
    expect(texto(pagina)).toContain('Os valores abaixo são os finais');
    expect(texto(pagina)).not.toContain('Total parcial');
    expect(pagina.querySelector('[aria-label="Atualizar agora"]')).toBeNull();
  });

  it('código inexistente não vira erro de sistema: explica o que fazer', async () => {
    const pagina = await renderizar('nao-encontrada', 'XXXXXX');

    expect(texto(pagina)).toContain('Não encontramos essa conta');
    expect(texto(pagina)).toContain('XXXXXX');
  });

  it('com a conta aberta, nao oferece avaliar: o atendimento ainda esta acontecendo', async () => {
    const pagina = await renderizar(CONTA);

    expect(texto(pagina)).not.toContain('Como foi o atendimento?');
  });

  it('envia a nota e o comentario, e depois agradece (RF25)', async () => {
    const pagina = await renderizar(FECHADA_AVALIAVEL);

    expect(texto(pagina)).toContain('Como foi o atendimento?');

    const quatro = [...pagina.querySelectorAll<HTMLButtonElement>('.nota')].find((b) => b.textContent?.trim() === '4')!;
    quatro.click();
    const campo = pagina.querySelector<HTMLTextAreaElement>('#comentario')!;
    campo.value = 'Atendimento rapido.';
    campo.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    [...pagina.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.includes('Enviar avaliacao'.replace('avaliacao', 'avaliação')))!.click();

    const requisicao = http.expectOne({ method: 'POST', url: `${API}/api/comandas/consulta/M3R8TD/avaliacao` });
    expect(requisicao.request.body).toEqual({ nota: 4, comentario: 'Atendimento rapido.' });
    requisicao.flush(null);

    // A tela recarrega em vez de marcar na mao: quem decide se ja foi avaliada e a API.
    http.expectOne(`${API}/api/comandas/consulta/M3R8TD`).flush({ ...FECHADA_AVALIAVEL, podeAvaliar: false, avaliacaoEnviada: true });
    await fixture.whenStable();

    expect(texto(pagina)).toContain('Obrigado pela avaliação');
    expect(texto(pagina)).not.toContain('Como foi o atendimento?');
  });

  it('sem escolher nota, o botao de enviar fica desabilitado', async () => {
    const pagina = await renderizar(FECHADA_AVALIAVEL);

    const enviar = [...pagina.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.includes('Enviar'))!;

    expect(enviar.disabled).toBe(true);
  });

  it('erro da API na avaliacao aparece dentro do bloco, sem derrubar a conta', async () => {
    const pagina = await renderizar(FECHADA_AVALIAVEL);

    [...pagina.querySelectorAll<HTMLButtonElement>('.nota')].find((b) => b.textContent?.trim() === '5')!.click();
    // Sem esperar a detecção de mudanças, o botão ainda estaria desabilitado e o clique não faria nada.
    await fixture.whenStable();
    [...pagina.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.includes('Enviar'))!.click();
    http.expectOne({ method: 'POST', url: `${API}/api/comandas/consulta/M3R8TD/avaliacao` }).flush(
      { erros: ['Esta conta ja foi avaliada. Obrigado!'] },
      { status: 422, statusText: 'Unprocessable Entity' },
    );
    await fixture.whenStable();

    expect(pagina.querySelector('.avaliacao [role="alert"]')?.textContent).toContain('ja foi avaliada');
    expect(texto(pagina)).toContain('Total');
  });

  it('avisa para nao escrever dado pessoal no comentario', async () => {
    const pagina = await renderizar(FECHADA_AVALIAVEL);

    expect(texto(pagina)).toContain('anônima');
  });

  it('atualizar agora busca a conta de novo', async () => {
    const pagina = await renderizar(CONTA);

    pagina.querySelector<HTMLButtonElement>('[aria-label="Atualizar agora"]')!.click();
    http.expectOne(`${API}/api/comandas/consulta/M3R8TD`).flush({
      ...CONTA,
      itens: [...CONTA.itens, { nome: 'Pudim de leite', quantidade: 1, valor: 19, situacao: 'Pendente' }],
    });
    await fixture.whenStable();

    expect(texto(pagina)).toContain('Pudim de leite');
  });
});
