using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Sistema;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.UI.Main
{
    /// <summary>
    /// Presenter de la escena 02_Main.
    /// Coordina las 5 páginas internas, la persistencia y el botón atrás de Android.
    /// Especificado en Docs/UI/03-escena-main.md y 07-navegacion-y-estado-ui.md.
    /// </summary>
    public class MainPresenter : MonoBehaviour
    {
        [SerializeField] private MainView view;
        [SerializeField] private NavigationBackHandler backHandler;

        private PaginaMain _paginaActual = PaginaMain.Inicio;

        private void Start()
        {
            if (view == null) view = FindAnyObjectByType<MainView>();

            if (view != null && view.BottomNav != null)
            {
                view.BottomNav.OnTabSelected += CambiarPagina;
            }

            ConfigurarBackHandler();

            // Abrir en la página solicitada por el contexto de navegación
            CambiarPagina(ContextoNavegacion.PaginaDestino);
        }

        private void OnDestroy()
        {
            if (view != null && view.BottomNav != null)
            {
                view.BottomNav.OnTabSelected -= CambiarPagina;
            }
        }

        public void CambiarPagina(PaginaMain pagina)
        {
            _paginaActual = pagina;
            if (view != null)
            {
                view.MostrarPagina(pagina);
            }
        }

        private void ConfigurarBackHandler()
        {
            if (backHandler == null) backHandler = FindAnyObjectByType<NavigationBackHandler>();

            if (backHandler != null)
            {
                backHandler.Configurar(OnBackPresionado);
            }
        }

        private void OnBackPresionado()
        {
            if (_paginaActual != PaginaMain.Inicio)
            {
                // Desde cualquier página distinta a Inicio, volver a Inicio
                CambiarPagina(PaginaMain.Inicio);
            }
            else
            {
                // Desde Inicio, mostrar diálogo de confirmación de salida
                if (view != null)
                    view.MostrarDialogoSalida(true);
            }
        }
    }
}
