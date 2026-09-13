using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.UI.Theme
{
    [CreateAssetMenu(fileName = "UITheme", menuName = "Uchumataqu/UI Theme")]
    public class UITheme : ScriptableObject
    {
        [Header("Fondos")]
        public Color colorFondo = new Color(0.9843137f, 0.9764706f, 0.9529412f, 1f);
        public Color colorSuperficie = Color.white;
        public Color colorSuperficieSuave = new Color(0.97f, 0.95f, 0.89f, 1f);
        public Color colorBorde = new Color(0.82f, 0.84f, 0.86f, 1f);

        [Header("Marca")]
        [Tooltip("Color de marca para progreso, iconos y textos de acento.")]
        public Color colorPrimario = new Color(0.8509804f, 0.4705882f, 0.0431373f, 1f);

        [Header("Tarjetas")]
        public Color colorTarjetaPrincipal = new Color(1f, 0.9921569f, 0.9686275f, 1f);
        public Color colorTarjetaSecundaria = new Color(1f, 0.9568627f, 0.8392157f, 1f);
        public Color colorTarjetaDestacada = new Color(1f, 0.9058824f, 0.6392157f, 1f);

        [Header("Bordes de logros")]
        public Color colorBordeLogroObtenido = new Color(0.1f, 0.65f, 0.31f, 1f);
        public Color colorBordeLogroEnProgreso = new Color(0.85f, 0.47f, 0.04f, 1f);
        public Color colorBordeLogroPendiente = new Color(0.55f, 0.58f, 0.6f, 1f);

        [Header("Colores de texto")]
        [FormerlySerializedAs("colorTexto")]
        public Color colorTextoPrimario = new Color(0.06f, 0.14f, 0.2f, 1f);
        public Color colorTextoSecundario = new Color(0.25f, 0.31f, 0.36f, 1f);
        public Color colorTextoTerciario = new Color(0.02f, 0.55f, 0.56f, 1f);
        [FormerlySerializedAs("colorTextoSobrePrimario")]
        public Color colorTextoSobreOscuro = Color.white;
        public Color colorTituloSobreOscuro = new Color(0.9607843f, 0.6196079f, 0.0431373f, 1f);

        [Header("Estados")]
        public Color colorCorrecto;
        public Color colorIncorrecto;
        public Color colorAdvertencia = new Color(0.85f, 0.60f, 0f, 1f);
        public Color colorInformacion = new Color(0.16f, 0.47f, 0.71f, 1f);


        [Header("Botones")]
        public Color colorBoton;
        public Color colorBotonSuave = new Color(1f, 0.94f, 0.80f, 1f);
        public Color colorScrim = new Color(0.03f, 0.05f, 0.06f, 0.76f);
        public Sprite spriteBoton;
        public Sprite spriteCircular;

        [Header("Contenedores y Sprites")]
        public Sprite spritePanel;
        public Sprite spritePill;
        public Sprite spriteProgreso;
        public Sprite spriteSombraTarjeta;

        [Header("Colores complementarios")]
        public Color colorProgresoFondo = new Color(0.91f, 0.88f, 0.82f, 1f);
        public Color colorLogroDorado = new Color(0.95f, 0.70f, 0.15f, 1f);
        public Color colorBadgeDesbloqueado = new Color(0.88f, 0.96f, 0.90f, 1f);
        public Color colorBadgeProgreso = new Color(1f, 0.93f, 0.80f, 1f);
        public Color colorBadgeBloqueado = new Color(0.92f, 0.90f, 0.86f, 1f);



        [Header("Fuente")]
        public TMP_FontAsset fuentePrincipal;

        [Header("Tamaños de texto")]
        [Min(1f)] public float escalaTextoGlobal = 1.265f;
        public int tituloGrande = 48;
        public int titulo = 36;
        public int subtitulo = 28;
        public int textoNormal = 24;
        public int textoPequeno = 18;

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall -= AplicarCambiosEnEditor;
            UnityEditor.EditorApplication.delayCall += AplicarCambiosEnEditor;
        }

        private void AplicarCambiosEnEditor()
        {
            if (this == null || Application.isPlaying) return;
            UIThemeManager.SetTheme(this);
        }
#endif
    }
}
