import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MedalhasDoGarcom, Usuario } from '../../core/api/modelos';
import { API, sessaoDoGarcom } from '../../core/api/testes';
import { ARMAZENAMENTO_DA_SESSAO } from '../../core/configuracao';
import { SessaoService } from '../../core/sessao/sessao.service';
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
  { id: 1, nome: 'Roberta Lima', email: 'roberta@gastra.test', papel: 'Gerente', ativo: true, segundoFatorConfigurado: true, foto: null },
  { id: 2, nome: 'Marcos Tavares', email: 'marcos@gastra.test', papel: 'Coordenador', ativo: true, segundoFatorConfigurado: true, foto: null },
  { id: 11, nome: 'Carla Mendes', email: 'carla@gastra.test', papel: 'Garcom', ativo: true, segundoFatorConfigurado: false, foto: null },
  { id: 19, nome: 'Paulo Henrique', email: 'paulo@gastra.test', papel: 'Garcom', ativo: false, segundoFatorConfigurado: false, foto: null },
];

const MEDALHAS: MedalhasDoGarcom = {
  garcomId: 11,
  nome: 'Carla Mendes',
  mes: '2026-11-01',
  medalhas: [
    { codigo: 'top-3', conquistada: true, progresso: 2, alvo: 3 },
    { codigo: 'mesa-cheia', conquistada: false, progresso: 64, alvo: 100 },
    { codigo: 'maratonista', conquistada: true, progresso: 20, alvo: 20 },
    { codigo: 'bem-avaliado', conquistada: false, progresso: 0, alvo: 4.5 },
    { codigo: 'vendedor-de-destaque', conquistada: false, progresso: 8, alvo: 20 },
    { codigo: 'meta-batida', conquistada: false, progresso: 0, alvo: 1 },
  ],
  historico: [
    { mes: '2026-11-01', conquistadas: ['top-3', 'maratonista'] },
    { mes: '2026-10-01', conquistadas: ['mesa-cheia', 'meta-batida'] },
    { mes: '2026-09-01', conquistadas: [] },
  ],
  mencoes: [],
};

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
  it('no perfil do garçom, o Gerente vê as medalhas e o histórico de conquistas', async () => {
    const pagina = await renderizar();

    botao(linha(pagina, 'Carla Mendes'), 'Editar').click();
    await fixture.whenStable();
    http.expectOne(`${API}/api/indicadores/medalhas?garcomId=11`).flush(MEDALHAS);
    await fixture.whenStable();

    expect(pagina.querySelector('.medalhas-da-conta')?.textContent).toContain('Maratonista');
    expect(pagina.querySelectorAll('.medalhas-da-conta .historico li').length).toBe(2);
  });

  it('em conta que não é de garçom, não busca medalhas', async () => {
    const pagina = await renderizar();

    botao(linha(pagina, 'Marcos Tavares'), 'Editar').click();
    await fixture.whenStable();

    expect(pagina.querySelector('.medalhas-da-conta')).toBeNull();
  });
  it('o Gerente dá uma menção honrosa escolhendo ícone, cor e texto, e pode removê-la', async () => {
    const pagina = await renderizar();
    botao(linha(pagina, 'Carla Mendes'), 'Editar').click();
    await fixture.whenStable();
    http.expectOne(`${API}/api/indicadores/medalhas?garcomId=11`).flush(MEDALHAS);
    await fixture.whenStable();

    botao(pagina, 'Menção honrosa').click();
    await fixture.whenStable();
    pagina.querySelector<HTMLButtonElement>('[role=radio][aria-label="Estrela"]')!.click();
    pagina.querySelector<HTMLButtonElement>('[role=radio][aria-label="Roxo"]')!.click();
    preencher(pagina, '#mencao-titulo', 'Melhor ideia do mês');
    preencher(pagina, '#mencao-descricao', 'Sugeriu o cardápio infantil.');
    await fixture.whenStable();
    expect(pagina.querySelector('.previa')?.textContent).toContain('Melhor ideia do mês');
    botao(pagina, 'Dar menção').click();

    const pedido = http.expectOne({ method: 'POST', url: `${API}/api/mencoes` });
    expect(pedido.request.body).toEqual({
      garcomId: 11,
      titulo: 'Melhor ideia do mês',
      descricao: 'Sugeriu o cardápio infantil.',
      icone: 'estrela',
      cor: 'roxo',
      mes: '2026-11-01',
    });
    pedido.flush({ id: 9, titulo: 'Melhor ideia do mês', descricao: 'Sugeriu o cardápio infantil.', icone: 'estrela', cor: 'roxo', mes: '2026-11-01' });
    await fixture.whenStable();

    expect(pagina.querySelector('app-nova-mencao')).toBeNull();
    expect(pagina.querySelector('.mencoes')?.textContent).toContain('Melhor ideia do mês');
    expect(avisos.aviso()?.mensagem).toContain('Menção honrosa dada');

    pagina.querySelector<HTMLButtonElement>('[aria-label="Remover Melhor ideia do mês"]')!.click();
    http.expectOne({ method: 'DELETE', url: `${API}/api/mencoes/9` }).flush(null);
    await fixture.whenStable();

    expect(pagina.querySelector('.mencoes')).toBeNull();
  });

  describe('foto do crachá', () => {
    const originais = { criar: URL.createObjectURL, liberar: URL.revokeObjectURL };

    beforeEach(() => {
      // O jsdom não implementa endereços blob:.
      URL.createObjectURL = () => 'blob:teste';
      URL.revokeObjectURL = () => undefined;
    });

    afterEach(() => {
      URL.createObjectURL = originais.criar;
      URL.revokeObjectURL = originais.liberar;
    });

    function escolherArquivo(pagina: HTMLElement): File {
      const campo = pagina.querySelector<HTMLInputElement>('#conta-foto')!;
      const foto = new File([new Uint8Array([1, 2, 3])], 'carla.png', { type: 'image/png' });
      Object.defineProperty(campo, 'files', { value: [foto], configurable: true });
      campo.dispatchEvent(new Event('change'));
      return foto;
    }

    /** O crachá da lista e o do painel buscam a foto com o token, cada um com o seu pedido. */
    async function entregarFotos(endereco: string): Promise<number> {
      await fixture.whenStable();
      fixture.detectChanges();
      const pedidos = http.match(`${API}${endereco}`);
      pedidos.forEach((p) => p.flush(new Blob([new Uint8Array([1])], { type: 'image/png' })));
      await fixture.whenStable();
      fixture.detectChanges();
      return pedidos.length;
    }

    it('envia o arquivo direto para a conta, e o crachá busca a foto pela rota com login', async () => {
      const pagina = await renderizar();
      botao(linha(pagina, 'Carla Mendes'), 'Editar').click();
      await fixture.whenStable();
      http.expectOne(`${API}/api/indicadores/medalhas?garcomId=11`).flush(MEDALHAS);
      await fixture.whenStable();

      const foto = escolherArquivo(pagina);

      const envio = http.expectOne({ method: 'PUT', url: `${API}/api/usuarios/11/foto` });
      expect((envio.request.body as FormData).get('arquivo')).toBe(foto);
      envio.flush({ ...CONTAS[2], foto: '/api/usuarios/fotos/nova.png' });

      expect(await entregarFotos('/api/usuarios/fotos/nova.png')).toBe(2);
      expect(avisos.aviso()?.mensagem).toBe('Foto atualizada');
      expect(linha(pagina, 'Carla Mendes').querySelector('app-avatar img')!.getAttribute('src')).toBe('blob:teste');
    });

    it('tirar a foto pede à API que apague o arquivo', async () => {
      CONTAS[2] = { ...CONTAS[2], foto: '/api/usuarios/fotos/antiga.png' };
      try {
        const pagina = await renderizar();
        await entregarFotos('/api/usuarios/fotos/antiga.png');
        botao(linha(pagina, 'Carla Mendes'), 'Editar').click();
        await fixture.whenStable();
        http.expectOne(`${API}/api/indicadores/medalhas?garcomId=11`).flush(MEDALHAS);
        await entregarFotos('/api/usuarios/fotos/antiga.png');

        botao(pagina, 'Tirar foto').click();
        http.expectOne({ method: 'DELETE', url: `${API}/api/usuarios/11/foto` }).flush({ ...CONTAS[2], foto: null });
        await fixture.whenStable();
        fixture.detectChanges();

        expect(avisos.aviso()?.mensagem).toBe('Foto removida');
        expect(linha(pagina, 'Carla Mendes').querySelector('app-avatar img')).toBeNull();
      } finally {
        CONTAS[2] = { ...CONTAS[2], foto: null };
      }
    });

    it('quando o Gerente troca a própria foto, o crachá do cabeçalho acompanha', async () => {
      const pagina = await renderizar();
      botao(linha(pagina, 'Roberta Lima'), 'Editar').click();
      await fixture.whenStable();

      escolherArquivo(pagina);
      http.expectOne({ method: 'PUT', url: `${API}/api/usuarios/1/foto` })
        .flush({ ...CONTAS[0], foto: '/api/usuarios/fotos/minha.png' });
      await entregarFotos('/api/usuarios/fotos/minha.png');

      expect(TestBed.inject(SessaoService).usuario()?.foto).toBe('/api/usuarios/fotos/minha.png');
    });
  });
});
