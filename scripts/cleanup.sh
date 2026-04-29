#!/bin/bash

echo "🧹 Полная очистка Docker контейнеров и образов..."

# Останавливаем все контейнеры проекта
echo "Остановка всех контейнеров valuator..."
docker ps -a | grep -E "valuator|application|gateway|notification|rank-calculator|events-logger|redis|rabbitmq" | awk '{print $1}' | xargs -r docker stop

# Удаляем все контейнеры проекта
echo "Удаление всех контейнеров valuator..."
docker ps -a | grep -E "valuator|application|gateway|notification|rank-calculator|events-logger|redis|rabbitmq" | awk '{print $1}' | xargs -r docker rm

# Удаляем образы проекта
echo "Удаление образов valuator..."
docker images | grep valuator | awk '{print $3}' | xargs -r docker rmi -f

# Удаляем неиспользуемые сети
echo "Очистка неиспользуемых сетей..."
docker network prune -f

# Удаляем неиспользуемые volumes
echo "Очистка неиспользуемых volumes..."
docker volume prune -f

echo "✅ Очистка завершена!"
echo ""
echo "Теперь можно запустить систему заново: ./scripts/start.sh"
