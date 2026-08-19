using UnityEngine;
using TMPro;

namespace Assets.Scripts.Core.UI.Theme
{
    [CreateAssetMenu(fileName = "UITheme", menuName = "Uchumataqu/UI Theme")]
    public class UITheme : ScriptableObject
    {
        [Header("Colores principales")]
        public Color colorPrimario;
        public Color colorSecundario;

        [Header("Fondos")]
        public Color colorFondo;

        [Header("Texto")]
        public Color colorTexto;

        [Header("Estados")]
        public Color colorCorrecto;
        public Color colorIncorrecto;

        [Header("Botones")]
        public Color colorBoton;
        public Color colorBotonHover;

        [Header("Fuente")]
        public TMP_FontAsset fuentePrincipal;

        [Header("Tamaños de texto")]
        public int tituloGrande = 48;
        public int titulo = 36;
        public int subtitulo = 28;
        public int textoNormal = 24;
        public int textoPequeno = 18;
    }
}