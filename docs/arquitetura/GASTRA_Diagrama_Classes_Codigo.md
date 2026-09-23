# GASTRA — Diagrama de Classes gerado do código

> **Arquivo gerado automaticamente** por `backend/tools/GeradorDiagramaClasses` a partir dos assemblies
> compilados. Não edite à mão: rode a ferramenta de novo (ver o README dela). A especificação original,
> de antes da implementação, continua em `GASTRA_Diagrama_Classes_Camadas.md`.

Diagramas: [domínio](../diagramas/classe/GASTRA_Classe_Dominio.png) · [contratos e implementações](../diagramas/classe/GASTRA_Classe_Contratos.png).

Resumo: 14 entidades, 11 enumerações, 13 regras e objetos de valor, 16 interfaces e 16 implementações.

## 1. Entidades (`Gastra.Domain.Entidades`)

### Alocacao

| Atributos | Métodos |
|---|---|
| `+Data: DateOnly`<br>`+Periodo: PeriodoAlocacao`<br>`+GarcomId: int`<br>`+PracaId: int`<br>`+Confirmada: bool` | `+Ajustar(novaPracaId: int): void`<br>`+Confirmar(): void` |

### AvaliacaoAtendimento

| Atributos | Métodos |
|---|---|
| `+NotaMinima: int = 1 «static»`<br>`+NotaMaxima: int = 5 «static»`<br>`+TamanhoMaximoDoComentario: int = 280 «static»`<br>`+ComandaId: int`<br>`+Nota: int`<br>`+Comentario: string?`<br>`+DataHoraEnvio: DateTime` | — |

### Comanda

| Atributos | Métodos |
|---|---|
| `+PercentualTaxaServico: decimal = 0.10 «static»`<br>`+JanelaDeAvaliacao: TimeSpan «static»`<br>`+MesaId: int`<br>`+GarcomId: int`<br>`+DataHoraAbertura: DateTime`<br>`+DataHoraFechamento: DateTime?`<br>`+Status: StatusComanda`<br>`+QuantidadePessoas: int`<br>`+Composicao: ComposicaoMesa`<br>`+TaxaServicoRemovida: bool`<br>`+ComposicaoAjustadaManualmente: bool`<br>`+CodigoAcessoCliente: string`<br>`+Itens: IReadOnlyCollection<ItemDoPedido>`<br>`+Restricoes: IReadOnlyCollection<RestricaoAlimentar>`<br>`+Avaliacao: AvaliacaoAtendimento?` | `+AdicionarItem(item: ItemDoCardapio, quantidade: int, precoPromocional: decimal?): ItemDoPedido`<br>`+Avaliar(nota: int, comentario: string?, agoraUtc: DateTime): AvaliacaoAtendimento`<br>`+CalcularSubtotal(): decimal`<br>`+CalcularTaxaServico(): decimal`<br>`+CalcularTotal(): decimal`<br>`+ConfirmarComposicao(quantidadePessoas: int, composicao: ComposicaoMesa): void`<br>`+Fechar(): void`<br>`+PodeSerAvaliada(agoraUtc: DateTime): bool`<br>`+PossuiPendencias(): bool`<br>`+RegistrarRestricao(categoria: CategoriaRestricao, observacaoLivre: string?): RestricaoAlimentar`<br>`+RemoverTaxaServico(): void`<br>`+RestricoesVisiveisPara(papel: PapelUsuario): IReadOnlyCollection<RestricaoAlimentar>`<br>`+SugerirComposicao(quantidadePessoas: int): ComposicaoMesa` |

### EntidadeBase

| Atributos | Métodos |
|---|---|
| `+Id: int` | — |

### ItemCardapioFlag

| Atributos | Métodos |
|---|---|
| `+Flag: FlagDietetica` | — |

### ItemDoCardapio

| Atributos | Métodos |
|---|---|
| `+Nome: string`<br>`+Categoria: CategoriaItemCardapio`<br>`+Preco: decimal`<br>`+Descricao: string`<br>`+Disponivel: bool`<br>`+Imagem: string?`<br>`+FlagsDieteticas: IReadOnlyCollection<FlagDietetica>` | `+AtendeRestricao(restricao: CategoriaRestricao): bool`<br>`+AtualizarPreco(novoPreco: decimal): void`<br>`+DefinirImagem(imagem: string?): void`<br>`+MarcarDisponibilidade(disponivel: bool): void` |

### ItemDoPedido

| Atributos | Métodos |
|---|---|
| `+ComandaId: int`<br>`+ItemDoCardapioId: int`<br>`+Quantidade: int`<br>`+PrecoUnitarioNoMomento: decimal`<br>`+DataHoraRegistro: DateTime`<br>`+Status: StatusItemPedido`<br>`+MotivoCancelamento: MotivoCancelamento?` | `+CalcularValor(): decimal`<br>`+Cancelar(motivo: MotivoCancelamento): void`<br>`+MarcarEntregue(): void` |

