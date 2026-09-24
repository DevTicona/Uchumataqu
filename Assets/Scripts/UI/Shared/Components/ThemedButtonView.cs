using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Core.UI.Theme;

namespace Assets.Scripts.Core.UI.Components
{
    [ExecuteAlways]
    [RequireComponent(typeof(Button))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.Core.UI.Components", sourceAssembly: "Assembly-CSharp", sourceClassName: "ThemedButton")]
    public class ThemedButtonView : MonoBehaviour
    {
        public enum EstiloBoton
        {
            Primario = 0,
            Suave = 1,
            Error = 3,
            Superficie = 4
        }

        public enum EstadoBoton { Normal, Seleccionado, Correcto, Incorrecto }

        [SerializeField] private EstiloBoton estilo = EstiloBoton.Primario;

        private EstadoBoton _estado;

        public void MostrarEstado(EstadoBoton estado)
        {
            _estado = estado;
            AplicarTema();
        }

        public EstiloBoton Estilo
        {
            get => estilo;
            set
            {
                estilo = value;
                AplicarTema();
            }
        }

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            UIThemeController.OnThemeChanged += AplicarTema;

            // IMPORTANTE: aplicar inmediatamente si ya existe theme
            if (UIThemeController.Theme != null)
            {
                AplicarTema();
            }
        }

        private void OnDisable()
        {
            UIThemeController.OnThemeChanged -= AplicarTema;
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
            AplicarTema();
            UnityEditor.EditorUtility.SetDirty(this);
            if (button != null && button.targetGraphic != null)
                UnityEditor.EditorUtility.SetDirty(button.targetGraphic);
            UnityEditor.SceneView.RepaintAll();
        }
#endif

        public void AplicarTema()
        {
            var theme = UIThemeController.Theme;
            if (theme == null) return;

            if (button == null) button = GetComponent<Button>();
            if (button == null) return;

            button.transition = Selectable.Transition.ColorTint;

            Color normal = ObtenerColorNormal(theme);
            Color resaltado = Color.Lerp(normal, Color.white, 0.10f);
            Color presionado = Color.Lerp(normal, Color.black, 0.12f);

            var colors = button.colors;

            colors.normalColor = normal;
            colors.highlightedColor = resaltado;
            colors.pressedColor = presionado;
            colors.selectedColor = resaltado;
            colors.disabledColor = _estado == EstadoBoton.Correcto ? normal : theme.colorDeshabilitado;

            button.colors = colors;

            // Fuerza actualización visual
            if (button.targetGraphic != null)
            {
                // El color base debe permanecer blanco: Button.ColorTint ya aplica el
                // estilo en CanvasRenderer. Teñir ambos multiplica el color y oscurece
                // un primario naranja hasta que parece rojo.
                button.targetGraphic.color = Color.white;
                button.targetGraphic.SetAllDirty();
                Color colorActual = button.interactable ? normal : colors.disabledColor;
                button.targetGraphic.CrossFadeColor(colorActual, 0f, true, true);
            }
        }

        private Color ObtenerColorNormal(UIThemeModel theme)
        {
            switch (_estado)
            {
                case EstadoBoton.Seleccionado: return Color.Lerp(theme.colorSuperficie, theme.colorPrimario, 0.18f);
                case EstadoBoton.Correcto: return Color.Lerp(theme.colorSuperficie, theme.colorExito, 0.18f);
                case EstadoBoton.Incorrecto: return Color.Lerp(theme.colorSuperficie, theme.colorError, 0.18f);
            }
            switch (estilo)
            {
                case EstiloBoton.Superficie:
                    return theme.colorSuperficie;
                case EstiloBoton.Suave:
                    return theme.colorControlSuave;
                case EstiloBoton.Error:
                    return theme.colorError;
                default:
                    return theme.colorPrimario;
            }
        }
    }
}
