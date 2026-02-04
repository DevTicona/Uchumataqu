using UnityEngine;

/// <summary>
/// Ajusta el RectTransform para respetar la Safe Area del dispositivo
/// (notch, barras del sistema, etc.)
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeAreaUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect ultimaSafeArea;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        AplicarSafeArea();
    }

    private void Update()
    {
        // Reaplica solo si la SafeArea cambió (rotación, resize, etc.)
        if (Screen.safeArea != ultimaSafeArea)
        {
            AplicarSafeArea();
        }
    }

    private void AplicarSafeArea()
    {
        Rect safeArea = Screen.safeArea;
        ultimaSafeArea = safeArea;

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
