FROM momar/web AS base

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ChatConsoleClient/ChatConsoleClient.csproj", "ChatConsoleClient/"]
RUN dotnet restore "ChatConsoleClient/ChatConsoleClient.csproj"
COPY . .
WORKDIR "/src/ChatConsoleClient"
RUN dotnet build "ChatConsoleClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatConsoleClient.csproj" -c Release -o /app/publish
RUN tar -zcvf /app/everything.tar.gz /app/publish/
RUN cp /app/everything.tar.gz /app/publish/

FROM base AS final
EXPOSE 80
WORKDIR /var/www
ENV ENABLE_INDEX=1
ENV ENABLE_HIDDEN=1
ENV ENABLE_COMPRESSION=1
ENV ENABLE_CORS=1
COPY --from=publish /app/publish /var/www
