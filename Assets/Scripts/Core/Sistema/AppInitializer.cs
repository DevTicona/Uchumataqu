using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Assets.Scripts.Core.UI.Theme;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core; // necesario para AppConfig
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.Core.Sistema
{
    public class AppInitializer : MonoBehaviour
    {
        private Task _tareaInicializacion;

        public Task InicializarAsync()
        {
            return _tareaInicializacion ??= InicializarInternoAsync();
        }

        public void Reiniciar()
        {
            _tareaInicializacion = null;
        }

        private static async Task InicializarInternoAsync()
        {
            Debug.Log("[AppInitializer] Iniciando aplicación...");

            var config = AppConfig.Instance;
            if (config == null)
                throw new InvalidOperationException("No se encontró AppConfig en Resources.");

            Debug.unityLogger.logEnabled = config.enableDebugLogs;
            GestorOrientacionPantalla.ConfigurarInicial();
            AudioListener.volume = ConfiguracionRepository.Obtener().volumen_general;
            _ = AudioService.Instance;
            var addressables = Addressables.InitializeAsync();
            await addressables.Task;
            if (addressables.Status != AsyncOperationStatus.Succeeded)
                throw new InvalidOperationException("No se pudo inicializar el catálogo Addressables.");
            await UIThemeManager.CargarTemaAsync(ContentAddress.TemaUi);
            Debug.Log("[AppInitializer] Inicialización completa.");
        }
    }
}
