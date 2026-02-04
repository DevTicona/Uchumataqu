using UnityEngine;

namespace Assets.Scripts.Core
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "Uchumataqu/App Config")]
    public class AppConfig : ScriptableObject
    {
        // -------------------------------------------------------
        // Debug
        // -------------------------------------------------------
        [Header("Debug")]
        public bool enableDebugLogs = true;
        public bool enableVerboseLogs = false;

        // -------------------------------------------------------
        // Audio
        // -------------------------------------------------------
        [Header("Audio")]
        [Range(0f, 1f)] public float defaultMasterVolume = 1.0f;
        [Range(0f, 1f)] public float defaultWordVolume = 1.0f;
        [Range(0f, 1f)] public float defaultSFXVolume = 0.8f;
        [Range(0f, 1f)] public float defaultMusicVolume = 0.5f;

        // -------------------------------------------------------
        // UI Animation
        // -------------------------------------------------------
        [Header("UI Animation")]
        [Range(0.1f, 2f)] public float fadeInDuration = 0.5f;
        [Range(0.1f, 1f)] public float cardAnimDuration = 0.3f;
        [Range(0.05f, 0.5f)] public float buttonScaleDuration = 0.2f;

        // -------------------------------------------------------
        // Gameplay
        // -------------------------------------------------------
        [Header("Gameplay")]
        public int minReviewsToMaster = 3;
        public bool enableProgressiveUnlock = false;

        // -------------------------------------------------------
        // AR Settings
        // -------------------------------------------------------
        [Header("AR Settings")]
        public bool enableARDebugPlane = true;
        public float arPlacementDistance = 1.5f;

        // -------------------------------------------------------
        // Singleton (Read-Only Config)
        // -------------------------------------------------------
        private static AppConfig _instance;

        public static AppConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<AppConfig>("AppConfig");

                    if (_instance == null)
                    {
                        Debug.LogError(
                            "[AppConfig] No se encontró AppConfig en Resources/. " +
                            "Crea un asset AppConfig para evitar valores inconsistentes."
                        );
                    }
                }

                return _instance;
            }
        }
    }
}
