using System;
using UnityEngine;
using Assets.Scripts.PLN;

namespace Assets.Scripts.ASR
{
    /// <summary>
    /// Evaluador de práctica oral que consume IReconocedorVoz y delega al MotorEvaluacion.
    /// Modalidad complementaria y no bloqueante.
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
                return MotorEvaluacion.EvaluarTexto(esperadoFonetico, prediccionFonetica, origenEntrada: "voz",
                    textoEsperadoOriginal: esperado);
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
    }
}
