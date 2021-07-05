FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ChatWebClient/ChatWebClient.csproj", "ChatWebClient/"]
RUN dotnet restore "ChatWebClient/ChatWebClient.csproj"
COPY . .
WORKDIR "/src/ChatWebClient"
RUN dotnet build "ChatWebClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatWebClient.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatWebClient.dll"]
