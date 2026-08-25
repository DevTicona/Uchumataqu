using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.UI.Main.Paginas;

namespace Assets.Scripts.UI.Main
{
    public static class MapaLeccionesPresentacionService
    {
        public static List<IslaMapaPresentacionData> Crear(
            IReadOnlyList<LeccionCompiladaData> lecciones,
            IReadOnlyList<EscenarioRAData> escenarios,
            ProgresoUsuario progreso)
        {
            var hitos = escenarios
                .Where(e => e.lecciones_asociadas != null && e.lecciones_asociadas.Count > 0)
                .ToDictionary(e => e.lecciones_asociadas[e.lecciones_asociadas.Count - 1], e => e);
            var regiones = escenarios
                .Where(e => e.lecciones_asociadas != null)
                .SelectMany(e => e.lecciones_asociadas.Select(id => new { id, e.titulo }))
                .ToDictionary(x => x.id, x => x.titulo);

            var resultado = new List<IslaMapaPresentacionData>();
            bool actualAsignada = false;
            for (int i = 0; i < lecciones.Count; i++)
            {
                LeccionCompiladaData leccion = lecciones[i];
                bool completada = progreso != null && progreso.LeccionCompletada(leccion.id);
                bool disponible = i == 0 || progreso != null && progreso.LeccionCompletada(lecciones[i - 1].id);
                bool esActual = !actualAsignada && disponible && !completada;
                if (esActual) actualAsignada = true;

                resultado.Add(new IslaMapaPresentacionData
                {
                    LeccionId = leccion.id,
                    Numero = i + 1,
                    Titulo = leccion.titulo,
                    Region = regiones.TryGetValue(leccion.id, out string region) ? region : "Recorrido",
                    EsActual = esActual,
                    Estado = completada
                        ? EstadoIsla.Completada
                        : disponible ? EstadoIsla.Disponible : EstadoIsla.Bloqueada
                });

                if (!hitos.TryGetValue(leccion.id, out EscenarioRAData escenario)) continue;
                bool desbloqueado = escenario.lecciones_asociadas.All(id => progreso != null && progreso.LeccionCompletada(id));
                bool visitado = progreso != null && progreso.EscenarioVisitado(escenario.id);
                resultado.Add(new IslaMapaPresentacionData
                {
                    EscenarioRaId = escenario.id,
                    Titulo = escenario.titulo,
                    Region = escenario.titulo,
                    Estado = visitado
                        ? EstadoIsla.Completada
                        : desbloqueado ? EstadoIsla.Disponible : EstadoIsla.Bloqueada
                });
            }

            if (!actualAsignada && resultado.Count > 0)
                resultado[resultado.Count - 1].EsActual = true;
            return resultado;
        }
    }
}
