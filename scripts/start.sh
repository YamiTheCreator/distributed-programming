#!/bin/bash

# Переходим в директорию с docker-compose
cd "$(dirname "$0")/../.docker" || exit 1

# Останавливаем старые контейнеры если они есть
docker-compose down 2>/dev/null

# Собираем и запускаем контейнеры
docker-compose up --build -d

# Проверяем статус
sleep 5
docker-compose ps
