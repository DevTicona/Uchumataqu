using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Data.Persistencia
{
    [Serializable]
    public class ProgresoLeccionModel
    {
        public string leccion_id;
        public string estado = "no_iniciada"; // "no_iniciada", "en_progreso", "completada"
        public int aciertos;
        public int errores;
        public int total_actividades;
        public float mejor_precision;
        public int veces_completada;
        public string fecha_ultima_completada;
        public int paso_actual;
        public string revision_contenido;
        public bool sesion_en_progreso;
    }
}
