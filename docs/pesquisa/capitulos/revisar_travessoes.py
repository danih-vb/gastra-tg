"""Revisão de estilo do Gastra II (M8): troca os travessões do texto corrido por vírgulas, parênteses, dois-pontos ou
ponto e vírgula, conforme o caso. O texto tinha 134 travessões em 66 parágrafos; os que ficam são os das tabelas
(US01 — …) e os das referências.

    pip install python-docx
    python docs/pesquisa/capitulos/revisar_travessoes.py

Cada troca é de um trecho exato; rodar de novo não muda nada. O que não for encontrado é listado no fim.
"""

from pathlib import Path

from docx import Document

from registrar_fase_final import trocar_texto

DOCX = Path(__file__).resolve().parents[1] / "Gastra II.docx"

TROCAS = [
    ("registra a operação do salão — abertura de mesa, pedidos e fechamento da conta — e usa",
     "registra a operação do salão (abertura de mesa, pedidos e fechamento da conta) e usa"),
    ("records the dining room operation — opening a table, placing orders and closing the bill — and uses",
     "records the dining room operation (opening a table, placing orders and closing the bill) and uses"),
    ("dados operacionais — vendas, ocupação de mesas, horários de atendimento e pedidos —, mas",
     "dados operacionais, como vendas, ocupação de mesas, horários de atendimento e pedidos, mas"),
    ("na maioria dos estabelecimentos — sobretudo os de menor porte,", "na maioria dos estabelecimentos, sobretudo nos de menor porte,"),
    ("hierárquico e informal — o garçom mais antigo escolhe a praça que prefere, e o restante é distribuído por sorteio "
     "ou pelo critério do metre —, sem considerar",
     "hierárquico e informal: o garçom mais antigo escolhe a praça que prefere, e o restante é distribuído por sorteio "
     "ou pelo critério do metre, sem considerar"),
    ("padrões de consumo — o que resulta", "padrões de consumo, o que resulta"),
    ("Desenvolver a aplicação web — backend em ASP.NET Core (MICROSOFT, 2026a) e frontend em Angular (GOOGLE, 2026) —, incluindo",
     "Desenvolver a aplicação web, com backend em ASP.NET Core (MICROSOFT, 2026a) e frontend em Angular (GOOGLE, 2026), incluindo"),
    ("distintos — o backend em C#", "distintos, o backend em C#"),
    ("(PYTHON SOFTWARE FOUNDATION, 2026) — comunicando-se", "(PYTHON SOFTWARE FOUNDATION, 2026), comunicando-se"),
    ("R$ 2.227 — recorde histórico do setor", "R$ 2.227, recorde histórico do setor"),
    ("pesquisa bibliográfica — para a fundamentação teórica dos temas envolvidos — e estudo de caso",
     "pesquisa bibliográfica, para a fundamentação teórica dos temas envolvidos, e estudo de caso"),
    ("não sustentaria — a exemplo da", "não sustentaria, a exemplo da"),
    ("cujas métricas — a taxa de acerto da recomendação, a silhueta da segmentação (ROUSSEEUW, 1987) e a desigualdade de "
     "faturamento entre os garçons — são apresentadas no capítulo 5.",
     "cujas métricas são apresentadas no capítulo 5: a taxa de acerto da recomendação, a silhueta da segmentação "
     "(ROUSSEEUW, 1987) e a desigualdade de faturamento entre os garçons."),
    ("de cada bloco do sistema — Business Intelligence, ciência de dados, programação linear e proteção de dados — e a comparação",
     "de cada bloco do sistema (Business Intelligence, ciência de dados, programação linear e proteção de dados) e a comparação"),
    ("critérios hierárquicos e informais — o garçom mais antigo escolhe a praça de sua preferência, e as demais são "
     "distribuídas por sorteio ou pelo critério do metre —, sem levar",
     "critérios hierárquicos e informais: o garçom mais antigo escolhe a praça de sua preferência, e as demais são "
     "distribuídas por sorteio ou pelo critério do metre, sem levar"),
    ("quando o valor total — incluindo, tipicamente, uma taxa de serviço de 10% sobre o consumo, removível a pedido do "
     "cliente — é apurado.",
     "quando o valor total é apurado, incluindo, tipicamente, uma taxa de serviço de 10% sobre o consumo, removível a "
     "pedido do cliente."),
    ("restrições alimentares específicas — como pratos veganos, vegetarianos, sem glúten ou sem lactose —, o que",
     "restrições alimentares específicas, como pratos veganos, vegetarianos, sem glúten ou sem lactose, o que"),
    ("Esses conceitos — praça, comissão, comanda e cardápio — formam", "Esses conceitos (praça, comissão, comanda e cardápio) formam"),
    ("de um sistema — apresentação, regras de negócio, contratos de comunicação e acesso a dados — em módulos",
     "de um sistema (apresentação, regras de negócio, contratos de comunicação e acesso a dados) em módulos"),
    ("Essa inversão de dependência — em que o módulo de mais alto nível define os contratos, e o módulo de mais baixo "
     "nível os implementa — permite",
     "Essa inversão de dependência, em que o módulo de mais alto nível define os contratos e o módulo de mais baixo "
     "nível os implementa, permite"),
    ("só apareceriam em execução — relevante em um sistema cujas regras de negócio, como o fechamento da comanda e o "
     "cálculo da taxa de serviço, precisam estar corretas —, e o fato",
     "só apareceriam em execução, o que importa em um sistema cujas regras de negócio, como o fechamento da comanda e o "
     "cálculo da taxa de serviço, precisam estar corretas, e o fato"),
    ("migrations versionadas — em vez de escrito manualmente em SQL —, o que mantém",
     "migrations versionadas, em vez de escrito manualmente em SQL, o que mantém"),
    ("A camada analítica — responsável pelos algoritmos de recomendação de pratos e de alocação de garçons — foi implementada",
     "A camada analítica, responsável pelos algoritmos de recomendação de pratos e de alocação de garçons, foi implementada"),
    ("otimização matemática — mlxtend", "otimização matemática, como mlxtend"),
    ("DUNNING, 2011) —, que não têm", "DUNNING, 2011), que não têm"),
    ("o restante do sistema — abertura de mesa, lançamento de pedido e fechamento de comanda — continua",
     "o restante do sistema (abertura de mesa, lançamento de pedido e fechamento de comanda) continua"),
    ("provedores de nuvem — inclusive o considerado na estimativa de custos da seção 3.2 —, o que",
     "provedores de nuvem, inclusive o considerado na estimativa de custos da seção 3.2, o que"),
    ("Os quatro componentes — frontend, backend, serviço analítico e banco de dados — são executados",
     "Os quatro componentes (frontend, backend, serviço analítico e banco de dados) são executados"),
    ("registra na operação — comandas, itens pedidos, praças e turnos —, sem planilhas",
     "registra na operação (comandas, itens pedidos, praças e turnos), sem planilhas"),
    ("apresentado ao Gerente — como o faturamento por praça e por período, os itens mais vendidos e o índice de "
     "desempenho dos garçons — foi definido",
     "apresentado ao Gerente, como o faturamento por praça e por período, os itens mais vendidos e o índice de "
     "desempenho dos garçons, foi definido"),
    ("referente à saúde — categoria", "referente à saúde, categoria"),
    ("inciso II) —, serve", "inciso II), serve"),
    ("ciência de dados — suas funcionalidades de IA", "ciência de dados; suas funcionalidades de IA"),
    ("analiticamente mais profundo — Business Intelligence, recomendação por clusterização e regras de associação, e "
     "alocação de garçons por programação linear —, com a",
     "analiticamente mais profundo (Business Intelligence, recomendação por clusterização e regras de associação, e "
     "alocação de garçons por programação linear), com a"),
    ("apoio à decisão gerencial — o próprio autor", "apoio à decisão gerencial; o próprio autor"),
    ("decisão gerencial — sobretudo à distribuição de trabalho entre a equipe de forma mensurável e justa — permanece",
     "decisão gerencial, sobretudo à distribuição de trabalho entre a equipe de forma mensurável e justa, permanece"),
    ("entre a equipe — o GASTRA propõe", "entre a equipe; o GASTRA propõe"),
    ("no momento certo — o sistema apoia", "no momento certo, e o sistema apoia"),
    ("porte do restaurante — número de mesas", "porte do restaurante, medido pelo número de mesas"),
    ("restaurante cliente — já que os pesos", "restaurante cliente, já que os pesos"),
    ("demonstração/piloto — um restaurante cliente, tráfego moderado —, compatível",
     "demonstração/piloto (um restaurante cliente, tráfego moderado), compatível"),
    ("A stack do GASTRA — backend em ASP.NET Core, serviço analítico em Python/FastAPI e banco de dados MySQL — mapeia",
     "A stack do GASTRA (backend em ASP.NET Core, serviço analítico em Python/FastAPI e banco de dados MySQL) mapeia"),
    ("via Registro.br — entidade responsável pelo registro de domínios sob o .br no Brasil —, custa",
     "via Registro.br, entidade responsável pelo registro de domínios sob o .br no Brasil, custa"),
    ("ao longo do projeto — Visual Studio Community", "ao longo do projeto, como Visual Studio Community"),
    ("(LUCID SOFTWARE, 2026) — não entram", "(LUCID SOFTWARE, 2026), não entram"),
    ("surgiriam com escala — múltiplos restaurantes clientes", "surgiriam com escala, como múltiplos restaurantes clientes"),
    ("seção 1.5 — os questionários", "seção 1.5 (os questionários"),
    ("à la carte — dentro do escopo", "à la carte), dentro do escopo"),
    ("hierarquia informal — evidência que sustenta", "hierarquia informal, evidência que sustenta"),
    ("que o originou — o controle do titular sobre os próprios dados — foi atendida",
     "que o originou, o controle do titular sobre os próprios dados, foi atendida"),
    ("papéis de funcionário — Gerente, Coordenador, Metre e Garçom, especializações de um ator genérico Usuário, que "
     "concentra os casos de uso de acesso — e o Cliente",
     "papéis de funcionário (Gerente, Coordenador, Metre e Garçom, especializações de um ator genérico Usuário, que "
     "concentra os casos de uso de acesso) e o Cliente"),
    ("já aberta — por exemplo, quando", "já aberta, por exemplo, quando"),
    ("dos demais grupos — autenticação, gestão de cardápio e configuração do salão — seguem",
     "dos demais grupos (autenticação, gestão de cardápio e configuração do salão) seguem"),
    ("o caminho de volta — do banco implementado ao modelo conceitual — foi percorrido",
     "o caminho de volta, do banco implementado ao modelo conceitual, foi percorrido"),
    ("da comanda — não há avaliação sem comanda —, dependência", "da comanda (não há avaliação sem comanda), dependência"),
    ("foram classificadas em justificadas — como chaves estrangeiras, que só existem no nível físico, e o atributo "
     "derivado, que não vira coluna — e em divergências reais.",
     "foram classificadas em divergências reais e em diferenças justificadas, como as chaves estrangeiras, que só "
     "existem no nível físico, e o atributo derivado, que não vira coluna."),
    ("que não vira coluna. Estas foram corrigidas:", "que não vira coluna. As divergências reais foram corrigidas:"),
    ("consultas agregadas — faturamento por comanda, por praça e turno, faturamento médio por praça, desempenho do "
     "garçom por turno, faturamento por item e itens por comanda — usadas",
     "consultas agregadas (faturamento por comanda, por praça e turno, faturamento médio por praça, desempenho do "
     "garçom por turno, faturamento por item e itens por comanda), usadas"),
    ("data de fechamento — vazia significa aberta —, o que", "data de fechamento (vazia significa aberta), o que"),
    ("resposta incoerente — que deixe garçom sem praça ou exceda as vagas de uma praça — é tratada",
     "resposta incoerente, que deixe garçom sem praça ou exceda as vagas de uma praça, é tratada"),
    ("a mesma coisa — a chance de a mesa pedir o item — e por isso", "a mesma coisa, a chance de a mesa pedir o item, e por isso"),
    ("quatro personas — Garçom e Cliente no formato de celular, Metre em tablet ou computador e Gerente em computador — além do acesso",
     "quatro personas (Garçom e Cliente no formato de celular, Metre em tablet ou computador e Gerente em computador), além do acesso"),
    ("sobre os algoritmos — a calibração", "sobre os algoritmos, isto é, a calibração"),
    ("histórico simulado (5.2.4) —, a avaliação", "histórico simulado (5.2.4), a avaliação"),
    ("abre uma mesa em dois toques — a mesa livre e a quantidade de pessoas —, dentro do limite",
     "abre uma mesa em dois toques (a mesa livre e a quantidade de pessoas), dentro do limite"),
    ("o motivo da escolha — a faixa de vendas em relação à equipe e há quantos turnos ele não recebe uma praça de alto "
     "movimento —, sem exibir",
     "o motivo da escolha (a faixa de vendas em relação à equipe e há quantos turnos ele não recebe uma praça de alto "
     "movimento), sem exibir"),
    ("tiver sido ignorado — o que impede", "tiver sido ignorado, o que impede"),
    ("sem banco nem rede — a composição da mesa, o cálculo da taxa, o bloqueio de conta, o índice de desempenho —, e os testes",
     "sem banco nem rede (a composição da mesa, o cálculo da taxa, o bloqueio de conta, o índice de desempenho), e os testes"),
    ("só existem no banco — views, triggers e restrições CHECK — são testados",
     "só existem no banco (views, triggers e restrições CHECK) são testados"),
    ("se ela for violada — por exemplo, o que verifica", "se ela for violada, como o que verifica"),
    ("potencial diferente — forte,", "potencial diferente (forte,"),
    ("R$ 600 —, presença", "R$ 600), presença"),
    ("puxam para lados opostos — mais peso no faturamento reduz a desigualdade e aumenta a espera —, e nenhum",
     "puxam para lados opostos, porque mais peso no faturamento reduz a desigualdade e aumenta a espera, e nenhum"),
    ("de 8,9 turnos — cerca de quatro dias", "de 8,9 turnos, cerca de quatro dias"),
    ("três perfis de consumo — um almoço executivo", "três perfis de consumo (um almoço executivo"),
    ("arroz de coco e sobremesas —, dos quais", "arroz de coco e sobremesas), dos quais"),
    ("índice de Rand ajustado — em que 1 indica concordância perfeita e 0, a que o acaso produziria —, ficou",
     "índice de Rand ajustado (em que 1 indica concordância perfeita e 0, a que o acaso produziria), ficou"),
    ("recomendação segmentada — as três fontes", "recomendação segmentada: as três fontes"),
    ("que quase toda mesa do perfil pedia —, corrigido ordenando-se", "que quase toda mesa do perfil pedia. O defeito foi corrigido ordenando-se"),
    ("à mesma pessoa — exatamente o rodízio", "à mesma pessoa, que é exatamente o rodízio"),
    ("propositalmente diferente — o que mais fatura ganha 2,2 vezes o que menos fatura — e distribuídos",
     "propositalmente diferente (o que mais fatura ganha 2,2 vezes o que menos fatura) e distribuídos"),
    ("primeiro fator da regra — levar quem faturou menos para onde há mais movimento — que o faz",
     "primeiro fator da regra, levar quem faturou menos para onde há mais movimento, que o faz"),
    ("em execução — as quatro do cliente", "em execução (as quatro do cliente"),
    ("conta não encontrada —, no tamanho", "conta não encontrada), no tamanho"),
    ("como na entrega — navegador", "como na entrega (navegador"),
    ("sem nenhum dublê —, seguindo", "sem nenhum dublê), seguindo"),
]


def main() -> None:
    documento = Document(str(DOCX))
    corpo = [p for p in documento.paragraphs if p.style.name == "Normal"]
    feitas, faltaram = 0, []
    for atual, novo in TROCAS:
        try:
            if any(trocar_texto(p, atual, novo) for p in corpo):
                feitas += 1
            elif not any(novo in p.text for p in corpo):
                faltaram.append(atual)
        except SystemExit as erro:
            faltaram.append(f"{atual}  [{erro}]")
    documento.save(str(DOCX))
    fim = next(i for i, p in enumerate(corpo) if p.text.startswith("AGRAWAL"))
    restantes = sum(p.text.count("—") for p in corpo[:fim] if len(p.text) > 120)
    print(f"{feitas} trocas feitas; {restantes} travessões continuam no texto corrido.")
    for trecho in faltaram:
        print("NÃO ENCONTRADO:", trecho[:110])


if __name__ == "__main__":
    main()
