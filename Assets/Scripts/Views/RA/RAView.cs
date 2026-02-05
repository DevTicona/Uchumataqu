using UnityEngine;

namespace Assets.Scripts.Views.RA
{
    /// <summary>
    /// Vista de Realidad Aumentada.
    /// Actualmente solo contiene la lógica de AR que se maneja en la escena.
    /// La navegación se maneja en RAPresenter.
    /// </summary>
    public class RAView : MonoBehaviour
    {
        // La lógica de AR se maneja directamente en la escena
        // mediante los componentes de AR Foundation
        
        private void Awake()
        {
            Debug.Log("[RAView] Vista de Realidad Aumentada inicializada");
        }
    }
}
