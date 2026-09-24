#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
using Assets.Scripts.UI.LeccionBase;
using Assets.Scripts.UI.LeccionBase.Actividades;
using Assets.Scripts.UI.LeccionBase.Componentes;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Editor
{
    public static partial class LeccionBaseUIBuilder
    {
        private const string ScenePath = "Assets/Scenes/03_LeccionBase.unity";
        private const string ActivitiesPath = "Assets/Content/Prefabs/Activities";
        private const string LessonPath = "Assets/Content/Prefabs/Lesson";
        private const string RegularFontPath = "Assets/Content/UI/Fonts/NotoSans-Regular SDF.asset";
        private const string BoldFontPath = "Assets/Content/UI/Fonts/NotoSans-Bold SDF.asset";
        private const string ThemePath = "Assets/Content/UI/UITheme.asset";
        private const string IconsPath = "Assets/Content/UI/Icons";
        private const string LessonIconsPath = "Assets/Content/UI/Icons/Lecciones";
        private const string StateIconsPath = "Assets/Content/UI/Icons/Estados";
        private const string NavigationIconsPath = "Assets/Content/UI/Icons/Navegacion";

        private static readonly Color Ink = new Color(0.075f, 0.12f, 0.15f, 1f);
        private static readonly Color MutedInk = new Color(0.30f, 0.34f, 0.36f, 1f);
        private static readonly Color Paper = new Color(0.985f, 0.97f, 0.91f, 1f);
        private static readonly Color SoftPaper = new Color(0.95f, 0.91f, 0.80f, 1f);
        private static readonly Color Accent = new Color(0.96f, 0.62f, 0.04f, 1f);
        private static readonly Color AccentSoft = new Color(0.93f, 0.78f, 0.43f, 1f);

        private static TMP_FontAsset _regularFont;
        private static TMP_FontAsset _boldFont;
        private static UIThemeModel _theme;

        [MenuItem("Uchumataqu/Herramientas/Lecciones/Construir LeccionBase y actividades")]
        public static void ConstruirTodo()
        {
            _regularFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RegularFontPath);
            _boldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath) ?? _regularFont;
            _theme = AssetDatabase.LoadAssetAtPath<UIThemeModel>(ThemePath);
            if (_theme == null) throw new InvalidOperationException($"No se encontró el tema en {ThemePath}.");
            UIThemeController.SetTheme(_theme);
            EnsureFolder(ActivitiesPath);
            EnsureFolder(LessonPath);

            ConstruirPages();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LeccionBaseUIBuilder] Escena y prefabs generados correctamente.");
        }

        private static FeedbackOverlayView CrearContenidoFeedback(Transform footer)
        {
            GameObject root = CrearObjetoUI("ContenidoFeedback", footer);
            Image fondo = root.AddComponent<Image>();
            ConfigurarImagenTematizada(fondo, ThemedGraphicView.RolColor.Exito);
            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 20, 140); layout.spacing = 6f;
            layout.childControlHeight = false; layout.childControlWidth = true;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            layout.childAlignment = TextAnchor.MiddleCenter;
            Image estado = CrearIcono(root.transform, "IconoEstado", "Estados/icono_correcto.png", 44f,
                ThemedGraphicView.RolColor.SobreColor);
            TextMeshProUGUI titulo = CrearTexto(root.transform, "Titulo", "¡Correcto!", 30f, FontStyles.Bold, Color.white, 48f);
            TextMeshProUGUI detalle = CrearTexto(root.transform, "Detalle", "¡Muy bien!", 23f, FontStyles.Normal, Color.white, 56f);
            FeedbackOverlayView view = root.AddComponent<FeedbackOverlayView>();
            AsignarReferencias(view, new Dictionary<string, UnityEngine.Object>
            {
                ["fondoFeedback"] = fondo, ["imgEstado"] = estado,
                ["iconoCorrecto"] = CargarIcono("Estados/icono_correcto.png"),
                ["iconoAdvertencia"] = CargarIcono("Estados/icono_advertencia.png"),
                ["iconoIncorrecto"] = CargarIcono("Estados/icono_incorrecto.png"), ["txtTitulo"] = titulo,
                ["txtMensajeDetalle"] = detalle
            });
            return view;
        }

        private static GameObject CrearPrefabResultado()
        {
            GameObject root = CrearObjetoUI("ModalResultado");
            ConfigurarImagenTematizada(root.AddComponent<Image>(), ThemedGraphicView.RolColor.Scrim);
            GameObject card = CrearContenedor("CardResultado", root.transform, 16f, new RectOffset(34, 34, 34, 34));
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0f, 0.5f); cardRect.anchorMax = new Vector2(1f, 0.5f);
            cardRect.offsetMin = new Vector2(64, -400); cardRect.offsetMax = new Vector2(-64, 400);
            ConfigurarImagenTematizada(card.AddComponent<Image>(), ThemedGraphicView.RolColor.Superficie);
            CrearIcono(card.transform, "IconoCompletado", "Estados/icono_completado.png", 72f,
                ThemedGraphicView.RolColor.Exito);
            TextMeshProUGUI titulo = CrearTexto(card.transform, "Titulo", "¡Lección completada!", 34f, FontStyles.Bold, Ink, 84f);
            TextMeshProUGUI aciertos = CrearTexto(card.transform, "Aciertos", "0 / 0 aciertos", 26f, FontStyles.Normal, Ink, 60f);
            TextMeshProUGUI precision = CrearTexto(card.transform, "Precision", "0 % de precisión", 26f, FontStyles.Normal, MutedInk, 60f);
            GameObject estrellas = CrearObjetoUI("Estrellas", card.transform);
            HorizontalLayoutGroup starLayout = estrellas.AddComponent<HorizontalLayoutGroup>();
            starLayout.spacing = 12f; starLayout.childAlignment = TextAnchor.MiddleCenter;
            starLayout.childControlWidth = true; starLayout.childForceExpandWidth = true;
            estrellas.AddComponent<LayoutElement>().preferredHeight = 84f;
            GameObject estrella1 = CrearIcono(estrellas.transform, "Estrella1", "Logros/icono_estrella.png", 72f,
                ThemedGraphicView.RolColor.Primario).gameObject;
            GameObject estrella2 = CrearIcono(estrellas.transform, "Estrella2", "Logros/icono_estrella.png", 72f,
                ThemedGraphicView.RolColor.Primario).gameObject;
            GameObject estrella3 = CrearIcono(estrellas.transform, "Estrella3", "Logros/icono_estrella.png", 72f,
                ThemedGraphicView.RolColor.Primario).gameObject;
            Button mapa = CrearBoton(card.transform, "BtnVolverMapa", "VOLVER AL MAPA", true, 72f, "Lecciones/icono_volver_mapa.png");
            ModalResultadoView view = root.AddComponent<ModalResultadoView>();
            AsignarReferencias(view, new Dictionary<string, UnityEngine.Object>
            {
                ["txtTitulo"] = titulo, ["txtAciertos"] = aciertos, ["txtPrecision"] = precision,
                ["estrella1"] = estrella1, ["estrella2"] = estrella2, ["estrella3"] = estrella3,
                ["btnVolverMapa"] = mapa
            });
            return GuardarPrefab(root, $"{LessonPath}/ModalResultado.prefab");
        }

        private static GameObject CrearPrefabConfirmacionSalida()
        {
            GameObject root = CrearObjetoUI("ConfirmacionSalida");
            ConfigurarImagenTematizada(root.AddComponent<Image>(), ThemedGraphicView.RolColor.Scrim);
            GameObject card = CrearContenedor("CardConfirmacion", root.transform, 18f, new RectOffset(34, 34, 34, 34));
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f); rect.anchorMax = new Vector2(1f, 0.5f);
            rect.offsetMin = new Vector2(64, -310); rect.offsetMax = new Vector2(-64, 310);
            ConfigurarImagenTematizada(card.AddComponent<Image>(), ThemedGraphicView.RolColor.Superficie);
            CrearIcono(card.transform, "IconoAdvertencia", "Estados/icono_advertencia.png", 64f,
                ThemedGraphicView.RolColor.Advertencia);
            CrearTexto(card.transform, "Titulo", "¿Quieres salir de la lección?", 30f, FontStyles.Bold, Ink, 86f);
            CrearTexto(card.transform, "Detalle", "Tu avance hasta este punto se guardará.", 23f, FontStyles.Normal, MutedInk, 78f);
            CrearBoton(card.transform, "BtnSeguirAprendiendo", "SEGUIR APRENDIENDO", false, 76f, "Lecciones/icono_continuar.png");
            CrearBoton(card.transform, "BtnConfirmarSalida", "SALIR", true, 72f, "Navegacion/icono_salir.png", ThemedButtonView.EstiloBoton.Error);
            return GuardarPrefab(root, $"{LessonPath}/ConfirmacionSalida.prefab");
        }

        private static void ConfigurarSlider(Scene scene)
        {
            Transform sliderTransform = BuscarEnEscena(scene, "SliderProgresoLeccion");
            Slider slider = sliderTransform != null ? sliderTransform.GetComponent<Slider>() : null;
            if (slider == null) return;
            for (int i = sliderTransform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(sliderTransform.GetChild(i).gameObject);
            GameObject background = CrearObjetoUI("Background", sliderTransform);
            ConfigurarImagenTematizada(background.AddComponent<Image>(), ThemedGraphicView.RolColor.ProgresoFondo);
            Estirar(background.GetComponent<RectTransform>());
            GameObject fillArea = CrearObjetoUI("FillArea", sliderTransform);
            Estirar(fillArea.GetComponent<RectTransform>(), 4f, 4f);
            GameObject fill = CrearObjetoUI("Fill", fillArea.transform);
            Image fillImage = fill.AddComponent<Image>();
            ConfigurarImagenTematizada(fillImage, ThemedGraphicView.RolColor.Primario);
            Estirar(fill.GetComponent<RectTransform>());
            slider.fillRect = fill.GetComponent<RectTransform>(); slider.handleRect = null;
            slider.targetGraphic = fillImage; slider.minValue = 0f; slider.maxValue = 1f;
            slider.value = 0f; slider.interactable = false; slider.transition = Selectable.Transition.None;
        }

        private static void ConfigurarShellEscena(Scene scene, RectTransform safeArea)
        {
            Transform background = safeArea.Find("Background");
            if (background == null)
            {
                GameObject backgroundObject = CrearObjetoUI("Background", safeArea);
                background = backgroundObject.transform;
            }
            background.SetAsFirstSibling();
            Image backgroundImage = background.GetComponent<Image>() ?? background.gameObject.AddComponent<Image>();
            backgroundImage.raycastTarget = false;
            ThemedGraphicView fondoTematizado = backgroundImage.GetComponent<ThemedGraphicView>();
            if (fondoTematizado != null) UnityEngine.Object.DestroyImmediate(fondoTematizado);
            backgroundImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/Imagenes/Fondos/fondo_principal.png");
            backgroundImage.color = Color.white;
            backgroundImage.type = Image.Type.Simple;
            Estirar(background.GetComponent<RectTransform>());

            Transform header = BuscarEnEscena(scene, "HeaderLeccion");
            Transform titleTransform = BuscarEnEscena(scene, "TxtTituloLeccion");
            Transform counterTransform = BuscarEnEscena(scene, "TxtPasoContador");
            Transform backTransform = BuscarEnEscena(scene, "BtnAtrasLeccion");
            Transform sliderTransform = BuscarEnEscena(scene, "SliderProgresoLeccion");
            if (header == null || titleTransform == null || backTransform == null || sliderTransform == null)
                throw new InvalidOperationException("El header de 03_LeccionBase está incompleto.");

            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 0.86f);
            headerRect.anchorMax = Vector2.one;
            headerRect.offsetMin = new Vector2(24f, 0f);
            headerRect.offsetMax = new Vector2(-24f, -12f);

            RectTransform backRect = backTransform.GetComponent<RectTransform>();
            backRect.anchorMin = backRect.anchorMax = new Vector2(0f, 0.62f);
            backRect.pivot = new Vector2(0f, 0.5f);
            backRect.anchoredPosition = Vector2.zero;
            backRect.sizeDelta = new Vector2(64f, 64f);
            Image backBackground = backTransform.GetComponent<Image>() ?? backTransform.gameObject.AddComponent<Image>();
            Button backButton = backTransform.GetComponent<Button>() ?? backTransform.gameObject.AddComponent<Button>();
            backButton.targetGraphic = backBackground;
            ThemedButtonView themedBack = backTransform.GetComponent<ThemedButtonView>() ?? backTransform.gameObject.AddComponent<ThemedButtonView>();
            themedBack.Estilo = ThemedButtonView.EstiloBoton.Primario;
            for (int i = backTransform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(backTransform.GetChild(i).gameObject);
            Image backIcon = CrearIcono(backTransform, "IconoAtras", "Navegacion/icono_atras.png", 34f,
                ThemedGraphicView.RolColor.SobreColor);
            RectTransform backIconRect = backIcon.rectTransform;
            backIconRect.anchorMin = backIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            backIconRect.anchoredPosition = Vector2.zero;
            backIconRect.sizeDelta = new Vector2(34f, 34f);

            TMP_Text title = titleTransform.GetComponent<TMP_Text>();
            title.text = "Saludos Básicos";
            title.alignment = TextAlignmentOptions.MidlineLeft;
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.overflowMode = TextOverflowModes.Truncate;
            title.fontStyle = FontStyles.Bold;
            ThemedTextView themedTitle = titleTransform.GetComponent<ThemedTextView>() ?? titleTransform.gameObject.AddComponent<ThemedTextView>();
            themedTitle.tipoTexto = ThemedTextView.TipoTexto.Subtitulo;
            themedTitle.tipoColor = ThemedTextView.ColorTexto.Primario;
            themedTitle.usarColorPersonalizado = false;
            themedTitle.AplicarTema();
            RectTransform titleRect = titleTransform.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.42f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(84f, 0f);
            titleRect.offsetMax = Vector2.zero;

            if (counterTransform != null)
                UnityEngine.Object.DestroyImmediate(counterTransform.gameObject);

            RectTransform progressRect = sliderTransform.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0f, 0.12f);
            progressRect.anchorMax = new Vector2(1f, 0.27f);
            progressRect.offsetMin = new Vector2(84f, 0f);
            progressRect.offsetMax = Vector2.zero;
        }

        private static T InstanciarOverlay<T>(GameObject prefab, Transform parent, string nombre) where T : Component
        {
            GameObject instance = InstanciarOverlay(prefab, parent, nombre);
            T component = instance.GetComponent<T>();
            if (component == null) throw new InvalidOperationException($"El prefab {prefab.name} no contiene {typeof(T).Name}.");
            return component;
        }

        private static GameObject InstanciarOverlay(GameObject prefab, Transform parent, string nombre)
        {
            Transform existente = parent.Find(nombre);
            if (existente != null) UnityEngine.Object.DestroyImmediate(existente.gameObject);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = nombre; instance.transform.SetAsLastSibling();
            return instance;
        }

        private static GameObject CrearContenedor(string nombre, Transform parent, float spacing, RectOffset padding)
        {
            GameObject go = CrearObjetoUI(nombre, parent);
            VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing; layout.padding = padding;
            layout.childControlHeight = false; layout.childControlWidth = true;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            return go;
        }

        private static Button CrearBoton(Transform parent, string nombre, string etiqueta, bool secundario,
            float altura = 76f, string rutaIcono = null, ThemedButtonView.EstiloBoton? estiloForzado = null,
            float tamanoIcono = 40f)
        {
            GameObject go = CrearObjetoUI(nombre, parent);
            Image image = go.AddComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Base/ui_button_rounded_24.png");
            image.type = Image.Type.Sliced;
            Button button = go.AddComponent<Button>(); button.targetGraphic = image;
            ThemedButtonView themedButton = go.AddComponent<ThemedButtonView>();
            themedButton.Estilo = estiloForzado ?? (secundario
                ? ThemedButtonView.EstiloBoton.Suave
                : ThemedButtonView.EstiloBoton.Primario);
            HorizontalLayoutGroup horizontal = go.AddComponent<HorizontalLayoutGroup>();
            horizontal.padding = new RectOffset(20, 20, 8, 8);
            horizontal.spacing = 12f;
            horizontal.childAlignment = TextAnchor.MiddleCenter;
            horizontal.childControlHeight = true;
            horizontal.childControlWidth = true;
            horizontal.childForceExpandHeight = false;
            horizontal.childForceExpandWidth = false;
            LayoutElement element = go.AddComponent<LayoutElement>(); element.minHeight = altura; element.preferredHeight = altura;
            if (!string.IsNullOrWhiteSpace(rutaIcono))
                CrearIcono(go.transform, "Icono", rutaIcono, tamanoIcono, secundario
                    ? ThemedGraphicView.RolColor.Primario
                    : ThemedGraphicView.RolColor.SobreColor);
            bool textoClaro = !secundario || themedButton.Estilo == ThemedButtonView.EstiloBoton.Error;
            TextMeshProUGUI text = CrearTexto(go.transform, "Texto", etiqueta, 23f, FontStyles.Bold,
                textoClaro ? Color.white : Ink, altura - 16f);
            LayoutElement textLayout = text.GetComponent<LayoutElement>();
            textLayout.flexibleWidth = 1f;
            return button;
        }

        private static TextMeshProUGUI CrearTexto(Transform parent, string nombre, string contenido,
            float tamano, FontStyles estilo, Color color, float altura)
        {
            GameObject go = CrearObjetoUI(nombre, parent);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = contenido; text.font = estilo.HasFlag(FontStyles.Bold) ? _boldFont : _regularFont;
            text.fontSize = tamano; text.fontStyle = estilo; text.color = color;
            text.alignment = TextAlignmentOptions.Center; text.textWrappingMode = TextWrappingModes.Normal;
            ThemedTextView themedText = go.AddComponent<ThemedTextView>();
            themedText.tipoTexto = ObtenerTipoTexto(tamano);
            themedText.tipoColor = ColorSimilar(color, Color.white)
                ? ThemedTextView.ColorTexto.SobreColor
                : ColorSimilar(color, MutedInk)
                    ? ThemedTextView.ColorTexto.Secundario
                    : ColorSimilar(color, Accent)
                        ? ThemedTextView.ColorTexto.Terciario
                        : ThemedTextView.ColorTexto.Primario;
            LayoutElement layout = go.AddComponent<LayoutElement>(); layout.minHeight = altura; layout.preferredHeight = altura;
            return text;
        }

        private static TextMeshProUGUI CrearEncabezadoActividad(Transform parent, string nombreTexto,
            string texto, string rutaIcono)
        {
            GameObject row = CrearObjetoUI($"Encabezado{nombreTexto}", parent);
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(4, 4, 2, 2);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = false;
            row.AddComponent<LayoutElement>().preferredHeight = 72f;
            CrearIcono(row.transform, "IconoActividad", rutaIcono, 44f, ThemedGraphicView.RolColor.Primario);
            TextMeshProUGUI label = CrearTexto(row.transform, nombreTexto, texto, 25f, FontStyles.Bold, Ink, 68f);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.GetComponent<LayoutElement>().flexibleWidth = 1f;
            return label;
        }

        private static void CrearEtiquetaColumna(Transform parent, string texto, string rutaIcono)
        {
            GameObject row = CrearObjetoUI("EtiquetaColumna", parent);
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            row.AddComponent<LayoutElement>().preferredHeight = 52f;
            CrearIcono(row.transform, "Icono", rutaIcono, 30f, ThemedGraphicView.RolColor.Primario);
            TextMeshProUGUI label = CrearTexto(row.transform, "Texto", texto, 20f, FontStyles.Bold, MutedInk, 48f);
            label.GetComponent<LayoutElement>().preferredWidth = 180f;
        }

        private static GameObject CrearTarjeta(string nombre, Transform parent, ThemedGraphicView.RolColor rol)
        {
            GameObject card = CrearContenedor(nombre, parent, 8f, new RectOffset(20, 20, 18, 18));
            ConfigurarImagenTematizada(card.AddComponent<Image>(), rol);
            return card;
        }

        private static Image CrearIcono(Transform parent, string nombre, string rutaRelativa,
            float tamano, ThemedGraphicView.RolColor? colorTematico = null)
        {
            GameObject go = CrearObjetoUI(nombre, parent);
            Image image = go.AddComponent<Image>();
            image.sprite = CargarIcono(rutaRelativa);
            image.preserveAspect = true;
            image.raycastTarget = false;
            if (colorTematico.HasValue)
                ConfigurarImagenTematizada(image, colorTematico.Value);
            LayoutElement layout = go.AddComponent<LayoutElement>();
            layout.minWidth = tamano; layout.preferredWidth = tamano;
            layout.minHeight = tamano; layout.preferredHeight = tamano;
            return image;
        }

        private static Sprite CargarIcono(string rutaRelativa)
        {
            string ruta = $"{IconsPath}/{rutaRelativa}";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            if (sprite == null) throw new InvalidOperationException($"No se encontró el icono {ruta}.");
            return sprite;
        }

        private static void ConfigurarImagenTematizada(Image image, ThemedGraphicView.RolColor rol)
        {
            ThemedGraphicView themed = image.GetComponent<ThemedGraphicView>() ?? image.gameObject.AddComponent<ThemedGraphicView>();
            themed.Rol = rol;
            themed.AplicarTema();
        }

        private static ThemedTextView.TipoTexto ObtenerTipoTexto(float tamano)
        {
            if (tamano >= 44f) return ThemedTextView.TipoTexto.Titulo;
            if (tamano >= 30f) return ThemedTextView.TipoTexto.Subtitulo;
            if (tamano <= 20f) return ThemedTextView.TipoTexto.Pequeño;
            return ThemedTextView.TipoTexto.Normal;
        }

        private static bool ColorSimilar(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) < 0.01f && Mathf.Abs(a.g - b.g) < 0.01f && Mathf.Abs(a.b - b.b) < 0.01f;

        private static GameObject CrearObjetoUI(string nombre, Transform parent = null)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject GuardarPrefab(GameObject root, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            if (prefab == null) throw new InvalidOperationException($"No se pudo guardar el prefab {path}.");
            return prefab;
        }

        private static void AsignarReferencias(Component target, IReadOnlyDictionary<string, UnityEngine.Object> references)
        {
            SerializedObject serialized = new SerializedObject(target);
            foreach (KeyValuePair<string, UnityEngine.Object> pair in references)
            {
                SerializedProperty property = serialized.FindProperty(pair.Key);
                if (property == null) throw new InvalidOperationException($"No existe el campo serializado '{pair.Key}' en {target.GetType().Name}.");
                property.objectReferenceValue = pair.Value;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AsignarLista<T>(Component target, string propertyName, IReadOnlyList<T> values) where T : UnityEngine.Object
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null) throw new InvalidOperationException($"No existe la lista serializada '{propertyName}' en {target.GetType().Name}.");
            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform BuscarEnEscena(Scene scene, string nombre)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                    if (transform.name == nombre) return transform;
            return null;
        }

        private static void Estirar(RectTransform rect, float horizontal = 0f, float vertical = 0f)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(horizontal, vertical); rect.offsetMax = new Vector2(-horizontal, -vertical);
        }

        private static void AnclarSuperior(RectTransform rect, float izquierda, float derecha, float superior, float altura)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(izquierda, -superior - altura);
            rect.offsetMax = new Vector2(-derecha, -superior);
        }

        private static void AnclarInferior(RectTransform rect, float izquierda, float derecha, float inferior, float altura)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(izquierda, inferior);
            rect.offsetMax = new Vector2(-derecha, inferior + altura);
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
