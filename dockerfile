FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Install mediainfo
RUN apt-get update && apt-get install -y mediainfo

# Build
FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src

# Copy project files
COPY PeterPedia.sln .
COPY src/PeterPedia/*.csproj ./src/PeterPedia/

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
ENTRYPOINT ["dotnet", "PeterPedia.dll"]