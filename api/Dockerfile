#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install --global dotnet-ef
RUN dotnet dev-certs https -t
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["api/JogandoBack.API.csproj", "api/"]
RUN dotnet restore "api/JogandoBack.API.csproj"
COPY . .
WORKDIR "/src/api"
RUN dotnet build "JogandoBack.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JogandoBack.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JogandoBack.API.dll"]
