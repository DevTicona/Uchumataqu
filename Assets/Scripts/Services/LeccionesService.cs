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

        // ===============================
        // Carga de iconos de lecciones
        // ===============================
        
        private const string PREFIJO_ICONO = "icono_";
        private static readonly List<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.Sprite>> handlesIconos = new();
        private static readonly Dictionary<string, UnityEngine.Sprite> cacheIconos = new();

        /// <summary>
        /// Carga un ícono de lección de forma asincrónica usando el ID de la lección.
        /// Utiliza caché para evitar cargas duplicadas.
        /// </summary>
        public static void CargarIconoLeccion(
            string idLeccion,
            System.Action<UnityEngine.Sprite> onSuccess,
            System.Action onError = null
        )
        {
            if (string.IsNullOrEmpty(idLeccion))
            {
                onSuccess?.Invoke(null);
                return;
            }

            if (cacheIconos.TryGetValue(idLeccion, out var sprite))
            {
                onSuccess?.Invoke(sprite);
                return;
            }

            string addressableKey = PREFIJO_ICONO + idLeccion;

            var handle = Addressables.LoadAssetAsync<UnityEngine.Sprite>(addressableKey);
            handlesIconos.Add(handle);

            handle.Completed += h =>
            {
                if (h.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    cacheIconos[idLeccion] = h.Result;
                    onSuccess?.Invoke(h.Result);
                }
                else
                {
                    Debug.LogWarning(
                        $"[LeccionesService] Icono no encontrado en Addressables: {addressableKey}"
                    );
                    onError?.Invoke();
                }
            };
        }

        /// <summary>
        /// Libera los recursos de iconos cargados.
        /// </summary>
        public static void LimpiarRecursosIconos()
        {
            foreach (var h in handlesIconos)
                if (h.IsValid())
                    Addressables.Release(h);

            handlesIconos.Clear();
            cacheIconos.Clear();
        }
    }
}
