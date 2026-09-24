using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Persistencia
{
    /// <summary>
    /// Repositorio central en memoria para el corpus validado de Uchumataqu.
    /// Carga palabras.json, oraciones.json, lecciones_compiladas.json y escenarios_ra.json.
    /// </summary>
    public static class CorpusRepository
    {
        private const string KEY_PALABRAS = ContentAddress.DatosPalabras;
        private const string KEY_ORACIONES = ContentAddress.DatosOraciones;
        private const string KEY_LECCIONES = ContentAddress.DatosLecciones;
        private const string KEY_ESCENARIOS_RA = ContentAddress.DatosEscenariosRa;

        private static Dictionary<string, PalabraModel> _palabrasDict;
        private static Dictionary<string, FormaModel> _formasDict;
        private static Dictionary<string, OracionModel> _oracionesDict;
        private static Dictionary<string, LeccionCompiladaModel> _leccionesDict;
        private static Dictionary<string, EscenarioRAModel> _escenariosDict;
        private static List<LeccionCompiladaModel> _leccionesOrdenadas;
        private static List<EscenarioRAModel> _escenariosList;

        public static bool EstaInicializado => _palabrasDict != null && _leccionesDict != null && _palabrasDict.Count > 0;

        public static bool Inicializar()
        {
            if (EstaInicializado) return true;

#if UNITY_EDITOR
            try
            {
                ProcesarPalabrasJson(CorpusJsonService.LeerJsonDirecto("palabras.json"));
                ProcesarOracionesJson(CorpusJsonService.LeerJsonDirecto("oraciones.json"));
                ProcesarLeccionesJson(CorpusJsonService.LeerJsonDirecto("lecciones_compiladas.json"));
                ProcesarEscenariosJson(CorpusJsonService.LeerJsonDirecto("escenarios_ra.json"));
                return true;
            }
            catch (Exception ex)
            {
                JsonUtilityHelper.LogError($"[CorpusRepository] Error inicializando corpus directo: {ex.Message}");
                return false;
            }
#else
            JsonUtilityHelper.LogError("[CorpusRepository] La carga síncrona directa solo está disponible para pruebas de Editor. Use InicializarAsync en runtime.");
            return false;
#endif
        }

        public static async Task<bool> InicializarAsync()
        {
            if (EstaInicializado) return true;

            try
            {
                var taskPalabras = CargarPalabrasAsync();
                var taskOraciones = CargarOracionesAsync();
                var taskLecciones = CargarLeccionesCompiladasAsync();
                var taskEscenarios = CargarEscenariosRAAsync();

                await Task.WhenAll(taskPalabras, taskOraciones, taskLecciones, taskEscenarios);
                return true;
            }
            catch (Exception ex)
            {
                JsonUtilityHelper.LogError($"[CorpusRepository] Error inicializando corpus async: {ex.Message}");
                return false;
            }
        }

        private static async Task CargarPalabrasAsync()
        {
            var json = await CargarJsonTextoAsync(KEY_PALABRAS, "palabras.json");
            ProcesarPalabrasJson(json);
        }

        private static void ProcesarPalabrasJson(string json)
        {
            var wrapper = JsonUtilityHelper.FromJson<PalabrasCorpusModel>(json);
            _palabrasDict = new Dictionary<string, PalabraModel>();
            _formasDict = new Dictionary<string, FormaModel>();

            if (wrapper?.palabras != null)
            {
                foreach (var p in wrapper.palabras)
                {
                    _palabrasDict[p.id] = p;
                    if (p.formas != null)
                    {
                        foreach (var f in p.formas)
                        {
                            _formasDict[f.id] = f;
                        }
                    }
                }
            }
        }

        private static async Task CargarOracionesAsync()
        {
            var json = await CargarJsonTextoAsync(KEY_ORACIONES, "oraciones.json");
            ProcesarOracionesJson(json);
        }

        private static void ProcesarOracionesJson(string json)
        {
            var wrapper = JsonUtilityHelper.FromJson<OracionesCorpusModel>(json);
            _oracionesDict = new Dictionary<string, OracionModel>();

            if (wrapper?.oraciones != null)
            {
                foreach (var o in wrapper.oraciones)
                {
                    _oracionesDict[o.id] = o;
                }
            }
        }

        private static async Task CargarLeccionesCompiladasAsync()
        {
            var json = await CargarJsonTextoAsync(KEY_LECCIONES, "lecciones_compiladas.json");
            ProcesarLeccionesJson(json);
        }

        private static void ProcesarLeccionesJson(string json)
        {
            var wrapper = JsonUtilityHelper.FromJson<LeccionesCompiladasCorpusModel>(json);
            if (wrapper == null || (wrapper.version != 3 && wrapper.version != 4))
                throw new InvalidDataException(
                    $"Se esperaba lecciones_compiladas.json versión 3 o 4 y se recibió '{wrapper?.version.ToString() ?? "sin versión"}'.");
            _leccionesDict = new Dictionary<string, LeccionCompiladaModel>();
            _leccionesOrdenadas = new List<LeccionCompiladaModel>();

            if (wrapper?.lecciones != null)
            {
                _leccionesOrdenadas = wrapper.lecciones.OrderBy(l => l.orden).ToList();
                foreach (var l in _leccionesOrdenadas)
                {
                    _leccionesDict[l.id] = l;
                }
            }
        }

        private static async Task CargarEscenariosRAAsync()
        {
            var json = await CargarJsonTextoAsync(KEY_ESCENARIOS_RA, "escenarios_ra.json");
            ProcesarEscenariosJson(json);
        }

        private static void ProcesarEscenariosJson(string json)
        {
            var wrapper = JsonUtilityHelper.FromJson<EscenariosRACorpusModel>(json);
            _escenariosDict = new Dictionary<string, EscenarioRAModel>();
            _escenariosList = new List<EscenarioRAModel>();

            if (wrapper?.escenarios != null)
            {
                _escenariosList = wrapper.escenarios;
                foreach (var e in wrapper.escenarios)
                {
                    _escenariosDict[e.id] = e;
                }
            }
        }

        private static async Task<string> CargarJsonTextoAsync(string addressableKey, string fallbackFileName)
        {
            try
            {
                var texto = await CorpusJsonService.CargarTextoAsync(addressableKey);
                if (!string.IsNullOrWhiteSpace(texto)) return texto;
            }
            catch (Exception ex)
            {
                JsonUtilityHelper.LogError($"[CorpusRepository] Falló Addressable '{addressableKey}': {ex.Message}");
            }

#if UNITY_EDITOR
            try
            {
                string directo = CorpusJsonService.LeerJsonDirecto(fallbackFileName);
                if (!string.IsNullOrWhiteSpace(directo)) return directo;
            }
            catch (Exception ex)
            {
                JsonUtilityHelper.LogError($"[CorpusRepository] Falló fallback de Editor '{fallbackFileName}': {ex.Message}");
            }
#endif

            throw new FileNotFoundException($"[CorpusRepository] No se encontró el recurso '{fallbackFileName}'.");
        }

        // =========================================
        // CONSULTAS DE PALABRAS Y FORMAS
        // =========================================

        public static PalabraModel ObtenerPalabra(string id)
        {
            if (string.IsNullOrEmpty(id) || _palabrasDict == null) return null;
            _palabrasDict.TryGetValue(id, out var p);
            return p;
        }

        public static FormaModel ObtenerForma(string id)
        {
            if (string.IsNullOrEmpty(id) || _formasDict == null) return null;
            _formasDict.TryGetValue(id, out var f);
            return f;
        }

        public static List<PalabraModel> ObtenerPalabras(IEnumerable<string> ids)
        {
            var res = new List<PalabraModel>();
            if (ids == null || _palabrasDict == null) return res;
            foreach (var id in ids)
            {
                if (_palabrasDict.TryGetValue(id, out var p)) res.Add(p);
            }
            return res;
        }

        public static List<PalabraModel> ObtenerTodasLasPalabras() => _palabrasDict?.Values.ToList() ?? new List<PalabraModel>();

        // =========================================
        // CONSULTAS DE ORACIONES
        // =========================================

        public static OracionModel ObtenerOracion(string id)
        {
            if (string.IsNullOrEmpty(id) || _oracionesDict == null) return null;
            _oracionesDict.TryGetValue(id, out var o);
            return o;
        }

        public static List<OracionModel> ObtenerOraciones(IEnumerable<string> ids)
        {
            var res = new List<OracionModel>();
            if (ids == null || _oracionesDict == null) return res;
            foreach (var id in ids)
            {
                if (_oracionesDict.TryGetValue(id, out var o)) res.Add(o);
            }
            return res;
        }

        public static List<OracionModel> ObtenerTodasLasOraciones() => _oracionesDict?.Values.ToList() ?? new List<OracionModel>();

        // =========================================
        // CONSULTAS DE LECCIONES
        // =========================================

        public static LeccionCompiladaModel ObtenerLeccion(string id)
        {
            if (string.IsNullOrEmpty(id) || _leccionesDict == null) return null;
            _leccionesDict.TryGetValue(id, out var l);
            return l;
        }

        public static List<LeccionCompiladaModel> ObtenerTodasLasLecciones() => _leccionesOrdenadas ?? new List<LeccionCompiladaModel>();

        // =========================================
        // CONSULTAS DE ESCENARIOS RA
        // =========================================

        public static EscenarioRAModel ObtenerEscenarioRA(string id)
        {
            if (string.IsNullOrEmpty(id) || _escenariosDict == null) return null;
            _escenariosDict.TryGetValue(id, out var e);
            return e;
        }

        public static List<EscenarioRAModel> ObtenerTodosLosEscenariosRA() => _escenariosList ?? new List<EscenarioRAModel>();

        // Aliases de conveniencia
        public static PalabraModel ObtenerPalabraPorId(string id) => ObtenerPalabra(id);
        public static List<PalabraModel> ObtenerTodasPalabras() => ObtenerTodasLasPalabras();
        public static OracionModel ObtenerOracionPorId(string id) => ObtenerOracion(id);
        public static List<OracionModel> ObtenerTodasOraciones() => ObtenerTodasLasOraciones();
        public static LeccionCompiladaModel ObtenerLeccionPorId(string id) => ObtenerLeccion(id);
        public static List<LeccionCompiladaModel> ObtenerTodasLecciones() => ObtenerTodasLasLecciones();
        public static EscenarioRAModel ObtenerEscenarioRaPorId(string id) => ObtenerEscenarioRA(id);
        public static List<EscenarioRAModel> ObtenerTodosEscenariosRa() => ObtenerTodosLosEscenariosRA();

        public static void LimpiarCache()
        {
            _palabrasDict = null;
            _formasDict = null;
            _oracionesDict = null;
            _leccionesDict = null;
            _escenariosDict = null;
            _leccionesOrdenadas = null;
            _escenariosList = null;
        }
    }
}
