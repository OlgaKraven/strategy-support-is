using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace StrategyGame.API
{
    /// <summary>
    /// Низкоуровневый HTTP-клиент.
    /// Умеет отправлять GET и POST с JSON-телом.
    /// При наличии AccessToken автоматически добавляет заголовок Authorization.
    /// Все методы — корутины: вызывайте через StartCoroutine().
    /// </summary>
    public static class ApiClient
    {
        // ─── GET ────────────────────────────────────────────────

        /// <summary>
        /// Отправить GET-запрос и получить JSON-ответ.
        /// </summary>
        public static IEnumerator Get<TResponse>(
            string url,
            Action<TResponse> onSuccess,
            Action<string>    onError,
            bool              withAuth = true)
        {
            using var req = UnityWebRequest.Get(url);

            SetHeaders(req, withAuth);

            yield return req.SendWebRequest();

            HandleResponse(req, onSuccess, onError);
        }

        // ─── POST ───────────────────────────────────────────────

        /// <summary>
        /// Отправить POST-запрос с JSON-телом и получить JSON-ответ.
        /// </summary>
        public static IEnumerator Post<TRequest, TResponse>(
            string   url,
            TRequest body,
            Action<TResponse> onSuccess,
            Action<string>    onError,
            bool              withAuth = true)
        {
            string json = JsonUtility.ToJson(body);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            using var req = new UnityWebRequest(url, "POST");
            req.uploadHandler   = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();

            SetHeaders(req, withAuth);

            yield return req.SendWebRequest();

            HandleResponse<TResponse>(req, onSuccess, onError);
        }

        // ─── Helpers ────────────────────────────────────────────

        private static void SetHeaders(UnityWebRequest req, bool withAuth)
        {
            req.SetRequestHeader("Content-Type",  "application/json");
            req.SetRequestHeader("Accept",        "application/json");

            if (withAuth && AuthSession.IsLoggedIn)
                req.SetRequestHeader("Authorization", "Bearer " + AuthSession.AccessToken);
        }

        private static void HandleResponse<TResponse>(
            UnityWebRequest   req,
            Action<TResponse> onSuccess,
            Action<string>    onError)
        {
            if (req.result != UnityWebRequest.Result.Success)
            {
                // Попытаться распарсить тело ошибки
                string raw = req.downloadHandler?.text ?? "";
                try
                {
                    var errWrapper = JsonUtility.FromJson<ApiResponse<TResponse>>(raw);
                    string msg = errWrapper?.error?.message ?? req.error;
                    onError?.Invoke($"[{req.responseCode}] {msg}");
                }
                catch
                {
                    onError?.Invoke($"[{req.responseCode}] {req.error}");
                }
                return;
            }

            try
            {
                var wrapper = JsonUtility.FromJson<ApiResponse<TResponse>>(req.downloadHandler.text);
                if (wrapper.ok)
                    onSuccess?.Invoke(wrapper.data);
                else
                    onError?.Invoke(wrapper.error?.message ?? "Unknown server error");
            }
            catch (Exception ex)
            {
                onError?.Invoke("Parse error: " + ex.Message);
            }
        }
    }
}
