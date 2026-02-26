namespace StrategyGame.API
{
    /// <summary>
    /// Хранит JWT-токен и данные текущего пользователя.
    /// Живёт в памяти на протяжении игровой сессии.
    /// Используется всеми API-клиентами для подстановки Bearer-заголовка.
    /// </summary>
    public static class AuthSession
    {
        public static string AccessToken { get; private set; }
        public static int    UserId      { get; private set; }
        public static string Nickname    { get; private set; }
        public static bool   IsLoggedIn  => !string.IsNullOrEmpty(AccessToken);

        public static void Save(LoginResponse resp)
        {
            AccessToken = resp.accessToken;
            UserId      = resp.userId;
            Nickname    = resp.nickname;
        }

        public static void Clear()
        {
            AccessToken = null;
            UserId      = 0;
            Nickname    = null;
        }
    }
}
