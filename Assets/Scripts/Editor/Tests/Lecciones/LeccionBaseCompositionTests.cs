using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Lecciones;
using Assets.Scripts.Persistencia;
using Assets.Scripts.UI.LeccionBase;
using Assets.Scripts.UI.LeccionBase.Actividades;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Editor.Tests
{
    public class LeccionBaseCompositionTests
    {
        [Test]
        public void BancoDePalabras_CentraFichasYConservaOrdenConTokensRepetidos()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(ScenePath);
            try
            {
                WordBankView view = scene.GetRootGameObjects()
                    .SelectMany(r => r.GetComponentsInChildren<WordBankView>(true)).Single();
                view.Configurar("ordenar_oracion", "", new List<string> { "la", "la", "casa" });
                Transform destino = Referencia<Transform>(view, "contenedorDestino");
                Transform origen = Referencia<Transform>(view, "contenedorOrigen");
                TMP_Text vacia = Referencia<TMP_Text>(view, "txtRespuestaVacia");
                Assert.IsTrue(vacia.gameObject.activeSelf);
                Button[] repetidas = origen.GetComponentsInChildren<Button>(true)
                    .Where(b => b.GetComponentInChildren<TMP_Text>().text == "la").ToArray();
                repetidas[0].onClick.Invoke();
                repetidas[1].onClick.Invoke();
                Assert.IsFalse(vacia.gameObject.activeSelf);
                Assert.IsTrue(repetidas.All(b => b.transform.IsChildOf(destino)));
                foreach (HorizontalLayoutGroup fila in destino.GetComponentsInChildren<HorizontalLayoutGroup>())
                    Assert.AreEqual(TextAnchor.MiddleCenter, fila.childAlignment);
                repetidas[0].onClick.Invoke();
                Assert.IsTrue(repetidas[0].transform.IsChildOf(origen));
                repetidas[0].onClick.Invoke();
                List<string> enviada = null;
                int envios = 0;
                view.OnTokensComprobados += tokens => { enviada = tokens; envios++; };
                view.Comprobar();
                view.Comprobar();
                CollectionAssert.AreEqual(new[] { "la", "la" }, enviada);
                Assert.AreEqual(1, envios);
                Assert.IsFalse(view.PuedeEnviarRespuesta);
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }
        [Test]
        public void PagesYPrefabs_TienenTipografiaIconosYBotonesTematizados()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(ScenePath);
            try
            {
                Transform host = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                    .Single(t => t.name == "ActividadHost");
                foreach (Transform page in host)
                {
                    foreach (TMP_Text text in page.GetComponentsInChildren<TMP_Text>(true))
                    {
                        var themed = text.GetComponent<Assets.Scripts.Core.UI.Components.ThemedTextView>();
                        Assert.IsNotNull(themed, page.name + "/" + text.name);
                        Assert.IsFalse(themed.usarColorPersonalizado, text.name);
                    }
                    Image icon = page.GetComponentsInChildren<Image>(true).Single(i => i.name == "IconoActividad");
                    Assert.IsNotNull(icon.sprite, page.name);
                    Assert.IsTrue(icon.preserveAspect, page.name);
                    Assert.AreEqual(96, icon.GetComponent<LayoutElement>().preferredWidth, page.name);
                    var title = icon.transform.parent.GetComponentInChildren<Assets.Scripts.Core.UI.Components.ThemedTextView>();
                    Assert.AreEqual(Assets.Scripts.Core.UI.Components.ThemedTextView.TipoTexto.Titulo, title.tipoTexto, page.name);
                    foreach (Button button in page.GetComponentsInChildren<Button>(true))
                    {
                        Assert.IsNotNull(button.GetComponent<Image>().sprite, page.name + "/" + button.name);
                        Assert.IsTrue(button.GetComponent<Image>().raycastTarget, button.name);
                    }
                }
                foreach (string name in new[] { "fila_forma_familia", "opcion_respuesta", "item_emparejamiento", "tarjeta_memoria", "ficha_wordbank" })
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ElementsPath}/{name}.prefab");
                    foreach (TMP_Text text in prefab.GetComponentsInChildren<TMP_Text>(true))
                        Assert.IsNotNull(text.GetComponent<Assets.Scripts.Core.UI.Components.ThemedTextView>(), name + "/" + text.name);
                    foreach (Button button in prefab.GetComponentsInChildren<Button>(true))
                    {
                        Assert.IsTrue(button.GetComponent<Image>().raycastTarget, name + "/" + button.name);
                        Assert.IsNotNull(button.GetComponent<Image>().sprite, name + "/" + button.name);
                        Assert.IsTrue(button.GetComponent<Assets.Scripts.Core.UI.Components.ThemedButtonView>() != null ||
                            button.GetComponent<TarjetaSeleccionView>() != null, name);
                    }
                }
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        [Test]
        public void Tarjetas_ConservanSeleccionYResultadoAlCambiarTema()
        {
            var original = AssetDatabase.LoadAssetAtPath<Assets.Scripts.Core.UI.Theme.UIThemeModel>("Assets/Content/UI/UITheme.asset");
            var previo = Assets.Scripts.Core.UI.Theme.UIThemeController.Theme;
            var alternate = Object.Instantiate(original);
            GameObject option = null, pair = null;
            try
            {
                Assets.Scripts.Core.UI.Theme.UIThemeController.SetTheme(original);
                option = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>($"{ElementsPath}/opcion_respuesta.prefab"));
                pair = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>($"{ElementsPath}/item_emparejamiento.prefab"));
                var selection = option.GetComponent<TarjetaSeleccionView>();
                selection.MostrarSeleccion(true);
                var themed = pair.GetComponent<Assets.Scripts.Core.UI.Components.ThemedButtonView>();
                Image pairImage = pair.GetComponent<Image>();
                Sprite manualSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Base/ui_panel_shadow_arriba.png");
                pairImage.sprite = manualSprite;
                pairImage.type = Image.Type.Simple;
                pairImage.pixelsPerUnitMultiplier = 3f;
                themed.MostrarEstado(Assets.Scripts.Core.UI.Components.ThemedButtonView.EstadoBoton.Correcto);
                pair.GetComponent<Button>().interactable = false;
                alternate.colorExito = new Color(0.2f, 0.4f, 0.8f);
                alternate.escalaTextoGlobal = 1.2f;
                Assets.Scripts.Core.UI.Theme.UIThemeController.SetTheme(alternate);
                Assert.AreEqual(alternate.colorExito, option.transform.Find("Marco").GetComponent<Image>().color);
                Assert.AreEqual(alternate.textoNormal * 1.2f, option.GetComponentInChildren<TMP_Text>().fontSize, 0.01f);
                Assert.AreEqual(FontStyles.Bold, option.GetComponentInChildren<TMP_Text>().fontStyle);
                Assert.AreSame(manualSprite, pairImage.sprite);
                Assert.AreEqual(Image.Type.Simple, pairImage.type);
                Assert.AreEqual(3f, pairImage.pixelsPerUnitMultiplier);
                Assert.AreEqual(Color.Lerp(alternate.colorSuperficie, alternate.colorExito, 0.18f), pair.GetComponent<Button>().colors.disabledColor);
                selection.MostrarResultado(false);
                Assert.AreEqual(alternate.colorError, option.transform.Find("Marco").GetComponent<Image>().color);
                selection.MostrarSeleccion(false);
                Assert.AreEqual(alternate.colorBorde, option.transform.Find("Marco").GetComponent<Image>().color);
            }
            finally
            {
                if (option != null) Object.DestroyImmediate(option);
                if (pair != null) Object.DestroyImmediate(pair);
                Assets.Scripts.Core.UI.Theme.UIThemeController.SetTheme(previo);
                Object.DestroyImmediate(alternate);
            }
        }
        private const string ScenePath = "Assets/Scenes/03_LeccionBase.unity";
        private const string ElementsPath = "Assets/Content/Prefabs/Actividades";

        [Test]
        public void ElementosDinamicos_TienenReferenciasYTransformacionesValidas()
        {
            foreach (string nombre in new[] { "fila_forma_familia", "opcion_respuesta", "item_emparejamiento", "tarjeta_memoria", "ficha_wordbank" })
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ElementsPath}/{nombre}.prefab");
                Assert.IsNotNull(prefab, nombre);
                Assert.IsNotEmpty(prefab.GetComponentsInChildren<Button>(true));
                foreach (TMP_Text texto in prefab.GetComponentsInChildren<TMP_Text>(true)) Assert.IsNotNull(texto.font, nombre);
                ValidarTransformaciones(prefab);
            }
        }

        [Test]
        public void Pages_CubrenCorpusYFormatosMoviles() => ValidarPages(false);

        [Test]
        public void Footer_UnificaPresentacionYDespachaSoloAccionesHabilitadas()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(ScenePath);
            try
            {
                LeccionBaseView view = scene.GetRootGameObjects()
                    .SelectMany(r => r.GetComponentsInChildren<LeccionBaseView>(true)).Single();
                LeccionBasePresenter presenter = scene.GetRootGameObjects()
                    .SelectMany(r => r.GetComponentsInChildren<LeccionBasePresenter>(true)).Single();
                Assert.IsTrue(view.ValidarConfiguracion(out string error), error);
                Button principal = Referencia<Button>(view, "btnPrincipal");
                Assert.AreEqual(3, principal.transform.parent.childCount);
                Assert.AreEqual(0, view.Feedback.GetComponentsInChildren<Button>(true).Length);
                Assert.IsFalse(PrefabUtility.IsPartOfPrefabInstance(view.Feedback));
                TMP_Text texto = Referencia<TMP_Text>(view, "txtAccionPrincipal");
                Image icono = Referencia<Image>(view, "iconoAccionPrincipal");
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                var configurar = typeof(LeccionBasePresenter).GetMethod("ConfigurarAccionPrincipal", flags);
                var ejecutar = typeof(LeccionBasePresenter).GetMethod("EjecutarAccionPrincipal", flags);
                int acciones = 0;
                bool habilitado = false;
                foreach (LeccionBaseView.AccionPrincipal estado in Enum.GetValues(typeof(LeccionBaseView.AccionPrincipal)))
                {
                    configurar.Invoke(presenter, new object[] { estado, (Action)(() => acciones++), (Func<bool>)(() => habilitado) });
                    Assert.AreEqual(estado.ToString().ToUpperInvariant(), texto.text);
                    string campoIcono = estado == LeccionBaseView.AccionPrincipal.Continuar ? "iconoContinuar" :
                        estado == LeccionBaseView.AccionPrincipal.Comprobar ? "iconoComprobar" : "iconoFinalizar";
                    Assert.AreSame(Referencia<Sprite>(view, campoIcono), icono.sprite);
                    Assert.IsFalse(principal.interactable);
                    int antes = acciones;
                    ejecutar.Invoke(presenter, null);
                    Assert.AreEqual(antes, acciones);
                    habilitado = true;
                    typeof(LeccionBasePresenter).GetMethod("ActualizarAccionPrincipal", flags).Invoke(presenter, null);
                    Assert.IsTrue(principal.interactable);
                    ejecutar.Invoke(presenter, null);
                    Assert.AreEqual(antes + 1, acciones);
                    presenter.enabled = false;
                    ejecutar.Invoke(presenter, null);
                    Assert.AreEqual(antes + 1, acciones);
                    presenter.enabled = true;
                    habilitado = false;
                }
                view.MostrarOmitirASR(true);
                Assert.IsTrue(Referencia<Button>(view, "btnOmitirASR").gameObject.activeSelf);
                view.MostrarOmitirASR(false);
                Assert.IsFalse(Referencia<Button>(view, "btnOmitirASR").gameObject.activeSelf);
                Assert.AreSame(principal.transform.parent, view.Feedback.transform.parent);
                Assert.AreEqual(0, view.Feedback.transform.GetSiblingIndex());
                view.Feedback.MostrarCorrecto();
                Assert.IsTrue(view.Feedback.gameObject.activeSelf);
                Assert.IsTrue(principal.gameObject.activeSelf);
                view.Feedback.Ocultar();
                Assert.IsFalse(view.Feedback.gameObject.activeSelf);
                Assert.IsTrue(principal.gameObject.activeSelf);
                Assert.Less(principal.transform.parent.GetSiblingIndex(), view.ModalResultado.transform.GetSiblingIndex());
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        public static string ValidarPages(bool capturar)
        {
            Assert.IsTrue(CorpusRepository.Inicializar());
            var pasos = CorpusRepository.ObtenerTodasLecciones().SelectMany(l => l.pasos).ToList();
            Assert.AreEqual(19, pasos.Select(p => p.tipo).Distinct().Count());
            foreach (PasoLeccionModel paso in pasos)
            {
                Assert.IsTrue(TipoPasoLeccion.EsSoportado(paso.tipo));
                if (TipoPasoLeccion.EsSeleccion(paso.tipo)) Assert.That(GestorActividadLeccion.CrearSeleccion(paso).Opciones.Count, Is.InRange(2, 4));
                if (TipoPasoLeccion.EsWordBank(paso.tipo))
                {
                    var datos = GestorActividadLeccion.CrearWordBank(paso);
                    Assert.IsNotEmpty(datos.Tokens);
                    if (paso.tipo == TipoPasoLeccion.TraduccionConstructiva)
                        Assert.AreEqual(CorpusRepository.ObtenerOracion(paso.oracion_id).uch, datos.Prompt);
                }
            }
            Scene preview = EditorSceneManager.OpenPreviewScene(ScenePath);
            RenderTexture target = null;
            try
            {
                var roots = preview.GetRootGameObjects();
                LeccionBaseView view = roots.SelectMany(r => r.GetComponentsInChildren<LeccionBaseView>(true)).Single();
                Assert.IsTrue(view.ValidarConfiguracion(out string error), error);
                Canvas canvas = roots.SelectMany(r => r.GetComponentsInChildren<Canvas>(true)).Single();
                Camera camera = roots.SelectMany(r => r.GetComponentsInChildren<Camera>(true)).Single();
                camera.enabled = false; camera.scene = preview; camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(preview);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.white;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                var scaler = canvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; scaler.scaleFactor = 1;
                var capturadas = new HashSet<string>();
                foreach (int alto in new[] { 1920, 2340 })
                {
                    target = new RenderTexture(1080, alto, 24);
                    target.Create(); camera.targetTexture = target;
                    Canvas.ForceUpdateCanvases();
                    foreach (PasoLeccionModel paso in pasos.GroupBy(p => p.tipo).Select(g => g.Last()))
                    {
                        Configurar(view, paso);
                        Canvas.ForceUpdateCanvases();
                        RectTransform host = (RectTransform)new SerializedObject(view).FindProperty("actividadContainer").objectReferenceValue;
                        var pages = host.Cast<Transform>().ToArray();
                        Assert.AreEqual(6, pages.Length);
                        Assert.AreEqual(1, pages.Count(p => p.gameObject.activeSelf), paso.tipo);
                        GameObject page = pages.Single(p => p.gameObject.activeSelf).gameObject;
                        for (int i = 0; i < 3; i++) page.GetComponent<ActividadLayoutController>().SolicitarReconstruccion();
                        ValidarTransformaciones(canvas.gameObject);
                        ScrollRect scroll = page.GetComponentInChildren<ScrollRect>();
                        Assert.Greater(scroll.viewport.rect.height, 100, paso.tipo);
                        foreach (TMP_Text text in page.GetComponentsInChildren<TMP_Text>())
                            Assert.Greater(text.rectTransform.rect.width, 0, paso.tipo + ": " + text.name);
                        if (capturar && capturadas.Add(page.name + alto)) Capturar(camera, target, page.name + "-" + alto);
                        if (capturar && capturadas.Add(page.name + alto + "-interaccion"))
                        {
                            if (page.TryGetComponent(out SeleccionMultipleView seleccion))
                            {
                                page.GetComponentsInChildren<Button>().First(b => b.GetComponent<TarjetaSeleccionView>() != null).onClick.Invoke();
                                Capturar(camera, target, page.name + "-seleccionada-" + alto);
                            }
                            else if (page.TryGetComponent(out WordBankView banco))
                            {
                                Transform origen = Referencia<Transform>(banco, "contenedorOrigen");
                                foreach (Button ficha in origen.GetComponentsInChildren<Button>().Take(2).ToArray()) ficha.onClick.Invoke();
                                page.GetComponent<ActividadLayoutController>().SolicitarReconstruccion();
                                Capturar(camera, target, page.name + "-respuesta-" + alto);
                            }
                        }
                    }
                    camera.targetTexture = null; target.Release(); Object.DestroyImmediate(target); target = null;
                }
                return $"{pasos.Count} pasos validados; 19 tipos renderizados en 1080x1920 y 1080x2340; seis Pages y cinco prefabs.";
            }
            finally
            {
                if (target != null) { target.Release(); Object.DestroyImmediate(target); }
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        public static void Configurar(LeccionBaseView view, PasoLeccionModel paso)
        {
            if (TipoPasoLeccion.EsPresentacion(paso.tipo)) view.InstanciarPresentacion().Configurar(paso, CorpusRepository.ObtenerPalabra(paso.objetivo_id));
            else if (TipoPasoLeccion.EsSeleccion(paso.tipo))
            {
                var d = GestorActividadLeccion.CrearSeleccion(paso);
                view.InstanciarSeleccionMultiple().Configurar(d.Tipo, d.Prompt, d.Opciones, d.AudioPalabraId);
            }
            else if (paso.tipo == TipoPasoLeccion.MemoriaUchEspanol) view.InstanciarMemoria().Configurar(GestorActividadLeccion.CrearPares(paso));
            else if (TipoPasoLeccion.EsEmparejamiento(paso.tipo))
                view.InstanciarEmparejamiento().Configurar(GestorActividadLeccion.CrearPares(paso), paso.tipo == TipoPasoLeccion.EmparejarAudioUch);
            else if (TipoPasoLeccion.EsWordBank(paso.tipo))
            {
                var d = GestorActividadLeccion.CrearWordBank(paso);
                view.InstanciarWordBank().Configurar(d.Tipo, d.Prompt, d.Tokens, d.AudioOracionId);
            }
            else if (paso.tipo == TipoPasoLeccion.EscuchaYRepiteAsr) view.InstanciarASR().Configurar("Chhaq", false);
            else Assert.Fail(paso.tipo);
        }

        public static void ValidarInteracciones(LeccionBaseView view)
        {
            Assert.IsTrue(Application.isPlaying, "Esta comprobación necesita Play Mode.");
            int envios = 0;
            for (int vuelta = 0; vuelta < 3; vuelta++)
            {
                SeleccionMultipleView seleccion = view.InstanciarSeleccionMultiple();
                seleccion.Configurar("uch_a_es", "Chhaq", new List<string> { "Grande", "Pequeño" });
                seleccion.OnRespuestaEnviada += _ => envios++;
                Button opcion = seleccion.GetComponentsInChildren<Button>().First(b => b.GetComponentInChildren<TMP_Text>().text == "Grande");
                opcion.onClick.Invoke();
                Assert.IsTrue(seleccion.PuedeEnviarRespuesta);
                seleccion.ConfirmarRespuesta(); seleccion.ConfirmarRespuesta();
                Assert.IsFalse(seleccion.PuedeEnviarRespuesta);
                Assert.AreEqual(vuelta + 1, envios, "Sin eventos acumulados ni doble envío.");
            }
            WordBankView banco = view.InstanciarWordBank();
            banco.Configurar("traduccion_constructiva", "Prueba", new List<string> { "la", "la", "casa" });
            List<string> respuesta = null;
            banco.OnTokensComprobados += tokens => respuesta = tokens;
            Button[] fichas = banco.GetComponentsInChildren<Button>().Where(b => b.GetComponentInChildren<TMP_Text>().text == "la").ToArray();
            Assert.AreEqual(2, fichas.Length);
            fichas[0].onClick.Invoke(); fichas[1].onClick.Invoke();
            fichas[0].onClick.Invoke(); fichas[0].onClick.Invoke();
            Assert.IsTrue(banco.PuedeEnviarRespuesta);
            banco.Comprobar(); banco.Comprobar();
            Assert.IsFalse(banco.PuedeEnviarRespuesta);
            CollectionAssert.AreEqual(new[] { "la", "la" }, respuesta);

            EmparejamientoView pares = view.InstanciarEmparejamiento();
            pares.Configurar(new Dictionary<string, string> { ["Chhaq"] = "Grande" });
            bool completado = false;
            pares.OnParPropuesto += (a, b) => pares.MostrarResultadoPar(a == "Chhaq" && b == "Grande");
            pares.OnEmparejamientoCompletado += ok => completado = ok;
            foreach (Button boton in pares.GetComponentsInChildren<Button>()) boton.onClick.Invoke();
            Assert.IsTrue(completado);

            MemoriaView memoria = view.InstanciarMemoria();
            memoria.Configurar(new Dictionary<string, string> { ["Chhaq"] = "Grande" });
            completado = false;
            memoria.OnParPropuesto += (a, b) => memoria.MostrarResultadoPar(a == b);
            memoria.OnMemoriaCompletada += ok => completado = ok;
            foreach (Button boton in memoria.GetComponentsInChildren<Button>()) boton.onClick.Invoke();
            Assert.IsTrue(completado);

            ASRView asr = view.InstanciarASR();
            asr.Configurar("Chhaq", false);
            bool omitido = false;
            asr.OnOmitirClicked += () => omitido = true;
            Referencia<Button>(asr, "btnOmitir").onClick.Invoke();
            Assert.IsTrue(omitido);
        }

        private static T Referencia<T>(Component componente, string campo) where T : Object =>
            (T)new SerializedObject(componente).FindProperty(campo).objectReferenceValue;

        private static void ValidarTransformaciones(GameObject root)
        {
            foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
            {
                foreach (float valor in new[] { rect.anchorMin.x, rect.anchorMin.y, rect.anchorMax.x, rect.anchorMax.y, rect.pivot.x, rect.pivot.y })
                    Assert.IsTrue(valor == 0 || valor == .5f || valor == 1, rect.name + ": " + valor);
                // Canvas applies its own screen conversion; descendants retain unit scale.
                if (rect.GetComponent<Canvas>() == null) Assert.AreEqual(Vector3.one, rect.localScale, rect.name);
                Assert.Less(Quaternion.Angle(Quaternion.identity, rect.localRotation), .01f, rect.name);
            }
        }

        private static void Capturar(Camera camera, RenderTexture target, string nombre)
        {
            string folder = "Temp/LeccionBaseQA";
            Directory.CreateDirectory(folder);
            camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            Texture2D image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
            File.WriteAllBytes($"{folder}/{nombre}.png", image.EncodeToPNG());
            Object.DestroyImmediate(image); RenderTexture.active = previous;
        }
    }
}
