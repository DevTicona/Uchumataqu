using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Assets.Scripts.Data.Persistencia;
using UnityEngine;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Persistencia
{
    /// <summary>Acceso técnico a archivos JSON de la persistencia local.</summary>
    public static class JsonLocalService
    {
        private static readonly object Acceso = new object();
        private static readonly HashSet<string> DirectoriosPreparados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
#if UNITY_EDITOR || DEVELOPMENT_BUILD || UCHUMATAQU_VALIDACION
#if UNITY_EDITOR
        private const string DirectorioSesion = "Uchumataqu.Persistencia.DirectorioPruebas";
        private static string _directorioPruebas = LeerDirectorioSesion();

        private static string LeerDirectorioSesion()
        {
            string ruta = UnityEditor.SessionState.GetString(DirectorioSesion, string.Empty);
            return string.IsNullOrEmpty(ruta) ? null : ruta;
        }

        public static void RestablecerDirectorioPruebas() => EstablecerDirectorioPruebas(null);
#else
        private static string _directorioPruebas;
#endif

        private static void EstablecerDirectorioPruebas(string directorio)
        {
            _directorioPruebas = directorio;
#if UNITY_EDITOR
            if (directorio == null) UnityEditor.SessionState.EraseString(DirectorioSesion);
            else UnityEditor.SessionState.SetString(DirectorioSesion, directorio);
#endif
        }

#if UNITY_EDITOR
        public static IDisposable UsarDirectorioPruebas(string directorio)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Solo se permite aislar persistencia en pruebas de Editor.");
            string ruta = Path.GetFullPath(directorio);
            string temporal = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp")) + Path.DirectorySeparatorChar;
            if (!ruta.StartsWith(temporal, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Las pruebas deben usar un directorio temporal del proyecto.", nameof(directorio));
            return new DirectorioPruebasScope(ruta);
        }
#endif

#if UNITY_ANDROID && (DEVELOPMENT_BUILD || UCHUMATAQU_VALIDACION) && !UNITY_EDITOR
        public static IDisposable UsarDirectorioValidacionDispositivo() =>
            new DirectorioPruebasScope(Path.Combine(Application.persistentDataPath,
                "ValidacionDispositivo", Guid.NewGuid().ToString("N")));
#endif

        private sealed class DirectorioPruebasScope : IDisposable
        {
            private readonly string _anterior;
            private bool _disposed;

            public DirectorioPruebasScope(string directorio)
            {
                _anterior = _directorioPruebas;
                EstablecerDirectorioPruebas(directorio);
            }

            public void Dispose()
            {
                if (_disposed) return;
                EstablecerDirectorioPruebas(_anterior);
                _disposed = true;
            }
        }
#endif
        /// <summary>
        /// Recupera el archivo principal o un respaldo/temporal autenticado. Un candidato
        /// corrupto nunca se deserializa; la operación completa se serializa con el guardado.
        /// </summary>
        public static T Leer<T>(string nombreArchivo) where T : class
        {
            string ruta = ObtenerRuta(nombreArchivo);
            lock (Acceso)
            {
                PrepararDirectorio(Path.GetDirectoryName(ruta));
                Exception error = null;
                foreach (string candidato in new[] { ruta, ruta + ".bak", ruta + ".tmp" })
                {
                    if (!File.Exists(candidato)) continue;
                    try
                    {
                        byte[] contenido = File.ReadAllBytes(candidato);
                        string json = Utf8.GetString(ProteccionDatosLocal.Descifrar(contenido, nombreArchivo));
                        T datos = JsonUtility.FromJson<T>(json);
                        if (datos == null) throw new InvalidDataException("Contenido persistido vacío.");
                        if (candidato != ruta) EscrituraAtomica.Guardar(ruta, contenido, conservarRespaldo: false);
                        return datos;
                    }
                    catch (Exception ex) when (ex is IOException || ex is CryptographicException ||
                        ex is ArgumentException || ex is DecoderFallbackException)
                    {
                        error = ex;
                    }
                }
                if (error != null) throw new InvalidDataException("La persistencia no supera la comprobación de integridad.", error);
                return null;
            }
        }

        public static void Escribir<T>(string nombreArchivo, T datos) where T : class
        {
            if (datos == null) throw new ArgumentNullException(nameof(datos));
            string ruta = ObtenerRuta(nombreArchivo);
            lock (Acceso)
            {
                PrepararDirectorio(Path.GetDirectoryName(ruta));
                byte[] contenido = ProteccionDatosLocal.Cifrar(Utf8.GetBytes(JsonUtilityHelper.ToJson(datos, prettyPrint: true)), nombreArchivo);
                EscrituraAtomica.Guardar(ruta, contenido);
            }
        }

        private static void PrepararDirectorio(string directorio)
        {
            if (DirectoriosPreparados.Contains(directorio)) return;
            const string marca = ".persistencia-segura-v1";
            string rutaMarca = Path.Combine(directorio, marca);
            bool directorioPersonal = string.Equals(Path.GetFullPath(directorio),
                Path.GetFullPath(Application.persistentDataPath), StringComparison.OrdinalIgnoreCase);
            bool finalizada = directorioPersonal && ProteccionDatosLocal.MigracionFinalizadaEnSistema();
            if (!finalizada && File.Exists(rutaMarca))
                finalizada = Utf8.GetString(ProteccionDatosLocal.Descifrar(File.ReadAllBytes(rutaMarca), marca)) == "SEC1";
            if (!finalizada)
            {
                MigrarArchivo<PerfilUsuarioModel>(directorio, "PerfilUsuario.json");
                MigrarArchivo<ProgresoUsuarioModel>(directorio, "ProgresoUsuario.json");
                MigrarArchivo<ConfiguracionUsuarioModel>(directorio, "ConfiguracionUsuario.json");
            }
            if (!File.Exists(rutaMarca))
                EscrituraAtomica.Guardar(rutaMarca, ProteccionDatosLocal.Cifrar(Utf8.GetBytes("SEC1"), marca), false);
            if (directorioPersonal) ProteccionDatosLocal.FinalizarMigracionEnSistema();
            DirectoriosPreparados.Add(directorio);
        }

        private static void MigrarArchivo<T>(string directorio, string nombre) where T : class
        {
            foreach (string sufijo in new[] { "", ".bak" })
            {
                string ruta = Path.Combine(directorio, nombre + sufijo);
                if (!File.Exists(ruta)) continue;
                byte[] datos = File.ReadAllBytes(ruta);
                if (ProteccionDatosLocal.EstaProtegido(datos)) continue;
                string json = Utf8.GetString(datos).TrimStart('\uFEFF');
                if (JsonUtility.FromJson<T>(json) == null) throw new InvalidDataException("No se puede migrar la persistencia previa.");
                EscrituraAtomica.Guardar(ruta, ProteccionDatosLocal.Cifrar(Utf8.GetBytes(json), nombre), false);
            }
        }

        private static string ObtenerRuta(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo) || nombreArchivo.Length > 128 ||
                Path.GetFileName(nombreArchivo) != nombreArchivo || nombreArchivo.Contains("\\") || nombreArchivo.Contains("/") ||
                nombreArchivo.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                !nombreArchivo.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Se requiere un nombre de archivo local, sin ruta.", nameof(nombreArchivo));
            string directorio = Application.persistentDataPath;
#if UNITY_EDITOR || DEVELOPMENT_BUILD || UCHUMATAQU_VALIDACION
            directorio = _directorioPruebas ?? directorio;
#endif
            return Path.Combine(directorio, nombreArchivo);
        }
    }
}
