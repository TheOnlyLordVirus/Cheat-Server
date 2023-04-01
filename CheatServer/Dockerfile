#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CheatServer/CheatServer.csproj", "CheatServer/"]
RUN dotnet restore "CheatServer/CheatServer.csproj"
COPY . .
WORKDIR "/src/CheatServer"
RUN dotnet build "CheatServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CheatServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=https://+:5000
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CheatServer.dll"]