using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Persistencia
{
    public static class GestorProgreso
    {
        public static bool EstaLeccionDesbloqueada(string leccionId)
        {
            var lecciones = CorpusRepository.ObtenerTodasLasLecciones().OrderBy(l => l.orden).ToList();
            int indice = lecciones.FindIndex(l => l.id == leccionId);
            if (indice < 0) return false;
            var progreso = ProgresoRepository.ObtenerProgreso();
            return progreso.LeccionCompletada(leccionId) || indice == 0 ||
                progreso.LeccionCompletada(lecciones[indice - 1].id);
        }

        public static bool EstaEscenarioDesbloqueado(string escenarioId)
        {
            var escenario = CorpusRepository.ObtenerEscenarioRA(escenarioId);
            if (escenario == null || escenario.lecciones_asociadas == null || escenario.lecciones_asociadas.Count == 0)
                return false;

            var progreso = ProgresoRepository.ObtenerProgreso();
            foreach (var leccId in escenario.lecciones_asociadas)
            {
                if (!progreso.LeccionCompletada(leccId))
                    return false;
            }
            return true;
        }

        public static bool CompletarLeccion(string leccionId, int aciertos, int errores, int totalActividades)
        {
            var progreso = ProgresoRepository.ObtenerProgreso();
            var lecc = progreso.ObtenerOCrearLeccion(leccionId);

            lecc.estado = "completada";
            lecc.aciertos = aciertos;
            lecc.errores = errores;
            lecc.total_actividades = totalActividades;
            float precision = totalActividades > 0 ? ((float)aciertos / totalActividades) * 100f : 100f;
            if (precision > lecc.mejor_precision) lecc.mejor_precision = precision;
            lecc.veces_completada++;
            lecc.fecha_ultima_completada = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            lecc.paso_actual = 0;
            lecc.sesion_en_progreso = false;

            progreso.ultima_leccion_id = leccionId;
            return ProgresoRepository.Guardar();
        }

        public static bool GuardarLeccionCompletada(string leccionId, int aciertos, int errores, int totalActividades)
        {
            return CompletarLeccion(leccionId, aciertos, errores, totalActividades);
        }

        public static bool GuardarLeccionEnProgreso(string leccionId, int pasoActual, int aciertos, int errores,
            string revisionContenido = null)
        {
            if (string.IsNullOrWhiteSpace(leccionId)) return false;
            var progreso = ProgresoRepository.ObtenerProgreso();
            var leccion = progreso.ObtenerOCrearLeccion(leccionId);
            // Una repetición también se reanuda sin perder los desbloqueos obtenidos.
            if (leccion.estado != "completada") leccion.estado = "en_progreso";
            leccion.sesion_en_progreso = true;
            leccion.paso_actual = Mathf.Max(0, pasoActual);
            leccion.aciertos = Mathf.Max(0, aciertos);
            leccion.errores = Mathf.Max(0, errores);
            if (revisionContenido != null) leccion.revision_contenido = revisionContenido;
            progreso.ultima_leccion_id = leccionId;
            return ProgresoRepository.Guardar();
        }

    }
}
