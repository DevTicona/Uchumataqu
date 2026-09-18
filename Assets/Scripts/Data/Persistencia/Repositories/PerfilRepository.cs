using System;

using Assets.Scripts.Data.Persistencia;

namespace Assets.Scripts.Persistencia
{
    /// <summary>
    /// Repositorio de guardado y carga local del perfil del usuario.
    /// Especificado en Docs/UI/02-escena-onboarding.md (Sección 7) y 08-perfil-logros-ajustes-acerca.md.
    /// </summary>
    public static class PerfilRepository
    {
        public static void LimpiarCache() => _perfilCache = null;

        private const string NombreArchivo = "PerfilUsuario.json";

        private static PerfilUsuarioModel _perfilCache;

        public static PerfilUsuarioModel ObtenerPerfil()
        {
            if (_perfilCache != null) return _perfilCache;
            _perfilCache = Cargar();
            return _perfilCache;
        }

        public static PerfilUsuarioModel CargarPerfil() => ObtenerPerfil();

        private static PerfilUsuarioModel Cargar()
        {
            try
            {
                var perfil = JsonLocalService.Leer<PerfilUsuarioModel>(NombreArchivo);
                if (perfil != null)
                {
                    perfil.version = 2;
                    return perfil;
                }
            }
            catch (Exception ex)
            {
                Assets.Scripts.Utils.JsonUtilityHelper.LogError($"[PerfilRepository] Error cargando perfil: {ex.Message}");
            }

            return new PerfilUsuarioModel();
        }

        public static bool GuardarPerfil(PerfilUsuarioModel perfil)
        {
            if (perfil == null) throw new ArgumentNullException(nameof(perfil));

            try
            {
                JsonLocalService.Escribir(NombreArchivo, perfil);
                _perfilCache = perfil;
                return true;
            }
            catch (Exception ex)
            {
                Assets.Scripts.Utils.JsonUtilityHelper.LogError($"[PerfilRepository] Error guardando perfil: {ex.Message}");
                _perfilCache = null;
                return false;
            }
        }

    }
}
