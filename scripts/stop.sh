#!/bin/bash

echo "🛑 Остановка системы Valuator..."

# Переходим в корневую директорию проекта
cd "$(dirname "$0")/.." || exit 1

# Останавливаем контейнеры с явным указанием project name
docker-compose -f .docker/docker-compose.yaml -p valuator down --remove-orphans

echo "✅ Система остановлена!"
echo ""
echo "💡 Для полной очистки (удаление образов): ./scripts/cleanup.sh"