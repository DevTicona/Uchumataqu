using TMPro;
using UnityEngine;
using Assets.Scripts.Core.UI.Theme;

namespace Assets.Scripts.Core.UI.Components
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.Core.UI.Components", sourceAssembly: "Assembly-CSharp", sourceClassName: "ThemedText")]
    public class ThemedTextView : MonoBehaviour
    {
        public enum TipoTexto
        {
            TituloGrande,
            Titulo,
            Subtitulo,
            Normal,
            Pequeño
        }

        public enum ColorTexto
        {
            Primario = 0,
            Secundario = 1,
            Terciario = 2,
            SobreColor = 3,
            AcentoPrimario = 4,
            Exito = 6,
            Advertencia = 7,
            Error = 9,
            Informacion = 10
        }

        [Header("Configuración")]
        public TipoTexto tipoTexto = TipoTexto.Normal;
        public ColorTexto tipoColor = ColorTexto.Primario;

        [Header("Opcional")]
        public bool usarColorPersonalizado;
        public Color colorPersonalizado;

        private TextMeshProUGUI texto;
        private bool estaSuscrito;

        private void Awake()
        {
            texto = GetComponent<TextMeshProUGUI>();
            if (texto == null)
                Debug.LogError($"[ThemedText] No se encontró TextMeshProUGUI en {gameObject.name}");
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall -= AplicarCambiosEnEditor;
            UnityEditor.EditorApplication.delayCall += AplicarCambiosEnEditor;
        }

        private void AplicarCambiosEnEditor()
        {
            if (this == null) return;

            if (UIThemeController.Theme == null)
            {
                UIThemeModel theme = UnityEditor.AssetDatabase.LoadAssetAtPath<UIThemeModel>(
                    "Assets/Content/UI/UITheme.asset");
                if (theme != null) UIThemeController.SetTheme(theme);
            }

            AplicarTema();
            UnityEditor.EditorUtility.SetDirty(this);
            if (texto != null) UnityEditor.EditorUtility.SetDirty(texto);
            UnityEditor.SceneView.RepaintAll();
        }
#endif

        private void OnEnable()
        {
            SuscribirseEvento();
            if (UIThemeController.Theme != null) AplicarTema();
        }

        private void OnDisable() => DesuscribirseEvento();

        private void SuscribirseEvento()
        {
            if (estaSuscrito) return;
            UIThemeController.OnThemeChanged += AplicarTema;
            estaSuscrito = true;
        }

        private void DesuscribirseEvento()
        {
            if (!estaSuscrito) return;
            UIThemeController.OnThemeChanged -= AplicarTema;
            estaSuscrito = false;
        }

        public void AplicarTema()
        {
            if (texto == null) texto = GetComponent<TextMeshProUGUI>();
            if (texto == null) return;

            UIThemeModel theme = UIThemeController.Theme;
            if (theme == null) return;

            // The font asset, style and overflow remain static prefab settings.
            // The assigned UITheme role controls only color and semantic text size.
            texto.color = usarColorPersonalizado ? colorPersonalizado : ObtenerColor(theme);
            texto.fontSize = UIThemeController.EscalarTexto(ObtenerTamano(theme));
        }

        private float ObtenerTamano(UIThemeModel theme)
        {
            switch (tipoTexto)
            {
                case TipoTexto.TituloGrande: return theme.tituloGrande;
                case TipoTexto.Titulo: return theme.titulo;
                case TipoTexto.Subtitulo: return theme.subtitulo;
                case TipoTexto.Pequeño: return theme.textoPequeno;
                default: return theme.textoNormal;
            }
        }

        private Color ObtenerColor(UIThemeModel theme)
        {
            switch (tipoColor)
            {
                case ColorTexto.Secundario: return theme.colorTextoSecundario;
                case ColorTexto.Terciario: return theme.colorTextoTerciario;
                case ColorTexto.SobreColor: return theme.colorTextoSobreColor;
                case ColorTexto.AcentoPrimario: return theme.colorPrimario;
                case ColorTexto.Exito: return theme.colorExito;
                case ColorTexto.Advertencia: return theme.colorAdvertencia;
                case ColorTexto.Error: return theme.colorError;
                case ColorTexto.Informacion: return theme.colorInformacion;
                default: return theme.colorTextoPrimario;
            }
        }
    }
}
