#!/bin/bash

echo "🚀 Запуск системы Valuator..."

# Переходим в корневую директорию проекта
cd "$(dirname "$0")/.." || exit 1

# Останавливаем и удаляем старые контейнеры с явным указанием project name
echo "🛑 Остановка старых контейнеров..."
docker-compose -f .docker/docker-compose.yaml -p valuator down --remove-orphans

# Собираем и запускаем все сервисы с явным указанием project name
echo "🔨 Сборка и запуск сервисов..."
docker-compose -f .docker/docker-compose.yaml -p valuator up -d --build --remove-orphans --quiet-pull

echo "⏳ Ожидание готовности сервисов..."
sleep 15

echo "📊 Проверка статуса сервисов..."
docker-compose -f .docker/docker-compose.yaml -p valuator ps

echo ""
echo "✅ Система запущена!"
echo ""
echo "🌐 Веб-приложение: http://localhost:8080"
echo "🐰 RabbitMQ UI: http://localhost:15672 (guest/guest)"
echo ""
echo "📝 Просмотр логов:"
echo "  docker-compose -f .docker/docker-compose.yaml -p valuator logs -f [service-name]"
echo ""
echo "🛑 Остановка: ./scripts/stop.sh"
echo "🧹 Полная очистка: ./scripts/cleanup.sh"
