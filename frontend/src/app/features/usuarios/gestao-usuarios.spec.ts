import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Usuario } from '../../core/api/modelos';
import { API, sessaoDoGarcom } from '../../core/api/testes';
import { ARMAZENAMENTO_DA_SESSAO } from '../../core/configuracao';
import { AvisoService } from '../../shared/aviso/aviso.service';
import { GestaoUsuarios } from './gestao-usuarios';

/** A sessão é do Gerente de id 1: é ele quem usa esta tela. */
function sessaoDoGerente(): Storage {
  const armazenamento = sessaoDoGarcom(1, 'Roberta Lima');
  armazenamento.setItem(
    'gastra.sessao',
    JSON.stringify({ token: 'token-de-teste', id: 1, nome: 'Roberta Lima', papel: 'Gerente', expiraEm: Date.now() + 3_600_000 }),
  );
  return armazenamento;
}

const CONTAS: Usuario[] = [
  { id: 1, nome: 'Roberta Lima', email: 'roberta@gastra.test', papel: 'Gerente', ativo: true, segundoFatorConfigurado: true },
  { id: 2, nome: 'Marcos Tavares', email: 'marcos@gastra.test', papel: 'Coordenador', ativo: true, segundoFatorConfigurado: true },
  { id: 11, nome: 'Carla Mendes', email: 'carla@gastra.test', papel: 'Garcom', ativo: true, segundoFatorConfigurado: false },
  { id: 19, nome: 'Paulo Henrique', email: 'paulo@gastra.test', papel: 'Garcom', ativo: false, segundoFatorConfigurado: false },
];

