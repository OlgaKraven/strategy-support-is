using System;
using System.Collections.Generic;

namespace StrategyGame.API
{
    // ─────────────────────────────────────────
    // Общая обёртка ответов сервера
    // { "ok": true/false, "data": {...}, "error": {...} }
    // ─────────────────────────────────────────

    [Serializable]
    public class ApiResponse<T>
    {
        public bool ok;
        public T    data;
        public ApiError error;
    }

    [Serializable]
    public class ApiError
    {
        public string code;
        public string message;
        public string requestId;
    }

    // ─────────────────────────────────────────
    // POST /api/v1/auth/register
    // ─────────────────────────────────────────

    [Serializable]
    public class RegisterRequest
    {
        public string email;
        public string password;
        public string nickname;
    }

    [Serializable]
    public class RegisterResponse
    {
        public int    userId;
        public string email;
        public string nickname;
    }

    // ─────────────────────────────────────────
    // POST /api/v1/auth/login
    // ─────────────────────────────────────────

    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class LoginResponse
    {
        public string accessToken;
        public int    userId;
        public string nickname;
    }

    // ─────────────────────────────────────────
    // GET /api/v1/profile
    // ─────────────────────────────────────────

    [Serializable]
    public class ProfileUser
    {
        public int    id;
        public string email;
        public string nickname;
        public List<string> roles;
    }

    [Serializable]
    public class ProfileProgress
    {
        public int level;
        public int xp;
        public int softCurrency;
        public int hardCurrency;
    }

    [Serializable]
    public class ProfileResponse
    {
        public int              userId;
        public string           email;
        public string           nickname;
        public List<string>     roles;
        public ProfileProgress  progress;
    }

    // ─────────────────────────────────────────
    // POST /api/v1/events
    // ─────────────────────────────────────────

    [Serializable]
    public class EventRequest
    {
        public string eventId;      // UUID — обязателен для дедупликации
        public string eventType;
        public int?   sessionId;    // nullable
        public string clientTime;   // ISO 8601
        public object payload;      // произвольный JSON-объект
    }

    [Serializable]
    public class EventResponse
    {
        public string eventId;
        public string status;       // "accepted"
    }

    // ─────────────────────────────────────────
    // POST /api/v1/match/finish
    // ─────────────────────────────────────────

    [Serializable]
    public class MatchResult
    {
        public bool   isWin;
        public int    score;
        public int    durationSeconds;
        public int?   powerDelta;    // опционально
    }

    [Serializable]
    public class MatchFinishRequest
    {
        public int         matchId;
        public MatchResult result;
    }

    [Serializable]
    public class MatchFinishResponse
    {
        public int  matchId;
        public bool isWin;
        public int  xpGained;
        public int  softCurrencyGained;
    }

    // ─────────────────────────────────────────
    // GET /api/v1/leaderboard
    // ─────────────────────────────────────────

    [Serializable]
    public class LeaderboardItem
    {
        public int    rank;
        public int    userId;
        public string nickname;
        public int    score;
    }

    [Serializable]
    public class LeaderboardResponse
    {
        public string                boardCode;
        public int                   season;
        public List<LeaderboardItem> items;
    }
}
