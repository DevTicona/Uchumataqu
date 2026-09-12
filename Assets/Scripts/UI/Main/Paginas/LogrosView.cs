using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
using Assets.Scripts.UI.Main;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista de la pestaña de Logros en 02_Main.
    /// Especificado en Docs/UI/08-perfil-logros-ajustes-acerca.md §3.
    /// Integra el sistema de diseño visual UITheme y componentes ThemedText/ThemedGraphic.
    /// </summary>
    public class LogrosView : MonoBehaviour
    {
        private enum TipoSeccion
        {
            Obtenidos,
            EnProgreso,
            PorDescubrir
        }

        [Header("Contenedor de Lista")]
        [SerializeField] private Transform contenedorLogros;
        [SerializeField] private GameObject prefabLogroItem;

        [Header("Galería de obtenidos (composición de escena)")]
        [SerializeField] private Transform galeriaObtenidos;
        [SerializeField] private GameObject encabezadoObtenidos;

        [Header("Resumen")]
        [SerializeField] private TMP_Text txtTotalLogros;

        private static readonly Dictionary<string, Sprite> CacheIconos = new Dictionary<string, Sprite>();

        private void OnEnable()
        {
            OnSolicitarActualizacion?.Invoke();
        }

        public event Action OnSolicitarActualizacion;

        public void Mostrar(IReadOnlyList<LogroData> logros)
        {
            if (logros == null) return;

            int totalDesbloqueados = logros.Count(l => l.Desbloqueado);
            int totalLogros = logros.Count;
            int totalEnProgreso = logros.Count(l => !l.Desbloqueado && l.ProgresoActual > 0);
            int totalBloqueados = logros.Count(l => !l.Desbloqueado && l.ProgresoActual == 0);

            if (txtTotalLogros != null)
            {
                txtTotalLogros.text = $"{totalDesbloqueados} de {totalLogros} logros obtenidos";
                AsignarThemedText(txtTotalLogros, ThemedText.TipoTexto.Subtitulo, ThemedText.ColorTexto.Secundario);
            }

            if (contenedorLogros == null) return;

            UITheme tema = UIThemeManager.Theme;

            // 1. Tarjeta Hero de Resumen Global al tope del scroll (preserva si ya existe en escena)
            Transform resumenExistente = contenedorLogros.Find("CardResumenGlobal");
            if (resumenExistente != null)
            {
                ActualizarTarjetaResumen(resumenExistente, totalDesbloqueados, totalEnProgreso, totalBloqueados, totalLogros, tema);
            }
            else
            {
                Debug.LogError("[LogrosView] Falta la tarjeta de resumen en la escena.");
                return;
            }

            // 2. Limpiar solo los elementos de secciones y tarjetas anteriores, preservando CardResumenGlobal
            for (int i = contenedorLogros.childCount - 1; i >= 0; i--)
            {
                Transform child = contenedorLogros.GetChild(i);
                if (child.name == "CardResumenGlobal" || child == galeriaObtenidos ||
                    child.gameObject == encabezadoObtenidos) continue;

                child.gameObject.SetActive(false);

                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }

            // 3. Secciones organizadas (solo las que tengan elementos)
            List<LogroData> obtenidos = logros.Where(l => l.Desbloqueado).ToList();
            List<LogroData> enProgreso = logros.Where(l => !l.Desbloqueado && l.ProgresoActual > 0).ToList();
            List<LogroData> bloqueados = logros.Where(l => !l.Desbloqueado && l.ProgresoActual == 0).ToList();

            ActualizarGaleriaObtenidos(obtenidos);

            if (enProgreso.Count > 0)
                CrearSeccion("En progreso", "icono_progreso", enProgreso, TipoSeccion.EnProgreso, tema);

            if (bloqueados.Count > 0)
                CrearSeccion("Por descubrir", "icono_candado", bloqueados, TipoSeccion.PorDescubrir, tema);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contenedorLogros);
            ActualizarRellenos();
        }

        public static void ActualizarTarjetaResumen(
            Transform card,
            int desbloqueados,
            int enProgreso,
            int bloqueados,
            int total,
            UITheme theme)
        {
            if (card == null) return;

            int porcentaje = total > 0 ? Mathf.RoundToInt((float)desbloqueados / total * 100f) : 0;
            card.Find("FilaSuperior/ColTexto/TxtDetalleResumen").GetComponent<TMP_Text>().text =
                $"{desbloqueados} de {total} completados ({porcentaje}%)";
            ConfigurarRelleno(card.Find("BarraProgreso/Relleno").GetComponent<Image>(),
                total > 0 ? (float)desbloqueados / total : 0f);

            Transform pills = card.Find("FilaPills");
            string[] etiquetas = { $"{desbloqueados} Obtenidos", $"{enProgreso} En curso", $"{bloqueados} Pendientes" };
            for (int i = 0; i < etiquetas.Length; i++)
                pills.GetChild(i).Find("Texto").GetComponent<TMP_Text>().text = etiquetas[i];
        }
        private void ActualizarGaleriaObtenidos(IReadOnlyList<LogroData> logros)
        {
            if (galeriaObtenidos == null || encabezadoObtenidos == null)
            {
                Debug.LogError("[LogrosView] Falta la composición de la galería de obtenidos en la escena.");
                return;
            }

            int capacidad = 0;
            foreach (Transform fila in galeriaObtenidos)
                capacidad += fila.childCount;
            if (logros.Count > capacidad)
            {
                Debug.LogError("[LogrosView] La galería no tiene suficientes tarjetas para el catálogo de logros.");
                return;
            }

            encabezadoObtenidos.SetActive(logros.Count > 0);
            galeriaObtenidos.gameObject.SetActive(logros.Count > 0);
            int indice = 0;
            foreach (Transform fila in galeriaObtenidos)
            {
                fila.gameObject.SetActive(indice < logros.Count);
                foreach (Transform celda in fila)
                {
                    Transform tarjeta = celda.GetChild(0);
                    bool visible = indice < logros.Count;
                    tarjeta.gameObject.SetActive(visible);
                    if (visible)
                    {
                        LogroData logro = logros[indice];
                        tarjeta.Find("Titulo").GetComponent<TMP_Text>().text = logro.Titulo;
                        CargarIconoLogro(tarjeta.Find("BadgeIcono/Icono").GetComponent<Image>(), logro);
                    }
                    indice++;
                }
            }
        }
        private void CrearSeccion(
            string titulo,
            string iconoNombre,
            IReadOnlyList<LogroData> logros,
            TipoSeccion tipo,
            UITheme theme)
        {
            CrearEncabezadoSeccion(titulo, iconoNombre, logros.Count, tipo, true);

            foreach (LogroData logro in logros)
                CrearElementoLogro(logro, tipo, theme);
        }

        private void CrearEncabezadoSeccion(
            string titulo,
            string iconoNombre,
            int cantidad,
            TipoSeccion tipo,
            bool mostrarCantidad)
        {
            var encabezado = new GameObject(
                $"Seccion_{titulo}",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup),
                typeof(LayoutElement));
            encabezado.transform.SetParent(contenedorLogros, false);

            var layoutElem = encabezado.GetComponent<LayoutElement>();
            layoutElem.preferredHeight = 52f;
            layoutElem.minHeight = 52f;
            layoutElem.flexibleWidth = 1f;

            var layout = encabezado.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(8, 8, 12, 4);

            ThemedGraphic.RolColor rolIcono = tipo switch
            {
                TipoSeccion.Obtenidos => ThemedGraphic.RolColor.Exito,
                TipoSeccion.EnProgreso => ThemedGraphic.RolColor.Primario,
                _ => ThemedGraphic.RolColor.Secundario
            };

            var iconoObj = new GameObject("IconoSeccion", typeof(RectTransform), typeof(Image));
            iconoObj.transform.SetParent(encabezado.transform, false);
            var rectIcono = (RectTransform)iconoObj.transform;
            rectIcono.sizeDelta = new Vector2(36f, 36f);
            var imgIcono = iconoObj.GetComponent<Image>();
            imgIcono.preserveAspect = true;
            AsignarThemedGraphic(imgIcono, rolIcono);

            string cat = tipo == TipoSeccion.Obtenidos ? "logros" : "estados";
            CargarSpriteIcono(imgIcono, cat, iconoNombre);

            var textoObj = new GameObject("TxtEncabezado", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            textoObj.transform.SetParent(encabezado.transform, false);
            var leTxt = textoObj.GetComponent<LayoutElement>();
            leTxt.preferredHeight = 42f;
            leTxt.minHeight = 42f;
            leTxt.minWidth = 300f;

            var texto = textoObj.GetComponent<TextMeshProUGUI>();
            texto.text = mostrarCantidad ? $"{titulo} ({cantidad})" : titulo;
            texto.fontStyle = FontStyles.Bold;
            texto.alignment = TextAlignmentOptions.MidlineLeft;
            texto.textWrappingMode = TextWrappingModes.NoWrap;
            AsignarThemedText(texto, ThemedText.TipoTexto.Subtitulo, ThemedText.ColorTexto.Primario);

        }

        private void CrearElementoLogro(LogroData logro, TipoSeccion tipo, UITheme theme)
        {
            GameObject item = (prefabLogroItem != null)
                ? Instantiate(prefabLogroItem, contenedorLogros)
                : CrearElementoRuntime(logro.Id, tipo);

            ConfigurarElementoLogro(item, logro, tipo, theme);
        }

        private GameObject CrearElementoRuntime(string id, TipoSeccion tipo)
        {
            var item = new GameObject(
                $"Logro_{id}",
                typeof(RectTransform),
                typeof(Image),
                typeof(HorizontalLayoutGroup),
                typeof(LayoutElement));
            item.transform.SetParent(contenedorLogros, false);

            var layoutElem = item.GetComponent<LayoutElement>();
            layoutElem.preferredHeight = 160f;
            layoutElem.minHeight = 160f;
            layoutElem.flexibleWidth = 1f;
            layoutElem.layoutPriority = 1;

            var layout = item.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 0, 0);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Image fondo = item.GetComponent<Image>();
            UIThemeManager.AplicarSpritePanel(fondo);
            ThemedGraphic.RolColor rolFondo = tipo switch
            {
                TipoSeccion.Obtenidos => ThemedGraphic.RolColor.TarjetaPrincipal,
                TipoSeccion.EnProgreso => ThemedGraphic.RolColor.TarjetaPrincipal,
                _ => ThemedGraphic.RolColor.TarjetaSecundaria
            };
            AsignarThemedGraphic(fondo, rolFondo);

            // 1. Badge circular para el icono (80x80)
            var badgeIcono = new GameObject("BadgeIcono", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            badgeIcono.transform.SetParent(item.transform, false);
            var layoutBadge = badgeIcono.GetComponent<LayoutElement>();
            layoutBadge.preferredWidth = 80f;
            layoutBadge.preferredHeight = 80f;
            layoutBadge.minWidth = 80f;
            layoutBadge.minHeight = 80f;
            layoutBadge.flexibleWidth = 0f;

            var imgBadge = badgeIcono.GetComponent<Image>();
            UIThemeManager.AplicarSpriteCircular(imgBadge);
            ThemedGraphic.RolColor rolBadge = tipo switch
            {
                TipoSeccion.Obtenidos => ThemedGraphic.RolColor.BadgeDesbloqueado,
                TipoSeccion.EnProgreso => ThemedGraphic.RolColor.BadgeProgreso,
                _ => ThemedGraphic.RolColor.BadgeBloqueado
            };
            AsignarThemedGraphic(imgBadge, rolBadge);

            var iconoObj = new GameObject("Icono", typeof(RectTransform), typeof(Image));
            iconoObj.transform.SetParent(badgeIcono.transform, false);
            var rectIcono = (RectTransform)iconoObj.transform;
            rectIcono.anchorMin = Vector2.zero;
            rectIcono.anchorMax = Vector2.one;
            rectIcono.offsetMin = new Vector2(11f, 11f);
            rectIcono.offsetMax = new Vector2(-11f, -11f);
            var imgIcono = iconoObj.GetComponent<Image>();
            imgIcono.preserveAspect = true;
            imgIcono.raycastTarget = false;
            ThemedGraphic.RolColor rolIcono = tipo switch
            {
                TipoSeccion.Obtenidos => ThemedGraphic.RolColor.Exito,
                TipoSeccion.EnProgreso => ThemedGraphic.RolColor.Primario,
                _ => ThemedGraphic.RolColor.Secundario
            };
            AsignarThemedGraphic(imgIcono, rolIcono);

            // 2. Columna central limpia (sin LayoutElement redundante): Título, Descripción y Barra de progreso
            var columna = new GameObject("ColumnaInfo", typeof(RectTransform), typeof(VerticalLayoutGroup));
            columna.transform.SetParent(item.transform, false);

            var colLayoutGroup = columna.GetComponent<VerticalLayoutGroup>();
            colLayoutGroup.padding = new RectOffset(0, 0, 25, 25);
            colLayoutGroup.spacing = 10f;
            colLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
            colLayoutGroup.childControlWidth = true;
            colLayoutGroup.childControlHeight = false;
            colLayoutGroup.childForceExpandWidth = true;
            colLayoutGroup.childForceExpandHeight = false;

            CrearTextoThemed(columna.transform, "Titulo", "Título Logro", ThemedText.TipoTexto.Normal, ThemedText.ColorTexto.Primario, FontStyles.Bold, 32f);
            CrearTextoThemed(columna.transform, "Descripcion", "Descripción del logro.", ThemedText.TipoTexto.Pequeño, ThemedText.ColorTexto.Secundario, FontStyles.Normal, 24f);

            // Barra de progreso interna (altura 20f)
            ThemedGraphic.RolColor rolRelleno = tipo switch
            {
                TipoSeccion.Obtenidos => ThemedGraphic.RolColor.Exito,
                TipoSeccion.EnProgreso => ThemedGraphic.RolColor.Primario,
                _ => ThemedGraphic.RolColor.ProgresoFondo
            };
            CrearBarraProgresoUI(columna.transform, 0f, 20f, rolRelleno);

            // 3. Pill lateral de estado (112x44)
            var pillEstado = new GameObject("PillEstado", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            pillEstado.transform.SetParent(item.transform, false);
            var layoutPill = pillEstado.GetComponent<LayoutElement>();
            layoutPill.preferredWidth = 112f;
            layoutPill.preferredHeight = 44f;
            layoutPill.minWidth = 112f;
            layoutPill.minHeight = 44f;
            layoutPill.flexibleWidth = 0f;

            var imgPill = pillEstado.GetComponent<Image>();
            UIThemeManager.AplicarSpritePill(imgPill);
            ThemedGraphic.RolColor rolPill = tipo switch
            {
                TipoSeccion.Obtenidos => ThemedGraphic.RolColor.BadgeDesbloqueado,
                TipoSeccion.EnProgreso => ThemedGraphic.RolColor.BadgeProgreso,
                _ => ThemedGraphic.RolColor.BadgeBloqueado
            };
            AsignarThemedGraphic(imgPill, rolPill);

            var txtPillObj = new GameObject("TxtPill", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtPillObj.transform.SetParent(pillEstado.transform, false);
            var rectTxtPill = (RectTransform)txtPillObj.transform;
            rectTxtPill.anchorMin = Vector2.zero;
            rectTxtPill.anchorMax = Vector2.one;
            rectTxtPill.offsetMin = Vector2.zero;
            rectTxtPill.offsetMax = Vector2.zero;

            var txtPill = txtPillObj.GetComponent<TextMeshProUGUI>();
            txtPill.fontSize = 24f;
            txtPill.fontStyle = FontStyles.Bold;
            txtPill.alignment = TextAlignmentOptions.Center;
            txtPill.textWrappingMode = TextWrappingModes.NoWrap;

            ThemedText.ColorTexto colorTxtPill = tipo switch
            {
                TipoSeccion.Obtenidos => ThemedText.ColorTexto.Exito,
                TipoSeccion.EnProgreso => ThemedText.ColorTexto.Boton,
                _ => ThemedText.ColorTexto.Secundario
            };
            AsignarThemedText(txtPill, ThemedText.TipoTexto.Pequeño, colorTxtPill);

            return item;
        }

        private void ConfigurarElementoLogro(
            GameObject item,
            LogroData logro,
            TipoSeccion tipo,
            UITheme theme)
        {
            var leItem = item.GetComponent<LayoutElement>();
            if (leItem != null)
            {
                leItem.preferredHeight = 160f;
                leItem.minHeight = 160f;
                leItem.flexibleWidth = 1f;
            }

            Image fondo = item.GetComponent<Image>();
            Transform badgeTransform = item.transform.Find("BadgeIcono");
            Image imgBadge = badgeTransform != null ? badgeTransform.GetComponent<Image>() : null;
            Image imgIcono = badgeTransform != null ? badgeTransform.Find("Icono")?.GetComponent<Image>() : null;
            Transform columna = item.transform.Find("ColumnaInfo");
            TMP_Text txtTitulo = columna != null ? columna.Find("Titulo")?.GetComponent<TMP_Text>() : null;
            TMP_Text txtDesc = columna != null ? columna.Find("Descripcion")?.GetComponent<TMP_Text>() : null;
            Transform barraObj = columna != null ? columna.Find("BarraProgreso") : null;
            Transform pillObj = item.transform.Find("PillEstado");
            Image imgPill = pillObj != null ? pillObj.GetComponent<Image>() : null;
            TMP_Text txtPill = pillObj != null ? pillObj.Find("TxtPill")?.GetComponent<TMP_Text>() : null;

            if (txtTitulo == null || txtDesc == null)
            {
                var textos = item.GetComponentsInChildren<TMP_Text>();
                if (textos.Length >= 2)
                {
                    txtTitulo = textos[0];
                    txtDesc = textos[1];
                }
            }

            if (txtTitulo != null)
            {
                txtTitulo.text = logro.Titulo;
                txtTitulo.fontStyle = FontStyles.Bold;
                txtTitulo.fontSize = UIThemeManager.EscalarTexto(32f);
                AsignarThemedText(txtTitulo, ThemedText.TipoTexto.Normal, ThemedText.ColorTexto.Primario);
                var rtTit = txtTitulo.rectTransform;
                if (rtTit != null) rtTit.sizeDelta = new Vector2(rtTit.sizeDelta.x, 32f);
            }

            if (txtDesc != null)
            {
                txtDesc.text = logro.Descripcion;
                txtDesc.fontStyle = FontStyles.Normal;
                txtDesc.fontSize = UIThemeManager.EscalarTexto(24f);
                AsignarThemedText(txtDesc, ThemedText.TipoTexto.Pequeño, ThemedText.ColorTexto.Secundario);
                var rtDesc = txtDesc.rectTransform;
                if (rtDesc != null) rtDesc.sizeDelta = new Vector2(rtDesc.sizeDelta.x, 24f);
            }

            float ratio = logro.Objetivo > 0 ? Mathf.Clamp01((float)logro.ProgresoActual / logro.Objetivo) : 0f;

            if (fondo != null)
            {
                UIThemeManager.AplicarSpritePanel(fondo);
                ThemedGraphic.RolColor rolFondo = tipo switch
                {
                    TipoSeccion.Obtenidos => ThemedGraphic.RolColor.TarjetaPrincipal,
                    TipoSeccion.EnProgreso => ThemedGraphic.RolColor.TarjetaPrincipal,
                    _ => ThemedGraphic.RolColor.TarjetaSecundaria
                };
                AsignarThemedGraphic(fondo, rolFondo);
            }

            if (badgeTransform != null)
            {
                var leBadge = badgeTransform.GetComponent<LayoutElement>();
                if (leBadge != null)
                {
                    leBadge.minWidth = 74f;
                    leBadge.minHeight = 74f;
                    leBadge.preferredWidth = 74f;
                    leBadge.preferredHeight = 74f;
                }
            }

            if (imgBadge != null)
            {
                UIThemeManager.AplicarSpriteCircular(imgBadge);
                ThemedGraphic.RolColor rolBadge = tipo switch
                {
                    TipoSeccion.Obtenidos => ThemedGraphic.RolColor.BadgeDesbloqueado,
                    TipoSeccion.EnProgreso => ThemedGraphic.RolColor.BadgeProgreso,
                    _ => ThemedGraphic.RolColor.BadgeBloqueado
                };
                AsignarThemedGraphic(imgBadge, rolBadge);
            }

            if (imgIcono != null)
            {
                imgIcono.preserveAspect = true;
                ThemedGraphic.RolColor rolIcono = tipo switch
                {
                    TipoSeccion.Obtenidos => ThemedGraphic.RolColor.Exito,
                    TipoSeccion.EnProgreso => ThemedGraphic.RolColor.Primario,
                    _ => ThemedGraphic.RolColor.Secundario
                };
                AsignarThemedGraphic(imgIcono, rolIcono);
                CargarIconoLogro(imgIcono, logro);
            }

            if (barraObj != null)
            {
                var leBarra = barraObj.GetComponent<LayoutElement>();
                if (leBarra != null) { leBarra.minHeight = 16f; leBarra.preferredHeight = 16f; }
                ActualizarBarraProgresoUI(barraObj, ratio, tipo);
            }

            if (pillObj != null)
            {
                var lePill = pillObj.GetComponent<LayoutElement>();
                if (lePill != null)
                {
                    lePill.minWidth = 112f;
                    lePill.preferredWidth = 112f;
                    lePill.minHeight = 44f;
                    lePill.preferredHeight = 44f;
                }
            }

            if (imgPill != null && txtPill != null)
            {
                UIThemeManager.AplicarSpritePill(imgPill);
                switch (tipo)
                {
                    case TipoSeccion.Obtenidos:
                        AsignarThemedGraphic(imgPill, ThemedGraphic.RolColor.BadgeDesbloqueado);
                        txtPill.text = "Listo";
                        AsignarThemedText(txtPill, ThemedText.TipoTexto.Pequeño, ThemedText.ColorTexto.Exito);
                        break;
                    case TipoSeccion.EnProgreso:
                        AsignarThemedGraphic(imgPill, ThemedGraphic.RolColor.BadgeProgreso);
                        txtPill.text = $"{logro.ProgresoActual}/{logro.Objetivo}";
                        AsignarThemedText(txtPill, ThemedText.TipoTexto.Pequeño, ThemedText.ColorTexto.Boton);
                        break;
                    default:
                        AsignarThemedGraphic(imgPill, ThemedGraphic.RolColor.BadgeBloqueado);
                        txtPill.text = $"{logro.ProgresoActual}/{logro.Objetivo}";
                        AsignarThemedText(txtPill, ThemedText.TipoTexto.Pequeño, ThemedText.ColorTexto.Secundario);
                        break;
                }
            }
        }

        private static void CrearBarraProgresoUI(Transform padre, float ratio, float altura, ThemedGraphic.RolColor rolRelleno)
        {
            var barra = new GameObject("BarraProgreso", typeof(RectTransform), typeof(Image));
            barra.transform.SetParent(padre, false);

            var rectBarra = (RectTransform)barra.transform;
            rectBarra.sizeDelta = new Vector2(0f, altura);

            var imgFondo = barra.GetComponent<Image>();
            UIThemeManager.AplicarSpriteProgreso(imgFondo);
            AsignarThemedGraphic(imgFondo, ThemedGraphic.RolColor.ProgresoFondo);

            var relleno = new GameObject("Relleno", typeof(RectTransform), typeof(Image));
            relleno.transform.SetParent(barra.transform, false);
            var rectRelleno = (RectTransform)relleno.transform;
            rectRelleno.anchorMin = Vector2.zero;
            rectRelleno.anchorMax = Vector2.one;
            rectRelleno.offsetMin = Vector2.zero;
            rectRelleno.offsetMax = Vector2.zero;

            var imgRelleno = relleno.GetComponent<Image>();
            UIThemeManager.AplicarSpriteProgreso(imgRelleno);
            AsignarThemedGraphic(imgRelleno, rolRelleno);
            ConfigurarRelleno(imgRelleno, ratio);
        }

        private static void ActualizarBarraProgresoUI(
            Transform barraObj,
            float ratio,
            TipoSeccion tipo)
        {
            var imgFondo = barraObj.GetComponent<Image>();
            if (imgFondo != null)
            {
                UIThemeManager.AplicarSpriteProgreso(imgFondo);
                AsignarThemedGraphic(imgFondo, ThemedGraphic.RolColor.ProgresoFondo);
            }

            Transform rellenoObj = barraObj.Find("Relleno");
            if (rellenoObj == null) return;

            var imgRelleno = rellenoObj.GetComponent<Image>();
            if (imgRelleno != null)
            {
                UIThemeManager.AplicarSpriteProgreso(imgRelleno);
                ThemedGraphic.RolColor rol = tipo switch
                {
                    TipoSeccion.Obtenidos => ThemedGraphic.RolColor.Exito,
                    TipoSeccion.EnProgreso => ThemedGraphic.RolColor.Primario,
                    _ => ThemedGraphic.RolColor.ProgresoFondo
                };
                AsignarThemedGraphic(imgRelleno, rol);
                ConfigurarRelleno(imgRelleno, ratio);
            }
        }

        private static void ConfigurarRelleno(Image imagen, float ratio)
        {
            float progreso = Mathf.Clamp01(ratio);
            RectTransform rect = imagen.rectTransform;
            if (!(rect.parent is RectTransform fondo)) return;

            imagen.type = Image.Type.Sliced;
            imagen.fillAmount = progreso;
            imagen.enabled = progreso > 0f;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(fondo.rect.width * progreso, 0f);
        }

        private void ActualizarRellenos()
        {
            if (contenedorLogros == null) return;

            foreach (Image imagen in contenedorLogros.GetComponentsInChildren<Image>(true))
            {
                if (imagen.name == "Relleno")
                    ConfigurarRelleno(imagen, imagen.fillAmount);
            }
        }

        private static void CargarIconoLogro(Image target, LogroData logro)
        {
            string iconoFallback = logro.Id.StartsWith("ra_", StringComparison.Ordinal)
                ? "icono_ra_completada"
                : "icono_lecciones_completadas";

            if (string.IsNullOrWhiteSpace(logro.IconoId))
            {
                CargarSpriteIcono(target, "logros", iconoFallback);
                return;
            }

            CargarSpriteIcono(target, "logros", logro.IconoId, "logros", iconoFallback);
        }

        private static void CargarSpriteIcono(
            Image target,
            string categoria,
            string nombre,
            string categoriaFallback = null,
            string nombreFallback = null)
        {
            if (target == null) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                target.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                    $"Assets/Content/UI/Icons/{categoria}/{nombre}.png");
                if (target.sprite == null && !string.IsNullOrEmpty(nombreFallback))
                    target.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                        $"Assets/Content/UI/Icons/{categoriaFallback}/{nombreFallback}.png");
                return;
            }
