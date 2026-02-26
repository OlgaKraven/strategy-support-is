# 2. Практика. 3. Жанровая адаптация

**Жанр:** Strategy (пошаговая стратегия / RTS)  
**Проект:** strategy-support-is

---

## 3.1. Жанр: Strategy

Игра сочетает два режима: `pve` (рейды на карты) и `pvp` (дуэли игроков).
Прогресс выражается в развитии армии (`army_units`) и города (`city_buildings`).
Центральная единица процесса — матч.

---

## 3.2. Единица процесса: `match`

**Единица процесса:** `match` — одно сражение в режиме pve или pvp.

**Назначение:** матч имеет явный старт и финиш, фиксирует результат (`win`/`lose`, `score`, длительность) и является основой транзакции «результат → награды → прогресс → рейтинг».

**Ключевые endpoints:**

- `POST /api/v1/events` с `eventType = 'match_start'` — клиент сигнализирует о начале
- `POST /api/v1/match/finish` — закрыть матч и начислить награды
- `GET /api/v1/profile` — получить актуальный прогресс после матча

**Что является истиной на сервере:**

Клиент **не присылает** `xpGained` и `softCurrencyGained`. Сервер рассчитывает их в `match_service.py` по константам:

```python
_XP_WIN   = 100   _XP_LOSS   = 30
_SOFT_WIN = 50    _SOFT_LOSS = 10
```

**Таблицы ядра, затрагиваемые при закрытии матча:**

- `matches` — статус меняется на `finished`, фиксируется `ended_at`
- `battle_results` — записывается итог (is_win, score, duration_seconds)
- `player_progress` — начисляются xp и soft_currency
- `leaderboard_scores` — upsert с GREATEST(score) по доске `default`, сезон `1`
- `statistics_daily` — upsert агрегатов (events_count, playtime_seconds, wins/losses, score_sum)
- `game_events` — при необходимости логируется событие `reward_granted`

---

## 3.3. Жанровые таблицы

### 1) `matches` — единица процесса

- **PK:** `id` BIGINT AUTO_INCREMENT
- **Ключевые поля:**
  - `user_id` INT — владелец матча
  - `mode` ENUM('pve','pvp') — режим
  - `map_code` VARCHAR(64) — код карты (`forest_raid`, `desert_clash`, `mountain_pass`, `coastal_siege`)
  - `season` INT DEFAULT 1 — игровой сезон
  - `status` ENUM('started','finished','abandoned') — текущий статус
  - `started_at` DATETIME(3), `ended_at` DATETIME(3)
- **FK:** `user_id → users(id)` ON DELETE CASCADE
- **Индексы:** INDEX(user_id, started_at)
- **CHECK:** `season >= 1`

### 2) `battle_results` — итог матча

- **PK:** `match_id` BIGINT (1:1 с `matches`)
- **Ключевые поля:**
  - `is_win` TINYINT(1) — результат
  - `score` INT — итоговые очки
  - `duration_seconds` INT — длительность матча
  - `units_lost` INT — потери игрока
  - `units_killed` INT — уничтожено противников
- **FK:** `match_id → matches(id)` ON DELETE CASCADE
- **CHECK:** `score >= 0`, `duration_seconds >= 0`, `units_lost >= 0`, `units_killed >= 0`

### 3) `army_units` — армия пользователя

- **PK:** `id` INT AUTO_INCREMENT
- **Ключевые поля:**
  - `user_id` INT
  - `unit_code` VARCHAR(64) — тип юнита (`infantry`, `cavalry`, `archer`, `siege`)
  - `amount` INT — количество юнитов
  - `power` INT — суммарная боевая сила
- **FK:** `user_id → users(id)` ON DELETE CASCADE
- **Уникальность:** UNIQUE(user_id, unit_code)
- **CHECK:** `amount >= 0`, `power >= 0`

### 4) `city_buildings` — здания города

- **PK:** `id` INT AUTO_INCREMENT
- **Ключевые поля:**
  - `user_id` INT
  - `building_code` VARCHAR(64) — тип здания (`barracks`, `farm`, `forge`, `walls`)
  - `level` INT — текущий уровень
  - `status` ENUM('idle','upgrading') — не улучшается / идёт апгрейд
