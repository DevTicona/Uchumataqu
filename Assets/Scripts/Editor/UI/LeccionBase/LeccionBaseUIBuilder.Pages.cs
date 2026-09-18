#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.UI.Components;
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
        private const string ElementsPath = "Assets/Content/Prefabs/Actividades";

        private static void ConstruirPages()
        {
            EnsureFolder(ElementsPath);
            GameObject opcion = ElementoBoton("opcion_respuesta", 104f);
            GameObject item = ElementoBoton("item_emparejamiento", 120f);
            GameObject carta = ElementoBoton("tarjeta_memoria", 140f);
            GameObject ficha = ElementoBoton("ficha_wordbank", 72f);
            GameObject fila = CrearFilaForma();
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            LeccionBaseView view = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<LeccionBaseView>(true)).Single();
            RectTransform safe = (RectTransform)BuscarEnEscena(scene, "SafeArea");
            ConfigurarShellEscena(scene, safe);
            AnclarSuperior((RectTransform)BuscarEnEscena(scene, "HeaderLeccion"), 28, 28, 20, 150);
            AnclarSuperior((RectTransform)BuscarEnEscena(scene, "TxtTituloLeccion"), 86, 0, 12, 64);
            AnclarSuperior((RectTransform)BuscarEnEscena(scene, "SliderProgresoLeccion"), 86, 0, 96, 18);
            RectTransform back = (RectTransform)BuscarEnEscena(scene, "BtnAtrasLeccion");
            back.anchorMin = back.anchorMax = new Vector2(0, 1);
            back.anchoredPosition = new Vector2(0, -44);
            ConfigurarSlider(scene);
            // Filled Image keeps slider progress without writing fractional anchors.
            Slider slider = BuscarEnEscena(scene, "SliderProgresoLeccion").GetComponent<Slider>();
            Image fill = slider.fillRect.GetComponent<Image>();
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal;
            fill.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            foreach (string name in new[] { "ActividadContainer", "ActividadHost", "FooterLeccion", "FeedbackOverlay", "PanelFeedback", "ModalResultado", "ConfirmacionSalida", "ModalConfirmarSalida" })
            {
                Transform old = safe.Find(name);
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            GameObject host = CrearObjetoUI("ActividadHost", safe);
            Estirar((RectTransform)host.transform);
            ((RectTransform)host.transform).offsetMin = new Vector2(28, 150);
            ((RectTransform)host.transform).offsetMax = new Vector2(-28, -186);
            GameObject footer = CrearObjetoUI("FooterLeccion", safe);
            AnclarInferior((RectTransform)footer.transform, 0, 0, 0, 340);
            Button continuar = CrearBoton(footer.transform, "BtnPrincipal", "CONTINUAR", false, 88, "Lecciones/icono_continuar.png");
            Button omitir = CrearBoton(footer.transform, "OmitirASR", "CONTINUAR SIN GRABAR", true, 88);
            var acciones = new[] { continuar.gameObject, omitir.gameObject };
            foreach (GameObject accion in acciones)
            {
                AnclarInferior((RectTransform)accion.transform, 28, 28, 24, 88);
                accion.SetActive(false);
            }

            PresentacionFamiliaView presentacion = CrearPage<PresentacionFamiliaView>(host.transform, "PagePresentacionFamilia", out Transform cp);
            CrearEncabezadoActividad(cp, "TxtIndicacion", "Conoce una nueva palabra", "Estados/icono_nueva_palabra.png");
            GameObject hero = Bloque(cp, "CardRaiz", ThemedGraphicView.RolColor.Superficie);
            Texto(hero.transform, "EtiquetaUch", "UCHUMATAQU", 20, 34);
            TMP_Text raiz = Texto(hero.transform, "TxtUchRaiz", "Chhaq", 52, 100);
            Texto(hero.transform, "EtiquetaEs", "ESPAÑOL", 20, 34);
            TMP_Text es = Texto(hero.transform, "TxtEsRaiz", "Grande", 32, 80);
            Button audio = CrearBoton(hero.transform, "BtnAudioRaiz", "ESCUCHAR", true, 88, "Lecciones/icono_escuchar.png");
            GameObject formas = Bloque(cp, "FormasContainer", ThemedGraphicView.RolColor.SuperficieSuave);
            TMP_Text tituloFormas = Texto(formas.transform, "TxtTituloFormas", "Formas documentadas", 26, 54);
            AsignarReferencias(presentacion, new Dictionary<string, UnityEngine.Object> {
                ["txtBadge"] = cp.Find("EncabezadoTxtIndicacion/TxtIndicacion").GetComponent<TMP_Text>(),
                ["txtRaizUch"] = raiz, ["txtSignificadoEs"] = es, ["btnAudio"] = audio,
                ["contenedorFormas"] = formas,
                ["txtTituloFormas"] = tituloFormas, ["prefabFilaForma"] = fila });

            SeleccionMultipleView seleccion = CrearPage<SeleccionMultipleView>(host.transform, "PageSeleccionMultiple", out Transform cs);
            TMP_Text instruccion = CrearEncabezadoActividad(cs, "TxtInstruccion", "Selecciona la respuesta correcta", "Lecciones/icono_seleccion.png");
            GameObject estimulo = Bloque(cs, "EstimuloHost", ThemedGraphicView.RolColor.SuperficieSuave);
            GameObject panelTexto = Vertical("PanelTexto", estimulo.transform);
            TMP_Text pregunta = Texto(panelTexto.transform, "TxtEstimulo", "¿Cuál es la traducción?", 36, 120);
            GameObject panelHueco = Vertical("PanelOracionHueco", estimulo.transform);
            TMP_Text hueco = Texto(panelHueco.transform, "TxtOracion", "", 34, 120);
            Button audioPregunta = CrearBoton(estimulo.transform, "PanelAudio", "ESCUCHAR AUDIO", true, 108, "Lecciones/icono_escuchar.png");
            GameObject opciones = Vertical("OpcionesContainer", cs);
            AsignarReferencias(seleccion, new Dictionary<string, UnityEngine.Object> {
                ["txtInstruccion"] = instruccion, ["txtTextoPregunta"] = pregunta, ["txtOracionHueco"] = hueco,
                ["panelTexto"] = panelTexto, ["panelOracionHueco"] = panelHueco, ["btnAudioPregunta"] = audioPregunta,
                ["contenedorOpciones"] = opciones.transform, ["prefabOpcion"] = opcion });

            EmparejamientoView pares = CrearPage<EmparejamientoView>(host.transform, "PageEmparejamiento", out Transform ce);
            TMP_Text ip = CrearEncabezadoActividad(ce, "TxtInstruccion", "Une cada pareja", "Lecciones/icono_emparejar.png");
            GameObject pairArea = CrearObjetoUI("PairArea", ce);
            HorizontalLayoutGroup h = pairArea.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 18; h.childControlHeight = h.childControlWidth = true; h.childForceExpandHeight = false;
            GameObject izq = Vertical("ColumnaIzquierda", pairArea.transform);
            GameObject der = Vertical("ColumnaDerecha", pairArea.transform);
            izq.AddComponent<LayoutElement>().flexibleWidth = 1;
            der.AddComponent<LayoutElement>().flexibleWidth = 1;
            AsignarReferencias(pares, new Dictionary<string, UnityEngine.Object> { ["txtInstruccion"] = ip,
                ["columnaIzquierda"] = izq.transform, ["columnaDerecha"] = der.transform, ["prefabItem"] = item });

            MemoriaView memoria = CrearPage<MemoriaView>(host.transform, "PageMemoria", out Transform cm);
            CrearEncabezadoActividad(cm, "TxtInstruccion", "Encuentra las parejas", "Lecciones/icono_memoria.png");
            Texto(cm, "Ayuda", "Toca dos tarjetas. Recuerda cada palabra y su significado.", 24, 82);
            GameObject grid = CrearObjetoUI("GridTarjetas", cm);
            GridLayoutGroup gl = grid.AddComponent<GridLayoutGroup>();
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount; gl.constraintCount = 2;
            gl.spacing = new Vector2(16, 16); gl.cellSize = new Vector2(280, 148);
            AsignarReferencias(memoria, new Dictionary<string, UnityEngine.Object> { ["contenedorCartas"] = grid.transform, ["prefabTarjeta"] = carta });

            WordBankView banco = CrearPage<WordBankView>(host.transform, "PageWordBank", out Transform cw);
            TMP_Text iw = CrearEncabezadoActividad(cw, "TxtInstruccion", "Construye la oración", "Lecciones/icono_ordenar.png");
            GameObject ew = Bloque(cw, "EstimuloHost", ThemedGraphicView.RolColor.SuperficieSuave);
            TMP_Text prompt = Texto(ew.transform, "TxtEstimulo", "Oración", 32, 110);
            Button aw = CrearBoton(ew.transform, "BtnAudio", "ESCUCHAR ORACIÓN", true, 88, "Lecciones/icono_escuchar.png");
            Texto(cw, "TxtRespuesta", "TU RESPUESTA · toca una ficha para devolverla", 22, 64);
            GameObject destino = Bloque(cw, "RespuestaRows", ThemedGraphicView.RolColor.SuperficieSuave);
            destino.AddComponent<LayoutElement>().minHeight = 108;
            Texto(cw, "TxtBanco", "BANCO DE PALABRAS", 22, 48);
            GameObject origen = Bloque(cw, "BancoRows", ThemedGraphicView.RolColor.SuperficieSuave);
            AsignarReferencias(banco, new Dictionary<string, UnityEngine.Object> { ["txtInstruccion"] = iw, ["txtPromptEs"] = prompt,
                ["btnAudioPrompt"] = aw, ["contenedorOrigen"] = origen.transform, ["contenedorDestino"] = destino.transform,
                ["prefabFichaToken"] = ficha });

            ASRView asr = CrearPage<ASRView>(host.transform, "PageASR", out Transform ca);
            CrearEncabezadoActividad(ca, "TxtInstruccion", "Escucha y repite · opcional", "Lecciones/icono_pronunciacion.png");
            GameObject objetivo = Bloque(ca, "Referencia", ThemedGraphicView.RolColor.Superficie);
            TMP_Text ta = Texto(objetivo.transform, "TxtObjetivo", "Chhaq", 48, 110);
            Button aa = CrearBoton(objetivo.transform, "BtnAudioReferencia", "ESCUCHAR", true, 88, "Lecciones/icono_escuchar.png");
            GameObject micro = Bloque(ca, "PanelMicrofono", ThemedGraphicView.RolColor.SuperficieSuave);
            Button bm = CrearBoton(micro.transform, "BtnMicrofono", "PRACTICAR", true, 110, "Lecciones/icono_pronunciacion.png");
            TMP_Text estado = Texto(micro.transform, "TxtEstado", "Práctica de voz opcional", 26, 140);
            TMP_Text indicador = Texto(micro.transform, "IndicadorGrabacion", "Grabando…", 24, 48);
            GameObject pr = Bloque(ca, "PanelResultado", ThemedGraphicView.RolColor.SuperficieSuave);
            TMP_Text transcripcion = Texto(pr.transform, "TxtTranscripcion", "", 28, 90);
            TMP_Text porcentaje = Texto(pr.transform, "TxtPorcentaje", "", 28, 54);
            AsignarReferencias(asr, new Dictionary<string, UnityEngine.Object> { ["txtObjetivo"] = ta, ["txtEstado"] = estado,
                ["txtTranscripcion"] = transcripcion, ["txtPorcentaje"] = porcentaje, ["panelResultado"] = pr,
                ["indicadorGrabacion"] = indicador.gameObject, ["btnAudioReferencia"] = aa, ["btnMicrofono"] = bm, ["btnOmitir"] = omitir });
            indicador.gameObject.SetActive(false); pr.SetActive(false);

            FeedbackOverlayView feedback = CrearContenidoFeedback(footer.transform);
            Estirar((RectTransform)feedback.transform);
            feedback.transform.SetAsFirstSibling();
            footer.transform.SetAsLastSibling();
            ModalResultadoView resultado = InstanciarOverlay<ModalResultadoView>(CrearPrefabResultado(), safe, "ModalResultado");
            Estirar((RectTransform)resultado.transform);
            GameObject salida = InstanciarOverlay(CrearPrefabConfirmacionSalida(), safe, "ModalConfirmarSalida");
            Estirar((RectTransform)salida.transform);
            AsignarReferencias(view, new Dictionary<string, UnityEngine.Object> { ["actividadContainer"] = host.transform,
                ["pagePresentacion"] = presentacion.gameObject, ["pageSeleccionMultiple"] = seleccion.gameObject,
                ["pageEmparejamiento"] = pares.gameObject, ["pageMemoria"] = memoria.gameObject,
                ["pageWordBank"] = banco.gameObject, ["pageASR"] = asr.gameObject,
                ["feedbackOverlay"] = feedback, ["modalResultado"] = resultado, ["panelConfirmacionSalida"] = salida,
                ["btnSeguirAprendiendo"] = salida.transform.Find("CardConfirmacion/BtnSeguirAprendiendo").GetComponent<Button>(),
                ["btnConfirmarSalida"] = salida.transform.Find("CardConfirmacion/BtnConfirmarSalida").GetComponent<Button>() });
            AsignarReferencias(view, new Dictionary<string, UnityEngine.Object> {
                ["btnPrincipal"] = continuar,
                ["txtAccionPrincipal"] = continuar.transform.Find("Texto").GetComponent<TMP_Text>(),
                ["iconoAccionPrincipal"] = continuar.transform.Find("Icono").GetComponent<Image>(),
                ["iconoContinuar"] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Lecciones/icono_continuar.png"),
                ["iconoComprobar"] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Lecciones/icono_comprobar.png"),
                ["iconoFinalizar"] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Estados/icono_completado.png"),
                ["btnOmitirASR"] = omitir });
            feedback.gameObject.SetActive(false); resultado.gameObject.SetActive(false); salida.SetActive(false);
            presentacion.gameObject.SetActive(true); continuar.gameObject.SetActive(true);
            foreach (RectTransform rect in safe.GetComponentsInChildren<RectTransform>(true))
            { rect.localRotation = Quaternion.identity; rect.localScale = Vector3.one; }
            LeccionActivityStyleUpdater.Aplicar(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static T CrearPage<T>(Transform host, string nombre, out Transform content) where T : Component
        {
            GameObject page = CrearObjetoUI(nombre, host);
            page.SetActive(false);
            Estirar((RectTransform)page.transform);
            T view = page.AddComponent<T>();
            page.AddComponent<ActividadLayoutController>();
            GameObject scrollObject = CrearObjetoUI("ScrollRect", page.transform);
            Estirar((RectTransform)scrollObject.transform);
            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
            GameObject viewport = CrearObjetoUI("Viewport", scrollObject.transform);
            Estirar((RectTransform)viewport.transform);
            viewport.AddComponent<RectMask2D>();
            Image touch = viewport.AddComponent<Image>(); touch.color = new Color(1, 1, 1, 0.001f);
            GameObject body = Vertical("Content", viewport.transform);
            var layout = body.GetComponent<VerticalLayoutGroup>(); layout.spacing = 22;
            layout.padding = new RectOffset(8, 8, 12, 28);
            RectTransform rect = (RectTransform)body.transform;
            rect.anchorMin = new Vector2(0, 1); rect.anchorMax = Vector2.one; rect.pivot = new Vector2(.5f, 1);
            rect.sizeDelta = Vector2.zero; rect.anchoredPosition = Vector2.zero;
            body.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = (RectTransform)viewport.transform; scroll.content = rect;
            scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 36;
            content = body.transform;
            return view;
        }

        private static GameObject Vertical(string name, Transform parent)
        {
            GameObject go = CrearContenedor(name, parent, 14, new RectOffset());
            go.GetComponent<VerticalLayoutGroup>().childControlHeight = true;
            return go;
        }

        private static GameObject Bloque(Transform parent, string name, ThemedGraphicView.RolColor color)
        {
            GameObject block = Vertical(name, parent);
            block.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(24, 24, 24, 24);
            ConfigurarImagenTematizada(block.AddComponent<Image>(), color);
            block.GetComponent<Image>().raycastTarget = false;
            block.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Base/ui_panel.png");
            block.GetComponent<Image>().type = Image.Type.Sliced;
            return block;
        }

        private static TMP_Text Texto(Transform parent, string name, string text, float size, float height)
        {
            TMP_Text label = CrearTexto(parent, name, text, size, FontStyles.Normal, Ink, height);
            label.raycastTarget = false;
            label.GetComponent<LayoutElement>().preferredHeight = -1;
            return label;
        }

        private static GameObject ElementoBoton(string nombre, float altura)
        {
            Button button = CrearBoton(null, nombre, nombre == "tarjeta_memoria" ? "?" : "Palabra", true, altura,
                null, ThemedButtonView.EstiloBoton.Suave);
            button.GetComponent<LayoutElement>().preferredHeight = -1;
            ((RectTransform)button.transform).sizeDelta = new Vector2(320, altura);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            label.GetComponent<LayoutElement>().preferredHeight = -1;
            if (nombre == "ficha_wordbank") label.textWrappingMode = TextWrappingModes.NoWrap;
            return GuardarPrefab(button.gameObject, $"{ElementsPath}/{nombre}.prefab");
        }

        private static GameObject CrearFilaForma()
        {
            GameObject row = CrearObjetoUI("fila_forma_familia");
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16; layout.childControlHeight = layout.childControlWidth = true;
            layout.childForceExpandWidth = false; layout.childForceExpandHeight = false; layout.padding = new RectOffset(12, 12, 12, 12);
            row.AddComponent<LayoutElement>().minHeight = 120;
            GameObject col = Vertical("ColTexto", row.transform);
            col.AddComponent<LayoutElement>().flexibleWidth = 1;
            Texto(col.transform, "TxtUchForma", "Forma", 28, 46);
            Texto(col.transform, "TxtEsForma", "Significado", 24, 42);
            Button audio = CrearBoton(row.transform, "BtnAudio", "", true, 72, "Lecciones/icono_escuchar.png");
            audio.GetComponent<LayoutElement>().preferredWidth = 88;
            return GuardarPrefab(row, $"{ElementsPath}/fila_forma_familia.prefab");
        }
    }
}
#endif
