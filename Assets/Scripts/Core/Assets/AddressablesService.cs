using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Scripts.Core.Services
{
    /// <summary>
    /// Servicio global para gestión de Addressables.
    /// Maneja carga asincrónica, cache, control de concurrencia y liberación de memoria.
    /// </summary>
    public static class AddressablesService
    {
        // =========================================
        // CACHE TIPADO
        // =========================================

        private static readonly Dictionary<(string, Type), object> cache = new();

        // =========================================
        // CONTROL DE CARGAS EN PROGRESO
        // =========================================

        private static readonly Dictionary<(string, Type), Task<object>> loadingTasks = new();

        // =========================================
        // HANDLES ACTIVOS
        // =========================================

        private static readonly List<AsyncOperationHandle> activeHandles = new();

        // =========================================
        // VALIDACIÓN
        // =========================================

        private static void ValidarKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("[AddressablesService] Key inválida.");
        }

        // =========================================
        // CARGA DE UN ASSET (CON CACHE + CONTROL)
        // =========================================

        public static async Task<T> CargarAssetAsync<T>(string key)
        {
            ValidarKey(key);

            var cacheKey = (key, typeof(T));

            // 1. Revisar cache
            if (cache.TryGetValue(cacheKey, out var cached))
            {
                return (T)cached;
            }

            // 2. Revisar si ya se está cargando
            if (loadingTasks.TryGetValue(cacheKey, out var existingTask))
            {
                return (T)await existingTask;
            }

            // 3. Crear nueva carga
            var task = CargarInterno<T>(key, cacheKey);
            loadingTasks[cacheKey] = task;

            var result = (T)await task;

            loadingTasks.Remove(cacheKey);

            return result;
        }

        private static async Task<object> CargarInterno<T>(string key, (string, Type) cacheKey)
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            activeHandles.Add(handle);

            try
            {
                var result = await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded || result == null)
                {
                    throw new Exception($"Error cargando asset: {key}");
                }

                cache[cacheKey] = result;

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddressablesService] {ex.Message}");
                throw;
            }
        }

        // =========================================
        // CARGA MÚLTIPLE
        // =========================================
        public static async Task<Dictionary<string, T>> CargarAssetsConKeyAsync<T>(string label)
        {
            ValidarKey(label);

            var dict = new Dictionary<string, T>();

            var locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
            activeHandles.Add(locationsHandle);

            var locations = await locationsHandle.Task;

            if (locations == null || locations.Count == 0)
            {
                Debug.LogWarning($"[AddressablesService] No se encontraron assets para label: {label}");
                return dict;
            }

            foreach (var loc in locations)
            {
                var handle = Addressables.LoadAssetAsync<T>(loc);
                activeHandles.Add(handle);

                try
                {
                    var asset = await handle.Task;

                    if (handle.Status == AsyncOperationStatus.Succeeded && asset != null)
                    {
                        dict[loc.PrimaryKey] = asset;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[AddressablesService] Error cargando {loc.PrimaryKey}: {ex.Message}");
                }
            }

            return dict;
        }
        /// <summary>
        /// Instancia un prefab UI dentro de un contenedor (LayoutGroup, Panel, etc.)
        /// </summary>
        public static async Task<GameObject> InstanciarUIAsync(
            string key,
            Transform parent)
        {
            ValidarKey(key);

            if (parent == null)
                throw new ArgumentNullException(nameof(parent), "[AddressablesService] Parent UI es null.");

            var handle = Addressables.InstantiateAsync(key, parent);
            activeHandles.Add(handle);

            try
            {
                var instance = await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded || instance == null)
                {
                    throw new Exception($"Error instanciando UI prefab: {key}");
                }

                // Ajuste importante para UI
                var rect = instance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.localScale = Vector3.one;
                    rect.anchoredPosition = Vector2.zero;
                }

                return instance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddressablesService] {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Instancia un prefab en el mundo (3D o AR)
        /// </summary>
        public static async Task<GameObject> InstanciarMundoAsync(
            string key,
            Vector3 posicion,
            Quaternion rotacion,
            Transform parent = null)
        {
            ValidarKey(key);

            var handle = Addressables.InstantiateAsync(key, posicion, rotacion, parent);
            activeHandles.Add(handle);

            try
            {
                var instance = await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded || instance == null)
                {
                    throw new Exception($"Error instanciando prefab mundo: {key}");
                }

                return instance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddressablesService] {ex.Message}");
                throw;
            }
        }
        // =========================================
        // JSON (CLAVE PARA TU APP)
        // =========================================

        public static async Task<T> CargarJsonAsync<T>(string key)
        {
            var textAsset = await CargarAssetAsync<TextAsset>(key);

            try
            {
                return JsonUtility.FromJson<T>(textAsset.text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddressablesService] Error parseando JSON: {key} - {ex.Message}");
                throw;
            }
        }

        // =========================================
        // LIBERACIÓN INDIVIDUAL
        // =========================================

        public static void LiberarAsset<T>(string key)
        {
            var cacheKey = (key, typeof(T));

            if (cache.TryGetValue(cacheKey, out var asset))
            {
                Addressables.Release(asset);
                cache.Remove(cacheKey);
            }
        }

        // =========================================
        // LIBERAR INSTANCIA
        // =========================================

        public static void LiberarInstancia(GameObject instance)
        {
            if (instance == null)
            {
                Debug.LogWarning("[AddressablesService] Intento de liberar instancia null.");
                return;
            }

            Addressables.ReleaseInstance(instance);
        }

        // =========================================
        // LIMPIAR CACHE
        // =========================================

        public static void LimpiarCacheCompleto()
        {
            foreach (var kvp in cache)
            {
                Addressables.Release(kvp.Value);
            }

            cache.Clear();
        }

        // =========================================
        // LIBERAR TODO
        // =========================================

        public static void LiberarRecursos()
        {
            foreach (var handle in activeHandles)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            activeHandles.Clear();
            LimpiarCacheCompleto();
            loadingTasks.Clear();
        }
    }
}

namespace Assets.Scripts.Core
{
    public static class ContentAddress
    {
        public const string DatosPalabras = "json_palabras";
        public const string DatosOraciones = "json_oraciones";
        public const string DatosLecciones = "json_lecciones";
        public const string DatosEscenariosRa = "json_escenarios_ra";
        public const string PersonajeBase = "personaje_base";

        public const string EtiquetaIslas = "islands";

        public static string Isla(string nombre) => $"imagen/isla/{ValidarSegmento(nombre)}";

        private static string ValidarSegmento(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("El segmento de la dirección Addressable no puede estar vacío.");

            return valor.Trim().Replace(" ", "_").Replace("-", "_").ToLowerInvariant();
        }
    }
}
