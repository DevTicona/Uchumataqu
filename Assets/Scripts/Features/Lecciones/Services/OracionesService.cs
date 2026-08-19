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
    /// Servicio encargado de cargar y cachear las oraciones desde oraciones.json.
    /// Permite consultar oraciones por ID o por lista de IDs de una lección.
    /// </summary>
    public static class OracionesService
    {
        private const string KEY_ORACIONES = "json_oraciones";

        private static List<OracionItemData> cache;

        // =========================================
        // CARGA GENERAL
        // =========================================

        public static async Task<List<OracionItemData>> ObtenerTodasLasOracionesAsync()
        {
            if (cache != null)
                return cache;

            try
            {
                var asset = await AddressablesService.CargarAssetAsync<TextAsset>(KEY_ORACIONES);
                var wrapper = JsonUtility.FromJson<OracionesWrapperData>(asset.text);
                cache = wrapper?.items ?? new List<OracionItemData>();
                return cache;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OracionesService] Error cargando oraciones: {ex.Message}");
                return null;
            }
        }

        // =========================================
        // CONSULTAS
        // =========================================

        public static async Task<OracionItemData> ObtenerOracionPorIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var todas = await ObtenerTodasLasOracionesAsync();
            return todas?.FirstOrDefault(o => o.id == id);
        }

        /// <summary>
        /// Devuelve las oraciones que corresponden a la lista de IDs dada (oraciones_ids de una lección).
        /// Mantiene el orden de la lista de IDs.
        /// </summary>
        public static async Task<List<OracionItemData>> ObtenerOracionesPorIdsAsync(List<string> ids)
        {
            if (ids == null || ids.Count == 0) return new List<OracionItemData>();

            var todas = await ObtenerTodasLasOracionesAsync();
            if (todas == null) return null;

            var mapa = todas.ToDictionary(o => o.id, o => o);
            var resultado = new List<OracionItemData>();

            foreach (var id in ids)
            {
                if (mapa.TryGetValue(id, out var oracion))
                    resultado.Add(oracion);
                else
                    Debug.LogWarning($"[OracionesService] Oración no encontrada: {id}");
            }

            return resultado;
        }

        /// <summary>
        /// Devuelve las oraciones de una lección según su LeccionItemData.
        /// </summary>
        public static async Task<List<OracionItemData>> ObtenerOracionesDeLeccionAsync(LeccionItemData leccion)
        {
            if (leccion == null) return null;
            return await ObtenerOracionesPorIdsAsync(leccion.oraciones_ids);
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
