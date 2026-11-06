using TMPro;
using UnityEngine;

namespace Assets.Scripts.Core.UI.Components
{
    [DisallowMultipleComponent]
    public sealed class DiagnosticoMorfologicoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text txtDetalle;

        public void Mostrar(string diagnostico)
        {
            bool visible = !string.IsNullOrWhiteSpace(diagnostico);
            if (txtDetalle != null)
            {
                txtDetalle.richText = false;
                txtDetalle.text = visible ? diagnostico : string.Empty;
            }
            gameObject.SetActive(visible);
        }
    }
}
