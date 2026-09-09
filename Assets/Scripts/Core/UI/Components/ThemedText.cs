using TMPro;
using UnityEngine;
using Assets.Scripts.Core.UI.Theme;

namespace Assets.Scripts.Core.UI.Components
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ThemedText : MonoBehaviour
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
            Primario,
            Secundario,
            Terciario
        }

        [Header("Configuración")]
        public TipoTexto tipoTexto = TipoTexto.Normal;
        public ColorTexto tipoColor = ColorTexto.Primario;

        [Header("Opcional")]
        public bool usarColorPersonalizado = false;
        public Color colorPersonalizado;

        private TextMeshProUGUI texto;
        private bool estaSuscrito = false;

        private void Awake()
        {
            texto = GetComponent<TextMeshProUGUI>();

            if (texto == null)
            {
                Debug.LogError($"[ThemedText] No se encontró TextMeshProUGUI en {gameObject.name}");
            }
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

            if (UIThemeManager.Theme == null)
            {
                UITheme theme = UnityEditor.AssetDatabase.LoadAssetAtPath<UITheme>(
                    "Assets/Content/UI/UITheme.asset");
                if (theme != null)
                    UIThemeManager.SetTheme(theme);
            }

            AplicarTema();
            UnityEditor.EditorUtility.SetDirty(this);
            if (texto != null)
                UnityEditor.EditorUtility.SetDirty(texto);
            UnityEditor.SceneView.RepaintAll();
        }
        #endif

        private void OnEnable()
        {
            SuscribirseEvento();
            if (UIThemeManager.Theme != null) AplicarTema();
        }

        private void OnDisable()
        {
            DesuscribirseEvento();
        }

        private void SuscribirseEvento()
        {
            if (estaSuscrito) return;

            UIThemeManager.OnThemeChanged += AplicarTema;
            estaSuscrito = true;
        }

        private void DesuscribirseEvento()
        {
            if (!estaSuscrito) return;

            UIThemeManager.OnThemeChanged -= AplicarTema;
            estaSuscrito = false;
        }

        public void AplicarTema()
        {
            if (texto == null) texto = GetComponent<TextMeshProUGUI>();
            if (texto == null) return;

            var theme = UIThemeManager.Theme;
            if (theme == null) return;

            // =========================
            // FUENTE (reactivada segura)
            // =========================
            if (theme.fuentePrincipal != null)
                texto.font = theme.fuentePrincipal;

            // =========================
            // COLOR
            // =========================
            texto.color = usarColorPersonalizado
                ? colorPersonalizado
                : ObtenerColor(theme);

            // =========================
            // TAMAÑO
            // =========================
            switch (tipoTexto)
            {
                case TipoTexto.TituloGrande:
                    texto.fontSize = UIThemeManager.EscalarTexto(theme.tituloGrande);
                    break;

                case TipoTexto.Titulo:
                    texto.fontSize = UIThemeManager.EscalarTexto(theme.titulo);
                    break;

                case TipoTexto.Subtitulo:
                    texto.fontSize = UIThemeManager.EscalarTexto(theme.subtitulo);
                    break;

                case TipoTexto.Normal:
                    texto.fontSize = UIThemeManager.EscalarTexto(theme.textoNormal);
                    break;

                case TipoTexto.Pequeño:
                    texto.fontSize = UIThemeManager.EscalarTexto(theme.textoPequeno);
                    break;

                default: return;
            }

            // =========================
            // FORZAR ACTUALIZACIÓN
            // =========================
            texto.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
        }

        private Color ObtenerColor(UITheme theme)
        {
            switch (tipoColor)
            {
                case ColorTexto.Secundario:
                    return theme.colorTextoSecundario;
                case ColorTexto.Terciario:
                    return theme.colorTextoTerciario;
                default:
                    return theme.colorTextoPrimario;
            }
        }
    }
}
