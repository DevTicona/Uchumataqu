using System;

using Assets.Scripts.Data.Persistencia;

namespace Assets.Scripts.Persistencia
{
    /// <summary>
    /// Repositorio de guardado y carga local del progreso del usuario.
    /// Especificado en Docs/Specifications/04-flujo-leccion-app.md (Sección 11).
    /// </summary>
    public static class ProgresoRepository
    {
        public static void LimpiarCache() => _progresoCache = null;

        private const string NombreArchivo = "ProgresoUsuario.json";

        private static ProgresoUsuarioModel _progresoCache;

        public static ProgresoUsuarioModel ObtenerProgreso()
        {
            if (_progresoCache != null) return _progresoCache;
            _progresoCache = Cargar();
            return _progresoCache;
        }

        public static ProgresoUsuarioModel CargarProgreso() => ObtenerProgreso();

        private static ProgresoUsuarioModel Cargar()
        {
            try
            {
                return JsonLocalService.Leer<ProgresoUsuarioModel>(NombreArchivo) ?? new ProgresoUsuarioModel();
            }
            catch (Exception ex)
            {
                Assets.Scripts.Utils.JsonUtilityHelper.LogError($"[ProgresoRepository] Error cargando progreso: {ex.Message}");
            }

            return new ProgresoUsuarioModel();
        }

        public static bool Guardar()
        {
            ObtenerProgreso();

            try
            {
                JsonLocalService.Escribir(NombreArchivo, _progresoCache);
                return true;
            }
            catch (Exception ex)
            {
                Assets.Scripts.Utils.JsonUtilityHelper.LogError($"[ProgresoRepository] Error guardando progreso: {ex.Message}");
                _progresoCache = null;
                return false;
            }
        }

        public static bool EstaLeccionCompletada(string leccionId)
        {
            return ObtenerProgreso().LeccionCompletada(leccionId);
        }

        public static bool ReiniciarProgreso()
        {
            _progresoCache = new ProgresoUsuarioModel();
            return Guardar();
        }
    }
}