describe('Gestão de usuários (UC04)', () => {
  let http!: HttpTestingController;
  let fixture: ComponentFixture<GestaoUsuarios>;
  let avisos: AvisoService;

  async function renderizar(): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [GestaoUsuarios],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: sessaoDoGerente() },
      ],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    avisos = TestBed.inject(AvisoService);
    fixture = TestBed.createComponent(GestaoUsuarios);
    await fixture.whenStable();
    http.expectOne(`${API}/api/usuarios`).flush(CONTAS);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function linha(pagina: HTMLElement, nome: string): HTMLTableRowElement {
    return [...pagina.querySelectorAll('tbody tr')].find((tr) => tr.textContent?.includes(nome)) as HTMLTableRowElement;
  }

  function botao(raiz: ParentNode, texto: string): HTMLButtonElement {
    return [...raiz.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.trim().includes(texto))!;
  }

  function preencher(pagina: HTMLElement, seletor: string, valor: string): void {
    const campo = pagina.querySelector<HTMLInputElement | HTMLSelectElement>(seletor)!;
    campo.value = valor;
    campo.dispatchEvent(new Event(campo.tagName === 'SELECT' ? 'change' : 'input'));
  }

  afterEach(() => http.verify());

  it('lista os ativos por padrão e filtra pela situação', async () => {
    const pagina = await renderizar();

    expect(pagina.querySelectorAll('tbody tr').length).toBe(3);

    botao(pagina, 'Inativos').click();
    await fixture.whenStable();

    expect(pagina.querySelectorAll('tbody tr').length).toBe(1);
    expect(pagina.textContent).toContain('Paulo Henrique');
  });

  it('mostra a verificação em duas etapas só para quem usa (RF16)', async () => {
    const pagina = await renderizar();

    expect(linha(pagina, 'Marcos Tavares').textContent).toContain('Configurada');
    expect(linha(pagina, 'Carla Mendes').textContent).toContain('Não exigida');
  });

  it('na própria conta não oferece inativar, senha nem zerar as duas etapas', async () => {
    const pagina = await renderizar();

    const minha = linha(pagina, 'Roberta Lima');
    expect(minha.textContent).toContain('você');
    expect([...minha.querySelectorAll('button')].map((b) => b.textContent?.trim())).toEqual(['Editar']);
  });

  it('cadastra a conta com papel e senha inicial', async () => {
    const pagina = await renderizar();

    botao(pagina, 'Novo usuário').click();
    await fixture.whenStable();
    preencher(pagina, '#conta-nome', 'Beatriz Alves');
    preencher(pagina, '#conta-email', 'beatriz@gastra.test');
    preencher(pagina, '#conta-papel', 'Garcom');
    preencher(pagina, '#conta-senha', 'senha-inicial-de-teste');
    pagina.querySelector('app-folha form')!.dispatchEvent(new Event('submit'));

    const requisicao = http.expectOne({ method: 'POST', url: `${API}/api/usuarios` });
    expect(requisicao.request.body).toEqual({
      nome: 'Beatriz Alves',
      email: 'beatriz@gastra.test',
      senha: 'senha-inicial-de-teste',
      papel: 'Garcom',
    });
    requisicao.flush(CONTAS[2]);
    http.expectOne(`${API}/api/usuarios`).flush(CONTAS);
    await fixture.whenStable();

    expect(avisos.aviso()?.mensagem).toBe('Beatriz Alves cadastrado');
  });

  it('erro da API aparece dentro do painel de cadastro', async () => {
    const pagina = await renderizar();

    botao(pagina, 'Novo usuário').click();
    await fixture.whenStable();
    preencher(pagina, '#conta-nome', 'Alguém');
    preencher(pagina, '#conta-email', 'carla@gastra.test');
    preencher(pagina, '#conta-papel', 'Garcom');
    preencher(pagina, '#conta-senha', '1234');
    pagina.querySelector('app-folha form')!.dispatchEvent(new Event('submit'));
    http.expectOne({ method: 'POST', url: `${API}/api/usuarios` }).flush(
      { erros: ['Já existe uma conta com este e-mail.', 'A senha deve ter pelo menos 8 caracteres.'] },
      { status: 400, statusText: 'Bad Request' },
    );
    await fixture.whenStable();

    const alerta = pagina.querySelector('app-folha [role="alert"]')!;
    expect(alerta.textContent).toContain('Já existe uma conta com este e-mail.');
    expect(alerta.textContent).toContain('A senha deve ter pelo menos 8 caracteres.');
  });

  it('inativar pede confirmação e explica que o acesso cai na hora (RF18)', async () => {
    const pagina = await renderizar();

    botao(linha(pagina, 'Carla Mendes'), 'Inativar').click();
    await fixture.whenStable();
    expect(pagina.querySelector('app-folha')?.textContent).toContain('perde o acesso');

    botao(pagina.querySelector('app-folha')!, 'Inativar').click();
    const requisicao = http.expectOne({ method: 'PATCH', url: `${API}/api/usuarios/11/situacao` });
    expect(requisicao.request.body).toEqual({ ativo: false });
    requisicao.flush(null);
    http.expectOne(`${API}/api/usuarios`).flush(CONTAS);
    await fixture.whenStable();

    expect(avisos.aviso()?.mensagem).toBe('Carla Mendes inativado');
  });

  it('reativar não pede confirmação: é ação sem perda', async () => {
    const pagina = await renderizar();

    botao(pagina, 'Inativos').click();
    await fixture.whenStable();
    botao(linha(pagina, 'Paulo Henrique'), 'Reativar').click();

    const requisicao = http.expectOne({ method: 'PATCH', url: `${API}/api/usuarios/19/situacao` });
    expect(requisicao.request.body).toEqual({ ativo: true });
    requisicao.flush(null);
    http.expectOne(`${API}/api/usuarios`).flush(CONTAS);
    await fixture.whenStable();
  });

  it('redefine a senha avisando que as sessões caem (#141)', async () => {
    const pagina = await renderizar();

    botao(linha(pagina, 'Carla Mendes'), 'Senha').click();
    await fixture.whenStable();
    expect(pagina.querySelector('app-folha')?.textContent).toContain('sessões abertas dessa conta caem');

    preencher(pagina, '#senha-nova', 'senha-nova-de-teste');
    pagina.querySelector('app-folha form')!.dispatchEvent(new Event('submit'));
    const requisicao = http.expectOne({ method: 'POST', url: `${API}/api/usuarios/11/senha` });
    expect(requisicao.request.body).toEqual({ senha: 'senha-nova-de-teste' });
    requisicao.flush(null);
    http.expectOne(`${API}/api/usuarios`).flush(CONTAS);
    await fixture.whenStable();

    expect(avisos.aviso()?.mensagem).toContain('Senha de Carla Mendes redefinida');
  });

  it('zerar as duas etapas só aparece para quem já configurou, e explica o efeito (RN07)', async () => {
    const pagina = await renderizar();

    expect(botao(linha(pagina, 'Carla Mendes'), 'Zerar 2 etapas')).toBeUndefined();

    botao(linha(pagina, 'Marcos Tavares'), 'Zerar 2 etapas').click();
    await fixture.whenStable();
    expect(pagina.querySelector('app-folha')?.textContent).toContain('vincula o aplicativo autenticador de novo');

    botao(pagina.querySelector('app-folha')!, 'Zerar').click();
    http.expectOne({ method: 'DELETE', url: `${API}/api/usuarios/2/segundo-fator` }).flush(null);
    http.expectOne(`${API}/api/usuarios`).flush(CONTAS);
    await fixture.whenStable();

    expect(avisos.aviso()?.mensagem).toContain('zerada');
  });
});
