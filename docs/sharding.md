# Сегментирование данных (Sharding)

## Обзор

Система использует **стратегию поиска (Shard Map)** для сегментирования данных по регионам.

## Архитектура

```
┌─────────────────────────────────────────────┐
│  Main Redis (localhost:6379)                │
│  Shard Map: ID → Region                     │
│  - shardmap:{id} → "RU" | "EU" | "ASIA"    │
└─────────────────────────────────────────────┘
                    │
        ┌───────────┼───────────┐
        ▼           ▼           ▼
┌──────────┐  ┌──────────┐  ┌──────────┐
│ Redis-RU │  │ Redis-EU │  │Redis-ASIA│
│ :6380    │  │ :6381    │  │ :6382    │
│          │  │          │  │          │
│ Russia   │  │ France   │  │  UAE     │
│          │  │ Germany  │  │  India   │
└──────────┘  └──────────┘  └──────────┘
```

## Страны и регионы

| Страна  | Код | Регион | Redis Port |
|---------|-----|--------|------------|
| Russia  | RU  | RU     | 6380       |
| France  | FR  | EU     | 6381       |
| Germany | DE  | EU     | 6381       |
| UAE     | AE  | ASIA   | 6382       |
| India   | IN  | ASIA   | 6382       |

## Стратегия сегментирования

**Shard Map (Стратегия поиска)**

1. **Выбор региона**: Страна пользователя определяет регион
2. **Сохранение маппинга**: `Main Redis` хранит `ID → Region`
3. **Сохранение данных**: Данные сохраняются в Redis сегмента
4. **Чтение данных**: 
   - Получаем регион из Main Redis
   - Читаем данные из Redis сегмента

## Переменные окружения

```bash
# Main Redis (Shard Map)
ConnectionStrings__Redis=redis:6379

# Regional Shards
ConnectionStrings__RedisRU=redis-ru:6379
ConnectionStrings__RedisEU=redis-eu:6379
ConnectionStrings__RedisASIA=redis-asia:6379
```

## Логирование

Каждый компонент, обращающийся к данным по ID, логирует:

```
LOOKUP: {ID}, {Region}
```

Пример:
```
LOOKUP: 9B2FA05D-C54D-427F-B443-2B912D96AAA6, RU
```

## Структура данных в сегментах

### Main Redis (6379)
Хранит только карту сегментирования:
```
shardmap:{guid} → "RU" | "EU" | "ASIA"
```

Пример:
```
shardmap:9b2fa05d-c54d-427f-b443-2b912d96aaa6 → "RU"
```

### Региональные Redis (6380, 6381, 6382)
Каждый сегмент хранит:

1. **Текст анализа**
   ```
   text:{guid} → "текст для анализа"
   ```

2. **Оценка содержания (Rank)**
   ```
   rank:{guid} → 0.85
   ```

3. **Показатель плагиата (Similarity)**
   ```
   similarity:{guid} → 0.0
   ```

4. **Множество уникальных текстов региона**
   ```
   textset:{region} → Set["текст1", "текст2", ...]
   ```

### Пример данных

**Main Redis:**
```
shardmap:123e4567-e89b-12d3-a456-426614174000 → "RU"
shardmap:223e4567-e89b-12d3-a456-426614174001 → "EU"
shardmap:323e4567-e89b-12d3-a456-426614174002 → "ASIA"
```

**Redis-RU (6380):**
```
text:123e4567-e89b-12d3-a456-426614174000 → "Пример текста"
rank:123e4567-e89b-12d3-a456-426614174000 → 0.75
similarity:123e4567-e89b-12d3-a456-426614174000 → 0.0
textset:RU → {"Пример текста", "Другой текст", ...}
```

**Redis-EU (6381):**
```
text:223e4567-e89b-12d3-a456-426614174001 → "Sample text"
rank:223e4567-e89b-12d3-a456-426614174001 → 0.82
similarity:223e4567-e89b-12d3-a456-426614174001 → 1.0
textset:EU → {"Sample text", "Another text", ...}
```

## Компоненты

### 1. ShardMapManager (`ValuatorLib/Services/ShardMapManager.cs`)
**Назначение**: Управление картой сегментирования

**Методы**:
- `SaveShardKeyAsync(AnalysisId id, ShardKey shardKey)` - Сохраняет маппинг ID → Region в Main Redis
- `GetShardKeyAsync(AnalysisId id)` - Получает регион по ID из Main Redis
- Логирует `LOOKUP: {id}, {region}` при каждом чтении

**Пример использования**:
```csharp
// Сохранение
await shardMapManager.SaveShardKeyAsync(id, ShardKey.FromRegion(Region.RU));

// Чтение
ShardKey? shardKey = await shardMapManager.GetShardKeyAsync(id);
// Лог: LOOKUP: 9b2fa05d-c54d-427f-b443-2b912d96aaa6, RU
```

