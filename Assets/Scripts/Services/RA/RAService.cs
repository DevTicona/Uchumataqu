using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Scripts.Services.RA
{
    /// <summary>
    /// Servicio responsable de la gestión de recursos
    /// de Realidad Aumentada mediante Addressables.
    /// </summary>
    public class RAService
    {
        // Addressable key del personaje base
        private const string PERSONAJE_BASE_KEY = "personaje_base";

        private bool addressablesInicializado = false;

        public RAService()
        {
            InicializarAddressables();
        }

        // -------------------------------------------------
        // INICIALIZACIÓN
        // -------------------------------------------------

        private void InicializarAddressables()
        {
            Addressables.InitializeAsync().Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    addressablesInicializado = true;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    Debug.Log("Addressables inicializado correctamente");
#endif
                }
                else
                {
                    Debug.LogError("Error al inicializar Addressables");
                }
            };
        }

        // -------------------------------------------------
        // INSTANCIACIÓN
        // -------------------------------------------------

        /// <summary>
        /// Instancia el personaje base en la escena RA.
        /// </summary>
        public void InstanciarPersonajeBase(
            Vector3 posicion,
            Quaternion rotacion,
            System.Action<GameObject> onExito = null)
        {
            if (!addressablesInicializado)
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                Debug.LogWarning("Addressables aún no está listo");
#endif
                return;
            }

            Addressables.InstantiateAsync(
                PERSONAJE_BASE_KEY,
                posicion,
                rotacion
            ).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    Debug.Log("Personaje RA instanciado");
#endif
                    onExito?.Invoke(handle.Result);
                }
                else
                {
                    Debug.LogError("Error al instanciar personaje RA");
                }
            };
        }
    }
}
