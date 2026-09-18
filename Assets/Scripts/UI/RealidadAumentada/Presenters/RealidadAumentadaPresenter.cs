using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.Assets;
using Assets.Scripts.Core.Sistema;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.RA;
using Assets.Scripts.Persistencia;

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
        [SerializeField] private GestorEscenarioRA gestorRA;
        [SerializeField] private NavegacionAtrasView backHandler;
        [SerializeField] private ObjetoRAView[] objetosRA = System.Array.Empty<ObjetoRAView>();

        private PalabraModel _palabraActiva;
        private readonly HashSet<ObjetoRAView> _objetosRegistrados = new HashSet<ObjetoRAView>();

        public void RegistrarObjeto(ObjetoRAView objeto)
        {
            if (objeto != null && _objetosRegistrados.Add(objeto))
                objeto.OnObjetoTocado += SeleccionarObjeto;
        }

        public void RetirarObjeto(ObjetoRAView objeto)
        {
            if (objeto != null && _objetosRegistrados.Remove(objeto))
                objeto.OnObjetoTocado -= SeleccionarObjeto;
        }

        private void Start()
        {
            if (view == null || gestorRA == null || backHandler == null)
            {
                Debug.LogError("[RealidadAumentadaPresenter] Faltan referencias de composición de la escena.");
                enabled = false;
                return;
            }

            if (backHandler != null)
                backHandler.Configurar(VolverAlMapa);

            if (view != null)
            {
                view.OnVolverClick += VolverAlMapa;
                view.OnAudioObjetoClick += ReproducirAudioObjeto;
            }

            foreach (var objeto in objetosRA)
                RegistrarObjeto(objeto);
            InicializarEscenario();
        }

        private void OnDestroy()
        {
            foreach (var objeto in _objetosRegistrados)
                if (objeto != null) objeto.OnObjetoTocado -= SeleccionarObjeto;
            _objetosRegistrados.Clear();
            if (backHandler != null) backHandler.Configurar(null);
            if (view != null)
            {
                view.OnVolverClick -= VolverAlMapa;
                view.OnAudioObjetoClick -= ReproducirAudioObjeto;
            }
        }

        private void InicializarEscenario()
        {
            string escenarioId = ContextoNavegacionModel.EscenarioRaId;
            if (gestorRA.CargarEscenario(escenarioId))
            {
                if (view != null)
                    view.ConfigurarHeader(gestorRA.EscenarioActual.titulo);

                if (!gestorRA.RegistrarVisita()) return;
                Debug.Log($"[RealidadAumentadaPresenter] Escenario '{escenarioId}' cargado correctamente.");
            }
            else
            {
                Debug.LogError($"[RealidadAumentadaPresenter] Error cargando escenario '{escenarioId}'. Volviendo...");
                VolverAlMapa();
            }
        }

        public void MostrarDetalleObjeto(PalabraModel palabra)
        {
            _palabraActiva = palabra;
            if (palabra != null && view != null)
            {
                view.MostrarPanelObjeto(palabra.uch, palabra.es);
            }
        }

        private void SeleccionarObjeto(string id)
        {
            MostrarDetalleObjeto(CorpusRepository.ObtenerPalabra(id));
            if (_palabraActiva != null) ReproducirAudioObjeto();
        }

        private async void ReproducirAudioObjeto()
        {
            try
            {
                if (_palabraActiva != null)
                    await AudioService.Instance.ReproducirPalabraAsync(_palabraActiva.id);
            }
            catch (System.Exception ex) { Debug.LogError($"[RealidadAumentadaPresenter] Error de audio: {ex.Message}"); }
        }

        private void VolverAlMapa()
        {
            NavigationService.IrAMain(PaginaMain.Lecciones);
        }
    }
}
