using UnityEngine;
using Assets.Scripts.Models;

namespace Assets.Scripts.RealidadAumentada
{
    /// <summary>
    /// Responsable de instanciar y limpiar el contenido de Realidad Aumentada
    /// según el tipo de experiencia RA.
    /// </summary>
    public class RAContenidoLoader : MonoBehaviour
    {
        [Header("RA Root")]
        [SerializeField] private Transform raInstanciaRoot;

        private GameObject contenidoActual;

        /// <summary>
        /// Carga el contenido RA correspondiente al tipo indicado.
        /// </summary>
        public void CargarContenidoRA(TipoRA tipoRa, GameObject prefabRa)
        {
            LimpiarContenido();

            if (prefabRa == null)
            {
                Debug.LogWarning("[RAContenidoLoader] Prefab RA nulo.");
                return;
            }

            if (raInstanciaRoot == null)
            {
                Debug.LogError("[RAContenidoLoader] raInstanciaRoot no asignado.");
                return;
            }

            contenidoActual = Instantiate(prefabRa, raInstanciaRoot);
        }

        /// <summary>
        /// Elimina el contenido RA actual.
        /// </summary>
        public void LimpiarContenido()
        {
            if (contenidoActual != null)
            {
                Destroy(contenidoActual);
                contenidoActual = null;
            }
        }
    }
}
