/**
 * Produção (contêiner): o nginx serve a SPA e repassa /api para a API, então tudo sai da mesma origem e a
 * URL é relativa (decisão D12). Sem origem cruzada não há CORS, e o celular na rede local funciona sem
 * recompilar, porque o endereço usado é sempre o mesmo pelo qual a página foi aberta.
 */
export const ambiente = {
  urlDaApi: '',
};
