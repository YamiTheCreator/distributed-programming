FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Копируем файлы проектов и восстанавливаем зависимости
COPY ["ValuatorLib/ValuatorLib.csproj", "ValuatorLib/"]
COPY ["RankCalculator/RankCalculator.csproj", "RankCalculator/"]
RUN dotnet restore "RankCalculator/RankCalculator.csproj"

# Копируем остальные файлы и собираем приложение
COPY ValuatorLib/ ValuatorLib/
COPY RankCalculator/ RankCalculator/
WORKDIR "/src/RankCalculator"
RUN dotnet build "RankCalculator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RankCalculator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RankCalculator.dll"]