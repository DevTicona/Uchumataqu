using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Presenta la informacion comunitaria, institucional y tecnica de la aplicacion.
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
            {
                txtCreditosComunidad.text =
                    "Aplicacion educativa dedicada a la lengua uchumataqu y a la Comunidad Irohito Urus. " +
                    "Apoya la preservacion y el aprendizaje de la lengua.\n\n" +
                    "El perfil y el progreso se guardan solamente en este dispositivo.";
            }

            if (txtCreditosInstitucionales != null)
            {
                txtCreditosInstitucionales.text =
                    "Institucion: FUNPROEIB Andes\n" +
                    "Comunidad: Irohito Urus\n" +
                    "Modalidad: Proyecto de grado\n" +
                    "Fuentes: corpus linguistico local validado para el proyecto\n" +
                    "Tecnologia: Unity 6, C#, Addressables y AR Foundation";
            }

            if (txtVersionApp != null)
                txtVersionApp.text = $"Version {Application.version} - 100% offline";
        }
    }
}
