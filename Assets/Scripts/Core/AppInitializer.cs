using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Assets.Scripts.Core.UI.Theme;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.Assets;
using Assets.Scripts.Core;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.Core.Sistema
{
    public class AppInitializer : MonoBehaviour
    {
        [SerializeField] private UIThemeModel temaUI;
        private Task _tareaInicializacion;

        public Task InicializarAsync()
        {
            return _tareaInicializacion ??= InicializarInternoAsync();
        }

        public void Reiniciar()
        {
            _tareaInicializacion = null;
        }

        private async Task InicializarInternoAsync()
        {
            Debug.Log("[AppInitializer] Iniciando aplicación...");

            var config = AppConfigRepository.Obtener();
            if (config == null)
                throw new InvalidOperationException("No se encontró AppConfig en Resources.");

            Debug.unityLogger.logEnabled = config.enableDebugLogs;
            OrientacionPantallaService.ConfigurarInicial();
            AudioListener.volume = ConfiguracionRepository.Obtener().volumen_general;
            AudioService.Instance.ConfigurarVolumen(config.defaultWordVolume);
            // Se conserva el handle hasta consultar su estado. InitializeAsync() usa
            // autoReleaseHandle=true por defecto y en dispositivos puede invalidarlo
            // justo antes de que continúe este await.
            var addressables = Addressables.InitializeAsync(false);
            try
            {
                await addressables.Task;
                if (addressables.Status != AsyncOperationStatus.Succeeded)
                    throw new InvalidOperationException("No se pudo inicializar el catálogo Addressables.");
            }
            finally
            {
                if (addressables.IsValid())
                    Addressables.Release(addressables);
            }
            if (temaUI == null) throw new InvalidOperationException("Falta asignar UITheme en AppInitializer.");
            UIThemeController.SetTheme(temaUI);
            Debug.Log("[AppInitializer] Inicialización completa.");
        }
    }
}
