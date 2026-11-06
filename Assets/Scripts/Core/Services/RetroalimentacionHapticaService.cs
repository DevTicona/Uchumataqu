using Assets.Scripts.Persistencia;
using UnityEngine;

namespace Assets.Scripts.Core.Services
{
    public static class RetroalimentacionHapticaService
    {
        private static float ultimaConfirmacion = float.NegativeInfinity;

        public static void ConfirmarColocacion() => EmitirPulso(25L);

        public static void ConfirmarSuperficieDetectada() => EmitirPulso(45L);

        private static void EmitirPulso(long duracion)
        {
            if (!Application.isMobilePlatform || !ConfiguracionRepository.Obtener().vibracion ||
                Time.unscaledTime - ultimaConfirmacion < 0.2f) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var actividad = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var vibrador = actividad.Call<AndroidJavaObject>("getSystemService", "vibrator");
                if (vibrador == null || !vibrador.Call<bool>("hasVibrator")) return;
                using var efectos = new AndroidJavaClass("android.os.VibrationEffect");
                using var pulso = efectos.CallStatic<AndroidJavaObject>("createOneShot", duracion, -1);
                vibrador.Call("vibrate", pulso);
                ultimaConfirmacion = Time.unscaledTime;
            }
            catch (AndroidJavaException ex)
            {
                Debug.LogWarning("[HAPTICA] Confirmación táctil no disponible: " + ex.Message);
            }
#endif
        }
    }
}
