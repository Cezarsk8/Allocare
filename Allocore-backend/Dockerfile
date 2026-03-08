# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first for NuGet cache
COPY src/Allocore.Domain/Allocore.Domain.csproj src/Allocore.Domain/
COPY src/Allocore.Application/Allocore.Application.csproj src/Allocore.Application/
COPY src/Allocore.Infrastructure/Allocore.Infrastructure.csproj src/Allocore.Infrastructure/
COPY src/Allocore.API/Allocore.API.csproj src/Allocore.API/
COPY Allocore.sln .
RUN dotnet restore

# Copy everything and publish
COPY . .
RUN dotnet publish src/Allocore.API/Allocore.API.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --no-create-home appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["sh", "-c", "dotnet Allocore.API.dll --urls http://+:${PORT:-8080}"]
