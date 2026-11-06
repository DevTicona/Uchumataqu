using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.UI.Main.Paginas;

namespace Assets.Scripts.UI.Main
{
    public static class MapaLeccionesModelFactory
    {
        public static List<IslaMapaModel> Crear(
            IReadOnlyList<LeccionCompiladaModel> lecciones,
            IReadOnlyList<EscenarioRAModel> escenarios,
            ProgresoUsuarioModel progreso,
            bool accesoLibre = false)
        {
            var hitos = escenarios
                .Where(e => e.lecciones_asociadas != null && e.lecciones_asociadas.Count > 0)
                .ToDictionary(e => e.lecciones_asociadas[e.lecciones_asociadas.Count - 1], e => e);
            var regiones = escenarios
                .Where(e => e.lecciones_asociadas != null)
                .SelectMany(e => e.lecciones_asociadas.Select(id => new { id, e.titulo }))
                .ToDictionary(x => x.id, x => x.titulo);

            var resultado = new List<IslaMapaModel>();
            if (lecciones == null || lecciones.Count == 0) return resultado;

            var leccionesOrdenadas = lecciones.OrderBy(l => l.orden).ToList();
            bool actualAsignada = false;
            for (int i = 0; i < leccionesOrdenadas.Count; i++)
            {
                LeccionCompiladaModel leccion = leccionesOrdenadas[i];
                bool completada = progreso != null && progreso.LeccionCompletada(leccion.id);
                bool disponible = accesoLibre || i == 0 || progreso != null && progreso.LeccionCompletada(leccionesOrdenadas[i - 1].id);
                bool esActual = !actualAsignada && disponible && !completada;
                if (esActual) actualAsignada = true;

                resultado.Add(new IslaMapaModel
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

                if (!hitos.TryGetValue(leccion.id, out EscenarioRAModel escenario)) continue;
                bool desbloqueado = accesoLibre || escenario.lecciones_asociadas.All(id => progreso != null && progreso.LeccionCompletada(id));
                bool visitado = progreso != null && progreso.EscenarioVisitado(escenario.id);
                resultado.Add(new IslaMapaModel
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
            foreach (var isla in resultado) isla.TituloSeccion = ObtenerTextoSeccion(isla.Region);
            return resultado;
        }
        private static string ObtenerTextoSeccion(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                return "¡Aprende y avanza!";

            string lower = titulo.ToLowerInvariant();
            if (lower.Contains("encuentro") || lower.Contains("comunicaci"))
                return "¡Primeros pasos y saludos!";
            if (lower.Contains("familia") || lower.Contains("vestimenta"))
                return "¡Conoce a tu familia y vestimenta!";
            if (lower.Contains("casa") || lower.Contains("objetos"))
                return "¡El hogar y la vida diaria!";
            if (lower.Contains("cocina") || lower.Contains("alimentos"))
                return "¡Sabores de la cocina tradicional!";
            if (lower.Contains("animal") || lower.Contains("ecosistema"))
                return "¡Amigos del reino animal!";
            if (lower.Contains("naturaleza") || lower.Contains("orientaci"))
                return "¡Caminos y secretos de la naturaleza!";

            return $"¡{titulo}!";
        }
    }

}
