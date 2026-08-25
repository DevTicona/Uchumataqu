using System;
using System.IO;
using UnityEngine;
using Assets.Scripts.Data.Persistencia;

namespace Assets.Scripts.Persistencia
{
    public static class ConfiguracionRepository
    {
        private const string NombreArchivo = "ConfiguracionUsuario.json";
        private static ConfiguracionUsuario _cache;

        private static string RutaArchivo => Path.Combine(Application.persistentDataPath, NombreArchivo);

        public static ConfiguracionUsuario Obtener()
        {
            return _cache ??= Cargar();
        }

        public static void Guardar(ConfiguracionUsuario configuracion)
        {
            if (configuracion == null) throw new ArgumentNullException(nameof(configuracion));

            configuracion.volumen_general = Mathf.Clamp01(configuracion.volumen_general);
            _cache = configuracion;
            try
            {
                string json = Assets.Scripts.Utils.JsonUtilityHelper.ToJson(configuracion, prettyPrint: true);
                File.WriteAllText(RutaArchivo, json);
            }
            catch (Exception ex)
            {
                Assets.Scripts.Utils.JsonUtilityHelper.LogError(
                    $"[ConfiguracionRepository] Error guardando configuración: {ex.Message}");
            }
        }

        public static void CambiarVolumen(float volumen)
        {
            ConfiguracionUsuario configuracion = Obtener();
            configuracion.volumen_general = Mathf.Clamp01(volumen);
            Guardar(configuracion);
        }

        private static ConfiguracionUsuario Cargar()
        {
            try
            {
                if (File.Exists(RutaArchivo))
                {
                    string json = File.ReadAllText(RutaArchivo);
                    ConfiguracionUsuario configuracion =
                        Assets.Scripts.Utils.JsonUtilityHelper.FromJson<ConfiguracionUsuario>(json);
                    if (configuracion != null)
                    {
                        configuracion.volumen_general = Mathf.Clamp01(configuracion.volumen_general);
                        return configuracion;
                    }
                }
            }
            catch (Exception ex)
            {
                Assets.Scripts.Utils.JsonUtilityHelper.LogError(
                    $"[ConfiguracionRepository] Error cargando configuración: {ex.Message}");
            }

            return new ConfiguracionUsuario();
        }
    }
}