### Mesa

| Atributos | Métodos |
|---|---|
| `+Numero: string`<br>`+Capacidade: int`<br>`+PracaId: int` | `+Atualizar(numero: string, capacidade: int): void` |

### Praca

| Atributos | Métodos |
|---|---|
| `+Codigo: string`<br>`+QuantidadeGarcons: int` | `+Atualizar(codigo: string, quantidadeGarcons: int): void` |

### Promocao

| Atributos | Métodos |
|---|---|
| `+PrecoMinimo: decimal = 0.01 «static»`<br>`+TamanhoMaximoDescricao: int = 200 «static»`<br>`+Descricao: string`<br>`+TipoDesconto: TipoDesconto`<br>`+ValorDesconto: decimal`<br>`+DataInicio: DateOnly`<br>`+DataFim: DateOnly`<br>`+Ativa: bool`<br>`+ItemCardapioIds: IReadOnlyCollection<int>` | `+AplicarDesconto(preco: decimal): decimal`<br>`+Desativar(): void`<br>`+IncluiItem(itemCardapioId: int): bool`<br>`+PrecoPromocional(item: ItemDoCardapio, promocoes: IEnumerable<Promocao>, data: DateOnly): decimal? «static»`<br>`+VigenteEm(data: DateOnly): bool` |

### PromocaoItem

| Atributos | Métodos |
|---|---|
| `+ItemCardapioId: int` | — |

### RegistroAuditoria

| Atributos | Métodos |
|---|---|
| `+MesesDeRetencao: int = 6 «static»`<br>`+DataHoraUtc: DateTime`<br>`+Evento: string`<br>`+Resultado: ResultadoAuditoria`<br>`+UsuarioId: int?`<br>`+Papel: PapelUsuario?`<br>`+Entidade: string?`<br>`+IdEntidade: int?`<br>`+Detalhes: string?`<br>`+Ip: string?`<br>`+IdCorrelacao: string?` | `+LimiteDeRetencao(agoraUtc: DateTime): DateTime «static»` |

### RestricaoAlimentar

| Atributos | Métodos |
|---|---|
| `+ComandaId: int`<br>`+Categoria: CategoriaRestricao`<br>`+VerificavelPeloCardapio: bool`<br>`+ObservacaoLivre: string?` | — |

### Usuario

| Atributos | Métodos |
|---|---|
| `+Nome: string`<br>`+Email: string`<br>`+SenhaHash: string`<br>`+Papel: PapelUsuario`<br>`+Ativo: bool`<br>`+SegredoTotp: string?`<br>`+ChaveSessao: Guid`<br>`+SegundoFatorConfigurado: bool` | `+AtualizarDados(nome: string, email: string, papel: PapelUsuario): void`<br>`+DefinirSegredoTotp(segredoProtegido: string): void`<br>`+EncerrarSessoes(): void`<br>`+ExigeSegundoFator(): bool`<br>`+Inativar(): void`<br>`+NormalizarEmail(email: string): string «static»`<br>`+Reativar(): void`<br>`+RedefinirSenha(senhaHash: string): void`<br>`+ReiniciarSegundoFator(): void` |

## 2. Enumerações (`Gastra.Domain.Enums`)

| Enumeração | Valores |
|---|---|
| `CategoriaItemCardapio` | Entrada, PratoPrincipal, Sobremesa, Bebida |
| `CategoriaRestricao` | Vegano, Vegetariano, SemGluten, SemLactose, Alergia, Outro |
| `ComposicaoMesa` | Solo, Casal, GrupoPequeno, Familia, GrupoGrande |
| `FlagDietetica` | Vegano, Vegetariano, SemGluten, SemLactose, OpcaoInfantil |
| `MotivoCancelamento` | ErroDeLancamento, ClienteDesistiu, ItemEmFalta |
| `PapelUsuario` | Gerente, Coordenador, Metre, Garcom |
| `PeriodoAlocacao` | Almoco, Jantar |
| `ResultadoAuditoria` | Sucesso, Falha |
| `StatusComanda` | Aberta, Fechada |
| `StatusItemPedido` | Pendente, Entregue, Cancelado |
| `TipoDesconto` | Percentual, ValorFixo |

## 3. Regras de negócio e objetos de valor

### RegraDeDistribuicao

