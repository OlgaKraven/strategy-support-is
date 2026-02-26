using UnityEngine;
using TMPro;
using StrategyGame.API;

namespace StrategyGame.UI
{
    /// <summary>
    /// Отображает профиль текущего игрока.
    ///
    /// Как подключить:
    ///   1. GameObject "ProfilePanel" → добавить ProfileController + GameApiService.
    ///   2. Привязать TMP-поля в Inspector.
    ///   3. Вызывается автоматически при Start().
    /// </summary>
    public class ProfileController : MonoBehaviour
    {
        [Header("UI Labels")]
        [SerializeField] private TMP_Text nicknameText;
        [SerializeField] private TMP_Text emailText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text xpText;
        [SerializeField] private TMP_Text softCurrencyText;
        [SerializeField] private TMP_Text hardCurrencyText;
        [SerializeField] private TMP_Text statusText;

        private GameApiService _api;

        private void Awake()
        {
            _api = GetComponent<GameApiService>();
            if (_api == null)
                _api = gameObject.AddComponent<GameApiService>();
        }

        private void Start()
        {
            if (!AuthSession.IsLoggedIn)
            {
                SetStatus("Не авторизован", error: true);
                return;
            }
            Refresh();
        }

        /// <summary>Перезагрузить данные профиля с сервера.</summary>
        public void Refresh()
        {
            SetStatus("Загрузка...");
            StartCoroutine(_api.GetProfile(
                onSuccess: data =>
                {
                    ApplyProfile(data);
                    SetStatus("");
                },
                onError: err => SetStatus(err, error: true)));
        }

        // ─── Применение данных ────────────────────────────────────

        private void ApplyProfile(ProfileResponse data)
        {
            if (nicknameText)     nicknameText.text     = data.nickname;
            if (emailText)        emailText.text        = data.email;
            if (levelText)        levelText.text        = $"Уровень {data.progress?.level}";
            if (xpText)           xpText.text           = $"XP: {data.progress?.xp}";
            if (softCurrencyText) softCurrencyText.text = $"Монеты: {data.progress?.softCurrency}";
            if (hardCurrencyText) hardCurrencyText.text = $"Гемы: {data.progress?.hardCurrency}";
        }

        private void SetStatus(string msg, bool error = false)
        {
            if (statusText == null) return;
            statusText.text  = msg;
            statusText.color = error ? Color.red : Color.gray;
        }
    }
}
