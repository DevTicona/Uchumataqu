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
    /// Servicio que carga y cachea las lecciones desde lecciones.json.
    /// El JSON tiene estructura plana: { "items": [ { id, id_tema, nombre, orden, palavras_base, oraciones_ids } ] }
    /// </summary>
    public static class LeccionesService
    {
        private const string KEY_LECCIONES = "json_lecciones";

        private static List<LeccionItemData> cache;

        // =========================================
        // CARGA GENERAL
        // =========================================

        public static async Task<List<LeccionItemData>> ObtenerLeccionesAsync()
        {
            if (cache != null)
                return cache;

            try
            {
                var asset = await AddressablesService.CargarAssetAsync<TextAsset>(KEY_LECCIONES);
                var wrapper = JsonUtility.FromJson<LeccionesWrapperData>(asset.text);
                cache = (wrapper?.items ?? new List<LeccionItemData>())
                    .OrderBy(l => l.orden)
                    .ToList();
                return cache;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LeccionesService] Error cargando lecciones: {ex.Message}");
                return null;
            }
        }

        // =========================================
        // CONSULTAS
        // =========================================

        public static async Task<LeccionItemData> ObtenerLeccionPorIdAsync(string idLeccion)
        {
            if (string.IsNullOrWhiteSpace(idLeccion)) return null;

            var lista = await ObtenerLeccionesAsync();
            var leccion = lista?.FirstOrDefault(l => l.id == idLeccion);

            if (leccion == null)
                Debug.LogWarning($"[LeccionesService] No se encontró la lección: {idLeccion}");

            return leccion;
        }

        /// <summary>
        /// Devuelve todas las lecciones que pertenecen a un tema específico.
        /// </summary>
        public static async Task<List<LeccionItemData>> ObtenerLeccionesPorTemaAsync(string idTema)
        {
            if (string.IsNullOrWhiteSpace(idTema)) return new List<LeccionItemData>();

            var lista = await ObtenerLeccionesAsync();
            return lista?.Where(l => l.id_tema == idTema).ToList()
                   ?? new List<LeccionItemData>();
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