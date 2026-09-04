# ---- build ----------------------------------------------------------------------------
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Optional build-time default; the container entrypoint still overrides it at runtime.
ARG VITE_API_BASE_URL=""
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
RUN npm run build

# ---- runtime -------------------------------------------------------------------------
FROM nginx:1.27-alpine AS runtime

RUN rm /etc/nginx/conf.d/default.conf
COPY nginx.conf /etc/nginx/conf.d/cpg.conf
COPY --from=build /app/dist /usr/share/nginx/html
COPY docker-entrypoint.sh /docker-entrypoint.d/99-cpg-config.sh
RUN chmod +x /docker-entrypoint.d/99-cpg-config.sh

EXPOSE 80

# nginx:alpine already runs /docker-entrypoint.d/*.sh before starting nginx, so the
# runtime config is generated from API_BASE_URL / VITE_API_BASE_URL on every start.
