import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { LOCALE_ID, Provider } from '@angular/core';

/** Valores em reais e datas no formato daqui: "R$ 119,00" e "17/09/2026". Usado pelo app e pelos testes. */
export function provedoresDeLocalizacao(): Provider[] {
  registerLocaleData(localePt);
  return [{ provide: LOCALE_ID, useValue: 'pt-BR' }];
}
