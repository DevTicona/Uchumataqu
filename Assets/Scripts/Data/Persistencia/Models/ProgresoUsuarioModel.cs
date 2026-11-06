using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Data.Persistencia
{
    [Serializable]
    public class ProgresoUsuarioModel
    {
        public const int VersionActual = 2;
        public int version = VersionActual;
        public string ultima_leccion_id = "";
        public List<ProgresoLeccionModel> lecciones = new List<ProgresoLeccionModel>();
        public List<ProgresoEscenarioRAModel> escenarios_ra = new List<ProgresoEscenarioRAModel>();

        // Métodos de consulta rápidos
        public int TotalLeccionesCompletadas => lecciones?.Count(l => l.estado == "completada") ?? 0;
        public int TotalEscenariosVisitados => escenarios_ra?.Count(e => e.visitado) ?? 0;

        public List<string> ObtenerIdsLeccionesCompletadas() =>
            lecciones?.Where(l => l.estado == "completada").Select(l => l.leccion_id).ToList() ?? new List<string>();

        public bool LeccionCompletada(string leccionId)
        {
            if (string.IsNullOrEmpty(leccionId)) return false;
            var lecc = lecciones.FirstOrDefault(l => l.leccion_id == leccionId);
            return lecc != null && lecc.estado == "completada";
        }

        public ProgresoLeccionModel ObtenerOCrearLeccion(string leccionId)
        {
            var lecc = lecciones.FirstOrDefault(l => l.leccion_id == leccionId);
            if (lecc == null)
            {
                lecc = new ProgresoLeccionModel { leccion_id = leccionId };
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
                esc = new ProgresoEscenarioRAModel { escenario_id = escenarioId, visitado = true };
                escenarios_ra.Add(esc);
            }
            else
            {
                esc.visitado = true;
            }
        }
    }
}
