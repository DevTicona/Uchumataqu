using System;

using Assets.Scripts.Data.Persistencia;

namespace Assets.Scripts.Persistencia
{
    public static class ConfiguracionRepository
    {
        public static void LimpiarCache() => _cache = null;

        private const string NombreArchivo = "ConfiguracionUsuario.json";
        private static ConfiguracionUsuarioModel _cache;

        public static ConfiguracionUsuarioModel Obtener()
        {
            _cache ??= Cargar();
            _cache.volumen_general = UnityEngine.Mathf.Clamp01(_cache.volumen_general);
            return _cache;
        }

        public static bool Guardar(ConfiguracionUsuarioModel configuracion)
        {
            if (configuracion == null) throw new ArgumentNullException(nameof(configuracion));

            configuracion.volumen_general = UnityEngine.Mathf.Clamp01(configuracion.volumen_general);
            try
            {
                JsonLocalService.Escribir(NombreArchivo, configuracion);
                _cache = configuracion;
                return true;
            }
            catch (Exception ex)
            {
                Assets.Scripts.Utils.JsonUtilityHelper.LogError(
                    $"[ConfiguracionRepository] Error guardando configuración: {ex.Message}");
                _cache = null;
                return false;
            }
        }

        private static ConfiguracionUsuarioModel Cargar()
        {
            try
            {
                return JsonLocalService.Leer<ConfiguracionUsuarioModel>(NombreArchivo) ?? new ConfiguracionUsuarioModel();
            }
            catch (Exception ex)
            {
                Assets.Scripts.Utils.JsonUtilityHelper.LogError(
                    $"[ConfiguracionRepository] Error cargando configuración: {ex.Message}");
            }

            return new ConfiguracionUsuarioModel();
        }
    }
}
