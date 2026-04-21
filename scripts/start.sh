#!/bin/bash

echo "Запуск системы Valuator..."

# Переходим в директорию с docker-compose
cd "$(dirname "$0")/../.docker" || exit 1

# Останавливаем старые контейнеры если они есть
echo "Остановка старых контейнеров..."
docker-compose down

# Собираем и запускаем все сервисы
echo "Сборка и запуск сервисов..."
docker-compose up -d --build

echo "Ожидание готовности сервисов..."
sleep 15

echo "Проверка статуса сервисов..."
docker-compose ps

echo ""
echo "Система запущена!"
echo "Веб-приложение доступно по адресу: http://localhost:8080"
echo "RabbitMQ Management UI доступен по адресу: http://localhost:15672 (guest/guest)"
echo ""
echo "Для просмотра логов выполните:"
echo "  Все сервисы:        docker-compose logs -f"
echo "  EventsLogger:       docker-compose logs -f eventslogger-1 eventslogger-2"
echo "  RankCalculator:     docker-compose logs -f rankcalculator-1 rankcalculator-2"
echo "  Valuator:           docker-compose logs -f valuator-1 valuator-2"
echo ""
echo "Для остановки системы выполните: ./scripts/stop.sh"