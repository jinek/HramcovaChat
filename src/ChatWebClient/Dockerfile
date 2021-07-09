FROM nginx:alpine AS base

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
EXPOSE 80
COPY --from=publish /app/publish/wwwroot /usr/share/nginx/html
