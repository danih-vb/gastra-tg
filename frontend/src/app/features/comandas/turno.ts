import { PeriodoAlocacao } from '../../core/api/modelos';

/**
 * Turno atual. O almoço vai até as 17h, o mesmo corte usado nas views de BI do banco
 * (backend/.../Migrations, vw_faturamento_praca_turno).
 */
export function periodoDoTurno(agora = new Date()): PeriodoAlocacao {
  return agora.getHours() < 17 ? 'Almoco' : 'Jantar';
}

/** Data de hoje no formato aaaa-mm-dd, no fuso do aparelho (é o salão de hoje, não UTC). */
export function dataDeHoje(agora = new Date()): string {
  const doisDigitos = (valor: number) => String(valor).padStart(2, '0');
  return `${agora.getFullYear()}-${doisDigitos(agora.getMonth() + 1)}-${doisDigitos(agora.getDate())}`;
}
