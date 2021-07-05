FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ChatConsoleClient/ChatConsoleClient.csproj", "ChatConsoleClient/"]
RUN dotnet restore "ChatConsoleClient/ChatConsoleClient.csproj"
COPY . .
WORKDIR "/src/ChatConsoleClient"
RUN dotnet build "ChatConsoleClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatConsoleClient.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatConsoleClient.dll"]
