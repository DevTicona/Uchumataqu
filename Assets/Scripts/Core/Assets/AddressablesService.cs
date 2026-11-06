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

        private static readonly Dictionary<(string, Type), AsyncOperationHandle> cachedHandles = new();
        private static readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> instanceHandles = new();
        private static int resourceVersion;

        /// <summary>Carga con propiedad explícita, independiente de la caché compartida.</summary>
        public static async Task<RecursoAddressable<T>> CargarTemporalAsync<T>(string key) where T : UnityEngine.Object
        {
            ValidarKey(key);
            var handle = Addressables.LoadAssetAsync<T>(key);
            try
            {
                var asset = await handle.Task;
                if (handle.Status != AsyncOperationStatus.Succeeded || asset == null)
                    throw new InvalidOperationException($"Error cargando asset temporal: {key}");
                return new RecursoAddressable<T>(handle, asset);
            }
            catch
            {
                if (handle.IsValid()) Addressables.Release(handle);
                throw;
            }
        }

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

            try
            {
                return (T)await task;
            }
            finally
            {
                if (loadingTasks.TryGetValue(cacheKey, out var current) && current == task)
                    loadingTasks.Remove(cacheKey);
            }
        }

        private static async Task<object> CargarInterno<T>(string key, (string, Type) cacheKey)
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            int version = resourceVersion;

            try
            {
                var result = await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded || result == null)
                {
                    throw new Exception($"Error cargando asset: {key}");
                }

                if (version != resourceVersion)
                    throw new OperationCanceledException("Los recursos se liberaron durante la carga.");

                cache[cacheKey] = result;
                cachedHandles[cacheKey] = handle;

                return result;
            }
            catch (Exception ex)
            {
                if (handle.IsValid()) Addressables.Release(handle);
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
            int version = resourceVersion;

            var locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
            try
            {
                var locations = await locationsHandle.Task;
                if (version != resourceVersion) throw new OperationCanceledException("Se liberaron los recursos.");

                if (locations == null || locations.Count == 0)
                {
                    Debug.LogWarning($"[AddressablesService] No se encontraron assets para label: {label}");
                    return dict;
                }

                foreach (var loc in locations)
                {
                    try
                    {
                        dict[loc.PrimaryKey] = await CargarAssetAsync<T>(loc.PrimaryKey);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[AddressablesService] Error cargando {loc.PrimaryKey}: {ex.Message}");
                    }
                }

                return dict;
            }
            finally
            {
                if (locationsHandle.IsValid()) Addressables.Release(locationsHandle);
            }
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
            int version = resourceVersion;

            try
            {
                var instance = await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded || instance == null)
                {
                    throw new Exception($"Error instanciando UI prefab: {key}");
                }

                if (version != resourceVersion)
                    throw new OperationCanceledException("Los recursos se liberaron durante la carga.");
                instanceHandles[instance] = handle;

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
                LiberarHandleInstancia(handle);
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
            int version = resourceVersion;

            try
            {
                var instance = await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded || instance == null)
                {
                    throw new Exception($"Error instanciando prefab mundo: {key}");
                }

                if (version != resourceVersion)
                    throw new OperationCanceledException("Los recursos se liberaron durante la carga.");
                instanceHandles[instance] = handle;

                return instance;
            }
            catch (Exception ex)
            {
                LiberarHandleInstancia(handle);
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

            if (cachedHandles.TryGetValue(cacheKey, out var handle))
            {
                if (handle.IsValid()) Addressables.Release(handle);
                cachedHandles.Remove(cacheKey);
                cache.Remove(cacheKey);
            }
        }

        // =========================================
        // LIBERAR INSTANCIA
        // =========================================

        public static void LiberarInstancia(GameObject instance)
        {
            // Unity puede haber destruido el objeto antes de liberar su handle.
            if (ReferenceEquals(instance, null))
            {
                Debug.LogWarning("[AddressablesService] Intento de liberar instancia null.");
                return;
            }

            if (instanceHandles.TryGetValue(instance, out var handle))
            {
                instanceHandles.Remove(instance);
                LiberarHandleInstancia(handle);
            }
        }

        // =========================================
        // LIMPIAR CACHE
        // =========================================

        public static void LimpiarCacheCompleto()
        {
            foreach (var handle in cachedHandles.Values)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }

            cachedHandles.Clear();
            cache.Clear();
        }

        // =========================================
        // LIBERAR TODO
        // =========================================

        public static void LiberarRecursos()
        {
            resourceVersion++;
            foreach (var handle in instanceHandles.Values) LiberarHandleInstancia(handle);
            instanceHandles.Clear();
            LimpiarCacheCompleto();
            loadingTasks.Clear();
        }

        private static void LiberarHandleInstancia(AsyncOperationHandle<GameObject> handle)
        {
            if (!handle.IsValid()) return;
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                Addressables.ReleaseInstance(handle);
            else
                Addressables.Release(handle);
        }
    }

    public sealed class RecursoAddressable<T> : IDisposable where T : UnityEngine.Object
    {
        private AsyncOperationHandle<T> handle;
        private bool liberado;
        public T Asset { get; private set; }

        internal RecursoAddressable(AsyncOperationHandle<T> handle, T asset)
        {
            this.handle = handle;
            Asset = asset;
        }

        public void Dispose()
        {
            if (liberado) return;
            liberado = true;
            Asset = null;
            if (handle.IsValid()) Addressables.Release(handle);
            handle = default;
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
        public const string ArchivoLecciones = "lecciones_compiladas.json";
        public const string DatosAnalisisMorfologicos = "json_analyses";
        public const string DatosIndiceMorfologico = "json_lookup_index";
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
