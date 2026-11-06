using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public sealed class CursorEscrituraRAView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField entrada;
        [SerializeField] private Image cursor;

        public void Configurar(TMP_InputField campo, Image indicador)
        {
            entrada = campo;
            cursor = indicador;
        }

        private void LateUpdate()
        {
            if (entrada == null || cursor == null) return;
            bool visible = entrada.interactable && (entrada.isFocused
                ? Time.unscaledTime % 1.2f < 0.7f : string.IsNullOrEmpty(entrada.text));
            cursor.enabled = visible;
            if (!visible) return;
            var texto = entrada.textComponent;
            var info = texto.textInfo;
            float x = texto.rectTransform.rect.xMin + texto.margin.x;
            float y = 0f;
            float alto = texto.fontSize;
            int posicion = Mathf.Clamp(entrada.caretPosition, 0, info.characterCount);
            if (info.characterCount > 0 && entrada.text.Length > 0)
            {
                var caracter = info.characterInfo[Mathf.Min(posicion, info.characterCount - 1)];
                x = posicion < info.characterCount ? caracter.origin : caracter.xAdvance;
                y = (caracter.ascender + caracter.descender) * 0.5f;
                alto = caracter.ascender - caracter.descender;
            }
            cursor.rectTransform.anchoredPosition = new Vector2(x, y);
            cursor.rectTransform.sizeDelta = new Vector2(3f, Mathf.Max(20f, alto));
        }
    }
}
