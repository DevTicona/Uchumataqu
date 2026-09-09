using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Core.UI.Theme;

namespace Assets.Scripts.Core.UI.Components
{
    [ExecuteAlways]
    [RequireComponent(typeof(Button))]
    public class ThemedButton : MonoBehaviour
    {
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

            if (button.targetGraphic is Image image)
                image.color = Color.white;

            var colors = button.colors;

            // Un único color para todos los estados: sin variaciones de hover/press
            colors.normalColor     = theme.colorBoton;
            colors.highlightedColor = theme.colorBoton;
            colors.pressedColor    = theme.colorBoton;
            colors.selectedColor   = theme.colorBoton;
            colors.disabledColor   = new Color(
                theme.colorBoton.r,
                theme.colorBoton.g,
                theme.colorBoton.b,
                0.45f);

            button.colors = colors;

            // Fuerza actualización visual
            button.targetGraphic?.SetAllDirty();
        }
    }
}
