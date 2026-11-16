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
        private const float IntervaloConsultaInset = 0.25f;

        private RectTransform rectTransform;
        private Rect ultimoSafeArea;
        private int insetInferiorAndroid;
        private float siguienteConsultaInset;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ActualizarInsetAndroid();
            AplicarSafeArea();
        }

        private void OnEnable()
        {
            ActualizarInsetAndroid();
            AplicarSafeArea();
        }

        private void Update()
        {
            bool cambioInset = Time.unscaledTime >= siguienteConsultaInset && ActualizarInsetAndroid();
            if (cambioInset || Screen.safeArea != ultimoSafeArea)
            {
                AplicarSafeArea();
            }
        }

        private static int ObtenerInsetInferiorAndroid()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var ventana = activity.Call<AndroidJavaObject>("getWindow"))
                using (var decorView = ventana.Call<AndroidJavaObject>("getDecorView"))
                using (var windowInsets = decorView.Call<AndroidJavaObject>("getRootWindowInsets"))
                {
                    if (windowInsets == null) return 0;

                    int sdk;
                    using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    {
                        sdk = version.GetStatic<int>("SDK_INT");
                    }

                    if (sdk >= 30)
                    {
                        using (var tipos = new AndroidJavaClass("android.view.WindowInsets$Type"))
                        using (var insets = windowInsets.Call<AndroidJavaObject>("getInsetsIgnoringVisibility", tipos.CallStatic<int>("navigationBars")))
                        {
                            return insets != null ? insets.Get<int>("bottom") : 0;
                        }
                    }

                    return Mathf.Max(
                        windowInsets.Call<int>("getSystemWindowInsetBottom"),
                        windowInsets.Call<int>("getStableInsetBottom"));
                }
            }
            catch (System.Exception)
            {
                return 0;
            }
#else
            return 0;
#endif
        }

        private bool ActualizarInsetAndroid()
        {
            siguienteConsultaInset = Time.unscaledTime + IntervaloConsultaInset;
            int nuevoInset = ObtenerInsetInferiorAndroid();
            if (nuevoInset == insetInferiorAndroid) return false;

            insetInferiorAndroid = nuevoInset;
            return true;
        }

        private void AplicarSafeArea()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (Screen.width <= 0 || Screen.height <= 0) return;

            Rect safeArea = Screen.safeArea;
            ultimoSafeArea = safeArea;
            safeArea.yMin = Mathf.Max(safeArea.yMin, Mathf.Min(insetInferiorAndroid, Screen.height));

            Canvas canvas = GetComponentInParent<Canvas>();
            float factor = canvas != null ? canvas.scaleFactor : 1f;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = safeArea.min / factor;
            rectTransform.offsetMax = (safeArea.max - new Vector2(Screen.width, Screen.height)) / factor;
        }
    }
}