### 2. ShardedValuatorRepository (`ValuatorLib/Repositories/ShardedValuatorRepository.cs`)
**Назначение**: Абстракция доступа к данным в сегментах

**Зависимости**:
- `IConnectionMultiplexer` - Main Redis
- `Dictionary<Region, IConnectionMultiplexer>` - Региональные Redis
- `IShardMapManager` - Для определения региона

**Методы**:
- `SaveTextAsync(AnalysisId id, string text, ShardKey shardKey)` - Сохраняет текст в сегмент
- `GetTextAsync(AnalysisId id)` - Читает текст из сегмента
- `SaveRankAsync(AnalysisId id, double rank)` - Сохраняет оценку
- `GetRankAsync(AnalysisId id)` - Читает оценку
- `SaveSimilarityAsync(AnalysisId id, double similarity)` - Сохраняет показатель плагиата
- `GetSimilarityAsync(AnalysisId id)` - Читает показатель плагиата
- `TextExistsAsync(string text, ShardKey shardKey)` - Проверяет наличие текста в регионе
- `AddTextToSetAsync(string text, ShardKey shardKey)` - Добавляет текст в множество региона
- `GetUniqueTextsCountAsync(ShardKey shardKey)` - Количество уникальных текстов в регионе

**Логика работы**:
```csharp
// При чтении данных:
1. Получить ShardKey через ShardMapManager (логирует LOOKUP)
2. Выбрать соответствующий Redis из словаря
3. Прочитать данные из выбранного Redis
```

### 3. Valuator Service (`Valuator/Services/ValuatorService.cs`)
**Назначение**: Обработка текста и определение региона

**Процесс обработки**:
```csharp
1. Получить страну из запроса (CountryCode)
2. Определить регион: Country → Region
3. Создать ShardKey из региона
4. Сохранить маппинг в Main Redis через ShardMapManager
5. Сохранить текст в региональный Redis
6. Проверить плагиат в пределах региона
7. Отправить задание на вычисление ранга
```

**Важно**: Проверка плагиата происходит **только в пределах региона**:
```csharp
// Проверяем схожесть в пределах региона
double similarity = await repository.TextExistsAsync(request.Text, shardKey) ? 1.0 : 0.0;

if (similarity == 0.0)
{
    await repository.AddTextToSetAsync(request.Text, shardKey);
}
```

### 4. RankCalculator (`RankCalculator/Consumers/RankCalculationConsumer.cs`)
**Назначение**: Вычисление оценки содержания

**Процесс**:
```csharp
1. Получить ID из сообщения RabbitMQ
2. Прочитать текст через repository (автоматически определяет регион)
3. Вычислить ранг
4. Сохранить ранг в тот же сегмент
5. Опубликовать событие RankCalculatedEvent
```

**Логирование**: Repository автоматически логирует LOOKUP при чтении текста

### 5. NotificationService (`NotificationService/Consumers/RankCalculatedEventConsumer.cs`)
**Назначение**: Уведомление клиентов через SignalR

**Процесс**:
```csharp
1. Получить событие RankCalculatedEvent
2. Преобразовать AnalysisId в строку
3. Отправить уведомление в SignalR группу
```

**Важно**: ID преобразуется в строку для совместимости с JavaScript клиентом

## Поток данных

### Сохранение текста (детальный процесс)

