using System;
using System.IO;
using UnityEngine;
using Assets.Scripts.Data.Persistencia;

namespace Assets.Scripts.Persistencia
{
    /// <summary>
    /// Repositorio de guardado y carga local del progreso del usuario.
    /// Especificado en Docs/Specifications/04-flujo-leccion-app.md (Sección 11).
    /// </summary>
    public static class ProgresoRepository
    {
        private static string RutaArchivo
        {
            get
            {
                try
                {
                    return Path.Combine(Application.persistentDataPath, "ProgresoUsuario.json");
                }
                catch
                {
                    return Path.Combine(Path.GetTempPath(), "ProgresoUsuario.json");
                }
            }
        }

        private static ProgresoUsuario _progresoCache;

        public static ProgresoUsuario ObtenerProgreso()
        {
            if (_progresoCache != null) return _progresoCache;
            _progresoCache = Cargar();
            return _progresoCache;
        }

        public static ProgresoUsuario CargarProgreso() => ObtenerProgreso();

        private static ProgresoUsuario Cargar()
        {
            try
            {
                if (File.Exists(RutaArchivo))
                {
                    string json = File.ReadAllText(RutaArchivo);
                    var progreso = Assets.Scripts.Utils.JsonUtilityHelper.FromJson<ProgresoUsuario>(json);
                    if (progreso != null) return progreso;
                }
            }
            catch (Exception ex)
            {
                Assets.Scripts.Utils.JsonUtilityHelper.LogError($"[ProgresoRepository] Error cargando progreso: {ex.Message}");
            }

            return new ProgresoUsuario();
        }

        public static void Guardar()
        {
            if (_progresoCache == null) return;

            try
            {
                string json = Assets.Scripts.Utils.JsonUtilityHelper.ToJson(_progresoCache, prettyPrint: true);
                File.WriteAllText(RutaArchivo, json);
            }
            catch (Exception ex)
            {
                Assets.Scripts.Utils.JsonUtilityHelper.LogError($"[ProgresoRepository] Error guardando progreso: {ex.Message}");
            }
        }

        public static bool EstaLeccionCompletada(string leccionId)
        {
            return ObtenerProgreso().LeccionCompletada(leccionId);
        }

        public static bool EstaEscenarioDesbloqueado(string escenarioId)
        {
            var escenario = CorpusRepository.ObtenerEscenarioRA(escenarioId);
            if (escenario == null || escenario.lecciones_asociadas == null || escenario.lecciones_asociadas.Count == 0)
                return false;

            var progreso = ObtenerProgreso();
            foreach (var leccId in escenario.lecciones_asociadas)
            {
                if (!progreso.LeccionCompletada(leccId))
                    return false;
            }
            return true;
        }

        public static void CompletarLeccion(string leccionId, int aciertos, int errores, int totalActividades)
        {
            var progreso = ObtenerProgreso();
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

            progreso.ultima_leccion_id = leccionId;
            Guardar();
        }

        public static void GuardarLeccionCompletada(string leccionId, int aciertos, int errores, int totalActividades)
        {
            CompletarLeccion(leccionId, aciertos, errores, totalActividades);
        }

        public static void GuardarLeccionEnProgreso(string leccionId, int pasoActual, int aciertos, int errores)
        {
            if (string.IsNullOrWhiteSpace(leccionId)) return;
            var progreso = ObtenerProgreso();
            var leccion = progreso.ObtenerOCrearLeccion(leccionId);
            if (leccion.estado == "completada") return;
            leccion.estado = "en_progreso";
            leccion.paso_actual = Mathf.Max(0, pasoActual);
            leccion.aciertos = Mathf.Max(0, aciertos);
            leccion.errores = Mathf.Max(0, errores);
            progreso.ultima_leccion_id = leccionId;
            Guardar();
        }

        public static void ReiniciarProgreso()
        {
            _progresoCache = new ProgresoUsuario();
            Guardar();
            Assets.Scripts.Utils.JsonUtilityHelper.LogInfo("[ProgresoRepository] Progreso reiniciado.");
        }
    }
}
