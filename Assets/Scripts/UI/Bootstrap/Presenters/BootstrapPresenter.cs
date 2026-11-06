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
    /// Especificado en Docs/Specifications/06-guia-arquitectura-proyecto.md.
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
            view.MostrarProgreso(0f);
            view.MostrarCarga("Preparando recursos locales...");
            // Permite dibujar Bootstrap antes del trabajo de inicialización.
            await Awaitable.NextFrameAsync();
            if (this == null || !isActiveAndEnabled) return;
            Debug.Log($"[ARRANQUE] Primer cuadro de Bootstrap: {Time.realtimeSinceStartup:F2}s.");
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
            view.MostrarProgreso(0f);
            view.MostrarCarga("Preparando recursos locales...");

            try
            {
                float inicio = Time.realtimeSinceStartup;
                await appInitializer.InicializarAsync();
                view.MostrarProgreso(0.5f);
                view.MostrarCarga("Cargando contenido educativo...");
                bool corpusOk = await CorpusRepository.InicializarAsync();
                if (!corpusOk)
                    throw new System.InvalidOperationException("El corpus local no pudo inicializarse.");

                bool tienePerfil = GestorPerfil.ExistePerfil();
                view.MostrarProgreso(0.9f);
                ProgresoRepository.ObtenerProgreso();

                view.MostrarProgreso(1f);
                view.MostrarCarga("Todo listo");
                Debug.Log($"[ARRANQUE] Servicios y corpus: {Time.realtimeSinceStartup - inicio:F2}s.");
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
