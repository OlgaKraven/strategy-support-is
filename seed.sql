-- =============================================================
-- strategy_db — Seed Data
-- Run AFTER importing strategy_db.sql schema
-- =============================================================

USE strategy_db;

-- ---------------------------------------------------------------
-- Roles
-- ---------------------------------------------------------------
INSERT INTO roles (name) VALUES
  ('player'),
  ('moderator'),
  ('admin')
ON DUPLICATE KEY UPDATE name = name;

-- ---------------------------------------------------------------
-- Users  (passwords are bcrypt of the shown plaintext)
-- password: "password123"  →  bcrypt hash below
-- ---------------------------------------------------------------
INSERT INTO users (id, email, password, nickname, created_at, last_login_at, is_banned) VALUES
(1,  'alice@example.com',   '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6hsxq5/Qe2', 'Commander_Alice',  NOW() - INTERVAL 30 DAY, NOW() - INTERVAL 1 DAY,  0),
(2,  'bob@example.com',     '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6hsxq5/Qe2', 'General_Bob',      NOW() - INTERVAL 25 DAY, NOW() - INTERVAL 2 DAY,  0),
(3,  'carol@example.com',   '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6hsxq5/Qe2', 'WarLord_Carol',    NOW() - INTERVAL 20 DAY, NOW() - INTERVAL 3 DAY,  0),
(4,  'dave@example.com',    '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6hsxq5/Qe2', 'Tactician_Dave',   NOW() - INTERVAL 15 DAY, NOW() - INTERVAL 4 DAY,  0),
(5,  'eve@example.com',     '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6hsxq5/Qe2', 'Siege_Eve',        NOW() - INTERVAL 10 DAY, NOW() - INTERVAL 1 DAY,  0),
(6,  'frank@example.com',   '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6hsxq5/Qe2', 'Scout_Frank',      NOW() - INTERVAL  8 DAY, NOW() - INTERVAL 5 DAY,  0),
(7,  'grace@example.com',   '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6hsxq5/Qe2', 'Empress_Grace',    NOW() - INTERVAL  7 DAY, NOW() - INTERVAL 1 DAY,  0),
(8,  'hank@example.com',    '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6hsxq5/Qe2', 'Iron_Hank',        NOW() - INTERVAL  5 DAY, NOW() - INTERVAL 2 DAY,  0),
(9,  'ivan@example.com',    '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6hsxq5/Qe2', 'Phantom_Ivan',     NOW() - INTERVAL  3 DAY, NOW() - INTERVAL 1 DAY,  0),
(10, 'banned@example.com',  '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6hsxq5/Qe2', 'Cheater_X',        NOW() - INTERVAL 60 DAY, NOW() - INTERVAL 30 DAY, 1)
ON DUPLICATE KEY UPDATE email = email;

-- ---------------------------------------------------------------
-- User ↔ Roles
-- ---------------------------------------------------------------
INSERT INTO user_roles (user_id, role_id, assigned_at)
SELECT u.id, r.id, NOW()
FROM users u
JOIN roles r ON r.name = 'player'
WHERE u.id BETWEEN 1 AND 10
ON DUPLICATE KEY UPDATE assigned_at = assigned_at;

-- Admin and moderator for user 1 & 2
INSERT INTO user_roles (user_id, role_id, assigned_at)
SELECT 1, r.id, NOW() FROM roles r WHERE r.name = 'admin'
ON DUPLICATE KEY UPDATE assigned_at = assigned_at;

INSERT INTO user_roles (user_id, role_id, assigned_at)
SELECT 2, r.id, NOW() FROM roles r WHERE r.name = 'moderator'
ON DUPLICATE KEY UPDATE assigned_at = assigned_at;

-- ---------------------------------------------------------------
-- Player Progress
-- ---------------------------------------------------------------
INSERT INTO player_progress (user_id, level, xp, soft_currency, hard_currency) VALUES
(1,  42, 85400, 12500, 250),
(2,  38, 71200, 9800,  180),
(3,  55, 112000,18000, 400),
(4,  29, 48600, 7200,  90),
(5,  61, 138000,24000, 550),
(6,  17, 21000, 3400,  30),
(7,  48, 96000, 15000, 310),
(8,  33, 58000, 8600,  120),
(9,  22, 31000, 4800,  60),
(10,  5,  4200, 800,   10)
ON DUPLICATE KEY UPDATE level = VALUES(level), xp = VALUES(xp);

-- ---------------------------------------------------------------
-- Leaderboard Scores  (board: "default", season 1)
-- ---------------------------------------------------------------
INSERT INTO leaderboard_scores (user_id, board_code, season, score) VALUES
(5,  'default', 1, 9850),
(3,  'default', 1, 8720),
(7,  'default', 1, 7600),
(1,  'default', 1, 7200),
(2,  'default', 1, 6540),
(8,  'default', 1, 5800),
(4,  'default', 1, 4900),
(9,  'default', 1, 3200),
(6,  'default', 1, 2100),
(10, 'default', 1, 150)
ON DUPLICATE KEY UPDATE score = VALUES(score);

