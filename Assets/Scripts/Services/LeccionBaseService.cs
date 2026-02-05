using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections.Generic;
using Assets.Scripts.Models.Activities;

namespace Assets.Scripts.Services
{
    /// <summary>
    /// Servicio centralizado para cargar recursos Addressables de actividades y UI.
    /// Maneja: prefabs de actividades e instanciación de componentes UI dinámicos.
    /// </summary>
    public static class LeccionBaseService
    {
        // -------------------------------------------------
        // CONSTANTES - CLAVES ADDRESSABLES
        // -------------------------------------------------
        private const string CLAVE_PREFAB_BOTON = "prefab_boton";
        private const string CLAVE_PREFAB_WORD_CARD = "prefab_word_card";

        // -------------------------------------------------
        // HANDLES ACTIVOS (para liberar)
        // -------------------------------------------------
        private static readonly List<AsyncOperationHandle<GameObject>> handlesActividadPrefabs = new();
        private static readonly List<AsyncOperationHandle<GameObject>> handlesUIPrefabs = new();

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
            handlesUIPrefabs.Add(handle);

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
            foreach (var h in handlesActividadPrefabs)
                if (h.IsValid())
                    Addressables.Release(h);

            foreach (var h in handlesUIPrefabs)
                if (h.IsValid())
                    Addressables.Release(h);

            handlesActividadPrefabs.Clear();
            handlesUIPrefabs.Clear();
        }
    }
}
