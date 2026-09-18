using System;

namespace Assets.Scripts.Data.Persistencia
{
    [Serializable]
    public class ConfiguracionUsuarioModel
    {
        public int version = 1;
        public float volumen_general = 1f;
        public bool audio_automatico = true;
        public bool vibracion = true;
    }
}
