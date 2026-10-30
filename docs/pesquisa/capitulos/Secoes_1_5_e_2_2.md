# Seções 1.5 e 2.2 revisadas e referências novas

Texto revisado das seções 1.5 (issue #206) e 2.2 (issue #204) do `Gastra II.docx`, escrito aqui para revisão linha a
linha, como os capítulos 4 a 6. Os parágrafos sem marcação são os do documento, sem alteração; os marcados com
**[novo]** ou **[alterado]** são os que mudam. No fim, as referências que entram na lista do documento, cobrindo
também as citações acrescentadas aos capítulos 4, 5 e 6.

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
