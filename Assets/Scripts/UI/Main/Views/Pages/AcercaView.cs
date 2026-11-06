
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista de la página "Acerca de" dentro de la escena 02_Main.
    /// El contenido y el diseño se mantienen en la escena; la versión procede de PlayerSettings.
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
