using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Data.Corpus;

namespace Assets.Scripts.Persistencia
{
    /// <summary>
    /// Repositorio central en memoria para el corpus validado de Uchumataqu.
    /// Carga palabras.json, oraciones.json, lecciones_compiladas.json y escenarios_ra.json.
    /// </summary>
    public static class CorpusRepository
    {
        private const string KEY_PALABRAS = "json_palabras";
        private const string KEY_ORACIONES = "json_oraciones";
        private const string KEY_LECCIONES = "json_lecciones";
        private const string KEY_ESCENARIOS_RA = "json_escenarios_ra";

        private static Dictionary<string, PalabraData> _palabrasDict;
        private static Dictionary<string, FormaData> _formasDict;
        private static Dictionary<string, OracionData> _oracionesDict;
        private static Dictionary<string, LeccionCompiladaData> _leccionesDict;
        private static Dictionary<string, EscenarioRAData> _escenariosDict;
        private static List<LeccionCompiladaData> _leccionesOrdenadas;
        private static List<EscenarioRAData> _escenariosList;

        public static bool EstaInicializado => _palabrasDict != null && _leccionesDict != null;

        public static async Task<bool> InicializarAsync()
        {
            if (EstaInicializado) return true;

            try
            {
                // Cargar en paralelo los 4 archivos JSON de Content/Data
                var taskPalabras = CargarPalabrasAsync();
                var taskOraciones = CargarOracionesAsync();
                var taskLecciones = CargarLeccionesCompiladasAsync();
                var taskEscenarios = CargarEscenariosRAAsync();

                await Task.WhenAll(taskPalabras, taskOraciones, taskLecciones, taskEscenarios);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CorpusRepository] Error inicializando corpus: {ex.Message}");
                return false;
            }
        }

        private static async Task CargarPalabrasAsync()
        {
            var json = await CargarJsonTextoAsync(KEY_PALABRAS, "palabras.json");
            var wrapper = JsonUtility.FromJson<PalabrasWrapperData>(json);
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
            var wrapper = JsonUtility.FromJson<OracionesWrapperData>(json);
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
            var wrapper = JsonUtility.FromJson<LeccionesCompiladasWrapperData>(json);
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
            var wrapper = JsonUtility.FromJson<EscenariosRAWrapperData>(json);
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
                if (textAsset != null) return textAsset.text;
            }
            catch
            {
                // Fallback directo a StreamingAssets o Content/Data en Editor
            }

            var path = System.IO.Path.Combine(Application.dataPath, "Content", "Data", fallbackFileName);
            if (System.IO.File.Exists(path))
            {
                return System.IO.File.ReadAllText(path);
            }

            throw new System.IO.FileNotFoundException($"[CorpusRepository] No se encontró el recurso '{fallbackFileName}'.");
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
