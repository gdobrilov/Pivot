# Build context is the repository root:
#   docker build -f src/RubiksCube.Api/Dockerfile -t rubiks-api .
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/RubiksCube.Domain/RubiksCube.Domain.csproj src/RubiksCube.Domain/
COPY src/RubiksCube.Application/RubiksCube.Application.csproj src/RubiksCube.Application/
COPY src/RubiksCube.Infrastructure/RubiksCube.Infrastructure.csproj src/RubiksCube.Infrastructure/
COPY src/RubiksCube.Api/RubiksCube.Api.csproj src/RubiksCube.Api/
RUN dotnet restore src/RubiksCube.Api/RubiksCube.Api.csproj
COPY src/RubiksCube.Domain src/RubiksCube.Domain
COPY src/RubiksCube.Application src/RubiksCube.Application
COPY src/RubiksCube.Infrastructure src/RubiksCube.Infrastructure
COPY src/RubiksCube.Api src/RubiksCube.Api
RUN dotnet publish src/RubiksCube.Api/RubiksCube.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
# SQLite database lives in a volume-friendly directory owned by the non-root app user.
RUN mkdir -p /app/data && chown $APP_UID /app/data
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__Rubiks="Data Source=/app/data/rubiks.db"
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "RubiksCube.Api.dll"]
