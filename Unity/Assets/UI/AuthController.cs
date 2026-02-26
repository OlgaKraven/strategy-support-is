using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StrategyGame.API;

namespace StrategyGame.UI
{
    /// <summary>
    /// Контроллер экрана авторизации.
    ///
    /// Как подключить в сцене:
    ///   1. Создайте пустой GameObject "AuthManager".
    ///   2. Добавьте компоненты AuthController и GameApiService.
    ///   3. Привяжите UI-поля в Inspector.
    ///   4. Укажите onLoginSuccess: имя сцены которую надо загрузить после входа.
    /// </summary>
    public class AuthController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject registerPanel;

        [Header("Login Fields")]
        [SerializeField] private TMP_InputField loginEmail;
        [SerializeField] private TMP_InputField loginPassword;
        [SerializeField] private Button         loginButton;

        [Header("Register Fields")]
        [SerializeField] private TMP_InputField regEmail;
        [SerializeField] private TMP_InputField regPassword;
        [SerializeField] private TMP_InputField regNickname;
        [SerializeField] private Button         registerButton;

        [Header("Status")]
        [SerializeField] private TMP_Text statusText;

        [Header("Navigation")]
        [SerializeField] private string sceneAfterLogin = "MainMenu";

        private GameApiService _api;

        private void Awake()
        {
            _api = GetComponent<GameApiService>();
            if (_api == null)
                _api = gameObject.AddComponent<GameApiService>();

            loginButton   .onClick.AddListener(OnLoginClick);
            registerButton.onClick.AddListener(OnRegisterClick);

            ShowLogin();
        }

        // ─── Переключение панелей ────────────────────────────────

        public void ShowLogin()
        {
            loginPanel   .SetActive(true);
            registerPanel.SetActive(false);
            SetStatus("");
        }

        public void ShowRegister()
        {
            loginPanel   .SetActive(false);
            registerPanel.SetActive(true);
            SetStatus("");
        }

        // ─── Логин ──────────────────────────────────────────────

        private void OnLoginClick()
        {
            string email = loginEmail.text.Trim();
            string pass  = loginPassword.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                SetStatus("Заполните email и пароль", error: true);
                return;
            }

            SetStatus("Вход...");
            SetInteractable(false);

            StartCoroutine(_api.Login(email, pass,
                onSuccess: resp =>
                {
                    SetStatus($"Добро пожаловать, {resp.nickname}!");
                    SetInteractable(true);
                    // Переход в главное меню
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneAfterLogin);
                },
                onError: err =>
                {
                    SetStatus(err, error: true);
                    SetInteractable(true);
                }));
        }

        // ─── Регистрация ─────────────────────────────────────────

        private void OnRegisterClick()
        {
            string email    = regEmail.text.Trim();
            string pass     = regPassword.text;
            string nickname = regNickname.text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(nickname))
            {
                SetStatus("Заполните все поля", error: true);
                return;
            }

            SetStatus("Регистрация...");
            SetInteractable(false);

            StartCoroutine(_api.Register(email, pass, nickname,
                onSuccess: resp =>
                {
                    SetStatus("Аккаунт создан! Войдите.");
                    SetInteractable(true);
                    ShowLogin();
                },
                onError: err =>
                {
                    SetStatus(err, error: true);
                    SetInteractable(true);
                }));
        }

        // ─── Helpers ─────────────────────────────────────────────

        private void SetStatus(string msg, bool error = false)
        {
            if (statusText == null) return;
            statusText.text  = msg;
            statusText.color = error ? Color.red : Color.white;
        }

        private void SetInteractable(bool value)
        {
            loginButton   .interactable = value;
            registerButton.interactable = value;
        }
    }
}
