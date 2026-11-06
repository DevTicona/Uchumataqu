using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using UnityEngine;

namespace Assets.Scripts.Core.Services
{
    [Serializable]
    public sealed class HuellaRecurso
    {
        public string archivo;
        public string ruta;
        public string direccion;
        public long bytes;
        public string sha256;
    }

    [Serializable]
    public sealed class ManifiestoIntegridad
    {
        public int version;
        public HuellaRecurso[] recursos;
    }

    public static class IntegridadRecursos
    {
        private static Dictionary<string, HuellaRecurso> huellas;
#if UNITY_ANDROID && !UNITY_EDITOR
        private static readonly HashSet<string> verificadosAndroid = new HashSet<string>();
        private static readonly object Acceso = new object();
#endif
        public static void Cargar()
        {
            if (huellas != null) return;
            var asset = Resources.Load<TextAsset>("IntegridadRecursos");
            if (asset == null) throw new SecurityException("Falta el manifiesto de integridad de la instalación.");
            var datos = JsonUtility.FromJson<ManifiestoIntegridad>(asset.text);
            if (datos?.version != 1 || datos.recursos == null || datos.recursos.Length != 8)
                throw new SecurityException("Manifiesto de integridad no válido.");
            var indice = new Dictionary<string, HuellaRecurso>(StringComparer.Ordinal);
            foreach (var recurso in datos.recursos)
            {
                if (recurso == null || string.IsNullOrEmpty(recurso.archivo) ||
                    recurso.sha256?.Length != 64 || recurso.bytes <= 0)
                    throw new SecurityException("Huella de recurso incompleta.");
                indice.Add(recurso.archivo, recurso);
                if (!string.IsNullOrEmpty(recurso.direccion) && recurso.direccion != recurso.archivo)
                    indice.Add(recurso.direccion, recurso);
            }
            huellas = indice;
        }

        // Integridad de recursos curriculares: tamaño y SHA-256 deben coincidir con el manifiesto local.
        // Una discrepancia detiene la carga mediante SecurityException antes de usar esos bytes.
        public static void VerificarBytes(string archivo, byte[] contenido)
        {
            Cargar();
            if (!huellas.TryGetValue(archivo, out var esperado))
                throw new SecurityException("Recurso no declarado en el manifiesto: " + archivo);
            if (contenido == null || contenido.LongLength != esperado.bytes ||
                !string.Equals(CalcularSha256(contenido), esperado.sha256, StringComparison.OrdinalIgnoreCase))
                throw new SecurityException("El recurso local no supera la comprobación de integridad: " + esperado.archivo);
        }

        public static string CalcularSha256(byte[] contenido)
        {
            using var hash = SHA256.Create();
            return BitConverter.ToString(hash.ComputeHash(contenido)).Replace("-", "").ToLowerInvariant();
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        public static void VerificarAssetAndroid(AndroidJavaObject assets, string ruta)
        {
            if (huellas == null || !huellas.TryGetValue(ruta, out var esperado))
                throw new SecurityException("El modelo ASR no está declarado en el manifiesto.");
            lock (Acceso)
            {
                if (verificadosAndroid.Contains(ruta)) return;
                using var api = new AndroidJavaClass("com.devticona.uchumataqu.SeguridadLocal");
                if (!api.CallStatic<bool>("verificarAsset", assets, ruta, esperado.sha256))
                    throw new SecurityException("El recurso ASR no supera la comprobación de integridad: " + ruta);
                verificadosAndroid.Add(ruta);
            }
        }
#endif
#if UNITY_EDITOR
        public static void LimpiarCache() => huellas = null;
#endif
    }
}
