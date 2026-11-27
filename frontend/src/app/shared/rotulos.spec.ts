import { composicaoSugerida, duracao, minutosDesde, tempoRelativo } from './rotulos';

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
    expect(tempoRelativo('2026-09-13T20:30:00', agora)).toBe('há 4 dias');
  });

  it('escreve a duração em minutos, horas ou dias', () => {
    expect(duracao(45)).toBe('45 min');
    expect(duracao(120)).toBe('2 h');
    expect(duracao(135)).toBe('2 h 15 min');
    expect(duracao(1500)).toBe('1 dia');
    expect(duracao(5804)).toBe('4 dias');
  });
});
