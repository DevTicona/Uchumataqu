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
    /// Servicio encargado de cargar y cachear el vocabulario desde palabras.json.
    /// Permite consultar palabras por ID o por lista de IDs base de una lección.
    /// </summary>
    public static class VocabularioService
    {
        private const string KEY_PALABRAS = "json_palabras";

        private static List<PalabraItemData> cache;

        // =========================================
        // CARGA GENERAL
        // =========================================

        public static async Task<List<PalabraItemData>> ObtenerTodasLasPalabrasAsync()
        {
            if (cache != null)
                return cache;

            try
            {
                var asset = await AddressablesService.CargarAssetAsync<TextAsset>(KEY_PALABRAS);
                var wrapper = JsonUtility.FromJson<PalabrasWrapperData>(asset.text);
                cache = wrapper?.items ?? new List<PalabraItemData>();
                return cache;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VocabularioService] Error cargando palabras: {ex.Message}");
                return null;
            }
        }

        // =========================================
        // CONSULTAS
        // =========================================

        public static async Task<PalabraItemData> ObtenerPalabraPorIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var todas = await ObtenerTodasLasPalabrasAsync();
            return todas?.FirstOrDefault(p => p.id == id);
        }

        /// <summary>
        /// Devuelve las palabras que corresponden a la lista de IDs dada (palabras_base de una lección).
        /// Mantiene el orden de la lista de IDs.
        /// </summary>
        public static async Task<List<PalabraItemData>> ObtenerPalabrasPorIdsAsync(List<string> ids)
        {
            if (ids == null || ids.Count == 0) return new List<PalabraItemData>();

            var todas = await ObtenerTodasLasPalabrasAsync();
            if (todas == null) return null;

            var mapa = todas.ToDictionary(p => p.id, p => p);
            var resultado = new List<PalabraItemData>();

            foreach (var id in ids)
            {
                if (mapa.TryGetValue(id, out var palabra))
                    resultado.Add(palabra);
                else
                    Debug.LogWarning($"[VocabularioService] Palabra no encontrada: {id}");
            }

            return resultado;
        }

        /// <summary>
        /// Devuelve las palabras base de una lección según su LeccionItemData.
        /// </summary>
        public static async Task<List<PalabraItemData>> ObtenerPalabrasDeLeccionAsync(LeccionItemData leccion)
        {
            if (leccion == null) return null;
            return await ObtenerPalabrasPorIdsAsync(leccion.palabras_base);
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
