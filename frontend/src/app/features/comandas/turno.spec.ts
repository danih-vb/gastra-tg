import { dataDeHoje, periodoDoTurno } from './turno';

describe('turno atual', () => {
  it('trata até as 17h como almoço, como as views de BI', () => {
    expect(periodoDoTurno(new Date('2026-09-17T11:30:00'))).toBe('Almoco');
    expect(periodoDoTurno(new Date('2026-09-17T16:59:00'))).toBe('Almoco');
    expect(periodoDoTurno(new Date('2026-09-17T17:00:00'))).toBe('Jantar');
    expect(periodoDoTurno(new Date('2026-09-17T22:10:00'))).toBe('Jantar');
  });

  it('usa a data do aparelho, e não UTC', () => {
    expect(dataDeHoje(new Date('2026-09-17T23:30:00'))).toBe('2026-09-17');
    expect(dataDeHoje(new Date('2026-01-05T08:00:00'))).toBe('2026-01-05');
  });
});
