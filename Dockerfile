# Build from repository root (where Porfolio.sln lives):
#   docker build .
#
# In Railway / Render / etc.: set "Root Directory" to the repo root (leave empty or "."),
# not src/WebApiHost.

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
