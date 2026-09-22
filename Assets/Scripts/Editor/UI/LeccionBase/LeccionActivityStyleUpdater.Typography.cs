#if UNITY_EDITOR
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.UI.LeccionBase.Actividades;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Editor
{
    public static partial class LeccionActivityStyleUpdater
    {
        private const float IconoEncabezado = 96;
        private const float IconoBoton = 70;

        public static void AplicarEscritura(EscrituraView escritura)
        {
            if (escritura != null) ConfigurarPage(escritura.gameObject);
        }

        public static void AplicarASR(ASRView asr)
        {
            if (asr != null) ConfigurarPage(asr.gameObject);
        }

        private static void ConfigurarPage(GameObject page)
        {
            Transform content = page.transform.Find("ScrollRect/Viewport/Content");
            var contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.spacing = EspaciadoVertical(page);
            foreach (TMP_Text text in page.GetComponentsInChildren<TMP_Text>(true))
            {
                var themed = text.GetComponent<ThemedTextView>();
                TextoTema(text, themed != null ? themed.tipoTexto : ThemedTextView.TipoTexto.Normal,
                    themed != null ? themed.tipoColor : ThemedTextView.ColorTexto.Primario, text.fontStyle);
            }
            foreach (Image image in page.GetComponentsInChildren<Image>(true))
            {
                if (image.name == "IconoActividad")
                {
                    IconoTema(image, IconoEncabezado);
                    Transform header = image.transform.parent;
                    TMP_Text title = header.GetComponentInChildren<TMP_Text>();
                    TextoTema(title, ThemedTextView.TipoTexto.Titulo, ThemedTextView.ColorTexto.Secundario);
                    Elemento(title.gameObject).flexibleWidth = 1;
                    Elemento(header.gameObject).minHeight = IconoEncabezado;
                    Elemento(header.gameObject).preferredHeight = -1;
                    var horizontal = header.GetComponent<HorizontalLayoutGroup>();
                    horizontal.childControlWidth = horizontal.childControlHeight = true;
                    horizontal.childForceExpandHeight = horizontal.childForceExpandWidth = false;
                    horizontal.childAlignment = TextAnchor.MiddleLeft;
                    horizontal.spacing = 24;
                }
            }
            foreach (Button button in page.GetComponentsInChildren<Button>(true)) ConfigurarBoton(button);

            if (page.TryGetComponent(out PresentacionFamiliaView presentacion))
            {
                foreach (HorizontalOrVerticalLayoutGroup layout in content.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(true))
                {
                    layout.childControlHeight = true;
                    layout.childForceExpandHeight = false;
                }
                CampoTexto(presentacion, "txtRaizUch", ThemedTextView.TipoTexto.TituloGrande, ThemedTextView.ColorTexto.Primario, FontStyles.Bold);
                CampoTexto(presentacion, "txtSignificadoEs", ThemedTextView.TipoTexto.Titulo, ThemedTextView.ColorTexto.Primario);
                CampoTexto(presentacion, "txtTituloFormas", ThemedTextView.TipoTexto.Subtitulo, ThemedTextView.ColorTexto.Secundario);
                foreach (Image icon in content.Find("FormasContainer").GetComponentsInChildren<Image>(true))
                    if (icon.name == "Icono") IconoTema(icon, 64);
                Panel(content.Find("FormasContainer").gameObject, false);
            }
            else if (page.TryGetComponent(out SeleccionMultipleView seleccion))
            {
                CampoTexto(seleccion, "txtTextoPregunta", ThemedTextView.TipoTexto.TituloGrande, ThemedTextView.ColorTexto.Primario, FontStyles.Bold);
                CampoTexto(seleccion, "txtOracionHueco", ThemedTextView.TipoTexto.Titulo, ThemedTextView.ColorTexto.Primario);
                TextoTema(content.Find("TxtAyudaOpciones")?.GetComponent<TMP_Text>(),
                    ThemedTextView.TipoTexto.Pequeño, ThemedTextView.ColorTexto.Secundario);
                Button audio = (Button)new SerializedObject(seleccion).FindProperty("btnAudioPregunta").objectReferenceValue;
                Elemento(audio.gameObject).minWidth = Elemento(audio.gameObject).preferredWidth = 104;
            }
            else if (page.TryGetComponent(out MemoriaView _))
            {
                TextoTema(content.Find("Ayuda").GetComponent<TMP_Text>(),
                    ThemedTextView.TipoTexto.Pequeño, ThemedTextView.ColorTexto.Secundario);
            }
            else if (page.TryGetComponent(out WordBankView banco))
            {
                CampoTexto(banco, "txtPromptEs", ThemedTextView.TipoTexto.Titulo, ThemedTextView.ColorTexto.Primario);
                CampoTexto(banco, "txtRespuestaVacia", ThemedTextView.TipoTexto.Pequeño, ThemedTextView.ColorTexto.Terciario);
                TextoTema(content.Find("TxtRespuesta").GetComponent<TMP_Text>(),
                    ThemedTextView.TipoTexto.Pequeño, ThemedTextView.ColorTexto.Secundario);
                TextoTema(content.Find("TxtBanco").GetComponent<TMP_Text>(),
                    ThemedTextView.TipoTexto.Subtitulo, ThemedTextView.ColorTexto.Secundario);
            }
            else if (page.TryGetComponent(out EscrituraView escritura))
            {
                CampoTexto(escritura, "txtInstruccion", ThemedTextView.TipoTexto.Titulo,
                    ThemedTextView.ColorTexto.Primario);
            }
            else if (page.TryGetComponent(out ASRView asr))
            {
                CampoTexto(asr, "txtObjetivo", ThemedTextView.TipoTexto.TituloGrande, ThemedTextView.ColorTexto.Primario, FontStyles.Bold);
                TextoTema(content.Find("Referencia/EtiquetaUch")?.GetComponent<TMP_Text>(),
                    ThemedTextView.TipoTexto.Pequeño, ThemedTextView.ColorTexto.AcentoPrimario);
            }
        }

        private static float EspaciadoVertical(GameObject page)
        {
            if (page.TryGetComponent<SeleccionMultipleView>(out _)) return 28f;
            if (page.TryGetComponent<WordBankView>(out _)) return 28f;
            if (page.TryGetComponent<EscrituraView>(out _)) return 30f;
            if (page.TryGetComponent<ASRView>(out _)) return 28f;
            if (page.TryGetComponent<EmparejamientoView>(out _)) return 26f;
            return 24f;
        }

        private static void ConfigurarFilaForma(GameObject root)
        {
            Panel(root, false);
            TextoTema(root.transform.Find("ColTexto/TxtUchForma").GetComponent<TMP_Text>(),
                ThemedTextView.TipoTexto.Normal, ThemedTextView.ColorTexto.Primario, FontStyles.Bold);
            TextoTema(root.transform.Find("ColTexto/TxtEsForma").GetComponent<TMP_Text>(),
                ThemedTextView.TipoTexto.Normal, ThemedTextView.ColorTexto.Secundario);
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                ConfigurarBoton(button);
                var element = Elemento(button.gameObject);
                element.minWidth = element.preferredWidth = 104;
                element.flexibleWidth = 0;
            }
            var horizontal = root.GetComponent<HorizontalLayoutGroup>();
            horizontal.padding = new RectOffset(24, 24, 16, 16);
            horizontal.childControlWidth = horizontal.childControlHeight = true;
            horizontal.childForceExpandHeight = horizontal.childForceExpandWidth = false;
            Elemento(root).minHeight = 136;
            Elemento(root).preferredHeight = -1;
        }

        private static void ConfigurarBoton(Button button)
        {
            var themed = button.GetComponent<ThemedButtonView>() ?? button.gameObject.AddComponent<ThemedButtonView>();
            themed.Estilo = ThemedButtonView.EstiloBoton.Suave;
            var image = button.GetComponent<Image>();
            image.raycastTarget = true;
            if (image.sprite == null) image.sprite = Sprite("ui_button_rounded_24");
            image.type = Image.Type.Sliced;
            themed.AplicarTema();
            Elemento(button.gameObject).minHeight = 104;
            Elemento(button.gameObject).preferredHeight = -1;
            var horizontal = button.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null)
            {
                horizontal.childControlWidth = horizontal.childControlHeight = true;
                horizontal.childForceExpandHeight = horizontal.childForceExpandWidth = false;
                horizontal.childAlignment = TextAnchor.MiddleCenter;
                horizontal.padding = new RectOffset(16, 16, 16, 16);
            }
            foreach (TMP_Text text in button.GetComponentsInChildren<TMP_Text>(true))
                TextoTema(text, ThemedTextView.TipoTexto.Pequeño, ThemedTextView.ColorTexto.AcentoPrimario, FontStyles.Bold);
            Transform icon = button.transform.Find("Icono");
            if (icon != null)
            {
                Image graphic = icon.GetComponent<Image>();
                IconoTema(graphic, IconoBoton);
                if (button.name.Contains("Audio") || button.name.Contains("Escuchar"))
                    graphic.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Audio/icono_audio.png");
            }
        }

        private static void IconoTema(Image image, float size)
        {
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.rectTransform.sizeDelta = new Vector2(size, size);
            var element = Elemento(image.gameObject);
            element.minWidth = element.preferredWidth = element.minHeight = element.preferredHeight = size;
            element.flexibleWidth = element.flexibleHeight = 0;
            var themed = image.GetComponent<ThemedGraphicView>() ?? image.gameObject.AddComponent<ThemedGraphicView>();
            themed.Rol = ThemedGraphicView.RolColor.Primario;
        }

        private static void CampoTexto(Component view, string field, ThemedTextView.TipoTexto size,
            ThemedTextView.ColorTexto color, FontStyles style = FontStyles.Normal)
        {
            var text = new SerializedObject(view).FindProperty(field).objectReferenceValue as TMP_Text;
            TextoTema(text, size, color, style);
        }

        private static void TextoTema(TMP_Text text, ThemedTextView.TipoTexto size,
            ThemedTextView.ColorTexto color, FontStyles style = FontStyles.Normal)
        {
            if (text == null) return;
            var themed = text.GetComponent<ThemedTextView>() ?? text.gameObject.AddComponent<ThemedTextView>();
            themed.tipoTexto = size;
            themed.tipoColor = color;
            themed.usarColorPersonalizado = false;
            themed.AplicarTema();
            text.fontStyle = style;
            text.enableAutoSizing = false;
            text.raycastTarget = false;
            if (!Elemento(text.gameObject).ignoreLayout)
                Elemento(text.gameObject).minHeight = Elemento(text.gameObject).preferredHeight = -1;
        }

        private static LayoutElement Elemento(GameObject root) => root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
    }
}
#endif
