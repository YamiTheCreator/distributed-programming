#!/bin/bash

echo "Остановка системы Valuator..."

# Переходим в директорию с docker-compose
cd "$(dirname "$0")/../.docker" || exit 1

# Останавливаем контейнеры
docker-compose down

echo "Система остановлена!"