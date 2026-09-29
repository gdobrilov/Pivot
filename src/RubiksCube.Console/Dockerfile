# Build context is the repository root:
#   docker build -f src/RubiksCube.Console/Dockerfile -t rubiks-console .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props global.json .editorconfig ./
COPY src/RubiksCube.Domain/RubiksCube.Domain.csproj src/RubiksCube.Domain/
COPY src/RubiksCube.Application/RubiksCube.Application.csproj src/RubiksCube.Application/
COPY src/RubiksCube.Console/RubiksCube.Console.csproj src/RubiksCube.Console/
RUN dotnet restore src/RubiksCube.Console/RubiksCube.Console.csproj
COPY src/RubiksCube.Domain src/RubiksCube.Domain
COPY src/RubiksCube.Application src/RubiksCube.Application
COPY src/RubiksCube.Console src/RubiksCube.Console
RUN dotnet publish src/RubiksCube.Console/RubiksCube.Console.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "rubiks.dll"]
