using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.UI.Theme
{
    [CreateAssetMenu(fileName = "UITheme", menuName = "Uchumataqu/UI Theme")]
    public class UITheme : ScriptableObject
    {
        [Header("Fondos")]
        public Color colorFondo;

        [Header("Colores de texto")]
        [FormerlySerializedAs("colorTexto")]
        public Color colorTextoPrimario = new Color(0.06f, 0.14f, 0.2f, 1f);
        public Color colorTextoSecundario = new Color(0.25f, 0.31f, 0.36f, 1f);
        public Color colorTextoTerciario = new Color(0.02f, 0.55f, 0.56f, 1f);

        [Header("Estados")]
        public Color colorCorrecto;
        public Color colorIncorrecto;

        [Header("Botones")]
        public Color colorBoton;
        public Sprite spriteBoton;
        public Sprite spriteCircular;

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
