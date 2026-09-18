using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista para el encabezado / separador de sección en el mapa de lecciones.
    /// </summary>
    public class SeparadorSeccionView : MonoBehaviour
    {
        [Header("Elementos de UI")]
        [SerializeField] private TextMeshProUGUI textoRegion;
        [SerializeField] private Image separadorSuperior;
        [SerializeField] private Image separadorInferior;

        public TextMeshProUGUI TextoRegion => textoRegion;
        public Image SeparadorSuperior => separadorSuperior;
        public Image SeparadorInferior => separadorInferior;

        public void Configurar(string textoFormateado)
        {
            if (textoRegion == null)
                textoRegion = GetComponentInChildren<TextMeshProUGUI>(true);

            if (textoRegion != null)
            {
                textoRegion.text = textoFormateado;
                textoRegion.alignment = TextAlignmentOptions.Center;
                textoRegion.textWrappingMode = TextWrappingModes.NoWrap;
                textoRegion.overflowMode = TextOverflowModes.Overflow;
            }
        }

        public void ConfigurarColorLineas(Color colorSeparador)
        {
            if (separadorSuperior != null)
                separadorSuperior.color = colorSeparador;

            if (separadorInferior != null)
                separadorInferior.color = colorSeparador;
        }
    }
}
