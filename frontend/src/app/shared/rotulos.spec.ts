import { composicaoSugerida, minutosDesde, tempoRelativo } from './rotulos';

describe('rótulos e regras de apresentação', () => {
  it.each([
    [1, false, 'Solo'],
    [2, false, 'Casal'],
    [3, false, 'GrupoPequeno'],
    [4, false, 'GrupoPequeno'],
    [5, false, 'GrupoGrande'],
    [3, true, 'Familia'],
    [6, true, 'Familia'],
  ])('sugere a composição de %i pessoas (infantil: %s) como %s', (pessoas, infantil, esperado) => {
    expect(composicaoSugerida(pessoas, infantil)).toBe(esperado);
  });

  it('conta os minutos desde o lançamento', () => {
    const agora = new Date('2026-09-17T20:30:00').getTime();

    expect(minutosDesde('2026-09-17T20:18:00', agora)).toBe(12);
    expect(minutosDesde('2026-09-17T20:31:00', agora)).toBe(0);
  });

  it('escreve o tempo do jeito que o garçom lê', () => {
    const agora = new Date('2026-09-17T20:30:00').getTime();

    expect(tempoRelativo('2026-09-17T20:29:40', agora)).toBe('agora');
    expect(tempoRelativo('2026-09-17T20:18:00', agora)).toBe('há 12 min');
    expect(tempoRelativo('2026-09-17T18:30:00', agora)).toBe('há 2 h');
  });
});
