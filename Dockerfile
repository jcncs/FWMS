# Set .NET 5.0
ARG REPO=mcr.microsoft.com/dotnet
FROM $REPO/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Docker restore & build image from FWMS.csproj
FROM $REPO/sdk:5.0 AS build
ENV BuildingDocker true
WORKDIR /src
COPY ["FWMS.csproj", ""]
RUN dotnet restore "FWMS.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "FWMS.csproj" -c Release -o /app/build

# Publish csproj as release
FROM build AS publish
RUN dotnet publish "FWMS.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet FWMS.dll