FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ChatServer/ChatServer.csproj", "ChatServer/"]
RUN dotnet restore "ChatServer/ChatServer.csproj"
COPY . .
WORKDIR "/src/ChatServer"
RUN dotnet build "ChatServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatServer.csproj" -c Release -o /app/publish

FROM base AS final
# These ports are set in ChatProtocol
EXPOSE 24523
EXPOSE 24524

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatServer.dll"]
