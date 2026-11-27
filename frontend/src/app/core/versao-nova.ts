import { NavigationError } from '@angular/router';

/**
 * Cada tela é um arquivo carregado na primeira vez que alguém entra nela, com um código da versão no nome.
 * Quando o sistema é atualizado, uma aba aberta antes da atualização (o celular do garçom fica o turno inteiro
 * na mesma página) ainda pede o arquivo da versão anterior, que deixou de existir, e o toque no botão não leva a
 * lugar nenhum. Nesse caso, a página é carregada de novo já no endereço de destino.
 */
const MARCA = 'gastra.recarregadoPorVersaoNova';
/** Se o arquivo continuar faltando logo depois de recarregar, o problema é outro (rede): não recarrega em loop. */
const INTERVALO_MINIMO_MS = 10_000;

export function faltaArquivoDaVersao(erro: unknown): boolean {
  const mensagem = erro instanceof Error ? `${erro.name} ${erro.message}` : String(erro);
  return /dynamically imported module|importing a module script failed|ChunkLoadError|loading chunk/i.test(
    mensagem,
  );
}

export function recarregarSeVersaoMudou(
  evento: Pick<NavigationError, 'error' | 'url'>,
  recarregar: (url: string) => void = (url) => window.location.assign(url),
  armazenamento: Pick<Storage, 'getItem' | 'setItem'> = sessionStorage,
  agora: () => number = Date.now,
): void {
  if (!faltaArquivoDaVersao(evento.error)) {
    return;
  }
  try {
    const ultima = Number(armazenamento.getItem(MARCA) ?? 0);
    if (agora() - ultima < INTERVALO_MINIMO_MS) {
      return;
    }
    armazenamento.setItem(MARCA, String(agora()));
  } catch {
    // Sem acesso ao armazenamento, recarrega mesmo assim: é uma vez por toque.
  }
  recarregar(evento.url);
}
