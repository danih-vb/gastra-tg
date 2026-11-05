"""Camada analítica do GASTRA.

Responsabilidade: calcular e devolver resultados. A gravação no banco de dados é
responsabilidade do backend em C#.

- ``api``: serviço FastAPI consumido pelo backend.
- ``recomendacao``: clusterização e regras de associação (RF09).
- ``alocacao``: programação linear para alocação de garçons (RF06, RN03).

Os módulos de algoritmos não dependem da API, para poderem ser usados também nos notebooks.
"""
