namespace StrategyGame.API
{
    /// <summary>
    /// Единая точка настройки адреса сервера.
    /// Меняйте BaseUrl под своё окружение:
    ///   Локально:   http://127.0.0.1:5000
    ///   Production: https://your-server.com
    /// </summary>
    public static class ApiConfig
    {
        public const string BaseUrl = "http://127.0.0.1:5000";

        // Endpoints
        public const string Register       = BaseUrl + "/api/v1/auth/register";
        public const string Login          = BaseUrl + "/api/v1/auth/login";
        public const string Profile        = BaseUrl + "/api/v1/profile";
        public const string Events         = BaseUrl + "/api/v1/events";
        public const string MatchFinish    = BaseUrl + "/api/v1/match/finish";
        public const string Leaderboard    = BaseUrl + "/api/v1/leaderboard";

        // Leaderboard defaults
        public const string DefaultBoard   = "default";
        public const int    DefaultSeason  = 1;
        public const int    DefaultLimit   = 10;
    }
}
