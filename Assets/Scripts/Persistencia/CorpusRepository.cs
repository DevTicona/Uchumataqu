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

        private static Dictionary<string, PalabraData> _palabrasDict;
        private static Dictionary<string, FormaData> _formasDict;
        private static Dictionary<string, OracionData> _oracionesDict;
        private static Dictionary<string, LeccionCompiladaData> _leccionesDict;
        private static Dictionary<string, EscenarioRAData> _escenariosDict;
        private static List<LeccionCompiladaData> _leccionesOrdenadas;
        private static List<EscenarioRAData> _escenariosList;

        public static bool EstaInicializado => _palabrasDict != null && _leccionesDict != null && _palabrasDict.Count > 0;

        public static bool Inicializar()
        {
            if (EstaInicializado) return true;

#if UNITY_EDITOR
            try
            {
                ProcesarPalabrasJson(LeerJsonDirecto("palabras.json"));
                ProcesarOracionesJson(LeerJsonDirecto("oraciones.json"));
                ProcesarLeccionesJson(LeerJsonDirecto("lecciones_compiladas.json"));
                ProcesarEscenariosJson(LeerJsonDirecto("escenarios_ra.json"));
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

        private static string LeerJsonDirecto(string fileName)
        {
            var pathProyecto = Path.Combine(Application.dataPath, "Content", "Data", fileName);
            if (File.Exists(pathProyecto)) return File.ReadAllText(pathProyecto);

            string[] possiblePaths = new string[]
            {
                Path.Combine("Assets", "Content", "Data", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Content", "Data", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "Assets", "Content", "Data", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Assets", "Content", "Data", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Content", "Data", fileName)
            };

            foreach (var p in possiblePaths)
            {
                if (File.Exists(p)) return File.ReadAllText(Path.GetFullPath(p));
            }

            var pathStreaming = Path.Combine(Application.streamingAssetsPath, "Data", fileName);
            if (File.Exists(pathStreaming)) return File.ReadAllText(pathStreaming);

            throw new FileNotFoundException($"[CorpusRepository] No se encontró el recurso '{fileName}'.");
        }

        private static async Task CargarPalabrasAsync()
        {
            var json = await CargarJsonTextoAsync(KEY_PALABRAS, "palabras.json");
            ProcesarPalabrasJson(json);
        }

        private static void ProcesarPalabrasJson(string json)
        {
            var wrapper = JsonUtilityHelper.FromJson<PalabrasWrapperData>(json);
            _palabrasDict = new Dictionary<string, PalabraData>();
            _formasDict = new Dictionary<string, FormaData>();

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
            var wrapper = JsonUtilityHelper.FromJson<OracionesWrapperData>(json);
            _oracionesDict = new Dictionary<string, OracionData>();

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
            var wrapper = JsonUtilityHelper.FromJson<LeccionesCompiladasWrapperData>(json);
            _leccionesDict = new Dictionary<string, LeccionCompiladaData>();
            _leccionesOrdenadas = new List<LeccionCompiladaData>();

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
            var wrapper = JsonUtilityHelper.FromJson<EscenariosRAWrapperData>(json);
            _escenariosDict = new Dictionary<string, EscenarioRAData>();
            _escenariosList = new List<EscenarioRAData>();

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
                var textAsset = await AddressablesService.CargarAssetAsync<TextAsset>(addressableKey);
                if (textAsset != null && !string.IsNullOrWhiteSpace(textAsset.text)) return textAsset.text;
            }
            catch (Exception ex)
            {
                JsonUtilityHelper.LogError($"[CorpusRepository] Falló Addressable '{addressableKey}': {ex.Message}");
            }

#if UNITY_EDITOR
            try
            {
                string directo = LeerJsonDirecto(fallbackFileName);
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

        public static PalabraData ObtenerPalabra(string id)
        {
            if (string.IsNullOrEmpty(id) || _palabrasDict == null) return null;
            _palabrasDict.TryGetValue(id, out var p);
            return p;
        }

        public static FormaData ObtenerForma(string id)
        {
            if (string.IsNullOrEmpty(id) || _formasDict == null) return null;
            _formasDict.TryGetValue(id, out var f);
            return f;
        }

        public static List<PalabraData> ObtenerPalabras(IEnumerable<string> ids)
        {
            var res = new List<PalabraData>();
            if (ids == null || _palabrasDict == null) return res;
            foreach (var id in ids)
            {
                if (_palabrasDict.TryGetValue(id, out var p)) res.Add(p);
            }
            return res;
        }

        public static List<PalabraData> ObtenerTodasLasPalabras() => _palabrasDict?.Values.ToList() ?? new List<PalabraData>();

        // =========================================
        // CONSULTAS DE ORACIONES
        // =========================================

        public static OracionData ObtenerOracion(string id)
        {
            if (string.IsNullOrEmpty(id) || _oracionesDict == null) return null;
            _oracionesDict.TryGetValue(id, out var o);
            return o;
        }

        public static List<OracionData> ObtenerOraciones(IEnumerable<string> ids)
        {
            var res = new List<OracionData>();
            if (ids == null || _oracionesDict == null) return res;
            foreach (var id in ids)
            {
                if (_oracionesDict.TryGetValue(id, out var o)) res.Add(o);
            }
            return res;
        }

        public static List<OracionData> ObtenerTodasLasOraciones() => _oracionesDict?.Values.ToList() ?? new List<OracionData>();

        // =========================================
        // CONSULTAS DE LECCIONES
        // =========================================

        public static LeccionCompiladaData ObtenerLeccion(string id)
        {
            if (string.IsNullOrEmpty(id) || _leccionesDict == null) return null;
            _leccionesDict.TryGetValue(id, out var l);
            return l;
        }

        public static List<LeccionCompiladaData> ObtenerTodasLasLecciones() => _leccionesOrdenadas ?? new List<LeccionCompiladaData>();

        // =========================================
        // CONSULTAS DE ESCENARIOS RA
        // =========================================

        public static EscenarioRAData ObtenerEscenarioRA(string id)
        {
            if (string.IsNullOrEmpty(id) || _escenariosDict == null) return null;
            _escenariosDict.TryGetValue(id, out var e);
            return e;
        }

        public static List<EscenarioRAData> ObtenerTodosLosEscenariosRA() => _escenariosList ?? new List<EscenarioRAData>();

        // Aliases de conveniencia
        public static PalabraData ObtenerPalabraPorId(string id) => ObtenerPalabra(id);
        public static List<PalabraData> ObtenerTodasPalabras() => ObtenerTodasLasPalabras();
        public static OracionData ObtenerOracionPorId(string id) => ObtenerOracion(id);
        public static List<OracionData> ObtenerTodasOraciones() => ObtenerTodasLasOraciones();
        public static LeccionCompiladaData ObtenerLeccionPorId(string id) => ObtenerLeccion(id);
        public static List<LeccionCompiladaData> ObtenerTodasLecciones() => ObtenerTodasLasLecciones();
        public static EscenarioRAData ObtenerEscenarioRaPorId(string id) => ObtenerEscenarioRA(id);
        public static List<EscenarioRAData> ObtenerTodosEscenariosRa() => ObtenerTodosLosEscenariosRA();

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
