# ========================================
# Build Stage
# ========================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore (best caching)
COPY ["My_Microservices.csproj", "./"]
RUN dotnet restore "My_Microservices.csproj"

# Copy everything else
COPY . .

# Publish
WORKDIR "/src"
RUN dotnet publish "My_Microservices.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ========================================
# Runtime Stage
# ========================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "My_Microservices.dll"]