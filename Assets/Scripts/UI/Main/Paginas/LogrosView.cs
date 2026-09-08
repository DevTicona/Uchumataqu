using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.UI.Main;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista de la pestaña de Logros en 02_Main.
    /// Especificado en Docs/UI/08-perfil-logros-ajustes-acerca.md §3.
    /// </summary>
    public class LogrosView : MonoBehaviour
    {
        [Header("Contenedor de Lista")]
        [SerializeField] private Transform contenedorLogros;
        [SerializeField] private GameObject prefabLogroItem;

        [Header("Resumen")]
        [SerializeField] private TMP_Text txtTotalLogros;

        private void OnEnable()
        {
            OnSolicitarActualizacion?.Invoke();
        }

        public event Action OnSolicitarActualizacion;

        public void Mostrar(IReadOnlyList<LogroPresentacionData> logros)
        {
            if (logros == null) return;

            int totalDesbloqueados = 0;
            foreach (var logro in logros) if (logro.Desbloqueado) totalDesbloqueados++;
            if (txtTotalLogros != null)
            {
                txtTotalLogros.text = $"{totalDesbloqueados} / {logros.Count} logros conseguidos";
            }

            if (contenedorLogros != null)
            {
                foreach (Transform child in contenedorLogros)
                {
                    Destroy(child.gameObject);
                }

                List<LogroPresentacionData> desbloqueados = logros.Where(logro => logro.Desbloqueado).ToList();
                List<LogroPresentacionData> enProgreso = logros
                    .Where(logro => !logro.Desbloqueado && logro.ProgresoActual > 0)
                    .ToList();
                List<LogroPresentacionData> bloqueados = logros
                    .Where(logro => !logro.Desbloqueado && logro.ProgresoActual == 0)
                    .ToList();

                CrearSeccion("Desbloqueados", desbloqueados);
                CrearSeccion("En progreso", enProgreso);
                CrearSeccion("Por descubrir", bloqueados);
            }
        }

        private void CrearSeccion(string titulo, IReadOnlyList<LogroPresentacionData> logros)
        {
            var encabezado = new GameObject(
                $"Seccion_{titulo}",
                typeof(RectTransform),
                typeof(TextMeshProUGUI),
                typeof(LayoutElement));
            encabezado.transform.SetParent(contenedorLogros, false);
            TMP_Text texto = encabezado.GetComponent<TextMeshProUGUI>();
            texto.text = $"{titulo} · {logros.Count}";
            texto.fontSize = 22f;
            texto.fontStyle = FontStyles.Bold;
            texto.color = new Color(0.55f, 0.29f, 0.03f, 1f);
            encabezado.GetComponent<LayoutElement>().preferredHeight = 44f;

            foreach (LogroPresentacionData logro in logros)
                CrearElementoLogro(logro);
        }

        private void CrearElementoLogro(LogroPresentacionData logro)
        {
            GameObject item = (prefabLogroItem != null)
                ? Instantiate(prefabLogroItem, contenedorLogros)
                : CrearElementoRuntime(logro.Id);

            Image fondo = item.GetComponent<Image>();
            if (fondo != null)
                fondo.color = logro.Desbloqueado
                    ? new Color(0.15f, 0.55f, 0.34f, 1f)
                    : new Color(0.24f, 0.28f, 0.34f, 0.9f);

            var textos = item.GetComponentsInChildren<TMP_Text>();
            if (textos.Length >= 2)
            {
                textos[0].text = logro.Titulo;
                textos[1].text = logro.Desbloqueado
                    ? $"{logro.Descripcion}  Completado"
                    : $"{logro.Descripcion}  {logro.ProgresoActual}/{logro.Objetivo}";
            }

            float progreso = logro.Objetivo > 0 ? (float)logro.ProgresoActual / logro.Objetivo : 0f;
            CrearBarraProgreso(item.transform, progreso);
            _ = CargarIconoAsync(item.transform, logro.Id.StartsWith("ra_", StringComparison.Ordinal));
        }

        private GameObject CrearElementoRuntime(string id)
        {
            var item = new GameObject(
                $"Logro_{id}",
                typeof(RectTransform),
                typeof(Image),
                typeof(VerticalLayoutGroup),
                typeof(LayoutElement));
            item.transform.SetParent(contenedorLogros, false);

            var layout = item.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 70, 14, 14);
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            item.GetComponent<LayoutElement>().preferredHeight = 132f;

            CrearTexto(item.transform, "Titulo", 24f, FontStyles.Bold);
            CrearTexto(item.transform, "Descripcion", 18f, FontStyles.Normal);
            return item;
        }

        private static void CrearBarraProgreso(Transform padre, float progreso)
        {
            Transform existente = padre.Find("BarraProgreso");
            if (existente != null) Destroy(existente.gameObject);

            var fondo = new GameObject("BarraProgreso", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            fondo.transform.SetParent(padre, false);
            fondo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
            fondo.GetComponent<LayoutElement>().preferredHeight = 8f;

            var relleno = new GameObject("Relleno", typeof(RectTransform), typeof(Image));
            relleno.transform.SetParent(fondo.transform, false);
            RectTransform rect = (RectTransform)relleno.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(Mathf.Clamp01(progreso), 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            relleno.GetComponent<Image>().color = new Color(0.91f, 0.61f, 0.19f, 1f);
        }

        private static async Task CargarIconoAsync(Transform padre, bool esRa)
        {
            try
            {
                string nombre = esRa ? "icono_ra_completada" : "icono_medalla";
                Sprite sprite = await AddressablesService.CargarAssetAsync<Sprite>(
                    ContentAddress.Icono("logros", nombre));
                if (padre == null) return;

                var objeto = new GameObject("IconoLogro", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                objeto.transform.SetParent(padre, false);
                LayoutElement layout = objeto.GetComponent<LayoutElement>();
                layout.ignoreLayout = true;
                RectTransform rect = (RectTransform)objeto.transform;
                rect.anchorMin = new Vector2(1f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.anchoredPosition = new Vector2(-16f, 0f);
                rect.sizeDelta = new Vector2(44f, 44f);
                Image imagen = objeto.GetComponent<Image>();
                imagen.sprite = sprite;
                imagen.preserveAspect = true;
                imagen.raycastTarget = false;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LogrosView] No se pudo cargar el icono: {ex.Message}");
            }
        }

        private static void CrearTexto(Transform padre, string nombre, float tamano, FontStyles estilo)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            objeto.transform.SetParent(padre, false);
            var texto = objeto.GetComponent<TextMeshProUGUI>();
            texto.fontSize = tamano;
            texto.fontStyle = estilo;
            texto.color = Color.white;
            texto.textWrappingMode = TextWrappingModes.Normal;
        }
    }
}
