using UnityEngine;
using TMPro;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista de la pestaña de Acerca de / Créditos en 02_Main.
    /// Especificado en Docs/UI/08-perfil-logros-ajustes-acerca.md §5.
    /// </summary>
    public class AcercaView : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TMP_Text txtTitulo;
        [SerializeField] private TMP_Text txtCreditosComunidad;
        [SerializeField] private TMP_Text txtCreditosInstitucionales;
        [SerializeField] private TMP_Text txtVersionApp;

        private void OnEnable()
        {
            if (txtTitulo != null)
                txtTitulo.text = "Acerca de Uchumataqu";

            if (txtCreditosComunidad != null)
                txtCreditosComunidad.text =
                    "Aplicación educativa dedicada a la lengua uchumataqu y a la Comunidad Irohito Urus. " +
                    "Los datos de perfil y progreso se guardan localmente en el dispositivo.";

            if (txtCreditosInstitucionales != null)
                txtCreditosInstitucionales.text =
                    "Proyecto de Grado con apoyo documental de FUNPROEIB ANDES. " +
                    "Los créditos nominales y las fuentes lingüísticas se completarán tras su validación.";

            if (txtVersionApp != null)
                txtVersionApp.text = $"Versión {Application.version} — 100% Offline";
        }
    }
}
