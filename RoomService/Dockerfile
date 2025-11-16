# -----------------------------
# Stage 1: Build
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos do csproj e restaura dependências
COPY *.csproj ./
RUN dotnet restore

# Copia todo o código
COPY . ./

# Build para produção (Release) por padrão
ARG DOTNET_ENVIRONMENT=Production
RUN if [ "$DOTNET_ENVIRONMENT" = "Production" ]; then \
        dotnet publish -c Release -o /app/publish ; \
    else \
        echo "Development build, skip publish" ; \
    fi

# -----------------------------
# Stage 2: Runtime
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Variáveis de ambiente padrão
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://+:80

# Copia arquivos da build para produção
ARG DOTNET_ENVIRONMENT=Production
COPY --from=build /app/publish ./

# Para dev: copia todo o código para hot reload
COPY --from=build /src ./

# Porta exposta
EXPOSE 80

# Comando padrão para rodar
ARG DOTNET_ENVIRONMENT=Production
CMD if [ "$DOTNET_ENVIRONMENT" = "Production" ]; then \
        dotnet RoomService.dll ; \
    else \
        dotnet watch run --urls=http://+:80 ; \
    fi