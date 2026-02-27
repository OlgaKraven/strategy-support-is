-- =============================================================
-- strategy_db — Schema Creation
-- Run this BEFORE importing seed data
-- =============================================================

CREATE DATABASE IF NOT EXISTS strategy_db 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE strategy_db;

-- ---------------------------------------------------------------
-- Roles (справочник ролей)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS roles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------
-- Users (пользователи)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL, -- bcrypt hash
    nickname VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP NULL,
    is_banned TINYINT(1) DEFAULT 0,
    INDEX idx_email (email),
    INDEX idx_nickname (nickname),
    INDEX idx_banned (is_banned)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------
-- User ↔ Roles (связь многие-ко-многим)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_roles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    role_id INT NOT NULL,
    assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY unique_user_role (user_id, role_id),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
    INDEX idx_user_id (user_id),
    INDEX idx_role_id (role_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------
-- Player Progress (прогресс игрока)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS player_progress (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL UNIQUE,
    level INT DEFAULT 1,
    xp INT DEFAULT 0,
    soft_currency INT DEFAULT 0,
    hard_currency INT DEFAULT 0,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_level (level),
    INDEX idx_xp (xp)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------
-- Leaderboard Scores (таблицы лидеров)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS leaderboard_scores (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    board_code VARCHAR(50) NOT NULL, -- 'default', 'pvp', etc.
    season INT NOT NULL DEFAULT 1,
    score INT DEFAULT 0,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY unique_user_board_season (user_id, board_code, season),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_board_season_score (board_code, season, score),
    INDEX idx_score (score)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------
-- Game Sessions (игровые сессии)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS game_sessions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    client_platform VARCHAR(50) NOT NULL, -- 'unity', 'mobile', 'web'
    client_version VARCHAR(20) NOT NULL,  -- '1.2.0'
    ip VARCHAR(45) NOT NULL,              -- IPv4/IPv6
    started_at TIMESTAMP NOT NULL,
    ended_at TIMESTAMP NULL,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_user_started (user_id, started_at),
    INDEX idx_platform (client_platform),
    INDEX idx_started_at (started_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------
-- Matches (матчи/бои)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS matches (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    mode VARCHAR(20) NOT NULL,      -- 'pve', 'pvp'
    map_code VARCHAR(50) NOT NULL,  -- 'forest_raid', 'desert_clash'
    season INT DEFAULT 1,
    status VARCHAR(20) NOT NULL,    -- 'started', 'finished', 'abandoned'
    started_at TIMESTAMP NOT NULL,
    ended_at TIMESTAMP NULL,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_user_mode (user_id, mode),
    INDEX idx_status (status),
    INDEX idx_started_at (started_at),
    INDEX idx_map (map_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------
-- Battle Results (результаты боёв)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS battle_results (
    id INT AUTO_INCREMENT PRIMARY KEY,
    match_id INT NOT NULL UNIQUE,
    is_win TINYINT(1) NOT NULL,     -- 1 = победа, 0 = поражение
    score INT DEFAULT 0,
    duration_seconds INT DEFAULT 0, -- длительность в секундах
    units_lost INT DEFAULT 0,
    units_killed INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (match_id) REFERENCES matches(id) ON DELETE CASCADE,
    INDEX idx_is_win (is_win),
    INDEX idx_score (score)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------
-- Army Units (армия игрока)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS army_units (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    unit_code VARCHAR(50) NOT NULL, -- 'infantry', 'cavalry', 'archer', 'siege'
    amount INT DEFAULT 0,
    power INT DEFAULT 0,            -- боевая мощь
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY unique_user_unit (user_id, unit_code),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_unit_code (unit_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------
-- City Buildings (здания города)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS city_buildings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    building_code VARCHAR(50) NOT NULL, -- 'barracks', 'farm', 'forge', 'walls'
    level INT DEFAULT 1,
    status VARCHAR(20) DEFAULT 'idle',  -- 'idle', 'upgrading'
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY unique_user_building (user_id, building_code),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_building_code (building_code),
    INDEX idx_level (level)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------
-- Daily Statistics (ежедневная статистика)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS statistics_daily (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    day DATE NOT NULL,
    sessions_count INT DEFAULT 0,
    events_count INT DEFAULT 0,
    playtime_seconds INT DEFAULT 0,
    wins INT DEFAULT 0,
    losses INT DEFAULT 0,
    score_sum INT DEFAULT 0,
    UNIQUE KEY unique_user_day (user_id, day),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_day (day),
    INDEX idx_wins (wins)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------
-- Processed Events (лог обработанных событий для дедупликации)
-- ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS processed_events (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    event_id VARCHAR(100) NOT NULL,     -- уникальный ID события
    event_type VARCHAR(50) NOT NULL,    -- 'match_start', 'unit_deploy'
    processed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY unique_event (event_id),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_event_type (event_type),
    INDEX idx_processed_at (processed_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
