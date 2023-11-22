FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5100

ENV ASPNETCORE_URLS=http://+:5100

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["SKD.Server/SKD.Server.csproj", "SKD.Server/"]
RUN dotnet restore "SKD.Server/SKD.Server.csproj"
COPY . .
WORKDIR "/src/SKD.Server"
RUN dotnet build "SKD.Server.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "SKD.Server.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SKD.Server.dll"]
