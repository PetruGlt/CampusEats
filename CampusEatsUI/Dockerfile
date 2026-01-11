FROM nginx:alpine as final

RUN rm -rf /usr/share/nginx/html/*
WORKDIR /usr/share/nginx/html/
COPY --from=publish ../out/wwwroot/ .
EXPOSE 80

