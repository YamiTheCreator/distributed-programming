# Используем официальный образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Копируем файлы проекта
COPY ["Gateway/Gateway.csproj", "Gateway/"]

# Восстанавливаем зависимости
RUN dotnet restore "Gateway/Gateway.csproj"

# Копируем весь исходный код
COPY . .

# Собираем приложение
WORKDIR "/src/Gateway"
RUN dotnet build "Gateway.csproj" -c Release -o /app/build

# Публикуем приложение
FROM build AS publish
RUN dotnet publish "Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Используем runtime образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://+:8080

# Открываем порт
EXPOSE 8080

ENTRYPOINT ["dotnet", "Gateway.dll"]