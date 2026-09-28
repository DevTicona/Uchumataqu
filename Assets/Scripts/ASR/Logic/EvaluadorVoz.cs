using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.PLN;

namespace Assets.Scripts.ASR
{
    /// <summary>
    /// Evaluador de pronunciación que consume IReconocedorVoz y compara
    /// la transcripción fonética del modelo CTC contra la forma esperada.
    /// La comparación opera a nivel de fonemas (token a token) para mayor
    /// precisión que la comparación carácter a carácter sobre la cadena CTC.
    /// Especificado en Docs/Specifications/02-modulo-asr-voz.md (Sección 7 y 8).
    /// </summary>
    public static class EvaluadorVoz
    {
        public static ResultadoEvaluacion EvaluarPronunciacion(string esperado, AudioClip audioUsuario, IReconocedorVoz asr)
        {
            if (asr == null || audioUsuario == null)
                return CrearResultadoNoEvaluado(esperado);

            try
            {
                if (!asr.EstaDisponible()) return CrearResultadoNoEvaluado(esperado);

                string prediccionFonetica = NormalizadorFoneticoAsr.NormalizarPrediccionSherpa(
                    asr.TranscribirAudio(audioUsuario));
                string esperadoFonetico = NormalizadorFoneticoAsr.ConvertirEsperado(esperado);

                var fonemasEsperados = NormalizadorFoneticoAsr.TextoAFonemas(esperadoFonetico);
                var fonemasPredichos = NormalizadorFoneticoAsr.TextoAFonemas(prediccionFonetica);
                float precision = CalcularSimilitudFonetica(fonemasEsperados, fonemasPredichos);

                bool aprobado = precision >= MotorEvaluacion.UMBRAL_VERDE;
                string semaforo = precision >= MotorEvaluacion.UMBRAL_VERDE ? "verde" :
                    (precision >= MotorEvaluacion.UMBRAL_AMARILLO ? "amarillo" : "rojo");

                string textoReconocido = DecodificadorFoneticoAsr.Decodificar(prediccionFonetica, esperado);

                return new ResultadoEvaluacion
                {
                    evaluado = true,
                    aprobado = aprobado,
                    porcentajeSimilitud = precision,
                    colorSemaforo = semaforo,
                    textoEsperado = esperado,
                    textoIngresado = prediccionFonetica,
                    textoReconocido = textoReconocido,
                    origenEntrada = "voz"
                };
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EvaluadorVoz] La práctica no pudo evaluarse: {ex.Message}");
                return CrearResultadoNoEvaluado(esperado);
            }
        }

        private static ResultadoEvaluacion CrearResultadoNoEvaluado(string esperado)
        {
            return new ResultadoEvaluacion
            {
                evaluado = false,
                aprobado = true,
                porcentajeSimilitud = 0f,
                colorSemaforo = "neutral",
                textoEsperado = esperado,
                textoIngresado = "(Práctica no evaluada / ASR opcional)",
                origenEntrada = "voz"
            };
        }

        /// <summary>
        /// Distancia Levenshtein normalizada a porcentaje sobre listas de fonemas.
        /// Compara token a token (cada fonema es un token), lo que da mayor
        /// precisión que comparar carácter a carácter sobre la cadena fonética.
        /// </summary>
        private static float CalcularSimilitudFonetica(
            IList<string> esperado, IList<string> predicho)
        {
            int longitudEsperada = esperado == null ? 0 : esperado.Count;
            int longitudPredicha = predicho == null ? 0 : predicho.Count;
            int[,] matriz = new int[longitudEsperada + 1, longitudPredicha + 1];

            for (int fila = 0; fila <= longitudEsperada; fila++) matriz[fila, 0] = fila;
            for (int columna = 0; columna <= longitudPredicha; columna++) matriz[0, columna] = columna;
            for (int fila = 1; fila <= longitudEsperada; fila++)
            {
                for (int columna = 1; columna <= longitudPredicha; columna++)
                {
                    int sustitucion = esperado[fila - 1] == predicho[columna - 1] ? 0 : 1;
                    matriz[fila, columna] = Math.Min(
                        Math.Min(matriz[fila - 1, columna] + 1, matriz[fila, columna - 1] + 1),
                        matriz[fila - 1, columna - 1] + sustitucion);
                }
            }

            int mayorLongitud = Math.Max(longitudEsperada, longitudPredicha);
            return mayorLongitud == 0 ? 100f : Math.Max(0f,
                (1f - (float)matriz[longitudEsperada, longitudPredicha] / mayorLongitud) * 100f);
        }
    }
}
