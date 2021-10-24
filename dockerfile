FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Build
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src

# Copy project files
COPY PeterPedia.sln .
COPY src/Client.Book/*.csproj ./src/Client.Book/
COPY src/Client.Bookmark/*.csproj ./src/Client.Bookmark/
COPY src/Client.Episodes/*.csproj ./src/Client.Episodes/
COPY src/Client.Movie/*.csproj ./src/Client.Movie/
COPY src/Client.Reader/*.csproj ./src/Client.Reader/
COPY src/Server/*.csproj ./src/Server/
COPY src/Shared/*.csproj ./src/Shared/

# Copy test projects
COPY tests/PeterPedia.Tests/*.csproj ./tests/PeterPedia.Tests/

# Restore
RUN dotnet restore

# Copy code
COPY ./tests ./tests
COPY ./src ./src

# Build
RUN dotnet build -c Release --no-restore

# Test
RUN dotnet test --no-restore

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Create final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PeterPedia.Server.dll"]