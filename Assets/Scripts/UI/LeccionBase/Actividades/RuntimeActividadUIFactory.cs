using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    internal static class RuntimeActividadUIFactory
    {
        private static readonly Color FondoBoton = new Color(0.93f, 0.86f, 0.68f, 1f);

        public static GameObject CrearContenedor(string nombre, Transform padre)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(padre, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(24f, 24f);
            rect.offsetMax = new Vector2(-24f, -24f);

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = go.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return go;
        }

        public static TextMeshProUGUI CrearTexto(Transform padre, string nombre, float tamano = 30f)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(padre, false);
            var texto = go.GetComponent<TextMeshProUGUI>();
            texto.fontSize = tamano;
            texto.color = Color.black;
            texto.alignment = TextAlignmentOptions.Center;
            texto.textWrappingMode = TextWrappingModes.Normal;
            go.GetComponent<LayoutElement>().preferredHeight = tamano * 1.8f;
            return texto;
        }

        public static Button CrearBoton(Transform padre, string nombre, string etiqueta)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(padre, false);
            go.GetComponent<Image>().color = FondoBoton;
            go.GetComponent<LayoutElement>().preferredHeight = 64f;

            TextMeshProUGUI texto = CrearTexto(go.transform, "Texto", 25f);
            texto.text = etiqueta;
            var rect = (RectTransform)texto.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(10f, 4f);
            rect.offsetMax = new Vector2(-10f, -4f);
            return go.GetComponent<Button>();
        }

        public static Transform CrearSubcontenedor(Transform padre, string nombre)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(padre, false);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            var fitter = go.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return go.transform;
        }
    }
}
