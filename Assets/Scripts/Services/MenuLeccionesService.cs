using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Services
{
    /// <summary>
    /// Servicio centralizado para cargar recursos Addressables del menú de lecciones.
    /// Maneja: prefabs de islas, sprites de islas e iconos.
    /// </summary>
    public static class MenuLeccionesService
    {
        // -------------------------------------------------
        // CONSTANTES - CLAVES ADDRESSABLES
        // -------------------------------------------------
        private const string CLAVE_PREFAB_ISLA = "prefab_isla_leccion";
        private const string CLAVE_ICONO_BLOQUEADA = "icono_bloqueado";
        private const string CLAVE_ICONO_EN_PROGRESO = "icono_enprogreso";
        private const string CLAVE_ICONO_COMPLETADA = "icono_completado";

        // Handles activos (para liberar)
        private static AsyncOperationHandle<IList<Sprite>> handleIslas;
        private static AsyncOperationHandle<GameObject> handlePrefabIsla;
        private static readonly List<AsyncOperationHandle<Sprite>> handlesIconos = new();
        private static readonly List<AsyncOperationHandle<Sprite>> handlesSpriteEstado = new();

        // Cache de sprites
        private static readonly Dictionary<string, Sprite> cacheIconos = new();
        private static readonly Dictionary<string, Sprite> cacheSpriteEstado = new();

        // -------------------------------------------------
        // PREFAB ISLA
        // -------------------------------------------------

        /// <summary>
        /// Carga el prefab de isla de forma asincrónica.
        /// </summary>
        public static void CargarPrefabIsla(
            System.Action<GameObject> onSuccess,
            System.Action onError = null
        )
        {
            handlePrefabIsla = Addressables.LoadAssetAsync<GameObject>(CLAVE_PREFAB_ISLA);

            handlePrefabIsla.Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError("[MenuLeccionesService] No se pudo cargar el prefab IslaLeccion.");
                    onError?.Invoke();
                    return;
                }

                onSuccess?.Invoke(handle.Result);
            };
        }

        // -------------------------------------------------
        // SPRITES ISLAS
        // -------------------------------------------------

        /// <summary>
        /// Carga todos los sprites de islas (label: img_islas) ordenados por nombre.
        /// </summary>
        public static void CargarSpritesIslas(
            System.Action<List<Sprite>> onSuccess,
            System.Action onError = null
        )
        {
            handleIslas = Addressables.LoadAssetsAsync<Sprite>("img_islas", null);

            handleIslas.Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError("[MenuLeccionesService] Error al cargar sprites de islas.");
                    onError?.Invoke();
                    return;
                }

                var spritesOrdenados = handle.Result
                    .OrderBy(s => s.name)
                    .ToList();

                if (spritesOrdenados.Count == 0)
                {
                    Debug.LogError("[MenuLeccionesService] No se encontraron sprites con label img_islas.");
                    onError?.Invoke();
                    return;
                }

                onSuccess?.Invoke(spritesOrdenados);
            };
        }

        // -------------------------------------------------
        // ICONOS
        // -------------------------------------------------

        /// <summary>
        /// Carga un ícono individual (formato: icono_{key}).
        /// Utiliza caché para evitar cargas duplicadas.
        /// </summary>
        public static void CargarIcono(
            string key,
            System.Action<Sprite> onSuccess,
            System.Action onError = null
        )
        {
            if (cacheIconos.TryGetValue(key, out var sprite))
            {
                onSuccess?.Invoke(sprite);
                return;
            }

            var handle = Addressables.LoadAssetAsync<Sprite>($"icono_{key}");
            handlesIconos.Add(handle);

            handle.Completed += h =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    cacheIconos[key] = h.Result;
                    onSuccess?.Invoke(h.Result);
                }
                else
                {
                    Debug.LogWarning($"[MenuLeccionesService] No se pudo cargar ícono: icono_{key}");
                    onError?.Invoke();
                }
            };
        }

        // -------------------------------------------------
        // SPRITES DE ESTADO (Bloqueada, En Progreso, Completada)
        // -------------------------------------------------

        /// <summary>
        /// Carga los sprites de estado (bloqueada, en progreso, completada).
        /// Devuelve los tres sprites en una acción callback.
        /// </summary>
        public static void CargarSpritesEstado(
            System.Action<Sprite, Sprite, Sprite> onSuccess,
            System.Action onError = null
        )
        {
            // Verificar si ya están en caché
            if (cacheSpriteEstado.TryGetValue(CLAVE_ICONO_BLOQUEADA, out var bloqueada) &&
                cacheSpriteEstado.TryGetValue(CLAVE_ICONO_EN_PROGRESO, out var enProgreso) &&
                cacheSpriteEstado.TryGetValue(CLAVE_ICONO_COMPLETADA, out var completada))
            {
                onSuccess?.Invoke(bloqueada, enProgreso, completada);
                return;
            }

            // Cargar en paralelo
            int spritesRequeridos = 3;
            int spritesObtenidos = 0;
            Sprite spriteBloqueada = null;
            Sprite spriteEnProgreso = null;
            Sprite spriteCompletada = null;

            void VerificarCompleción()
            {
                if (spritesObtenidos == spritesRequeridos)
                {
                    onSuccess?.Invoke(spriteBloqueada, spriteEnProgreso, spriteCompletada);
                }
            }

            // Cargar icono bloqueada
            CargarSpriteEstadoInterno(CLAVE_ICONO_BLOQUEADA, sprite =>
            {
                spriteBloqueada = sprite;
                spritesObtenidos++;
                VerificarCompleción();
            }, onError);

            // Cargar icono en progreso
            CargarSpriteEstadoInterno(CLAVE_ICONO_EN_PROGRESO, sprite =>
            {
                spriteEnProgreso = sprite;
                spritesObtenidos++;
                VerificarCompleción();
            }, onError);

            // Cargar icono completada
            CargarSpriteEstadoInterno(CLAVE_ICONO_COMPLETADA, sprite =>
            {
                spriteCompletada = sprite;
                spritesObtenidos++;
                VerificarCompleción();
            }, onError);
        }

        /// <summary>
        /// Carga un sprite de estado individual.
        /// </summary>
        private static void CargarSpriteEstadoInterno(
            string clave,
            System.Action<Sprite> onSuccess,
            System.Action onError = null
        )
        {
            if (cacheSpriteEstado.TryGetValue(clave, out var sprite))
            {
                onSuccess?.Invoke(sprite);
                return;
            }

            var handle = Addressables.LoadAssetAsync<Sprite>(clave);
            handlesSpriteEstado.Add(handle);

            handle.Completed += h =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    cacheSpriteEstado[clave] = h.Result;
                    onSuccess?.Invoke(h.Result);
                }
                else
                {
                    Debug.LogError($"[MenuLeccionesService] No se pudo cargar sprite de estado: {clave}");
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
            if (handleIslas.IsValid())
                Addressables.Release(handleIslas);

            if (handlePrefabIsla.IsValid())
                Addressables.Release(handlePrefabIsla);

            foreach (var h in handlesIconos)
                if (h.IsValid())
                    Addressables.Release(h);

            foreach (var h in handlesSpriteEstado)
                if (h.IsValid())
                    Addressables.Release(h);

            handlesIconos.Clear();
            handlesSpriteEstado.Clear();
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