| Atributos | Métodos |
|---|---|
| `+PesoDesequilibrio: double = 0.6 «static»`<br>`+DiasDaJanelaDeFaturamento: int = 30 «static»`<br>`+PesoEspera: double «static»` | `+PracasDeAltoPotencial(faturamentoMedioPorPraca: IReadOnlyDictionary<int, decimal>): IReadOnlySet<int> «static»`<br>`+TurnosDesdePracaDeAltoPotencial(pracasDosTurnosAnteriores: IEnumerable<int>, altoPotencial: IReadOnlySet<int>): int «static»` |

### DesempenhoNoPeriodo

| Atributos | Métodos |
|---|---|
| `+GarcomId: int`<br>`+Faturamento: decimal`<br>`+Turnos: int`<br>`+MesasAtendidas: int` | — |

### IndicadorGarcom

| Atributos | Métodos |
|---|---|
| `+GarcomId: int`<br>`+Faturamento: decimal`<br>`+Comandas: int`<br>`+Turnos: int`<br>`+MesasAtendidas: int`<br>`+SomaMinutosAtendimento: int` | — |

### IndicadorItemCardapio

| Atributos | Métodos |
|---|---|
| `+ItemCardapioId: int`<br>`+Categoria: string`<br>`+Quantidade: decimal`<br>`+Faturamento: decimal` | — |

### IndicadorPraca

| Atributos | Métodos |
|---|---|
| `+PracaId: int`<br>`+Faturamento: decimal`<br>`+Comandas: int`<br>`+Turnos: int` | — |

### IndicadorPracaNoTempo

| Atributos | Métodos |
|---|---|
| `+PracaId: int`<br>`+Fatia: int`<br>`+Faturamento: decimal`<br>`+Comandas: int` | — |

### IndiceDeDesempenho

| Atributos | Métodos |
|---|---|
| `+PesoFaturamento: decimal = 0.5 «static»` | `+Calcular(desempenhos: IEnumerable<DesempenhoNoPeriodo>): IReadOnlyList<PosicaoNoRanking> «static»` |

### LinhaAvaliacao

| Atributos | Métodos |
|---|---|
| `+Nota: int`<br>`+Quantidade: int` | — |

### PosicaoNoRanking

| Atributos | Métodos |
|---|---|
| `+GarcomId: int`<br>`+Posicao: int`<br>`+Indice: decimal`<br>`+FaturamentoPorTurno: decimal`<br>`+MesasPorTurno: decimal`<br>`+Turnos: int` | — |

### DesignacaoSugerida

| Atributos | Métodos |
|---|---|
| `+GarcomId: int`<br>`+PracaId: int` | — |

### GarcomParaAlocacao

| Atributos | Métodos |
|---|---|
| `+GarcomId: int`<br>`+FaturamentoPorTurno: decimal`<br>`+TurnosDesdePracaDeAltoPotencial: int` | — |

### PracaParaAlocacao

| Atributos | Métodos |
|---|---|
| `+PracaId: int`<br>`+Vagas: int`<br>`+FaturamentoMedioHistorico: decimal` | — |

### ServicoAnaliticoIndisponivelException

| Atributos | Métodos |
|---|---|
| — | — |

## 4. Contratos e implementações

