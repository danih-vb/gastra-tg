import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GastraApiService } from '../../core/api/gastra-api.service';
import { CorDaMencao, IconeDaMencao, MencaoHonrosa } from '../../core/api/modelos';
import { mensagensDeErro } from '../erros';
import { Icone } from '../icone/icone';
import { CORES_DA_MENCAO, ICONES_DA_MENCAO } from './medalhas';

/**
 * O Gerente monta uma menção honrosa: ícone e cor de uma lista pronta, título e descrição livres. A prévia em cima
 * mostra como o garçom vai ver.
 */
@Component({
  selector: 'app-nova-mencao',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, FormsModule, Icone],
  template: `
    <form class="formulario" novalidate (ngSubmit)="salvar()">
      @if (erros().length) {
        <div class="alerta erro" role="alert">
          <app-icone nome="alerta" />
          <div>
            @for (erro of erros(); track erro) {
              <div>{{ erro }}</div>
            }
          </div>
        </div>
      }

      <div class="previa" aria-hidden="true">
        <span class="selo" [style.background]="CORES[cor()].valor"><app-icone [nome]="icone()" /></span>
        <div>
          <strong>{{ titulo.trim() || 'Título' }}</strong>
          @if (descricao.trim()) {
            <span class="pequeno">{{ descricao.trim() }}</span>
          }
        </div>
      </div>

      <fieldset>
        <legend>Ícone</legend>
        <div class="opcoes" role="radiogroup" aria-label="Ícone">
          @for (opcao of ICONES; track opcao) {
            <button
              type="button"
              class="opcao"
              role="radio"
              [attr.aria-checked]="icone() === opcao"
              [attr.aria-label]="NOMES_DOS_ICONES[opcao]"
              [title]="NOMES_DOS_ICONES[opcao]"
              (click)="icone.set(opcao)"
            >
              <app-icone [nome]="opcao" />
            </button>
          }
        </div>
      </fieldset>

      <fieldset>
        <legend>Cor</legend>
        <div class="opcoes" role="radiogroup" aria-label="Cor">
          @for (opcao of CORES_LISTA; track opcao) {
            <button
              type="button"
              class="opcao cor"
              role="radio"
              [style.background]="CORES[opcao].valor"
              [attr.aria-checked]="cor() === opcao"
              [attr.aria-label]="CORES[opcao].nome"
              [title]="CORES[opcao].nome"
              (click)="cor.set(opcao)"
            >
              @if (cor() === opcao) {
                <app-icone nome="check" />
              }
            </button>
          }
        </div>
      </fieldset>

      <div class="campo">
        <label for="mencao-titulo">Título</label>
        <input class="entrada" id="mencao-titulo" name="titulo" maxlength="60" autocomplete="off" [(ngModel)]="titulo" />
      </div>
      <div class="campo">
        <label for="mencao-descricao">Descrição</label>
        <textarea class="entrada" id="mencao-descricao" name="descricao" rows="2" maxlength="200" [(ngModel)]="descricao"></textarea>
      </div>
      <div class="campo">
        <label for="mencao-mes">Mês</label>
        <select class="entrada" id="mencao-mes" name="mes" [(ngModel)]="mes">
          @for (opcao of meses(); track opcao) {
            <option [value]="opcao">{{ opcao | date: 'MMMM yyyy' }}</option>
          }
        </select>
      </div>

      <div class="rodape-folha">
        <button class="botao secundario" type="button" (click)="cancelar.emit()">Cancelar</button>
        <button class="botao" type="submit" [disabled]="salvando() || !titulo.trim()">
          <app-icone nome="trofeu" /> Dar menção
        </button>
      </div>
    </form>
  `,
  styles: `
    .formulario { display: flex; flex-direction: column; gap: 14px; }
    fieldset { margin: 0; padding: 0; border: 0; }
    legend { margin-bottom: 6px; font-weight: 600; font-size: 0.875rem; }
    .previa {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px;
      border: 1px solid var(--borda);
      border-radius: var(--raio);
      background: var(--fundo);
    }
    .previa div { display: grid; gap: 2px; min-width: 0; }
    .previa strong { overflow-wrap: anywhere; }
    .selo {
      display: grid;
      place-items: center;
      flex: none;
      width: 44px;
      height: 44px;
      border-radius: 50%;
      color: #fff;
      font-size: 1.375rem;
    }
    .opcoes { display: flex; flex-wrap: wrap; gap: 8px; }
    .opcao {
      display: grid;
      place-items: center;
      width: var(--toque-minimo);
      height: var(--toque-minimo);
      border: 1px solid var(--borda-forte);
      border-radius: 50%;
      background: var(--superficie);
      color: var(--texto);
      font-size: 1.25rem;
      cursor: pointer;
    }
    .opcao[aria-checked='true'] { border: 2px solid var(--laranja-forte); color: var(--laranja-forte); background: var(--laranja-suave); }
    .opcao.cor { border: 2px solid transparent; color: #fff; }
    .opcao.cor[aria-checked='true'] { outline: 2px solid var(--texto); outline-offset: 2px; }
  `,
})
export class NovaMencao {
  private readonly api = inject(GastraApiService);

  readonly garcomId = input.required<number>();
  /** Meses que podem ser escolhidos, do atual para trás (primeiro dia de cada um). */
  readonly meses = input.required<string[]>();
  readonly salva = output<MencaoHonrosa>();
  readonly cancelar = output<void>();

  protected readonly ICONES = Object.keys(ICONES_DA_MENCAO) as IconeDaMencao[];
  protected readonly NOMES_DOS_ICONES = ICONES_DA_MENCAO;
  protected readonly CORES = CORES_DA_MENCAO;
  protected readonly CORES_LISTA = Object.keys(CORES_DA_MENCAO) as CorDaMencao[];

  protected readonly icone = signal<IconeDaMencao>('trofeu');
  protected readonly cor = signal<CorDaMencao>('laranja');
  protected readonly salvando = signal(false);
  protected readonly erros = signal<string[]>([]);
  protected titulo = '';
  protected descricao = '';
  protected mes = '';

  protected salvar(): void {
    if (!this.titulo.trim()) return;
    this.salvando.set(true);
    this.erros.set([]);
    this.api
      .concederMencao({
        garcomId: this.garcomId(),
        titulo: this.titulo.trim(),
        descricao: this.descricao.trim() || null,
        icone: this.icone(),
        cor: this.cor(),
        mes: this.mes || this.meses()[0],
      })
      .subscribe({
        next: (mencao) => {
          this.salvando.set(false);
          this.salva.emit(mencao);
        },
        error: (erro: unknown) => {
          this.salvando.set(false);
          this.erros.set(mensagensDeErro(erro));
        },
      });
  }
}
