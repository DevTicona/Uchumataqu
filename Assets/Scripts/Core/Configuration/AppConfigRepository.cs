using UnityEngine;

namespace Assets.Scripts.Core
{
    public static class AppConfigRepository
    {
        private static AppConfigModel _cache;

        public static bool AccesoLibrePruebas => Obtener() != null && Obtener().AccesoLibrePruebas;

        public static AppConfigModel Obtener()
        {
            if (_cache != null) return _cache;
            _cache = Resources.Load<AppConfigModel>("AppConfig");
            if (_cache == null) Debug.LogError("[AppConfigRepository] Falta el asset Assets/Resources/AppConfig.asset.");
            return _cache;
        }
    }
}
