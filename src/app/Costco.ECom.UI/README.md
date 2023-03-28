# Inventory Availability App

Inventory availability app implementation

### Dependencies
General:
```
npm i react-router-dom
npm i axios
npm i bulma
```

FontAwesome:
```
npm i --save @fortawesome/fontawesome-svg-core

npm i --save @fortawesome/free-solid-svg-icons
npm i --save @fortawesome/free-regular-svg-icons

npm i --save @fortawesome/react-fontawesome@latest
```

### Docker Setup
```
docker build -t ecom-ui .
docker image ls
docker run -d -it  -p 80:80/tcp --name docker-ecom-ui ecom-ui:latest
```

### Nginx 404 React Routing Issue

The react app redirected to 404 page when doing a manual page refresh. To prevent this follow these steps:

1. Create `nginx` folder on root
2. Create `nginx.conf` file within `nginx` folder (entire path `nginx/nginx.conf`)
3. Add following code in `nginx/nginx.conf`:
```
server {
    listen 80;
    location / {
        root   /usr/share/nginx/html;
        index  index.html index.htm;
        try_files $uri $uri/ /index.html;
    }
    error_page   500 502 503 504  /50x.html;
    location = /50x.html {
        root   /usr/share/nginx/html;
    }
}
```
4. Go to `Dockerfile` and add below line, after assigning the build-env:
```
COPY nginx/nginx.conf /etc/nginx/conf.d/default.conf
```
```
FROM nginx:alpine
COPY --from=build-env /app/build /usr/share/nginx/html

# here
COPY nginx/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
```