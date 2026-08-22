using System;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    /// <summary>
    /// Utilidad segura de serialización JSON con soporte dual Unity / System.Text.Json
    /// configurado con IncludeFields = true para campos públicos serializables.
    /// </summary>
    public static class JsonUtilityHelper
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default;

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(json, _jsonOptions);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[JsonUtilityHelper] Error deserializando: {ex.Message}");
                    return default;
                }
            }
        }

        public static string ToJson(object obj, bool prettyPrint = false)
        {
            if (obj == null) return "{}";

            try
            {
                return JsonUtility.ToJson(obj, prettyPrint);
            }
            catch
            {
                try
                {
                    return JsonSerializer.Serialize(obj, _jsonOptions);
                }
                catch
                {
                    return "{}";
                }
            }
        }

        public static void LogError(string message)
        {
            try
            {
                Debug.LogError(message);
            }
            catch
            {
                Console.WriteLine($"[ERROR] {message}");
            }
        }

        public static void LogWarning(string message)
        {
            try
            {
                Debug.LogWarning(message);
            }
            catch
            {
                Console.WriteLine($"[WARN] {message}");
            }
        }

        public static void LogInfo(string message)
        {
            try
            {
                Debug.Log(message);
            }
            catch
            {
                Console.WriteLine($"[INFO] {message}");
            }
        }
    }
}
