using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Features.Lecciones.Models;

namespace Assets.Scripts.Features.Lecciones.Services
{
    /// <summary>
    /// Servicio que carga y cachea el catálogo de actividades desde actividades.json.
    /// El JSON tiene estructura plana: { "items": [ { id, nombre, tipo, fase, categoria, ... } ] }
    /// Permite filtrar por fase (exposicion | bloque | produccion | tema) y categoria (palabras | oraciones | ra).
    /// </summary>
    public static class ActividadesCatalogoService
    {
        private const string KEY_ACTIVIDADES = "json_actividades";

        private static List<ActividadCatalogoItemData> cache;

        // =========================================
        // CARGA GENERAL
        // =========================================

        public static async Task<List<ActividadCatalogoItemData>> ObtenerCatalogoAsync()
        {
            if (cache != null)
                return cache;

            try
            {
                var asset = await AddressablesService.CargarAssetAsync<TextAsset>(KEY_ACTIVIDADES);
                var wrapper = JsonUtility.FromJson<ActividadesWrapperData>(asset.text);
                cache = wrapper?.items ?? new List<ActividadCatalogoItemData>();
                return cache;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ActividadesCatalogoService] Error cargando actividades: {ex.Message}");
                return null;
            }
        }

        // =========================================
        // FILTROS POR FASE
        // =========================================

        /// <summary>Actividad estructural de pronunciación (fase exposicion).</summary>
        public static async Task<ActividadCatalogoItemData> ObtenerActividadExposicionAsync()
        {
            var catalogo = await ObtenerCatalogoAsync();
            return catalogo?.FirstOrDefault(a => a.fase == "exposicion");
        }

        /// <summary>Actividades pedagógicas de bloque (anclaje, fase bloque).</summary>
        public static async Task<List<ActividadCatalogoItemData>> ObtenerActividadesBloqueAsync()
        {
            var catalogo = await ObtenerCatalogoAsync();
            return catalogo?.Where(a => a.fase == "bloque").ToList()
                   ?? new List<ActividadCatalogoItemData>();
        }

        /// <summary>Actividades de producción (oraciones, fase produccion).</summary>
        public static async Task<List<ActividadCatalogoItemData>> ObtenerActividadesProduccionAsync()
        {
            var catalogo = await ObtenerCatalogoAsync();
            return catalogo?.Where(a => a.fase == "produccion").ToList()
                   ?? new List<ActividadCatalogoItemData>();
        }

        /// <summary>Actividades de realidad aumentada (fase tema, categoria ra).</summary>
        public static async Task<List<ActividadCatalogoItemData>> ObtenerActividadesRAAsync()
        {
            var catalogo = await ObtenerCatalogoAsync();
            return catalogo?.Where(a => a.categoria == "ra").ToList()
                   ?? new List<ActividadCatalogoItemData>();
        }

        // =========================================
        // FILTROS POR CATEGORÍA
        // =========================================

        public static async Task<List<ActividadCatalogoItemData>> ObtenerPorCategoriaAsync(string categoria)
        {
            var catalogo = await ObtenerCatalogoAsync();
            return catalogo?.Where(a => a.categoria == categoria).ToList()
                   ?? new List<ActividadCatalogoItemData>();
        }

        public static async Task<ActividadCatalogoItemData> ObtenerPorIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var catalogo = await ObtenerCatalogoAsync();
            return catalogo?.FirstOrDefault(a => a.id == id);
        }

        // =========================================
        // CACHÉ
        // =========================================

        public static void LimpiarCache()
        {
            cache = null;
        }
    }
}
