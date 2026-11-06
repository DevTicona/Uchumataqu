using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Core.UI.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_InputField))]
    public sealed class EnfoqueEntradaMovilView : MonoBehaviour, IPointerDownHandler
    {
        private TMP_InputField entrada;
        public event Action EdicionSolicitada;

        private void Awake()
        {
            entrada = GetComponent<TMP_InputField>();
            entrada.shouldActivateOnSelect = true;
            entrada.onFocusSelectAll = false;
            entrada.shouldHideSoftKeyboard = false;
        }

        public void OnPointerDown(PointerEventData datos)
        {
            if (datos.button != PointerEventData.InputButton.Left || !entrada.IsActive() ||
                !entrada.IsInteractable() || entrada.readOnly) return;
            EdicionSolicitada?.Invoke();
            entrada.ActivateInputField();
        }
    }
}