#endif

            string key = $"{categoria}/{nombre}";
            if (CacheIconos.TryGetValue(key, out Sprite cached) && cached != null)
            {
                target.sprite = cached;
                return;
            }

            // Carga asíncrona vía Addressables en runtime
            string direccionFallback = string.IsNullOrWhiteSpace(categoriaFallback) || string.IsNullOrWhiteSpace(nombreFallback)
                ? null
                : ContentAddress.Icono(categoriaFallback, nombreFallback);
            _ = CargarIconoAddressableAsync(
                target,
                key,
                ContentAddress.Icono(categoria, nombre),
                direccionFallback);
        }

        private static async Task CargarIconoAddressableAsync(
            Image target,
            string cacheKey,
            string direccion,
            string direccionFallback = null)
        {
            try
            {
                Sprite sprite = await AddressablesService.CargarAssetAsync<Sprite>(direccion);
                if (target != null && sprite != null)
                {
                    CacheIconos[cacheKey] = sprite;
                    target.sprite = sprite;
                }
            }
            catch (Exception ex)
            {
                if (string.IsNullOrWhiteSpace(direccionFallback))
                {
                    Debug.LogError($"[LogrosView] Error cargando icono Addressables '{direccion}': {ex.Message}");
                    return;
                }

                try
                {
                    Sprite fallback = await AddressablesService.CargarAssetAsync<Sprite>(direccionFallback);
                    if (target != null && fallback != null)
                        target.sprite = fallback;
                }
                catch (Exception fallbackEx)
                {
                    Debug.LogError(
                        $"[LogrosView] No se pudo cargar '{direccion}' ni su respaldo '{direccionFallback}': {fallbackEx.Message}");
                }
            }
        }

        public static ThemedText AsignarThemedText(
            TMP_Text texto,
            ThemedText.TipoTexto tipo,
            ThemedText.ColorTexto color,
            bool usarPersonalizado = false,
            Color colorPersonalizado = default)
        {
            if (texto == null) return null;
            var themed = texto.GetComponent<ThemedText>();
            if (themed == null)
                themed = texto.gameObject.AddComponent<ThemedText>();

            themed.tipoTexto = tipo;
            themed.tipoColor = color;
            themed.usarColorPersonalizado = usarPersonalizado;
            if (usarPersonalizado)
                themed.colorPersonalizado = colorPersonalizado;

            themed.AplicarTema();
            return themed;
        }

        public static ThemedGraphic AsignarThemedGraphic(
            Image imagen,
            ThemedGraphic.RolColor rol)
        {
            if (imagen == null) return null;
            var tg = imagen.GetComponent<ThemedGraphic>();
            if (tg == null)
                tg = imagen.gameObject.AddComponent<ThemedGraphic>();

            tg.Rol = rol;
            tg.AplicarTema();
            return tg;
        }

        private static void CrearTextoThemed(
            Transform padre,
            string nombre,
            string contenido,
            ThemedText.TipoTexto tipoTexto,
            ThemedText.ColorTexto colorTexto,
            FontStyles estilo,
            float altura = 34f)
        {
            var obj = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(padre, false);

            var rt = (RectTransform)obj.transform;
            rt.sizeDelta = new Vector2(0f, altura);

            var txt = obj.GetComponent<TextMeshProUGUI>();
            txt.text = contenido;
            txt.fontStyle = estilo;
            txt.fontSize = UIThemeManager.EscalarTexto(altura == 32f ? 32f : altura == 24f ? 24f : 28f);
            txt.textWrappingMode = TextWrappingModes.Normal;
            txt.alignment = TextAlignmentOptions.MidlineLeft;

            AsignarThemedText(txt, tipoTexto, colorTexto);
        }
    }
}
