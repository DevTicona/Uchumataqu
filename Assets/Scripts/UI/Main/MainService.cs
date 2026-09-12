using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Data.Persistencia;

namespace Assets.Scripts.UI.Main
{
    public static class MainService
    {
        public static InicioData CrearInicio(
            PerfilUsuario perfil,
            ProgresoUsuario progreso,
            IEnumerable<LeccionCompiladaData> lecciones,
            IEnumerable<EscenarioRAData> escenarios)
        {
            progreso ??= new ProgresoUsuario();
            List<LeccionCompiladaData> leccionesOrdenadas = (lecciones ?? Enumerable.Empty<LeccionCompiladaData>())
                .Where(leccion => leccion != null)
                .OrderBy(leccion => leccion.orden)
                .ToList();
            if (leccionesOrdenadas.Count == 0)
                throw new InvalidOperationException("No existen lecciones para construir la página de inicio.");

            ProgresoLeccionData enProgreso = BuscarLeccionEnProgreso(progreso, leccionesOrdenadas);
            LeccionCompiladaData siguiente = enProgreso != null
                ? leccionesOrdenadas.First(leccion => leccion.id == enProgreso.leccion_id)
                : leccionesOrdenadas.FirstOrDefault(leccion => !progreso.LeccionCompletada(leccion.id));

            bool curriculoCompletado = siguiente == null;
            siguiente ??= leccionesOrdenadas[leccionesOrdenadas.Count - 1];
            int porcentajeLeccionActual = CalcularPorcentajeLeccion(enProgreso, siguiente);

            EscenarioRAData proximoRa = (escenarios ?? Enumerable.Empty<EscenarioRAData>())
                .FirstOrDefault(escenario => escenario != null && !progreso.EscenarioVisitado(escenario.id));
            int totalRa = proximoRa?.lecciones_asociadas?.Count ?? 0;
            int completadasRa = proximoRa?.lecciones_asociadas?.Count(progreso.LeccionCompletada) ?? 0;

            return new InicioData
            {
                Nombre = perfil?.nombre,
                AvatarId = perfil?.avatar_id,
                LeccionId = siguiente.id,
                TituloLeccion = siguiente.titulo,
                PorcentajeLeccionActual = porcentajeLeccionActual,
                EsContinuacion = porcentajeLeccionActual > 0,
                CurriculoCompletado = curriculoCompletado,
                LeccionesCompletadas = progreso.TotalLeccionesCompletadas,
                EscenariosVisitados = progreso.TotalEscenariosVisitados,
                ProximoRaTitulo = proximoRa?.titulo,
                ProximoRaCompletadas = completadasRa,
                ProximoRaTotal = totalRa
            };
        }

        public static List<LogroData> CrearLogros(ProgresoUsuario progreso)
        {
            progreso ??= new ProgresoUsuario();
            int lecciones = progreso.TotalLeccionesCompletadas;
            int escenarios = progreso.TotalEscenariosVisitados;

            return new List<LogroData>
            {
                CrearLogro("lecciones_1", "Primer paso", "Completa tu primera lección.", "icono_logro_primer_paso", lecciones, 1),
                CrearLogro("lecciones_5", "Caminante del lago", "Completa 5 lecciones.", "icono_logro_caminante_lago", lecciones, 5),
                CrearLogro("lecciones_14", "Guardián de la palabra", "Completa 14 lecciones.", "icono_logro_guardian_palabra", lecciones, 14),
                CrearLogro("lecciones_27", "Heredero de la lengua", "Completa las 27 lecciones.", "icono_logro_heredero_lengua", lecciones, 27),
                CrearLogro("ra_1", "Primera exploración", "Visita tu primer escenario RA.", "icono_logro_primera_exploracion", escenarios, 1),
                CrearLogro("ra_3", "Explorador Uru", "Visita 3 escenarios RA.", "icono_logro_explorador_uru", escenarios, 3),
                CrearLogro("ra_6", "Guía de los seis territorios", "Visita los 6 escenarios RA.", "icono_logro_guia_territorios", escenarios, 6)
            };
        }

        private static ProgresoLeccionData BuscarLeccionEnProgreso(
            ProgresoUsuario progreso,
            IReadOnlyCollection<LeccionCompiladaData> lecciones)
        {
            if (progreso.lecciones == null) return null;

            if (!string.IsNullOrWhiteSpace(progreso.ultima_leccion_id))
            {
                ProgresoLeccionData ultima = progreso.lecciones.FirstOrDefault(item =>
                    item.leccion_id == progreso.ultima_leccion_id && item.estado == "en_progreso");
                if (ultima != null && lecciones.Any(leccion => leccion.id == ultima.leccion_id)) return ultima;
            }

            return progreso.lecciones
                .Where(item => item.estado == "en_progreso" &&
                    lecciones.Any(leccion => leccion.id == item.leccion_id))
                .OrderBy(item => lecciones.First(leccion => leccion.id == item.leccion_id).orden)
                .FirstOrDefault();
        }

        private static int CalcularPorcentajeLeccion(
            ProgresoLeccionData progresoLeccion,
            LeccionCompiladaData leccion)
        {
            if (progresoLeccion == null || leccion?.pasos == null || leccion.pasos.Count == 0)
                return 0;

            int totalObligatorios = leccion.pasos.Count(paso => !paso.opcional);
            if (totalObligatorios == 0) return 0;

            int pasoActual = Math.Max(0, Math.Min(progresoLeccion.paso_actual, leccion.pasos.Count));
            int completados = leccion.pasos.Take(pasoActual).Count(paso => !paso.opcional);
            return (int)Math.Round((double)completados / totalObligatorios * 100d);
        }

        private static LogroData CrearLogro(
            string id,
            string titulo,
            string descripcion,
            string iconoId,
            int progreso,
            int objetivo)
        {
            int progresoAcotado = Math.Max(0, Math.Min(progreso, objetivo));
            return new LogroData
            {
                Id = id,
                Titulo = titulo,
                Descripcion = descripcion,
                IconoId = iconoId,
                Desbloqueado = progreso >= objetivo,
                ProgresoActual = progresoAcotado,
                Objetivo = objetivo
            };
        }
    }

    [Obsolete("Usar MainService en su lugar.")]
    public static class MainPresentacionService
    {
        public static InicioData CrearInicio(
            PerfilUsuario perfil,
            ProgresoUsuario progreso,
            IEnumerable<LeccionCompiladaData> lecciones,
            IEnumerable<EscenarioRAData> escenarios) =>
            MainService.CrearInicio(perfil, progreso, lecciones, escenarios);

        public static List<LogroData> CrearLogros(ProgresoUsuario progreso) =>
            MainService.CrearLogros(progreso);
    }
}
