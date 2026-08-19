using UnityEngine;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Configuración global de la aplicación.
    /// Contiene parámetros que pueden ajustarse desde el editor sin modificar código.
    /// </summary>
    [CreateAssetMenu(fileName = "AppConfig", menuName = "Uchumataqu/App Config")]
    public class AppConfig : ScriptableObject
    {
        // -------------------------------------------------------
        // DEBUG
        // -------------------------------------------------------

        [Header("Debug")]
        [Tooltip("Activa logs básicos del sistema")]
        public bool enableDebugLogs = true;

        [Tooltip("Activa logs detallados para diagnóstico")]
        public bool enableVerboseLogs = false;

        // -------------------------------------------------------
        // AUDIO
        // -------------------------------------------------------

        [Header("Audio")]

        [Range(0f, 1f)]
        public float defaultMasterVolume = 1.0f;

        [Range(0f, 1f)]
        public float defaultWordVolume = 1.0f;

        [Range(0f, 1f)]
        public float defaultSFXVolume = 0.8f;

        [Range(0f, 1f)]
        public float defaultMusicVolume = 0.5f;

        // -------------------------------------------------------
        // UI ANIMATION
        // -------------------------------------------------------

        [Header("UI Animation")]

        [Range(0.1f, 2f)]
        public float fadeInDuration = 0.5f;

        [Range(0.1f, 1f)]
        public float cardAnimDuration = 0.3f;

        [Range(0.05f, 0.5f)]
        public float buttonScaleDuration = 0.2f;

        // -------------------------------------------------------
        // GAMEPLAY
        // -------------------------------------------------------

        [Header("Gameplay")]

        [Tooltip("Si está activo, desbloquea lecciones progresivamente")]
        public bool enableProgressiveUnlock = false;

        // -------------------------------------------------------
        // AR SETTINGS
        // -------------------------------------------------------

        [Header("AR Settings")]

        [Tooltip("Muestra plano debug para AR")]
        public bool enableARDebugPlane = true;

        [Tooltip("Distancia de colocación de objetos AR")]
        public float arPlacementDistance = 1.5f;

        // -------------------------------------------------------
        // SINGLETON (CONFIGURACIÓN GLOBAL)
        // -------------------------------------------------------

        private static AppConfig _instance;

        /// <summary>
        /// Acceso global a la configuración de la aplicación.
        /// El asset debe estar dentro de Resources/AppConfig.asset
        /// </summary>
        public static AppConfig Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = Resources.Load<AppConfig>("AppConfig");

                if (_instance == null)
                {
                    Debug.LogError(
                        "[AppConfig] No se encontró AppConfig en Resources/. " +
                        "Debes crear el asset en: Assets/Resources/AppConfig.asset"
                    );
                }

                return _instance;
            }
        }
    }
}
