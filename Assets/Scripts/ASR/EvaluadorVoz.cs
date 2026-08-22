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
            if (asr == null || !asr.EstaDisponible() || audioUsuario == null)
            {
                // Si el ASR no está disponible, no bloquear la experiencia
                return new ResultadoEvaluacion
                {
                    aprobado = true,
                    porcentajeSimilitud = 100f,
                    colorSemaforo = "verde",
                    textoEsperado = esperado,
                    textoIngresado = "(Práctica no evaluada / ASR opcional)",
                    origenEntrada = "voz"
                };
            }

            string transcrito = asr.TranscribirAudio(audioUsuario);
            return MotorEvaluacion.EvaluarTexto(esperado, transcrito, origenEntrada: "voz");
        }
    }
}
