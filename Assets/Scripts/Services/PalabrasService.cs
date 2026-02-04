using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Assets.Scripts.Models;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Services
{
    public static class PalabrasService
    {
        private static List<PalabraData> cache;

        public static void ObtenerPalabrasPorLeccion(
            string idLeccion,
            System.Action<List<PalabraData>> callback
        )
        {
            // Si ya cargamos antes, solo filtramos
            if (cache != null)
            {
                callback?.Invoke(
                    cache.Where(p => p.leccion == idLeccion).ToList()
                );
                return;
            }

            Addressables
                .LoadAssetAsync<TextAsset>("json_palabras")
                .Completed += handle =>
                {
                    var todas = JsonUtilityHelper
                        .FromJsonList<PalabraData>(handle.Result.text);

                    cache = todas;

                    callback?.Invoke(
                        todas.Where(p => p.leccion == idLeccion).ToList()
                    );
                };
        }
    }
}
