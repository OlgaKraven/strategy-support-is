using System.Collections;
using UnityEngine;
using TMPro;
using StrategyGame.API;

namespace StrategyGame.Game
{
    /// <summary>
    /// Управляет жизненным циклом матча в Unity:
    ///   • Отправляет событие match_start
    ///   • Считает время матча
    ///   • При вызове EndMatch() — отправляет POST /api/v1/match/finish
    ///   • Показывает результат (награды) в UI
    ///
    /// Как подключить:
    ///   1. Создайте GameObject "MatchManager".
    ///   2. Добавьте MatchController + GameApiService.
    ///   3. Привяжите UI-поля в Inspector.
    ///   4. Назначьте matchId в Inspector (или задайте через код перед стартом матча).
    ///   5. Вызывайте EndMatch(isWin, score) из игровой логики.
    /// </summary>
    public class MatchController : MonoBehaviour
    {
        [Header("Match Settings")]
        [SerializeField] private int matchId = 9;    // id матча из БД (seed.sql → id=9)

        [Header("UI")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private GameObject resultPanel;

        private GameApiService _api;
        private float          _elapsed;
        private bool           _running;

        // ─── Lifecycle ───────────────────────────────────────────

        private void Awake()
        {
            _api = GetComponent<GameApiService>();
            if (_api == null)
                _api = gameObject.AddComponent<GameApiService>();

            if (resultPanel) resultPanel.SetActive(false);
        }

        private void Start()
        {
            StartMatch();
        }

        private void Update()
        {
            if (!_running) return;
            _elapsed += Time.deltaTime;
            if (timerText)
                timerText.text = FormatTime(_elapsed);
        }

        // ─── Старт матча ─────────────────────────────────────────

        private void StartMatch()
        {
            _elapsed = 0f;
            _running = true;

            SetStatus("Матч начался!");

            // Отправляем событие match_start
            var payload = new MatchStartPayload
            {
                matchId  = matchId,
                mode     = "pve",
                mapCode  = "coastal_siege",
                season   = 1
            };

            StartCoroutine(_api.SendEvent(
                eventType: "match_start",
                payload:   payload,
                onSuccess: _ => Debug.Log("[Match] match_start принят"),
                onError:   err => Debug.LogWarning("[Match] match_start ошибка: " + err)));
        }

        // ─── Конец матча (вызывать из игровой логики) ────────────

        /// <summary>
        /// Завершить матч. Вызывайте когда игра выиграна или проиграна.
        /// </summary>
        public void EndMatch(bool isWin, int score)
        {
            if (!_running) return;
            _running = false;

            int duration = Mathf.RoundToInt(_elapsed);
            SetStatus(isWin ? "Победа! Отправляем результат..." : "Поражение... Отправляем результат...");

            StartCoroutine(SendMatchFinish(isWin, score, duration));
        }

        private IEnumerator SendMatchFinish(bool isWin, int score, int duration)
        {
            yield return StartCoroutine(_api.FinishMatch(
                matchId:         matchId,
                isWin:           isWin,
                score:           score,
                durationSeconds: duration,
                onSuccess: data =>
                {
                    ShowResult(data);
                },
                onError: err =>
                {
                    SetStatus("Ошибка: " + err, error: true);
                    Debug.LogError("[Match] FinishMatch error: " + err);
                }));
        }

        // ─── Отображение результата ──────────────────────────────

        private void ShowResult(MatchFinishResponse data)
        {
            if (resultPanel) resultPanel.SetActive(true);

            string result = data.isWin ? "🏆 ПОБЕДА!" : "💀 ПОРАЖЕНИЕ";
            SetStatus(result);

            if (rewardText)
                rewardText.text = $"+{data.xpGained} XP\n+{data.softCurrencyGained} монет";

            Debug.Log($"[Match] Завершён. isWin={data.isWin} xp={data.xpGained} soft={data.softCurrencyGained}");
        }

        // ─── Helpers ─────────────────────────────────────────────

        private void SetStatus(string msg, bool error = false)
        {
            if (statusText == null) return;
            statusText.text  = msg;
            statusText.color = error ? Color.red : Color.white;
        }

        private static string FormatTime(float seconds)
        {
            int m = (int)seconds / 60;
            int s = (int)seconds % 60;
            return $"{m:00}:{s:00}";
        }
    }

    // ─── Payload для match_start ──────────────────────────────────

    [System.Serializable]
    public class MatchStartPayload
    {
        public int    matchId;
        public string mode;
        public string mapCode;
        public int    season;
    }
}
