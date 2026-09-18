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
            return asset != null ? asset.text : null;
        }
#if UNITY_EDITOR
        public static string LeerJsonDirecto(string fileName)
        {
            var pathProyecto = Path.Combine(Application.dataPath, "Content", "Data", fileName);
            if (File.Exists(pathProyecto)) return File.ReadAllText(pathProyecto);

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
                if (File.Exists(p)) return File.ReadAllText(Path.GetFullPath(p));
            }

            var pathStreaming = Path.Combine(Application.streamingAssetsPath, "Data", fileName);
            if (File.Exists(pathStreaming)) return File.ReadAllText(pathStreaming);

            throw new FileNotFoundException($"[CorpusRepository] No se encontró el recurso '{fileName}'.");
        }
#endif
    }
}
