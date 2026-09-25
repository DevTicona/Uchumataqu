using System;
using System.Collections;
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
    public sealed class LeccionBaseVisualTests
    {
        private const string ScenePath = "Assets/Scenes/03_LeccionBase.unity";
        private static string _reportPath;
        private static IEnumerator _auditoria;
        private static int _ultimoFrame;
        private static readonly List<string> Resultados = new List<string>();
        public static string EstadoAuditoria { get; private set; } = "Sin iniciar";
        private static bool _pausaAnterior;

        [Test]
        public void Vista_ReservaSoloElEspacioDelFooterVisible()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(ScenePath);
            RenderTexture target = null;
            try
            {
                var roots = scene.GetRootGameObjects();
                var view = Encontrar<LeccionBaseView>(roots);
                var canvas = Encontrar<Canvas>(roots);
                var camera = Encontrar<Camera>(roots);
                camera.scene = scene;
                camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1;
                target = new RenderTexture(1080, 1920, 24);
                target.Create();
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                var host = Referencia<RectTransform>(view, "actividadContainer");
                view.OcultarFeedback();
                view.PresentarAccionPrincipal(LeccionBaseView.AccionPrincipal.Comprobar, true);
                float normal = host.offsetMin.y;
                view.MostrarFeedback("verde", "Correcto");
                Assert.Greater(host.offsetMin.y, normal + 100f);
                view.OcultarFeedback();
                Assert.AreEqual(normal, host.offsetMin.y, 1f);
                view.PresentarAccionPrincipal(LeccionBaseView.AccionPrincipal.Comprobar, false, false);
                Assert.AreEqual(28f, host.offsetMin.y, 1f);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
                if (target != null) { target.Release(); Object.DestroyImmediate(target); }
            }
        }

        [Test]
        public void Audio_UsaBotonesIndependientesConIconoAudio()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(ScenePath);
            try
            {
                var roots = scene.GetRootGameObjects();
                ValidarAudio(Encontrar<SeleccionMultipleView>(roots), "btnAudioPregunta");
                ValidarAudio(Encontrar<WordBankView>(roots), "btnAudioPrompt");
                ValidarAudio(Encontrar<ASRView>(roots), "btnAudioReferencia");
                ValidarAudio(Encontrar<PresentacionFamiliaView>(roots), "btnAudio");
                var seleccion = Encontrar<SeleccionMultipleView>(roots);
                foreach (string tipo in new[] { "audio_a_uch", "audio_a_es", "audio_a_forma", "audio_a_es_oracion" })
                {
                    seleccion.Configurar(tipo, "", new List<string> { "Uno", "Dos" }, "prueba");
                    Assert.IsFalse(Referencia<GameObject>(seleccion, "panelTexto").transform.parent.gameObject.activeSelf);
                    Assert.IsTrue(Referencia<Button>(seleccion, "btnAudioPregunta").gameObject.activeSelf);
                }
                var banco = Encontrar<WordBankView>(roots);
                banco.Configurar("dictado_word_bank", "", new List<string> { "uno", "dos" });
                Assert.IsFalse(Referencia<TMP_Text>(banco, "txtPromptEs").transform.parent.gameObject.activeSelf);
                Assert.IsTrue(Referencia<Button>(banco, "btnAudioPrompt").gameObject.activeSelf);
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        [MenuItem("Uchumataqu/Pruebas/Auditar actividades en Play Mode")]
        public static void IniciarEnPlay() => Iniciar(AuditarEnPlay(), "Temp/LeccionBaseQA/interacciones-visuales.txt");

        [MenuItem("Uchumataqu/Pruebas/Auditar presenter en Play Mode")]
        public static void IniciarFlujoEnPlay() => Iniciar(AuditarPresenterEnPlay(), "Temp/LeccionBaseQA/flujo-presenter.txt");

        private static void Iniciar(IEnumerator auditoria, string reportPath)
        {
            if (!Application.isPlaying || SceneManager.GetActiveScene().path != ScenePath)
                throw new InvalidOperationException("Abre 03_LeccionBase en Play Mode.");
            if (_auditoria != null) throw new InvalidOperationException("La auditoría ya está en ejecución.");
            Resultados.Clear();
            EstadoAuditoria = "En ejecución";
            _auditoria = auditoria;
            _reportPath = reportPath;
            _ultimoFrame = -1;
            _pausaAnterior = EditorApplication.isPaused;
            EditorApplication.isPaused = true;
            EditorApplication.update += AvanzarAuditoria;
            EditorApplication.Step();
        }

        private static void AvanzarAuditoria()
        {
            if (!Application.isPlaying) { TerminarAuditoria("Interrumpida al salir de Play Mode"); return; }
            if (Time.frameCount == _ultimoFrame) return;
            _ultimoFrame = Time.frameCount;
            try
            {
                if (!_auditoria.MoveNext()) { TerminarAuditoria("PASS " + Resultados.Count + " comprobaciones"); return; }
                EditorApplication.Step();
            }
            catch (Exception ex)
            {
                TerminarAuditoria("FAIL " + EstadoAuditoria + ": " + ex);
                Debug.LogError(EstadoAuditoria);
            }
        }

        private static void TerminarAuditoria(string estado)
        {
            EditorApplication.update -= AvanzarAuditoria;
            (_auditoria as IDisposable)?.Dispose();
            _auditoria = null;
            if (Application.isPlaying) EditorApplication.isPaused = _pausaAnterior;
            EstadoAuditoria = estado;
            Directory.CreateDirectory(Path.GetDirectoryName(_reportPath));
            File.WriteAllLines(_reportPath, Resultados.Concat(new[] { estado }));
        }

        private static IEnumerator AuditarPresenterEnPlay()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            var presenter = Encontrar<LeccionBasePresenter>(roots);
            var view = Encontrar<LeccionBaseView>(roots);
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var gestorField = typeof(LeccionBasePresenter).GetField("_gestorLeccion", flags);
            var leccionProperty = typeof(GestorLeccion).GetProperty("LeccionActual");
            var muestras = CorpusRepository.ObtenerTodasLecciones().SelectMany(l => l.pasos)
                .GroupBy(p => p.tipo).Select(g => g.OrderByDescending(Peso).First()).ToArray();
            var principal = Referencia<Button>(view, "btnPrincipal");
            presenter.enabled = true;
            view.ModalResultado.Ocultar();
            view.MostrarConfirmacionSalida(false);
            try
            {
                foreach (var paso in muestras)
                {
                    EstadoAuditoria = "Presenter " + paso.tipo;
                    var gestor = new GestorLeccion();
                    // Cada caso evalúa el primer paso; no avanza ni escribe progreso del usuario.
                    leccionProperty.SetValue(gestor, new LeccionCompiladaModel {
                        id = "QA_LeccionBase", titulo = "Auditoría de actividades",
                        pasos = new List<PasoLeccionModel> { paso, muestras[0] }
                    });
                    gestorField.SetValue(presenter, gestor);
                    presenter.SendMessage("RenderizarPasoActual");
                    yield return null;
                    yield return null;
                    var host = Referencia<RectTransform>(view, "actividadContainer");
                    var page = host.Cast<Transform>().Single(t => t.gameObject.activeSelf);
                    if (!TipoPasoLeccion.EsPracticaVoz(paso.tipo))
                        Assert.IsFalse(view.Feedback.gameObject.activeSelf);
                    if (TipoPasoLeccion.EsSeleccion(paso.tipo))
                    {
                        Assert.IsTrue(principal.gameObject.activeSelf);
                        Assert.IsFalse(principal.interactable);
                        var datos = GestorActividadLeccion.CrearSeleccion(paso);
                        Button opcion = page.GetComponentsInChildren<Button>()
                            .First(b => b.GetComponent<TarjetaSeleccionView>() != null && Etiqueta(b) != datos.RespuestaCorrecta);
                        opcion.onClick.Invoke();
                        Assert.IsTrue(principal.interactable);
                        principal.onClick.Invoke();
                        Assert.AreEqual(1, gestor.Errores);
                        Assert.AreEqual("icono_incorrecto", opcion.transform.Find("IndicadorSeleccion/Check").GetComponent<Image>().sprite.name);
                        Assert.IsTrue(view.Feedback.GetComponentsInChildren<TMP_Text>()
                            .Any(t => t.text.Contains(datos.RespuestaCorrecta)));
                    }
                    else if (TipoPasoLeccion.EsWordBank(paso.tipo))
                    {
                        Assert.IsFalse(principal.interactable);
                        var datos = GestorActividadLeccion.CrearWordBank(paso);
                        var banco = page.GetComponent<WordBankView>();
                        var origen = Referencia<Transform>(banco, "contenedorOrigen");
                        foreach (string token in datos.Tokens)
                            origen.GetComponentsInChildren<Button>().First(b => Etiqueta(b) == token).onClick.Invoke();
                        Assert.IsTrue(principal.interactable);
                        principal.onClick.Invoke();
                        Assert.AreEqual(1, gestor.Aciertos);
                    }
                    else if (TipoPasoLeccion.EsEscritura(paso.tipo))
                    {
                        Assert.IsFalse(principal.interactable);
                        var datos = GestorActividadLeccion.CrearEscritura(paso);
                        var escritura = page.GetComponent<EscrituraView>();
                        Referencia<TMP_InputField>(escritura, "inputRespuesta").text = datos.RespuestaEsperada;
                        Assert.IsTrue(principal.interactable);
                        principal.onClick.Invoke();
                        Assert.AreEqual(1, gestor.Aciertos);
                    }
                    else if (paso.tipo == TipoPasoLeccion.MemoriaUchEspanol)
                    {
                        var memoria = page.GetComponent<MemoriaView>();
                        var datos = GestorActividadLeccion.CrearPares(paso);
                        var botones = ObtenerCartasMemoria(memoria);
                        foreach (var par in datos)
                        {
                            botones[par.Key].onClick.Invoke(); botones[par.Value].onClick.Invoke();
                        }
                        Assert.AreEqual(1, gestor.Aciertos);
                    }
                    else if (TipoPasoLeccion.EsEmparejamiento(paso.tipo))
                    {
                        var pares = page.GetComponent<EmparejamientoView>();
                        var datos = GestorActividadLeccion.CrearPares(paso);
                        var ids = (List<string>)typeof(EmparejamientoView).GetField("_idsIzquierda", flags).GetValue(pares);
                        var izq = Referencia<Transform>(pares, "columnaIzquierda").GetComponentsInChildren<Button>();
                        var der = Referencia<Transform>(pares, "columnaDerecha").GetComponentsInChildren<Button>();
                        foreach (var par in datos)
                        {
                            izq[ids.IndexOf(par.Key)].onClick.Invoke();
                            der.First(b => Etiqueta(b) == par.Value).onClick.Invoke();
                        }
                        Assert.AreEqual(1, gestor.Aciertos);
                    }
                    else if (TipoPasoLeccion.EsPresentacion(paso.tipo)) Assert.IsTrue(principal.interactable);
                    else
                    {
                        Assert.IsTrue(TipoPasoLeccion.EsPracticaVoz(paso.tipo));
                        Assert.IsTrue(principal.gameObject.activeSelf);
                        Button practicar = Referencia<Button>(page.GetComponent<ASRView>(), "btnPracticar");
                        Assert.AreEqual(!practicar.interactable, principal.interactable,
                            "PageASR solo permite continuar sin repetir cuando ASR no está disponible.");
                        Assert.AreEqual("CONTINUAR", Referencia<TMP_Text>(view, "txtAccionPrincipal").text);
                        Assert.IsFalse(view.GetComponentsInChildren<Transform>(true)
                            .Any(t => t.name == "OmitirASR"));
                    }
                    yield return null;
                    yield return null;
                    bool evaluable = TipoPasoLeccion.EsSeleccion(paso.tipo) || TipoPasoLeccion.EsWordBank(paso.tipo) ||
                        TipoPasoLeccion.EsEmparejamiento(paso.tipo) || TipoPasoLeccion.EsEscritura(paso.tipo);
                    if (evaluable)
                    {
                        Assert.AreEqual(1, gestor.Aciertos + gestor.Errores);
                        Assert.IsTrue(view.Feedback.gameObject.activeSelf);
                        Assert.IsTrue(principal.interactable);
                        Assert.AreEqual("CONTINUAR", Referencia<TMP_Text>(view, "txtAccionPrincipal").text);
                        ValidarTextos(view.Feedback.transform);
                        Bounds viewport = RectTransformUtility.CalculateRelativeRectTransformBounds(
                            (RectTransform)host.parent, page.GetComponentInChildren<ScrollRect>().viewport);
                        Bounds feedback = RectTransformUtility.CalculateRelativeRectTransformBounds(
                            (RectTransform)host.parent, (RectTransform)view.Feedback.transform);
                        Assert.LessOrEqual(feedback.max.y, viewport.min.y + 2f, "Feedback tapa la actividad");
                    }
                    Assert.AreEqual(0, gestor.IndicePasoActual);
                    Resultados.Add("PASS " + EstadoAuditoria);
                }
            }
            finally
            {
                presenter.SendMessage("DesuscribirActividadActual");
                presenter.enabled = false;
            }
        }

        private static IEnumerator AuditarEnPlay()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            var presenter = Encontrar<LeccionBasePresenter>(roots);
            presenter.SendMessage("DesuscribirActividadActual");
            presenter.enabled = false;
            var view = Encontrar<LeccionBaseView>(roots);
            view.OcultarFeedback();
            view.ModalResultado.Ocultar();
            view.MostrarConfirmacionSalida(false);
            var canvas = Encontrar<Canvas>(roots);
            var camera = Encontrar<Camera>(roots);
            var scaler = canvas.GetComponent<CanvasScaler>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1;
            var pasos = CorpusRepository.ObtenerTodasLecciones().SelectMany(l => l.pasos).ToList();
            var muestras = pasos.GroupBy(p => p.tipo).Select(g => g.OrderByDescending(Peso).First())
                .Concat(pasos.Where(p => TipoPasoLeccion.EsWordBank(p.tipo))
                    .GroupBy(p => p.tipo + ":" + p.oracion_id).Select(g => g.First())).Distinct().ToArray();
            foreach (int alto in new[] { 1440, 1920, 2340 })
            {
                var target = new RenderTexture(1080, alto, 24);
                target.Create();
                camera.targetTexture = target;
                try
                {
                    foreach (PasoLeccionModel paso in muestras)
                    {
                        EstadoAuditoria = paso.tipo + ":" + paso.oracion_id + " @1080x" + alto;
                        LeccionBaseCompositionTests.Configurar(view, paso);
                        view.PresentarAccionPrincipal(TipoPasoLeccion.EsPresentacion(paso.tipo)
                            ? LeccionBaseView.AccionPrincipal.Continuar : LeccionBaseView.AccionPrincipal.Comprobar,
                            false, TipoPasoLeccion.EsPresentacion(paso.tipo) || TipoPasoLeccion.EsSeleccion(paso.tipo) || TipoPasoLeccion.EsWordBank(paso.tipo));
                        yield return null;
                        yield return null;
                        var host = Referencia<RectTransform>(view, "actividadContainer");
                        var page = host.Cast<Transform>().Single(t => t.gameObject.activeSelf);
                        camera.Render();
                        ValidarTextos(page);
                        if (TipoPasoLeccion.EsWordBank(paso.tipo))
                        {
                            var banco = page.GetComponent<WordBankView>();
                            var origen = Referencia<Transform>(banco, "contenedorOrigen");
                            var destino = Referencia<Transform>(banco, "contenedorDestino");
                            Button[] fichas = origen.GetComponentsInChildren<Button>();
                            var esperada = fichas.Select(b => b.GetComponentInChildren<TMP_Text>().text).ToArray();
                            foreach (Button ficha in fichas)
                            {
                                ficha.onClick.Invoke();
                                yield return null;
                                yield return null;
                                camera.Render();
                                Assert.IsTrue(ficha.transform.IsChildOf(destino));
                                ValidarFicha(ficha, banco.GetComponentInChildren<ScrollRect>().viewport);
                                ValidarFilas(destino);
                            }
                            fichas[0].onClick.Invoke();
                            yield return null;
                            Assert.IsTrue(fichas[0].transform.IsChildOf(origen));
                            fichas[0].onClick.Invoke();
                            yield return null;
                            var reordenada = esperada.Skip(1).Concat(esperada.Take(1)).ToArray();
                            List<string> enviada = null;
                            int envios = 0;
                            Action<List<string>> enviar = tokens => { enviada = tokens; envios++; };
                            banco.OnTokensComprobados += enviar;
                            banco.Comprobar(); banco.Comprobar();
                            banco.OnTokensComprobados -= enviar;
                            Assert.AreEqual(1, envios);
                            CollectionAssert.AreEqual(reordenada, enviada);
                            Assert.IsFalse(banco.PuedeEnviarRespuesta);
                            if (paso == muestras.First(p => p.tipo == paso.tipo)) Capturar(camera, target, paso.tipo + "-respuesta-" + alto);
                        }
                        else if (TipoPasoLeccion.EsSeleccion(paso.tipo))
                        {
                            var seleccion = page.GetComponent<SeleccionMultipleView>();
                            string correcta = GestorActividadLeccion.CrearSeleccion(paso).RespuestaCorrecta;
                            Button boton = page.GetComponentsInChildren<Button>().First(b =>
                                b.GetComponent<TarjetaSeleccionView>() != null && Etiqueta(b) != correcta);
                            int envios = 0;
                            Action<string> enviar = _ => envios++;
                            seleccion.OnRespuestaEnviada += enviar;
                            boton.onClick.Invoke();
                            Assert.IsTrue(seleccion.PuedeEnviarRespuesta);
                            seleccion.ConfirmarRespuesta(); seleccion.ConfirmarRespuesta();
                            seleccion.OnRespuestaEnviada -= enviar;
                            Assert.AreEqual(1, envios);
                            Assert.IsFalse(seleccion.PuedeEnviarRespuesta);
                            seleccion.MostrarResultado(false, GestorActividadLeccion.CrearSeleccion(paso).RespuestaCorrecta);
                            yield return null;
                            yield return null;
                            ValidarTextos(page);
                            Capturar(camera, target, paso.tipo + "-seleccion-" + alto);
                        }
                        else Capturar(camera, target, paso.tipo + "-" + alto);
                        Resultados.Add("PASS " + EstadoAuditoria);
                    }
                    // Conserva fichas repetidas y textos largos al reutilizar la misma Page.
                    var prueba = view.InstanciarWordBank();
                    prueba.Configurar("traduccion_constructiva", "Prueba de palabras repetidas",
                        new List<string> { "la", "la", "extraordinariamente", "casa", "grande" });
                    yield return null;
                    yield return null;
                    var bancoOrigen = Referencia<Transform>(prueba, "contenedorOrigen");
                    foreach (Button ficha in bancoOrigen.GetComponentsInChildren<Button>())
                    {
                        ficha.onClick.Invoke();
                        yield return null;
                        yield return null;
                        camera.Render();
                        ValidarFicha(ficha, prueba.GetComponentInChildren<ScrollRect>().viewport);
                    }
                    Resultados.Add("PASS fichas repetidas @1080x" + alto);
                }
                finally
                {
                    camera.targetTexture = null;
                    target.Release();
                    Object.Destroy(target);
                }
            }
            foreach (object espera in AuditarParesYMemoria(view)) yield return espera;
        }

        private static IEnumerable AuditarParesYMemoria(LeccionBaseView view)
        {
            EstadoAuditoria = "Emparejamiento: error, reintento y cambio de cantidad";
            var pares = view.InstanciarEmparejamiento();
            var datos = new Dictionary<string, string> { ["uno"] = "one", ["dos"] = "two" };
            pares.Configurar(datos);
            var izquierda = Referencia<Transform>(pares, "columnaIzquierda");
            var derecha = Referencia<Transform>(pares, "columnaDerecha");
            int completados = 0;
            Action<string, string> evaluar = (a, b) => pares.MostrarResultadoPar(datos[a] == b);
            Action<bool> completar = _ => completados++;
            pares.OnParPropuesto += evaluar;
            pares.OnEmparejamientoCompletado += completar;
            try
            {
                izquierda.GetComponentsInChildren<Button>().First(b => Etiqueta(b) == "uno").onClick.Invoke();
                derecha.GetComponentsInChildren<Button>().First(b => Etiqueta(b) == "two").onClick.Invoke();
                Assert.AreEqual(0, completados);
                foreach (var par in datos)
                {
                    izquierda.GetComponentsInChildren<Button>().First(b => Etiqueta(b) == par.Key).onClick.Invoke();
                    derecha.GetComponentsInChildren<Button>().First(b => Etiqueta(b) == par.Value).onClick.Invoke();
                }
                Assert.AreEqual(1, completados);
                pares.Configurar(new Dictionary<string, string> { ["nuevo"] = "new" }, true);
                Assert.AreEqual(1, izquierda.GetComponentsInChildren<Button>().Length);
                Assert.IsTrue(izquierda.GetComponentsInChildren<Button>().Single().transform.Find("IconoAudio").gameObject.activeSelf);
            }
            finally { pares.OnParPropuesto -= evaluar; pares.OnEmparejamientoCompletado -= completar; }
            Resultados.Add("PASS " + EstadoAuditoria);

            EstadoAuditoria = "Memoria: error, ocultación, interrupción y reintento";
            var memoria = view.InstanciarMemoria();
            memoria.Configurar(datos);
            var botones = ObtenerCartasMemoria(memoria);
            Action<string, string> evaluarMemoria = (a, b) => memoria.MostrarResultadoPar(a == b);
            completados = 0;
            memoria.OnParPropuesto += evaluarMemoria;
            memoria.OnMemoriaCompletada += completar;
            try
            {
                botones["uno"].onClick.Invoke(); botones["dos"].onClick.Invoke();
                float fin = Time.realtimeSinceStartup + .75f;
                while (Time.realtimeSinceStartup < fin) yield return null;
                Assert.AreEqual("?", Etiqueta(botones["uno"]));
                Assert.AreEqual("?", Etiqueta(botones["dos"]));
                botones["uno"].onClick.Invoke(); botones["dos"].onClick.Invoke();
                memoria.gameObject.SetActive(false); memoria.gameObject.SetActive(true);
                foreach (var par in datos)
                {
                    botones[par.Key].onClick.Invoke(); botones[par.Value].onClick.Invoke();
                    memoria.MostrarResultadoPar(true);
                }
                Assert.AreEqual(1, completados);
                Assert.IsTrue(botones.Values.All(b => !b.interactable));
            }
            finally { memoria.OnParPropuesto -= evaluarMemoria; memoria.OnMemoriaCompletada -= completar; }
            Resultados.Add("PASS " + EstadoAuditoria);
        }

        private static int Peso(PasoLeccionModel paso)
        {
            if (TipoPasoLeccion.EsSeleccion(paso.tipo))
            {
                var datos = GestorActividadLeccion.CrearSeleccion(paso);
                return datos.Prompt.Length + datos.Opciones.Sum(s => s.Length);
            }
            if (TipoPasoLeccion.EsWordBank(paso.tipo))
            {
                var datos = GestorActividadLeccion.CrearWordBank(paso);
                return datos.Prompt.Length + datos.Tokens.Sum(s => s.Length);
            }
            if (TipoPasoLeccion.EsPresentacion(paso.tipo))
            {
                var palabra = CorpusRepository.ObtenerPalabra(paso.objetivo_id);
                return (palabra?.uch?.Length ?? 0) + (palabra?.es?.Length ?? 0) +
                    (palabra?.formas?.Sum(f => f.uch.Length + f.es.Length + 200) ?? 0);
            }
            if (TipoPasoLeccion.EsEmparejamiento(paso.tipo))
                return GestorActividadLeccion.CrearPares(paso).Sum(p => p.Key.Length + p.Value.Length + 200);
            return 0;
        }

        private static Dictionary<string, Button> ObtenerCartasMemoria(MemoriaView memoria)
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var cartas = (IEnumerable)typeof(MemoriaView).GetField("_cartas", flags).GetValue(memoria);
            var botones = new Dictionary<string, Button>();
            foreach (object carta in cartas)
                botones.Add((string)carta.GetType().GetProperty("Texto").GetValue(carta),
                    (Button)carta.GetType().GetProperty("Boton").GetValue(carta));
            return botones;
        }

        private static void ValidarAudio(Component view, string campo)
        {
            Button button = Referencia<Button>(view, campo);
            Transform content = view.transform.Find("ScrollRect/Viewport/Content");
            Assert.IsTrue(button.transform.IsChildOf(content));
            for (Transform parent = button.transform.parent; parent != content; parent = parent.parent)
                Assert.IsNull(parent.GetComponent<Image>(), "El audio sigue dentro de una tarjeta");
            Image icon = button.transform.Find("Icono").GetComponent<Image>();
            Assert.AreEqual("icono_audio", icon.sprite.name);
            Assert.IsTrue(icon.preserveAspect);
            Assert.IsTrue(button.GetComponentInChildren<TMP_Text>(true).gameObject.activeSelf);
        }

        private static void ValidarTextos(Transform page)
        {
            foreach (TMP_Text text in page.GetComponentsInChildren<TMP_Text>())
            {
                if (text.GetComponent<LayoutElement>()?.ignoreLayout == true) continue;
                Assert.Greater(text.rectTransform.rect.width, 0, text.name);
                float alto = text.GetPreferredValues(text.text, text.rectTransform.rect.width, Mathf.Infinity).y;
                Assert.GreaterOrEqual(text.rectTransform.rect.height + 2f, alto,
                    text.transform.parent.name + "/" + text.name + ": texto recortado");
            }
        }

        private static void ValidarFilas(Transform destino)
        {
            foreach (HorizontalLayoutGroup fila in destino.GetComponentsInChildren<HorizontalLayoutGroup>())
            {
                Button[] botones = fila.GetComponentsInChildren<Button>();
                float ancho = botones.Sum(b => ((RectTransform)b.transform).rect.width) + Mathf.Max(0, botones.Length - 1) * fila.spacing;
                Assert.LessOrEqual(ancho, ((RectTransform)fila.transform).rect.width + 2f, "Fichas fuera de la fila");
            }
        }

        private static void ValidarFicha(Button ficha, RectTransform viewport)
        {
            Assert.IsTrue(ficha.gameObject.activeInHierarchy, "Ficha inactiva después de seleccionarla");
            Assert.Greater(((RectTransform)ficha.transform).rect.height, 40, "Fila colapsada");
            Assert.IsFalse(ficha.GetComponent<CanvasRenderer>().cull, "Ficha seleccionada invisible");
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, (RectTransform)ficha.transform);
            Assert.GreaterOrEqual(bounds.min.y + 2, viewport.rect.yMin, "Ficha debajo del área visible");
            Assert.LessOrEqual(bounds.max.y - 2, viewport.rect.yMax, "Ficha encima del área visible");
        }

        private static void Capturar(Camera camera, RenderTexture target, string nombre)
        {
            camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            Directory.CreateDirectory("Temp/LeccionBaseQA");
            File.WriteAllBytes("Temp/LeccionBaseQA/play-" + nombre + ".png", image.EncodeToPNG());
            RenderTexture.active = previous;
            Object.Destroy(image);
        }

        private static string Etiqueta(Button button) => button.GetComponentInChildren<TMP_Text>().text;
        private static T Encontrar<T>(IEnumerable<GameObject> roots) where T : Component =>
            roots.SelectMany(r => r.GetComponentsInChildren<T>(true)).Single();
        private static T Referencia<T>(Component componente, string campo) where T : Object =>
            (T)new SerializedObject(componente).FindProperty(campo).objectReferenceValue;
    }
}
