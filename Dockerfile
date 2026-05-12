# Multi-stage build: compile Blazor WASM with wasm-tools, then serve static files via Nginx
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# WebAssembly AOT toolchain requires python to be available in PATH.
RUN apt-get update \
    && apt-get install -y --no-install-recommends python3 \
    && ln -sf /usr/bin/python3 /usr/bin/python \
    && rm -rf /var/lib/apt/lists/*

# Copy project files for dependency resolution
COPY src/Shared/NicolasQuiPaieData/NicolasQuiPaieData.csproj src/Shared/NicolasQuiPaieData/
COPY src/Front/NicolasQuiPaieWeb/NicolasQuiPaieWeb.csproj src/Front/NicolasQuiPaieWeb/

# Restore dependencies
RUN dotnet restore src/Front/NicolasQuiPaieWeb/NicolasQuiPaieWeb.csproj

# Copy remaining source and install wasm-tools
COPY . .
RUN dotnet workload install wasm-tools --skip-manifest-update

# Publish Blazor WASM
RUN dotnet publish src/Front/NicolasQuiPaieWeb/NicolasQuiPaieWeb.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Stage 2: Serve static files with Nginx
FROM nginx:1.27-alpine AS final
WORKDIR /usr/share/nginx/html

# Copy compiled Blazor WASM wwwroot from build stage
COPY --from=build /app/publish/wwwroot ./
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 10000
CMD ["nginx", "-g", "daemon off;"]