```
┌─────────┐
│  User   │ Выбирает страну: Russia
└────┬────┘
     │
     ▼
┌─────────────────┐
│  Application    │ POST /api/textanalysis
│  (UI)           │ { text: "...", countryCode: "RU" }
└────┬────────────┘
     │
     ▼
┌─────────────────┐
│  Gateway        │ Проксирует запрос
│  (YARP)         │
└────┬────────────┘
     │
     ▼
┌─────────────────────────────────────────┐
│  Valuator Service                       │
│  1. Countries.GetByCode("RU")           │
│     → Country(RU, "Russia", Region.RU)  │
│  2. ShardKey.FromCountry(country)       │
│     → ShardKey(Region.RU)               │
│  3. AnalysisId.New()                    │
│     → 9b2fa05d-c54d-427f-b443-...       │
└────┬────────────────────────────────────┘
     │
     ├─────────────────────────────────────┐
     │                                     │
     ▼                                     ▼
┌──────────────────────┐      ┌──────────────────────┐
│  ShardMapManager     │      │ ShardedRepository    │
│  SaveShardKeyAsync() │      │ SaveTextAsync()      │
│                      │      │                      │
│  Main Redis (6379)   │      │  Redis-RU (6380)     │
│  shardmap:{id}→"RU"  │      │  text:{id}→"текст"   │
└──────────────────────┘      └──────────────────────┘
     │
     ▼
┌──────────────────────┐
│  RabbitMQ            │
│  RankCalculation     │
│  Message             │
└────┬─────────────────┘
     │
     ▼
┌─────────────────────────────────────────┐
│  RankCalculator Consumer                │
│  1. GetTextAsync(id)                    │
│     → ShardMapManager.GetShardKey(id)   │
│     → LOOKUP: {id}, RU                  │
│     → Read from Redis-RU                │
│  2. CalculateRankAsync(text)            │
│  3. SaveRankAsync(id, rank)             │
│     → Save to Redis-RU                  │
└────┬────────────────────────────────────┘
     │
     ▼
┌──────────────────────┐
│  RabbitMQ            │
│  RankCalculated      │
│  Event               │
└────┬─────────────────┘
     │
     ├─────────────────────────────────────┐
     │                                     │
     ▼                                     ▼
┌──────────────────────┐      ┌──────────────────────┐
│  EventsLogger        │      │ NotificationService  │
│  Логирует событие    │      │ SignalR Hub          │
└──────────────────────┘      └────┬─────────────────┘
                                   │
                                   ▼
                              ┌──────────────────────┐
                              │  Browser (SignalR)   │
                              │  Обновляет UI        │
                              └──────────────────────┘
```

### Чтение данных (детальный процесс)

```
┌─────────┐
│  User   │ Открывает /Summary?id={guid}
└────┬────┘
     │
     ▼
┌─────────────────┐
│  Application    │ GET /api/textanalysis/{id}
│  (UI)           │
└────┬────────────┘
     │
     ▼
┌─────────────────┐
│  Gateway        │ Проксирует запрос
│  (YARP)         │
└────┬────────────┘
     │
     ▼
┌─────────────────────────────────────────┐
│  Valuator Service                       │
│  GetAnalysisResultAsync(id)             │
└────┬────────────────────────────────────┘
     │
     ▼
┌─────────────────────────────────────────┐
│  ShardedRepository                      │
│  1. GetRankAsync(id)                    │
│     ├─ ShardMapManager.GetShardKey(id)  │
│     │  ├─ Read: shardmap:{id} from Main │
│     │  └─ LOOKUP: {id}, RU              │
│     └─ Read: rank:{id} from Redis-RU    │
│                                         │
│  2. GetSimilarityAsync(id)              │
│     ├─ ShardMapManager.GetShardKey(id)  │
│     │  └─ LOOKUP: {id}, RU              │
│     └─ Read: similarity:{id} from RU    │
└─────────────────────────────────────────┘
```

## Проверка плагиата (Similarity)

### Региональная изоляция

Проверка плагиата работает **только в пределах региона**:

```
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  Redis-RU    │  │  Redis-EU    │  │  Redis-ASIA  │
│              │  │              │  │              │
│ textset:RU   │  │ textset:EU   │  │ textset:ASIA │
│ ├─ "текст1"  │  │ ├─ "text1"   │  │ ├─ "text1"   │
│ ├─ "текст2"  │  │ ├─ "text2"   │  │ ├─ "text2"   │
│ └─ "текст3"  │  │ └─ "text3"   │  │ └─ "text3"   │
└──────────────┘  └──────────────┘  └──────────────┘
      ↑                 ↑                 ↑
      │                 │                 │
  Россия          Франция, Германия   ОАЭ, Индия
```

### Алгоритм проверки

```csharp
// В ValuatorService.ProcessTextAsync()

// 1. Проверяем существование текста в региональном множестве
bool exists = await repository.TextExistsAsync(request.Text, shardKey);
double similarity = exists ? 1.0 : 0.0;

// 2. Если текст новый - добавляем в множество
if (similarity == 0.0)
{
    await repository.AddTextToSetAsync(request.Text, shardKey);
}

// 3. Сохраняем результат
await repository.SaveSimilarityAsync(id, similarity);
```

### Примеры

**Сценарий 1: Новый текст**
```
Пользователь из России отправляет: "Уникальный текст"
→ Проверка в textset:RU
→ Текст не найден
→ similarity = 0.0 (нет плагиата)
→ Добавляем в textset:RU
```

**Сценарий 2: Дубликат в том же регионе**
```
Другой пользователь из России отправляет: "Уникальный текст"
→ Проверка в textset:RU
→ Текст найден!
→ similarity = 1.0 (100% плагиат)
→ НЕ добавляем в textset:RU (уже есть)
```

**Сценарий 3: Тот же текст в другом регионе**
```
Пользователь из Франции отправляет: "Уникальный текст"
→ Проверка в textset:EU
→ Текст не найден (в EU его нет)
→ similarity = 0.0 (нет плагиата в EU регионе)
→ Добавляем в textset:EU
```

