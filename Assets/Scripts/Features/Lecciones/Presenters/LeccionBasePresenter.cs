using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.Core.UI.Views;
using Assets.Scripts.Core.Sistema;
using Assets.Scripts.Features.Lecciones.Models;
using Assets.Scripts.Features.Lecciones.Services;
using Assets.Scripts.Features.Lecciones.Views;
using Assets.Scripts.Features.ProgresoUsuario.Services;

namespace Assets.Scripts.Features.Lecciones.Presenters
{
    public class LeccionBasePresenter : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private LeccionBaseView leccionView;
        [SerializeField] private HeaderView headerView;

        [Header("Sistema")]
        [SerializeField] private NavigationBackHandler backHandler;

        // Datos de la lección
        private LeccionItemData leccionActual;
        private List<PalabraItemData> palabras;
        private List<OracionItemData> oraciones;

        // Bloques
        private List<List<PalabraItemData>> bloques;
        private int indiceBloqueActual;

        // Estado dentro del bloque
        private int indicePalabraActual;
        private List<string> actividadesAnclaje;
        private int indiceActividadAnclaje;

        // Producción
        private List<string> actividadesProduccion;
        private int indiceActividadProduccion;

        // Fase actual
        private FaseLeccion faseActual;
        private bool completoPlnPalabraExposicion;

        private enum FaseLeccion { PlnPalabraExposicion, Exposicion, Anclaje, Produccion }

        // =========================================
        // INICIO
        // =========================================

        private void Start()
        {
            if (leccionView == null || headerView == null)
            {
                Debug.LogError("[LeccionBasePresenter] Views no asignadas.");
                enabled = false;
                return;
            }

            ConfigurarBackHandler();
            CargarLeccion();
        }

        private void OnDestroy()
        {
            LeccionesService.LimpiarCache();
            VocabularioService.LimpiarCache();
            OracionesService.LimpiarCache();
        }

        private void ConfigurarBackHandler()
        {
            if (backHandler != null)
                backHandler.Configurar(VolverAlMapa);
        }

        // =========================================
        // CARGA
        // =========================================

        private async void CargarLeccion()
        {
            string idLeccion = PlayerPrefs.GetString("LECCION_ACTUAL", "");
            if (string.IsNullOrEmpty(idLeccion))
            {
                Debug.LogError("[LeccionBasePresenter] No hay lección seleccionada.");
                VolverAlMapa();
                return;
            }

            leccionActual = await LeccionesService.ObtenerLeccionPorIdAsync(idLeccion);
            if (leccionActual == null)
            {
                Debug.LogError($"[LeccionBasePresenter] Lección no encontrada: {idLeccion}");
                VolverAlMapa();
                return;
            }

            var taskPalabras  = VocabularioService.ObtenerPalabrasPorIdsAsync(leccionActual.palabras_base);
            var taskOraciones = OracionesService.ObtenerOracionesDeLeccionAsync(leccionActual);

            await Task.WhenAll(taskPalabras, taskOraciones);

            palabras  = taskPalabras.Result;
            oraciones = taskOraciones.Result;

            if (palabras == null || palabras.Count == 0)
            {
                Debug.LogError("[LeccionBasePresenter] La lección no tiene palabras.");
                VolverAlMapa();
                return;
            }

            headerView.SetTitulo(leccionActual.nombre);

            IniciarLeccion();
        }

        // =========================================
        // FLUJO PRINCIPAL
        // =========================================

        private void IniciarLeccion()
        {
            bloques = LeccionBaseService.DividirEnBloques(palabras);
            indiceBloqueActual = 0;
            completoPlnPalabraExposicion = false;

            // Mostrar primero la actividad pln_palabra con todas las palabras
            IniciarPlnPalabraExposicion();
        }

        // =========================================
        // FASE PLN PALABRA EXPOSICIÓN (ACTIVIDAD 1)
        // =========================================

