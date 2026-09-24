using TMPro;
using UnityEngine;

namespace Assets.Scripts.Core.UI.Components
{
    /// <summary>
    /// Mantiene visible el cursor del TMP_InputField mientras se usa el teclado móvil.
    /// </summary>
    public static class MobileInputFieldStyle
    {
        private static readonly Color CaretColor = new Color(0.05f, 0.14f, 0.22f, 1f);

        public static void Aplicar(TMP_InputField input)
        {
            if (input == null) return;

            input.shouldHideMobileInput = true;
            input.shouldHideSoftKeyboard = false;
            input.customCaretColor = true;
            input.caretColor = CaretColor;
            input.caretWidth = 3;
            input.caretBlinkRate = 0.7f;
            input.ForceLabelUpdate();
        }
    }
}
