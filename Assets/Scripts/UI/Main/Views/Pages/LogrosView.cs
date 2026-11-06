using System;
using System.Collections.Generic;
using System.Linq;
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

        [Header("Galería de logros obtenidos")]
        [SerializeField] private Vector2 tamanoLogroObtenido = new(420f, 350f);
        [SerializeField] private Vector2 separacionLogrosObtenidos = new(20f, 30f);
        private const int ColumnasObtenidos = 2;

        [SerializeField] private Transform contenedorLogros;
        [SerializeField] private LogroItemView prefabLogroObtenido;
        [SerializeField] private LogroItemView prefabLogroHorizontal;
        [SerializeField] private Slider barraProgresoGlobal;

        private readonly Dictionary<TipoSeccion, SeccionLogros> secciones = new();

        public event Action OnSolicitarActualizacion;

        private void Awake() => OcultarComposicionLegacy();
        private void OnEnable() => OnSolicitarActualizacion?.Invoke();

#if UNITY_EDITOR
        public void ReconstruirEnEditor(IReadOnlyList<LogroModel> logros)
        {
            secciones.Clear();
            Mostrar(LogrosPresenter.CrearModel(logros));
        }
#endif

        public void Mostrar(LogrosPageModel logros)
        {
            if (logros == null || contenedorLogros == null || !ValidarPrefab()) return;
            OcultarComposicionLegacy();

            int total = logros.Obtenidos.Count + logros.EnProgreso.Count + logros.PorDescubrir.Count;
            ActualizarResumen(logros.Obtenidos.Count, logros.EnProgreso.Count, logros.PorDescubrir.Count, total);
            ActualizarSeccion(TipoSeccion.Obtenidos, logros.Obtenidos.ToList());
            ActualizarSeccion(TipoSeccion.EnProgreso, logros.EnProgreso.ToList());
            ActualizarSeccion(TipoSeccion.PorDescubrir, logros.PorDescubrir.ToList());
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
            Transform card = contenedorLogros.Find("CardResumenGlobal");
            if (card == null) return;
            int porcentaje = total > 0 ? Mathf.RoundToInt((float)obtenidos / total * 100f) : 0;
            AsignarTexto(card, "FilaSuperior/ColTexto/TxtDetalleResumen", $"{obtenidos} de {total} completados ({porcentaje}%)");
            if (barraProgresoGlobal != null)
                barraProgresoGlobal.SetValueWithoutNotify(total > 0 ? (float)obtenidos / total : 0f);
            Transform pills = card.Find("FilaPills");
            string[] etiquetas = { $"{obtenidos} Obtenidos", $"{enProgreso} En curso", $"{bloqueados} Pendientes" };
            for (int i = 0; pills != null && i < Mathf.Min(pills.childCount, etiquetas.Length); i++)
                AsignarTexto(pills.GetChild(i), "Texto", etiquetas[i]);
        }

        private void ActualizarSeccion(TipoSeccion tipo, IReadOnlyList<LogroModel> logros)
        {
            SeccionLogros seccion = ObtenerSeccion(tipo);
            seccion.Raiz.SetActive(logros.Count > 0);
            if (logros.Count == 0) return;
            seccion.Titulo.text = $"{TituloSeccion(tipo)} ({logros.Count})";
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

            RectTransform rectItems = (RectTransform)seccion.Contenedor;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectItems);
            LayoutGroup layoutItems = rectItems.GetComponent<LayoutGroup>();
            LayoutElement tamanoItems = rectItems.GetComponent<LayoutElement>();
            if (tamanoItems != null)
            {
                tamanoItems.minHeight = -1f;
                tamanoItems.preferredHeight = layoutItems.preferredHeight;
            }

            RectTransform rectSeccion = (RectTransform)seccion.Raiz.transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectSeccion);
            VerticalLayoutGroup layoutSeccion = rectSeccion.GetComponent<VerticalLayoutGroup>();
            LayoutElement tamanoSeccion = rectSeccion.GetComponent<LayoutElement>();
            if (tamanoSeccion != null)
            {
                tamanoSeccion.minHeight = -1f;
                tamanoSeccion.preferredHeight = layoutSeccion.preferredHeight;
            }

            VerticalLayoutGroup layoutContenedor = contenedorLogros.GetComponent<VerticalLayoutGroup>();
            if (layoutContenedor != null && !layoutContenedor.childControlHeight)
                rectSeccion.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layoutSeccion.preferredHeight);
        }

        private SeccionLogros ObtenerSeccion(TipoSeccion tipo)
        {
            if (secciones.TryGetValue(tipo, out SeccionLogros seccion)) return seccion;
            Transform existente = contenedorLogros.Find($"SeccionLogros_{tipo}");
            if (existente != null)
            {
                Transform contenedor = existente.Find("Items");
                seccion = new SeccionLogros(existente.gameObject, (existente.Find("Titulo") ?? existente.Find("Encabezado/Titulo")).GetComponent<TMP_Text>(), contenedor);
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
            AsignarThemedText(titulo, ThemedTextView.TipoTexto.Subtitulo, ThemedTextView.ColorTexto.Primario);

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

        private void ConfigurarGrillaObtenidos(GridLayoutGroup grilla)
        {
            grilla.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grilla.constraintCount = ColumnasObtenidos;
            grilla.cellSize = tamanoLogroObtenido;
            grilla.spacing = separacionLogrosObtenidos;
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
            if (texto == null) return;
            texto.SetText(valor);
        }

        public static ThemedTextView AsignarThemedText(TMP_Text texto, ThemedTextView.TipoTexto tipo, ThemedTextView.ColorTexto color)
        {
            if (texto == null) return null;
            ThemedTextView themed = texto.GetComponent<ThemedTextView>() ?? texto.gameObject.AddComponent<ThemedTextView>();
            themed.tipoTexto = tipo; themed.tipoColor = color; themed.usarColorPersonalizado = false; themed.AplicarTema(); return themed;
        }
        public static ThemedGraphicView AsignarThemedGraphic(Image imagen, ThemedGraphicView.RolColor rol)
        {
            if (imagen == null) return null;
            ThemedGraphicView themed = imagen.GetComponent<ThemedGraphicView>() ?? imagen.gameObject.AddComponent<ThemedGraphicView>();
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
