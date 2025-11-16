# =========================
# Stage 1: Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia csproj e restaura dependências (cacheável)
COPY *.csproj ./
RUN dotnet restore

# Copia todo o código
COPY . ./

# Build e publish para produção
RUN dotnet publish -c Release -o /app/publish

# =========================
# Stage 2: Runtime (prod)
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80

# Copia do build
COPY --from=build /app/publish ./

# Variáveis padrão
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "UserService.dll"]