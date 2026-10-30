# Revisões dos capítulos 1 a 3 e referências novas

Mudanças no texto já existente do `Gastra II.docx` (capítulos 1 a 3), escritas aqui para revisão linha a linha,
como os capítulos 4 a 6: as seções 1.5 (issue #206) e 2.2 (issue #204), a seção 2.3 nova, a 1.6 e três correções
pontuais. Os parágrafos sem marcação são os do documento, sem alteração; os marcados com **[novo]** ou
**[alterado]** são os que mudam. No fim, as referências novas.

## 1.5 METODOLOGIA DA PESQUISA E DESENVOLVIMENTO

Quanto à natureza, esta pesquisa caracteriza-se como aplicada *(parágrafo sem alteração)*.

O levantamento de requisitos partiu de duas frentes complementares *(parágrafo sem alteração)*.

**[alterado]** O desenvolvimento seguiu processo iterativo baseado no Scrum (SCHWABER; SUTHERLAND, 2020), adaptado à
realidade de uma dupla, sem os papéis formais de Product Owner e Scrum Master: as tarefas foram registradas como
issues, com critérios de aceite, prioridade e marco, e organizadas em sprints semanais no quadro do GitHub
Projects, com revisão e retrospectiva ao final de cada sprint e planejamento da seguinte a partir delas. O controle
de versão seguiu o modelo GitFlow (DRIESSEN, 2010), com as branches main, dev e uma branch por funcionalidade. Toda
mudança entrou por Pull Request e só foi integrada depois da revisão do outro integrante, regra imposta pelo
próprio repositório; a cada Pull Request, a integração contínua do GitHub Actions (GITHUB, 2026) compilou o sistema
e executou os testes automatizados, com o resultado exibido ao revisor antes da decisão. As convenções desse
processo estão registradas no guia de contribuição do repositório.

**[alterado — trecho que estava no parágrafo anterior]** A modelagem e a implementação do banco de dados relacional
seguiram a abordagem Code First, com o esquema físico gerado a partir de migrations do Entity Framework Core e
validado por engenharia reversa contra o modelo conceitual. A aplicação web foi desenvolvida com backend em ASP.NET
Core, organizado em arquitetura de camadas com regra de dependência verificada por teste automatizado, e frontend
em Angular. As telas foram desenhadas primeiro em um protótipo navegável, e as telas implementadas passaram por
avaliação heurística (NIELSEN, 1994), descrita na seção 4.4.6. A camada analítica, em Python, expõe os algoritmos
de recomendação de pratos, de segmentação das comandas em perfis de consumo e de alocação de garçons por meio de um
serviço HTTP consumido pelo backend.

Em razão das restrições impostas pela Lei Geral de Proteção de Dados *(parágrafo sem alteração)*.

**[alterado]** A validação do sistema ocorreu em três frentes: testes automatizados de unidade, integração e
arquitetura, cobrindo os quatro blocos do projeto (núcleo de comandas, alocação por programação linear,
recomendação de pratos e conformidade com a LGPD); a avaliação heurística das telas; e cenários operacionais de
ponta a ponta, executados com o sistema completo em contêineres. Os algoritmos analíticos foram avaliados, além
disso, por experimentos sobre os dados simulados, cujas métricas — a taxa de acerto da recomendação, a silhueta da
segmentação e a desigualdade de faturamento entre os garçons — são apresentadas no capítulo 5.

## 1.6 ESTRUTURA DO TRABALHO

**[alterado: duas frases]** No segundo capítulo, trocar "apresenta os conceitos e regras de negócio do domínio de
restaurantes, a fundamentação teórica das tecnologias e da arquitetura escolhidas, e a comparação do GASTRA com
trabalhos correlatos" por: "apresenta os conceitos e regras de negócio do domínio de restaurantes, a fundamentação
teórica das tecnologias e da arquitetura escolhidas, os fundamentos de cada bloco do sistema — Business
Intelligence, ciência de dados, programação linear e proteção de dados — e a comparação do GASTRA com trabalhos
correlatos". No quarto capítulo, trocar o título "Engenharia de Software e Desenvolvimento do Produto" por
"Engenharia de Software e Projeto do Sistema", que é o título do capítulo.

## 2.2 TECNOLOGIAS E ARQUITETURA

A arquitetura do GASTRA é organizada em camadas *(parágrafo sem alteração; já cita PRESSMAN; MAXIM, 2011 e
SOMMERVILLE, 2019)*.

**[alterado: só as citações]** O backend foi desenvolvido em ASP.NET Core (MICROSOFT, 2026a), framework definido no
projeto de pesquisa formal do GASTRA para a construção da API responsável pelas regras de negócio, autenticação e
gravação no banco de dados. O acesso a dados utiliza o Entity Framework Core (MICROSOFT, 2026b) como mapeador
objeto-relacional, com a abordagem Code First *(resto do parágrafo sem alteração)*.

**[alterado: só a citação]** O frontend foi desenvolvido em Angular (GOOGLE, 2026), também definido no projeto de
pesquisa formal *(resto do parágrafo sem alteração)*.

**[alterado: só as citações]** A camada analítica — responsável pelos algoritmos de recomendação de pratos e de
alocação de garçons — foi implementada separadamente, em Python, e exposta ao backend por meio de um serviço HTTP
com FastAPI (RAMÍREZ, 2026). Essa separação segue o princípio de que cada tecnologia deve ser usada onde apresenta
a maior vantagem: o ecossistema Python concentra bibliotecas maduras de ciência de dados e otimização matemática —
mlxtend (RASCHKA, 2018), scikit-learn (PEDREGOSA et al., 2011) e PuLP (MITCHELL; O'SULLIVAN; DUNNING, 2011) —, que
não têm equivalente igualmente consolidado no ecossistema .NET. *(resto do parágrafo sem alteração)*

**[alterado: só a citação]** O banco de dados relacional utiliza MySQL (ORACLE, 2026), também definido no projeto de
pesquisa formal *(resto do parágrafo sem alteração)*.

**[novo]** Os quatro componentes — frontend, backend, serviço analítico e banco de dados — são executados em
contêineres Docker (DOCKER, 2026). Um contêiner empacota o componente com tudo de que ele precisa para rodar, de
modo que a mesma configuração vale no desenvolvimento, na integração contínua e na entrega, e o sistema completo
sobe com um único comando em qualquer máquina com o Docker instalado, sem instalar .NET, Node.js ou Python.

**[novo]** A autenticação utiliza o JSON Web Token (JWT), padrão aberto para transmitir, de forma assinada, a
identidade e o papel do usuário entre o navegador e o servidor (JONES; BRADLEY; SAKIMURA, 2015). Como um JWT
continua válido até expirar, o GASTRA associa a cada token uma chave de sessão guardada no banco, o que permite
invalidá-lo no logoff, na troca de papel e na inativação da conta; para os perfis de gestão, o login exige ainda
um segundo fator, o código de um aplicativo autenticador. A revisão de segurança tomou como referência o OWASP Top
10 (OWASP FOUNDATION, 2021), lista dos riscos mais críticos de segurança em aplicações web: para cada risco
aplicável, a documentação de arquitetura do repositório registra como o sistema o trata.

## 2.3 FUNDAMENTOS DOS BLOCOS DO SISTEMA **[nova; a atual 2.3 passa a ser 2.4]**

O GASTRA apoia-se em quatro eixos teóricos, um para cada bloco do sistema: o Business Intelligence, base dos
relatórios gerenciais; a ciência de dados, com as regras de associação e a clusterização que sustentam a
recomendação de pratos; a programação linear, aplicada à distribuição dos garçons entre as praças; e a Lei Geral de
Proteção de Dados, que orienta o tratamento das informações em todos os outros. As subseções a seguir apresentam os
fundamentos de cada eixo e a sua relação com a solução. O texto parte do referencial teórico do TG I, revisto para
corresponder ao que foi de fato construído.

### 2.3.1 Business Intelligence e apoio à decisão

O Business Intelligence (BI) é o conjunto de técnicas e ferramentas voltadas à coleta, organização, análise e
apresentação de informações que subsidiam a tomada de decisão gerencial. Segundo Santos e Gibertoni (2022), o BI
permite que gestores tomem decisões mais assertivas e ágeis, convertendo grandes volumes de dados em relatórios e
indicadores relevantes para a competitividade das empresas. No GASTRA, essa conversão é feita sobre os dados que o
próprio sistema registra na operação — comandas, itens pedidos, praças e turnos —, sem planilhas paralelas.

A aplicação do BI ao setor de alimentação encontra respaldo em Martínez Ortiz (2025), que analisa as condições de
implementação de Business Intelligence em micro e pequenas empresas do ramo de restaurantes e destaca que o uso de
indicadores-chave de desempenho (KPIs) alinhados aos objetivos do estabelecimento contribui para a gestão, a análise
de vendas e a experiência do cliente. O GASTRA segue essa orientação: cada indicador apresentado ao Gerente — como o
faturamento por praça e por período, os itens mais vendidos e o índice de desempenho dos garçons — foi definido
junto com a pergunta de gestão que ele responde e o critério com que é calculado, registrados na documentação de
requisitos do projeto.

### 2.3.2 Ciência de dados: regras de associação e clusterização

A ciência de dados amplia as possibilidades do BI ao aplicar técnicas de mineração sobre os dados operacionais. No
contexto de restaurantes, Huallpa Tapia et al. (2019) aplicam mineração de dados e técnicas de classificação para
compreender e elevar a satisfação dos clientes, com precisão preditiva superior a 80%, o que evidencia o potencial
da análise do comportamento de consumo para apoiar decisões no setor.

A recomendação de pratos do GASTRA apoia-se nas regras de associação, técnica também conhecida como análise de
cesta de compras. O algoritmo Apriori (AGRAWAL; SRIKANT, 1994) encontra, em um conjunto de transações, os grupos de
itens que aparecem juntos com frequência e, a partir deles, regras do tipo "quem leva A também leva B". Cada regra
é avaliada por três medidas: o suporte, fração das transações que contêm A e B juntos; a confiança, fração das
transações com A que também contêm B; e o lift, razão entre a confiança e a frequência de B, que indica se levar A
aumenta (lift acima de 1) ou diminui (abaixo de 1) a chance de levar B. Gonçalves e Soler (2012) aplicam essa
técnica à cesta de compras do varejo, identificando produtos adquiridos em conjunto para orientar ações
comerciais. No GASTRA, a transação é a comanda, e a regra é usada no momento do pedido, para sugerir ao garçom o
complemento que costuma acompanhar o que a mesa já pediu.

A clusterização, por sua vez, agrupa elementos semelhantes sem que os grupos sejam conhecidos de antemão. O
algoritmo K-Means (MACQUEEN, 1967) parte de uma quantidade k de grupos, associa cada elemento ao centro mais
próximo e recalcula os centros até que a divisão se estabilize. Como k precisa ser informado, a escolha costuma
usar o coeficiente de silhueta (ROUSSEEUW, 1987), que compara, para cada elemento, a distância média aos elementos
do próprio grupo com a distância média aos do grupo vizinho mais próximo: varia de -1 a 1, e valores mais altos
indicam grupos mais coesos e mais separados entre si. Cavique (2003) propõe a micro-segmentação de clientes a
partir de dados de consumo, combinando a segmentação com a análise da cesta de compras para identificar o produto
adequado a cada perfil, e Syed Marzuki, Hall e Ballantine (2014) empregam análise de clusters no próprio setor de
restaurantes, para agrupar perfis de gestores a partir de questionários.

O GASTRA combina as duas técnicas, como propõe Cavique (2003), com uma diferença deliberada: os grupos são de
comandas, e não de clientes. Como o cliente não é identificado (seção 2.3.4), o perfil é definido apenas pelo que
foi pedido na mesa, e a recomendação passa a considerar, além das regras gerais, as regras próprias do perfil de
consumo a que a comanda em andamento se assemelha.

### 2.3.3 Programação linear e o problema de designação

A programação linear trata da alocação de recursos limitados entre atividades concorrentes, otimizando uma função
objetivo linear sujeita a restrições também lineares (HILLIER; LIEBERMAN, 2006). Um de seus casos particulares é o
problema de designação, em que cada agente deve ser atribuído a uma tarefa de modo a minimizar o custo total. Por
ser também um caso particular do problema de transporte, a sua solução ótima é inteira sem que seja preciso impor
essa condição ao modelo (HILLIER; LIEBERMAN, 2006), o que permite resolvê-lo com rapidez a cada turno.

A distribuição de garçons entre as praças configura-se como um problema desse tipo. Zaniol (2011) modela a
alocação de funcionários como um emparelhamento em grafos bipartidos, resolvido por programação linear, buscando a
alocação ideal segundo as aptidões dos trabalhadores. Coelho (2021) desenvolve um modelo de programação linear
inteira mista para a designação de entregadores em uma empresa de food delivery, validado com casos reais. De forma
mais próxima a este trabalho, Vieira et al. (2015) determinam escalas de trabalho de funcionários de restaurantes
considerando as suas preferências, por programação linear inteira, com o objetivo de homogeneizar a carga de
trabalho. O GASTRA compartilha com este último o domínio e o princípio de distribuir de forma mais justa; o critério,
porém, não é a carga de trabalho nem a aptidão, mas a oportunidade de faturamento: o custo de cada designação
combina a diferença de faturamento entre os garçons e o tempo desde a última praça de alto potencial.

Para verificar se a distribuição fica de fato mais justa ao longo do tempo, é preciso medir a desigualdade. O
coeficiente de Gini (GINI, 1912), medida clássica de concentração, vale zero quando todos os valores são iguais e
se aproxima de um quando um único elemento concentra o total. No capítulo 5, ele é aplicado ao faturamento médio
por turno dos garçons, para comparar a regra do sistema com a distribuição sem critério.

### 2.3.4 Proteção de dados e a LGPD

Por tratar dados de clientes e funcionários, o GASTRA deve observar a Lei Geral de Proteção de Dados Pessoais
(LGPD), Lei nº 13.709/2018, cujo princípio da necessidade estabelece a:

> **[citação longa: recuo de 4 cm, fonte menor e espaçamento simples, como no TG I]** limitação do tratamento ao
> mínimo necessário para a realização de suas finalidades, com abrangência dos dados pertinentes, proporcionais e
> não excessivos em relação às finalidades do tratamento de dados (BRASIL, 2018, art. 6º, inciso III).

Rapôso et al. (2019), em revisão sistemática sobre a LGPD aplicada à tecnologia da informação, apontam os desafios
de adaptação dos sistemas às exigências legais e a importância de tratá-las desde o desenvolvimento do software, e
não depois dele. O GASTRA seguiu esse caminho: a proteção de dados foi tratada como requisito de projeto. O cliente
não é identificado, porque nenhuma funcionalidade precisa saber quem ele é; a restrição alimentar, que pode revelar
dado referente à saúde — categoria que a lei classifica como dado pessoal sensível (BRASIL, 2018, art. 5º, inciso
II) —, serve ao atendimento da mesa e não chega aos relatórios; e as análises usam somente dados agregados. As
decisões e os testes que as verificam são apresentados nos capítulos 4 e 5.

## Correções pontuais

- **3.2, depois do parágrafo que começa "A stack do GASTRA":** apagar a linha "Azure Pricing 2026: Real Costs,
  Plans & Cheaper Options • Kuberns +2", que é resto de uma página copiada do navegador. A fonte da Tabela 1 já
  cita Kuberns (2026).
- **2.4 (atual 2.3), parágrafo do Saipos:** a obra não é citada no texto. Trocar "O Saipos é um sistema de gestão"
  por "O Saipos (SAIPOS, [20--]) é um sistema de gestão".
- **3.1, fonte do Canvas:** o texto diz "Strategyzer (2026)", mas a referência não tem ano. Trocar por
  "Strategyzer ([20--])".
- **Referências do Saipos e do Strategyzer:** trocar "[ano não informado]" por "[20--]", a forma da NBR 6023 para
  data desconhecida, com a década provável.

## Referências novas

Entram na lista de referências do documento, em ordem alfabética junto às existentes.

AGRAWAL, Rakesh; SRIKANT, Ramakrishnan. Fast algorithms for mining association rules. In: INTERNATIONAL CONFERENCE
ON VERY LARGE DATA BASES, 20., 1994, Santiago. Proceedings [...]. San Francisco: Morgan Kaufmann, 1994. p. 487-499.

DOCKER INC. Docker Docs. [S. l.], 2026. Disponível em: https://docs.docker.com/. Acesso em: 30 out. 2026.

DRIESSEN, Vincent. A successful Git branching model. [S. l.], 2010. Disponível em:
https://nvie.com/posts/a-successful-git-branching-model/. Acesso em: 30 out. 2026.

GINI, Corrado. Variabilità e mutabilità: contributo allo studio delle distribuzioni e delle relazioni statistiche.
Bologna: Tipografia di Paolo Cuppini, 1912.

GITHUB. GitHub Actions documentation. [S. l.], 2026. Disponível em: https://docs.github.com/actions. Acesso em:
30 out. 2026.

GOOGLE. Angular. [S. l.], 2026. Disponível em: https://angular.dev/. Acesso em: 30 out. 2026.

JONES, Michael B.; BRADLEY, John; SAKIMURA, Nat. JSON Web Token (JWT). RFC 7519. [S. l.]: Internet Engineering
Task Force, 2015. Disponível em: https://www.rfc-editor.org/rfc/rfc7519. Acesso em: 30 out. 2026.

MACQUEEN, James B. Some methods for classification and analysis of multivariate observations. In: BERKELEY
SYMPOSIUM ON MATHEMATICAL STATISTICS AND PROBABILITY, 5., 1967, Berkeley. Proceedings [...]. Berkeley: University
of California Press, 1967. v. 1, p. 281-297.

MICROSOFT. ASP.NET Core documentation. [S. l.], 2026a. Disponível em: https://learn.microsoft.com/aspnet/core/.
Acesso em: 30 out. 2026.

MICROSOFT. Entity Framework Core. [S. l.], 2026b. Disponível em: https://learn.microsoft.com/ef/core/. Acesso em:
30 out. 2026.

MITCHELL, Stuart; O'SULLIVAN, Michael; DUNNING, Iain. PuLP: a linear programming toolkit for Python. Auckland: The
University of Auckland, 2011. Disponível em: https://optimization-online.org/2011/09/3178/. Acesso em: 30 out. 2026.

NIELSEN, Jakob. Heuristic evaluation. In: NIELSEN, Jakob; MACK, Robert L. (ed.). Usability inspection methods. New
York: John Wiley & Sons, 1994. p. 25-62.

ORACLE. MySQL 8.4 reference manual. [S. l.], 2026. Disponível em: https://dev.mysql.com/doc/refman/8.4/en/. Acesso
em: 30 out. 2026.

OWASP FOUNDATION. OWASP Top 10:2021. [S. l.], 2021. Disponível em: https://owasp.org/Top10/2021/. Acesso em:
30 out. 2026.

PEDREGOSA, Fabian et al. Scikit-learn: machine learning in Python. Journal of Machine Learning Research, v. 12,
p. 2825-2830, 2011.

RAMÍREZ, Sebastián. FastAPI. [S. l.], 2026. Disponível em: https://fastapi.tiangolo.com/. Acesso em: 30 out. 2026.

RASCHKA, Sebastian. MLxtend: providing machine learning and data science utilities and extensions to Python's
scientific computing stack. Journal of Open Source Software, v. 3, n. 24, p. 638, 2018. DOI: 10.21105/joss.00638.

ROUSSEEUW, Peter J. Silhouettes: a graphical aid to the interpretation and validation of cluster analysis. Journal
of Computational and Applied Mathematics, v. 20, p. 53-65, 1987. DOI: 10.1016/0377-0427(87)90125-7.

SCHWABER, Ken; SUTHERLAND, Jeff. O Guia do Scrum: o guia definitivo para o Scrum: as regras do jogo. [S. l.],
2020. Disponível em: https://scrumguides.org/docs/scrumguide/v2020/2020-Scrum-Guide-PortugueseBR-3.0.pdf. Acesso
em: 30 out. 2026.
