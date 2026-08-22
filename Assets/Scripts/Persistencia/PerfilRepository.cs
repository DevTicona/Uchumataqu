using System;
using System.IO;
using UnityEngine;
using Assets.Scripts.Data.Persistencia;

namespace Assets.Scripts.Persistencia
{
    /// <summary>
    /// Repositorio de guardado y carga local del perfil del usuario.
    /// Especificado en Docs/UI/02-escena-onboarding.md (Sección 7) y 08-perfil-logros-ajustes-acerca.md.
    /// </summary>
    public static class PerfilRepository
    {
        private static string RutaArchivo => Path.Combine(Application.persistentDataPath, "PerfilUsuario.json");

        private static PerfilUsuario _perfilCache;

        public static bool ExistePerfil()
        {
            var perfil = ObtenerPerfil();
            return perfil != null && perfil.onboarding_completado && !string.IsNullOrWhiteSpace(perfil.nombre);
        }

        public static PerfilUsuario ObtenerPerfil()
        {
            if (_perfilCache != null) return _perfilCache;
            _perfilCache = Cargar();
            return _perfilCache;
        }

        private static PerfilUsuario Cargar()
        {
            try
            {
                if (File.Exists(RutaArchivo))
                {
                    string json = File.ReadAllText(RutaArchivo);
                    var perfil = JsonUtility.FromJson<PerfilUsuario>(json);
                    if (perfil != null) return perfil;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PerfilRepository] Error cargando perfil: {ex.Message}");
            }

            return new PerfilUsuario();
        }

        public static void GuardarPerfil(PerfilUsuario perfil)
        {
            _perfilCache = perfil;

            try
            {
                string json = JsonUtility.ToJson(_perfilCache, prettyPrint: true);
                File.WriteAllText(RutaArchivo, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PerfilRepository] Error guardando perfil: {ex.Message}");
            }
        }

        public static void CrearOActualizar(string nombre, int edad, string rangoEdad, string avatarId, bool onboardingCompletado = true)
        {
            var perfil = ObtenerPerfil();
            perfil.nombre = nombre?.Trim() ?? "";
            perfil.edad = edad;
            perfil.rango_edad = rangoEdad;
            perfil.avatar_id = avatarId;
            perfil.onboarding_completado = onboardingCompletado;
            GuardarPerfil(perfil);
        }
    }
}
