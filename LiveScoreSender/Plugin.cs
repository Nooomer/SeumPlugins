using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace LiveScoreSender
{ 

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        private static Plugin _instance;

        private static readonly int MaxRetryCount = 5;
        private static readonly float[] RetryDelays = { 60f, 300f, 900f, 3600f, 21600f }; // секунды

        private readonly List<PendingScore> _pendingScores = new List<PendingScore>();
        private string _storagePath;
        private bool _isProcessing;

        private void Awake()
        {
            // Настройка Newtonsoft.Json для совместимости с Unity (отключаем генерацию кода)
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = { new Newtonsoft.Json.Converters.VersionConverter() }
            };

            _instance = this;
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} loaded!");

            _storagePath = global::System.IO.Path.Combine(Paths.PluginPath, "pending_scores.json");
            LoadPendingScores();
            StartCoroutine(ProcessQueueCoroutine());

            // Применяем Harmony патч на LeaderboardsBackend.submitLevelScore
            try
            {
                var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
                var method = AccessTools.Method(typeof(LeaderboardsBackend), "submitLevelScore");
                if (method == null)
                {
                    Logger.LogError("[LiveScore] Не удалось найти метод LeaderboardsBackend.submitLevelScore");
                    return;
                }
                harmony.Patch(method, postfix: new HarmonyMethod(typeof(Plugin), nameof(OnSubmitLevelScore)));
                Logger.LogInfo($"[LiveScore] Патч успешно применён к {method.DeclaringType?.FullName}.{method.Name}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[LiveScore] Ошибка патча: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            SavePendingScores();
        }

        // === Хуки для Harmony ===
        public static void OnSubmitLevelScore(int level, string name, int scoreMilliseconds, int mutator, MonoBehaviour behaviour, byte[] data, byte[] replay)
        {
            Logger.LogInfo($"[LiveScore] Перехвачен submitLevelScore: level={level}, score={scoreMilliseconds}, mutator={mutator}");
            _instance?.StartCoroutine(_instance.SendScoreIfBest(level, scoreMilliseconds, mutator));
        }

        // === Проверка PB и запуск отправки ===
        private IEnumerator SendScoreIfBest(int level, int score, int mutator)
        {
            string key = $"PB_{level}_{mutator}";
            int currentBest = PlayerPrefs.GetInt(key, int.MaxValue);
            Logger.LogInfo($"[LiveScore] Сравнение PB: новый={score}, текущий лучший={currentBest}");

            if (score >= currentBest)
            {
                Logger.LogInfo($"[LiveScore] Игнорируем нелучший рекорд {score} (лучший {currentBest})");
                yield break;
            }

            PlayerPrefs.SetInt(key, score);
            PlayerPrefs.Save();
            Logger.LogInfo($"[LiveScore] Сохранён новый лучший рекорд {score}");

            yield return StartCoroutine(SendScoreCoroutine(level, score, mutator, 0));
        }

        // === Отправка на сервер (с ретраями и очередью) ===
        private IEnumerator SendScoreCoroutine(int level, int score, int mutator, int retryCount)
        {
            if (!SteamAPI.IsSteamRunning() || !SteamUser.BLoggedOn())
            {
                Logger.LogError("[LiveScore] Steam не инициализирован или пользователь не залогинен");
                EnqueueOrRetry(level, score, mutator, retryCount);
                yield break;
            }

            byte[] ticketBuffer = new byte[1024];
            bool ticketReady = false;
            EResult ticketResult = EResult.k_EResultOK;
            uint ticketLength = 0;

            HAuthTicket authTicket = SteamUser.GetAuthSessionTicket(ticketBuffer, ticketBuffer.Length, out ticketLength);
            float timeout = Time.time + 10f;

            Callback<GetAuthSessionTicketResponse_t> ticketCallback = null;
            ticketCallback = Callback<GetAuthSessionTicketResponse_t>.Create(callback =>
            {
                ticketResult = callback.m_eResult;
                ticketReady = true;
                ticketCallback?.Dispose();
            });

            yield return new WaitUntil(() => ticketReady || Time.time > timeout);

            if (!ticketReady || ticketResult != EResult.k_EResultOK)
            {
                Logger.LogError($"[LiveScore] Ошибка тикета Steam: {ticketResult}");
                SteamUser.CancelAuthTicket(authTicket);
                EnqueueOrRetry(level, score, mutator, retryCount);
                yield break;
            }

            string ticketHex = BitConverter.ToString(ticketBuffer, 0, (int)ticketLength).Replace("-", "").ToLower();
            string steamId = SteamUser.GetSteamID().m_SteamID.ToString();
            string playerName = SteamFriends.GetPersonaName().Replace("\"", "\\\"").Replace("\n", "");

            var payload = new ScorePayload
            {
                steamid = steamId,
                name = playerName,
                level = level,
                score = score,
                mutator = mutator,
                ticket = ticketHex,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            string json = JsonUtility.ToJson(payload);  // payload отправляем через JsonUtility – он подходит
            string signature = GetSignature(steamId, score);

            using (UnityWebRequest request = new UnityWebRequest("https://seum.online/api/live_submit", "POST"))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-Signature", signature);
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();

                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError || request.responseCode != 200)
                {
                    Logger.LogError($"[LiveScore] Ошибка отправки: {request.error} | Код: {request.responseCode}");
                    EnqueueOrRetry(level, score, mutator, retryCount);
                }
                else
                {
                    Logger.LogInfo($"[LiveScore] Успешно отправлен рекорд: уровень {level}, время {score} мс");
                    RemovePendingScore(level, score, mutator);
                }
            }

            SteamUser.CancelAuthTicket(authTicket);
        }

        // === Управление очередью неотправленных записей ===
        private void EnqueueOrRetry(int level, int score, int mutator, int currentRetry)
        {
            if (currentRetry >= MaxRetryCount)
            {
                Logger.LogWarning($"[LiveScore] Превышено число попыток для (lvl:{level}, score:{score})");
                return;
            }

            var existing = _pendingScores.FirstOrDefault(p => p.level == level && p.mutator == mutator);
            if (existing != null)
            {
                if (score >= existing.score)
                {
                    Logger.LogInfo($"[LiveScore] Игнорируем нелучший рекорд {score} (в очереди уже {existing.score})");
                    return;
                }
                else
                {
                    _pendingScores.Remove(existing);
                    Logger.LogInfo($"[LiveScore] Заменяем старый рекорд {existing.score} на лучший {score}");
                }
            }

            _pendingScores.Add(new PendingScore
            {
                level = level,
                score = score,
                mutator = mutator,
                retryCount = currentRetry + 1,
                nextAttemptTime = Time.time + RetryDelays[Math.Min(currentRetry, RetryDelays.Length - 1)]
            });
            SavePendingScores();
            Logger.LogInfo($"[LiveScore] Запись добавлена в очередь. Попытка #{currentRetry + 1} через {RetryDelays[Math.Min(currentRetry, RetryDelays.Length - 1)]} сек.");
        }

        private void RemovePendingScore(int level, int score, int mutator)
        {
            var item = _pendingScores.FirstOrDefault(p => p.level == level && p.mutator == mutator && p.score == score);
            if (item != null)
            {
                _pendingScores.Remove(item);
                SavePendingScores();
            }
        }

        private IEnumerator ProcessQueueCoroutine()
        {
            while (true)
            {
                if (!_isProcessing && _pendingScores.Any(p => p.nextAttemptTime <= Time.time))
                {
                    _isProcessing = true;
                    var toSend = _pendingScores.Where(p => p.nextAttemptTime <= Time.time).ToList();
                    foreach (var pending in toSend)
                    {
                        yield return StartCoroutine(SendScoreCoroutine(pending.level, pending.score, pending.mutator, pending.retryCount));
                    }
                    _isProcessing = false;
                }
                yield return new WaitForSeconds(5f);
            }
        }

        // === Вспомогательные функции ===
        private static string GetSignature(string steamId, int score)
        {
            string secret = "SeuM_RSc_pRoject_9912";
            string raw = steamId + score.ToString() + secret;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(raw));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // === Персистентная очередь (файл) с Newtonsoft.Json ===
        private void LoadPendingScores()
        {
            string path = _storagePath;
            if (!File.Exists(path))
            {
                Logger.LogInfo("[LiveScore] Файл очереди не найден, будет создан при необходимости");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                var list = JsonConvert.DeserializeObject<PendingScoreList>(json);
                if (list?.items != null && list.items.Count > 0)
                {
                    var bestPerLevel = list.items
                        .GroupBy(p => $"{p.level}_{p.mutator}")
                        .Select(g => g.OrderBy(p => p.score).First())
                        .ToList();
                    _pendingScores.Clear();
                    _pendingScores.AddRange(bestPerLevel);
                    Logger.LogInfo($"[LiveScore] Загружено {_pendingScores.Count} неотправленных записей");
                }
                else
                {
                    _pendingScores.Clear();
                    Logger.LogWarning("[LiveScore] Файл очереди пуст или повреждён");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[LiveScore] Ошибка загрузки очереди: {e.Message}");
                _pendingScores.Clear();
            }
        }

        private void SavePendingScores()
        {
            try
            {
                var wrapper = new PendingScoreList { items = _pendingScores };
                string json = JsonConvert.SerializeObject(wrapper, Formatting.Indented);
                File.WriteAllText(_storagePath, json);
            }
            catch (Exception e)
            {
                Logger.LogError($"[LiveScore] Ошибка сохранения очереди: {e.Message}");
            }
        }

        // === Классы для сериализации ===
        private class PendingScoreList
        {
            public List<PendingScore> items = new List<PendingScore>();
        }

        private class PendingScore
        {
            public int level;
            public int score;
            public int mutator;
            public int retryCount;
            public float nextAttemptTime;
        }

        private struct ScorePayload
        {
            public string steamid;
            public string name;
            public int level;
            public int score;
            public int mutator;
            public string ticket;
            public long timestamp;
        }
    }
}