using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Assets.Scripts.Models.Activities;

namespace Assets.Scripts.Services.Activities
{
    public static class ActividadesCatalogoService
    {
        private const string ADDRESSABLE_CATALOGO = "Actividades_Catalogo";

        private static Dictionary<string, ActividadCatalogoData> cache;

        public static void Cargar(Action onCompletado)
        {
            if (cache != null)
            {
                onCompletado?.Invoke();
                return;
            }

            Addressables.LoadAssetAsync<TextAsset>(ADDRESSABLE_CATALOGO)
                .Completed += handle =>
                {
                    cache = new Dictionary<string, ActividadCatalogoData>();

                    var wrapper = JsonUtility.FromJson<CatalogoWrapper>(
                        handle.Result.text
                    );

                    foreach (var actividad in wrapper.actividades)
                        cache[actividad.idActividad] = actividad;

                    onCompletado?.Invoke();
                };
        }

        public static string ObtenerTitulo(string idActividad)
        {
            if (cache != null &&
                cache.TryGetValue(idActividad, out var data))
            {
                return data.titulo;
            }

            return string.Empty;
        }

        [Serializable]
        private class CatalogoWrapper
        {
            public List<ActividadCatalogoData> actividades;
        }
    }
}
