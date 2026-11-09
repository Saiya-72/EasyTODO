# Development image: runs the Vite dev server with hot reload.
# docker-compose.yml mounts ./frontend into /app so your edits show up instantly.
FROM node:22-alpine

WORKDIR /app

# Install dependencies first so this layer is cached until package.json changes.
COPY package*.json ./
RUN npm install

COPY . .

EXPOSE 5173

CMD ["npm", "run", "dev"]

# ---------------------------------------------------------------------------
# LATER (production): build static files and serve them with nginx instead.
#
#   FROM node:22-alpine AS build
#   WORKDIR /app
#   COPY package*.json ./
#   RUN npm ci
#   COPY . .
#   RUN npm run build
#
#   FROM nginx:alpine
#   COPY --from=build /app/dist /usr/share/nginx/html
#   # You'd also add an nginx.conf that proxies /api to the backend container.
# ---------------------------------------------------------------------------
