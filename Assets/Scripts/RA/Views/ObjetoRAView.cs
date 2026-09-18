using System;
using UnityEngine;

namespace Assets.Scripts.RA
{
    /// <summary>
    /// Componente interactivo asignado a cada modelo 3D en la escena de Realidad Aumentada.
    /// Al tocar el objeto notifica su identificador al Presentador.
    /// Especificado en Docs/Specifications/03-modulo-ra.md (Sección 6).
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.RA", sourceAssembly: "Assembly-CSharp", sourceClassName: "InteractorObjetoRA")]
    public class ObjetoRAView : MonoBehaviour
    {
        [SerializeField] private string palabraId;

        public event Action<string> OnObjetoTocado;

        public void Configurar(string palabraId)
        {
            this.palabraId = palabraId;
        }

        public void TocarObjeto()
        {
            if (string.IsNullOrEmpty(palabraId)) return;

            OnObjetoTocado?.Invoke(palabraId);
        }

        private void OnMouseDown()
        {
            TocarObjeto();
        }
    }
}
