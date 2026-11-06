using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public sealed class AjusteTecladoRAView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField entrada;
        private RectTransform footer;
        private Canvas canvas;
        private Vector2 posicionInicial;
        private float siguienteLectura;

        private void Awake()
        {
            footer = (RectTransform)transform;
            canvas = GetComponentInParent<Canvas>();
            posicionInicial = footer.anchoredPosition;
        }

        private void LateUpdate()
        {
            if (entrada == null || !entrada.isFocused)
            {
                Restaurar();
                return;
            }
            if (Time.unscaledTime < siguienteLectura) return;
            siguienteLectura = Time.unscaledTime + 0.15f;
            float altura = AlturaTeclado();
            float desplazamiento = altura > 0f ? Mathf.Max(0f, altura - Screen.safeArea.y) + 12f : 0f;
            footer.anchoredPosition = posicionInicial + Vector2.up * (desplazamiento / canvas.scaleFactor);
        }

        private static float AlturaTeclado()
        {
            float altura = TouchScreenKeyboard.area.height;
#if UNITY_ANDROID && !UNITY_EDITOR
            // Algunos teclados Android devuelven un área vacía a Unity.
            if (altura <= 0f)
            {
                using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var window = activity.Call<AndroidJavaObject>("getWindow"))
                using (var decor = window.Call<AndroidJavaObject>("getDecorView"))
                using (var visible = new AndroidJavaObject("android.graphics.Rect"))
                {
                    decor.Call("getWindowVisibleDisplayFrame", visible);
                    int altoVentana = decor.Call<int>("getHeight");
                    int altoVisible = visible.Call<int>("height");
                    if (altoVentana > 0 && altoVentana - altoVisible > altoVentana * 0.15f)
                        altura = Screen.height * (1f - (float)altoVisible / altoVentana);
                }
            }
#endif
            return altura;
        }

        private void Restaurar()
        {
            if (footer != null) footer.anchoredPosition = posicionInicial;
        }

        private void OnDisable() => Restaurar();
    }
}
