FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости
COPY ["Valuator/Valuator.csproj", "Valuator/"]
RUN dotnet restore "Valuator/Valuator.csproj"

# Копируем остальные файлы и собираем приложение
COPY Valuator/ Valuator/
WORKDIR "/src/Valuator"
RUN dotnet build "Valuator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Valuator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Valuator.dll"]
