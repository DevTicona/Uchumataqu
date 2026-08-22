using System;

namespace Assets.Scripts.Data.Persistencia
{
    [Serializable]
    public class PerfilUsuario
    {
        public int version = 1;
        public string nombre = "";
        public int edad = 0;
        public string rango_edad = "";
        public string avatar_id = "";
        public bool onboarding_completado = false;
    }
}
