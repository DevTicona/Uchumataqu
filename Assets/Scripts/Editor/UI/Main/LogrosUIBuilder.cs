#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
using Assets.Scripts.Persistencia;
using Assets.Scripts.UI.Main;
using Assets.Scripts.UI.Main.Paginas;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Editor
{
    /// <summary>Compone los dos diseños de tarjeta y conecta sus prefabs a la escena.</summary>
    public static class LogrosUIBuilder
    {
        private const string Folder = "Assets/Content/Prefabs/Logros";
        private const string ScenePath = "Assets/Scenes/02_Main.unity";

        [MenuItem("Uchumataqu/UI/Migrar Logros a tarjetas reutilizables")]
        public static void Migrar()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("La composición requiere salir de Play.");

            UIThemeController.SetTheme(AssetDatabase.LoadAssetAtPath<UIThemeModel>("Assets/Content/UI/UITheme.asset"));
            GameObject obtenido = CrearPrefab(true);
            GameObject horizontal = CrearPrefab(false);
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            LogrosView view = BuscarVista(scene);
            var serialized = new SerializedObject(view);
            serialized.FindProperty("prefabLogroObtenido").objectReferenceValue = obtenido.GetComponent<LogroItemView>();
            serialized.FindProperty("prefabLogroHorizontal").objectReferenceValue = horizontal.GetComponent<LogroItemView>();
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Transform contenedor = (Transform)serialized.FindProperty("contenedorLogros").objectReferenceValue;
            for (int i = contenedor.childCount - 1; i >= 0; i--)
            {
                Transform hijo = contenedor.GetChild(i);
                if (hijo.name.StartsWith("Seccion", StringComparison.Ordinal) ||
                    hijo.name.StartsWith("Logro_", StringComparison.Ordinal) || hijo.name == "GaleriaObtenidos")
                    UnityEngine.Object.DestroyImmediate(hijo.gameObject);
            }

            // La vista reconoce estas instancias conectadas al prefab al entrar en Play.
            Canvas.ForceUpdateCanvases();
            view.ReconstruirEnEditor(MainModelFactory.CrearLogros(ProgresoRepository.ObtenerProgreso()));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static GameObject CrearPrefab(bool obtenido)
        {
            string nombre = obtenido ? "LogroObtenido" : "LogroHorizontal";
            GameObject root = Crear(nombre);
            root.GetComponent<RectTransform>().sizeDelta = obtenido ? new Vector2(420, 420) : new Vector2(880, 160);
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredHeight = obtenido ? 420 : 160;
            Image fondo = Imagen(root, ThemedGraphicView.RolColor.Superficie);
            AsignarSprite(fondo, "ui_panel", Image.Type.Sliced);
            fondo.pixelsPerUnitMultiplier = 4f;

            GameObject badgeObject = Crear("BadgeIcono", root.transform);
            RectTransform badgeRect = badgeObject.GetComponent<RectTransform>();
            if (obtenido)
            {
                badgeRect.anchorMin = badgeRect.anchorMax = new Vector2(.5f, 1f);
                badgeRect.pivot = new Vector2(.5f, 1f);
                badgeRect.anchoredPosition = new Vector2(0, -28);
                badgeRect.sizeDelta = new Vector2(104, 104);
            }
            else
            {
                badgeRect.anchorMin = badgeRect.anchorMax = new Vector2(0, .5f);
                badgeRect.pivot = new Vector2(0, .5f);
                badgeRect.anchoredPosition = new Vector2(20, 0);
                badgeRect.sizeDelta = new Vector2(80, 80);
            }
            Image badge = badgeObject.AddComponent<Image>();
            badge.raycastTarget = false;
            AsignarSprite(badge, "ui_circle", Image.Type.Simple);
            GameObject iconoObject = Crear("Icono", badgeObject.transform);
            Estirar(iconoObject.GetComponent<RectTransform>(), 14);
            Image icono = Imagen(iconoObject, ThemedGraphicView.RolColor.Exito);
            icono.preserveAspect = true;

            Transform info = root.transform;
            if (!obtenido)
            {
                GameObject columna = Crear("ColumnaInfo", root.transform);
                Estirar(columna.GetComponent<RectTransform>());
                columna.GetComponent<RectTransform>().offsetMin = new Vector2(116, 22);
                columna.GetComponent<RectTransform>().offsetMax = new Vector2(-148, -22);
                info = columna.transform;
            }
            TMP_Text titulo = Texto(info, "Titulo", ThemedTextView.TipoTexto.Normal, FontStyles.Bold, obtenido);
            TMP_Text descripcion = Texto(info, "Descripcion", ThemedTextView.TipoTexto.Pequeño, FontStyles.Normal, obtenido);
            if (obtenido)
            {
                Arriba(titulo.rectTransform, 20, 20, 146, 74);
                Arriba(descripcion.rectTransform, 24, 24, 226, 66);
            }
            else
            {
                Arriba(titulo.rectTransform, 0, 0, 0, 48);
                Arriba(descripcion.rectTransform, 0, 0, 50, 36);
            }

            Slider slider = obtenido ? null : CrearSlider(info);
            GameObject estadoObject = Crear("Estado", root.transform);
            RectTransform estadoRect = estadoObject.GetComponent<RectTransform>();
            estadoRect.anchorMin = estadoRect.anchorMax = obtenido ? new Vector2(.5f, 0) : new Vector2(1, .5f);
            estadoRect.pivot = obtenido ? new Vector2(.5f, 0) : new Vector2(1, .5f);
            estadoRect.anchoredPosition = obtenido ? new Vector2(0, 28) : new Vector2(-20, 0);
            estadoRect.sizeDelta = obtenido ? new Vector2(160, 44) : new Vector2(112, 44);
            Image estado = estadoObject.AddComponent<Image>();
            estado.raycastTarget = false;
            AsignarSprite(estado, "ui_pill", Image.Type.Sliced);
            TMP_Text textoEstado = Texto(estadoObject.transform, "Texto", ThemedTextView.TipoTexto.Pequeño, FontStyles.Normal, true);
            Estirar(textoEstado.rectTransform, 4);

            GameObject marcoObject = Crear("Marco", root.transform);
            Estirar(marcoObject.GetComponent<RectTransform>());
            Image marco = marcoObject.AddComponent<Image>();
            marco.raycastTarget = false;
            marco.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Base/ui_aro_cuadrado_solido.png");
            marco.type = Image.Type.Sliced;
            marco.fillCenter = false;
            marco.pixelsPerUnitMultiplier = 1f;

            LogroItemView view = root.AddComponent<LogroItemView>();
            Asignar(view, new Dictionary<string, UnityEngine.Object>
            {
                ["fondo"] = fondo, ["badgeIcono"] = badge, ["icono"] = icono, ["txtTitulo"] = titulo,
                ["txtDescripcion"] = descripcion, ["sliderProgreso"] = slider,
                ["rellenoProgreso"] = slider != null ? slider.fillRect.GetComponent<Image>() : null,
                ["fondoEstado"] = estado, ["txtEstado"] = textoEstado, ["marco"] = marco,
                ["catalogoIconos"] = AssetDatabase.LoadAssetAtPath<CatalogoIconosLogrosModel>("Assets/Content/UI/CatalogoIconosLogros.asset")
            });
            var estilos = new SerializedObject(view);
            badge.color = estado.color = estilos.FindProperty("colorBadgeObtenido").colorValue;
            marco.color = estilos.FindProperty("colorObtenido").colorValue;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{Folder}/{nombre}.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void AsignarSprite(Image imagen, string nombre, Image.Type tipo)
        {
            imagen.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Content/UI/Base/{nombre}.png");
            if (imagen.sprite == null) throw new InvalidOperationException($"Falta el sprite {nombre}.");
            imagen.type = tipo;
            imagen.preserveAspect = tipo == Image.Type.Simple;
        }

        private static Slider CrearSlider(Transform parent)
        {
            GameObject root = Crear("SliderProgreso", parent);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(.5f, 0);
            rect.sizeDelta = new Vector2(0, 18);
            Image background = Imagen(root, ThemedGraphicView.RolColor.ProgresoFondo);
            AsignarSprite(background, "ui_progress_rounded", Image.Type.Sliced);
            GameObject fillArea = Crear("Fill Area", root.transform);
            Estirar(fillArea.GetComponent<RectTransform>());
            GameObject fill = Crear("Fill", fillArea.transform);
            Estirar(fill.GetComponent<RectTransform>());
            Image fillImage = Imagen(fill, ThemedGraphicView.RolColor.Primario);
            AsignarSprite(fillImage, "ui_progress_rounded", Image.Type.Sliced);
            background.pixelsPerUnitMultiplier = fillImage.pixelsPerUnitMultiplier = 4f;
            Slider slider = root.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.targetGraphic = fillImage;
            slider.SetValueWithoutNotify(0);
            return slider;
        }

        private static Image Imagen(GameObject go, ThemedGraphicView.RolColor rol)
        {
            Image image = go.AddComponent<Image>();
            image.raycastTarget = false;
            LogrosView.AsignarThemedGraphic(image, rol);
            return image;
        }

        private static TMP_Text Texto(Transform parent, string nombre, ThemedTextView.TipoTexto tipo, FontStyles estilo, bool centrado)
        {
            TextMeshProUGUI text = Crear(nombre, parent).AddComponent<TextMeshProUGUI>();
            text.fontStyle = estilo;
            text.alignment = centrado ? TextAlignmentOptions.Center : TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            LogrosView.AsignarThemedText(text, tipo,
                tipo == ThemedTextView.TipoTexto.Pequeño ? ThemedTextView.ColorTexto.Secundario : ThemedTextView.ColorTexto.Primario);
            return text;
        }

        private static GameObject Crear(string nombre, Transform parent = null)
        {
            GameObject go = new(nombre, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        private static void Estirar(RectTransform rect, float margen = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(margen, margen);
            rect.offsetMax = new Vector2(-margen, -margen);
        }

        private static void Arriba(RectTransform rect, float izquierda, float derecha, float superior, float alto)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(.5f, 1);
            rect.offsetMin = new Vector2(izquierda, -superior - alto);
            rect.offsetMax = new Vector2(-derecha, -superior);
        }

        private static void Asignar(Component component, IReadOnlyDictionary<string, UnityEngine.Object> referencias)
        {
            var serialized = new SerializedObject(component);
            foreach (var referencia in referencias)
                serialized.FindProperty(referencia.Key).objectReferenceValue = referencia.Value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static LogrosView BuscarVista(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                LogrosView view = root.GetComponentInChildren<LogrosView>(true);
                if (view != null) return view;
            }
            throw new InvalidOperationException("No se encontró LogrosView en 02_Main.");
        }
    }
}
#endif
