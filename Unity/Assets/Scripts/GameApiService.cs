using System;
using System.Collections;
using UnityEngine;

namespace StrategyGame.API
{
    /// <summary>
    /// Высокоуровневый сервис — одна точка входа для всех вызовов API.
    /// Использует ApiClient для HTTP и AuthSession для хранения токена.
    ///
    /// Использование:
    ///   GameApiService svc = GetComponent<GameApiService>();
    ///   StartCoroutine(svc.Login("alice@example.com", "password123", onOk, onErr));
    /// </summary>
    public class GameApiService : MonoBehaviour
    {
        // ─── Auth ────────────────────────────────────────────────

        /// <summary>POST /api/v1/auth/register</summary>
        public IEnumerator Register(
            string email, string password, string nickname,
            Action<RegisterResponse> onSuccess,
            Action<string>           onError)
        {
            var body = new RegisterRequest
            {
                email    = email,
                password = password,
                nickname = nickname
            };
            yield return StartCoroutine(
                ApiClient.Post<RegisterRequest, RegisterResponse>(
                    ApiConfig.Register, body, onSuccess, onError, withAuth: false));
        }

        /// <summary>POST /api/v1/auth/login — сохраняет токен в AuthSession</summary>
        public IEnumerator Login(
            string email, string password,
            Action<LoginResponse> onSuccess,
            Action<string>        onError)
        {
            var body = new LoginRequest { email = email, password = password };
            yield return StartCoroutine(
                ApiClient.Post<LoginRequest, LoginResponse>(
                    ApiConfig.Login, body,
                    resp =>
                    {
                        AuthSession.Save(resp);
                        onSuccess?.Invoke(resp);
                    },
                    onError,
                    withAuth: false));
        }

        public void Logout() => AuthSession.Clear();

        // ─── Profile ─────────────────────────────────────────────

        /// <summary>GET /api/v1/profile — требует авторизации</summary>
        public IEnumerator GetProfile(
            Action<ProfileResponse> onSuccess,
            Action<string>          onError)
        {
            yield return StartCoroutine(
                ApiClient.Get<ProfileResponse>(
                    ApiConfig.Profile, onSuccess, onError));
        }

        // ─── Events ──────────────────────────────────────────────

        /// <summary>POST /api/v1/events — отправить игровое событие</summary>
        public IEnumerator SendEvent(
            string eventType,
            object payload,
            Action<EventResponse> onSuccess,
            Action<string>        onError,
            int?  sessionId  = null,
            string clientTime = null)
        {
            var body = new EventRequest
            {
                eventId    = Guid.NewGuid().ToString(),   // уникальный UUID для дедупликации
                eventType  = eventType,
                sessionId  = sessionId,
                clientTime = clientTime ?? DateTime.UtcNow.ToString("o"),
                payload    = payload
            };
            yield return StartCoroutine(
                ApiClient.Post<EventRequest, EventResponse>(
                    ApiConfig.Events, body, onSuccess, onError));
        }

        // ─── Match ───────────────────────────────────────────────

        /// <summary>POST /api/v1/match/finish — завершить матч</summary>
        public IEnumerator FinishMatch(
            int  matchId,
            bool isWin,
            int  score,
            int  durationSeconds,
            Action<MatchFinishResponse> onSuccess,
            Action<string>              onError,
            int? powerDelta = null)
        {
            var body = new MatchFinishRequest
            {
                matchId = matchId,
                result  = new MatchResult
                {
                    isWin           = isWin,
                    score           = score,
                    durationSeconds = durationSeconds,
                    powerDelta      = powerDelta
                }
            };
            yield return StartCoroutine(
                ApiClient.Post<MatchFinishRequest, MatchFinishResponse>(
                    ApiConfig.MatchFinish, body, onSuccess, onError));
        }

        // ─── Leaderboard ─────────────────────────────────────────

        /// <summary>GET /api/v1/leaderboard?boardCode=...&season=...&limit=...</summary>
        public IEnumerator GetLeaderboard(
            Action<LeaderboardResponse> onSuccess,
            Action<string>              onError,
            string boardCode = ApiConfig.DefaultBoard,
            int    season    = ApiConfig.DefaultSeason,
            int    limit     = ApiConfig.DefaultLimit)
        {
            string url = $"{ApiConfig.Leaderboard}?boardCode={boardCode}&season={season}&limit={limit}";
            yield return StartCoroutine(
                ApiClient.Get<LeaderboardResponse>(url, onSuccess, onError));
        }
    }
}
