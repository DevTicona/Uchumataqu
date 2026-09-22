#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
using Assets.Scripts.UI.LeccionBase.Actividades;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Editor
{
    public static partial class LeccionActivityStyleUpdater
    {
        private const string ScenePath = "Assets/Scenes/03_LeccionBase.unity";
        private const string BasePath = "Assets/Content/UI/Base";

        [MenuItem("Uchumataqu/UI/Actualizar estilo de actividades")]
        public static void Actualizar()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Sal del modo Play para actualizar el diseño.");
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                Aplicar(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        public static void Aplicar(Scene scene)
        {
            UIThemeModel theme = AssetDatabase.LoadAssetAtPath<UIThemeModel>("Assets/Content/UI/UITheme.asset");
            UIThemeController.SetTheme(theme);
            Transform host = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .Single(t => t.name == "ActividadHost");
            foreach (Transform page in host)
            {
                Component view = page.GetComponent<SeleccionMultipleView>();
                if (view != null) SepararBotonAudio(view, "btnAudioPregunta", "ESCUCHAR PALABRA");
                view = page.GetComponent<WordBankView>();
                if (view != null) SepararBotonAudio(view, "btnAudioPrompt", "ESCUCHAR ORACIÓN");
                view = page.GetComponent<PresentacionFamiliaView>();
                if (view != null) SepararBotonAudio(view, "btnAudio", "ESCUCHAR PALABRA");
            }
            foreach (Transform page in host)
            {
                if (page.name == "PagePresentacionFamilia") continue;
                Transform content = page.Find("ScrollRect/Viewport/Content");
                foreach (Transform block in content)
                {
                    if (block.name == "EstimuloHost" || block.name == "Referencia") Panel(block.gameObject, true);
                    else if (block.name == "PanelMicrofono" || block.name == "PanelResultado") Panel(block.gameObject, false);
                }
                foreach (RectTransform rect in page.GetComponentsInChildren<RectTransform>(true))
                    rect.gameObject.layer = LayerMask.NameToLayer("UI");
            }
            Transform seleccion = host.Find("PageSeleccionMultiple/ScrollRect/Viewport/Content");
            seleccion.Find("OpcionesContainer").GetComponent<VerticalLayoutGroup>().spacing = 24f;
            Transform asr = host.Find("PageASR/ScrollRect/Viewport/Content");
            foreach (Button button in asr.GetComponentsInChildren<Button>(true))
                if (button.transform.Find("Icono") != null) button.transform.Find("Icono").GetComponent<Image>().sprite =
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/" +
                        (button.name == "BtnPracticar" ? "Audio/icono_microfono.png" : "Audio/icono_audio.png"));
            ConfigurarBanco(host.Find("PageWordBank").GetComponent<WordBankView>());
            foreach (Transform page in host) ConfigurarPage(page.gameObject);
            foreach (Transform page in host)
                foreach (Button button in page.Find("ScrollRect/Viewport/Content").GetComponentsInChildren<Button>(true))
                    if (button.transform.parent.name == "Content" && button.transform.Find("Icono") != null)
                    {
                        LayoutElement element = button.GetComponent<LayoutElement>();
                        element.minHeight = element.preferredHeight = 112f;
                        element.minWidth = 280f;
                        element.preferredWidth = 440f;
                        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                        label.gameObject.SetActive(true);
                        label.GetComponent<LayoutElement>().flexibleWidth = 0;
                    }
            ConfigurarAccionesPresentacion(host.Find("PagePresentacionFamilia").GetComponent<PresentacionFamiliaView>());
            var paths = new HashSet<string>();
            foreach (MonoBehaviour view in host.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (view == null) continue;
                var serialized = new SerializedObject(view);
                var property = serialized.GetIterator();
                while (property.NextVisible(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference && property.name.StartsWith("prefab") &&
                        property.objectReferenceValue is GameObject)
                        paths.Add(AssetDatabase.GetAssetPath(property.objectReferenceValue));
            }
            foreach (string path in paths) ActualizarElemento(path);
        }

        private static Sprite Sprite(string name)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{BasePath}/{name}.png");
            if (sprite == null) throw new InvalidOperationException($"Falta el sprite {name}.");
            return sprite;
        }

        private static void SepararBotonAudio(Component view, string field, string texto)
        {
            Button button = (Button)new SerializedObject(view).FindProperty(field).objectReferenceValue;
            Transform content = view.transform.Find("ScrollRect/Viewport/Content");
            Transform card = content.Find(view is PresentacionFamiliaView ? "CardRaiz" :
                view is ASRView ? "Referencia" : "EstimuloHost");
            button.transform.SetParent(content, false);
            button.transform.SetSiblingIndex(card.GetSiblingIndex() + 1);
            if (view is SeleccionMultipleView) button.name = "BtnAudioPregunta";
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            label.gameObject.SetActive(true);
            label.text = texto;
            Image icon = button.transform.Find("Icono").GetComponent<Image>();
            icon.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Audio/icono_audio.png");
            icon.preserveAspect = true;
        }

        private static void ConfigurarAccionesPresentacion(PresentacionFamiliaView view)
        {
            Transform content = view.transform.Find("ScrollRect/Viewport/Content");
            Transform card = content.Find("CardRaiz");
            LayoutElement cardElement = card.GetComponent<LayoutElement>();
            cardElement.minHeight = cardElement.preferredHeight = -1f;
            var cardLayout = card.GetComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(24, 24, 24, 24);
            cardLayout.spacing = 24;
            Transform actions = content.Find("AccionesPresentacion");
            if (actions == null) actions = Objeto("AccionesPresentacion", content).transform;
            actions.SetSiblingIndex(card.GetSiblingIndex() + 1);
            var layout = actions.GetComponent<HorizontalLayoutGroup>() ?? actions.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;
            var serialized = new SerializedObject(view);
            foreach (string field in new[] { "btnAudio", "btnPracticar" })
            {
                var button = (Button)serialized.FindProperty(field).objectReferenceValue;
                button.transform.SetParent(actions, false);
                button.transform.SetSiblingIndex(field == "btnAudio" ? 0 : 1);
                var element = button.GetComponent<LayoutElement>();
                element.minWidth = 0;
                element.preferredWidth = 0;
                element.flexibleWidth = 1;
                element.minHeight = 112;
                element.preferredHeight = -1;
            }
            Transform oldActions = card.Find("ContentBotones");
            if (oldActions != null && oldActions.childCount == 0) oldActions.gameObject.SetActive(false);
        }

        private static void Panel(GameObject root, bool destacado, bool discontinuo = false)
        {
            Image image = root.GetComponent<Image>() ?? root.AddComponent<Image>();
            image.sprite = Sprite(destacado ? "ui_panel_shadow_abajo" : "ui_panel");
            image.type = Image.Type.Sliced;
            image.raycastTarget = false;
            if (!destacado) image.pixelsPerUnitMultiplier = discontinuo ? 1f : 2f;
            var themed = root.GetComponent<ThemedGraphicView>() ?? root.AddComponent<ThemedGraphicView>();
            themed.Rol = ThemedGraphicView.RolColor.Superficie;
            if (destacado)
            {
                Transform old = root.transform.Find("Marco");
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            else Marco(root.transform, discontinuo);
        }

        private static Image Marco(Transform parent, bool discontinuo = false)
        {
            Transform old = parent.Find("Marco");
            GameObject root = old != null ? old.gameObject : Objeto("Marco", parent);
            Image image = root.GetComponent<Image>() ?? root.AddComponent<Image>();
            image.sprite = Sprite(discontinuo ? "ui_aro_cuadrado_discontinuo" : "ui_aro_cuadrado_solido");
            image.type = Image.Type.Sliced;
            image.fillCenter = false;
            image.raycastTarget = false;
            image.pixelsPerUnitMultiplier = discontinuo ? 1f : 2f;
            Stretch((RectTransform)root.transform);
            (root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>()).ignoreLayout = true;
            var themed = root.GetComponent<ThemedGraphicView>() ?? root.AddComponent<ThemedGraphicView>();
            themed.Rol = discontinuo ? ThemedGraphicView.RolColor.TextoSecundario : ThemedGraphicView.RolColor.Borde;
            root.transform.SetAsLastSibling();
            return image;
        }

        private static void ConfigurarBanco(WordBankView view)
        {
            var serialized = new SerializedObject(view);
            Transform destino = (Transform)serialized.FindProperty("contenedorDestino").objectReferenceValue;
            Transform origen = (Transform)serialized.FindProperty("contenedorOrigen").objectReferenceValue;
            Transform content = view.transform.Find("ScrollRect/Viewport/Content");
            Transform caja = content.Find("CajaOracion");
            if (caja == null)
            {
                caja = Objeto("CajaOracion", content).transform;
                caja.SetSiblingIndex(destino.GetSiblingIndex());
                destino.SetParent(caja, false);
            }
            Panel(caja.gameObject, false, true);
            VerticalLayoutGroup layout = caja.GetComponent<VerticalLayoutGroup>() ?? caja.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            (caja.GetComponent<LayoutElement>() ?? caja.gameObject.AddComponent<LayoutElement>()).minHeight = 220f;
            foreach (Transform container in new[] { destino, origen })
            {
                Quitar<ThemedGraphicView>(container.gameObject);
                Quitar<Image>(container.gameObject);
                Transform frame = container.Find("Marco");
                if (frame != null) UnityEngine.Object.DestroyImmediate(frame.gameObject);
                var vertical = container.GetComponent<VerticalLayoutGroup>();
                vertical.padding = new RectOffset();
                vertical.spacing = 16f;
                vertical.childAlignment = TextAnchor.MiddleCenter;
                vertical.childControlHeight = vertical.childControlWidth = true;
                vertical.childForceExpandHeight = false;
                vertical.childForceExpandWidth = true;
            }
            Panel(origen.gameObject, false);
            origen.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(24, 24, 24, 24);
            var destinationElement = destino.GetComponent<LayoutElement>() ?? destino.gameObject.AddComponent<LayoutElement>();
            destinationElement.minHeight = destinationElement.preferredHeight = -1f;
            var bankElement = origen.GetComponent<LayoutElement>() ?? origen.gameObject.AddComponent<LayoutElement>();
            bankElement.minHeight = 160f;
            bankElement.preferredHeight = -1f;
            Transform placeholder = caja.Find("TxtRespuestaVacia");
            if (placeholder == null)
            {
                var text = UnityEngine.Object.Instantiate(content.Find("TxtRespuesta").GetComponent<TMP_Text>(), caja);
                placeholder = text.transform;
                placeholder.name = "TxtRespuestaVacia";
            }
            var label = placeholder.GetComponent<TMP_Text>();
            label.text = "Toca las palabras para ordenarlas aquí";
            TextoTema(label, ThemedTextView.TipoTexto.Pequeño, ThemedTextView.ColorTexto.Terciario);
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.GetComponent<LayoutElement>().ignoreLayout = true;
            Stretch(label.rectTransform, 24);
            serialized.FindProperty("txtRespuestaVacia").objectReferenceValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            content.Find("TxtRespuesta").GetComponent<TMP_Text>().text = "Toca una palabra para añadirla o devolverla";
            content.Find("TxtBanco").GetComponent<TMP_Text>().text = "Banco de palabras";
        }

        private static void ActualizarElemento(string path)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                string name = root.name;
                if (name == "fila_forma_familia")
                {
                    ConfigurarFilaForma(root);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    return;
                }
                Quitar<ThemedButtonView>(root);
                Panel(root, false);
                Button button = root.GetComponent<Button>();
                root.GetComponent<Image>().raycastTarget = true;
                button.transition = Selectable.Transition.None;
                button.targetGraphic = root.GetComponent<Image>();
                TMP_Text text = root.GetComponentInChildren<TMP_Text>(true);
                text.raycastTarget = false;
                var horizontal = root.GetComponent<HorizontalLayoutGroup>();
                horizontal.padding = new RectOffset(24, 24, 12, 12);
                horizontal.childAlignment = TextAnchor.MiddleCenter;
                horizontal.childControlWidth = horizontal.childControlHeight = true;
                horizontal.childForceExpandWidth = horizontal.childForceExpandHeight = false;
                float height = name == "ficha_wordbank" ? 76 : name == "tarjeta_memoria" ? 164 : 128;
                LayoutElement element = root.GetComponent<LayoutElement>();
                element.minHeight = height;
                element.preferredHeight = -1;
                text.GetComponent<LayoutElement>().minHeight = text.GetComponent<LayoutElement>().preferredHeight = -1;
                if (name == "opcion_respuesta")
                {
                    Quitar<ThemedGraphicView>(root);
                    Image frame = root.transform.Find("Marco").GetComponent<Image>();
                    Quitar<ThemedGraphicView>(frame.gameObject);
                    TextoTema(text, ThemedTextView.TipoTexto.Normal, ThemedTextView.ColorTexto.Primario);
                    text.alignment = TextAlignmentOptions.MidlineLeft;
                    text.transform.SetAsFirstSibling();
                    text.GetComponent<LayoutElement>().flexibleWidth = 1;
                    Image indicator = Icono(root.transform, "IndicadorSeleccion", Sprite("ui_aro_circle"), 44);
                    Image check = Icono(indicator.transform, "Check",
                        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Estados/icono_correcto.png"), 36);
                    check.color = Color.white;
                    var checkRect = check.rectTransform;
                    checkRect.anchorMin = checkRect.anchorMax = new Vector2(.5f, .5f);
                    checkRect.anchoredPosition = Vector2.zero;
                    var selection = root.GetComponent<TarjetaSeleccionView>() ?? root.AddComponent<TarjetaSeleccionView>();
                    Asignar(selection, new Dictionary<string, UnityEngine.Object> {
                        ["fondo"] = root.GetComponent<Image>(), ["marco"] = frame, ["indicador"] = indicator,
                        ["check"] = check, ["texto"] = text, ["circulo"] = Sprite("ui_circle"), ["aro"] = Sprite("ui_aro_circle"),
                        ["iconoCorrecto"] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Estados/icono_correcto.png"),
                        ["iconoIncorrecto"] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Estados/icono_incorrecto.png") });
                    selection.MostrarSeleccion(false);
                }
                else
                {
                    Quitar<ThemedGraphicView>(root);
                    var themed = root.GetComponent<ThemedButtonView>() ?? root.AddComponent<ThemedButtonView>();
                    themed.Estilo = ThemedButtonView.EstiloBoton.Superficie;
                    root.GetComponent<Image>().sprite = Sprite("ui_panel");
                    root.GetComponent<Image>().type = Image.Type.Sliced;
                    TextoTema(text, ThemedTextView.TipoTexto.Normal, ThemedTextView.ColorTexto.Primario);
                    text.fontStyle = FontStyles.Normal;
                    text.GetComponent<LayoutElement>().flexibleWidth = 1;
                }
                if (name == "ficha_wordbank")
                {
                    horizontal.padding = new RectOffset(24, 24, 8, 8);
                    text.fontStyle = FontStyles.Normal;
                    text.alignment = TextAlignmentOptions.Center;
                    text.GetComponent<LayoutElement>().flexibleWidth = 0;
                }
                if (name == "item_emparejamiento")
                {
                    Image audio = Icono(root.transform, "IconoAudio",
                        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Audio/icono_audio.png"), 56);
                    IconoTema(audio, 56);
                    audio.transform.SetAsFirstSibling();
                    audio.gameObject.SetActive(false);
                }
                foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
                    rect.gameObject.layer = LayerMask.NameToLayer("UI");
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static Image Icono(Transform parent, string name, Sprite sprite, float size)
        {
            Transform old = parent.Find(name);
            GameObject root = old != null ? old.gameObject : Objeto(name, parent);
            Image image = root.GetComponent<Image>() ?? root.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            image.rectTransform.sizeDelta = new Vector2(size, size);
            LayoutElement element = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
            element.minWidth = element.preferredWidth = element.minHeight = element.preferredHeight = size;
            return image;
        }

        private static void Asignar(Component component, Dictionary<string, UnityEngine.Object> fields)
        {
            var serialized = new SerializedObject(component);
            foreach (var field in fields) serialized.FindProperty(field.Key).objectReferenceValue = field.Value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Quitar<T>(GameObject root) where T : Component
        {
            T component = root.GetComponent<T>();
            if (component != null) UnityEngine.Object.DestroyImmediate(component);
        }

        private static GameObject Objeto(string name, Transform parent)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.layer = LayerMask.NameToLayer("UI");
            root.transform.SetParent(parent, false);
            return root;
        }

        private static void Stretch(RectTransform rect, float inset = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }
    }
}
#endif
