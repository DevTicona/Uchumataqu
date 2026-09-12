using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Core.UI.Theme;

namespace Assets.Scripts.Core.UI.Components
{
    [ExecuteAlways]
    [RequireComponent(typeof(Button))]
    public class ThemedButton : MonoBehaviour
    {
        public enum EstiloBoton
        {
            Primario,
            Secundario,
            Respuesta,
            Peligro
        }

        [SerializeField] private EstiloBoton estilo = EstiloBoton.Primario;

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
            UIThemeManager.OnThemeChanged += AplicarTema;

            // IMPORTANTE: aplicar inmediatamente si ya existe theme
            if (UIThemeManager.Theme != null)
            {
                AplicarTema();
            }
        }

        private void OnDisable()
        {
            UIThemeManager.OnThemeChanged -= AplicarTema;
        }

        public void AplicarTema()
        {
            var theme = UIThemeManager.Theme;
            if (theme == null) return;

            if (button == null) button = GetComponent<Button>();
            if (button == null) return;

            button.transition = Selectable.Transition.ColorTint;

            Color normal = ObtenerColorNormal(theme);
            Color resaltado = Color.Lerp(normal, Color.white, 0.10f);
            Color presionado = Color.Lerp(normal, Color.black, 0.12f);

            if (button.targetGraphic is Image image)
            {
                image.color = Color.white;
                if (theme.spriteBoton != null)
                {
                    image.sprite = theme.spriteBoton;
                    image.type = Image.Type.Sliced;
                }
            }

            var colors = button.colors;

            // Un único color para todos los estados: sin variaciones de hover/press
            colors.normalColor = normal;
            colors.highlightedColor = resaltado;
            colors.pressedColor = presionado;
            colors.selectedColor = resaltado;
            colors.disabledColor   = new Color(
                normal.r,
                normal.g,
                normal.b,
                0.45f);

            button.colors = colors;

            // Fuerza actualización visual
            button.targetGraphic?.SetAllDirty();
        }

        private Color ObtenerColorNormal(UITheme theme)
        {
            switch (estilo)
            {
                case EstiloBoton.Secundario:
                case EstiloBoton.Respuesta:
                    return theme.colorBotonSuave;
                case EstiloBoton.Peligro:
                    return theme.colorIncorrecto;
                default:
                    return theme.colorBoton;
            }
        }
    }
}
