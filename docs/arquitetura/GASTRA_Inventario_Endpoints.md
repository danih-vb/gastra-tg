# Inventário de endpoints — GASTRA

Gerado pelo teste `InventarioDeEndpointsTests` a partir do documento OpenAPI da API (o mesmo de
`/openapi/v1.json`) e dos atributos de acesso dos controllers. **Não edite à mão:** o teste falha se este
arquivo ficar diferente do código. Para regerar:

```bash
GASTRA_ATUALIZAR_INVENTARIO=1 dotnet test --filter InventarioDeEndpoints
```

68 endpoints. A coluna **Quem** é conferida em execução por `MatrizDePapeisTests`: sem token,
401; papel fora da lista, 403.

| Método | Rota | O que faz | Rastreio | Quem |
|---|---|---|---|---|
| GET | `/api/alocacoes/garcons` | UC15 — Garçons ativos que o Metre pode marcar como presentes no turno. | UC15 | Gerente, Metre |
| POST | `/api/alocacoes/sugestao` | UC15 — Gerar a sugestão do turno a partir dos garçons presentes. | UC15 | Metre |
| GET | `/api/alocacoes/{data}/{periodo}` | Quem está em qual praça no turno. | RF06, UC15, UC21 | Gerente, Metre, Garçom |
| POST | `/api/alocacoes/{data}/{periodo}/confirmacao` | UC21 — Confirmar o turno. Depois disso, a alocação não muda mais. | UC21 | Metre |
| PUT | `/api/alocacoes/{data}/{periodo}/garcons/{garcomId}` | UC22 — Colocar um garçom numa praça: ajuste da sugestão ou alocação manual. Com a praça cheia, informe `trocarComGarcomId` para trocar os dois de lugar (#140). | UC22 | Metre |
| POST | `/api/autenticacao/login` | UC01 — Autenticar-se com e-mail e senha. | UC01 | público |
| POST | `/api/autenticacao/logoff` | UC03 — Encerrar sessão: invalida os tokens de acesso do usuário. | UC03 | qualquer usuário logado |
| POST | `/api/autenticacao/segundo-fator/configurar` | Vincula o app autenticador no primeiro acesso de Gerente/Coordenador (RN07). | RN07 | público |
| POST | `/api/autenticacao/segundo-fator/confirmar` | UC02 — Confirmar segundo fator com o código do app autenticador. | UC02 | público |
| GET | `/api/cardapio` | Todos os itens, disponíveis ou não: gestão do cardápio e lançamento de pedidos. | RF19, UC05, UC12 | Gerente, Coordenador, Metre, Garçom |
| POST | `/api/cardapio` | UC05 — Cadastrar item do cardápio. | UC05 | Gerente, Coordenador |
| GET | `/api/cardapio/digital` | UC19 — Cardápio digital: público (o cliente não faz login), só itens disponíveis. | UC19 | público |
| POST | `/api/cardapio/fotos` | RF05 — Importar a foto do computador. Devolve o endereço, que o item grava como qualquer outra imagem. | RF05 | Gerente, Coordenador |
| GET | `/api/cardapio/fotos/{nome}` | Foto importada (pública): é o que o cardápio digital mostra ao cliente, que não faz login. As fotos dos funcionários não passam por aqui: ficam noutra pasta e só saem por /api/usuarios/fotos, com login. | RF05, UC19 | público |
| GET | `/api/cardapio/{id}` | Consultar um item do cardápio (público). | RF05, UC19 | público |
| PATCH | `/api/cardapio/{id}/disponibilidade` | UC07 — Marcar item disponível/indisponível. | UC07 | Gerente, Coordenador |
| PATCH | `/api/cardapio/{id}/imagem` | RF05 — Definir ou tirar a foto do item mostrada no cardápio digital. | RF05 | Gerente, Coordenador |
| PATCH | `/api/cardapio/{id}/preco` | UC06 — Atualizar preço de item. | UC06 | Gerente, Coordenador |
| GET | `/api/comandas` | Comandas abertas agora: é o painel do salão. | RF01, UC10 | Gerente, Coordenador, Metre, Garçom |
| POST | `/api/comandas` | UC10 — Abrir comanda numa mesa, informando o número de pessoas. | UC10 | Garçom |
| GET | `/api/comandas/consulta/{codigoAcesso}` | UC20 — Consulta do cliente pelo QR code da mesa, sem login (RF13). O código de acesso funciona como senha da comanda, por isso vai na rota e nunca em log (política de log, seção 4.4). | RF13, UC20 | público |
| POST | `/api/comandas/consulta/{codigoAcesso}/avaliacao` | UC25 — Avaliação do atendimento pelo cliente, sem login (RF25). Além do chamado, é a única escrita que o código de acesso permite, e só depois de a conta fechar, uma vez e dentro da janela (RN08). | RF25, RN08, UC25 | público |
| POST | `/api/comandas/consulta/{codigoAcesso}/chamado` | O cliente chama o garçom pelo celular, sem login, com o código de acesso da própria conta. | UC20 | público |
| GET | `/api/comandas/{id}` | Consultar uma comanda pelo salão. | RF01, UC10 | Gerente, Coordenador, Metre, Garçom |
| DELETE | `/api/comandas/{id}/chamado` | O garçom foi até a mesa que chamou. | UC20 | Garçom |
| PATCH | `/api/comandas/{id}/composicao` | UC11 — Confirmar ou ajustar a composição sugerida pelo sistema. | UC11 | Garçom |
| POST | `/api/comandas/{id}/cortesias` | Cortesia: o Metre ou o Gerente lança um item por R$ 0,00, com motivo. | RF26, UC26 | Gerente, Metre |
| POST | `/api/comandas/{id}/fechamento` | UC14 — Fechar a comanda e calcular o total (RN02, RF04). | RF04, RN02, UC14 | Garçom |
| POST | `/api/comandas/{id}/itens` | UC12 — Registrar item do pedido na comanda. Pelo Metre ou pelo Gerente, o item espera o garçom. | UC12 | Gerente, Metre, Garçom |
| POST | `/api/comandas/{id}/itens/{itemId}/confirmacao` | O garçom confirma ou recusa um item que o Metre ou o Gerente lançou na mesa. | RF27, UC27 | Garçom |
| PATCH | `/api/comandas/{id}/itens/{itemId}/preco` | O Gerente muda o preço de um item já lançado, com motivo; o preço original fica guardado. | RF26, UC26 | Gerente |
| PATCH | `/api/comandas/{id}/itens/{itemId}/situacao` | UC23 — Marcar item como entregue ou cancelá-lo com justificativa (RN02). | RN02, UC23 | Garçom |
| POST | `/api/comandas/{id}/restricoes` | UC13 — Registrar restrição alimentar informada pelo cliente (RF14). | RF14, UC13 | Garçom |
| GET | `/api/comandas/{id}/sugestoes` | UC18 — Sugestões de itens para oferecer na mesa (RF09). Se a camada analítica estiver fora do ar, responde 200 com a lista vazia e `servicoDisponivel = false`: o atendimento não para (D3). | RF09, UC18 | Garçom |
| DELETE | `/api/comandas/{id}/taxa-servico` | RF04 — Remover a taxa de serviço a pedido do cliente. | RF04 | Garçom |
| POST | `/api/comandas/{id}/taxa-servico` | RF04 — Recolocar a taxa de serviço, se o cliente mudar de ideia antes do fechamento. | RF04 | Garçom |
| GET | `/api/destaques` |  | RF28, UC28 | Gerente, Coordenador, Metre, Garçom |
| POST | `/api/destaques` |  | RF28, UC28 | Gerente, Coordenador |
| DELETE | `/api/destaques/{id}` |  | RF28, UC28 | Gerente, Coordenador |
| GET | `/api/indicadores/avaliacoes` | Média e distribuição das notas que os clientes deixaram no período (RF25). Só agregado: comentário e garçom não saem daqui. | RF25 | Gerente |
| GET | `/api/indicadores/cardapio` | Faturamento por item e por categoria do cardápio. | RF10, UC16 | Gerente |
| GET | `/api/indicadores/desempenho` | UC17 — ranking pelo índice de desempenho. O Gerente vê todos; o Garçom vê só a própria posição. | UC17 | Gerente, Garçom |
| GET | `/api/indicadores/garcons` | Faturamento total e por turno, comandas, mesas atendidas, ticket médio e tempo de atendimento por garçom. | RF10, UC16 | Gerente |
| GET | `/api/indicadores/horarios` | Faturamento por praça por hora do dia e por dia da semana. | RF10, UC16 | Gerente |
| GET | `/api/indicadores/medalhas` | Medalhas do mês e histórico de conquistas: o garçom vê as próprias; o Gerente, as de qualquer garçom. | RF29, UC29 | Gerente, Garçom |
| GET | `/api/indicadores/perfis-consumo` | Perfis de consumo que a clusterização encontrou no último ano (RF09, RF10): quantas mesas de cada tipo e o que caracteriza cada uma. Com o serviço analítico fora do ar, responde 200 sem perfis (D3). | RF09, RF10 | Gerente |
| GET | `/api/indicadores/pracas` | Faturamento, ticket médio e faturamento médio histórico por praça. | RF10, UC16 | Gerente |
| POST | `/api/mencoes` |  | RF29, UC29 | Gerente |
| DELETE | `/api/mencoes/{id}` |  | RF29, UC29 | Gerente |
| GET | `/api/mesas` | Mesas do salão: é a lista que o garçom usa para abrir a comanda (UC10). | RF23, RF30, UC10, UC24, UC30 | Gerente, Metre, Garçom |
| POST | `/api/mesas` |  | RF23, UC24 | Gerente |
| PUT | `/api/mesas/{id}` | Edita número e capacidade. A praça não muda: o vínculo é fixo (REL01). | RF23, UC24 | Gerente |
| GET | `/api/pracas` | Praças do salão, com quantos garçons cada uma comporta (entrada da alocação, RN03). | RN03 | Gerente, Metre, Garçom |
| POST | `/api/pracas` |  | RF23, UC24 | Gerente |
| PUT | `/api/pracas/{id}` |  | RF23, UC24 | Gerente |
| GET | `/api/promocoes` | Promoções cadastradas. Por padrão só as ativas; `?todas=true` inclui as removidas. | RF22, UC08, UC09 | Gerente, Coordenador |
| POST | `/api/promocoes` | UC08 — Criar promoção para um ou mais itens. | UC08 | Gerente, Coordenador |
| DELETE | `/api/promocoes/{id}` | UC09 — Remover promoção. Ela deixa de valer, mas fica no histórico. | UC09 | Gerente, Coordenador |
| GET | `/api/usuarios` | UC04 — Listar contas, ativas e inativas. | UC04 | Gerente |
| POST | `/api/usuarios` | UC04 — Cadastrar conta de usuário com o papel correspondente. | UC04 | Gerente |
| GET | `/api/usuarios/fotos/{nome}` | Foto do crachá: o Gerente vê a de qualquer conta; os demais, só a própria. | RF18, UC04 | qualquer usuário logado |
| GET | `/api/usuarios/{id}` | UC04 — Consultar uma conta. | UC04 | Gerente |
| PUT | `/api/usuarios/{id}` | UC04 — Editar nome, e-mail e papel. | UC04 | Gerente |
| DELETE | `/api/usuarios/{id}/foto` | Tirar a foto do crachá. O arquivo é apagado do servidor. | RF18, UC04 | Gerente |
| PUT | `/api/usuarios/{id}/foto` | Pôr ou trocar a foto do crachá do funcionário (JPG, PNG ou WEBP, até 5 MB). A foto anterior é apagada do servidor, e a nova só é vista por quem está logado (GET /api/usuarios/fotos/{nome}). | RF18, UC04 | Gerente |
| DELETE | `/api/usuarios/{id}/segundo-fator` | UC04 — Desfazer a vinculação do app autenticador de um Gerente ou Coordenador que perdeu o celular. O próximo login volta a pedir a configuração, com um segredo novo (RN07). | RN07, UC04 | Gerente |
| POST | `/api/usuarios/{id}/senha` | UC04 — Definir uma senha nova para quem esqueceu a sua. A senha antiga deixa de valer, e as sessões abertas daquela conta caem na hora. | UC04 | Gerente |
| PATCH | `/api/usuarios/{id}/situacao` | UC04 — Inativar (soft delete) ou reativar a conta. | UC04 | Gerente |
