using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.Persistencia;

namespace Assets.Scripts.Persistencia
{
    /// <summary>Conserva las identidades del currículo anterior al unir L03 y L04.</summary>
    public static class MigracionCurriculo26
    {
        public static bool Aplicar(ProgresoUsuarioModel progreso)
        {
            if (progreso == null) throw new ArgumentNullException(nameof(progreso));
            bool reparado = false;
            if (progreso.lecciones == null)
            {
                progreso.lecciones = new List<ProgresoLeccionModel>();
                reparado = true;
            }
            if (progreso.escenarios_ra == null)
            {
                progreso.escenarios_ra = new List<ProgresoEscenarioRAModel>();
                reparado = true;
            }
            reparado |= progreso.lecciones.RemoveAll(leccion => leccion == null) > 0;
            reparado |= progreso.escenarios_ra.RemoveAll(escenario => escenario == null) > 0;
            if (progreso.version >= ProgresoUsuarioModel.VersionActual) return reparado;

            var anteriores = progreso.lecciones ?? new List<ProgresoLeccionModel>();
            var tercera = anteriores.FirstOrDefault(leccion => leccion.leccion_id == "L03");
            var cuarta = anteriores.FirstOrDefault(leccion => leccion.leccion_id == "L04");
            var actuales = anteriores.Where(leccion => leccion.leccion_id != "L03" && leccion.leccion_id != "L04").ToList();
            foreach (var leccion in actuales)
            {
                string nuevoId = ConvertirId(leccion.leccion_id);
                if (nuevoId == leccion.leccion_id) continue;
                leccion.leccion_id = nuevoId;
                // Los índices y revisiones anteriores no describen los pasos recompilados.
                leccion.paso_actual = 0;
                leccion.revision_contenido = null;
                if (leccion.estado == "en_progreso")
                {
                    leccion.aciertos = 0;
                    leccion.errores = 0;
                }
            }

            if (tercera != null || cuarta != null)
                actuales.Add(Unir(tercera, cuarta));
            progreso.lecciones = actuales.OrderBy(leccion => leccion.leccion_id, StringComparer.Ordinal).ToList();
            progreso.ultima_leccion_id = ConvertirId(progreso.ultima_leccion_id);
            progreso.version = ProgresoUsuarioModel.VersionActual;
            return true;
        }

        private static string ConvertirId(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length != 3 || id[0] != 'L' ||
                !int.TryParse(id.Substring(1), out int numero) || numero < 4 || numero > 27)
                return id;
            return $"L{numero - 1:D2}";
        }

        private static ProgresoLeccionModel Unir(ProgresoLeccionModel tercera, ProgresoLeccionModel cuarta)
        {
            bool completada = tercera?.estado == "completada" && cuarta?.estado == "completada";
            int total = (tercera?.total_actividades ?? 0) + (cuarta?.total_actividades ?? 0);
            return new ProgresoLeccionModel
            {
                leccion_id = "L03",
                estado = completada ? "completada" : "en_progreso",
                aciertos = completada ? tercera.aciertos + cuarta.aciertos : 0,
                errores = completada ? tercera.errores + cuarta.errores : 0,
                total_actividades = completada ? total : 0,
                mejor_precision = completada && total > 0
                    ? (tercera.mejor_precision * tercera.total_actividades + cuarta.mejor_precision * cuarta.total_actividades) / total
                    : 0f,
                veces_completada = completada ? Math.Min(tercera.veces_completada, cuarta.veces_completada) : 0,
                fecha_ultima_completada = completada
                    ? (string.CompareOrdinal(tercera.fecha_ultima_completada, cuarta.fecha_ultima_completada) >= 0
                        ? tercera.fecha_ultima_completada : cuarta.fecha_ultima_completada)
                    : null
            };
        }
    }
}
