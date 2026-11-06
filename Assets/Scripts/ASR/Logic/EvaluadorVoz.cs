using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public static async Task<ResultadoEvaluacion> EvaluarPronunciacionAsync(
            string esperado, AudioClip audioUsuario, SherpaOnnxReconocedorVoz asr)
        {
            if (asr == null || audioUsuario == null) return CrearResultadoNoEvaluado(esperado);
            try
            {
                // El servicio copia las muestras antes de ceder el control; inicializa y decodifica en su trabajador.
                string transcripcion = await asr.TranscribirAudioAsync(audioUsuario);
                var resultado = EvaluarTranscripcion(esperado, transcripcion);
#if UNITY_ANDROID && (DEVELOPMENT_BUILD || UCHUMATAQU_VALIDACION) && !UNITY_EDITOR
                DiagnosticoAsrAndroid.RegistrarPronunciacion(audioUsuario, resultado);
#endif
                return resultado;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EvaluadorVoz] La práctica no pudo evaluarse: {ex.Message}");
                return CrearResultadoNoEvaluado(esperado);
            }
        }

        public static ResultadoEvaluacion EvaluarPronunciacion(string esperado, AudioClip audioUsuario, IReconocedorVoz asr)
        {
            if (asr == null || audioUsuario == null)
                return CrearResultadoNoEvaluado(esperado);

            try
            {
                if (!asr.EstaDisponible()) return CrearResultadoNoEvaluado(esperado);

                return EvaluarTranscripcion(esperado, asr.TranscribirAudio(audioUsuario));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EvaluadorVoz] La práctica no pudo evaluarse: {ex.Message}");
                return CrearResultadoNoEvaluado(esperado);
            }
        }

        public static ResultadoEvaluacion EvaluarTranscripcion(string esperado, string transcripcion,
            bool compararPrefijo = false, string textoVisible = null, string prefijoValido = null)
        {
            string prediccionFonetica = NormalizadorFoneticoAsr.NormalizarPrediccionSherpa(transcripcion);
            string esperadoFonetico = NormalizadorFoneticoAsr.ConvertirEsperado(esperado);

            // Lleva referencia y predicción al mismo inventario: CH_EJ cuenta como un fonema, no cinco letras.
            var fonemasEsperados = NormalizadorFoneticoAsr.TextoAFonemas(esperadoFonetico);
            var fonemasPredichos = NormalizadorFoneticoAsr.TextoAFonemas(prediccionFonetica);
            var prefijo = NormalizadorFoneticoAsr.TextoAFonemas(
                NormalizadorFoneticoAsr.ConvertirEsperado(prefijoValido));
            bool coincidePrefijo = compararPrefijo && prefijo.Count > 0 &&
                ComienzaCon(fonemasEsperados, prefijo) && ComienzaCon(fonemasPredichos, prefijo);
            // Las presentaciones personales evalúan la frase fija, sin el nombre ajeno al léxico.
            if (compararPrefijo && fonemasPredichos.Count > fonemasEsperados.Count)
                fonemasPredichos = fonemasPredichos.GetRange(0, fonemasEsperados.Count);
            // Un prefijo explícito evalúa únicamente los fonemas exigidos en ese turno.
            float precision = coincidePrefijo ? 100f : CalcularSimilitudFonetica(fonemasEsperados, fonemasPredichos);

            bool aprobado = precision >= MotorEvaluacion.UMBRAL_VERDE;
            string semaforo = precision >= MotorEvaluacion.UMBRAL_VERDE ? "verde" :
                (precision >= MotorEvaluacion.UMBRAL_AMARILLO ? "amarillo" : "rojo");

            // La reconstrucción ortográfica es para mostrar el resultado; la puntuación ya se calculó con fonemas.
            string textoReconocido = DecodificadorFoneticoAsr.Decodificar(prediccionFonetica, textoVisible ?? esperado);

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

        private static bool ComienzaCon(IList<string> fonemas, IList<string> prefijo)
        {
            if (fonemas.Count < prefijo.Count) return false;
            for (int i = 0; i < prefijo.Count; i++)
                if (fonemas[i] != prefijo[i]) return false;
            return true;
        }

        private static ResultadoEvaluacion CrearResultadoNoEvaluado(string esperado)
        {
            return new ResultadoEvaluacion
            {
                evaluado = false,
                aprobado = false,
                porcentajeSimilitud = 0f,
                colorSemaforo = "neutral",
                textoEsperado = esperado,
                textoIngresado = "(ASR no evaluado por fallo técnico)",
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

            // Similitud fonética = (1 - distancia / longitud máxima) * 100; no es una probabilidad del modelo.
            int mayorLongitud = Math.Max(longitudEsperada, longitudPredicha);
            return mayorLongitud == 0 ? 100f : Math.Max(0f,
                (1f - (float)matriz[longitudEsperada, longitudPredicha] / mayorLongitud) * 100f);
        }
    }
}
