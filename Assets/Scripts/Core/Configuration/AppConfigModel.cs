using UnityEngine;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Configuración global de la aplicación.
    /// Contiene parámetros que pueden ajustarse desde el editor sin modificar código.
    /// </summary>
    [CreateAssetMenu(fileName = "AppConfig", menuName = "Uchumataqu/App Config")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.Core", sourceAssembly: "Assembly-CSharp", sourceClassName: "AppConfig")]
    public class AppConfigModel : ScriptableObject
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

        [Tooltip("Ganancia de las voces del corpus, cuyos picos están normalizados a -3 dB.")]
        [Range(1f, 1.4f)]
        public float defaultVoiceGain = 1.35f;

        [Range(0f, 1f)]
        public float defaultSFXVolume = 1f;

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

        [Tooltip("Desactivar para acceso libre en Editor y Development Build. La versión final siempre usa progreso.")]
        public bool enableProgressiveUnlock = true;

        public bool AccesoLibrePruebas => !enableProgressiveUnlock &&
            (Application.isEditor || Debug.isDebugBuild);

        // -------------------------------------------------------
        // AR SETTINGS
        // -------------------------------------------------------

        [Header("AR Settings")]

        [Tooltip("Muestra plano debug para AR")]
        public bool enableARDebugPlane = true;

        [Tooltip("Distancia de colocación de objetos AR")]
        public float arPlacementDistance = 1.5f;

    }
}
