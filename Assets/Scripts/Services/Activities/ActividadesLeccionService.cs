using UnityEngine;
using UnityEngine.AddressableAssets;
using Assets.Scripts.Models.Activities;
using Assets.Scripts.Utils;
using System.Collections.Generic;

namespace Assets.Scripts.Services.Activities
{
    public static class ActividadesLeccionService
    {
        private const string KEY_ACTIVIDADES = "yaml_actividades_lecciones";

        public static void Cargar(
            System.Action<List<ActividadesPorLeccion>> onLoaded
        )
        {
            Addressables.LoadAssetAsync<TextAsset>(KEY_ACTIVIDADES)
                .Completed += handle =>
                {
                    if (handle.Status !=
                        UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError("No se pudo cargar el YAML de actividades.");
                        onLoaded?.Invoke(null);
                        return;
                    }

                    var lista = YamlLoader
                        .LoadFromText<List<ActividadesPorLeccion>>(handle.Result.text);

                    onLoaded?.Invoke(lista);
                };
        }
    }
}
