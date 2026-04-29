FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY WokwiSimulator/WokwiSimulator.csproj WokwiSimulator/
RUN dotnet restore WokwiSimulator/WokwiSimulator.csproj

COPY WokwiSimulator/ WokwiSimulator/
RUN dotnet publish WokwiSimulator/WokwiSimulator.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WokwiSimulator.dll"]
