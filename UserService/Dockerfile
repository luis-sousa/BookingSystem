# ================================
# Stage 1 — Build
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o csproj e restaura
COPY *.csproj ./
RUN dotnet restore

# Copia todo o código restante
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ================================
# Stage 2 — Runtime
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Copia apenas artefatos prontos
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "UserService.dll"]