using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections.Generic;
using Assets.Scripts.Models.Activities;

namespace Assets.Scripts.Services
{
    /// <summary>
    /// Servicio centralizado para cargar recursos Addressables de lecciones.
    /// Maneja: iconos de lecciones, prefabs de actividades e instanciación de actividades.
    /// </summary>
    public static class LeccionBaseService
    {
        // -------------------------------------------------
        // CONSTANTES - CLAVES ADDRESSABLES
        // -------------------------------------------------
        private const string PREFIJO_ICONO = "icono_";
        private const string CLAVE_PREFAB_BOTON = "prefab_boton";
        private const string CLAVE_PREFAB_WORD_CARD = "prefab_word_card";

        // Handles activos (para liberar)
        private static readonly List<AsyncOperationHandle<Sprite>> handlesIconos = new();
        private static readonly List<AsyncOperationHandle<GameObject>> handlesActividadPrefabs = new();
        private static readonly List<AsyncOperationHandle<GameObject>> handlesBotonPrefabs = new();

        // Cache de sprites
        private static readonly Dictionary<string, Sprite> cacheIconos = new();

        // -------------------------------------------------
        // ICONOS DE LECCIONES
        // -------------------------------------------------

        /// <summary>
        /// Carga un ícono de lección de forma asincrónica.
        /// Utiliza caché para evitar cargas duplicadas.
        /// </summary>
        public static void CargarIconoLeccion(
            string idIcono,
            System.Action<Sprite> onSuccess,
            System.Action onError = null
        )
        {
            if (string.IsNullOrEmpty(idIcono))
            {
                onSuccess?.Invoke(null);
                return;
            }

            if (cacheIconos.TryGetValue(idIcono, out var sprite))
            {
                onSuccess?.Invoke(sprite);
                return;
            }

            string addressableKey = PREFIJO_ICONO + idIcono;

            var handle = Addressables.LoadAssetAsync<Sprite>(addressableKey);
            handlesIconos.Add(handle);

            handle.Completed += h =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    cacheIconos[idIcono] = h.Result;
                    onSuccess?.Invoke(h.Result);
                }
                else
                {
                    Debug.LogError(
                        $"[LeccionBaseService] Icono no encontrado en Addressables: {addressableKey}"
                    );
                    onError?.Invoke();
                }
            };
        }

        // -------------------------------------------------
        // PREFABS DE ACTIVIDADES
        // -------------------------------------------------

        /// <summary>
        /// Obtiene la ruta Addressable para un tipo de actividad.
        /// </summary>
        public static string ObtenerAddressableActividad(TipoActividad tipo)
        {
            return tipo.id switch
            {
                "SeleccionSimple" => "prefab_actividad_seleccion",
                "EmparejamientoPalabras" => "prefab_actividad_emparejamiento",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Instancia de forma asincrónica un prefab de actividad.
        /// </summary>
        public static void InstanciarActividad(
            TipoActividad tipo,
            Transform parent,
            System.Action<GameObject> onSuccess,
            System.Action onError = null
        )
        {
            string addressableKey = ObtenerAddressableActividad(tipo);

            if (string.IsNullOrEmpty(addressableKey))
            {
                Debug.LogError($"[LeccionBaseService] Tipo de actividad desconocido: {tipo.id}");
                onError?.Invoke();
                return;
            }

            var handle = Addressables.InstantiateAsync(addressableKey, parent);
            handlesActividadPrefabs.Add(handle);

            handle.Completed += h =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    onSuccess?.Invoke(h.Result);
                }
                else
                {
                    Debug.LogError(
                        $"[LeccionBaseService] No se pudo instanciar actividad: {addressableKey}"
                    );
                    onError?.Invoke();
                }
            };
        }

        // -------------------------------------------------
        // PREFABS DE UI DINÁMICOS
        // -------------------------------------------------

        /// <summary>
        /// Instancia un botón de forma asincrónica.
        /// </summary>
        public static void InstanciarBoton(
            Transform parent,
            System.Action<GameObject> onSuccess,
            System.Action onError = null
        )
        {
            InstanciarPrefabUI(CLAVE_PREFAB_BOTON, parent, onSuccess, onError);
        }

        /// <summary>
        /// Instancia una Word Card de forma asincrónica.
        /// </summary>
        public static void InstanciarWordCard(
            Transform parent,
            System.Action<GameObject> onSuccess,
            System.Action onError = null
        )
        {
            InstanciarPrefabUI(CLAVE_PREFAB_WORD_CARD, parent, onSuccess, onError);
        }

        /// <summary>
        /// Instancia un prefab genérico de UI.
        /// </summary>
        private static void InstanciarPrefabUI(
            string clave,
            Transform parent,
            System.Action<GameObject> onSuccess,
            System.Action onError = null
        )
        {
            if (string.IsNullOrEmpty(clave) || parent == null)
            {
                Debug.LogError($"[LeccionBaseService] Clave inválida '{clave}' o parent nulo.");
                onError?.Invoke();
                return;
            }

            var handle = Addressables.InstantiateAsync(clave, parent);
            handlesBotonPrefabs.Add(handle);

            handle.Completed += h =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    onSuccess?.Invoke(h.Result);
                }
                else
                {
                    Debug.LogError($"[LeccionBaseService] No se pudo instanciar: {clave}");
                    onError?.Invoke();
                }
            };
        }

        // -------------------------------------------------
        // LIMPIEZA
        // -------------------------------------------------

        /// <summary>
        /// Libera todos los recursos Addressables cargados.
        /// Llamar en OnDestroy o cuando se navegue fuera de la escena.
        /// </summary>
        public static void LimpiarRecursos()
        {
            foreach (var h in handlesIconos)
                if (h.IsValid())
                    Addressables.Release(h);

            foreach (var h in handlesActividadPrefabs)
                if (h.IsValid())
                    Addressables.Release(h);

            foreach (var h in handlesBotonPrefabs)
                if (h.IsValid())
                    Addressables.Release(h);

            handlesIconos.Clear();
            handlesActividadPrefabs.Clear();
            handlesBotonPrefabs.Clear();
        }

        /// <summary>
        /// Limpia el caché de iconos.
        /// </summary>
        public static void LimpiarCacheIconos()
        {
            cacheIconos.Clear();
        }
    }
}
