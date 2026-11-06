using Assets.Scripts.Util;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Core.UI.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_InputField))]
    public sealed class InicialMayusculaInputField : MonoBehaviour
    {
        private TMP_InputField entrada;
        private bool composicionPendiente;

        private void Awake()
        {
            entrada = GetComponent<TMP_InputField>();
            ConfigurarTeclado(entrada);
        }

        public static void ConfigurarTeclado(TMP_InputField campo)
        {
            if (campo == null) return;
            campo.contentType = TMP_InputField.ContentType.Standard;
#if UNITY_ANDROID || UNITY_EDITOR
            // En Android Unity asigna a este tipo TEXT + CAP_SENTENCES, sin limitar caracteres Unicode.
            campo.keyboardType = TouchScreenKeyboardType.ASCIICapable;
#else
            campo.contentType = TMP_InputField.ContentType.Autocorrected;
#endif
        }

        private void OnEnable()
        {
            entrada.onValueChanged.AddListener(Normalizar);
            Normalizar(entrada.text);
        }

        private void OnDisable()
        {
            entrada.onValueChanged.RemoveListener(Normalizar);
            composicionPendiente = false;
        }

        private void LateUpdate()
        {
            if (composicionPendiente && string.IsNullOrEmpty(Input.compositionString))
                Normalizar(entrada.text);
        }

        private void Normalizar(string texto)
        {
            // Espera la confirmación de tildes y otros caracteres compuestos del teclado.
            composicionPendiente = !string.IsNullOrEmpty(Input.compositionString);
            if (composicionPendiente) return;
            string normalizado = FormatoTexto.ConInicialMayuscula(texto);
            if (normalizado == texto) return;
            int posicion = entrada.selectionStringAnchorPosition;
            int seleccion = entrada.selectionStringFocusPosition;
            entrada.SetTextWithoutNotify(normalizado);
            entrada.selectionStringAnchorPosition = Mathf.Min(posicion, normalizado.Length);
            entrada.selectionStringFocusPosition = Mathf.Min(seleccion, normalizado.Length);
        }
    }
}
