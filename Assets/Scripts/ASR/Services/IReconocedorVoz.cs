using UnityEngine;

namespace Assets.Scripts.ASR
{
    /// <summary>
    /// Interfaz para el motor ASR offline (Sherpa-onnx).
    /// Especificado en Docs/Specifications/02-modulo-asr-voz.md (Sección 7).
    /// </summary>
    public interface IReconocedorVoz
    {
        bool EstaDisponible();
        string TranscribirAudio(AudioClip clip);
    }
}
