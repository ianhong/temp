FROM node:18-alpine as build-env
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Development

COPY . ./
RUN npm install && \
    npm run build

FROM nginx:alpine
COPY nginx/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build-env /app/build /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]