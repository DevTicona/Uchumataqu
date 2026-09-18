using UnityEngine;

namespace Assets.Scripts.Core.UI.Components
{
    /// <summary>
    /// Ajusta automáticamente la interfaz al Safe Area del dispositivo.
    /// Funciona tanto en orientación vertical como horizontal.
    /// Evita que la UI quede debajo del notch o bordes del sistema.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.Core.UI.Components", sourceAssembly: "Assembly-CSharp", sourceClassName: "SafeAreaUI")]
    public class SafeAreaView : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect ultimoSafeArea;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            AplicarSafeArea();
        }

        private void OnEnable()
        {
            AplicarSafeArea();
        }

        private void Update()
        {
            if (Screen.safeArea != ultimoSafeArea)
            {
                AplicarSafeArea();
            }
        }

        private void AplicarSafeArea()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (Screen.width <= 0 || Screen.height <= 0) return;
            Rect safeArea = Screen.safeArea;
            ultimoSafeArea = safeArea;
            Canvas canvas = GetComponentInParent<Canvas>();
            float factor = canvas != null ? canvas.scaleFactor : 1f;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = safeArea.min / factor;
            rectTransform.offsetMax = (safeArea.max - new Vector2(Screen.width, Screen.height)) / factor;
        }
    }
}
