using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Core.UI.Theme;

namespace Assets.Scripts.Core.UI.Components
{
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

        private void AplicarTema()
        {
            var theme = UIThemeManager.Theme;
            if (theme == null) return;

            var colors = button.colors;

            colors.normalColor = theme.colorBoton;
            colors.highlightedColor = theme.colorBotonHover;
            colors.pressedColor = theme.colorPrimario;
            colors.selectedColor = theme.colorPrimario;
            colors.disabledColor = Color.gray;

            button.colors = colors;

            // Fuerza actualización visual
            button.targetGraphic?.SetAllDirty();
        }
    }
}