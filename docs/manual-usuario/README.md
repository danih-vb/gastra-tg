# Manual do usuário — GASTRA

| Arquivo | Conteúdo |
|---|---|
| [`GASTRA_Manual_do_Usuario.md`](GASTRA_Manual_do_Usuario.md) | Manual de uso por perfil: Garçom, Metre, Gerente, Coordenador e Cliente (issue #49) |

## Como manter

As imagens vêm de `docs/ux-ui/telas/`, geradas pelo script `docs/ux-ui/gerar_telas.py` a partir do protótipo
navegável. O manual referencia esses arquivos por caminho relativo (`../ux-ui/telas/…`), então regerar as imagens
atualiza o manual junto.

Quando uma tela implementada em Angular ficar diferente do protótipo, há dois caminhos:

1. Atualizar o protótipo e regerar a imagem — melhor quando a diferença é pequena;
2. Manter a imagem e **registrar a diferença no texto**, como está feito na seção 4.4 (praça da mesa).

O que **precisa** ser refletido aqui a cada mudança: tela nova, regra de negócio que muda o que o usuário pode
fazer, e mensagem de erro nova que valha entrar na tabela da seção 7.

## O que este manual não é

Não é documentação técnica nem guia de instalação. Para subir o ambiente, ver `infra/README.md`, `backend/README.md`,
`frontend/README.md` e `data-science/README.md`.
