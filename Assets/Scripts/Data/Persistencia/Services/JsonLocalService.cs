using System;
using System.IO;
using UnityEngine;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Persistencia
{
    /// <summary>Acceso técnico a archivos JSON de la persistencia local.</summary>
    public static class JsonLocalService
    {
#if UNITY_EDITOR
        private static string _directorioPruebas;

        public static IDisposable UsarDirectorioPruebas(string directorio)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Solo se permite aislar persistencia en pruebas de Editor.");
            string ruta = Path.GetFullPath(directorio);
            string temporal = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp")) + Path.DirectorySeparatorChar;
            if (!ruta.StartsWith(temporal, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Las pruebas deben usar un directorio temporal del proyecto.", nameof(directorio));
            return new DirectorioPruebasScope(ruta);
        }

        private sealed class DirectorioPruebasScope : IDisposable
        {
            private readonly string _anterior;
            private bool _disposed;

            public DirectorioPruebasScope(string directorio)
            {
                _anterior = _directorioPruebas;
                _directorioPruebas = directorio;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _directorioPruebas = _anterior;
                _disposed = true;
            }
        }
#endif
        public static T Leer<T>(string nombreArchivo) where T : class
        {
            string ruta = ObtenerRuta(nombreArchivo);
            return File.Exists(ruta)
                ? JsonUtilityHelper.FromJson<T>(File.ReadAllText(ruta))
                : null;
        }

        public static void Escribir<T>(string nombreArchivo, T datos) where T : class
        {
            if (datos == null) throw new ArgumentNullException(nameof(datos));
            string ruta = ObtenerRuta(nombreArchivo);
            Directory.CreateDirectory(Path.GetDirectoryName(ruta));
            File.WriteAllText(ruta, JsonUtilityHelper.ToJson(datos, prettyPrint: true));
        }

        private static string ObtenerRuta(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo) || Path.GetFileName(nombreArchivo) != nombreArchivo)
                throw new ArgumentException("Se requiere un nombre de archivo local, sin ruta.", nameof(nombreArchivo));
            string directorio = Application.persistentDataPath;
#if UNITY_EDITOR
            directorio = _directorioPruebas ?? directorio;
#endif
            return Path.Combine(directorio, nombreArchivo);
        }
    }
}
