using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts.Views;
using Assets.Scripts.Core;

namespace Assets.Scripts.Presenters
{
    /// <summary>
    /// Orquesta la lógica del menú principal.
    /// Maneja navegación y funciones del sistema.
    /// </summary>
    public class MenuPrincipalPresenter : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private MenuPrincipalView menuPrincipalView;

        [Header("Sistema")]
        [SerializeField] private NavigationBackHandler backHandler;

        private void Start()
        {
            if (menuPrincipalView == null)
            {
                Debug.LogError("[MenuPrincipalPresenter] MenuPrincipalView no asignada.");
                enabled = false;
                return;
            }

            SuscribirseEventos();
            ConfigurarBackHandler();
        }

        private void OnDestroy()
        {
            if (menuPrincipalView == null) return;

            menuPrincipalView.OnLeccionesPressed -= IrALecciones;
            menuPrincipalView.OnExplorarRAPressed -= IrARealidadAumentada;
            menuPrincipalView.OnSalirPressed -= menuPrincipalView.SolicitarConfirmacionSalida;
        }

        // -------------------------------------------------
        // EVENTOS
        // -------------------------------------------------

        private void SuscribirseEventos()
        {
            menuPrincipalView.OnLeccionesPressed += IrALecciones;
            menuPrincipalView.OnExplorarRAPressed += IrARealidadAumentada;
            menuPrincipalView.OnSalirPressed += menuPrincipalView.SolicitarConfirmacionSalida;
        }

        private void IrALecciones()
        {
            SceneManager.LoadScene("MenuLecciones");
        }

        private void IrARealidadAumentada()
        {
            SceneManager.LoadScene("RealidadAumentada");
        }

        // -------------------------------------------------
        // BACK HANDLER
        // -------------------------------------------------

        private void ConfigurarBackHandler()
        {
            if (backHandler != null)
                backHandler.Configurar(menuPrincipalView.SolicitarConfirmacionSalida);
        }
    }
}
