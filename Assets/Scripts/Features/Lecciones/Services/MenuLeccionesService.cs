using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Core.Services;

namespace Assets.Scripts.Features.Lecciones.Services
{
    public static class MenuLeccionesService
    {
        private const string CLAVE_PREFAB_ISLA       = "prefab_isla_leccion";
        private const string LABEL_SPRITES_ISLAS     = "img_islas";
        private const string CLAVE_ICONO_BLOQUEADO   = "icono_bloqueado";
        private const string CLAVE_ICONO_EN_PROGRESO = "icono_enprogreso";
        private const string CLAVE_ICONO_COMPLETADO  = "icono_completado";

        private static GameObject              cachePrefabIsla;
        private static Dictionary<string, Sprite> cacheIslas;
        private static Sprite cacheIconoBloqueado;
        private static Sprite cacheIconoEnProgreso;
        private static Sprite cacheIconoCompletado;

        // =========================================
        // PREFAB ISLA (compartido para lecciones y RA)
        // =========================================

        public static async Task<GameObject> CargarPrefabIslaAsync()
        {
            if (cachePrefabIsla != null) return cachePrefabIsla;

            try
            {
                cachePrefabIsla = await AddressablesService.CargarAssetAsync<GameObject>(CLAVE_PREFAB_ISLA);
                return cachePrefabIsla;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MenuLeccionesService] Error cargando prefab isla: {ex.Message}");
                return null;
            }
        }

        // =========================================
        // SPRITES ISLAS (key = ID de lección o ra_id)
        // =========================================
       public static async Task<Dictionary<string, Sprite>> CargarDictSpritesIslasAsync()
        {
            if (cacheIslas != null)
                return cacheIslas;

            try
            {
                // Carga directa usando PrimaryKey como clave
                cacheIslas = await AddressablesService.CargarAssetsConKeyAsync<Sprite>(LABEL_SPRITES_ISLAS);

                // Validación defensiva
                if (cacheIslas == null || cacheIslas.Count == 0)
                {
                    Debug.LogError("[MenuLeccionesService] No se encontraron sprites de islas.");
                    return null;
                }

                // Debug opcional (puedes quitarlo luego)
                foreach (var kv in cacheIslas)
                {
                    Debug.Log($"[MenuLeccionesService] KEY: {kv.Key} | SPRITE: {kv.Value.name}");
                }

                return cacheIslas;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MenuLeccionesService] Error cargando sprites de islas: {ex.Message}");
                return null;
            }
        }

        // =========================================
        // ICONOS DE ESTADO
        // =========================================

        public static async Task<(Sprite bloqueado, Sprite enProgreso, Sprite completado)> CargarSpritesEstadoAsync()
        {
            if (cacheIconoBloqueado != null && cacheIconoEnProgreso != null && cacheIconoCompletado != null)
                return (cacheIconoBloqueado, cacheIconoEnProgreso, cacheIconoCompletado);

            try
            {
                var t1 = AddressablesService.CargarAssetAsync<Sprite>(CLAVE_ICONO_BLOQUEADO);
                var t2 = AddressablesService.CargarAssetAsync<Sprite>(CLAVE_ICONO_EN_PROGRESO);
                var t3 = AddressablesService.CargarAssetAsync<Sprite>(CLAVE_ICONO_COMPLETADO);

                await Task.WhenAll(t1, t2, t3);

                cacheIconoBloqueado  = t1.Result;
                cacheIconoEnProgreso = t2.Result;
                cacheIconoCompletado = t3.Result;

                return (cacheIconoBloqueado, cacheIconoEnProgreso, cacheIconoCompletado);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MenuLeccionesService] Error cargando iconos de estado: {ex.Message}");
                return (null, null, null);
            }
        }

        // =========================================
        // CACHÉ
        // =========================================

        public static void LimpiarCache()
        {
            cachePrefabIsla      = null;
            cacheIslas           = null;
            cacheIconoBloqueado  = null;
            cacheIconoEnProgreso = null;
            cacheIconoCompletado = null;
        }
    }
}