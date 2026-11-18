# GASTRA — Protótipo navegável

**Issues:** #47 (protótipo), #99 (Garçom e Metre), #100 (Gerente e Cliente)
**Pasta:** `docs/ux-ui/prototipo/` · **Imagens:** `docs/ux-ui/telas/`

O protótipo cobre as telas das quatro personas (Garçom, Metre, Gerente/Coordenador e Cliente) e o acesso. Ele roda
no navegador, com dados fictícios, e segue as regras já implementadas na API:
- listas fechadas (composição da mesa, restrições, motivos de cancelamento, flags dietéticas);
- validações e mensagens de erro, copiadas de `Gastra.Exceptions/MensagensErro.resx`;
- permissões de cada papel.

Assim ele serve a três coisas: avaliar as heurísticas de Nielsen (#101, #102), medir o RNF02 e servir de base para as
telas em Angular.

---

## 1. Como abrir

**Direto do disco:** abra `docs/ux-ui/prototipo/index.html` no Chrome ou no Edge. Não precisa de servidor nem de
instalação.

**Com servidor**, se o navegador bloquear algo em arquivo local:

```bash
python -m http.server 8090 --directory docs/ux-ui/prototipo
```

Depois acesse `http://localhost:8090`.

| Página | Persona | Formato | Casos de uso |
|---|---|---|---|
| `garcom.html` | Garçom | Celular | UC10–UC14, UC17, UC18, UC23 |
| `metre.html` | Metre | Tablet ou computador | UC15, UC21, UC22 |
| `gerente.html` | Gerente | Computador | UC04–UC09, UC16, UC17, UC24 |
| `gerente.html?papel=Coordenador` | Coordenador | Computador | UC05–UC09 |
| `cliente.html` | Cliente | Celular (QR code) | UC19, UC20 |
| `acesso.html` | Todos | Todos | UC01–UC03 |

### Instrumentos do protótipo (não fazem parte do produto)

- **Painel roxo (canto inferior direito):**
  - conta os toques;
  - no Garçom, mede o RNF02;
  - liga estados alternativos: serviço de análise fora do ar, período sem movimento, conta já fechada.
- **Caixa "Só no protótipo" no login:** preenche o e-mail de cada persona. Qualquer senha funciona, menos `errada`,
  e qualquer código de 6 dígitos funciona, menos `000000`.
- **`?captura` na URL:** esconde o painel. É usado nas imagens.

### Importar no Figma

O plugin gratuito **html.to.design** transforma as páginas em camadas editáveis do Figma:

1. No Figma, abra um arquivo de design, vá em *Plugins → Manage plugins* e instale **html.to.design**.
2. Instale também a extensão **html.to.design** no Chrome. É ela que captura páginas abertas no seu computador.
3. Abra a tela desejada no Chrome (por exemplo `garcom.html#/comanda/502`), clique na extensão e exporte.
4. No Figma, rode o plugin e importe o arquivo exportado. Cada tela vira um *frame*.

Para as telas de celular, use o modo de dispositivo do Chrome (F12 → ícone de celular) com largura de 390 a
430 px antes de capturar. Os passos podem mudar entre versões do plugin, e o plano gratuito pode limitar a quantidade
de importações. Importe primeiro as telas que vão para o documento.

---

## 2. Sistema visual

As cores saem do logo (`docs/assets/logo/logo_v1.png`). Os tokens estão em `prototipo/assets/prototipo.css` e devem
ir para `frontend/src/styles.scss` quando as telas forem para o Angular.

| Token | Cor | Uso | Contraste com branco |
|---|---|---|---|
| `--marinho` | `#2B3A4E` | Barras, títulos, seleção | 11:1 |
| `--laranja-forte` | `#B84A1B` | Botão principal, preço promocional | 5,2:1 |
| `--laranja` | `#DC6232` | Destaques, gráficos, foco (não usado em texto) | 3,5:1 |
| `--sucesso` / `--aviso` / `--erro` | `#1D7349` / `#7D5300` / `#B3261E` | Estados, com fundo claro próprio | ≥ 4,5:1 |

- **Contraste:** todo texto passa do mínimo de 4,5:1 da WCAG AA. O laranja do logo (`#DC6232`) tem só 3,5:1, por isso
  fica em elementos decorativos. Botões e textos usam a versão escura.
- **Alvo de toque:** 48 px nas telas de celular. O garçom usa o sistema em pé, andando e com pressa.
- **Fonte:** a do sistema operacional (`system-ui`), que carrega na hora e já é otimizada para cada aparelho.
- **Valores em reais:** números tabulares, para as colunas de preço ficarem alinhadas.

---

## 3. Telas

### 3.1 Garçom (celular)

| Tela | O que mostra | Requisitos |
|---|---|---|
| <img src="telas/garcom-mesas.png" alt="Mesas" width="220"> | **Mesas do turno.** Começa na praça do garçom (alocação confirmada). Mesa tracejada está livre; borda escura é dele, com os itens a entregar; cinza é de um colega. | RF01, RF07 |
| <img src="telas/garcom-abrir-mesa.png" alt="Abrir mesa" width="220"> | **Abrir mesa.** Cada número de pessoas já traz a composição sugerida (RN01). **Tocar no número abre a comanda**: é a confirmação da sugestão. O número igual à capacidade da mesa vem destacado. | RF01, RF02, RNF02 |
| <img src="telas/garcom-comanda.png" alt="Comanda" width="220"> | **Comanda.** Mostra, na ordem: restrições da mesa (RN04), itens a entregar (RF03), sugestões (RF09) e lançamento por categoria. O subtotal fica fixo no rodapé. Alergia registrada gera aviso nas sugestões. | RF03, RF09, RF14, RF24 |
| <img src="telas/garcom-lancar-item.png" alt="Lançar item" width="220"> | **Lançar item.** Quantidade começa em 1, e o botão mostra o valor antes de confirmar. Depois aparece "Desfazer". | RF03, RNF02 |
| <img src="telas/garcom-restricao.png" alt="Restrição" width="220"> | **Restrição alimentar.** Categoria da lista fechada e detalhe opcional, com aviso para não anotar dado pessoal. O detalhe é apagado no fechamento. | RF14, RN04, RN05 |
| <img src="telas/garcom-cancelar.png" alt="Cancelar" width="220"> | **Cancelar item.** O botão só habilita depois de escolher o motivo. | RF24, RN02 |
| <img src="telas/garcom-fechar-bloqueado.png" alt="Fechar bloqueado" width="220"> | **Fechar com pendências.** Explica o bloqueio, lista os itens e leva para resolver. | RN02 |
| <img src="telas/garcom-fechar.png" alt="Fechar" width="220"> | **Fechar conta.** Taxa de 10% ligada por padrão; retirar só a pedido do cliente. Confirmação antes de fechar. | RF04 |
| <img src="telas/garcom-pendencias.png" alt="A entregar" width="220"> | **A entregar.** Todos os itens pendentes das mesas do garçom, do mais antigo para o mais novo. | RF03 |
| <img src="telas/garcom-desempenho.png" alt="Desempenho" width="220"> | **Desempenho.** Posição, índice e as duas partes do cálculo. Não mostra os números dos colegas. | RF11 |
| <img src="telas/garcom-sugestoes-indisponiveis.png" alt="Sem sugestões" width="220"> | **Serviço de análise fora do ar.** As sugestões somem com um aviso, e o resto da comanda funciona (decisão D3). | RF09, D3 |

### 3.2 Metre (tablet ou computador)

| Tela | O que mostra | Requisitos |
|---|---|---|
| <img src="telas/metre-presenca.png" alt="Presença" width="420"> | **Quem está no turno.** Mostra presentes × vagas. Com mais presentes que vagas, explica o problema e bloqueia a geração. | RF06 |
| <img src="telas/metre-sugestao.png" alt="Sugestão" width="420"> | **Sugestão.** Uma coluna por praça: alto potencial, faturamento por turno e vagas ocupadas. Cada garçom traz os dois fatores da RN03. Um quadro explica como a sugestão foi feita. | RF06, RN03 |
| <img src="telas/metre-troca.png" alt="Troca" width="420"> | **Mudar ou trocar de praça.** Se a praça de destino está cheia, o Metre escolhe com quem trocar. Garçom movido ganha a etiqueta "Ajustado". Depende da #140. | RF07, UC22 |
| <img src="telas/metre-confirmada.png" alt="Confirmada" width="420"> | **Confirmada.** Aviso de que não aceita mais ajuste; quadro só para leitura. | UC21 |
| <img src="telas/metre-servico-indisponivel.png" alt="Serviço fora do ar" width="420"> | **Sem sugestão.** Todos ficam em "Sem praça", e o Metre distribui à mão. | D3 |
| <img src="telas/metre-salao.png" alt="Salão agora" width="420"> | **Salão agora.** Comandas abertas por praça, com pendências e restrições. Somente leitura. | RN04 |

### 3.3 Gerente e Coordenador (computador)

| Tela | O que mostra | Requisitos |
|---|---|---|
| <img src="telas/gerente-analises.png" alt="Análises" width="420"> | **Análises.** Mostra, em blocos:<br>• KPIs do período;<br>• faturamento por praça, com as de alto potencial destacadas;<br>• mapa de calor por hora;<br>• ranking do índice de desempenho;<br>• itens mais vendidos.<br>Na implementação, os números vêm de `/api/indicadores` (views de BI). | RF10, RF11 |
| <img src="telas/gerente-cardapio.png" alt="Cardápio" width="420"> | **Cardápio.** Busca, filtro por categoria, preço promocional e disponibilidade com "Desfazer". | RF19–RF21 |
| <img src="telas/gerente-preco.png" alt="Preço" width="420"> | **Atualizar preço.** Avisa que comandas abertas mantêm o preço do momento do pedido. | RF20 |
| <img src="telas/gerente-promocoes.png" alt="Promoções" width="420"> | **Promoções.** Situação (vigente, agendada, encerrada) e preço antes/depois de cada item. | RF22 |
| <img src="telas/gerente-nova-promocao.png" alt="Nova promoção" width="420"> | **Nova promoção.** Prévia dos preços enquanto preenche, com as validações da API. | RF22 |
| <img src="telas/gerente-salao.png" alt="Salão" width="420"> | **Praças e mesas.** Vagas de garçom por praça. A praça da mesa não muda depois de criada, e a tela avisa antes de salvar. | RF23 |
| <img src="telas/gerente-usuarios.png" alt="Usuários" width="420"> | **Usuários.** Papel, situação da verificação em duas etapas e inativação com confirmação. O próprio Gerente não consegue se inativar. | RF18, RF16 |
| <img src="telas/gerente-erro-formulario.png" alt="Erro de formulário" width="420"> | **Erro de formulário.** Lista no topo com as mensagens da API e campos marcados. | — |

O **Coordenador** usa a mesma área, com o menu reduzido a Cardápio e Promoções (`gerente.html?papel=Coordenador`).

### 3.4 Cliente (celular, pelo QR code)

| Tela | O que mostra | Requisitos |
|---|---|---|
| <img src="telas/cliente-inicio.png" alt="Início" width="220"> | **Início.** Aberto pelo QR code da mesa: cardápio ou conta. Deixa claro que pedidos são feitos com o garçom. | RF05, RF13 |
| <img src="telas/cliente-cardapio.png" alt="Cardápio digital" width="220"> | **Cardápio digital.** Só itens disponíveis, com preço promocional e flags. As fotos são provisórias (ver #141). | RF05 |
| <img src="telas/cliente-filtro.png" alt="Filtros" width="220"> | **Filtros dietéticos.** Combinam entre si (vegetariano **e** sem glúten), com aviso para confirmar alergia com o garçom. | RF05 |
| <img src="telas/cliente-conta.png" alt="Conta" width="220"> | **Conta em tempo real.** Situação de cada item; cancelado aparece riscado como "não cobrado". Informa que a taxa é opcional. Nenhum dado pessoal. | RF13, RN04 |

### 3.5 Acesso

| Tela | O que mostra | Requisitos |
|---|---|---|
| <img src="telas/acesso-login.png" alt="Login" width="220"> | **Login** com opção de mostrar a senha. | RF15 |
| <img src="telas/acesso-login-erro.png" alt="Erro de login" width="220"> | **Erro.** A mesma mensagem para e-mail inexistente, conta inativa e senha errada, para não revelar quem tem conta. | RF15 |
| <img src="telas/acesso-configurar-2fa.png" alt="Configurar autenticador" width="220"> | **Primeiro acesso** de Gerente ou Coordenador: passos, QR code, chave manual e aviso de que ela aparece só uma vez. | RF16, RN07 |
| <img src="telas/acesso-codigo.png" alt="Código" width="220"> | **Código de 6 dígitos.** Confirma sozinho ao completar e explica o que fazer se o código expirar. | RF16, RN07 |

---

## 4. RNF02: contagem de toques

> A interface de comanda do Garçom deve permitir abrir mesa e registrar um item de pedido em, no máximo, 5 toques no
> total (2 para abrir mesa, 3 para registrar item).

Medido no protótipo com o painel roxo (**Medir RNF02**):

| Etapa | Toques | Quais |
|---|---|---|
| Abrir mesa | **2** | (1) tocar na mesa livre; (2) tocar no número de pessoas, que confirma a composição sugerida |
| Lançar item | **3** | (1) categoria; (2) item; (3) "Lançar" |
| **Total** | **5 ≤ 5** | ✔ atende |

**Como o desenho chegou a 2 toques na abertura:** a API pede o número de pessoas para sugerir a composição (RN01).
Com um campo numérico e um botão "confirmar", seriam 3 toques ou mais. Cada botão de número já mostra a composição
("4 · Grupo pequeno"), então tocar no número é, ao mesmo tempo, informar as pessoas e confirmar a sugestão. Se a
sugestão estiver errada, o aviso "Mesa aberta · Grupo pequeno" tem o botão **Ajustar** (UC11).

**Variações, registradas para não esconder casos piores:**
- **Menos toques:** se a categoria do item já estiver selecionada (ex.: um segundo item de bebida), lançar leva
  2 toques.
- **Mais toques:**
  - cada unidade a mais na quantidade soma 1 toque;
  - uma mesa com 9 pessoas ou mais exige ajustar a quantidade.
- **Alternativa:** a sugestão de prato (UC18) lança em 2 toques (item → "Lançar").

---

## 5. Decisões de interface (para a banca)

1. **Mobile first para quem atende, tela larga para quem gerencia.** Garçom e cliente usam celular, em pé. Metre e
   Gerente decidem olhando várias praças e tabelas ao mesmo tempo.
2. **A comanda segue a ordem do trabalho.** Primeiro o que pode dar problema (restrição, itens a entregar), depois o
   que ajuda a vender (sugestões) e por último o lançamento. O subtotal e o "Fechar conta" ficam sempre à vista.
3. **Desfazer em vez de confirmar nas ações frequentes.** Entregar, lançar e mudar disponibilidade mostram "Desfazer"
   por 6 segundos. Confirmação em janela fica só para o que não volta atrás: fechar conta, confirmar alocação,
   inativar usuário, remover promoção. Assim a pressa não vira clique extra, e o erro tem saída.
4. **Explicar os números.** A sugestão de alocação e o índice de desempenho mostram de onde vêm. Isso responde ao
   problema levantado na pesquisa: a distribuição hoje é vista como injusta.
5. **Falha parcial não para o salão (D3).** Sem o serviço de análise, somem só as sugestões e a alocação automática,
   com um aviso. Comandas, fechamento e alocação manual continuam.
6. **Privacidade visível.** Os textos lembram que a restrição é apagada no fechamento e que o detalhe livre não deve ter
   dado pessoal. O cliente vê só itens e valores. O garçom não vê os números dos colegas.
7. **Mesmas mensagens da API.** Os erros do protótipo são os mesmos textos do backend, e a tela mostra o que o servidor
   vai dizer.

---

## 6. Achados para o backend

Desenhar com os contratos reais revelou lacunas, registradas como issues:

| Issue | Achado | Impacto |
|---|---|---|
| #140 | O ajuste da alocação move um garçom por vez e recusa praça cheia. No turno comum (presentes = vagas), **todas** as praças estão cheias e nenhum ajuste é possível. | RF07 não funciona no caso mais frequente |
| #141 | `ComandaResponse` não traz o garçom. | Sem ele, o mapa de mesas do garçom e o "Salão agora" do Metre não sabem quem atende cada mesa |
| #141 | Foto do item existe no domínio, mas não há como cadastrar. | RF05 pede fotos no cardápio digital |
| #141 | Não há redefinição de senha nem do autenticador. | Quem esquece a senha ou perde o celular fica sem acesso |

---

## 7. Próximos passos

- **Avaliação de Nielsen (#101, #102):** checklist das 10 heurísticas por tela, usando os estados alternativos do
  painel roxo. Como o protótipo das quatro personas foi feito pela mesma pessoa, o Daniel deve preencher pelo menos a
  avaliação de uma das áreas, para que a avaliação não seja só do próprio autor.
- **Angular:** levar os tokens para `styles.scss` e implementar as telas nesta ordem:
  1. Garçom (núcleo do RNF02);
  2. Metre;
  3. Cliente;
  4. Gerente.
- **Manual do usuário (#49):** feito, em [`docs/manual-usuario/`](../manual-usuario/GASTRA_Manual_do_Usuario.md). Usa as 33 imagens de `telas/` por caminho relativo, então regerar as imagens atualiza o manual junto.

### Regenerar as imagens

As imagens saem de cenas reproduzíveis (`prototipo/assets/cenas.js`). Depois de mudar uma tela, rode na raiz do
repositório:

```bash
python docs/ux-ui/gerar_telas.py
```

Para só uma persona: `python docs/ux-ui/gerar_telas.py garcom`. Precisa do Chrome ou do Edge instalado.
