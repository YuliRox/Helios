# https://hub.docker.com/_/microsoft-dotnet
ARG BASE_IMAGE_ARCH=""
FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim$BASE_IMAGE_ARCH AS base
EXPOSE 80
RUN apt-get update && apt-get install -y sqlite3 libsqlite3-dev

# Dotnet is platform agnostic anyway so only the dotnet RUNTIME needs to be arm
FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim-amd64 AS build
WORKDIR /build

# copy csproj and restore as distinct layers
COPY Backend/Helios.csproj Helios.csproj
ARG DOTNET_ARCH=x64
RUN dotnet restore "Helios.csproj" -r linux-$DOTNET_ARCH

# copy and publish app and libraries
COPY Backend .
WORKDIR /build
ARG DOTNET_ARCH=x64
RUN dotnet build "Helios.csproj" -c Release -r linux-$DOTNET_ARCH --no-restore --no-self-contained

FROM build AS publish
ARG DOTNET_ARCH=x64
RUN  dotnet publish "Helios.csproj" --no-build --no-cache -c Release -r linux-$DOTNET_ARCH --no-self-contained -o /build/publish

# final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /build/publish .
ENTRYPOINT ["dotnet", "Helios.dll"]
