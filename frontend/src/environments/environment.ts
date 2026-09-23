/**
 * Desenvolvimento: `ng serve` na porta 4200 e a API em 5019 são origens diferentes, então a URL é absoluta
 * e o CORS da API precisa liberar http://localhost:4200 (já libera, ver PoliticaCors).
 */
export const ambiente = {
  urlDaApi: 'http://localhost:5019',
};
