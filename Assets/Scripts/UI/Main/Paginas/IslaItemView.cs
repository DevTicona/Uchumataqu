using System;
using System.Threading.Tasks;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI.Main.Paginas
{
    public enum EstadoIsla
    {
        Bloqueada,
        Disponible,
        Completada
    }

    /// <summary>
    /// Componente de botón de nodo/isla dentro del mapa curricular de 27 lecciones e hitos RA.
    /// Especificado en Docs/UI/03-escena-main.md §5.
    /// </summary>
    public class IslaItemView : MonoBehaviour
    {
        [Header("Elementos de UI")]
        [SerializeField] private Button btnIsla;
        [SerializeField] private TMP_Text txtNumero;
        [SerializeField] private TMP_Text txtTitulo;
        [SerializeField] private Image imgFondo;
        [SerializeField] private GameObject iconoCandado;
        [SerializeField] private GameObject iconoEstrella;
        [SerializeField] private GameObject indicadorRA; // Si es un hito de RA
        [SerializeField] private GameObject indicadorActual;

        [Header("Colores por Estado")]
        [SerializeField] private Color colorBloqueada = new Color(0.70f, 0.70f, 0.70f, 1f);
        [SerializeField] private Color colorDisponible = new Color(0.92f, 0.76f, 0.35f, 1f); // Tierra
        [SerializeField] private Color colorCompletada = new Color(0.35f, 0.72f, 0.45f, 1f); // Verde acierto

        public string LeccionId { get; private set; }
        public string EscenarioRaId { get; private set; }
        public bool EsHitoRA => !string.IsNullOrEmpty(EscenarioRaId);

        public event Action<string> OnLeccionSeleccionada;
        public event Action<string> OnHitoRaSeleccionado;

        private bool _iconosSolicitados;
        private string _spriteSolicitado;

        private void Awake()
        {
            AsegurarReferencias();
            if (btnIsla != null)
            {
                btnIsla.onClick.AddListener(OnIslaClicked);
            }
        }

        public void InicializarRuntime()
        {
            AsegurarReferencias();
            var rect = GetComponent<RectTransform>();
            if (rect.sizeDelta.y <= 0f) rect.sizeDelta = new Vector2(0f, 116f);
        }

        private void AsegurarReferencias()
        {
            btnIsla ??= GetComponent<Button>();
            imgFondo ??= transform.Find("ImgIsla")?.GetComponent<Image>() ?? GetComponent<Image>();
            txtTitulo ??= GetComponentInChildren<TMP_Text>(true);

            if (imgFondo == null) imgFondo = gameObject.AddComponent<Image>();
            if (btnIsla == null) btnIsla = gameObject.AddComponent<Button>();
            btnIsla.targetGraphic = imgFondo;

            txtNumero ??= CrearTexto("Numero", new Vector2(0f, 0f), new Vector2(0.24f, 1f), 26f);
            txtTitulo ??= CrearTexto("Titulo", new Vector2(0.24f, 0f), new Vector2(0.84f, 1f), 23f);
            iconoCandado ??= CrearIndicador("Candado", "BLOQ.", new Vector2(0.84f, 0f), new Vector2(1f, 1f));
            iconoEstrella ??= CrearIndicador("Completada", "✓", new Vector2(0.84f, 0f), new Vector2(1f, 1f));
            indicadorRA ??= CrearIndicador("IndicadorRA", "RA", new Vector2(0.02f, 0.72f), new Vector2(0.16f, 0.98f));
            indicadorActual ??= CrearIndicador("IndicadorActual", "ACTUAL", new Vector2(0.70f, 0.72f), new Vector2(0.98f, 0.98f));

            if (_iconosSolicitados) return;
            _iconosSolicitados = true;
            _ = ConfigurarIconosAsync();
        }

        public void ConfigurarLeccion(string id, int numero, string titulo, EstadoIsla estado, bool esActual = false)
        {
            LeccionId = id;
            EscenarioRaId = null;

            if (txtNumero != null) txtNumero.text = $"L{numero:D2}";
            if (txtTitulo != null) txtTitulo.text = titulo;
            if (indicadorRA != null) indicadorRA.SetActive(false);
            if (indicadorActual != null) indicadorActual.SetActive(esActual);

            ActualizarEstadoVisual(estado);
            _ = CargarSpriteAsync(ContentAddress.Isla(id));
        }

        public void ConfigurarHitoRA(string escenarioId, string titulo, EstadoIsla estado, bool esActual = false)
        {
            LeccionId = null;
            EscenarioRaId = escenarioId;

            if (txtNumero != null) txtNumero.text = "RA";
            if (txtTitulo != null) txtTitulo.text = titulo;
            if (indicadorRA != null) indicadorRA.SetActive(true);
            if (indicadorActual != null) indicadorActual.SetActive(esActual);

            ActualizarEstadoVisual(estado);
            _ = CargarSpriteAsync(ContentAddress.Isla(escenarioId));
        }

        private void ActualizarEstadoVisual(EstadoIsla estado)
        {
            if (btnIsla != null)
                btnIsla.interactable = (estado != EstadoIsla.Bloqueada);

            if (iconoCandado != null)
                iconoCandado.SetActive(estado == EstadoIsla.Bloqueada);

            if (iconoEstrella != null)
                iconoEstrella.SetActive(estado == EstadoIsla.Completada);

            if (imgFondo != null)
            {
                switch (estado)
                {
                    case EstadoIsla.Bloqueada:
                        imgFondo.color = colorBloqueada;
                        break;
                    case EstadoIsla.Disponible:
                        imgFondo.color = colorDisponible;
                        break;
                    case EstadoIsla.Completada:
                        imgFondo.color = colorCompletada;
                        break;
                }
            }
        }

        private void OnIslaClicked()
        {
            if (EsHitoRA)
            {
                OnHitoRaSeleccionado?.Invoke(EscenarioRaId);
            }
            else
            {
                OnLeccionSeleccionada?.Invoke(LeccionId);
            }
        }

        private TMP_Text CrearTexto(string nombre, Vector2 anchorMin, Vector2 anchorMax, float tamano)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            objeto.transform.SetParent(transform, false);
            var rect = (RectTransform)objeto.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(12f, 8f);
            rect.offsetMax = new Vector2(-12f, -8f);
            var texto = objeto.GetComponent<TextMeshProUGUI>();
            texto.fontSize = tamano;
            texto.alignment = TextAlignmentOptions.MidlineLeft;
            texto.color = Color.white;
            return texto;
        }

        private GameObject CrearIndicador(string nombre, string valor, Vector2 anchorMin, Vector2 anchorMax)
        {
            TMP_Text texto = CrearTexto(nombre, anchorMin, anchorMax, 22f);
            texto.text = valor;
            texto.alignment = TextAlignmentOptions.Center;
            return texto.gameObject;
        }

        private async Task ConfigurarIconosAsync()
        {
            await Task.WhenAll(
                AsignarIconoAsync(iconoCandado, "estados", "icono_candado"),
                AsignarIconoAsync(iconoEstrella, "estados", "icono_completado"),
                AsignarIconoAsync(indicadorActual, "estados", "icono_actual"),
                AsignarIconoAsync(indicadorRA, "ra", "icono_ra"));
        }

        private static async Task AsignarIconoAsync(GameObject contenedor, string categoria, string nombre)
        {
            if (contenedor == null) return;
            try
            {
                Sprite sprite = await AddressablesService.CargarAssetAsync<Sprite>(
                    ContentAddress.Icono(categoria, nombre));
                if (contenedor == null) return;

                Image imagen = contenedor.GetComponent<Image>();
                if (imagen == null)
                {
                    var objeto = new GameObject("Icono", typeof(RectTransform), typeof(Image));
                    objeto.transform.SetParent(contenedor.transform, false);
                    RectTransform rect = (RectTransform)objeto.transform;
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    imagen = objeto.GetComponent<Image>();
                }
                imagen.sprite = sprite;
                imagen.color = Color.white;
                imagen.preserveAspect = true;
                imagen.raycastTarget = false;

                TMP_Text texto = contenedor.GetComponent<TMP_Text>();
                if (texto != null) texto.enabled = false;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IslaItemView] No se pudo cargar {nombre}: {ex.Message}");
            }
        }

        private async Task CargarSpriteAsync(string direccion)
        {
            _spriteSolicitado = direccion;
            try
            {
                Sprite sprite = await AddressablesService.CargarAssetAsync<Sprite>(direccion);
                if (imgFondo == null || _spriteSolicitado != direccion) return;
                imgFondo.sprite = sprite;
                imgFondo.preserveAspect = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IslaItemView] No se pudo cargar {direccion}: {ex.Message}");
            }
        }
    }
}
