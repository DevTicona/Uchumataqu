using TMPro;
using UnityEngine;

namespace Assets.Scripts.Core.UI.Components
{
    /// <summary>
    /// Mantiene visible el cursor del TMP_InputField mientras se usa el teclado móvil.
    /// </summary>
    public static class MobileInputFieldStyle
    {
        public static void Aplicar(TMP_InputField input)
        {
            if (input == null) return;

            input.shouldHideMobileInput = true;
            input.shouldHideSoftKeyboard = false;
            input.ForceLabelUpdate();
        }
    }
}
