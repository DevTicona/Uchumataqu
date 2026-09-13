using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.UI.Main;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>Galería de obtenidos y listas horizontales de logros pendientes.</summary>
    public sealed class LogrosView : MonoBehaviour
    {
        private enum TipoSeccion { Obtenidos, EnProgreso, PorDescubrir }

        private static readonly Vector2 TamanoLogroObtenido = new(400f, 350f);
        private static readonly Vector2 SeparacionLogrosObtenidos = new(60f, 30f);
        private const int ColumnasObtenidos = 2;

        [SerializeField] private Transform contenedorLogros;
        [SerializeField] private LogroItemView prefabLogroObtenido;
        [SerializeField] private LogroItemView prefabLogroHorizontal;
        [SerializeField] private TMP_Text txtTotalLogros;

        private readonly Dictionary<TipoSeccion, SeccionLogros> secciones = new();
        private static readonly Dictionary<string, Sprite> CacheIconos = new();

        public event Action OnSolicitarActualizacion;

        private void Awake() => OcultarComposicionLegacy();
        private void OnEnable() => OnSolicitarActualizacion?.Invoke();

#if UNITY_EDITOR
        public void ReconstruirEnEditor(IReadOnlyList<LogroData> logros)
        {
            secciones.Clear();
            Mostrar(logros);
        }
#endif

        public void Mostrar(IReadOnlyList<LogroData> logros)
        {
            if (logros == null || contenedorLogros == null || !ValidarPrefab()) return;
            OcultarComposicionLegacy();

            int obtenidos = logros.Count(x => x.Desbloqueado);
            int enProgreso = logros.Count(x => !x.Desbloqueado && x.ProgresoActual > 0);
            ActualizarResumen(obtenidos, enProgreso, logros.Count - obtenidos - enProgreso, logros.Count);
            ActualizarSeccion(TipoSeccion.Obtenidos, logros.Where(x => x.Desbloqueado).ToList());
            ActualizarSeccion(TipoSeccion.EnProgreso, logros.Where(x => !x.Desbloqueado && x.ProgresoActual > 0).ToList());
            ActualizarSeccion(TipoSeccion.PorDescubrir, logros.Where(x => !x.Desbloqueado && x.ProgresoActual == 0).ToList());
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contenedorLogros);
        }

        private bool ValidarPrefab()
        {
            if (prefabLogroObtenido != null && prefabLogroHorizontal != null) return true;
            Debug.LogError("[LogrosView] Deben asignarse LogroObtenido y LogroHorizontal.", this);
            return false;
        }

        private void ActualizarResumen(int obtenidos, int enProgreso, int bloqueados, int total)
        {
            if (txtTotalLogros != null)
            {
                txtTotalLogros.text = $"{obtenidos} de {total} logros obtenidos";
                AsignarThemedText(txtTotalLogros, ThemedText.TipoTexto.Subtitulo, ThemedText.ColorTexto.Secundario);
            }

            Transform card = contenedorLogros.Find("CardResumenGlobal");
            if (card == null) return;
            int porcentaje = total > 0 ? Mathf.RoundToInt((float)obtenidos / total * 100f) : 0;
            AsignarTexto(card, "FilaSuperior/ColTexto/TxtDetalleResumen", $"{obtenidos} de {total} completados ({porcentaje}%)");
            ConfigurarRelleno(card.Find("BarraProgreso/Relleno")?.GetComponent<Image>(), total > 0 ? (float)obtenidos / total : 0f);
            Transform pills = card.Find("FilaPills");
            string[] etiquetas = { $"{obtenidos} Obtenidos", $"{enProgreso} En curso", $"{bloqueados} Pendientes" };
            for (int i = 0; pills != null && i < Mathf.Min(pills.childCount, etiquetas.Length); i++)
                AsignarTexto(pills.GetChild(i), "Texto", etiquetas[i]);
        }

        private void ActualizarSeccion(TipoSeccion tipo, IReadOnlyList<LogroData> logros)
        {
            SeccionLogros seccion = ObtenerSeccion(tipo);
            seccion.Raiz.SetActive(logros.Count > 0);
            if (logros.Count == 0) return;
            seccion.Titulo.text = $"{TituloSeccion(tipo)} ({logros.Count})";
            float altoItems;
            if (tipo == TipoSeccion.Obtenidos)
            {
                GridLayoutGroup grilla = seccion.Contenedor.GetComponent<GridLayoutGroup>();
                ConfigurarGrillaObtenidos(grilla);
                int filas = Mathf.CeilToInt((float)logros.Count / ColumnasObtenidos);
                altoItems = grilla.padding.vertical + filas * TamanoLogroObtenido.y +
                    (filas - 1) * SeparacionLogrosObtenidos.y;
            }
            else
            {
                altoItems = logros.Count * 160f + (logros.Count - 1) * 16f;
            }
            LayoutElement layoutItems = seccion.Contenedor.GetComponent<LayoutElement>();
            layoutItems.minHeight = -1f;
            layoutItems.preferredHeight = altoItems;
            VerticalLayoutGroup layoutSeccion = seccion.Raiz.GetComponent<VerticalLayoutGroup>();
            if (tipo == TipoSeccion.Obtenidos)
            {
                layoutSeccion.childControlHeight = true;
                layoutSeccion.childForceExpandHeight = false;
            }
            float altoEncabezado = LayoutUtility.GetPreferredHeight(
                (RectTransform)seccion.Titulo.transform.parent);
            LayoutElement layoutRaiz = seccion.Raiz.GetComponent<LayoutElement>();
            layoutRaiz.minHeight = -1f;
            layoutRaiz.preferredHeight = layoutSeccion.padding.vertical + altoEncabezado +
                layoutSeccion.spacing + altoItems;
            for (int i = 0; i < logros.Count; i++)
            {
                if (i == seccion.Items.Count)
                {
                    LogroItemView prefab = tipo == TipoSeccion.Obtenidos ? prefabLogroObtenido : prefabLogroHorizontal;
                    LogroItemView item = CrearInstancia(prefab, seccion.Contenedor);
                    seccion.Items.Add(item);
                }
                seccion.Items[i].gameObject.SetActive(true);
                seccion.Items[i].Configurar(logros[i], Estado(tipo));
            }
            for (int i = logros.Count; i < seccion.Items.Count; i++) seccion.Items[i].gameObject.SetActive(false);
        }

        private SeccionLogros ObtenerSeccion(TipoSeccion tipo)
        {
            if (secciones.TryGetValue(tipo, out SeccionLogros seccion)) return seccion;
            Transform existente = contenedorLogros.Find($"SeccionLogros_{tipo}");
            if (existente != null)
            {
                Transform contenedor = existente.Find("Items");
                seccion = new SeccionLogros(existente.gameObject, existente.Find("Encabezado/Titulo").GetComponent<TMP_Text>(), contenedor);
                foreach (Transform hijo in contenedor)
                {
                    LogroItemView item = hijo.GetComponent<LogroItemView>();
                    if (item != null) seccion.Items.Add(item);
                }
                secciones.Add(tipo, seccion);
                return seccion;
            }
            GameObject raiz = new($"SeccionLogros_{tipo}", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            raiz.transform.SetParent(contenedorLogros, false);
            VerticalLayoutGroup vertical = raiz.GetComponent<VerticalLayoutGroup>();
            vertical.spacing = 12; vertical.padding = new RectOffset(0, 0, 8, 16);
            vertical.childControlWidth = true; vertical.childControlHeight = true;
            vertical.childForceExpandWidth = true; vertical.childForceExpandHeight = false;

            GameObject encabezado = new("Encabezado", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            encabezado.transform.SetParent(raiz.transform, false);
            encabezado.GetComponent<LayoutElement>().preferredHeight = 52;
            HorizontalLayoutGroup horizontal = encabezado.GetComponent<HorizontalLayoutGroup>();
            horizontal.childAlignment = TextAnchor.MiddleLeft; horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandHeight = false;
            GameObject tituloGo = new("Titulo", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            tituloGo.transform.SetParent(encabezado.transform, false);
            TMP_Text titulo = tituloGo.GetComponent<TMP_Text>();
            titulo.fontStyle = FontStyles.Bold; titulo.alignment = TextAlignmentOptions.MidlineLeft;
            tituloGo.GetComponent<LayoutElement>().flexibleWidth = 1;
            AsignarThemedText(titulo, ThemedText.TipoTexto.Subtitulo, ThemedText.ColorTexto.Primario);

            GameObject items = new("Items", typeof(RectTransform), typeof(LayoutElement));
            items.transform.SetParent(raiz.transform, false);
            if (tipo == TipoSeccion.Obtenidos)
            {
                GridLayoutGroup grilla = items.AddComponent<GridLayoutGroup>();
                ConfigurarGrillaObtenidos(grilla);
            }
            else
            {
                VerticalLayoutGroup lista = items.AddComponent<VerticalLayoutGroup>();
                lista.spacing = 16f;
                lista.childControlWidth = true;
                lista.childControlHeight = true;
                lista.childForceExpandWidth = true;
                lista.childForceExpandHeight = false;
            }
            items.GetComponent<LayoutElement>().flexibleWidth = 1;
            seccion = new SeccionLogros(raiz, titulo, items.transform);
            secciones.Add(tipo, seccion);
            return seccion;
        }

        private static void ConfigurarGrillaObtenidos(GridLayoutGroup grilla)
        {
            grilla.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grilla.constraintCount = ColumnasObtenidos;
            grilla.cellSize = TamanoLogroObtenido;
            grilla.spacing = SeparacionLogrosObtenidos;
            grilla.childAlignment = TextAnchor.UpperCenter;
        }

        private static LogroItemView CrearInstancia(LogroItemView prefab, Transform padre)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, padre);
                return instancia.GetComponent<LogroItemView>();
            }
#endif
            return Instantiate(prefab, padre);
        }

        private void OcultarComposicionLegacy()
        {
            if (contenedorLogros == null) return;
            foreach (string nombre in new[] { "Seccion_Obtenidos", "GaleriaObtenidos" })
            {
                Transform legacy = contenedorLogros.Find(nombre);
                if (legacy != null) legacy.gameObject.SetActive(false);
            }
        }

        private static string TituloSeccion(TipoSeccion tipo) => tipo switch
        {
            TipoSeccion.Obtenidos => "Obtenidos", TipoSeccion.EnProgreso => "En progreso", _ => "Por descubrir"
        };
        private static LogroItemView.EstadoLogro Estado(TipoSeccion tipo) => tipo switch
        {
            TipoSeccion.Obtenidos => LogroItemView.EstadoLogro.Obtenido,
            TipoSeccion.EnProgreso => LogroItemView.EstadoLogro.EnProgreso,
            _ => LogroItemView.EstadoLogro.Bloqueado
        };
        private static void AsignarTexto(Transform padre, string ruta, string valor)
        {
            TMP_Text texto = padre?.Find(ruta)?.GetComponent<TMP_Text>();
            if (texto != null) texto.text = valor;
        }

        public static void ConfigurarRelleno(Image imagen, float ratio)
        {
            if (imagen == null) return;
            imagen.type = Image.Type.Filled; imagen.fillMethod = Image.FillMethod.Horizontal;
            imagen.fillOrigin = (int)Image.OriginHorizontal.Left; imagen.fillAmount = Mathf.Clamp01(ratio);
            imagen.enabled = imagen.fillAmount > 0;
            RectTransform rect = imagen.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void CargarIconoLogro(Image destino, LogroData logro)
        {
            string fallback = logro.Id.StartsWith("ra_", StringComparison.Ordinal) ? "icono_ra_completada" : "icono_lecciones_completadas";
            CargarSpriteIcono(destino, "logros", string.IsNullOrWhiteSpace(logro.IconoId) ? fallback : logro.IconoId, "logros", fallback);
        }
        private static void CargarSpriteIcono(Image destino, string categoria, string nombre, string categoriaFallback, string nombreFallback)
        {
            if (destino == null) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                destino.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Content/UI/Icons/{categoria}/{nombre}.png")
                    ?? AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Content/UI/Icons/{categoriaFallback}/{nombreFallback}.png");
                return;
            }
#endif
            string key = $"{categoria}/{nombre}";
            if (CacheIconos.TryGetValue(key, out Sprite sprite) && sprite != null) { destino.sprite = sprite; return; }
            _ = CargarIconoAsync(destino, key, ContentAddress.Icono(categoria, nombre), ContentAddress.Icono(categoriaFallback, nombreFallback));
        }
        private static async Task CargarIconoAsync(Image destino, string key, string direccion, string respaldo)
        {
            try
            {
                Sprite sprite = await AddressablesService.CargarAssetAsync<Sprite>(direccion);
                if (destino != null && sprite != null) { CacheIconos[key] = sprite; destino.sprite = sprite; }
            }
            catch (Exception)
            {
                try { if (destino != null) destino.sprite = await AddressablesService.CargarAssetAsync<Sprite>(respaldo); }
                catch (Exception) { }
            }
        }

        public static ThemedText AsignarThemedText(TMP_Text texto, ThemedText.TipoTexto tipo, ThemedText.ColorTexto color)
        {
            if (texto == null) return null;
            ThemedText themed = texto.GetComponent<ThemedText>() ?? texto.gameObject.AddComponent<ThemedText>();
            themed.tipoTexto = tipo; themed.tipoColor = color; themed.usarColorPersonalizado = false; themed.AplicarTema(); return themed;
        }
        public static ThemedGraphic AsignarThemedGraphic(Image imagen, ThemedGraphic.RolColor rol)
        {
            if (imagen == null) return null;
            ThemedGraphic themed = imagen.GetComponent<ThemedGraphic>() ?? imagen.gameObject.AddComponent<ThemedGraphic>();
            themed.Rol = rol; themed.AplicarTema(); return themed;
        }

        private sealed class SeccionLogros
        {
            public readonly GameObject Raiz; public readonly TMP_Text Titulo; public readonly Transform Contenedor;
            public readonly List<LogroItemView> Items = new();
            public SeccionLogros(GameObject raiz, TMP_Text titulo, Transform contenedor) { Raiz = raiz; Titulo = titulo; Contenedor = contenedor; }
        }
    }
}
