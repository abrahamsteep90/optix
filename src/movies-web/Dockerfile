# Build the React app, then serve the static files with nginx.
# nginx also forwards /api (and the API docs) to the API container, so the browser only talks to one address.

FROM node:24-alpine AS build
WORKDIR /app

# Install first, so this layer is cached until the dependencies change.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# The unprivileged image runs nginx as a non-root user, on port 8080.
FROM nginxinc/nginx-unprivileged:stable-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080
