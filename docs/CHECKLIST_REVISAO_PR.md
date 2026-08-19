# Roteiro de revisão de Pull Request — GASTRA

Este checklist é para quem está **revisando** um PR do outro integrante da dupla — é diferente do
checklist que já existe em `.github/PULL_REQUEST_TEMPLATE.md`, que é preenchido por quem **abre**
o PR. Os dois se complementam: um garante que o autor não esqueceu nada, este garante que o revisor
não aprova no automático.

Não precisa ser burocrático — a maioria dos PRs vai passar por isso em 2-3 minutos. A ideia é ter
uma lista fixa pra não depender de lembrar tudo de cabeça toda vez, principalmente em PRs de
documentação, onde é mais fácil "só ler por cima e aprovar".

## 1. A base do PR está certa?
- [ ] O PR está direcionado para `dev`, não para `main` (a menos que seja explicitamente um PR de
      release, `dev` → `main`, o que deve ser raro e combinado entre os dois antes).
- [ ] O título segue Conventional Commits (`feat:`, `fix:`, `docs:`, etc. — ver `CONTRIBUTING.md`,
      seção 3).

## 2. Consistência com escopo e regras do projeto
- [ ] Se o PR toca em módulo de comandas ou cardápio digital, ele **não trata esses itens como
      escopo fechado** — nem em comentário de código, nem em texto (regra do `CONTRIBUTING.md`,
      seção 2). Se tiver dúvida se algo já foi validado, confira `docs/GASTRA_STATUS.md`, seção 1,
      antes de aprovar.
- [ ] Se o PR menciona ou implementa um RF/RNF/RN, ele é consistente com o que está descrito em
      `docs/requisitos/GASTRA_Requisitos_RN.docx` e na Matriz de Rastreabilidade. Se o PR muda o
      comportamento de um requisito, os dois documentos foram atualizados juntos — não só um deles.
- [ ] Se o PR muda escopo, decisão de design, ou item do checklist, o `docs/GASTRA_STATUS.md` foi
      atualizado no mesmo PR (não deixado para depois).

## 3. Dados sensíveis e segurança
- [ ] Nenhum dado bruto de questionário, resposta identificável de garçom/cliente, ou qualquer
      informação pessoal foi adicionado fora de `data-science/data/processed/` (que só recebe dado
      já agregado/anonimizado).
- [ ] Nenhum segredo (chave de API, string de conexão, senha) aparece em texto plano em nenhum
      arquivo do PR — nem em código, nem em exemplo de configuração.

## 4. A prova da banca
- [ ] Você, como revisor, consegue entender o que esse PR faz e por quê **sem precisar perguntar ao
      autor**. Se precisou perguntar pra entender, o PR provavelmente precisa de mais contexto na
      descrição ou em comentários — peça o ajuste antes de aprovar, não aprove e pergunte depois.
- [ ] Terminologia usada é consistente com o resto dos documentos do projeto (ex.: não chamar de
      "pedido" num lugar e "comanda" em outro pra a mesma coisa, sem motivo).
- [ ] Se o PR faz uma afirmação numérica ou estatística (ex.: resultado de questionário), ela está
      corretamente atribuída à fonte e a amostra pequena/preliminar está sinalizada, quando for o
      caso — mesmo padrão já usado na seção 6 do `GASTRA_STATUS.md`.

## 5. Mecânica do GitHub
- [ ] Labels aplicadas fazem sentido (bloco certo + tipo certo).
- [ ] Se existe uma issue correspondente, o PR está vinculado a ela (`Closes #N` na descrição) —
      isso é o que move o card automaticamente no board.
- [ ] Sem conflitos de merge pendentes (o GitHub avisa isso na própria tela do PR).

## 6. Depois de aprovar
- [ ] Ao mergear, marcar a opção de deletar a branch de feature (o próprio GitHub oferece o botão
      logo após o merge) — evita acumular branches antigas já integradas.
- [ ] Conferir que o card da issue relacionada realmente moveu para "Concluído" no board (o
      workflow deveria fazer isso sozinho — se não mover, algo está errado na configuração do
      workflow, vale investigar, não só ignorar).

---

**Regra geral:** se alguma dessas checkboxes te deixar em dúvida, é melhor comentar no PR e pedir
ajuste do que aprovar "confiando que está tudo bem". O ponto inteiro de revisão cruzada é os dois
conseguirem defender qualquer parte do repositório na banca — aprovar sem entender de verdade
anula esse propósito.
