using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.Sistema;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.RA;

namespace Assets.Scripts.UI.RealidadAumentada
{
    /// <summary>
    /// Presenter de la escena 04_RealidadAumentada.
    /// Coordina el escenario activo según ContextoNavegacion.EscenarioRaId y la UI superpuesta.
    /// Especificado en Docs/UI/05-escena-realidad-aumentada.md.
    /// </summary>
    public class RealidadAumentadaPresenter : MonoBehaviour
    {
        [SerializeField] private RealidadAumentadaView view;
        [SerializeField] private GestorEscenaRA gestorRA;
        [SerializeField] private NavigationBackHandler backHandler;

        private PalabraData _palabraActiva;

        private void Start()
        {
            if (view == null) view = FindAnyObjectByType<RealidadAumentadaView>();
            if (gestorRA == null) gestorRA = FindAnyObjectByType<GestorEscenaRA>();
            if (backHandler == null) backHandler = FindAnyObjectByType<NavigationBackHandler>();

            if (backHandler != null)
                backHandler.Configurar(VolverAlMapa);

            if (view != null)
            {
                view.OnVolverClick += VolverAlMapa;
                view.OnAudioObjetoClick += ReproducirAudioObjeto;
            }

            InicializarEscenario();
        }

        private void OnDestroy()
        {
            if (view != null)
            {
                view.OnVolverClick -= VolverAlMapa;
                view.OnAudioObjetoClick -= ReproducirAudioObjeto;
            }
        }

        private void InicializarEscenario()
        {
            if (gestorRA == null)
            {
                var go = new GameObject("[GestorEscenaRA]");
                gestorRA = go.AddComponent<GestorEscenaRA>();
            }

            string escenarioId = ContextoNavegacion.EscenarioRaId;
            if (gestorRA.CargarEscenario(escenarioId))
            {
                if (view != null)
                    view.ConfigurarHeader(gestorRA.EscenarioActual.titulo);

                gestorRA.RegistrarVisita();
                Debug.Log($"[RealidadAumentadaPresenter] Escenario '{escenarioId}' cargado correctamente.");
            }
            else
            {
                Debug.LogError($"[RealidadAumentadaPresenter] Error cargando escenario '{escenarioId}'. Volviendo...");
                VolverAlMapa();
            }
        }

        public void MostrarDetalleObjeto(PalabraData palabra)
        {
            _palabraActiva = palabra;
            if (palabra != null && view != null)
            {
                view.MostrarPanelObjeto(palabra.uch, palabra.es);
            }
        }

        private async void ReproducirAudioObjeto()
        {
            if (_palabraActiva != null)
            {
                await AudioService.Instance.ReproducirPalabraAsync(_palabraActiva.id);
            }
        }

        private void VolverAlMapa()
        {
            NavigationService.IrAMain(PaginaMain.Lecciones);
        }
    }
}
