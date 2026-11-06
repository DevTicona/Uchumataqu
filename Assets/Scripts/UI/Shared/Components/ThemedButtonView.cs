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

        public enum EstadoBoton { Normal, Seleccionado, Correcto, Incorrecto, Casi }

        [SerializeField] private EstiloBoton estilo = EstiloBoton.Primario;
        [SerializeField] private ThemedGraphicView bordeEstado;
        [SerializeField, Range(0f, 1f)] private float intensidadFondoEstado = 0.18f;

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
            if (button == null) button = GetComponent<Button>();
            var coloresAnteriores = button.colors;
            var transicionAnterior = button.transition;
            Graphic imagen = button.targetGraphic;
            Color colorAnterior = imagen != null ? imagen.color : Color.white;

            AplicarTema();

            bool cambioBoton = transicionAnterior != button.transition || !coloresAnteriores.Equals(button.colors);
            bool cambioImagen = imagen != null && colorAnterior != imagen.color;
            if (cambioBoton) UnityEditor.EditorUtility.SetDirty(button);
            if (cambioImagen) UnityEditor.EditorUtility.SetDirty(imagen);
            if (cambioBoton || cambioImagen) UnityEditor.SceneView.RepaintAll();
        }
#endif

        public void AplicarTema()
        {
            var theme = UIThemeController.Theme;
            if (theme == null) return;

            if (button == null) button = GetComponent<Button>();
            if (button == null) return;

            if (button.transition != Selectable.Transition.ColorTint)
                button.transition = Selectable.Transition.ColorTint;

            Color normal = ObtenerColorNormal(theme);
            Color resaltado = Color.Lerp(normal, Color.white, 0.10f);
            Color presionado = Color.Lerp(normal, Color.black, 0.12f);

            var colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = resaltado;
            colors.pressedColor = presionado;
            colors.selectedColor = resaltado;
            colors.disabledColor = _estado == EstadoBoton.Correcto || bordeEstado != null && _estado != EstadoBoton.Normal
                ? normal : theme.colorDeshabilitado;
            if (!button.colors.Equals(colors)) button.colors = colors;

            if (bordeEstado != null)
                bordeEstado.Rol = _estado switch
                {
                    EstadoBoton.Correcto => ThemedGraphicView.RolColor.Exito,
                    EstadoBoton.Incorrecto => ThemedGraphicView.RolColor.Error,
                    EstadoBoton.Casi => ThemedGraphicView.RolColor.Advertencia,
                    EstadoBoton.Seleccionado => ThemedGraphicView.RolColor.Primario,
                    _ => ThemedGraphicView.RolColor.Borde
                };

            if (button.targetGraphic == null) return;

            // ColorTint aplica el estilo en CanvasRenderer; el color base debe ser blanco.
            if (button.targetGraphic.color != Color.white)
                button.targetGraphic.color = Color.white;
            Color colorActual = button.interactable ? normal : colors.disabledColor;
            button.targetGraphic.CrossFadeColor(colorActual, 0f, true, true);
        }

        private Color ObtenerColorNormal(UIThemeModel theme)
        {
            switch (_estado)
            {
                case EstadoBoton.Seleccionado: return Color.Lerp(theme.colorSuperficie, theme.colorPrimario, 0.18f);
                case EstadoBoton.Correcto: return Color.Lerp(theme.colorSuperficie, theme.colorExito, intensidadFondoEstado);
                case EstadoBoton.Incorrecto: return Color.Lerp(theme.colorSuperficie, theme.colorError, intensidadFondoEstado);
                case EstadoBoton.Casi: return Color.Lerp(theme.colorSuperficie, theme.colorAdvertencia, intensidadFondoEstado);
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
