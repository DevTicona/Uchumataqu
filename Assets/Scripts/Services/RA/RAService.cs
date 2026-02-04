using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Scripts.Services.RA
{
    /// <summary>
    /// Servicio responsable de gestionar recursos RA mediante Addressables.
    /// </summary>
    public class RAService
    {
        // Addressable key del personaje base (infraestructura)
        private const string PERSONAJE_BASE_KEY = "personaje_base";

        public void InstanciarPersonajeBase(
            Vector3 posicion,
            Quaternion rotacion)
        {
            Addressables.InstantiateAsync(
                PERSONAJE_BASE_KEY,
                posicion,
                rotacion
            ).Completed += OnPersonajeInstanciado;
        }

        private void OnPersonajeInstanciado(
            AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("❌ Error al instanciar personaje RA");
            }
        }
    }
}
