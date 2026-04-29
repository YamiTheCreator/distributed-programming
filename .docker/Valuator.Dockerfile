# Используем официальный образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Копируем файлы проекта
COPY ["Valuator/Valuator.csproj", "Valuator/"]
COPY ["ValuatorLib/ValuatorLib.csproj", "ValuatorLib/"]

# Восстанавливаем зависимости
RUN dotnet restore "Valuator/Valuator.csproj"

# Копируем весь исходный код
COPY . .

# Собираем приложение
WORKDIR "/src/Valuator"
RUN dotnet build "Valuator.csproj" -c Release -o /app/build

# Публикуем приложение
FROM build AS publish
RUN dotnet publish "Valuator.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Используем runtime образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://+:5020

# Открываем порт
EXPOSE 5020

ENTRYPOINT ["dotnet", "Valuator.dll"]