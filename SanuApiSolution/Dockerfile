# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar la solución y proyectos primero (para aprovechar la cache en restore)
COPY SanuApiSolution/SanuApi.sln ./
COPY SanuApiSolution/SanuApi.Api/SanuApi.Api.csproj SanuApi.Api/
COPY SanuApiSolution/SanuApi.Application/SanuApi.Application.csproj SanuApi.Application/
COPY SanuApiSolution/SanuApi.Domain/SanuApi.Domain.csproj SanuApi.Domain/
COPY SanuApiSolution/SanuApi.Infrastructure/SanuApi.Infrastructure.csproj SanuApi.Infrastructure/

# Restaurar dependencias
RUN dotnet restore "SanuApi.sln"

# Copiar el resto del código
COPY SanuApiSolution/ ./

# Publicar la API
WORKDIR /src
RUN dotnet publish "SanuApi.Api/SanuApi.Api.csproj" -c Release -o /app/publish --no-restore

# Etapa final: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SanuApi.Api.dll"]