-- PvP leaderboard season 1
INSERT INTO leaderboard_scores (user_id, board_code, season, score) VALUES
(1, 'pvp', 1, 4400),
(3, 'pvp', 1, 5900),
(5, 'pvp', 1, 7100),
(7, 'pvp', 1, 3800),
(2, 'pvp', 1, 2900)
ON DUPLICATE KEY UPDATE score = VALUES(score);

-- ---------------------------------------------------------------
-- Game Sessions
-- ---------------------------------------------------------------
INSERT INTO game_sessions (user_id, client_platform, client_version, ip, started_at, ended_at) VALUES
(1, 'unity',  '1.2.0', '192.168.1.10', NOW() - INTERVAL 5 DAY,  NOW() - INTERVAL 5 DAY  + INTERVAL 90 MINUTE),
(1, 'unity',  '1.2.0', '192.168.1.10', NOW() - INTERVAL 2 DAY,  NOW() - INTERVAL 2 DAY  + INTERVAL 60 MINUTE),
(2, 'mobile', '1.1.5', '10.0.0.22',    NOW() - INTERVAL 4 DAY,  NOW() - INTERVAL 4 DAY  + INTERVAL 45 MINUTE),
(3, 'unity',  '1.2.0', '10.0.0.33',    NOW() - INTERVAL 3 DAY,  NOW() - INTERVAL 3 DAY  + INTERVAL 120 MINUTE),
(5, 'web',    '1.2.1', '172.16.0.5',   NOW() - INTERVAL 1 DAY,  NOW() - INTERVAL 1 DAY  + INTERVAL 75 MINUTE),
(7, 'unity',  '1.2.0', '192.168.2.7',  NOW() - INTERVAL 6 HOUR, NULL);

-- ---------------------------------------------------------------
-- Matches
-- ---------------------------------------------------------------
INSERT INTO matches (id, user_id, mode, map_code, season, status, started_at, ended_at) VALUES
(1,  1, 'pve', 'forest_raid',   1, 'finished',  NOW() - INTERVAL 5 DAY, NOW() - INTERVAL 5 DAY  + INTERVAL 25 MINUTE),
(2,  1, 'pvp', 'desert_clash',  1, 'finished',  NOW() - INTERVAL 3 DAY, NOW() - INTERVAL 3 DAY  + INTERVAL 18 MINUTE),
(3,  2, 'pve', 'mountain_pass', 1, 'finished',  NOW() - INTERVAL 4 DAY, NOW() - INTERVAL 4 DAY  + INTERVAL 30 MINUTE),
(4,  3, 'pvp', 'desert_clash',  1, 'finished',  NOW() - INTERVAL 3 DAY, NOW() - INTERVAL 3 DAY  + INTERVAL 22 MINUTE),
(5,  3, 'pve', 'forest_raid',   1, 'finished',  NOW() - INTERVAL 2 DAY, NOW() - INTERVAL 2 DAY  + INTERVAL 28 MINUTE),
(6,  5, 'pvp', 'coastal_siege', 1, 'finished',  NOW() - INTERVAL 1 DAY, NOW() - INTERVAL 1 DAY  + INTERVAL 20 MINUTE),
(7,  5, 'pve', 'mountain_pass', 1, 'finished',  NOW() - INTERVAL 1 DAY, NOW() - INTERVAL 1 DAY  + INTERVAL 35 MINUTE),
(8,  7, 'pvp', 'forest_raid',   1, 'finished',  NOW() - INTERVAL 6 HOUR, NOW() - INTERVAL 5 HOUR),
(9,  1, 'pve', 'coastal_siege', 1, 'started',   NOW() - INTERVAL 10 MINUTE, NULL),
(10, 4, 'pve', 'forest_raid',   1, 'abandoned', NOW() - INTERVAL 2 DAY, NOW() - INTERVAL 2 DAY  + INTERVAL 5 MINUTE)
ON DUPLICATE KEY UPDATE status = status;

-- ---------------------------------------------------------------
-- Battle Results
-- ---------------------------------------------------------------
INSERT INTO battle_results (match_id, is_win, score, duration_seconds, units_lost, units_killed) VALUES
(1, 1, 1200, 1500, 12,  80),
(2, 0,  800, 1080, 45,  30),
(3, 1, 1100, 1800, 20,  70),
(4, 1, 1500, 1320, 10,  95),
(5, 1, 1300, 1680, 18,  88),
(6, 1, 1850, 1200,  5, 120),
(7, 1, 1400, 2100, 22,  90),
(8, 0, 950,  3600, 60,  40),
(10, 0,  0,   300, 80,   0)
ON DUPLICATE KEY UPDATE score = VALUES(score);

