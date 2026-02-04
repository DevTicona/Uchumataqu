using System.Collections.Generic;

namespace Assets.Scripts.Models.Activities
{
    [System.Serializable]
    public class Distractores
    {
        public List<string> espanol;
        public List<string> uchumataqu;
    }

    [System.Serializable]
    public class ActividadesPorLeccion
    {
        public string leccionId;
        public List<TipoActividad> actividades;
        public Distractores distractores;
    }
}
