# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy the solution and project files
COPY *.sln ./
COPY Porfolio.API/*.csproj ./Porfolio.API/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY Domain/*.csproj ./Domain/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . ./

# Publish
RUN dotnet publish Porfolio.API/Porfolio.API.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Porfolio.API.dll"]