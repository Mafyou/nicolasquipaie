FROM nginx:1.27-alpine AS final
WORKDIR /usr/share/nginx/html

# Serve the already-built static website output from ./dist
COPY dist/ ./
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 10000
CMD ["nginx", "-g", "daemon off;"]
