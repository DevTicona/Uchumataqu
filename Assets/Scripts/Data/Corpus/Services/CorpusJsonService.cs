using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Core.Services;

namespace Assets.Scripts.Persistencia
{
    public static class CorpusJsonService
    {
        public static async Task<string> CargarTextoAsync(string direccion)
        {
            var asset = await AddressablesService.CargarAssetAsync<TextAsset>(direccion);
            if (asset != null) IntegridadRecursos.VerificarBytes(direccion, asset.bytes);
            return asset != null ? asset.text : null;
        }
#if UNITY_EDITOR
        public static string LeerJsonDirecto(string fileName)
        {
            var pathProyecto = Path.Combine(Application.dataPath, "Content", "Data", fileName);
            if (File.Exists(pathProyecto)) return LeerVerificado(pathProyecto, fileName);

            string[] possiblePaths = new string[]
            {
                Path.Combine("Assets", "Content", "Data", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Content", "Data", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "Assets", "Content", "Data", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Assets", "Content", "Data", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Content", "Data", fileName)
            };

            foreach (var p in possiblePaths)
            {
                if (File.Exists(p)) return LeerVerificado(Path.GetFullPath(p), fileName);
            }

            var pathStreaming = Path.Combine(Application.streamingAssetsPath, "Data", fileName);
            if (File.Exists(pathStreaming)) return LeerVerificado(pathStreaming, fileName);

            throw new FileNotFoundException($"[CorpusRepository] No se encontró el recurso '{fileName}'.");
        }

        private static string LeerVerificado(string ruta, string archivo)
        {
            byte[] contenido = File.ReadAllBytes(ruta);
            IntegridadRecursos.VerificarBytes(archivo, contenido);
            return System.Text.Encoding.UTF8.GetString(contenido).TrimStart('\uFEFF');
        }
#endif
    }
}
