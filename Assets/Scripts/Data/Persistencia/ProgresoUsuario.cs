using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Data.Persistencia
{
    [Serializable]
    public class ProgresoLeccionData
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
    }

    [Serializable]
    public class ProgresoEscenarioRAData
    {
        public string escenario_id;
        public bool visitado;
    }

    [Serializable]
    public class ProgresoUsuario
    {
        public int version = 1;
        public string ultima_leccion_id = "";
        public List<ProgresoLeccionData> lecciones = new List<ProgresoLeccionData>();
        public List<ProgresoEscenarioRAData> escenarios_ra = new List<ProgresoEscenarioRAData>();

        // Métodos de consulta rápidos
        public bool LeccionCompletada(string leccionId)
        {
            if (string.IsNullOrEmpty(leccionId)) return false;
            var lecc = lecciones.FirstOrDefault(l => l.leccion_id == leccionId);
            return lecc != null && lecc.estado == "completada";
        }

        public ProgresoLeccionData ObtenerOCrearLeccion(string leccionId)
        {
            var lecc = lecciones.FirstOrDefault(l => l.leccion_id == leccionId);
            if (lecc == null)
            {
                lecc = new ProgresoLeccionData { leccion_id = leccionId };
                lecciones.Add(lecc);
            }
            return lecc;
        }

        public bool EscenarioVisitado(string escenarioId)
        {
            if (string.IsNullOrEmpty(escenarioId)) return false;
            var esc = escenarios_ra.FirstOrDefault(e => e.escenario_id == escenarioId);
            return esc != null && esc.visitado;
        }

        public void MarcarEscenarioVisitado(string escenarioId)
        {
            var esc = escenarios_ra.FirstOrDefault(e => e.escenario_id == escenarioId);
            if (esc == null)
            {
                esc = new ProgresoEscenarioRAData { escenario_id = escenarioId, visitado = true };
                escenarios_ra.Add(esc);
            }
            else
            {
                esc.visitado = true;
            }
        }
    }
}
