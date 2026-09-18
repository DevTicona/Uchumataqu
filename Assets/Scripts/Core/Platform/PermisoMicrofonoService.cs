using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Assets.Scripts.Core.Sistema
{
    public static class PermisoMicrofonoService
    {
        public static bool TienePermiso()
        {
#if UNITY_ANDROID
            return Permission.HasUserAuthorizedPermission(
                Permission.Microphone
            );
#else
            return true; // Editor / PC
#endif
        }

        public static void SolicitarPermiso()
        {
#if UNITY_ANDROID
            if (!TienePermiso())
            {
                Permission.RequestUserPermission(
                    Permission.Microphone
                );
            }
#endif
        }
    }
}
