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
                txtTitulo.text = "Uchumataqu — Revitalización Lingüística";

            if (txtCreditosComunidad != null)
                txtCreditosComunidad.text = "Desarrollado en memoria y homenaje a la comunidad Uru de Iruhito y Ch'imu, preservando su lengua ancestral y sabiduría milenaria.";

            if (txtCreditosInstitucionales != null)
                txtCreditosInstitucionales.text = "Basado en los estudios y registros lingüísticos de FUNPROEIB Andes y el Ministerio de Educación de Bolivia.";

            if (txtVersionApp != null)
                txtVersionApp.text = $"Versión {Application.version} — 100% Offline";
        }
    }
}
