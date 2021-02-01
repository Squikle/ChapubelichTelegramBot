FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim-arm64v8 AS base
WORKDIR /app

ENV TZ=Europe/Kiev

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Chapubelich/Chapubelich.csproj", "Chapubelich/"]
RUN dotnet restore "Chapubelich/Chapubelich.csproj"
COPY . .
WORKDIR "/src/Chapubelich"
RUN dotnet build "Chapubelich.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chapubelich.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChapubelichBot.dll"]