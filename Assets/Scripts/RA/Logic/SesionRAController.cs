using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Assets.Scripts.RA
{
    public sealed class SesionRAController : MonoBehaviour
    {
        [SerializeField] private Behaviour[] componentesAR;
        private XRManagerSettings manager;
        private bool propietario;
        public string ErrorInicio { get; private set; }

        public void Configurar(Behaviour[] componentes) => componentesAR = componentes;

        private IEnumerator Start()
        {
            // XR requiere que Start y la inicialización gráfica hayan terminado.
            yield return null;
            manager = XRGeneralSettings.Instance != null ? XRGeneralSettings.Instance.Manager : null;
            if (manager == null)
            {
                ErrorInicio = "No se ha configurado el proveedor de realidad aumentada.";
                yield break;
            }
            if (manager.activeLoader == null)
            {
                propietario = true;
                yield return manager.InitializeLoader();
            }
            if (manager.activeLoader == null)
            {
                ErrorInicio = "No se pudo iniciar la realidad aumentada en este dispositivo.";
                yield break;
            }
            if (propietario) manager.StartSubsystems();
            foreach (var componente in componentesAR)
                if (componente != null) componente.enabled = true;
        }

        private void OnDestroy()
        {
            foreach (var componente in componentesAR)
                if (componente != null) componente.enabled = false;
            if (propietario && manager != null && manager.activeLoader != null)
                manager.DeinitializeLoader();
        }
    }
}
