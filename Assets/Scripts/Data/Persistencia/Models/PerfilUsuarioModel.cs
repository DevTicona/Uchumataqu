using System;

namespace Assets.Scripts.Data.Persistencia
{
    [Serializable]
    public class PerfilUsuarioModel
    {
        public int version = 2;
        public string nombre = "";
        public string avatar_id = "";
        public bool onboarding_completado = false;
    }
}
