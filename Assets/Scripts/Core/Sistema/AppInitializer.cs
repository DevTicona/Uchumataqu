using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Assets.Scripts.Core.UI.Theme;
using Assets.Scripts.Core; // necesario para AppConfig

namespace Assets.Scripts.Core.Sistema
{
    public class AppInitializer : MonoBehaviour
    {
        private static bool inicializado = false;

        private async void Awake()
        {
            if (inicializado) return;

            inicializado = true;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[AppInitializer] Iniciando aplicación...");

            await InicializarAsync();

            Debug.Log("[AppInitializer] Inicialización completa.");
        }

        private async Task InicializarAsync()
        {
            // 1. Addressables
            await InicializarAddressables();

            // 2. Configuración global
            InicializarConfiguracion();

            // 3. Sistema de UI (Theme)
            InicializarUI();

            // 4. Sistema de plataforma (Android, orientación, etc.)
            InicializarSistema();
        }

        private async Task InicializarAddressables()
        {
            Debug.Log("[AppInitializer] Inicializando Addressables...");
            await Addressables.InitializeAsync().Task;
        }

        private void InicializarConfiguracion()
        {
            Debug.Log("[AppInitializer] Cargando configuración...");

            var config = AppConfig.Instance;

            if (config == null)
            {
                Debug.LogError("[AppInitializer] No se encontró AppConfig.");
                return;
            }

            // Ejemplo de uso real de configuración
            Debug.unityLogger.logEnabled = config.enableDebugLogs;
        }

        private void InicializarUI()
        {
            Debug.Log("[AppInitializer] Cargando tema...");
            UIThemeManager.CargarTema("UITheme");
        }

        private void InicializarSistema()
        {
            Debug.Log("[AppInitializer] Configurando sistema...");

            // Orientación inicial (menú)
            GestorOrientacionPantalla.ConfigurarInicial();

            // Permisos (ej: micrófono)
            AndroidMicrophonePermission.SolicitarPermiso();
        }
    }
}