| Interface (Domain) | Implementação (Infrastructure) | Métodos |
|---|---|---|
| `IRepositorioAlocacao` | `RepositorioAlocacao` | `+Adicionar(alocacao: Alocacao): Task`<br>`+ListarDoTurno(data: DateOnly, periodo: PeriodoAlocacao): Task<List<Alocacao>>`<br>`+ListarPracasConfirmadasAntesDe(garcomIds: IEnumerable<int>, data: DateOnly): Task<Dictionary<int, List<int>>>`<br>`+Remover(alocacoes: IEnumerable<Alocacao>): void` |
| `IRepositorioAuditoria` | `RepositorioAuditoria` | `+Adicionar(registro: RegistroAuditoria): Task`<br>`+EliminarAnterioresA(limiteUtc: DateTime): Task<int>` |
| `IRepositorioComanda` | `RepositorioComanda` | `+Adicionar(comanda: Comanda): Task`<br>`+ExisteAbertaNaMesa(mesaId: int): Task<bool>`<br>`+ListarAbertas(): Task<List<Comanda>>`<br>`+ObterParaAvaliacao(codigoAcesso: string): Task<Comanda?>`<br>`+ObterPorCodigoAcesso(codigoAcesso: string): Task<Comanda?>`<br>`+ObterPorId(id: int): Task<Comanda?>` |
| `IRepositorioIndicadores` | `RepositorioIndicadores` | `+ObterDistribuicaoDeAvaliacoes(inicio: DateOnly, fim: DateOnly): Task<List<LinhaAvaliacao>>`<br>`+ObterFaturamentoMedioPorPraca(): Task<Dictionary<int, decimal>>`<br>`+ObterFaturamentoMedioPorTurnoDoGarcom(inicio: DateOnly, fim: DateOnly): Task<Dictionary<int, decimal>>`<br>`+ObterFaturamentoPorPracaEDiaDaSemana(inicio: DateOnly, fim: DateOnly): Task<List<IndicadorPracaNoTempo>>`<br>`+ObterFaturamentoPorPracaEHora(inicio: DateOnly, fim: DateOnly): Task<List<IndicadorPracaNoTempo>>`<br>`+ObterIndicadoresPorGarcom(inicio: DateOnly, fim: DateOnly): Task<List<IndicadorGarcom>>`<br>`+ObterIndicadoresPorItem(inicio: DateOnly, fim: DateOnly): Task<List<IndicadorItemCardapio>>`<br>`+ObterIndicadoresPorPraca(inicio: DateOnly, fim: DateOnly): Task<List<IndicadorPraca>>` |
| `IRepositorioItemCardapio` | `RepositorioItemCardapio` | `+Adicionar(item: ItemDoCardapio): Task`<br>`+BuscarPorNome(nome: string): Task<List<ItemDoCardapio>>`<br>`+ListarDisponiveis(): Task<List<ItemDoCardapio>>`<br>`+ListarPorIds(ids: IEnumerable<int>): Task<List<ItemDoCardapio>>`<br>`+ListarTodos(): Task<List<ItemDoCardapio>>`<br>`+ObterPorId(id: int): Task<ItemDoCardapio?>` |
| `IRepositorioMesa` | `RepositorioMesa` | `+Adicionar(mesa: Mesa): Task`<br>`+ListarTodas(): Task<List<Mesa>>`<br>`+ObterPorId(id: int): Task<Mesa?>`<br>`+ObterPorNumero(numero: string): Task<Mesa?>` |
| `IRepositorioPraca` | `RepositorioPraca` | `+Adicionar(praca: Praca): Task`<br>`+ListarTodas(): Task<List<Praca>>`<br>`+ObterPorCodigo(codigo: string): Task<Praca?>`<br>`+ObterPorId(id: int): Task<Praca?>` |
| `IRepositorioPromocao` | `RepositorioPromocao` | `+Adicionar(promocao: Promocao): Task`<br>`+Listar(somenteAtivas: bool): Task<List<Promocao>>`<br>`+ListarVigentes(data: DateOnly): Task<List<Promocao>>`<br>`+ObterPorId(id: int): Task<Promocao?>` |
| `IRepositorioUsuario` | `RepositorioUsuario` | `+Adicionar(usuario: Usuario): Task`<br>`+ExisteAlgum(): Task<bool>`<br>`+ListarPorIds(ids: IEnumerable<int>): Task<List<Usuario>>`<br>`+ListarTodos(): Task<List<Usuario>>`<br>`+ObterPorEmail(email: string): Task<Usuario?>`<br>`+ObterPorId(id: int): Task<Usuario?>` |
| `IUnitOfWork` | `UnitOfWork` | `+Commit(): Task` |
| `IContextoRequisicao` | `ContextoRequisicao` | `+ObterIdCorrelacao(): string?`<br>`+ObterIp(): string?` |
| `ICriptografiaSenha` | `CriptografiaSenha` | `+GerarHash(senha: string): string`<br>`+Verificar(senha: string, hash: string): bool` |
| `IGeradorToken` | `GeradorToken` | `+GerarTokenAcesso(usuario: Usuario): string`<br>`+GerarTokenSegundoFator(usuario: Usuario): string`<br>`+ValidarTokenSegundoFator(token: string): Task<int?>` |
| `IUsuarioLogado` | `UsuarioLogado` | `+Obter(): Task<Usuario>`<br>`+ObterIdentificacao(): (int, PapelUsuario)?`<br>`+ObterPapel(): PapelUsuario` |
| `IValidadorTotp` | `ValidadorTotp` | `+GerarSegredo(email: string): SegredoTotpGerado`<br>`+Validar(segredoProtegido: string, codigo: string): bool` |
| `IServicoAnalitico` | `ServicoAnaliticoHttp` | `+SugerirAlocacao(garcons: IReadOnlyCollection<GarcomParaAlocacao>, pracas: IReadOnlyCollection<PracaParaAlocacao>, pesoDesequilibrio: double, pesoEspera: double, cancellationToken: CancellationToken): Task<IReadOnlyList<DesignacaoSugerida>>`<br>`+SugerirCombinacoes(itensPedidos: IReadOnlyCollection<int>, itensPermitidos: IReadOnlyCollection<int>, limite: int, cancellationToken: CancellationToken): Task<IReadOnlyList<int>>` |
