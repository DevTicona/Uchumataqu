using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Assets.Scripts.Models;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Services
{
    /// <summary>
    /// Servicio encargado de cargar y proveer información de las lecciones.
    /// Usa Addressables + YAML y mantiene cache en memoria.
    /// </summary>
    public static class LeccionesService
    {
        private static List<LeccionData> cache;

        // ===============================
        // Carga completa de lecciones
        // ===============================
        public static void ObtenerLecciones(
            System.Action<List<LeccionData>> callback
        )
        {
            if (cache != null)
            {
                callback?.Invoke(cache);
                return;
            }

            Addressables
                .LoadAssetAsync<TextAsset>("yaml_lecciones")
                .Completed += handle =>
                {
                    if (handle.Result == null)
                    {
                        Debug.LogError(
                            "[LeccionesService] No se pudo cargar yaml_lecciones_iniciales"
                        );
                        callback?.Invoke(null);
                        return;
                    }

                    var lista = YamlLoader
                        .LoadFromText<List<LeccionData>>(handle.Result.text)
                        .OrderBy(l => l.orden)
                        .ToList();

                    cache = lista;
                    callback?.Invoke(lista);
                };
        }

        // ===============================
        // Obtención de una lección por ID
        // ===============================
        public static void ObtenerLeccionPorId(
            string idLeccion,
            System.Action<LeccionData> callback
        )
        {
            if (string.IsNullOrEmpty(idLeccion))
            {
                callback?.Invoke(null);
                return;
            }

            ObtenerLecciones(lista =>
            {
                var leccion = lista?.FirstOrDefault(l => l.id == idLeccion);

                if (leccion == null)
                {
                    Debug.LogError(
                        $"[LeccionesService] No se encontró la lección con id: {idLeccion}"
                    );
                }

                callback?.Invoke(leccion);
            });
        }
    }
}
