using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StrategyGame.API;

namespace StrategyGame.UI
{
    /// <summary>
    /// Загружает и отображает таблицу лидеров.
    ///
    /// Как подключить:
    ///   1. GameObject "LeaderboardPanel" → добавить LeaderboardController + GameApiService.
    ///   2. Создайте Prefab строки: TMP_Text rank | nickname | score.
    ///      Привяжите как rowPrefab.
    ///   3. Привяжите ScrollView.Content как rowContainer.
    /// </summary>
    public class LeaderboardController : MonoBehaviour
    {
        [Header("List")]
        [SerializeField] private Transform rowContainer;   // ScrollView > Viewport > Content
        [SerializeField] private GameObject rowPrefab;     // Prefab строки таблицы

        [Header("Status")]
        [SerializeField] private TMP_Text statusText;

        [Header("Settings")]
        [SerializeField] private string boardCode = ApiConfig.DefaultBoard;
        [SerializeField] private int    season    = ApiConfig.DefaultSeason;
        [SerializeField] private int    limit     = ApiConfig.DefaultLimit;

        private GameApiService _api;

        private void Awake()
        {
            _api = GetComponent<GameApiService>();
            if (_api == null)
                _api = gameObject.AddComponent<GameApiService>();
        }

        private void Start() => Refresh();

        /// <summary>Перезагрузить таблицу с сервера.</summary>
        public void Refresh()
        {
            SetStatus("Загрузка...");
            StartCoroutine(_api.GetLeaderboard(
                onSuccess: data =>
                {
                    Render(data);
                    SetStatus("");
                },
                onError: err => SetStatus(err, error: true),
                boardCode: boardCode,
                season:    season,
                limit:     limit));
        }

        // ─── Рендер строк ─────────────────────────────────────────

        private void Render(LeaderboardResponse data)
        {
            // Очистить старые строки
            foreach (Transform child in rowContainer)
                Destroy(child.gameObject);

            if (data.items == null || data.items.Count == 0)
            {
                SetStatus("Таблица пуста");
                return;
            }

            foreach (var item in data.items)
            {
                var row = Instantiate(rowPrefab, rowContainer);

                // Ищем TMP-поля в prefab по тегам или именам объектов
                var labels = row.GetComponentsInChildren<TMP_Text>();
                if (labels.Length >= 3)
                {
                    labels[0].text = item.rank.ToString();
                    labels[1].text = item.nickname;
                    labels[2].text = item.score.ToString();
                }
            }
        }

        private void SetStatus(string msg, bool error = false)
        {
            if (statusText == null) return;
            statusText.text  = msg;
            statusText.color = error ? Color.red : Color.gray;
        }
    }
}
