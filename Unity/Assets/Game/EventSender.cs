using System;
using UnityEngine;
using StrategyGame.API;

namespace StrategyGame.Game
{
    /// <summary>
    /// Вспомогательный компонент для отправки произвольных игровых событий.
    ///
    /// Использование из любого MonoBehaviour:
    ///   EventSender.Send(this, "unit_deploy", new { matchId=9, unitCode="cavalry", amount=40 });
    /// </summary>
    public static class EventSender
    {
        /// <summary>
        /// Отправить событие через GameApiService, найденный в сцене.
        /// </summary>
        public static void Send(MonoBehaviour caller, string eventType, object payload,
            Action onSuccess = null, Action<string> onError = null)
        {
            var svc = UnityEngine.Object.FindObjectOfType<GameApiService>();
            if (svc == null)
            {
                Debug.LogError("[EventSender] GameApiService не найден в сцене!");
                return;
            }

            caller.StartCoroutine(svc.SendEvent(
                eventType: eventType,
                payload:   payload,
                onSuccess: resp =>
                {
                    Debug.Log($"[Event] {eventType} принят: {resp.eventId}");
                    onSuccess?.Invoke();
                },
                onError: err =>
                {
                    Debug.LogWarning($"[Event] {eventType} ошибка: {err}");
                    onError?.Invoke(err);
                }));
        }
    }

    // ─── Готовые payload-классы для жанровых событий ─────────────

    [Serializable]
    public class UnitDeployPayload
    {
        public int    matchId;
        public string unitCode;   // infantry / cavalry / archer / siege
        public int    amount;
        public int    atSecond;
    }

    [Serializable]
    public class UnitRetreatPayload
    {
        public int    matchId;
        public string unitCode;
        public int    lost;
        public int    atSecond;
    }

    [Serializable]
    public class BuildingUpgradePayload
    {
        public string buildingCode;  // barracks / farm / forge / walls
        public int    fromLevel;
        public int    toLevel;
        public int    goldCost;
    }
}
