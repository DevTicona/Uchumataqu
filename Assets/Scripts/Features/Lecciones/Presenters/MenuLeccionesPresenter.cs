using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Core.UI.Views;
using Assets.Scripts.Features.Lecciones.Models;
using Assets.Scripts.Features.Lecciones.Services;
using Assets.Scripts.Features.ProgresoUsuario.Services;
using Assets.Scripts.Features.Lecciones.Views;
using Assets.Scripts.Core.Sistema;

namespace Assets.Scripts.Features.MenuPrincipal.Presenters
{
    public class MenuLeccionesPresenter : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private MenuLeccionesView menuView;
        [SerializeField] private HeaderView headerView;

        [Header("Sistema")]
        [SerializeField] private NavigationBackHandler backHandler;

        private GameObject prefabIsla;
        private Sprite iconoBloqueado;
        private Sprite iconoEnProgreso;
        private Sprite iconoCompletado;
        private Dictionary<string, Sprite> spritesIslas;

        private List<IslaMapaData> islasMapa;

        private void Start()
        {
            if (menuView == null || headerView == null)
            {
                Debug.LogError("[MenuLeccionesPresenter] Views no asignadas.");
                enabled = false;
                return;
            }

            ConfigurarHeader();
            ConfigurarBackHandler();
            CargarRecursosIniciales();
        }

        private async void CargarRecursosIniciales()
        {
            prefabIsla = await MenuLeccionesService.CargarPrefabIslaAsync();

            if (prefabIsla == null)
            {
                Debug.LogError("[MenuLeccionesPresenter] No se pudo cargar el prefab de isla.");
                return;
            }

            menuView.SetPrefabIsla(prefabIsla);

            var taskIslas   = MenuLeccionesService.CargarDictSpritesIslasAsync();
            var taskIconos  = MenuLeccionesService.CargarSpritesEstadoAsync();
            var taskLeccs   = LeccionesService.ObtenerLeccionesAsync();
            var taskTemas   = TemasService.ObtenerTemasAsync();

            await Task.WhenAll(taskIslas, taskIconos, taskLeccs, taskTemas);

            spritesIslas = taskIslas.Result;
            (iconoBloqueado, iconoEnProgreso, iconoCompletado) = taskIconos.Result;
            var lecciones = taskLeccs.Result;
            var temas     = taskTemas.Result;

            if (lecciones == null || lecciones.Count == 0)
            {
                Debug.LogError("[MenuLeccionesPresenter] No hay lecciones disponibles.");
                return;
            }

            InicializarMapa(lecciones, temas);
        }

        private void OnDestroy()
        {
            MenuLeccionesService.LimpiarCache();
            LeccionesService.LimpiarCache();
            TemasService.LimpiarCache();
        }

        // -------------------------------------------------
        // CONFIGURACIÓN
        // -------------------------------------------------

        private void ConfigurarHeader()
        {
            headerView.BindBotonAtras(VolverAlMenuPrincipal);
            headerView.SetTitulo("Lecciones");
            headerView.SetSubtitulo("Elige qué aprender hoy");
            headerView.OcultarIcono();
        }

        private void ConfigurarBackHandler()
        {
            if (backHandler != null)
                backHandler.Configurar(VolverAlMenuPrincipal);
        }

        private void VolverAlMenuPrincipal()
        {
            SceneManager.LoadScene("MenuPrincipal");
        }

        // -------------------------------------------------
        // MAPA
        // -------------------------------------------------

        private void InicializarMapa(List<LeccionItemData> lecciones, List<TemaItemData> temas)
        {
            menuView.LimpiarIslas();

            islasMapa = ConstruirIslasMapa(lecciones, temas);

            int total = islasMapa.Count;
            menuView.PrepararLayout(total);

            for (int i = 0; i < total; i++)
            {
                var isla = islasMapa[i];

                menuView.CrearIsla(
                    isla,
                    iconoBloqueado,
                    iconoEnProgreso,
                    iconoCompletado,
                    i,
                    total,
                    OnIslaSeleccionada
                );

                var islaView = menuView.ObtenerIsla(isla.Id);
                if (islaView == null) continue;

                if (spritesIslas != null && spritesIslas.TryGetValue(isla.Id, out var sprite))
                    islaView.SetIslaSprite(sprite);
                else
                    Debug.LogWarning($"[MenuLeccionesPresenter] Sin sprite para isla: {isla.Id}");
            }

            Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Construye la lista ordenada de islas: lecciones del tema + isla RA al final de cada tema.
        /// </summary>
        private List<IslaMapaData> ConstruirIslasMapa(List<LeccionItemData> lecciones, List<TemaItemData> temas)
        {
            var resultado = new List<IslaMapaData>();

            var temasOrdenados    = temas?.OrderBy(t => t.orden).ToList() ?? new List<TemaItemData>();
            var leccionesPorTema  = lecciones
                .Where(l => l != null)
                .OrderBy(l => l.orden)
                .GroupBy(l => l.id_tema)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Usamos un índice global para determinar desbloqueo secuencial
            IslaMapaData islaAnterior = null;

            foreach (var tema in temasOrdenados)
            {
                if (!leccionesPorTema.TryGetValue(tema.id, out var leccionesDelTema))
                    continue;

                // Islas de lecciones normales
                foreach (var leccion in leccionesDelTema)
                {
                    var estado = ResolverEstado(leccion.id, islaAnterior);
                    var isla   = new IslaMapaData(leccion.id, leccion.nombre, TipoIsla.Leccion, tema.id, leccion.orden, estado);
                    resultado.Add(isla);
                    islaAnterior = isla;
                }

                // Isla RA al final del tema
                if (!string.IsNullOrEmpty(tema.ra_id))
                {
                    bool todasCompletadas = leccionesDelTema.All(l => ProgresoService.EstaCompletada(l.id));
                    var estadoRA = todasCompletadas
                        ? (ProgresoService.EstaCompletada(tema.ra_id) ? EstadoLeccion.Completada : EstadoLeccion.EnProgreso)
                        : EstadoLeccion.Bloqueada;

                    var islaRA = new IslaMapaData(tema.ra_id, $"RA: {tema.nombre}", TipoIsla.RA, tema.id, int.MaxValue, estadoRA);
                    resultado.Add(islaRA);
                    islaAnterior = islaRA;
                }
            }

            return resultado;
        }

        private EstadoLeccion ResolverEstado(string id, IslaMapaData anterior)
        {
            if (ProgresoService.EstaCompletada(id))
                return EstadoLeccion.Completada;

            // Primera isla o la anterior está completada → disponible
            if (anterior == null || anterior.Estado == EstadoLeccion.Completada)
                return EstadoLeccion.EnProgreso;

            return EstadoLeccion.Bloqueada;
        }

        // -------------------------------------------------
        // INTERACCIÓN
        // -------------------------------------------------

        private void OnIslaSeleccionada(IslaMapaData isla)
        {
            if (isla == null || isla.Estado == EstadoLeccion.Bloqueada) return;

            if (isla.Tipo == TipoIsla.RA)
            {
                PlayerPrefs.SetString("TEMA_RA_ACTUAL", isla.IdTema);
                PlayerPrefs.SetString("RA_ID_ACTUAL", isla.Id);
                PlayerPrefs.Save();
                SceneManager.LoadScene("RealidadAumentada");
            }
            else
            {
                PlayerPrefs.SetString("LECCION_ACTUAL", isla.Id);
                PlayerPrefs.Save();
                SceneManager.LoadScene("LeccionBase");
            }
        }
    }
}