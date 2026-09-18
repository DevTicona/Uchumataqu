using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.UI.Theme
{
    [CreateAssetMenu(fileName = "UITheme", menuName = "Uchumataqu/UI Theme")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.Core.UI.Theme", sourceAssembly: "Assembly-CSharp", sourceClassName: "UITheme")]
    public class UIThemeModel : ScriptableObject
    {

        [Header("Marca")]
        public Color colorPrimario = new Color(0.85098039f, 0.47058824f, 0f, 1f);

        [Header("Superficies")]
        public Color colorSuperficie = new Color(1f, 0.99215686f, 0.96862745f, 1f);
        public Color colorSuperficieSuave = new Color(1f, 0.95686275f, 0.83921569f, 1f);
        public Color colorBorde = new Color(0.86666667f, 0.83137255f, 0.76470588f, 1f);

        [Header("Texto")]
        [FormerlySerializedAs("colorTexto")]
        public Color colorTextoPrimario = new Color(0.09019608f, 0.1372549f, 0.20392157f, 1f);
        public Color colorTextoSecundario = new Color(0.34901961f, 0.38823529f, 0.43137255f, 1f);
        public Color colorTextoTerciario = new Color(0.54117647f, 0.56078431f, 0.58823529f, 1f);
        [FormerlySerializedAs("colorTextoSobreOscuro")]
        [FormerlySerializedAs("colorTextoSobrePrimario")]
        public Color colorTextoSobreColor = new Color(1f, 1f, 1f, 1f);

        [Header("Estados")]
        [FormerlySerializedAs("colorCorrecto")]
        public Color colorExito = new Color(0.12156863f, 0.68235294f, 0.35686275f, 1f);
        [FormerlySerializedAs("colorIncorrecto")]
        public Color colorError = new Color(0.8627451f, 0.22745098f, 0.22745098f, 1f);
        public Color colorAdvertencia = new Color(0.89803922f, 0.62745098f, 0f, 1f);
        public Color colorInformacion = new Color(0.14509804f, 0.5372549f, 0.78823529f, 1f);

        [Header("Controles")]
        [FormerlySerializedAs("colorBotonSuave")]
        public Color colorControlSuave = new Color(1f, 0.94117647f, 0.8f, 1f);
        public Color colorDeshabilitado = new Color(0.85098039f, 0.85098039f, 0.85098039f, 1f);
        public Color colorScrim = new Color(0f, 0f, 0f, 0.6f);

        [Header("Progreso")]
        public Color colorProgresoFondo = new Color(0.89411765f, 0.88235294f, 0.85098039f, 1f);

        [Header("Fuente")]
        public TMP_FontAsset fuentePrincipal;

        [Header("Tamaños de texto")]
        [Min(1f)] public float escalaTextoGlobal = 1.265f;
        public int tituloGrande = 48;
        public int titulo = 36;
        public int subtitulo = 28;
        public int textoNormal = 24;
        public int textoPequeno = 18;

    }
}
