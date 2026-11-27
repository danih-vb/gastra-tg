# Manual do usuário — GASTRA

| Arquivo | Conteúdo |
|---|---|
| [`GASTRA_Manual_do_Usuario.md`](GASTRA_Manual_do_Usuario.md) | Manual de uso por perfil: Garçom, Metre, Gerente, Coordenador e Cliente (issue #49). É a fonte |
| [`GASTRA_Manual_do_Usuario.docx`](GASTRA_Manual_do_Usuario.docx) | O mesmo manual para imprimir, com figuras numeradas, gerado por `gerar_manual_docx.py` |

O manual também está no **Apêndice A** do [`Gastra II`](../pesquisa/Gastra%20II.pdf), inserido por
`docs/pesquisa/capitulos/inserir_apendice_a.py`.

## Como manter

As imagens vêm de duas pastas:

- `docs/ux-ui/telas-reais/`: capturas do sistema em execução, feitas por `docs/ux-ui/capturar_telas_reais.py`. São as
  preferidas;
- `docs/ux-ui/telas/`: telas do protótipo navegável, geradas por `docs/ux-ui/gerar_telas.py`, para o que ainda não
  tem captura (entre outras, o login, o segundo fator, as promoções e as telas de erro).

Ao mudar o `.md`, rode de novo os dois geradores, para o `.docx` e o Apêndice A não ficarem para trás:

```bash
python docs/manual-usuario/gerar_manual_docx.py
python docs/pesquisa/capitulos/inserir_apendice_a.py
```

Depois, no Word, Ctrl+A e F9 em cada um (numeração das figuras), e exportar o PDF do Gastra II.

O que **precisa** ser refletido aqui a cada mudança: tela nova, regra de negócio que muda o que o usuário pode
fazer, e mensagem de erro nova que valha entrar na tabela da seção 7.

## O que este manual não é

Não é documentação técnica nem guia de instalação. Para subir o ambiente, ver `infra/README.md`, `backend/README.md`,
`frontend/README.md` e `data-science/README.md`.