- **FK:** `user_id → users(id)` ON DELETE CASCADE
- **Уникальность:** UNIQUE(user_id, building_code)
- **CHECK:** `level >= 1`

### 5) `processed_events` — реестр дедупликации

- **PK:** `id` BIGINT AUTO_INCREMENT
- **Ключевые поля:**
  - `user_id` INT
  - `event_id` VARCHAR(128) — UUID события от клиента
  - `event_type` VARCHAR(64)
  - `processed_at` DATETIME(3)
- **FK:** `user_id → users(id)` ON DELETE CASCADE
- **Уникальность:** UNIQUE(user_id, event_id) — повторный `event_id` → IntegrityError → `409 EVENT_REJECTED`

---

## 3.4. События (6 event_type) + payload

> Все события пишутся в `game_events` через `insert_game_event()` в `events_repo.py`.
> В каждом payload присутствует идентификатор процесса (`matchId`) или контекст (`buildingCode`).
> `user_id` берётся только из JWT — никогда из тела запроса.

**1) `match_start`** — клиент начинает матч

```json
{
  "matchId": 9,
  "mode": "pve",
  "mapCode": "coastal_siege",
  "season": 1
}
```

**2) `unit_deploy`** — отправка юнитов в атаку

```json
{
  "matchId": 9,
  "unitCode": "cavalry",
  "amount": 40,
  "atSecond": 15
}
```

**3) `unit_retreat`** — потери / отступление

```json
{
  "matchId": 9,
  "unitCode": "infantry",
  "lost": 25,
  "atSecond": 180
}
```

**4) `match_finish`** — факты завершения (без наград — сервер считает сам)

```json
{
  "matchId": 9,
  "isWin": true,
  "score": 1850,
  "durationSeconds": 1200
}
```

**5) `building_upgrade_started`** — начало улучшения здания

```json
{
  "buildingCode": "barracks",
  "fromLevel": 4,
  "toLevel": 5,
  "goldCost": 500
}
```

**6) `reward_granted`** — начисление наград (записывается сервером)

```json
{
  "matchId": 9,
  "xp": 100,
  "softCurrency": 50,
  "reason": "match_finish"
}
```

---

## 3.5. Связка с ядром

**player_progress** — при завершении матча (`match_finish`):

```text
xp            += 100 (победа) / 30 (поражение)
soft_currency += 50  (победа) / 10 (поражение)
```

Реализовано в `progress_repo.py → add_progress_rewards()`.

**leaderboard_scores** — upsert с `GREATEST(score, VALUES(score))`:

```text
board_code = 'default'
season     = 1
score      = значение из result.score
```

Реализовано в `leaderboard_repo.py → upsert_leaderboard_score()`. В `seed.sql` также присутствует доска `pvp`.

**statistics_daily** — upsert агрегатов за текущий день (`CURDATE()`):

```text
events_count     += 1
playtime_seconds += durationSeconds
wins             += 1 (если isWin)
losses           += 1 (если не isWin)
score_sum        += score
```

Реализовано в `stats_repo.py → upsert_daily_stats()`.

**game_events** — каждое событие от клиента логируется через `events_repo.py → insert_game_event()` с `payload_json` типа JSON.

**processed_events** — каждый `eventId` регистрируется через `dedupe_repo.py → register_event_or_conflict()`. Повторная отправка того же `eventId` вызывает `IntegrityError` → `409 EVENT_REJECTED`.

---

## 3.6. Транзакция «итог → награды → прогресс → рейтинг»

При вызове `POST /api/v1/match/finish` в `match_service.py` атомарно в одном `db.session`:

1. `ensure_match_ownership_started()` — SELECT без блокировки, проверка владения и статуса
2. `finish_match_row()` — UPDATE matches + INSERT INTO battle_results
3. `ensure_progress_row()` — INSERT IGNORE, гарантирует наличие строки
4. `lock_progress_row()` — SELECT FOR UPDATE, защита от гонок
5. `add_progress_rewards()` — UPDATE player_progress SET xp+, soft_currency+
6. `upsert_daily_stats()` — INSERT ... ON DUPLICATE KEY UPDATE statistics_daily
7. `upsert_leaderboard_score()` — INSERT ... ON DUPLICATE KEY UPDATE leaderboard_scores
8. `db.session.commit()` — всё или ничего

При любом исключении на любом шаге: `db.session.rollback()`.
