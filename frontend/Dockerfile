# Imagem do frontend: Node compila, nginx serve (issue #173).

FROM node:24-alpine AS compilacao
WORKDIR /app

# As dependências mudam bem menos que o código: copiadas primeiro, a camada fica em cache.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
# A configuração "production" troca o environment.ts pelo de produção (D12): a URL da API fica relativa.
RUN npm run build -- --configuration production

FROM nginx:1.27-alpine AS servidor

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=compilacao /app/dist/gastra-web/browser /usr/share/nginx/html

# Sem root: a imagem oficial do nginx já traz o usuário nginx, e a porta 8080 não exige privilégio.
RUN touch /var/run/nginx.pid \
 && chown -R nginx:nginx /var/run/nginx.pid /var/cache/nginx /usr/share/nginx/html
USER nginx

EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=3s --start-period=5s --retries=5 \
  CMD wget --quiet --tries=1 --spider http://localhost:8080/saude || exit 1
