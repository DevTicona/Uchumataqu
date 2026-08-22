namespace Assets.Scripts.Core
{
    /// <summary>
    /// Convención oficial para resolver claves Addressables de audio pedagógico según el id del corpus.
    /// Especificado en 06-guia-arquitectura-proyecto.md (Sección 2).
    /// </summary>
    public static class AudioAddress
    {
        public static string Palabra(string id) => $"audio_palabra/{id}";
        public static string Oracion(string id) => $"audio_oracion/{id}";
    }
}
