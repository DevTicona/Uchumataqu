#if UNITY_ANDROID && UCHUMATAQU_DEMO_FULL_PROGRESS && !UNITY_EDITOR
using System;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.Core.Diagnostics
{
    /// <summary>Prepara un perfil y progreso aislados para la compilación de demostración.</summary>
    internal static class DemoProgresoCompletoAndroid
    {
        private static IDisposable _directorioPersistencia;

        public static void Preparar()
        {
            if (_directorioPersistencia != null) return;

            _directorioPersistencia = JsonLocalService.UsarDirectorioDemoCompleto();
            var lecciones = CorpusRepository.ObtenerTodasLasLecciones()
                .Where(leccion => leccion != null)
                .OrderBy(leccion => leccion.orden)
                .ToList();
            var escenarios = CorpusRepository.ObtenerTodosLosEscenariosRA();
            if (lecciones.Count == 0)
                throw new InvalidOperationException("No hay lecciones para preparar la demo completa.");

            var progreso = ProgresoRepository.ObtenerProgreso();
            bool yaPreparado = GestorPerfil.ExistePerfil() &&
                progreso.TotalLeccionesCompletadas >= lecciones.Count &&
                progreso.TotalEscenariosVisitados >= escenarios.Count;
            if (yaPreparado) return;

            if (!GestorPerfil.CrearOActualizar("Estudiante Demo", "avatar_01"))
                throw new InvalidOperationException("No se pudo preparar el perfil de demostración.");

            progreso.lecciones.Clear();
            progreso.escenarios_ra.Clear();
            string fecha = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            foreach (var leccion in lecciones)
            {
                int total = leccion.pasos?.Count ?? 0;
                progreso.lecciones.Add(new ProgresoLeccionModel
                {
                    leccion_id = leccion.id,
                    estado = "completada",
                    aciertos = total,
                    total_actividades = total,
                    mejor_precision = 100f,
                    veces_completada = 1,
                    fecha_ultima_completada = fecha,
                    paso_actual = 0,
                    revision_contenido = leccion.RevisionContenido,
                    sesion_en_progreso = false
                });
            }

            progreso.ultima_leccion_id = lecciones[lecciones.Count - 1].id;
            foreach (var escenario in escenarios)
            {
                if (escenario != null && !string.IsNullOrWhiteSpace(escenario.id))
                    progreso.MarcarEscenarioVisitado(escenario.id);
            }

            if (!ProgresoRepository.Guardar())
                throw new InvalidOperationException("No se pudo guardar el progreso de demostración.");
        }
    }
}
#endif
