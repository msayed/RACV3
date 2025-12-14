FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["RACFlightDataService/RACFlightDataService.csproj", "RACFlightDataService/"]
RUN dotnet restore "RACFlightDataService/RACFlightDataService.csproj"
COPY . .
WORKDIR "/src/RACFlightDataService"
RUN dotnet build "RACFlightDataService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RACFlightDataService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RACFlightDataService.dll"]
