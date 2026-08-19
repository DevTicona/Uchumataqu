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
    /// Servicio que carga y cachea los temas desde temas.json.
    /// El JSON tiene estructura plana: { "items": [ { id, nombre, orden, ra_id } ] }
    /// </summary>
    public static class TemasService
    {
        private const string KEY_TEMAS = "json_temas";

        private static List<TemaItemData> cache;

        // =========================================
        // CARGA GENERAL
        // =========================================

        public static async Task<List<TemaItemData>> ObtenerTemasAsync()
        {
            if (cache != null)
                return cache;

            try
            {
                var asset = await AddressablesService.CargarAssetAsync<TextAsset>(KEY_TEMAS);
                var wrapper = JsonUtility.FromJson<TemasWrapperData>(asset.text);
                cache = (wrapper?.items ?? new List<TemaItemData>())
                    .OrderBy(t => t.orden)
                    .ToList();
                return cache;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TemasService] Error cargando temas: {ex.Message}");
                return null;
            }
        }

        // =========================================
        // CONSULTAS
        // =========================================

        public static async Task<TemaItemData> ObtenerTemaPorIdAsync(string idTema)
        {
            if (string.IsNullOrWhiteSpace(idTema)) return null;

            var lista = await ObtenerTemasAsync();
            var tema = lista?.FirstOrDefault(t => t.id == idTema);

            if (tema == null)
                Debug.LogWarning($"[TemasService] No se encontró el tema: {idTema}");

            return tema;
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
