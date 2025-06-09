FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM node:20 AS client-build
WORKDIR /src
COPY ["Jude.Client/package.json", "Jude.Client/"]
COPY ["Jude.Client/bun.lock", "Jude.Client/"]
WORKDIR /src/Jude.Client
RUN npm install -g bun
RUN bun install
COPY ["Jude.Client", "."]
RUN bun run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Jude.Server/Jude.Server.csproj", "Jude.Server/"]
RUN dotnet restore "Jude.Server/Jude.Server.csproj"
COPY . .
WORKDIR "/src/Jude.Server"
RUN dotnet build "Jude.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Jude.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

COPY --from=client-build /src/Jude.Client/dist /app/publish/wwwroot

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jude.Server.dll"]