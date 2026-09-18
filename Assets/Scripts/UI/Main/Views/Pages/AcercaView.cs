
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista de la página "Acerca de" dentro de la escena 02_Main.
    /// Especificado en Docs/instrucciones_implementar_page_acerca_uchumataqu.md.
    /// </summary>
    public class AcercaView : MonoBehaviour
    {
        [Header("Referencias de UI")]
        [SerializeField] private TMP_Text txtVersionApp;

        public void SetVersion(string version)
        {
            if (txtVersionApp != null)
                txtVersionApp.text = $"Versión {version}";
        }

    }
}
