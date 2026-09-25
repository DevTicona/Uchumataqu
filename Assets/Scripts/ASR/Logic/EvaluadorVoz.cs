using System;
using UnityEngine;
using Assets.Scripts.PLN;

namespace Assets.Scripts.ASR
{
    /// <summary>
    /// Evaluador de pronunciación que consume IReconocedorVoz y aplica asistencia léxica Viterbi.
    /// Especificado en Docs/Specifications/02-modulo-asr-voz.md (Sección 7 y 8).
    /// </summary>
    public static class EvaluadorVoz
    {
        private const float UmbralAprobacionDirecta = 85f;
        private const float UmbralMinimoAsistencia = 70f;
        private static readonly DecodificadorLexicoViterbi DecodificadorLexico = CrearDecodificadorLexico();

        public static ResultadoEvaluacion EvaluarPronunciacion(string esperado, AudioClip audioUsuario, IReconocedorVoz asr)
        {
            if (asr == null || audioUsuario == null)
                return CrearResultadoNoEvaluado(esperado);

            try
            {
                if (!asr.EstaDisponible()) return CrearResultadoNoEvaluado(esperado);
                string prediccionFonetica = NormalizadorFoneticoAsr.NormalizarPrediccionSherpa(
                    asr.TranscribirAudio(audioUsuario));
                var fonemasEsperados = NormalizadorFoneticoAsr.TextoAFonemas(
                    NormalizadorFoneticoAsr.ConvertirEsperado(esperado));
                var fonemasPredichos = NormalizadorFoneticoAsr.TextoAFonemas(prediccionFonetica);
                float precisionCruda = CalcularSimilitudFonetica(fonemasEsperados, fonemasPredichos);

                if (precisionCruda >= UmbralAprobacionDirecta)
                    return CrearResultado(esperado, prediccionFonetica, precisionCruda, true, "GreedyDirecto");

                if (precisionCruda < UmbralMinimoAsistencia)
                    return CrearResultado(esperado, prediccionFonetica, precisionCruda, false, "RechazoAcustico");

                DecodificadorLexicoViterbi.ResultadoDecodificacion reconstruccion =
                    DecodificadorLexico.DecodificarConRestricciones(fonemasPredichos);
                if (reconstruccion.EsExitoso)
                {
                    float precisionAsistida = CalcularSimilitudFonetica(
                        fonemasEsperados, reconstruccion.FonemasReconstruidos);
                    if (precisionAsistida >= UmbralAprobacionDirecta)
                    {
                        ResultadoEvaluacion resultado = CrearResultado(
                            esperado, prediccionFonetica, precisionAsistida, true, "AsistidoPorLexico");
                        resultado.textoReconocido = string.Join(" ", reconstruccion.PalabrasReconocidas);
                        Debug.Log($"[EvaluadorVoz] Correccion lexica: {resultado.textoReconocido}");
                        return resultado;
                    }
                }

                return CrearResultado(esperado, prediccionFonetica, precisionCruda, false, "Rechazado");
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
                aprobado = false,
                porcentajeSimilitud = 0f,
                colorSemaforo = "neutral",
                textoEsperado = esperado,
                textoIngresado = "(No se pudo evaluar la pronunciación)",
                origenEntrada = "voz"
            };
        }

        private static DecodificadorLexicoViterbi CrearDecodificadorLexico()
        {
            var decodificador = new DecodificadorLexicoViterbi();
            TextAsset lexico = Resources.Load<TextAsset>("Sherpa/lexicon");
            if (lexico == null)
            {
                Debug.LogWarning("[EvaluadorVoz] No se encontro el lexico ASR; la asistencia lexica se omitira.");
                return decodificador;
            }

            decodificador.CargarLexiconDesdeTexto(lexico.text);
            return decodificador;
        }

        private static ResultadoEvaluacion CrearResultado(
            string esperado, string prediccionFonetica, float precision, bool aprobado, string modo)
        {
            return new ResultadoEvaluacion
            {
                evaluado = true,
                aprobado = aprobado,
                porcentajeSimilitud = precision,
                colorSemaforo = precision >= UmbralAprobacionDirecta ? "verde" :
                    (precision >= MotorEvaluacion.UMBRAL_AMARILLO ? "amarillo" : "rojo"),
                textoEsperado = esperado,
                textoIngresado = prediccionFonetica,
                origenEntrada = "voz",
                modoEvaluacion = modo
            };
        }

        private static float CalcularSimilitudFonetica(
            System.Collections.Generic.IList<string> esperado,
            System.Collections.Generic.IList<string> predicho)
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
