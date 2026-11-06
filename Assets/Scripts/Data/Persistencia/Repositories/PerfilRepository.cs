using System;

using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.Util;

namespace Assets.Scripts.Persistencia
{
    /// <summary>
    /// Repositorio de guardado y carga local del perfil del usuario.
    /// Persistencia definida por la arquitectura y el flujo vigente de la aplicación.
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
                    string nombre = FormatoTexto.ConInicialMayuscula(perfil.nombre?.Trim());
                    if (perfil.nombre != nombre)
                    {
                        perfil.nombre = nombre;
                        GuardarPerfil(perfil);
                    }
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
            perfil.nombre = FormatoTexto.ConInicialMayuscula(perfil.nombre?.Trim());

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
