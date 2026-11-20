import { faltaArquivoDaVersao, recarregarSeVersaoMudou } from './versao-nova';

class Armazenamento {
  private readonly dados = new Map<string, string>();
  getItem(chave: string): string | null {
    return this.dados.get(chave) ?? null;
  }
  setItem(chave: string, valor: string): void {
    this.dados.set(chave, valor);
  }
}

const ARQUIVO_ANTIGO = new TypeError(
  'Failed to fetch dynamically imported module: http://192.168.1.3:4200/chunk-ANTIGO.js',
);

describe('recarregarSeVersaoMudou', () => {
  it('reconhece o arquivo de tela que sumiu com a atualização (Chrome, Safari e Firefox)', () => {
    expect(faltaArquivoDaVersao(ARQUIVO_ANTIGO)).toBe(true);
    expect(faltaArquivoDaVersao(new TypeError('Importing a module script failed.'))).toBe(true);
    expect(faltaArquivoDaVersao(new Error('error loading dynamically imported module'))).toBe(true);
    expect(faltaArquivoDaVersao(new Error('Http failure response: 500'))).toBe(false);
  });

  it('com a versão antiga na aba, carrega a página de novo no endereço de destino', () => {
    const destinos: string[] = [];

    recarregarSeVersaoMudou(
      { error: ARQUIVO_ANTIGO, url: '/comandas/4156/fechar' },
      (url) => destinos.push(url),
      new Armazenamento(),
      () => 100_000,
    );

    expect(destinos).toEqual(['/comandas/4156/fechar']);
  });

  it('não recarrega por outros erros de navegação', () => {
    const destinos: string[] = [];

    recarregarSeVersaoMudou(
      { error: new Error('guarda recusou'), url: '/comandas' },
      (url) => destinos.push(url),
      new Armazenamento(),
      () => 100_000,
    );

    expect(destinos).toEqual([]);
  });

  it('não entra em loop: se o arquivo continua faltando logo depois de recarregar, para', () => {
    const destinos: string[] = [];
    const armazenamento = new Armazenamento();
    let agora = 100_000;

    recarregarSeVersaoMudou(
      { error: ARQUIVO_ANTIGO, url: '/a' },
      (url) => destinos.push(url),
      armazenamento,
      () => agora,
    );
    agora += 3_000;
    recarregarSeVersaoMudou(
      { error: ARQUIVO_ANTIGO, url: '/a' },
      (url) => destinos.push(url),
      armazenamento,
      () => agora,
    );
    agora += 60_000;
    recarregarSeVersaoMudou(
      { error: ARQUIVO_ANTIGO, url: '/a' },
      (url) => destinos.push(url),
      armazenamento,
      () => agora,
    );

    expect(destinos).toEqual(['/a', '/a']);
  });
});
