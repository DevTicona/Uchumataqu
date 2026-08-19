using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Features.Lecciones.Models;

namespace Assets.Scripts.Features.Lecciones.Services
{
    public static class LeccionBaseService
    {
        // =========================================
        // DIVISIÓN EN BLOQUES
        // =========================================

        /// <summary>
        /// Divide una lista de palabras en bloques de máximo 4.
        /// </summary>
        public static List<List<PalabraItemData>> DividirEnBloques(List<PalabraItemData> palabras, int tamañoBloque = 4)
        {
            var bloques = new List<List<PalabraItemData>>();

            for (int i = 0; i < palabras.Count; i += tamañoBloque)
            {
                int count = Mathf.Min(tamañoBloque, palabras.Count - i);
                bloques.Add(palabras.GetRange(i, count));
            }

            return bloques;
        }

        // =========================================
        // SELECCIÓN DE ACTIVIDADES DE ANCLAJE
        // =========================================

        /// <summary>
        /// Devuelve los IDs de las 3 actividades de anclaje para un bloque:
        /// seleccion_traduccion, seleccion_audio + 1 aleatoria.
        /// </summary>
        public static List<string> SeleccionarActividadesAnclaje()
        {
            var aleatorias = new List<string> { "emparejamiento_audio_palabra", "emparejamiento_traduccion" };
            string aleatoria = aleatorias[UnityEngine.Random.Range(0, aleatorias.Count)];

            return new List<string>
            {
                "seleccion_traduccion",
                "seleccion_audio",
                aleatoria
            };
        }

        // =========================================
        // SELECCIÓN DE ACTIVIDADES DE PRODUCCIÓN
        // =========================================

        /// <summary>
        /// Selecciona 3 actividades aleatorias distintas del pool de producción.
        /// </summary>
        public static List<string> SeleccionarActividadesProduccion()
        {
            var pool = new List<string>
            {
                "ordenar_oracion",
                "traduccion_constructiva",
                "pln_oracion",
                "completar_espacio",
                "dictado",
                "traduccion_inversa"
            };

            // Fisher-Yates shuffle y tomar 3
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            return pool.GetRange(0, 3);
        }
    }
}