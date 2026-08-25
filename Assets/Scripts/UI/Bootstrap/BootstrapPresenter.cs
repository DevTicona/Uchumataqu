using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Persistencia;
using Assets.Scripts.Core.Sistema;

namespace Assets.Scripts.UI.Bootstrap
{
    /// <summary>
    /// Presenter para la escena técnica 00_Bootstrap.
    /// Inicializa servicios persistentes, carga el corpus y deriva a Onboarding o Main.
    /// Especificado en Docs/UI/01-escena-bootstrap.md.
    /// </summary>
    public class BootstrapPresenter : MonoBehaviour
    {
        [SerializeField] private BootstrapView view;
        [SerializeField] private AppInitializer appInitializer;

        private bool _inicializando;

        private void Awake()
        {
            if (view == null || appInitializer == null)
            {
                Debug.LogError("[BootstrapPresenter] Faltan referencias de composición.");
                enabled = false;
                return;
            }

            view.OnReintentar += Reintentar;
            view.OnCerrar += Cerrar;
        }

        private async void Start()
        {
            await InicializarYNavegarAsync();
        }

        private void OnDestroy()
        {
            if (view == null) return;
            view.OnReintentar -= Reintentar;
            view.OnCerrar -= Cerrar;
        }

        private async Task InicializarYNavegarAsync()
        {
            if (_inicializando) return;
            _inicializando = true;
            view.MostrarCarga("Preparando recursos locales...");

            try
            {
                await appInitializer.InicializarAsync();
                view.MostrarCarga("Cargando contenido educativo...");
                bool corpusOk = await CorpusRepository.InicializarAsync();
                if (!corpusOk)
                    throw new System.InvalidOperationException("El corpus local no pudo inicializarse.");

                bool tienePerfil = PerfilRepository.ExistePerfil();
                ProgresoRepository.ObtenerProgreso();
                _ = AudioService.Instance;

                view.MostrarCarga("Todo listo");
                await Task.Delay(200);
                if (this == null) return;

                if (tienePerfil) NavigationService.IrAMain();
                else NavigationService.IrAOnboarding();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BootstrapPresenter] Error crítico: {ex}");
                view.MostrarError("No se pudo iniciar la aplicación.\nFaltan recursos locales necesarios.");
            }
            finally
            {
                _inicializando = false;
            }
        }

        private void Reintentar()
        {
            CorpusRepository.LimpiarCache();
            appInitializer.Reiniciar();
            _ = InicializarYNavegarAsync();
        }

        private static void Cerrar() => Application.Quit();
    }
}
