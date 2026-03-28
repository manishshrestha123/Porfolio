# Build context must be the solution root (Portfolio folder), not this folder:
#   docker build -f src/WebApiHost/Dockerfile .

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Porfolio.sln ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/WebApiHost/WebApiHost.csproj src/WebApiHost/

RUN dotnet restore src/WebApiHost/WebApiHost.csproj

COPY src/ ./src/

RUN dotnet publish src/WebApiHost/WebApiHost.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "WebApiHost.dll"]
