# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln ./
COPY WebApiHost/*.csproj ./WebApiHost/
COPY Application/*.csproj ./Application/
COPY Domain/*.csproj ./Domain/
COPY Infrastructure/*.csproj ./Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . ./

# Publish the API project
RUN dotnet publish WebApiHost/WebApiHost.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/out .

# Expose the port your app listens on (default 80 or 8080)
EXPOSE 8080

# Run the API DLL
ENTRYPOINT ["dotnet", "WebApiHost.dll"]