using UnityEngine;

namespace Assets.Scripts.Core.UI.Components
{
    /// <summary>
    /// Ajusta automáticamente la interfaz al Safe Area del dispositivo.
    /// Funciona tanto en orientación vertical como horizontal.
    /// Evita que la UI quede debajo del notch o bordes del sistema.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaUI : MonoBehaviour
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
            Rect safeArea = Screen.safeArea;
            ultimoSafeArea = safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;

            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}