**Важно**: Один и тот же текст может существовать в разных регионах и считаться уникальным в каждом из них!

## Преимущества

1. **Масштабируемость**: Каждый регион на отдельном сервере
2. **Производительность**: Данные распределены по сегментам
3. **Региональность**: Данные хранятся в соответствующем регионе
4. **Простота**: Фиксированное количество регионов
5. **Гибкость**: Легко добавить новые регионы

## Ограничения

1. **Нельзя изменить регион после создания записи** - ID навсегда привязан к региону
2. **Схожесть проверяется только в пределах региона** - один текст может быть уникальным в разных регионах
3. **Требуется дополнительный запрос к Main Redis** для определения региона при каждом чтении
4. **Нет автоматической репликации** между регионами
5. **Фиксированное количество регионов** - добавление нового региона требует изменения кода

## Мониторинг и отладка

### Логи LOOKUP

Каждый компонент логирует обращение к данным:

```
[ShardMapManager] LOOKUP: 9b2fa05d-c54d-427f-b443-2b912d96aaa6, RU
[ShardedRepository] Saved text to shard RU: 9b2fa05d-c54d-427f-b443-2b912d96aaa6
[ShardedRepository] Saved rank to shard RU: 9b2fa05d-c54d-427f-b443-2b912d96aaa6
```

### Проверка данных в Redis

**Main Redis (6379):**
```bash
docker exec -it redis redis-cli
> KEYS shardmap:*
> GET shardmap:9b2fa05d-c54d-427f-b443-2b912d96aaa6
"RU"
```

**Regional Redis (6380):**
```bash
docker exec -it redis-ru redis-cli
> KEYS text:*
> GET text:9b2fa05d-c54d-427f-b443-2b912d96aaa6
"Пример текста"
> GET rank:9b2fa05d-c54d-427f-b443-2b912d96aaa6
"0.75"
> SMEMBERS textset:RU
1) "Пример текста"
2) "Другой текст"
```

### Статистика по регионам

```bash
# Количество записей в каждом регионе
docker exec -it redis-ru redis-cli SCARD textset:RU
docker exec -it redis-eu redis-cli SCARD textset:EU
docker exec -it redis-asia redis-cli SCARD textset:ASIA
```

## Масштабирование

### Текущая конфигурация

```yaml
# docker-compose.yaml
redis:      # Main - Shard Map
  ports: ["6379:6379"]
  
redis-ru:   # Russia
  ports: ["6380:6379"]
  
redis-eu:   # Europe
  ports: ["6381:6379"]
  
redis-asia: # Asia
  ports: ["6382:6379"]
```

### Добавление нового региона

1. **Добавить enum в код**:
```csharp
// ValuatorLib/Models/Country.cs
public enum Region
{
    RU,
    EU,
    ASIA,
    AMERICAS  // Новый регион
}
```

2. **Добавить страны**:
```csharp
public static readonly Country USA = new("US", "USA", Region.AMERICAS);
public static readonly Country Canada = new("CA", "Canada", Region.AMERICAS);
```

3. **Добавить Redis в docker-compose**:
```yaml
redis-americas:
  image: redis:alpine
  ports: ["6383:6379"]
```

4. **Добавить connection string**:
```bash
ConnectionStrings__RedisAMERICAS=redis-americas:6379
```

5. **Обновить конфигурацию сервисов**:
```csharp
// ServiceCollectionExtensions.cs
string redisAmericas = configuration.GetConnectionString("RedisAMERICAS") ?? "localhost:6383";
shardedRedis[Region.AMERICAS] = ConnectionMultiplexer.Connect(redisAmericas);
```

### Вертикальное масштабирование

Для увеличения производительности отдельного региона:

1. **Увеличить ресурсы Redis**:
```yaml
redis-ru:
  image: redis:alpine
  command: redis-server --maxmemory 2gb --maxmemory-policy allkeys-lru
  deploy:
    resources:
      limits:
        memory: 2G
```

2. **Использовать Redis Cluster** для горизонтального масштабирования внутри региона

### Горизонтальное масштабирование сервисов

Текущая конфигурация уже поддерживает несколько инстансов:
- 2x Valuator
- 2x RankCalculator
- 2x NotificationService
- 2x EventsLogger
- 2x Application

Все инстансы работают с одними и теми же Redis сегментами.

## Альтернативные стратегии

### Стратегия диапазонов
- Не подходит: нет числового ключа для разбиения

### Стратегия хеширования
- Не подходит: нужна привязка к региону, а не равномерное распределение

### Shard Map (выбрана)
- ✅ Подходит: фиксированные регионы, явная привязка к стране
