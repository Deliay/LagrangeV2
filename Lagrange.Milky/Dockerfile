﻿FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt update && apt install -y busybox && busybox --install
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV DOCKER=true
WORKDIR /src
COPY ["Lagrange.Milky/Lagrange.Milky.csproj", "Lagrange.Milky/"]
COPY ["Lagrange.Codec/Lagrange.Codec.csproj", "Lagrange.Codec/"]
COPY ["Lagrange.Core/Lagrange.Core.csproj", "Lagrange.Core/"]
COPY ["Lagrange.Proto.Generator/Lagrange.Proto.Generator.csproj", "Lagrange.Proto.Generator/"]
COPY ["Lagrange.Proto/Lagrange.Proto.csproj", "Lagrange.Proto/"]
COPY ["Lagrange.Milky.Implementation.Api.Generator/Lagrange.Milky.Implementation.Api.Generator.csproj", "Lagrange.Milky.Implementation.Api.Generator/"]
RUN dotnet restore "Lagrange.Milky/Lagrange.Milky.csproj"
COPY . .
WORKDIR "/src/Lagrange.Milky"
RUN dotnet build "./Lagrange.Milky.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lagrange.Milky.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV DOCKER=true
COPY --from=publish /app/publish .
WORKDIR /data
ENTRYPOINT ["dotnet", "/app/Lagrange.Milky.dll"]
