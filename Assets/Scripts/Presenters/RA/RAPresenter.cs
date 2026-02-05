using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts.Core;
using Assets.Scripts.Views.UI;

namespace Assets.Scripts.Presenters.RA
{
    /// <summary>
    /// Presenter de la escena de Realidad Aumentada.
    /// Maneja la navegación hacia atrás al menú principal.
    /// </summary>
    public class RAPresenter : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private HeaderView headerView;

        [Header("Navegación")]
        [SerializeField] private NavigationBackHandler backHandler;

        private void Start()
        {
            if (headerView == null)
            {
                Debug.LogError("[RAPresenter] HeaderView no asignado en el Inspector.");
                enabled = false;
                return;
            }

            ConfigurarHeader();
            ConfigurarBackHandler();
        }

        // -------------------------------------------------
        // CONFIGURACIÓN
        // -------------------------------------------------

        private void ConfigurarHeader()
        {
            headerView.BindBotonAtras(VolverAlMenuPrincipal);
            headerView.SetTitulo("Realidad Aumentada");
            headerView.SetSubtitulo("Explora en AR");
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
    }
}
