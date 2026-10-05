# Build context is the repository root (see docker-compose.yml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, so this layer is cached until a project file changes.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Movies.Domain/Movies.Domain.csproj src/Movies.Domain/
COPY src/Movies.Application/Movies.Application.csproj src/Movies.Application/
COPY src/Movies.Infrastructure/Movies.Infrastructure.csproj src/Movies.Infrastructure/
COPY src/Movies.Api/Movies.Api.csproj src/Movies.Api/
RUN dotnet restore src/Movies.Api/Movies.Api.csproj

COPY src/ src/
COPY data/ data/
# chmod: make sure the non-root user below can read everything, whatever the permissions were in the checkout.
RUN dotnet publish src/Movies.Api/Movies.Api.csproj --configuration Release --no-restore --output /app \
    && chmod -R a+rX /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Run as the image's built-in non-root user.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Movies.Api.dll"]
