using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Features.Lecciones.Models;

namespace Assets.Scripts.Features.Lecciones.Services
{
    /// <summary>
    /// Servicio que entrega palabras como List&lt;PalabraItemData&gt; directamente desde VocabularioService.
    /// Obtiene las palabras de una lección a partir de LeccionItemData.palabras_base usando LeccionesService.
    /// </summary>
    public static class PalabrasService
    {
        // =========================================
        // PALABRAS POR LECCIÓN
        // =========================================

        /// <summary>
        /// Devuelve las PalabraItemData de la lección indicada,
        /// en el orden definido por palabras_base en lecciones.json.
        /// </summary>
        public static async Task<List<PalabraItemData>> ObtenerPalabrasPorLeccionAsync(string idLeccion)
        {
            if (string.IsNullOrWhiteSpace(idLeccion))
            {
                Debug.LogError("[PalabrasService] idLeccion inválido.");
                return null;
            }

            try
            {
                var leccionItem = await LeccionesService.ObtenerLeccionPorIdAsync(idLeccion);

                if (leccionItem == null)
                {
                    Debug.LogWarning($"[PalabrasService] No se encontró la lección: {idLeccion}");
                    return new List<PalabraItemData>();
                }

                if (leccionItem.palabras_base == null || leccionItem.palabras_base.Count == 0)
                    return new List<PalabraItemData>();

                return await VocabularioService.ObtenerPalabrasPorIdsAsync(leccionItem.palabras_base);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PalabrasService] Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Devuelve TODAS las palabras del vocabulario como PalabraItemData.
        /// </summary>
        public static async Task<List<PalabraItemData>> ObtenerTodasLasPalabrasAsync()
        {
            return await VocabularioService.ObtenerTodasLasPalabrasAsync();
        }

        // =========================================
        // CACHÉ
        // =========================================

        public static void LimpiarCache()
        {
            VocabularioService.LimpiarCache();
        }
    }
}