        private void IniciarPlnPalabraExposicion()
        {
            faseActual = FaseLeccion.PlnPalabraExposicion;
            headerView.SetSubtitulo("Aprende las palabras");
            leccionView.MostrarActividadPlnPalabra(palabras, OnPlnPalabraExposicionCompletada);
        }

        private void OnPlnPalabraExposicionCompletada()
        {
            completoPlnPalabraExposicion = true;
            Debug.Log("[LeccionBasePresenter] PLN Palabra Exposición completada, iniciando bloques.");
            IniciarBloque();
        }

        private void IniciarBloque()
        {
            indicePalabraActual = 0;
            faseActual          = FaseLeccion.Exposicion;

            var bloque = bloques[indiceBloqueActual];
            headerView.SetSubtitulo($"Bloque {indiceBloqueActual + 1} de {bloques.Count}");

            MostrarPalabraActual(bloque);
        }

        // =========================================
        // FASE EXPOSICIÓN
        // =========================================

        private void MostrarPalabraActual(List<PalabraItemData> bloque)
        {
            var palabra = bloque[indicePalabraActual];
            leccionView.MostrarExposicion(
                palabra,
                indicePalabraActual + 1,
                bloque.Count,
                OnPLNCompletado
            );
        }

        /// <summary>
        /// Llamado por la View cuando el usuario completa el PLN de la palabra actual.
        /// </summary>
        private void OnPLNCompletado()
        {
            var bloque = bloques[indiceBloqueActual];
            indicePalabraActual++;

            if (indicePalabraActual < bloque.Count)
            {
                // Siguiente palabra del mismo bloque
                MostrarPalabraActual(bloque);
            }
            else
            {
                // Todas las palabras del bloque completadas → Anclaje
                IniciarAnclaje(bloque);
            }
        }

        // =========================================
        // FASE ANCLAJE
        // =========================================

        private void IniciarAnclaje(List<PalabraItemData> bloque)
        {
            faseActual              = FaseLeccion.Anclaje;
            actividadesAnclaje      = LeccionBaseService.SeleccionarActividadesAnclaje();
            indiceActividadAnclaje  = 0;

            MostrarActividadAnclaje(bloque);
        }

        private void MostrarActividadAnclaje(List<PalabraItemData> bloque)
        {
            string idActividad = actividadesAnclaje[indiceActividadAnclaje];
            leccionView.MostrarActividadAnclaje(idActividad, bloque, OnActividadAnclajCompletada);
        }

        private void OnActividadAnclajCompletada()
        {
            indiceActividadAnclaje++;

            if (indiceActividadAnclaje < actividadesAnclaje.Count)
            {
                MostrarActividadAnclaje(bloques[indiceBloqueActual]);
            }
            else
            {
                // Anclaje completo → siguiente bloque o producción
                indiceBloqueActual++;

                if (indiceBloqueActual < bloques.Count)
                    IniciarBloque();
                else
                    IniciarProduccion();
            }
        }

        // =========================================
        // FASE PRODUCCIÓN
        // =========================================

        private void IniciarProduccion()
        {
            faseActual                 = FaseLeccion.Produccion;
            actividadesProduccion      = LeccionBaseService.SeleccionarActividadesProduccion();
            indiceActividadProduccion  = 0;

            headerView.SetSubtitulo("Producción");
            MostrarActividadProduccion();
        }

        private void MostrarActividadProduccion()
        {
            string idActividad = actividadesProduccion[indiceActividadProduccion];
            leccionView.MostrarActividadProduccion(idActividad, oraciones, OnActividadProduccionCompletada);
        }

        private void OnActividadProduccionCompletada()
        {
            indiceActividadProduccion++;

            if (indiceActividadProduccion < actividadesProduccion.Count)
                MostrarActividadProduccion();
            else
                FinalizarLeccion();
        }

        // =========================================
        // FINALIZACIÓN
        // =========================================

        private void FinalizarLeccion()
        {
            ProgresoService.MarcarCompletada(leccionActual.id);
            VolverAlMapa();
        }

        private void VolverAlMapa()
        {
            SceneManager.LoadScene("MenuLecciones");
        }
    }
}