-- ---------------------------------------------------------------
-- Army Units
-- ---------------------------------------------------------------
INSERT INTO army_units (user_id, unit_code, amount, power) VALUES
(1, 'infantry',  150, 300),  (1, 'cavalry',   40,  280), (1, 'archer',  80,  240),
(2, 'infantry',  120, 240),  (2, 'siege',      10,  350), (2, 'archer',  60,  180),
(3, 'infantry',  200, 400),  (3, 'cavalry',    80,  560), (3, 'siege',   20,  700),
(5, 'infantry',  300, 600),  (5, 'cavalry',   120,  840), (5, 'siege',   35, 1225),
(7, 'infantry',  180, 360),  (7, 'cavalry',    60,  420), (7, 'archer', 100,  300)
ON DUPLICATE KEY UPDATE amount = VALUES(amount), power = VALUES(power);

-- ---------------------------------------------------------------
-- City Buildings
-- ---------------------------------------------------------------
INSERT INTO city_buildings (user_id, building_code, level, status) VALUES
(1, 'barracks',   5, 'idle'),     (1, 'farm',      4, 'idle'),    (1, 'forge', 3, 'idle'),
(2, 'barracks',   4, 'idle'),     (2, 'farm',      3, 'upgrading'),(2, 'walls', 2, 'idle'),
(3, 'barracks',   8, 'idle'),     (3, 'farm',      7, 'idle'),    (3, 'forge', 6, 'idle'),
(5, 'barracks',  10, 'idle'),     (5, 'farm',      9, 'idle'),    (5, 'forge', 8, 'upgrading'),
(7, 'barracks',   6, 'idle'),     (7, 'farm',      5, 'idle'),    (7, 'walls', 4, 'idle')
ON DUPLICATE KEY UPDATE level = VALUES(level);

-- ---------------------------------------------------------------
-- Daily Statistics  (last 7 days, top players)
-- ---------------------------------------------------------------
INSERT INTO statistics_daily (user_id, day, sessions_count, events_count, playtime_seconds, wins, losses, score_sum) VALUES
(1, CURDATE() - INTERVAL 6 DAY, 2, 14, 5400, 2, 0, 2400),
(1, CURDATE() - INTERVAL 5 DAY, 1,  8, 2700, 0, 1,  800),
(1, CURDATE() - INTERVAL 4 DAY, 3, 20, 7200, 3, 0, 3600),
(1, CURDATE() - INTERVAL 3 DAY, 2, 12, 4500, 1, 1, 1800),
(1, CURDATE() - INTERVAL 2 DAY, 1,  6, 1800, 1, 0, 1200),
(3, CURDATE() - INTERVAL 6 DAY, 3, 22, 8100, 3, 0, 4500),
(3, CURDATE() - INTERVAL 5 DAY, 2, 16, 5400, 2, 0, 3000),
(3, CURDATE() - INTERVAL 4 DAY, 4, 28, 9000, 4, 0, 5200),
(5, CURDATE() - INTERVAL 6 DAY, 4, 30, 9600, 4, 0, 7400),
(5, CURDATE() - INTERVAL 5 DAY, 3, 24, 7200, 3, 0, 5550),
(5, CURDATE() - INTERVAL 4 DAY, 5, 35,10800, 5, 0, 9250),
(5, CURDATE() - INTERVAL 3 DAY, 2, 18, 5400, 2, 0, 3700),
(7, CURDATE() - INTERVAL 6 DAY, 2, 15, 5400, 1, 1, 2850),
(7, CURDATE() - INTERVAL 5 DAY, 3, 20, 7200, 2, 1, 3800),
(2, CURDATE() - INTERVAL 6 DAY, 1,  9, 2700, 1, 0, 1100)
ON DUPLICATE KEY UPDATE wins = VALUES(wins), score_sum = VALUES(score_sum);

-- ---------------------------------------------------------------
-- Processed Events (dedup log)
-- ---------------------------------------------------------------
INSERT INTO processed_events (user_id, event_id, event_type, processed_at) VALUES
(1, 'evt-a1b2c3d4', 'match_start',  NOW() - INTERVAL 5 DAY),
(1, 'evt-b2c3d4e5', 'unit_deploy',  NOW() - INTERVAL 5 DAY),
(3, 'evt-c3d4e5f6', 'match_start',  NOW() - INTERVAL 3 DAY),
(5, 'evt-d4e5f6g7', 'match_start',  NOW() - INTERVAL 1 DAY),
(5, 'evt-e5f6g7h8', 'building_upg', NOW() - INTERVAL 1 DAY)
ON DUPLICATE KEY UPDATE processed_at = processed_at;
