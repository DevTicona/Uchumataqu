using UnityEngine;
using System;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Assets.Scripts.Core
{
    public class NavigationBackHandler : MonoBehaviour
    {
        private Action onBack;

        public void Configurar(Action accionAtras)
        {
            onBack = accionAtras;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            // Botón físico "atrás" (Android) / Escape (teclado)
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                onBack?.Invoke();
            }
#endif
        }
    }
}
