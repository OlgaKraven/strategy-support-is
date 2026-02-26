# 2. Практика. 2. Универсальная модель данных

**Проект:** strategy-support-is  
**База данных:** strategy_db (MySQL 8.0)  
**Схема:** `db/schema.sql`  
**Тестовые данные:** `seed.sql`

---

## 2.1. Сущности ядра

| Таблица | Назначение |
|---|---|
| `users` | Аккаунты пользователей: email, bcrypt-хэш пароля, никнейм, флаг бана |
| `roles` | Роли доступа: `player`, `moderator`, `admin` |
| `user_roles` | Связь M:N между `users` и `roles` |
| `game_sessions` | Игровые сессии пользователя: платформа (`unity`/`web`/`mobile`), IP, время старта/окончания |
| `game_events` | События пользователя: `event_type` + `payload_json` (тип JSON) |
| `player_progress` | Текущий прогресс: `level`, `xp`, `soft_currency`, `hard_currency` |
| `statistics_daily` | Дневные агрегаты: победы, поражения, время, сумма очков |
| `leaderboard_scores` | Рейтинги по доскам и сезонам |

---

## 2.2. Жанровые таблицы (Strategy)

| Таблица | Назначение |
|---|---|
| `matches` | Единица игрового процесса: режим (`pve`/`pvp`), карта, сезон, статус |
| `battle_results` | Итог матча 1:1 с `matches`: победа, очки, длительность, потери/убийства |
| `army_units` | Армия пользователя: тип юнита (`unit_code`), количество, боевая сила |
| `city_buildings` | Здания города: тип здания (`building_code`), уровень, статус апгрейда |
| `processed_events` | Реестр обработанных `eventId` — дедупликация повторных запросов |

---

## 2.3. Связи (кардинальности)

- `users` 1:M `user_roles`, `roles` 1:M `user_roles` — M:N через `user_roles`
- `users` 1:M `game_sessions`
- `users` 1:M `game_events`; `game_sessions` 1:M `game_events` (`session_id` может быть NULL)
- `users` 1:1 `player_progress`
- `users` 1:M `statistics_daily` — UNIQUE(user_id, day)
- `users` 1:M `leaderboard_scores` — UNIQUE(user_id, board_code, season)
- `users` 1:M `matches`
- `matches` 1:1 `battle_results`
- `users` 1:M `army_units` — UNIQUE(user_id, unit_code)
- `users` 1:M `city_buildings` — UNIQUE(user_id, building_code)
- `users` 1:M `processed_events` — UNIQUE(user_id, event_id)

---

## 2.4. Уникальности (UNIQUE)

| Таблица | Ключ |
|---|---|
| `users` | `email` |
| `roles` | `name` |
| `user_roles` | PRIMARY KEY `(user_id, role_id)` |
| `statistics_daily` | `(user_id, day)` |
| `leaderboard_scores` | `(user_id, board_code, season)` |
| `army_units` | `(user_id, unit_code)` |
| `city_buildings` | `(user_id, building_code)` |
| `processed_events` | `(user_id, event_id)` |

---

## 2.5. CHECK ограничения

| Таблица | Ограничение |
|---|---|
| `player_progress` | `level >= 1`, `xp >= 0`, `soft_currency >= 0`, `hard_currency >= 0` |
| `leaderboard_scores` | `season >= 1`, `score >= 0` |
| `statistics_daily` | все счётчики `>= 0` |
| `matches` | `season >= 1` |
| `battle_results` | `score >= 0`, `duration_seconds >= 0`, `units_lost >= 0`, `units_killed >= 0` |
| `army_units` | `amount >= 0`, `power >= 0` |
| `city_buildings` | `level >= 1` |

---

## 2.6. Примеры событий для жанра Strategy (game_events)

Все события пишутся в `game_events` через `insert_game_event()` в `events_repo.py`.
`user_id` берётся только из JWT, не из тела запроса.

**1) Начало матча**

`event_type = 'match_start'`
```json
{
  "matchId": 9,
  "mode": "pve",
  "mapCode": "coastal_siege",
  "season": 1
}
```

**2) Отправка юнитов в бой**

`event_type = 'unit_deploy'`
```json
{
  "matchId": 9,
  "unitCode": "cavalry",
  "amount": 40,
  "atSecond": 15
}
```

**3) Завершение матча (факты — без наград)**

`event_type = 'match_finish'`
```json
{
  "matchId": 9,
  "isWin": true,
  "score": 1850,
  "durationSeconds": 1200
}
```

**4) Начало улучшения здания**

`event_type = 'building_upgrade_started'`
```json
{
  "buildingCode": "barracks",
  "fromLevel": 4,
  "toLevel": 5,
  "goldCost": 500
}
```

**5) Начисление наград (пишется сервером после расчёта)**

`event_type = 'reward_granted'`
```json
{
  "matchId": 9,
  "xp": 100,
  "softCurrency": 50,
  "reason": "match_finish"
}
```

---

## 2.7. DDL (db/schema.sql)

Полный DDL находится в файле `db/schema.sql`.

Ключевые особенности реализации:

- Поле пароля называется `password` (хранит bcrypt-хэш длиной 60 символов, тип `VARCHAR(255)`)
- `game_events.payload_json` имеет тип `JSON NOT NULL` — обязательное требование для универсальности телеметрии
- `ON DELETE CASCADE` применён везде, где данные теряют смысл без пользователя
- `ON DELETE SET NULL` применён для `game_events.session_id` — событие сохраняется даже после удаления сессии
- `ON DELETE RESTRICT` применён для `user_roles.role_id` — нельзя удалить роль, пока она назначена пользователям

---

## 2.8. Проверка схемы (минимальная)

```sql
USE strategy_db;

-- 1) Создать базу и загрузить схему
SOURCE db/schema.sql;

-- 2) Загрузить тестовые данные
SOURCE seed.sql;

-- 3) Проверить пользователей
SELECT id, email, nickname, is_banned FROM users;

-- 4) Проверить лидерборд
SELECT u.nickname, ls.board_code, ls.season, ls.score
FROM leaderboard_scores ls
JOIN users u ON u.id = ls.user_id
WHERE ls.board_code = 'default' AND ls.season = 1
ORDER BY ls.score DESC;

-- 5) Проверить матч со статусом started
SELECT id, user_id, mode, map_code, status
FROM matches
WHERE status = 'started';
```

---

## 2.9. Связь с кодом

| Таблица | Репозиторий |
|---|---|
| `users` | `app/repositories/users_repo.py` |
| `roles`, `user_roles` | `app/repositories/roles_repo.py` |
| `game_events` | `app/repositories/events_repo.py` |
| `player_progress` | `app/repositories/progress_repo.py` |
| `statistics_daily` | `app/repositories/stats_repo.py` |
| `leaderboard_scores` | `app/repositories/leaderboard_repo.py` |
| `matches`, `battle_results` | `app/repositories/matches_repo.py` |
| `processed_events` | `app/repositories/dedupe_repo.py` |
