using UnityEngine;

namespace Assets.Scripts.Views.UI
{
    public class BalsaProgresoView : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;

        [Header("Offset lateral")]
        [SerializeField] private float offsetHorizontal = 80f;
        [SerializeField] private float offsetVertical = 50f;

        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
        }

        public void PosicionarEnIsla(RectTransform islaRect)
        {
            if (islaRect == null)
                return;

            Vector2 islaPos = islaRect.anchoredPosition;

            bool islaALaIzquierda = islaPos.x < 0f;

            float x = islaPos.x + (islaALaIzquierda ? offsetHorizontal : -offsetHorizontal);
            float y = islaPos.y + offsetVertical;

            rectTransform.anchoredPosition = new Vector2(x, y);
        }
    }
}
