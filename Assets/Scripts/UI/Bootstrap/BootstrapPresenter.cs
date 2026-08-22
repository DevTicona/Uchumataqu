using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.UI.Bootstrap
{
    /// <summary>
    /// Presenter para la escena técnica 00_Bootstrap.
    /// Inicializa servicios persistentes, carga el corpus y deriva a Onboarding o Main.
    /// Especificado en Docs/UI/01-escena-bootstrap.md.
    /// </summary>
    public class BootstrapPresenter : MonoBehaviour
    {
        private async void Start()
        {
            Debug.Log("[BootstrapPresenter] Iniciando verificación de arranque...");

            // 1. Inicializar corpus offline
            bool corpusOk = await CorpusRepository.InicializarAsync();
            if (!corpusOk)
            {
                Debug.LogError("[BootstrapPresenter] Error crítico al cargar el corpus.");
            }

            // 2. Cargar perfil y progreso
            bool tienePerfil = PerfilRepository.ExistePerfil();
            ProgresoRepository.ObtenerProgreso();

            // 3. Breve espera para estabilidad visual y enrutamiento
            await Task.Delay(300);

            if (tienePerfil)
            {
                Debug.Log("[BootstrapPresenter] Perfil existente detectado. Navegando a Main.");
                NavigationService.IrAMain();
            }
            else
            {
                Debug.Log("[BootstrapPresenter] Primer inicio detectado. Navegando a Onboarding.");
                NavigationService.IrAOnboarding();
            }
        }
    }
}
